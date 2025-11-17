using Dapper;
using Hangfire;
using Hangfire.MemoryStorage;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Data;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Configs;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Services;
using iTaxSuite.WebApi.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    _ = builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });

    _ = builder.Services.AddProblemDetails();
    _ = builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    _ = builder.Services.AddEndpointsApiExplorer();
    _ = builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "eTimSuite WebApi"
        });
    });

    // Add Hangfire services.
    try
    {
        _ = builder.Services.AddHangfire(options =>
        {
            options.UseMemoryStorage();
        });
        _ = builder.Services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        });

        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = 3
        });
    }
    catch (Exception iex)
    {
        UI.Fatal(iex, $"Fatal Error: Application cannot continue, Could Not Initialize JobScheduler..." +
            $"Error - {iex.GetBaseException().Message}");
        throw;
    }

    _ = builder.Services.AddScoped<IDBaseService, DBaseService>();
    _ = builder.Services.AddTransient<HttpLogger>();
    _ = builder.Services.AddTransient<RestRequestHandler>();

    _ = builder.Services.AddHttpClient();
    _ = builder.Services.AddHttpClient(GeneralConst.HTTP_CLIENT_UNSAFE, delegate (HttpClient m)
        {
            m.Timeout = TimeSpan.FromSeconds(180.0);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
        })
        .AddHttpMessageHandler<RestRequestHandler>()
        .RemoveAllLoggers()
        .AddLogger<HttpLogger>(wrapHandlersPipeline: true);

    ExtSystConfig _extSystConfig = null;
    try
    {
        var iTaxDBConnStr = builder.Configuration.GetConnectionString("ITaxDBConnection");
        if (string.IsNullOrWhiteSpace(iTaxDBConnStr))
            throw new ArgumentNullException($"Database Setup Failed, ITaxDBConnection {iTaxDBConnStr} is invalid");
        _ = builder.Services.AddSingleton(new DatabaseOptions { iTaxDBConnString = iTaxDBConnStr });
        _ = builder.Services.AddDbContext<ETimsDBContext>(options => options.UseSqlServer(iTaxDBConnStr), ServiceLifetime.Scoped);
        using (var connection = new SqlConnection(iTaxDBConnStr))
        {
            _extSystConfig = connection.QueryFirst<ExtSystConfig>("select * from [ExtSystConfig]");
        }
    }
    catch (Exception iex)
    {
        UI.Fatal(iex, $"Fatal Error: Application could not connect to Main SQL DB. Error - {iex.GetBaseException().Message}");
        throw;
    }
    if (_extSystConfig == null)
    {
        throw new Exception("Invalid Sage 300 ERP configuration");
    }
    _ = builder.Services.AddSingleton(_extSystConfig);

    try
    {
        var redisConnection = builder.Configuration.GetConnectionString("CacheConnection");
        if (string.IsNullOrWhiteSpace(redisConnection))
            throw new ArgumentNullException($"Cache Setup Failed, CacheConnection {redisConnection} is invalid");
        ConnectionMultiplexer _redisMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
        _ = builder.Services.AddSingleton<IConnectionMultiplexer>(s => _redisMultiplexer);
    }
    catch (Exception iex)
    {
        UI.Fatal(iex, $"Fatal Error: Application cannot continue, Cache database not reachable..." +
            $"Error - {iex.GetBaseException().Message}");
        throw;
    }

    _ = builder.Services.AddScoped<IMasterDataSvc, MasterDataSvc>();
    _ = builder.Services.AddScoped<ISetupService, SetupService>();
    _ = builder.Services.AddScoped<ISchedulerService, SchedulerService>();
    _ = builder.Services.AddScoped<IEtimsService, EtimsService>();
    _ = builder.Services.AddScoped<IS300ProductSvc, S300ProductSvc>();
    _ = builder.Services.AddScoped<IS300PurchaseSvc, S300PurchaseSvc>();
    _ = builder.Services.AddScoped<IS300SaleService, S300SaleService>();
    _ = builder.Services.AddScoped<ITransactService, TransactService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "etims-middleware";
            options.EnableTryItOutByDefault();
        });
    }

    app.UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/scheduler", new DashboardOptions
    {
        DarkModeEnabled = true,
        DashboardTitle = "Scheduler Monitor",
        DisplayStorageConnectionString = false,
        AppPath = null
    });

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ETimsDBContext>();
            DataSeeder.InitializeDB(context);

            var masterSvc = services.GetRequiredService<IMasterDataSvc>();
            await masterSvc.InitializeCacheData();
            await masterSvc.InitiateTaxSetup();

            var cancellationTokenSource = new CancellationTokenSource();
            var scheduler = services.GetRequiredService<ISchedulerService>();
            scheduler.ScheduleJobs(cancellationTokenSource.Token);
            UI.Debug($"Done Scheduling...");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed Running Startup Tasks, Error : {ex.GetBaseException()}");
        }
    }

    UI.Debug("Startup Tasks Complete, Running Host...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application Startup failed: {ex.GetBaseException().ToString()}");
    Environment.Exit(-1);
}
finally
{
    await Log.CloseAndFlushAsync();
}
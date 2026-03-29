using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.WebHook.Models;
using iTaxSuite.WebHook.Services;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    _ = builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    });

    var sectMWareConfig = builder.Configuration.GetRequiredSection("MWareSettings");
    var mWareConfig = sectMWareConfig.Get<MWareSettings>();
    if (mWareConfig == null)
        throw new Exception($"Invalid MiddleWare Settings configuration");
    _ = builder.Services.AddSingleton(mWareConfig);

    _ = builder.Services.AddProblemDetails();
    _ = builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    _ = builder.Services.AddEndpointsApiExplorer();
    _ = builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "eTimSuite WebHook"
        });
    });

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

    _ = builder.Services.AddScoped<IDTaxHookService, DTaxHookService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "etims-webhook";
            options.EnableTryItOutByDefault();
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application Startup failed: {ex.GetBaseException().ToString()}");
    UI.Fatal($"Application Startup failed: {ex.GetBaseException().ToString()}");
    Environment.Exit(-1);
}
finally
{
    await Log.CloseAndFlushAsync();
}
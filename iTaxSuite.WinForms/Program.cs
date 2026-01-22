using Dapper;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Configs;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Services;
using iTaxSuite.WinForms.Clients;
using iTaxSuite.WinForms.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace iTaxSuite.WinForms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
                IServiceCollection services = new ServiceCollection();

                IHost _host = Host.CreateDefaultBuilder().UseSerilog(delegate (HostBuilderContext context, LoggerConfiguration configuration)
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                }).ConfigureServices((Action<IServiceCollection>)delegate (IServiceCollection services)
                {
                    /*_ = services.AddSingleton<IDBaseService, DBaseService>();
                    _ = services.AddTransient<HttpLogger>();
                    _ = services.AddTransient<RestRequestHandler>();
                    */
                    services.AddHttpClient();
                    _ = services.AddHttpClient(GeneralConst.HTTP_CLIENT_UNSAFE, delegate (HttpClient m)
                    {
                        m.Timeout = TimeSpan.FromSeconds(180.0);
                    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
                    });
                    /*.AddHttpMessageHandler<RestRequestHandler>()
                    .RemoveAllLoggers()
                    .AddLogger<HttpLogger>(wrapHandlersPipeline: true);*/

                    ExtSystConfig? _extSystConfig = null;
                    try
                    {
                        var iTaxDBConnStr = configuration.GetConnectionString("ITaxDBConnection");
                        if (string.IsNullOrWhiteSpace(iTaxDBConnStr))
                            throw new ArgumentNullException($"Database Setup Failed, ITaxDBConnection {iTaxDBConnStr} is invalid");
                        _ = services.AddSingleton(new DatabaseOptions { iTaxDBConnString = iTaxDBConnStr });
                        _ = services.AddDbContext<ETimsDBContext>(options => options.UseSqlServer(iTaxDBConnStr), ServiceLifetime.Scoped);
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
                    _ = services.AddSingleton(_extSystConfig);

                    try
                    {
                        var redisConnection = configuration.GetConnectionString("CacheConnection");
                        if (string.IsNullOrWhiteSpace(redisConnection))
                            throw new ArgumentNullException($"Cache Setup Failed, CacheConnection {redisConnection} is invalid");
                        ConnectionMultiplexer _redisMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
                        _ = services.AddSingleton<IConnectionMultiplexer>(s => _redisMultiplexer);
                    }
                    catch (Exception iex)
                    {
                        UI.Fatal(iex, $"Fatal Error: Application cannot continue, Cache database not reachable... . Error - {iex.GetBaseException().Message}");
                        throw;
                    }

                    /* IConfigurationSection sectSage300ERP = configuration.GetRequiredSection("Sage300ERP");
                     var sage300ERPConfig = sectSage300ERP.Get<Sage300ERPConfig>();
                     if (sage300ERPConfig == null)
                     {
                         throw new Exception("Invalid Sage 300 ERP configuration");
                     }*/


                    var sectVSCUConfig = configuration.GetRequiredSection("VSCUConfig");
                    var vscuConfig = sectVSCUConfig.Get<VSCUConfig>();
                    if (vscuConfig == null)
                    {
                        throw new Exception("Invalid ETIMS VSCU configuration");
                    }
                    vscuConfig.InitializeConfig();
                    _ = services.AddSingleton(vscuConfig);

                    // setup injactable database configurations
                    var sectITaxConfig = configuration.GetRequiredSection("iTaxConfig");
                    var iTaxConfig = sectITaxConfig.Get<ITaxConfig>();
                    if (iTaxConfig == null || !iTaxConfig.IsValid())
                        throw new Exception($"Invalid ITax configuration");
                    _ = services.AddSingleton(iTaxConfig);

                    var sectZFPETRConfig = configuration.GetRequiredSection("ZFPETRConfig");
                    ZFPETRConfig zfpETRConfig = sectZFPETRConfig.Get<ZFPETRConfig>();
                    if (zfpETRConfig == null)
                        throw new Exception($"Invalid ZFP ETR configuration");
                    _ = services.AddSingleton(zfpETRConfig);

                    /*services.AddSingleton(sage300ERPConfig);

                    services.AddScoped<IETIMSetupSvc, ETIMSetupSvc>();
                    services.AddScoped<IETIMSCacheSvc, ETIMSCacheSvc>();

                    services.AddScoped<ISage300Svc, Sage300Svc>();
                    services.AddScoped<IProductService, ETIMItemSvc>();
                    services.AddScoped<IETIMSalesSvc, ETIMSalesSvc>();
                    services.AddScoped<IETIMSPuchaseSvc, ETIMSPuchaseSvc>();

                    services.AddScoped<IZFPTransactSvc, ZFPTransactSvc>();
                    */

                    _ = services.AddScoped<IMasterDataSvc, MasterDataSvc>();
                    _ = services.AddScoped<IEtimsService, EtimsService>();
                    _ = services.AddScoped<IS300SaleService, S300SaleService>();


                    _ = services.AddTransient<ETIMSClient>();
                    //services.AddTransient<ZFPClient>();
                    //services.AddTransient<TevinClient>();
                    _ = services.AddTransient<FormsTaxHost>();
                })
                .Build();

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.EnableVisualStyles();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var app = _host.Services.GetRequiredService<FormsTaxHost>();
                Application.Run(app);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application Startup failed: {ex.GetBaseException()}");
                Environment.Exit(-1);
            }
        }
    }
}
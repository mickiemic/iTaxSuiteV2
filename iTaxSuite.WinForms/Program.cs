using iTaxSuite.WinForms.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text.RegularExpressions;

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
                    /*_ = services.AddHttpClient(GeneralConst.HTTP_CLIENT_UNSAFE, delegate (HttpClient m)
                    {
                        m.Timeout = TimeSpan.FromSeconds(180.0);
                    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (HttpRequestMessage m, X509Certificate2 c, X509Chain ch, SslPolicyErrors e) => true
                    })
                   .AddHttpMessageHandler<RestRequestHandler>()
                   .RemoveAllLoggers()
                   .AddLogger<HttpLogger>(wrapHandlersPipeline: true);*/

                    /*try
                    {
                        var iTaxDBConnStr = configuration.GetConnectionString("ITaxDBConnection");
                        if (string.IsNullOrWhiteSpace(iTaxDBConnStr))
                            throw new ArgumentNullException($"Database Setup Failed, ITaxDBConnection {iTaxDBConnStr} is invalid");
                        _ = services.AddSingleton(new DatabaseOptions { iTaxDBConnString = iTaxDBConnStr });
                    }
                    catch (Exception iex)
                    {
                        UI.Fatal(iex, $"Fatal Error: Application could not connect to Main SQL DB. Error - {iex.GetBaseException().Message}");
                        throw;
                    }*/

                    /*try
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
                    }*/

                   /* IConfigurationSection sectSage300ERP = configuration.GetRequiredSection("Sage300ERP");
                    var sage300ERPConfig = sectSage300ERP.Get<Sage300ERPConfig>();
                    if (sage300ERPConfig == null)
                    {
                        throw new Exception("Invalid Sage 300 ERP configuration");
                    }*/

                    //services.AddMemoryCache();

                    /*IConfigurationSection sectVSCUConfig = configuration.GetRequiredSection("VSCUConfig");
                    var vscuConfig = sectVSCUConfig.Get<VSCUConfig>();
                    if (vscuConfig == null)
                    {
                        throw new Exception("Invalid ETIMS VSCU configuration");
                    }
                    vscuConfig.InitializeConfig();*/

                    // setup injactable database configurations
                    /*var sectITaxConfig = configuration.GetRequiredSection("iTaxConfig");
                    var iTaxConfig = sectITaxConfig.Get<ITaxConfig>();
                    if (iTaxConfig == null || !iTaxConfig.IsValid())
                        throw new Exception($"Invalid ITax configuration");

                    var sectZFPETRConfig = configuration.GetRequiredSection("ZFPETRConfig");
                    ZFPETRConfig zfpETRConfig = sectZFPETRConfig.Get<ZFPETRConfig>();
                    if (zfpETRConfig == null)
                        throw new Exception($"Invalid ZFP ETR configuration");*/

                    /*services.AddSingleton(sage300ERPConfig);
                    services.AddSingleton(iTaxConfig);
                    services.AddSingleton(vscuConfig);
                    services.AddSingleton(zfpETRConfig);

                    services.AddScoped<IETIMSetupSvc, ETIMSetupSvc>();
                    services.AddScoped<IETIMSCacheSvc, ETIMSCacheSvc>();

                    services.AddScoped<ISage300Svc, Sage300Svc>();
                    services.AddScoped<IProductService, ETIMItemSvc>();
                    services.AddScoped<IETIMSalesSvc, ETIMSalesSvc>();
                    services.AddScoped<IETIMSPuchaseSvc, ETIMSPuchaseSvc>();

                    services.AddScoped<IZFPTransactSvc, ZFPTransactSvc>();
                    */
                    
                    services.AddTransient<ETIMSClient>();
                    //services.AddTransient<ZFPClient>();
                    //services.AddTransient<TevinClient>();

                    services.AddTransient<FormsTaxHost>();
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
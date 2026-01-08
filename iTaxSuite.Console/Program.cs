// See https://aka.ms/new-console-template for more information
using iTaxSuite.CLIApp;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

try
{
    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    IConfigurationRoot configuration = builder.Build();
    IServiceCollection services = new ServiceCollection();

    IHost _host = Host.CreateDefaultBuilder().UseSerilog(delegate (HostBuilderContext context, LoggerConfiguration configuration)
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    }).ConfigureServices(delegate (IServiceCollection services)
    {
        _ = services.AddHttpClient();
        _ = services.AddHttpClient("UNSAFE", delegate (HttpClient m)
            {
                m.Timeout = TimeSpan.FromSeconds(180.0);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (HttpRequestMessage m, X509Certificate2 c, X509Chain ch, SslPolicyErrors e) => true
            });

        _ = services.AddDataProtection().SetApplicationName(GeneralConst.APPLICATION_NAME);
        bool UseSecureConn = configuration.GetValue<bool>(SecureConst.USESECURECONN, true);

        //services.AddSingleton(sage300ERPConfig);

        var serviceProvider = services.BuildServiceProvider();
        IDataProtectionProvider protectionProvider = serviceProvider.GetService<IDataProtectionProvider>();
        IDataProtector _dataProtector = protectionProvider.CreateProtector(SecureConst.DATA_PURPOSE);

        try
        {
            var ITaxDBConnection = configuration.GetConnectionString("ITaxDBConnection");
            string iTaxDBConnStr = UseSecureConn ? _dataProtector.Unprotect(ITaxDBConnection) : ITaxDBConnection;
            if (string.IsNullOrWhiteSpace(iTaxDBConnStr))
                throw new ArgumentNullException($"Database Setup Failed, ITaxDBConnection {iTaxDBConnStr} is invalid");
            UI.Debug($"ITaxDBConnection : {ITaxDBConnection} ==> {iTaxDBConnStr}");
            //_ = services.AddSingleton(new DatabaseOptions { iTaxDBConnString = iTaxDBConnStr });
        }
        catch (Exception iex)
        {
            UI.Fatal(iex, $"Fatal Error: Application could not connect to Main SQL DB. Error - {iex.GetBaseException().Message}");
            throw;
        }

        try
        {
            var CacheConnection = configuration.GetConnectionString("CacheConnection");
            var redisConnection = UseSecureConn ? _dataProtector.Unprotect(CacheConnection) : CacheConnection;
            if (string.IsNullOrWhiteSpace(redisConnection))
                throw new ArgumentNullException($"Cache Setup Failed, CacheConnection {redisConnection} is invalid");
            UI.Debug($"CacheConnection : {CacheConnection} ==> {redisConnection}");
            /*ConnectionMultiplexer _redisMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
            _ = services.AddSingleton<IConnectionMultiplexer>(s => _redisMultiplexer);*/
        }
        catch (Exception iex)
        {
            UI.Fatal(iex, $"Fatal Error: Application cannot continue, Cache database not reachable... . Error - {iex.GetBaseException().Message}");
            throw;
        }

        _ = services.AddSingleton<iTaxDriver>();
    })
    .Build();

    var app = _host.Services.GetRequiredService<iTaxDriver>();
    app.RunConsoleApp();
}
catch (Exception ex)
{
    Console.WriteLine("Application Startup failed: " + ex.GetBaseException().ToString());
    Environment.Exit(-1);
}
Console.ReadLine();

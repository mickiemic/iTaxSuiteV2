using FakeItEasy;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace iTaxSuite.Tester
{
    public class EtimsServiceTester
    {
        private readonly ETimsDBContext _dbContext;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IEtimsService _etimsService;

        public string connectionString = "Server=localhost;Database=eTimSuite;UID=sageadmin;Password=sageadmin;MultipleActiveResultSets=True;Encrypt=False;App=iTaxTester";

        public EtimsServiceTester()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ETimsDBContext>();
            builder.UseSqlServer(connectionString)
                    .UseInternalServiceProvider(serviceProvider);
            _dbContext = new ETimsDBContext(builder.Options);

            var _httpClientFactory = A.Fake<IHttpClientFactory>();
            var _connectionMultiplexer = A.Fake<IConnectionMultiplexer>();

            _masterDataSvc = new MasterDataSvc(_dbContext, _connectionMultiplexer);
            _etimsService = new EtimsService(_httpClientFactory, _dbContext, _masterDataSvc, _connectionMultiplexer);
        }

        [Fact]
        public async Task SelectItems_Test()
        {
            var result = await _etimsService.SelectItems("20240101000000");
            Assert.True(result.IsSuccess);
        }

    }
}

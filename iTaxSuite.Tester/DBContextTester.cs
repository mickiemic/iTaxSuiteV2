using iTaxSuite.Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace iTaxSuite.Tester
{
    public class DBContextTester : IDisposable
    {
        private readonly ETimsDBContext _dbContext;
        public string connectionString = "Server=localhost;Database=eTimSuite;UID=sageadmin;Password=sageadmin;MultipleActiveResultSets=True;Encrypt=False;App=iTaxTester";

        public DBContextTester()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<ETimsDBContext>();
            builder.UseSqlServer(connectionString)
                    .UseInternalServiceProvider(serviceProvider);
            _dbContext = new ETimsDBContext(builder.Options);
        }

        [Fact]
        public async Task Test_AllTables_Queries()
        {
            var taxClient = await _dbContext.TaxClient.Include(e => e.ExtSystConfig).ToListAsync();
            Console.WriteLine($"TaxClient Count: {taxClient.Count}");
            Assert.NotNull(taxClient);

            var extSystConfig = await _dbContext.ExtSystConfig.ToListAsync();
            Console.WriteLine($"ExtSystConfig Count: {extSystConfig.Count}");
            Assert.NotNull(extSystConfig);

            var branches = await _dbContext.ClientBranch.Include(e => e.TaxClient).ToListAsync();
            Console.WriteLine($"ClientBranch Count: {branches.Count}");
            Assert.NotNull(branches);

            var branchCustomers = await _dbContext.BranchCustomers.Include(e => e.ClientBranch).ToListAsync();
            Console.WriteLine($"BranchCustomers Count: {branchCustomers.Count}");
            Assert.NotNull(branchCustomers);

            var branchUsers = await _dbContext.BranchUsers.Include(e => e.ClientBranch).ToListAsync();
            Console.WriteLine($"BranchUsers Count: {branchUsers.Count}");
            Assert.NotNull(branchUsers);

            var taxAuths = await _dbContext.S300TaxAuthority.ToListAsync();
            Console.WriteLine($"S300TaxAuthority Count: {taxAuths.Count}");
            Assert.NotNull(taxAuths);

            var products = await _dbContext.Products.Include(e => e.ProductData).ToListAsync();
            Console.WriteLine($"Products Count: {products.Count}");
            Assert.NotNull(products);

            var stockItems = await _dbContext.StockItems.Include(e => e.Product).ToListAsync();
            Console.WriteLine($"StockItems Count: {stockItems.Count}");
            Assert.NotNull(stockItems);

            var stockMovm = await _dbContext.StockMovement.Include(e => e.StockMovData).ToListAsync();
            Console.WriteLine($"StockMovement Count: {stockMovm.Count}");
            Assert.NotNull(stockMovm);

            var purchTrxs = await _dbContext.PurchTransact.Include(e => e.PurchTrxData).ToListAsync();
            Console.WriteLine($"PurchTransact Count: {purchTrxs.Count}");
            Assert.NotNull(purchTrxs);

            var salesTrxs = await _dbContext.SalesTransact.Include(e => e.SalesTrxData).ToListAsync();
            Console.WriteLine($"SalesTransact Count: {salesTrxs.Count}");
            Assert.NotNull(salesTrxs);

            var etimsTrxs = await _dbContext.EtimsTransacts.ToListAsync();
            Console.WriteLine($"EtimsTransacts Count: {etimsTrxs.Count}");
            Assert.NotNull(etimsTrxs);

        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}

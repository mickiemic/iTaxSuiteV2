using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;

namespace iTaxSuite.Library.Interfaces
{
    public interface IS300ProductSvc
    {
        TaxDeviceType GetDeviceType();
        Task<Result<StockMovement, string>> CreateStockMovement(StockIORequest stockIORequest);
        Task<List<Product>> FetchARProducts();
        Task<List<Product>> FetchGLProducts();
        Task<List<Product>> FetchICProducts();
        Task<Result<PagedResult<Product>, string>> GetProducts(ProductFilter filter);
        Task<Result<PagedResult<StockItem>, string>> GetStockItems(StockFilter filter);
        Task<Result<PagedResult<StockMovement>, string>> GetStockMovements(MovementFilter filter);
        Task<Result<int, string>> ProcessItemCallback(ItemCallback itemCallback);
        Task<Result<EtimsTransact, string>> ProcessSaveProduct(EtimsTransact transact);
        Task<Result<EtimsTransact, string>> ProcessSaveStockIO(EtimsTransact transact);
        Task<Result<EtimsTransact, string>> QueueSaveProduct(BranchStockKey filter);
        Task<Result<Product, string>> ReFetchProduct(ProductKey productKey);
        Task<Result<BranchStockLevel, string>> SaveStockLevel(SaveStockLevel saveStockLevel);
        Task<Result<List<string>, string>> SyncTaxProducts();
        Task<Result<List<EtimsTransact>, string>> PostPendingProducts();
    }
}

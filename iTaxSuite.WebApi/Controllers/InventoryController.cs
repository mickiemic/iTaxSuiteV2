using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Inventory")]
    [Route("api/inventory")]
    [ApiController]
    public class InventoryController : MBaseController
    {
        private readonly IS300ProductSvc _productSvc;

        public InventoryController(IS300ProductSvc productSvc)
        {
            _productSvc = productSvc;
        }

        [HttpPost]
        [Route("getproducts")]
        public async Task<IActionResult> GetProducts(ProductFilter filter)
        {
            string _method_ = "GetProducts";
            try
            {
                var result = await _productSvc.GetProducts(filter);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("refechproduct")]
        public async Task<IActionResult> ReFetchProduct(ProductKey productKey)
        {
            string _method_ = "ReFetchProduct";
            try
            {
                var result = await _productSvc.ReFetchProduct(productKey);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("getstocks")]
        public async Task<IActionResult> GetStocks(StockFilter filter)
        {
            string _method_ = "GetStocks";
            try
            {
                var result = await _productSvc.GetStockItems(filter);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("getstockmovements")]
        public async Task<IActionResult> GetStockMovements(MovementFilter filter)
        {
            string _method_ = "GetStockMovements";
            try
            {
                var result = await _productSvc.GetStockMovements(filter);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("setitemtaxcode")]
        public async Task<IActionResult> SetItemTaxCode()
        {
            await Task.FromResult(0);
            return StatusCode(500, "Not Yet Implemented");
        }
        [HttpPost]
        [Route("registerproduct")]
        public async Task<IActionResult> RegisterTaxProduct(BranchStockKey filter)
        {
            string _method_ = "RegisterTaxProduct";
            try
            {
                var result = await _productSvc.QueueSaveProduct(filter);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("savestocklevel")]
        public async Task<IActionResult> SaveStockLevel(SaveStockLevel saveStockLevel)
        {
            string _method_ = "SaveStockLevel";
            try
            {
                if (saveStockLevel is null || !saveStockLevel.IsValid())
                    throw new Exception($"Invalid request received");

                var result = await _productSvc.SaveStockLevel(saveStockLevel);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

    }
}

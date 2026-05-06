using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Sales")]
    [Route("api/sales")]
    [ApiController]
    public class SalesController : MBaseController
    {
        private readonly IS300SaleService _saleService;

        public SalesController(IEnumerable<IS300SaleService> s300SaleServices)
        {
            _saleService = s300SaleServices.Single(x => x.GetDeviceType() == TaxDeviceType.DIGITAX);
        }

        [HttpPost]
        [Route("getsales")]
        public async Task<IActionResult> GetSales(SalesFilter filter)
        {
            string _method_ = "GetSales";
            try
            {
                var result = await _saleService.GetSales(filter);
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
        [Route("getsaleqr")]
        public async Task<IActionResult> GetQRImage(int salesTrxId, bool updateMeta = false)
        {
            string _method_ = "GetQRImage";
            try
            {
                var result = await _saleService.GetQRImage(salesTrxId, updateMeta);
                if (result.IsSuccess)
                {
                    var salesTransact = result.GetValue();
                    var file = File(salesTransact.QRImage, "image/png");

                    return Ok(new SaleQRView(salesTransact, 
                        file.FileContents, file.ContentType));
                }
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
        [Route("refetchoesale")]
        public async Task<IActionResult> ReFetchOESale(SaleTrxKey saleTrxKey)
        {
            string _method_ = "ReFetchOESale";
            try
            {
                var result = await _saleService.ReFetchOEInvoice(saleTrxKey);
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
        [Route("resyncsaleinv")]
        public async Task<IActionResult> ReSyncSaleInvoice(SaleTrxKey saleTrxKey)
        {
            string _method_ = "ReSyncSaleInvoice";
            try
            {
                var result = await _saleService.ReSyncTaxInvoice(saleTrxKey);
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
        [Route("querysalestrx")]
        public async Task<IActionResult> QuerySalesTrx(SaleTrxKey saleTrxKey, bool fixTransaction = true)
        {
            string _method_ = "QuerySalesTrx";
            try
            {
                var result = await _saleService.QuerySaleTransact(saleTrxKey, fixTransaction);
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
        [Route("postsale")]
        public async Task<IActionResult> PostSaleTrx(QueueSaveSale filter)
        {
            string _method_ = "PostSaleTrx";
            try
            {
                var result = await _saleService.QueueSaveSale(filter);
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
        [Route("salecallback")]
        public async Task<IActionResult> PostSaleCallback(SaleCallback saleCallback)
        {
            string _method_ = "PostSaleCallback";
            try
            {
                if (saleCallback is null)
                    throw new Exception($"Invalid request received");

                var result = await _saleService.ProcessSaleCallback(saleCallback);
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

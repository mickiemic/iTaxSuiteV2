using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Sales")]
    [Route("api/sales")]
    [ApiController]
    public class SalesController : MBaseController
    {
        private readonly IS300SaleService _saleService;

        public SalesController(IS300SaleService saleService)
        {
            _saleService = saleService;
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
    }
}

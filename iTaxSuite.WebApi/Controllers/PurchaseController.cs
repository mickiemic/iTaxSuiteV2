using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Purchases")]
    [Route("api/purchase")]
    [ApiController]
    public class PurchaseController : MBaseController
    {
        private readonly IS300PurchaseSvc _purchaseSvc;
        private readonly IEtimsService _etimsService;

        public PurchaseController(IS300PurchaseSvc purchaseSvc, IEtimsService etimsService)
        {
            _purchaseSvc = purchaseSvc;
            _etimsService = etimsService;
        }

        [HttpPost]
        [Route("getpurchases")]
        public async Task<IActionResult> GetPurchases(PurchaseFilter filter)
        {
            string _method_ = "GetPurchases";
            try
            {
                var result = await _purchaseSvc.GetPurchases(filter);
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
        [Route("importpurchases")]
        public async Task<IActionResult> ImportPurchases(DateTime? filter)
        {
            string _method_ = "ImportPurchases";
            try
            {
                DateTime filterDate = filter ?? DateTime.Today;
                string lastReqDt = filterDate.ToString(ETIMSConst.FMT_DATETIME);
                var result = await _etimsService.SelectPurchaseSales(lastReqDt);
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
        [Route("postpurchase")]
        public async Task<IActionResult> PostPurchaseTrx()
        {
            await Task.FromResult(0);
            return StatusCode(500, "Not Yet Implemented");
        }

        [HttpPost]
        [Route("acceptpurchase")]
        public async Task<IActionResult> AcceptPurchaseTrx(PurchTransact purchTransact)
        {
            string _method_ = "AcceptPurchaseTrx";
            try
            {
                if (purchTransact == null || !purchTransact.IsValid())
                {
                    UI.Error($"{_method_} Invalid: {JsonConvert.SerializeObject(purchTransact)}");
                    return StatusCode(500, "Invalid input received");
                }

                var result = await _purchaseSvc.AcceptPurchaseTrx(purchTransact);
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
            }
            catch(Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

    }
}

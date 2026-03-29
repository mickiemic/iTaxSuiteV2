using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.WebHook.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace iTaxSuite.WebHook.Controllers
{
    [Tags("DigiTax")]
    [Route("hook/digitax/{businessid}")]
    [ApiController]
    public class DigiTaxController : MBaseController
    {
        private readonly IDTaxHookService _dTaxHookService;

        public DigiTaxController(IDTaxHookService taxHookService)
        {
            _dTaxHookService = taxHookService;
        }

        [HttpPost]
        [Route("itemsync")]
        public async Task<IActionResult> ItemSync(string businessid, [FromBody] JsonElement callback)
        {
            string _method_ = "ItemSync";
            ItemCallback itemCallback;
            try
            {
                var jObject = JObject.Parse(callback.GetRawText());
                UI.Info($"{_method_} businessid:{businessid} >> {JsonConvert.SerializeObject(jObject)}");
                
                itemCallback = jObject.ToObject<ItemCallback>();
                if (itemCallback.Event != "item.sync")
                {
                    throw new Exception($"Invalid event type {itemCallback.Event}");
                }

                var result = await _dTaxHookService.ProcessItemCallback(itemCallback);
                if (result.IsError)
                {
                    string strError = result.GetError();
                    UI.Error($"{_method_} ID: {itemCallback.CBData.ID}, error:{strError}");
                    return StatusCode(500, strError);
                }
                return Ok(callback);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} hook/itemsync error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("salesync")]
        public async Task<IActionResult> SaleSync(string businessid, [FromBody] JsonElement callback)
        {
            string _method_ = "SaleSync";
            SaleCallback saleCallback;
            try
            {
                var jObject = JObject.Parse(callback.GetRawText());
                UI.Info($"{_method_} businessid:{businessid} >> {JsonConvert.SerializeObject(jObject)}");

                saleCallback = jObject.ToObject<SaleCallback>();
                if (saleCallback.Event != "sale.sync")
                {
                    throw new Exception($"Invalid event type {saleCallback.Event}");
                }

                var result = await _dTaxHookService.ProcessSaleCallback(saleCallback);
                if (result.IsError)
                {
                    string strError = result.GetError();
                    UI.Error($"{_method_} ID: {saleCallback.CBData.InvoiceNumber}, error:{strError}");
                    return StatusCode(500, strError);
                }
                return Ok(callback);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} hook/salesync error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

    }
}

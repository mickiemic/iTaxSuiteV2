using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.WebHook.Models;
using Newtonsoft.Json;

namespace iTaxSuite.WebHook.Services
{
    public interface IDTaxHookService
    {
        Task<Result<bool, string>> ProcessItemCallback(ItemCallback itemCallback);
        Task<Result<bool, string>> ProcessSaleCallback(SaleCallback saleCallback);
    }
    public class DTaxHookService : IDTaxHookService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MWareSettings _mWareSettings;

        public DTaxHookService(IHttpClientFactory httpClientFactory, MWareSettings mWareSettings)
        {
            _httpClientFactory = httpClientFactory;
            _mWareSettings = mWareSettings;
        }

        public async Task<Result<bool,string>> ProcessItemCallback(ItemCallback itemCallback)
        {
            string _method_ = "ProcessItemCallback";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_mWareSettings.Address}/api/inventory/itemcallback");
                string jsonRequest = JsonConvert.SerializeObject(itemCallback);
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                if (_httpResponse.IsSuccessStatusCode)
                {
                    _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                    return true;
                }
                else
                {
                    _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                    return _strResponse;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<bool, string>> ProcessSaleCallback(SaleCallback saleCallback)
        {
            string _method_ = "ProcessSaleCallback";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_mWareSettings.Address}/api/sales/salecallback");
                string jsonRequest = JsonConvert.SerializeObject(saleCallback);
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                if (_httpResponse.IsSuccessStatusCode)
                {
                    _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                    return true;
                }
                else
                {
                    _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                    return _strResponse;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

    }
}

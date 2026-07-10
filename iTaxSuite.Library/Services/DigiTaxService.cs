using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Newtonsoft.Json;

namespace iTaxSuite.Library.Services
{
    public interface IDigiTaxService
    {
        Task<Result<DTaxCreateItemResp, string>> CreateDTaxItem(DTaxCreateItemReq createItemReq);
        Task<Result<DTaxSaveSaleResp, string>> CreateDTaxSale(DTaxSaveSaleReq saveSaleReq);
        Task<Result<int, string>> GetBranchCount(bool log = false);
        Task<Result<DTaxSelectItemResp, string>> GetDTaxProducts();
        Task<Result<DTaxCreateItemResp, string>> GetDTaxOneProduct(string itemId);
        Task<Result<DTaxStockAdjustResp, string>> ItemStockAdjust(DTaxStockAdjustReq stockAdjustReq);
        Task<Result<DTaxSaveCNoteResp, string>> CreateDTaxCRNote(DTaxSaveCNoteReq saveCNoteReq);
        Task<Result<DTaxPurchaseResp, string>> GetDTaxPurchases(int pageSize = 0, string before = null, string after = null);
        Task<Result<DTaxNoticeResp, string>> SelectNotices();
        Task<Result<DTaxSaveSaleResp, string>> GetDTaxOneSale(string saleId);
    }
    public class DigiTaxService : IDigiTaxService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly ClientBranch _clientBranch = null;

        public DigiTaxService(IHttpClientFactory httpClientFactory, IMasterDataSvc masterDataSvc)
        {
            _httpClientFactory = httpClientFactory;
            _masterDataSvc = masterDataSvc;

            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
        }

        public async Task<Result<int, string>> GetBranchCount(bool log = false)
        {
            string _method_ = "SelectBranchCount";
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/branches");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                UI.Debug($">> {_method_} : {_reqUrl}");
               var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers);
                string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    if (log)
                        UI.Info($"<< {_method_}: {_strResponse}");
                    var result = JsonConvert.DeserializeObject<DTaxBranchResp>(_strResponse);
                    return result.Branches.Count;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxSelectItemResp, string>> GetDTaxProducts()
        {
            string _method_ = "GetDTaxProducts";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                UI.Debug($">> {_method_} : {_reqUrl}");
                var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                
                UI.Info($"<< {_method_}: {_strResponse}");
                var result = JsonConvert.DeserializeObject<DTaxSelectItemResp>(_strResponse);
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }
        public async Task<Result<DTaxCreateItemResp, string>> GetDTaxOneProduct(string itemId)
        {
            string _method_ = "GetDTaxOneProduct";
            DTaxCreateItemResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items/{itemId}");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                UI.Debug($">> {_method_} : {_reqUrl}");
                var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers);
                string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }

                UI.Info($"<< {_method_}: {_strResponse}");
                result = JsonConvert.DeserializeObject<DTaxCreateItemResp>(_strResponse);
                result.RawResponse = _strResponse;
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxCreateItemResp, string>> CreateDTaxItem(DTaxCreateItemReq createItemReq)
        {
            string _method_ = "CreateDTaxItem";
            string _strResponse = string.Empty;
            DTaxCreateItemResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                string jsonRequest = JsonConvert.SerializeObject(createItemReq);
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest, headers);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<DTaxCreateItemResp>(_strResponse);
                    result.RawResponse = _strResponse;
                    if (!result.IsSuccess)
                    {
                        UI.Error($"<< {_method_}: {_strResponse}");
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxStockAdjustResp, string>> ItemStockAdjust(DTaxStockAdjustReq stockAdjustReq)
        {
            string _method_ = "ItemStockAdjust";
            string _strResponse = string.Empty;
            DTaxStockAdjustResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                string jsonRequest = JsonConvert.SerializeObject(stockAdjustReq);
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPutJsonAsync(_reqUrl, jsonRequest, headers);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<DTaxStockAdjustResp>(_strResponse);
                    result.RawResponse = _strResponse;
                    if (!result.IsSuccess)
                    {
                        UI.Error($"<< {_method_}: {_strResponse}");
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxSaveSaleResp, string>> CreateDTaxSale(DTaxSaveSaleReq saveSaleReq)
        {
            string _method_ = "CreateDTaxSale";
            DTaxSaveSaleResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/sales-with-items");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                string jsonRequest = JsonConvert.SerializeObject(saveSaleReq);
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest, headers);
                string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<DTaxSaveSaleResp>(_strResponse);
                    result.RawResponse = _strResponse;
                    if (!result.IsSuccess)
                    {
                        UI.Error($"<< {_method_}: {_strResponse}");
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxSaveCNoteResp, string>> CreateDTaxCRNote(DTaxSaveCNoteReq saveCNoteReq)
        {
            string _method_ = "CreateDTaxCRNote";
            string _strResponse = string.Empty;
            DTaxSaveCNoteResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/credit-notes-with-barcode");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                string jsonRequest = JsonConvert.SerializeObject(saveCNoteReq);
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest, headers);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<DTaxSaveCNoteResp>(_strResponse);
                    result.RawResponse = _strResponse;
                    if (!result.IsSuccess)
                    {
                        UI.Error($"<< {_method_}: {_strResponse}");
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxPurchaseResp, string>> GetDTaxPurchases(int pageSize = 0, string before = null, string after = null)
        {
            string _method_ = "GetDTaxPurchases";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/purchases");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                var qParams = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(before))
                    qParams["before"] = before;
                if (!string.IsNullOrWhiteSpace(after))
                    qParams["after"] = after;
                if (pageSize > 0)
                    qParams["page_size"] = pageSize.ToString();
                UI.Debug($">> {_method_} : {_reqUrl}");
                var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers, qParams);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }

                UI.Info($"<< {_method_}: {_strResponse}");
                var result = JsonConvert.DeserializeObject<DTaxPurchaseResp>(_strResponse);
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxSaveSaleResp, string>> GetDTaxOneSale(string saleId)
        {
            string _method_ = "GetDTaxOneSale";
            DTaxSaveSaleResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/sales/{saleId}");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                UI.Debug($">> {_method_} : {_reqUrl}");
                var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers);
                string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }

                UI.Info($"<< {_method_}: {_strResponse}");
                result = JsonConvert.DeserializeObject<DTaxSaveSaleResp>(_strResponse);
                result.RawResponse = _strResponse;
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<DTaxNoticeResp, string>> SelectNotices()
        {
            string _method_ = "SelectNotices";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/notices");
                var headers = new Dictionary<string, string>() { { "x-api-key", _clientBranch.TaxClient.APIKey } };
                UI.Debug($">> {_method_} : {_reqUrl}");
                var _httpResponse = await client.ProcessGetJsonAsync(_reqUrl, headers);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                UI.Info($"<< {_method_}: {_strResponse}");
                var result = JsonConvert.DeserializeObject<DTaxNoticeResp>(_strResponse);
                return result;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

    }
}

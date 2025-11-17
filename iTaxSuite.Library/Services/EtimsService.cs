using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace iTaxSuite.Library.Services
{
    public interface IEtimsService
    {
        Task<Result<SaveItemResp, string>> CreateEtimsItem(SaveItemReq saveItemReq);
        Task<Result<TrnsPurchaseSaveResp, string>> SaveEtimsPurchase(TrnsPurchaseSaveReq savePurchaseReq);
        Task<Result<TrnsSalesSaveResp, string>> SaveEtimsSale(TrnsSalesSaveReq saveSaleReq);
        Task<Result<StockIOSaveResp, string>> SaveEtimsStockIO(StockIOSaveReq stockIOSaveReq);
        Task<Result<StockMstSaveResp, string>> SaveEtimsStockMaster(StockMstSaveReq stockMstSaveReq);
        Task<Result<bool, string>> SelectBranches();
        Task<Result<SelectItemResp, string>> SelectItems(string strInput);
        Task<Result<NoticeResp, string>> SelectNotices(string lastReqDt);
        Task<Result<TrnsPurchaseSalesResp, string>> SelectPurchaseSales(string lastReqDt);
    }
    public class EtimsService : IEtimsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IDatabase _baseDb;
        private readonly ClientBranch _clientBranch = null;

        public EtimsService(IHttpClientFactory httpClientFactory, IMasterDataSvc masterDataSvc)
        {
            _httpClientFactory = httpClientFactory;
            _masterDataSvc = masterDataSvc;

            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
        }

        public async Task<Result<bool, string>> SelectBranches()
        {
            string _method_ = "SelectBranches";
            string _strResponse = string.Empty;
            SelectBranchResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/branches/selectBranches");
                var requestObj = new SelectBranchReq(_clientBranch);
                string jsonRequest = JsonConvert.SerializeObject(requestObj);
                UI.Debug($">> {_method_}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<SelectBranchResp>(_strResponse);
                    return result.IsSuccess;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<SelectItemResp, string>> SelectItems(string strInput)
        {
            string _method_ = "SelectItems";
            string _strResponse = string.Empty;
            SelectItemResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items/selectItems");
                var requestObj = new SelectItemReq
                {
                    PIN = _clientBranch.TaxClient.TaxNumber,
                    LastRequest = strInput
                };
                string jsonRequest = JsonConvert.SerializeObject(requestObj);
                UI.Info($">> {_method_}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<SelectItemResp>(_strResponse);
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<NoticeResp, string>> SelectNotices(string lastReqDt)
        {
            string _method_ = "SelectNotices";
            string _strResponse = string.Empty;
            NoticeResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/notices/selectNotices");
                var requestObj = new NoticeReq
                {
                    PIN = _clientBranch.TaxClient.TaxNumber,
                    LastRequest = lastReqDt
                };
                string jsonRequest = JsonConvert.SerializeObject(requestObj);
                UI.Info($">> {_method_}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<NoticeResp>(_strResponse);
                    return result;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                return ex.GetBaseException().Message;
            }
        }

        public async Task<Result<TrnsPurchaseSalesResp, string>> SelectPurchaseSales(string lastReqDt)
        {
            string _method_ = "SelectPurchaseSales";
            string _strResponse = string.Empty;
            TrnsPurchaseSalesResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/trnsPurchase/selectTrnsPurchaseSales");
                var requestObj = new TrnsPurchaseSalesReq
                {
                    PIN = _clientBranch.TaxClient.TaxNumber,
                    LastRequest = lastReqDt
                };
                string jsonRequest = JsonConvert.SerializeObject(requestObj);
                UI.Info($">> {_method_}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<TrnsPurchaseSalesResp>(_strResponse);
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

        public async Task<Result<T, string>> POSTEtimsRequest<T>(IETIMSTransaction requestWrapper) where T : ETIMSBaseResp
        {
            string _method_ = "POSTEtimsRequest";
            string _strResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = requestWrapper.RequestAddress;
                string jsonRequest = JsonConvert.SerializeObject(requestWrapper.GetRequestEntity(), new DecimalFormatConverter());
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    var result = JsonConvert.DeserializeObject<T>(_strResponse);
                    if (result.ResultCode != "000")
                    {
                        string _strError = result.ResultMsg;
                        UI.Error($"<< {_method_}: {_strError}");
                        return _strError;
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

        public async Task<Result<SaveItemResp, string>> CreateEtimsItem(SaveItemReq saveItemReq)
        {
            string _method_ = "CreateEtimsItem";
            string _strResponse = string.Empty;
            SaveItemResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/items/saveItems");
                string jsonRequest = JsonConvert.SerializeObject(saveItemReq);
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<SaveItemResp>(_strResponse);
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

        public async Task<Result<TrnsSalesSaveResp, string>> SaveEtimsSale(TrnsSalesSaveReq saveSaleReq)
        {
            string _method_ = "SaveEtimsSale";
            string _strResponse = string.Empty;
            TrnsSalesSaveResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/trnsSales/saveSales");
                string jsonRequest = JsonConvert.SerializeObject(saveSaleReq, new DecimalFormatConverter());
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<TrnsSalesSaveResp>(_strResponse);
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

        public async Task<Result<StockIOSaveResp, string>> SaveEtimsStockIO(StockIOSaveReq stockIOSaveReq)
        {
            string _method_ = "SaveEtimsStockIO";
            string _strResponse = string.Empty;
            StockIOSaveResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/stock/saveStockItems");
                string jsonRequest = JsonConvert.SerializeObject(stockIOSaveReq, new DecimalFormatConverter());
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<StockIOSaveResp>(_strResponse);
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

        public async Task<Result<TrnsPurchaseSaveResp, string>> SaveEtimsPurchase(TrnsPurchaseSaveReq savePurchaseReq)
        {
            string _method_ = "SaveEtimsSale";
            string _strResponse = string.Empty;
            TrnsPurchaseSaveResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/trnsPurchase/savePurchases");
                string jsonRequest = JsonConvert.SerializeObject(savePurchaseReq, new DecimalFormatConverter());
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<TrnsPurchaseSaveResp>(_strResponse);
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

        public async Task<Result<StockMstSaveResp, string>> SaveEtimsStockMaster(StockMstSaveReq stockMstSaveReq)
        {
            string _method_ = "SaveEtimsSale";
            string _strResponse = string.Empty;
            StockMstSaveResp result = null;
            try
            {
                var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
                string _reqUrl = string.Format($"{_clientBranch.EtrAddress}/stockMaster/saveStockMaster");
                string jsonRequest = JsonConvert.SerializeObject(stockMstSaveReq, new DecimalFormatConverter());
                UI.Info($">> {_method_} {_reqUrl}: {jsonRequest}");
                var _httpResponse = await client.ProcessPostJsonAsync(_reqUrl, jsonRequest);
                _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                if (!_httpResponse.IsSuccessStatusCode)
                {
                    UI.Error($"<< {_method_}: {_strResponse}");
                    return _strResponse;
                }
                else
                {
                    UI.Info($"<< {_method_}: {_strResponse}");
                    result = JsonConvert.DeserializeObject<StockMstSaveResp>(_strResponse);
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

    }
}

using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace iTaxSuite.Library.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> ProcessGetXmlAsync<T>(this HttpClient _httpClient,
            string baseUrl, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
            where T : class
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent("{}", Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            var _httpResponse = await _httpClient.SendAsync(_restRequest);
            _httpResponse.EnsureSuccessStatusCode();
            string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
            return XmlUtils.Deserialize<T>(_strResponse);
        }

        public static async Task<T> ProcessGetJsonAsync<T>(this HttpClient _httpClient,
            string baseUrl, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
            where T : class
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent("{}", Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            var _httpResponse = await _httpClient.SendAsync(_restRequest);
            _httpResponse.EnsureSuccessStatusCode();
            string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(_strResponse);
        }

        /// <summary>
        /// Process a GET requests asynchronously<para/>
        /// Appends the nullable list <paramref name="parameters"/> to <paramref name="baseUrl"/> to form the final URL
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessGetReqAsync(this HttpClient _httpClient,
            string baseUrl, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent("{}", Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            return await _httpClient.SendAsync(_restRequest);
        }

        /// <summary>
        /// Process a GET request with Bearer authentication asynchronously.<para/>
        /// Appends the nullable list <paramref name="parameters"/> to <paramref name="baseUrl"/> to form the final URL
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl">Base URL on which the query string will be appended</param>
        /// <param name="bearerToken">The actual bearer authentication token</param>
        /// <param name="headers">A nullable dictionary of http request headers</param>
        /// <param name="parameters">A nullable dictionary of http request parameters</param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessGetReqBearerAsync(this HttpClient _httpClient,
            string baseUrl, string bearerToken, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent(string.Empty, Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            return await _httpClient.SendAsync(_restRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessGetReqBasicAsync(this HttpClient _httpClient,
            string baseUrl, string userName, string password, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);
            var _basicToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_basicToken));

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent("{}", Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            return await _httpClient.SendAsync(_restRequest);
        }

        public static async Task<T> ProcessGetReqBasicAsync<T>(this HttpClient _httpClient,
            string baseUrl, string userName, string password, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Get, _requestUrl);
            var _basicToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_basicToken));

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    if ((header.Key.ToLower().Equals("Content-Type".ToLower())) && (_restRequest.Method == HttpMethod.Get))
                    {
                        _restRequest.Content = new StringContent("{}", Encoding.UTF8, header.Value);
                    }
                    else
                        _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            var _httpResponse = await _httpClient.SendAsync(_restRequest);
            _httpResponse.EnsureSuccessStatusCode();
            string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(_strResponse);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="requestData"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessPostRestAsync(this HttpClient _httpClient,
            string baseUrl, HttpContent requestData, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = requestData;

            return await _httpClient.SendAsync(_restRequest);
        }

        public static async Task<HttpResponseMessage> ProcessPostFormAsync(this HttpClient _httpClient,
            string baseUrl, Dictionary<string, string> parameters, Dictionary<string, string> headers = null)
        {
            string _requestUrl = baseUrl;

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }
            var _formContent = new FormUrlEncodedContent(parameters);

            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = _formContent;

            return await _httpClient.SendAsync(_restRequest);
        }

        public static async Task<T> ProcessPostFormAsync<T>(this HttpClient _httpClient,
            string baseUrl, Dictionary<string, string> parameters, Dictionary<string, string> headers = null)
        {
            string _requestUrl = baseUrl;

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }
            var _formContent = new FormUrlEncodedContent(parameters);

            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = _formContent;
            var _httpResponse = await _httpClient.SendAsync(_restRequest);

            //_httpResponse.EnsureSuccessStatusCode();
            //return await _httpResponse.Content.ReadFromJsonAsync<T>();

            if (!_httpResponse.IsSuccessStatusCode)
            {
                string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                throw new Exception(string.Format($"GetToken Call: {(int)_httpResponse.StatusCode} : {_httpResponse.ReasonPhrase} >> {_strResponse}"));
            }
            else
            {
                var _result = await _httpResponse.Content.ReadFromJsonAsync<T>();
                //string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
                //var _result = JsonConvert.DeserializeObject<T>(_strResponse);
                //UI.Debug("<< ProcessPostFormAsync: {0} -> {1}", JsonConvert.SerializeObject(_result));
                return _result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="jsonRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessPostJsonAsync(this HttpClient _httpClient,
            string baseUrl, string jsonRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, ContentType.JSON);

            return await _httpClient.SendAsync(_restRequest);
        }

        public static async Task<HttpResponseMessage> ProcessPostJsonBasicAsync(this HttpClient _httpClient,
            string baseUrl, string userName, string password, string jsonRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);
            var _basicToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_basicToken));

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, ContentType.JSON);

            return await _httpClient.SendAsync(_restRequest);
        }

        /*public static async Task<HttpResponseMessage> ProcessPostMultiPartAsync(this HttpClient _httpClient,
            string baseUrl, string jsonRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null,
            Dictionary<string, string> files = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }

            using MultipartFormDataContent multipartFormData = new();
            if (!string.IsNullOrWhiteSpace(jsonRequest))
            {
                multipartFormData.Add(new StringContent(jsonRequest, Encoding.UTF8, ContentType.JSON));
            }

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    var fileStream = new FileStream(file.Value, FileMode.Open);
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(FileUtils.GetMimeTypeForFileExtension(file.Value));
                    multipartFormData.Add(streamContent, file.Key, Path.GetFileName(file.Value));
                }
            }
            _restRequest.Content = multipartFormData;

            return await _httpClient.SendAsync(_restRequest);
        }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="jsonRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<T> ProcessPostJsonAsync<T>(this HttpClient _httpClient,
            string baseUrl, string jsonRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, ContentType.JSON);

            var _httpResponse = await _httpClient.SendAsync(_restRequest);
            _httpResponse.EnsureSuccessStatusCode();
            return await _httpResponse.Content.ReadFromJsonAsync<T>();
        }

        /// <summary>
        /// Posts a Json Request to Sage300WebApi and gets the response object or error message.
        /// </summary>
        /// <typeparam name="T">An implementation of Sage300ERPResp</typeparam>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="jsonRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>Sage300ERPResp implementation for success, or a ApiErrorResp object with the error</returns>
        public static async Task<Result<T, ApiErrorResp>> PostSage300ERPAsync<T>(this HttpClient _httpClient,
            string baseUrl, string userName, string password, string jsonRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
            where T : Sage300ERPResp
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);
            var _basicToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_basicToken));

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(jsonRequest, Encoding.UTF8, ContentType.JSON);

            var _httpResponse = await _httpClient.SendAsync(_restRequest);
            string _strResponse = await _httpResponse.Content.ReadAsStringAsync();
            if (_httpResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(_strResponse);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_strResponse))
                {
                    return new ApiErrorResp(_httpResponse.StatusCode);
                }
                else
                {
                    var sageError = JsonConvert.DeserializeObject<Sage300ERPError>(_strResponse.JsonCompact());
                    return sageError.GetApiErrorResp(_httpResponse.StatusCode);
                }
            }
        }

        /// <summary>
        /// Process a POST XML request with <c>text/xml; charset=utf-8</c>
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="xmlRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessPostXmlAsync(this HttpClient _httpClient,
            string baseUrl, string xmlRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(xmlRequest, Encoding.UTF8, ContentType.XMLTEXT);

            return await _httpClient.SendAsync(_restRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="soapAction"></param>
        /// <param name="soapRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessPostSoapAsync(this HttpClient _httpClient,
            string baseUrl, string soapAction, string soapRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            if (soapAction != null)
                _restRequest.Headers.Add("SOAPAction", soapAction);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(soapRequest, Encoding.UTF8, ContentType.XMLTEXT);

            return await _httpClient.SendAsync(_restRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_httpClient"></param>
        /// <param name="baseUrl"></param>
        /// <param name="soapAction"></param>
        /// <param name="soapRequest"></param>
        /// <param name="headers"></param>
        /// <param name="parameters"></param>
        /// <returns>HttpResponseMessage</returns>
        public static async Task<HttpResponseMessage> ProcessPostSoapBasicAsync(this HttpClient _httpClient, string baseUrl, string soapAction,
            string userName, string password, string soapRequest, Dictionary<string, string> headers = null, Dictionary<string, string> parameters = null)
        {
            string _requestUrl = baseUrl;
            if (parameters != null && parameters.Any())
            {
                _requestUrl = QueryHelpers.AddQueryString(baseUrl, parameters);
            }
            HttpRequestMessage _restRequest = new HttpRequestMessage(HttpMethod.Post, _requestUrl);

            var _basicToken = Encoding.ASCII.GetBytes($"{userName}:{password}");
            _restRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(_basicToken));

            if (soapAction != null)
                _restRequest.Headers.Add("SOAPAction", soapAction);

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    _restRequest.Headers.Add(header.Key, header.Value);
                }
            }
            _restRequest.Content = new StringContent(soapRequest, Encoding.UTF8, ContentType.XMLTEXT);

            return await _httpClient.SendAsync(_restRequest);
        }


        // TODO: HttpUtil complete timeout implementation mcuh later
        // TODO: HttpUtil complete documentations and share much later
        // https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/

        /// <summary>
        /// Set the httpClient request timeout
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            request.Options.Set(new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey), timeout);
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Options.TryGetValue(new HttpRequestOptionsKey<TimeSpan?>(TimeoutPropertyKey),
                out TimeSpan? value) && value is TimeSpan timeout)
                return timeout;
            return null;
        }

        private static readonly string TimeoutPropertyKey = "RequestTimeout";
    }

    public class ContentType
    {
        public static string FORM { get { return "application/x-www-form-urlencoded"; } }
        public static string JSON { get { return "application/json"; } }
        public static string XML { get { return "application/xml"; } }
        public static string XMLTEXT { get { return "text/xml"; } }
    }

    public class HttpRespUtils
    {
        public static string GetStatusCodeMessage(HttpStatusCode statusCode)
        {
            return (int)statusCode switch
            {
                300 => "Multiple Choices",
                301 => "Moved Permanently",
                304 => "Not Modified",
                306 => "Switch Proxy",
                307 => "Temporary Redirect",
                308 => "Permanent Redirect",
                400 => "A bad request, you have made",
                401 => "Authorized, you are not",
                403 => "Forbidden from doing this, you are",
                404 => "Resource found, it was not",
                405 => "Method Not Allowed",
                406 => "Not Acceptable",
                407 => "Proxy Authentication Required",
                408 => "Request Timeout",
                409 => "Resource conflict",
                411 => "Length Required",
                412 => "Precondition Failed",
                413 => "Payload Too Large",
                414 => "URI Too Long",
                415 => "Unsupported Media Type",
                417 => "Expectation Failed",
                423 => "Locked",
                424 => "Failed Dependency",
                426 => "Upgrade Required",
                429 => "Too Many Requests",
                431 => "Request Header Fields Too Large",
                500 => "Internal Server Error",
                501 => "Not Implemented",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                504 => "Gateway Timeout",
                505 => "HTTP Version Not Supported",
                506 => "Variant Also Negotiates",
                507 => "Insufficient Storage",
                508 => "Loop Detected",
                510 => "Not Extended",
                511 => "Network Authentication Required",
                _ => null
            };
        }
    }

    // https://josef.codes/customize-the-httpclient-logging-dotnet-core/

    // https://dev.to/nikolicbojan/log-httpclient-request-and-response-based-on-custom-conditions-in-net-core-412f

    // https://mstack.nl/blogs/sanitize-http-logging/

    public class HttpLogger : IHttpClientLogger
    {
        private readonly ILogger<HttpLogger> _logger;
        private readonly IDBaseService _dbaseService;

        public HttpLogger(ILogger<HttpLogger> logger, IDBaseService dbaseService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbaseService = dbaseService;
        }

        public object LogRequestStart(HttpRequestMessage request)
        {
            string xRequestID = null;
            if (request.Headers.Contains(GeneralConst.HTTP_REQUEST_ID))
            {
                xRequestID = request.Headers.FirstOrDefault(x => x.Key == GeneralConst.HTTP_REQUEST_ID).Value.FirstOrDefault();
            }
            else
            {
                xRequestID = Guid.NewGuid().ToString();
                request.Headers.Add(GeneralConst.HTTP_REQUEST_ID, xRequestID);
            }
            var reqPayLoad = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            var requestLog = new ApiRequestLog(RequestDirection.OUTBOUND, xRequestID, request.Method.Method,
                string.Format("{0}{1}", request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
                request.RequestUri!.PathAndQuery), null, FormatRequestHeaders(request.Headers), string.Empty, reqPayLoad);
            _logger.LogInformation($">> {requestLog.FormatRequest()}");
            var res = _dbaseService.InsertRequestLogAsync(requestLog).GetAwaiter().GetResult();
            Console.WriteLine($"DBInsert LogRequestStart => {res}");
            return null;
        }

        public void LogRequestStop(object context, HttpRequestMessage request,
            HttpResponseMessage response, TimeSpan elapsed)
        {
            string xRequestID = null;
            if (request.Headers.Contains(GeneralConst.HTTP_REQUEST_ID))
                xRequestID = request.Headers.FirstOrDefault(x => x.Key == GeneralConst.HTTP_REQUEST_ID).Value.FirstOrDefault();

            int statusCode = (int)response.StatusCode;
            var respPayLoad = response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            var requestLog = new ApiRequestLog(RequestDirection.OUTBOUND)
            {
                RequestID = xRequestID,
                RespHeaders = FormatResponseHeaders(response.Headers),
                RespPayload = respPayLoad,
                Duration = Math.Round(elapsed.TotalMilliseconds, 4)
            };
            requestLog.SetResponse(statusCode, respPayLoad);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation($"<< {requestLog.FormatResponseSuccess()}");
            else
                _logger.LogError($"<< {requestLog.FormatResponseFailed()}");

            var res = _dbaseService.UpdateRequestLogAsync(requestLog).GetAwaiter().GetResult();
            Console.WriteLine($"DBUpdate LogRequestStop => {res}");
        }

        public void LogRequestFailed(object context, HttpRequestMessage request,
            HttpResponseMessage response, Exception exception, TimeSpan elapsed)
        {
            string xRequestID = null;
            if (request.Headers.Contains(GeneralConst.HTTP_REQUEST_ID))
                xRequestID = request.Headers.FirstOrDefault(x => x.Key == GeneralConst.HTTP_REQUEST_ID).Value.FirstOrDefault();

            int statusCode = 999;
            string respPayLoad = null;
            string respHeaders = null;
            if (response != null)
            {
                statusCode = (int)response.StatusCode;
                respPayLoad = response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
                respHeaders = FormatResponseHeaders(response.Headers);
            }
            else
            {
                respPayLoad = exception.GetBaseException().Message;
            }
            var requestLog = new ApiRequestLog(RequestDirection.OUTBOUND)
            {
                RequestID = xRequestID,
                RespHeaders = respHeaders,
                RespPayload = respPayLoad,
                Duration = Math.Round(elapsed.TotalMilliseconds, 4)
            };
            requestLog.SetResponse(statusCode, respPayLoad);
            _logger.LogError($"<< {requestLog.FormatResponseFailed()}");

            var res = _dbaseService.UpdateRequestLogAsync(requestLog).GetAwaiter().GetResult();
            Console.WriteLine($"DBUpdate LogRequestFailed => {res}");
        }

        private string FormatRequestHeaders(HttpRequestHeaders headers)
        {
            if (headers == null || headers.Count() == 0)
                return null;
            return string.Join(", ", headers.Where(x => !x.Key.Equals("Authorization"))
                .Select(kvp => $"{{{kvp.Key}: {string.Join(", ", kvp.Value!)}}}"));
        }

        private string FormatResponseHeaders(HttpResponseHeaders headers)
        {
            if (headers == null || headers.Count() == 0)
                return null;
            return string.Join(", ", headers.Where(x => !x.Key.Equals("Authorization"))
                .Select(kvp => $"{{{kvp.Key}: {string.Join(", ", kvp.Value!)}}}"));
        }

    }


    // https://www.milanjovanovic.tech/blog/extending-httpclient-with-delegating-handlers-in-aspnetcore
    public class RestRequestHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(GeneralConst.HTTP_REQUEST_ID))
            {
                string requestId = Guid.NewGuid().ToString();
                request.Headers.Add(GeneralConst.HTTP_REQUEST_ID, requestId);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }

}

using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Health")]
    [Route("api/health")]
    [ApiController]
    public class HealthController : MBaseController
    {
        protected ClientBranch _clientBranch = null;
        protected readonly IMasterDataSvc _masterDataSvc;

        private readonly IEtimsService _etimsService;
        private readonly IDigiTaxService _dTaxService;

        public HealthController(IEtimsService etimsService, IMasterDataSvc masterDataSvc, IDigiTaxService dTaxService)
        {
            _masterDataSvc = masterDataSvc;
            _etimsService = etimsService;
            _dTaxService = dTaxService;

            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Check Web Api and System Health
        /// </summary>
        /// <returns></returns>
        [HttpGet("echo")]
        public async Task<IActionResult> Echo(bool log = false)
        {
            string _method_ = "Echo";
            try
            {
                await Task.FromResult(0);

                var responseObject = new
                {
                    Status = string.Format($"eTims MidWare API reached at {DateTime.Now.ToString("s")}"),
                };
                if (log)
                {
                    /*string ipAddress = GetClientIpAddress();
                    UI.Info($"ClientIP: {ipAddress}, Method:{Request.Method}, Path: {Request.Path}");*/
                    UI.Info($"<< Status pinged: {responseObject.Status}");
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpGet("getversion")]
        public async Task<IActionResult> GetVersion(bool log = false)
        {
            string _method_ = "GetVersion";
            try
            {
                await Task.FromResult(0);
                var fileInfo = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                var responseObject = new
                {
                    Version = "v1.0.1",
                    BuildDate = fileInfo.LastWriteTime.ToString("s"),
                    TimeStamp = DateTime.Now.ToString("s")
                };
                if (log)
                {
                    /*string ipAddress = GetClientIpAddress();
                    UI.Info($"ClientIP: {ipAddress}, Method:{Request.Method}, Path: {Request.Path}");*/
                    UI.Info($"<< {_method_} : {responseObject}");
                }
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpGet("testetrconnect")]
        public async Task<IActionResult> TestETRConnect(bool log = false)
        {
            string _method_ = "TestETRConnect";
            try
            {
                if (_clientBranch.TaxClient.DeviceType == TaxDeviceType.DIGITAX)
                {
                    var result = await _dTaxService.GetBranchCount(log);
                    if (result.IsSuccess)
                        return Ok(result.GetValue());
                    else
                        return StatusCode(500, result.GetError());
                }
                else
                {
                    var result = await _etimsService.GetBranchCount(log);
                    if (result.IsSuccess)
                        return Ok(result.GetValue());
                    else
                        return StatusCode(500, result.GetError());
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpGet("testerpconnect")]
        public async Task<IActionResult> TestERPConnect()
        {
            string _method_ = "TestERPConnect";
            try
            {
                await Task.FromResult(0);
                return Ok(true);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [Authorize]
        [HttpGet("testauth")]
        public async Task<IActionResult> TestAuth()
        {
            string _method_ = "TestAuth";
            try
            {
                await Task.FromResult(0);
                var responseObject = new
                {
                    Status = string.Format($"eTims {_method_} API reached at {DateTime.Now.ToString("s")}"),
                };
                UI.Info($"<< {_method_} pinged: {responseObject.Status}");
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
    }
}

using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Health")]
    [Route("api/health")]
    [ApiController]
    public class HealthController : MBaseController
    {
        private readonly IEtimsService _etimsService;

        public HealthController(IEtimsService etimsService)
        {
            _etimsService = etimsService;
        }

        /// <summary>
        /// Check Web Api and System Health
        /// </summary>
        /// <returns></returns>
        [HttpGet("echo")]
        public async Task<IActionResult> Echo()
        {
            string _method_ = "Echo";
            try
            {
                await Task.FromResult(0);
                /*string ipAddress = GetClientIpAddress();
				UI.Info($"ClientIP: {ipAddress}, Method:{Request.Method}, Path: {Request.Path}");*/

                var responseObject = new
                {
                    Status = string.Format($"eTims MidWare API reached at {DateTime.Now.ToString("s")}"),
                };
                UI.Info($"<< Status pinged: {responseObject.Status}");
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpGet("testetrconnect")]
        public async Task<IActionResult> TestETRConnect()
        {
            string _method_ = "TestETRConnect";
            try
            {
                var result = await _etimsService.SelectBranches();
                if (result.IsSuccess)
                    return Ok(result.GetValue());
                else
                    return StatusCode(500, result.GetError());
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
                return Ok(true);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

    }
}

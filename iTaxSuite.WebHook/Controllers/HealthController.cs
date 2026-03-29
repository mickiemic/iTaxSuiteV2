using iTaxSuite.Library.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebHook.Controllers
{
    [Tags("Health")]
    [Route("api/health")]
    [ApiController]
    public class HealthController : MBaseController
    {

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
                    Status = string.Format($"eTims WebHook API reached at {DateTime.Now.ToString("s")}"),
                };
                if (log)
                {
                    string ipAddress = GetClientIpAddress();
                    UI.Info($"ClientIP: {ipAddress}, Method:{Request.Method}, Path: {Request.Path}");
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

    }
}

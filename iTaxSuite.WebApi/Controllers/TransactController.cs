using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Transaction")]
    [Route("api/transact")]
    [ApiController]
    public class TransactController : MBaseController
    {
        private readonly ITransactService _transactService;

        public TransactController(ITransactService transactService)
        {
            _transactService = transactService;
        }

        [HttpPost]
        [Route("gettransactions")]
        public async Task<IActionResult> GetTransactions(TransactFilter filter)
        {
            string _method_ = "GetTransactions";
            try
            {
                var result = await _transactService.GetTransactions(filter);
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
        [Route("posttransaction")]
        public async Task<IActionResult> PostTransaction()
        {
            string _method_ = "PostTransaction";
            try
            {
                var result = await _transactService.ExecTransaction();
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

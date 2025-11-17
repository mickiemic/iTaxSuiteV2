using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static iTaxSuite.Library.Models.Entities.S300TaxAuthority;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("Setup")]
    [Route("api/setup")]
    [ApiController]
    public class SetupController : MBaseController
    {
        private readonly ETimsDBContext _dbContext;
        private readonly ISetupService _setupService;
        private readonly IEtimsService _etimsService;

        public SetupController(ETimsDBContext dbContext, ISetupService setupService, IEtimsService etimsService)
        {
            _dbContext = dbContext;
            _setupService = setupService;
            _etimsService = etimsService;
        }

        [HttpGet("gettaxclient")]
        public async Task<IActionResult> GetTaxClient()
        {
            string _method_ = "GetTaxClient";
            try
            {
                var taxClient = await _dbContext.TaxClient.SingleOrDefaultAsync();
                return Ok(taxClient);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("setuptaxclient")]
        public async Task<IActionResult> SetupTaxClient()
        {
            await Task.FromResult(0);
            return StatusCode(500, "Not Yet Implemented");
        }

        [HttpGet("getbraches")]
        public async Task<IActionResult> GetBranches()
        {
            string _method_ = "GetBranches";
            try
            {
                var branches = await _dbContext.ClientBranch.Include(e => e.TaxClient).ToListAsync();
                return Ok(branches);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("getattributes")]
        public async Task<IActionResult> GetAttributes(AttributeFilter filter)
        {
            string _method_ = "GetAttributes";
            PagedResult<EntityAttribute> result = new();
            try
            {
                var query = _dbContext.EntityAttribute.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(filter.SearchKey))
                    query = query.Where(x => x.SearchKey.Equals(filter.SearchKey));

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("gettaxauthorities")]
        public async Task<IActionResult> GetTaxAuthorities(TaxAuthorityFilter filter)
        {
            string _method_ = "GetTaxAuthorities";
            PagedResult<S300TaxAuthority> result = new();
            try
            {
                var query = _dbContext.S300TaxAuthority.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(filter.AuthorityKey))
                    query = query.Where(x => x.AuthorityKey.Equals(filter.AuthorityKey));
                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    query = query.Where(x => x.Currency.Equals(filter.Currency));

                result.Count = await query.CountAsync();
                if (filter.Sort != null)
                    query = filter.PageAndOrder(query);
                else
                    query = filter.PageAndOrderByStamp(query);

                result.Result = await query.ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        [HttpPost("getusers")]
        public async Task<IActionResult> GetBranchUsers()
        {
            string _method_ = "GetBranchUsers";
            try
            {
                var branchUsers = await _dbContext.BranchUsers.Include(e => e.ClientBranch).ToListAsync();
                return Ok(branchUsers);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("registeruser")]
        public async Task<IActionResult> RegisterBranchUser()
        {
            await Task.FromResult(0);
            return StatusCode(500, "Not Yet Implemented");
        }

        [HttpPost("getcustomers")]
        public async Task<IActionResult> GetBranchCustomers()
        {
            string _method_ = "GetBranchCustomers";
            try
            {
                var branchCustomers = await _dbContext.BranchCustomers.Include(e => e.ClientBranch).ToListAsync();
                return Ok(branchCustomers);
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("registercustomer")]
        public async Task<IActionResult> RegisterBranchCustomer()
        {
            await Task.FromResult(0);
            return StatusCode(500, "Not Yet Implemented");
        }

        [HttpPost("getnotices")]
        public async Task<IActionResult> GetNotices()
        {
            string _method_ = "GetNotices";
            try
            {
                var result = await _etimsService.SelectNotices("20240101000000");
                if (result.IsSuccess)
                    return Ok(result.GetValue().NoticeData.NoticeList);
                else
                    return StatusCode(500, result.GetError());
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }

        /*[HttpGet("testdb")]
        public async Task<IActionResult> TestDatabase()
        {
            string _method_ = "TestDatabase";
            try
            {

                return Ok("All tests passed");
            }
            catch(Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException()}");
                return StatusCode(500, ex.GetBaseException().Message);
            }
        }*/

    }
}

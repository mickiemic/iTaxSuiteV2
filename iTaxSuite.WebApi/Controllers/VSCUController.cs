using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.WebApi.Controllers
{
    [Tags("VSCU Utils")]
    [Route("api/vscu")]
    [ApiController]
    public class VSCUController : MBaseController
    {
        protected ClientBranch _clientBranch = null;
        protected readonly IMasterDataSvc _masterDataSvc;

        private readonly IEtimsService _etimsService;

        public VSCUController(IMasterDataSvc masterDataSvc, IEtimsService etimsService)
        {
            _masterDataSvc = masterDataSvc;
            _etimsService = etimsService;

            _clientBranch = _masterDataSvc.GetBranchAsync().GetAwaiter().GetResult();
        }

        [HttpGet("getbranches")]
        public async Task<IActionResult> GetBranchList(string strInput = "20191130000000")
        {
            string _method_ = "GetBranchList";
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

        [HttpGet("getcodeslist")]
        public async Task<IActionResult> GetCodesList(string strInput = "20191130000000")
        {
            string _method_ = "GetCodesList";
            try
            {
                var result = await _etimsService.SelectCodes(strInput);
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

        [HttpGet("getitemlist")]
        public async Task<IActionResult> GetItemList(string strInput = "20191130000000")
        {
            string _method_ = "GetItemList";
            try
            {
                var result = await _etimsService.SelectItems(strInput);
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
        
        [HttpGet("getimportslist")]
        public async Task<IActionResult> GetImportsList(string strInput = "20191130000000")
        {
            string _method_ = "GetImportsList";
            try
            {
                var result = await _etimsService.SelectImportItems(strInput);
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
        
        [HttpGet("getstocklist")]
        public async Task<IActionResult> GetStockList(string strInput = "20191130000000")
        {
            string _method_ = "GetStockList";
            try
            {
                var result = await _etimsService.SelectStockItems(strInput);
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

        [HttpGet("getitemclasses")]
        public async Task<IActionResult> GetItemClasses(string strInput = "20191130000000")
        {
            string _method_ = "GetItemClasses";
            try
            {
                var result = await _etimsService.SelectItemClass(strInput);
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

        [HttpGet("getcustomers")]
        public async Task<IActionResult> GetCustomers(string strInput = "A987654321Z")
        {
            string _method_ = "GetCustomers";
            try
            {
                var result = await _etimsService.SelectCustomer(strInput);
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

        [HttpPost("savestockmaster")]
        public async Task<IActionResult> SaveStockMaster(StockIOSaveReq stockIOSaveReq)
        {
            string _method_ = "SaveStockMaster";
            try
            {
                var result = await _etimsService.SaveEtimsStockIO(stockIOSaveReq);
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

        [HttpPost("savecustomer")]
        public async Task<IActionResult> SaveCustomer(SaveBranchCustomerReq branchCustomerReq)
        {
            string _method_ = "SaveCustomer";
            try
            {
                var result = await _etimsService.SaveBranchCustomer(branchCustomerReq);
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

        [HttpPost("savebranchuser")]
        public async Task<IActionResult> SaveBranchUser(SaveBranchUserReq branchUserReq)
        {
            string _method_ = "SaveBranchUser";
            try
            {
                var result = await _etimsService.SaveBranchUser(branchUserReq);
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

        [HttpPost("savebranchinsurance")]
        public async Task<IActionResult> SaveBranchInsuracne(SaveBranchInsurReq branchInsurReq)
        {
            string _method_ = "SaveBranchInsuracne";
            try
            {
                var result = await _etimsService.SaveBranchInsurance(branchInsurReq);
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

    }
}
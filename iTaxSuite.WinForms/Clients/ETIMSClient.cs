using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using iTaxSuite.WinForms.Extensions;
using iTaxSuite.WinForms.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient : BaseForm
    {
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IS300ProductSvc _s300ProductSvc;
        private readonly IS300SaleService _saleService;
        private readonly IEtimsService _etimsService;
        private readonly VSCUConfig _vscuConfig;
        private readonly ETimsDBContext _dbContext;

        private readonly TestData _testData;
        public ETIMSClient(IMasterDataSvc masterDataSvc, IS300SaleService s300SaleService, VSCUConfig vscuConfig,
            ETimsDBContext dbContext, IEtimsService etimsService, IS300ProductSvc s300ProductSvc)
        {
            _masterDataSvc = masterDataSvc;
            _saleService = s300SaleService;
            _vscuConfig = vscuConfig;
            _dbContext = dbContext;
            _etimsService = etimsService;
            _s300ProductSvc = s300ProductSvc;

            InitializeComponent();
            FormClosing += MFormClosing;
            KeyDown += OnKeyDown;

            EditorHelper.initSyntaxColoring(reqEditor);
            EditorHelper.initCodeFolding(reqEditor);
            EditorHelper.initSyntaxColoring(respEditor);
            EditorHelper.initCodeFolding(respEditor);

            if (_vscuConfig.ClientCode == "TSCLTD")
            {
                _testData = new TestData()
                {
                    ICItem = "SPA001",
                    OEInvoice = "IN008422",
                    OECreditNote = "CN000299",
                    ARBatch = "9070",
                    ARInvoice = "",
                    ARCreditNote = "",
                    POReceipt = "ZO01"
                };
            }
            else if (_vscuConfig.ClientCode == "CARLTD")
            {
                _testData = new TestData()
                {
                    ICItem = "BX12024",
                    OEInvoice = "INV00012",
                    OECreditNote = "CN000001",
                    ARBatch = "56",
                    ARInvoice = "IN000204",
                    ARCreditNote = "",
                    POReceipt = ""
                };
            }
            else if (_vscuConfig.ClientCode == "LFKDB")
            {
                _testData = new TestData()
                {
                    ICItem = "BK001",
                    OEInvoice = "INV20242476",
                    OECreditNote = "CN20230069",
                    ARBatch = "22",
                    ARInvoice = "INV2022/179",
                    ARCreditNote = "",
                    POReceipt = "VINV2133"
                };
            }
            StartupSetup();
        }
        private async void StartupSetup()
        {
            ShowLoadingScreen(this, "Setting Up Tax Metadata");
            await _masterDataSvc.InitiateTaxSetup();
            await _masterDataSvc.InitializeCacheData();
            HideLoadingScreen();
        }

        public int GetCurrenttab()
        {
            return tabControlEtims.SelectedIndex;
        }
        public void SetCurrentTab(int tabIndex)
        {
            tabControlEtims.SelectedIndex = tabIndex;
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Space)
            {
                MessageBox.Show("Posting Request...");
            }
        }

        private void MFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private async void btnSetupTaxes_Click(object sender, EventArgs e)
        {
            ShowLoadingScreen(this, "Setting Up Tax Metadata");
            var result = await _masterDataSvc.InitiateTaxSetup();
            //await Task.Delay(TimeSpan.FromSeconds(3));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Tax Setup");
                return;
            }
        }

        private async void btnGetOEInvoice_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetOEInvoice_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strInput = Interaction.InputBox("Enter Invoice Number", "Select OE Invoice", _testData.OEInvoice);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Select OE Invoice");
                    return;
                }

                ShowLoadingScreen(this, $"Loading OE Invoice Number {strInput}");
                var convertRes = await _saleService.GetConvertOEInvoice(new SaleTrxKey { DocNumber = strInput });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    //reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select OE Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetOECRNote_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetOECRNote_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strInput = Interaction.InputBox("Enter OE CRNote Number", "Select OE CRNote", _testData.OECreditNote);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Select OE CRNote");
                    return;
                }

                ShowLoadingScreen(this, $"Loading OE CRNote Number {strInput}");
                var convertRes = await _saleService.GetConvertOECRNote(new SaleTrxKey { DocNumber = strInput });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    //reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select OE CRNote", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE CRNote Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetARInvoice_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetARInvoice_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strBatch = Interaction.InputBox("Enter Batch Number", "Enter AR Batch Number", _testData.ARBatch);
                if (string.IsNullOrWhiteSpace(strBatch))
                {
                    MessageBox.Show($"Invalid Request {strBatch}", "Select Item");
                    return;
                }
                string strInvoice = Interaction.InputBox("Enter Invoice Number", "Enter AR Invoice Number", _testData.ARInvoice);
                if (string.IsNullOrWhiteSpace(strInvoice))
                {
                    strInvoice = string.Empty;
                }

                UI.Info($"{_method_} running..");
                ShowLoadingScreen(this, $"Loading AR Batch Number {strBatch}, Invoice: {strInvoice}");
                var convertRes = await _saleService.GetConvertARInvoice(new SaleBatchTrxKey { BatchNumber = strBatch, DocNumber = strInvoice });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    // reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select AR Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "AR Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetARCRNote_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetARCRNote_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strBatch = Interaction.InputBox("Enter Batch Number", "Enter AR Batch Number", _testData.ARBatch);
                if (string.IsNullOrWhiteSpace(strBatch))
                {
                    MessageBox.Show($"Invalid Request {strBatch}", "Select Item");
                    return;
                }
                string strInvoice = Interaction.InputBox("Enter Credit Note Number", "Enter AR Credit Note Number", _testData.ARInvoice);
                if (string.IsNullOrWhiteSpace(strInvoice))
                {
                    strInvoice = string.Empty;
                }

                UI.Info($"{_method_} running..");
                ShowLoadingScreen(this, $"Loading AR Batch Number {strBatch}, Credit Note: {strInvoice}");
                var convertRes = await _saleService.GetConvertARCRNote(new SaleBatchTrxKey { BatchNumber = strBatch, DocNumber = strInvoice });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    // reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select AR Credit Note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "AR Credit Note Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSaveItem_Click(object sender, EventArgs e)
        {
            string _method_ = "btnSaveItem_Click";
            string _strError = string.Empty;
            try
            {
                string strInput = Interaction.InputBox("Enter Item Code", "Create Item", _testData.ICItem);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Create Item");
                    return;
                }

                var stockItem = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => e.ProductCode == strInput).OrderBy(e => e.CreatedOn)
                    .AsNoTracking().FirstOrDefaultAsync();
                var etimsRequest = stockItem.Product.ProductData.GetEtimsRequest();
                if (stockItem != null)
                {
                    respEditor.ClearAll();
                    respEditor.setEditorText(etimsRequest);

                    if (stockItem.RecordStatus == RecordStatus.POST_OK || stockItem.RecordStatus == RecordStatus.POST_DUPL)
                    {
                        if (MessageBox.Show($"{stockItem.Product} Already Registeded Fully Successfully. Do you want to update it?", "Update Item",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"{stockItem.Product} Already exists but failed. Do you want to update it?", "Update Item",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Item does not exist!", "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ShowLoadingScreen(this, $"Creating Product Code {strInput}");
                var eTimsResp = await _etimsService.CreateEtimsItem(etimsRequest);
                HideLoadingScreen();
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    var tStamp = DateTime.Now;
                    var recordStatus = RecordStatus.POST_FAIL;

                    if (eTimsResp.IsError)
                    {
                        _strError = eTimsResp.GetError();
                        UI.Error($"Saving Stock Item:{stockItem.CacheKey} failed: {eTimsResp.GetError()}");

                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _strError)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _strError)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        await _dbTrans.CommitAsync();

                        MessageBox.Show(_strError, "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        SaveItemResp saveItemResp = eTimsResp.GetValue();
                        _strError = saveItemResp.RawResponse;
                        recordStatus = RecordStatus.POST_OK;

                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, saveItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        await _dbTrans.CommitAsync();

                        MessageBox.Show(_strError, "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error($"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSelectCodes_Click(object sender, EventArgs e)
        {
            string strInput = Interaction.InputBox("Enter Last Request Date", "Select Codes", "20191130000000");
            if (string.IsNullOrWhiteSpace(strInput))
            {
                MessageBox.Show($"Invalid Request {strInput}", "Select Item");
                return;
            }

            ShowLoadingScreen(this, $"Loading Codes Request DT:{strInput}");
            var result = await Task.Run(() => _etimsService.SelectCodes(strInput));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Codes");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));
        }

        private async void btnSelectItem_Click(object sender, EventArgs e)
        {
            string strInput = Interaction.InputBox("Enter Last Request Date", "Select Codes", "20191130000000");
            if (string.IsNullOrWhiteSpace(strInput))
            {
                MessageBox.Show($"Invalid Request {strInput}", "Select Item");
                return;
            }

            ShowLoadingScreen(this, $"Loading Items Request DT:{strInput}");
            var result = await Task.Run(() => _etimsService.SelectItems(strInput));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Items");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));

            await SyncLocalProducts(result.GetValue());
        }

        private async void btnClearSales_Click(object sender, EventArgs e)
        {
            //await GenSalesItems();

            /*ShowLoadingScreen(this, $"Query SaleTransact");
            var result = await _saleService.QuerySaleTransact(new SaleTrxKey { DocNumber = "INV00005" });
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Query SaleTransact");
            }
            else
            {
                MessageBox.Show($"SaleTransact status is OK", "Query SaleTransact");
            }*/

            ShowLoadingScreen(this, "Querying Products");
            var glProducts = await _s300ProductSvc.FetchGLProducts();
            var strJson1 = JsonConvert.SerializeObject(glProducts);
            var arProducts = await _s300ProductSvc.FetchARProducts();
            var strJson2 = JsonConvert.SerializeObject(arProducts);
            HideLoadingScreen();
            Console.WriteLine($"GLProducts: {glProducts?.Count}, ARProducts:{arProducts?.Count}");
        }
    }

}

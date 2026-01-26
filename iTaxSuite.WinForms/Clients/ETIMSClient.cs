using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using iTaxSuite.WinForms.Extensions;
using iTaxSuite.WinForms.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient : BaseForm
    {
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly IS300SaleService _saleService;
        private readonly VSCUConfig _vscuConfig;

        private readonly TestData _testData;
        public ETIMSClient(IMasterDataSvc masterDataSvc, IS300SaleService s300SaleService, VSCUConfig vscuConfig)
        {
            _masterDataSvc = masterDataSvc;
            _saleService = s300SaleService;
            _vscuConfig = vscuConfig;

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
                    ARBatch = "22",
                    ARInvoice = "IN000115",
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

                ShowLoadingScreen(null, $"Loading OE Invoice Number {strInput}");
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

                ShowLoadingScreen(null, $"Loading OE CRNote Number {strInput}");
                var convertRes = await _saleService.GetConvertOECRNote(new SaleTrxKey { DocNumber = strInput });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    //reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
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
                ShowLoadingScreen(null, $"Loading AR Batch Number {strBatch}, Invoice: {strInvoice}");
                var convertRes = await _saleService.GetConvertARInvoice(new SaleBatchTrxKey { BatchNumber = strBatch, DocNumber = strInvoice });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    // reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                    reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
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
    }

}

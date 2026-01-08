using iTaxSuite.Library.Services;
using iTaxSuite.WinForms.Extensions;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient : BaseForm
    {
        private readonly IMasterDataSvc _masterDataSvc;
        //public ETIMSClient()
        public ETIMSClient(IMasterDataSvc masterDataSvc)
        {
            _masterDataSvc = masterDataSvc;

            InitializeComponent();
            FormClosing += MFormClosing;
            KeyDown += OnKeyDown;

            EditorHelper.initSyntaxColoring(reqEditor);
            EditorHelper.initCodeFolding(reqEditor);
            EditorHelper.initSyntaxColoring(respEditor);
            EditorHelper.initCodeFolding(respEditor);
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
    }
}

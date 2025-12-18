using iTaxSuite.WinForms.Extensions;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient : BaseForm
    {
        public ETIMSClient()
        {
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
    }
}

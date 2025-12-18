using iTaxSuite.Library.Extensions;
using iTaxSuite.WinForms.Clients;
using iTaxSuite.WinForms.Properties;
using System.Diagnostics;

namespace iTaxSuite.WinForms
{
    public partial class FormsTaxHost : Form
    {
        private readonly ETIMSClient _eTIMSClient;

        private readonly HashSet<string> _clients;
        public FormsTaxHost(ETIMSClient eTIMSClient)
        {
            _clients = new HashSet<string>()
            {
                "ETIMSClient", "ZFPClient", "TevinClient"
            };

            _eTIMSClient = eTIMSClient;

            Load += FormsTaxHost_Load;
            FormClosing += FormsTaxHost_FormClosing;

            InitializeComponent();
            SetStatus("We are loaded...");
        }

        private void SetStatus(string status)
        {
            toolStripStatusLabel.Text = status;
        }
        public void ClearStatus()
        {
            toolStripStatusLabel.Text = string.Empty;
        }

        private void RefreshMenuItems()
        {
            var activeClients = MdiChildren.Select(x => x.Name).ToList();

            foreach (var client in _clients)
            {
                if (activeClients.Contains(client))
                {
                    Debug.WriteLine($"{client} is ACTIVE");
                }
                else
                {
                    Debug.WriteLine($"{client} is CLOSED");
                    if (client.Equals("ETIMSClient"))
                    {
                        eTimsCloseMenuItem.Enabled = false;
                    }
                }
            }
        }

        private void FormsTaxHost_Load(object? sender, EventArgs e)
        {
            if (Settings.Default.WindowLocation != Point.Empty)
            {
                Location = Settings.Default.WindowLocation;
            }
            if (Settings.Default.WindowSize != Size.Empty)
            {
                Size = Settings.Default.WindowSize;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.LastForm))
            {
                if (Settings.Default.LastForm.Equals("ETIMSClient"))
                {
                    LoadETIMSClient();
                }
                else if (Settings.Default.LastForm.Equals("ZFPClient"))
                {
                    //LoadZFPClient();
                }
                else if (Settings.Default.LastForm.Equals("TevinClient"))
                {
                    //LoadTevinClient();
                }
            }
        }

        private void FormsTaxHost_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Settings.Default.WindowLocation = this.Location;
            Settings.Default.WindowSize = this.Size;
            string LastForm = string.Empty;
            if (ActiveMdiChild != null && !string.IsNullOrWhiteSpace(ActiveMdiChild.Name))
            {
                LastForm = ActiveMdiChild.Name;
            }
            if (_eTIMSClient is not null)
            {
                Settings.Default.ETIMSLastTab = _eTIMSClient.GetCurrenttab();
            }
            /*if (_zfpClient is not null)
            {
                Settings.Default.ZFPLastTab = _zfpClient.GetCurrenttab();
            }*/
            Settings.Default.LastForm = LastForm;
            Settings.Default.Save();
        }

        private void eTimsLaunchMenuItem_Click(object sender, EventArgs e)
        {
            LoadETIMSClient();
            RefreshMenuItems();
        }
        private void LoadETIMSClient()
        {
            _eTIMSClient.MdiParent = this;
            _eTIMSClient.StartPosition = FormStartPosition.CenterParent;
            _eTIMSClient.WindowState = FormWindowState.Minimized;
            _eTIMSClient.WindowState = FormWindowState.Maximized;
            _eTIMSClient.SetCurrentTab(Settings.Default.ETIMSLastTab);
            _eTIMSClient.Show();

            eTimsCloseMenuItem.Enabled = true;
            eTimsProcessRequest.Enabled = true;
        }

        private void zfpLaunchMenuItem_Click(object sender, EventArgs e)
        {
            /*LoadZFPClient();
            RefreshMenuItems();*/
        }

        private void tevinLaunchMenuItem_Click(object sender, EventArgs e)
        {
            /*LoadTevinClient();
            RefreshMenuItems();*/
        }

        private void eTimsCloseMenuItem_Click(object sender, EventArgs e)
        {
            //HideETIMSClient();
        }

        private void zfpCloseMenuItem_Click(object sender, EventArgs e)
        {
            //HideZFPClient();
        }

        private void tevinCloseMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}

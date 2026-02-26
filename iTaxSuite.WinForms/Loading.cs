namespace iTaxSuite.WinForms
{
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
            Load += Loading_Load;
        }

        private void Loading_Load(object? sender, EventArgs e)
        {
            this.CenterToParent();
        }

        public void SetMessage(string Message)
        {
            txtLoading.Text = Message;
        }
    }
}

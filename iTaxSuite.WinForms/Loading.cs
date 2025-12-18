namespace iTaxSuite.WinForms
{
    public partial class Loading : Form
    {
        public Loading()
        {
            InitializeComponent();
        }

        public void SetMessage(string Message)
        {
            txtLoading.Text = Message;
        }
    }
}

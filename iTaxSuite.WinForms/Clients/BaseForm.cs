namespace iTaxSuite.WinForms.Clients
{
    public partial class BaseForm : Form
    {
        private Loading? _loading;
        private Form? _parent;
        protected void ShowLoadingScreen(Form parent, string Message)
        {
            _loading = new Loading();
            _loading.SetMessage(Message);
            _loading.Show(parent);
            _parent = parent;

            if (_parent is not null)
                _parent.Enabled = false;
        }

        protected void UpdateProgress(string Progress)
        {
            _loading.SetMessage(Progress);
        }

        protected void HideLoadingScreen()
        {
            if (_loading != null)
                _loading.Hide();
            if (_parent != null)
                _parent.Enabled = true;
        }
    }
}

using iTaxSuite.Library.Extensions;

namespace iTaxSuite.WinForms.Models
{
    public class VSCUConfig
    {
        public string ClientCode { get; set; }
        public string PINNumber { get; set; }
        public string ClientName { get; set; }
        public string BranchID { get; set; } = "00";
        public string Address { get; set; }
        public string BaseDir { get; set; }

        public void InitializeConfig()
        {
            string _method_ = "InitializeConfig";
            try
            {
                BaseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EbmData", $"{PINNumber}_{BranchID}");
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                throw;
            }
        }
    }
}

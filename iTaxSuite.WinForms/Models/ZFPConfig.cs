namespace iTaxSuite.WinForms.Models
{
    internal class ZFPETRConfig
    {
        public string ETRScheme { get; set; } = "http";
        public string ETRAddress { get; set; }
        public int ETRPort { get; set; }
        public string EXTAddress { get; set; }
        public int EXTPort { get; set; }
        public string EXTPassword { get; set; }
        public string GetETRBaseUrl
        {
            get
            {
                return $"{ETRScheme}://{ETRAddress}:{ETRPort}";
            }
        }
    }
}

namespace iTaxSuite.WinForms.Models
{
    internal class ITaxConfig
    {
        public bool SecureSettings { get; set; }
        public List<string> TaxableCustomers { get; set; } = new();
        public List<string> ExemptCustomers { get; set; } = new();
        public string ExportHSCode { get; set; }
        public string ExportHSName { get; set; }

        /*
         1	
        TaxRate	0
        HSCode	"0001.12.00"
        HSDesc	"The exportation of g"
        FullDescription	"The exportation of goods"
         */

        public bool IsValid()
        {
            if (TaxableCustomers == null || !TaxableCustomers.Any()
                || ExemptCustomers == null || !ExemptCustomers.Any()
                || string.IsNullOrWhiteSpace(ExportHSCode) || string.IsNullOrWhiteSpace(ExportHSName))
            {
                return false;
            }

            foreach (var taxable in TaxableCustomers)
            {
                foreach (var excempt in ExemptCustomers)
                {
                    if (taxable == excempt)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}

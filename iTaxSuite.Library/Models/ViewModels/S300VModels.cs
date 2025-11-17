using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Models.Entities;
using System.Net;

namespace iTaxSuite.Library.Models.ViewModels
{
    public abstract class Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("@odata.context")]
        public string context { get; set; }

        [Newtonsoft.Json.JsonProperty("@odata.count")]
        public int count { get; set; }

        [Newtonsoft.Json.JsonProperty("@odata.nextLink")]
        public string nextLink { get; set; }
    }
    public class Sage300ERPError
    {
        public Error error { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string defaultMessage { get; set; }

        public string GetMessage()
        {
            if (!string.IsNullOrWhiteSpace(defaultMessage))
                return defaultMessage;

            if (error == null || string.IsNullOrWhiteSpace(error.code) || error.message == null)
                return null;

            return $"{error.code} :: {error.message.value}";
        }

        public ApiErrorResp GetApiErrorResp(HttpStatusCode statusCode)
        {
            ApiErrorResp resp = new ApiErrorResp(statusCode, GetMessage());

            if (statusCode == HttpStatusCode.Conflict)
            {
                if (error.code == "RecordDuplicate")
                {
                    if (resp.Message.Contains("Day End Processing", StringComparison.InvariantCultureIgnoreCase))
                    {
                        error.code = GeneralConst.DAY_END_ERRORCODE;
                        resp.HasDayEndError = true;
                    }
                    else
                    {
                        resp.RecordDuplicate = true;
                    }
                }
            }
            return resp;
        }

        public Sage300ERPError() { }

    }
    public class Error
    {
        public string code { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public string lang { get; set; }
        public string value { get; set; }
    }

    public class SageDocFilter
    {
        public string docKey { get; set; }
        public string docNumber { get; set; }
        public string dateField { get; set; }
        public string extraFilter { get; private set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public bool IsDateFilter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(dateField) || startDate.HasValue || endDate.HasValue)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsDocFilter
        {
            get
            {
                if (string.IsNullOrWhiteSpace(docKey) || string.IsNullOrWhiteSpace(docNumber))
                {
                    return false;
                }
                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (IsDateFilter)
                {
                    if (string.IsNullOrWhiteSpace(dateField) || !startDate.HasValue)
                    {
                        return false;
                    }
                    if (endDate.HasValue && startDate.Value.CompareTo(endDate.Value) > 0)
                    {
                        return false;
                    }
                }
                else if (!IsDocFilter)
                {
                    return !string.IsNullOrWhiteSpace(extraFilter);
                }
                return true;
            }
        }

        public void SetExtraFilter(string extraFilter)
        {
            this.extraFilter = extraFilter;
        }

        public bool IsSameDay()
        {
            if (!IsDateFilter || !IsValid)
            {
                return false;
            }
            if (!endDate.HasValue)
            {
                return true;
            }
            return startDate.Value.Date.CompareTo(endDate.Value.Date) == 0;
        }

        public string GetFilterString()
        {
            List<string> list = new List<string>();
            if (IsValid)
            {
                if (IsDateFilter)
                {
                    if (IsSameDay())
                    {
                        list.Add(string.Format("{0} eq {1}Z", dateField, startDate.Value.Date.ToString("s")));
                    }
                    else
                    {
                        list.Add(string.Format("{0} ge {1}Z and {0} le {2}Z", dateField, startDate.Value.Date.ToString("s"), endDate.Value.Date.ToString("s")));
                    }
                }
                if (IsDocFilter)
                {
                    list.Add($"{docKey} eq '{docNumber}'");
                }
            }
            else if (!string.IsNullOrWhiteSpace(dateField))
            {
                list.Add(string.Format("{0} eq {1}Z", dateField, DateTime.Now.Date.ToString("s")));
            }
            if (!string.IsNullOrWhiteSpace(extraFilter))
            {
                list.Add(extraFilter);
            }
            return string.Join(" and ", list);
        }

        public override string ToString()
        {
            return string.Format($"docKey:{docKey}, docNumber:{docNumber}, dateField:{dateField}, startDate:{startDate}, endDate:{endDate}, extraFilter: {extraFilter}");
        }

        public SageDocFilter()
        {
        }

        public SageDocFilter(string extraFilter)
        {
            this.extraFilter = extraFilter;
        }
    }

    public class TXTaxGroups : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxGroup> TaxGroups { get; set; }
    }

    public class TXTaxAuthorities : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority> Authories { get; set; }
    }
    public class TXTaxRates : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxRate> Rates { get; set; }
    }

    public class S300TaxGroup
    {
        public string GroupKey { get; set; }
        public string GroupTransactionType { get; set; }
        public string GroupCurrency { get; set; }
        public string GroupTitle { get; set; }
        public DateTime LastMaintained { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string CacheKey => $"{GroupKey}:{GroupCurrency}:{GroupTransactionType}";
        public Dictionary<string, S300TaxAuthority> Authorities { get; set; } = new();
        public S300TaxGroup()
        {
        }

        public S300TaxGroup(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxGroup group)
            : base()
        {
            GroupKey = group.TaxGroupKey;
            GroupTransactionType = Enum.GetName(group.TransactionType);
            GroupCurrency = group.TaxReportingCurrency;
            GroupTitle = group.Description;
            LastMaintained = group.LastMaintained.Value;
        }
    }
    public class S300TaxClass
    {
        public int ClassKey { get; set; }
        public string ClassType { get; set; }
        public string TransactionType { get; set; }
        public string ClassTitle { get; set; }
        public bool Exempt { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string CacheKey => $"{ClassKey}:{ClassType}:{TransactionType}";
        public S300TaxClass()
        {
        }
        public S300TaxClass(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass tClass)
            : base()
        {
            ClassKey = tClass.Class;
            ClassType = Enum.GetName(tClass.ClassType);
            TransactionType = Enum.GetName(tClass.TransactionType);
            Exempt = tClass.Exempt;
            ClassTitle = tClass.Description;
        }
    }
    public class S300TaxRate
    {
        public int BuyerClass { get; set; }
        public string TransactionType { get; set; }
        public decimal ItemRate1 { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string CacheKey => $"{BuyerClass}:{TransactionType}:{BuyerClass}";
        public S300TaxRate()
        {
        }
        public S300TaxRate(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxRate tRate)
            : base()
        {
            BuyerClass = tRate.BuyerClass;
            TransactionType = Enum.GetName(tRate.TransactionType);
            ItemRate1 = tRate.ItemRate1;
        }
    }
    public class S300TXTaxRate
    {
        public string TaxAuthority { get; set; }
        public string TransactionType { get; set; }
        public int Class { get; set; }
        public decimal ItemRate1 { get; set; }
    }

    // IC Models
    public class ICItems : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Item> Items { get; set; }
    }

    public class ICItem : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("DateLastMaintained")]
        public DateTime LastUpdated { get; set; }
    }

    public class ICReceipts : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Receipt> Receipts { get; set; }
    }

    // PO Models
    public class POInvoices : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice> Invoices { get; set; }
        [Newtonsoft.Json.JsonProperty("error")]
        public string apiError { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrWhiteSpace(apiError);
        }
    }

    // OE Models
    public class OEInvoices : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice> Invoices { get; set; }

        [Newtonsoft.Json.JsonProperty("error")]
        public string apiError { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrWhiteSpace(apiError);
        }
    }

    public class OECreditDebitNotes : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote> CreditDebitNotes { get; set; }

        [Newtonsoft.Json.JsonProperty("error")]
        public string apiError { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrWhiteSpace(apiError);
        }
    }

    // AR Models
    public class ARCustomers : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer> Customers { get; set; }
    }
    public class ARInvoiceBatches : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.InvoiceBatch> InvoiceBatches { get; set; }

        [Newtonsoft.Json.JsonProperty("error")]
        public string apiError { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrWhiteSpace(apiError);
        }
    }

    // AP Vendors
    public class APVendors : Sage300ERPResp
    {
        [Newtonsoft.Json.JsonProperty("value")]
        public List<Sage.CA.SBS.ERP.Sage300.AP.WebApi.Models.Vendor> Vendors { get; set; }
    }


}

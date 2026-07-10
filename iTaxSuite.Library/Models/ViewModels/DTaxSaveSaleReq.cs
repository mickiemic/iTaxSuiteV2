using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;

namespace iTaxSuite.Library.Models.ViewModels
{
    public class DTaxSaveSaleReq
    {
        [Newtonsoft.Json.JsonProperty("sale_date")]
        [System.Text.Json.Serialization.JsonPropertyName("sale_date")]
        public string SaleDate { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_tin", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("customer_tin")]
        public string CustomerPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_name", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_id", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("customer_id")]
        public string CustomerID { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_type_code")]
        public string ReceiptTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("payment_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("payment_type_code")]
        public string PaymentTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_status_code")]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_status_code")]
        public string InvoiceStatusCode { get; set; }
        [Newtonsoft.Json.JsonProperty("callback_url", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("callback_url")]
        public string CallbackURL { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_details", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_details")]
        public string InvoiceDetails { get; set; }
        [Newtonsoft.Json.JsonProperty("is_tax_exempt", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("is_tax_exempt")]
        public bool IsTaxExempt { get; set; }
        [Newtonsoft.Json.JsonProperty("service_charge_rate", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("service_charge_rate")]
        public double ServiceChargeRate { get; set; }
        [Newtonsoft.Json.JsonProperty("items")]
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<DTaxSaleItem> ItemList { get; set; } = new();

        public DTaxSaveSaleReq()
        {
        }

        public DTaxSaveSaleReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arInvoice,
            SalesTransact salesTransact, S300TaxGroup taxGroup, HashSet<string> taxAuthKeysx,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer) : this()
        {
            TraderInvoiceNo = arInvoice.DocumentNumber;
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.INVOICE);
            PaymentTypeCode = "01"; // ETimsUtils.ConvertPaymentMethod(arInvoice.PaymentType);
            InvoiceStatusCode = "02"; // ETimsUtils.ConvertTransactionStatus(invoice.InvoiceStatus);
            InvoiceDetails = arInvoice.InvoiceDescription;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/salesync";

            if (arInvoice.DocumentDate.HasValue)
                SaleDate = arInvoice.DocumentDate.Value.ToString(DTaxConst.FMT_DATEONLY);

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();
            }

            foreach (var _salesItem in salesTransact.SalesItems)
            {
                var salesItem = new DTaxSaleItem(_salesItem);
                ItemList.Add(salesItem);
            }
        }

        public DTaxSaveSaleReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice oeInvoice,
            SalesTransact salesTransact, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer) : this()
        {
            TraderInvoiceNo = oeInvoice.InvoiceNumber;
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.INVOICE);
            PaymentTypeCode = ETimsUtils.ConvertPaymentMethod(oeInvoice.PaymentType);
            InvoiceStatusCode = ETimsUtils.ConvertTransactionStatus(oeInvoice.InvoiceStatus);
            InvoiceDetails = oeInvoice.Description;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/salesync";

            if (oeInvoice.InvoiceDate.HasValue)
                SaleDate = oeInvoice.InvoiceDate.Value.ToString(DTaxConst.FMT_DATEONLY);

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();
            }

            foreach (var _salesItem in salesTransact.SalesItems)
            {
                var salesItem = new DTaxSaleItem(_salesItem);
                ItemList.Add(salesItem);
            }
        }

        public DTaxSaveSaleReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote,
            SalesTransact salesTransact, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer) : this()
        {
            TraderInvoiceNo = crNote.CreditDebitNoteNumber;
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.CREDITNOTE);
            //PaymentTypeCode = ETimsUtils.ConvertPaymentMethod(crNote.PaymentType);
            InvoiceStatusCode = ETimsUtils.ConvertTransactionStatus(crNote.CreditDebitNoteStatus);
            InvoiceDetails = crNote.Description;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/salesync";

            if (crNote.CreditDebitNoteDate.HasValue)
                SaleDate = crNote.CreditDebitNoteDate.Value.ToString(DTaxConst.FMT_DATEONLY);

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();
            }

            foreach (var _salesItem in salesTransact.SalesItems)
            {
                var salesItem = new DTaxSaleItem(_salesItem);
                ItemList.Add(salesItem);
            }
        }

        public string GetError()
        {
            return null;
        }

    }
    public class DTaxSaleItem
    {
        [Newtonsoft.Json.JsonProperty("item_bar_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_bar_code")]
        public string BarCode { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("total_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_quantity", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_quantity")]
        public decimal Package { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_rate", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("item_description", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("item_description")]
        public string ItemDescription { get; set; }
        [Newtonsoft.Json.JsonProperty("is_stockable", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("is_stockable")]
        public bool IsStockable { get; set; }
        [Newtonsoft.Json.JsonProperty("item_class_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_class_code")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_name")]
        [System.Text.Json.Serialization.JsonPropertyName("item_name")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("item_tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_tax_type_code")]
        public string ItemTaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount")]
        public decimal TaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }

        public DTaxSaleItem()
        {
        }
        public DTaxSaleItem(SalesItem salesItem)
            : this()
        {
            BarCode = salesItem.ProductCode;
            ItemTaxTypeCode = salesItem.TaxTypeCode;
            ItemClassCode = salesItem.ItemClassCode;
            IsStockable = salesItem.IsStockable;
            ItemName = salesItem.Description;
            ItemDescription = salesItem.Description;
            Quantity = salesItem.Quantity;
            Package = salesItem.Package;
            UnitPrice = salesItem.TotalAmount / salesItem.Quantity;
            DiscountRate = salesItem.DiscountRate;
            DiscountAmount = salesItem.DiscountAmount;
            TaxableAmount = salesItem.TaxableAmount;
            TaxAmount = salesItem.TaxAmount;
            TotalAmount = salesItem.TotalAmount;
        }
    }
    /*public class DTaxSaleItem
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("total_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_quantity", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_quantity")]
        public decimal Package { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_rate", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("item_description", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("item_description")]
        public string ItemDescription { get; set; }
        public DTaxSaleItem()
        {
        }
        public DTaxSaleItem(SalesItem salesItem)
            : this()
        {
            ID = salesItem.ExternalID;
            ItemDescription = salesItem.Description;
            Quantity = salesItem.Quantity;
            Package = salesItem.Package;
            UnitPrice = salesItem.TotalAmount / salesItem.Quantity;
            DiscountRate = salesItem.DiscountRate;
            DiscountAmount = salesItem.DiscountAmount;
            TotalAmount = salesItem.TotalAmount;
        }
    }*/

    public class DTaxSaveSaleResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("date")]
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; }
        [Newtonsoft.Json.JsonProperty("time")]
        [System.Text.Json.Serialization.JsonPropertyName("time")]
        public string Time { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_type_code")]
        public string ReceiptTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("sale_detail_url")]
        [System.Text.Json.Serialization.JsonPropertyName("sale_detail_url")]
        public string SaleDetailURL { get; set; }
        [Newtonsoft.Json.JsonProperty("serial_number")]
        [System.Text.Json.Serialization.JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_number")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_number")]
        public int ReceiptNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_number")]
        public int InvoiceNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_id")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_id")]
        public string CustomerID { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_name")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_tin")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_tin")]
        public string CustomerPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("internal_data")]
        [System.Text.Json.Serialization.JsonPropertyName("internal_data")]
        public string InternalData { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_signature")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_signature")]
        public string ReceiptSignature { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_url")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_url")]
        public string ETimsURL { get; set; }
        [Newtonsoft.Json.JsonProperty("offline_url")]
        [System.Text.Json.Serialization.JsonPropertyName("offline_url")]
        public string OfflineURL { get; set; }
        [Newtonsoft.Json.JsonProperty("status")]
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }
        [Newtonsoft.Json.JsonProperty("sales_tax_summary")]
        [System.Text.Json.Serialization.JsonPropertyName("sales_tax_summary")]
        public SalesTaxSummary SalesTaxSummary { get; set; }
        [Newtonsoft.Json.JsonProperty("item_list")]
        [System.Text.Json.Serialization.JsonPropertyName("item_list")]
        public List<DSaleListItem> ItemList { get; set; }

        public string GetCUNumber(ClientBranch clientBranch)
        {
            if (string.IsNullOrWhiteSpace(ReceiptSignature))
                return null;
            return $"{clientBranch.TaxClient.TaxNumber}{clientBranch.BranchCode}{ReceiptSignature}";
        }
        public string GetQRText()
        {
            if (string.IsNullOrWhiteSpace(ETimsURL))
                return OfflineURL;
            else
                return ETimsURL;
        }

    }
    public class DSaleListItem
    {

        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string SaleItemID { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("total_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount")]
        public decimal TaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate")]
        public double TaxRate { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_type_code")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_rate", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_item_code")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_item_code")]
        public string EtimsItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("is_stockable")]
        [System.Text.Json.Serialization.JsonPropertyName("is_stockable")]
        public bool IsStockable { get; set; }
        [Newtonsoft.Json.JsonProperty("item_id")]
        [System.Text.Json.Serialization.JsonPropertyName("item_id")]
        public string ItemID { get; set; }
    }

    public class DTaxSaveCNoteReq
    {
        [Newtonsoft.Json.JsonProperty("return_date")]
        [System.Text.Json.Serialization.JsonPropertyName("return_date")]
        public string ReturnDate { get; set; }
        [Newtonsoft.Json.JsonProperty("sale_id")]
        [System.Text.Json.Serialization.JsonPropertyName("sale_id")]
        public string SaleID { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("callback_url", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("callback_url")]
        public string CallbackURL { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_details", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_details")]
        public string InvoiceDetails { get; set; }
        [Newtonsoft.Json.JsonProperty("items")]
        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<DTaxCNoteItem> ItemList { get; set; } = new();

        public DTaxSaveCNoteReq()
        {
        }
        public DTaxSaveCNoteReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote oeCRNote,
            SalesTransact salesTransact, SalesTransact origTransact, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer) : this()
        {
            TraderInvoiceNo = oeCRNote.CreditDebitNoteNumber;
            InvoiceDetails = oeCRNote.Description;
            SaleID = origTransact?.ExternalID;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/salesync";

            if (oeCRNote.CreditDebitNoteDate.HasValue)
                ReturnDate = oeCRNote.CreditDebitNoteDate.Value.ToString(DTaxConst.FMT_DATEONLY);

            foreach (var _salesItem in salesTransact.SalesItems)
            {
                var salesItem = new DTaxCNoteItem(_salesItem);
                ItemList.Add(salesItem);
            }
        }

        public DTaxSaveCNoteReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arCRNote, 
            SalesTransact salesTransact, SalesTransact origTransact, S300TaxGroup taxGroup, HashSet<string> taxAuthKeys, 
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer) : this()
        {
            TraderInvoiceNo = arCRNote.DocumentNumber;
            InvoiceDetails = arCRNote.InvoiceDescription;
            SaleID = origTransact?.ExternalID;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/salesync";

            if (arCRNote.DocumentDate.HasValue)
                ReturnDate = arCRNote.DocumentDate.Value.ToString(DTaxConst.FMT_DATEONLY);

            foreach (var _salesItem in salesTransact.SalesItems)
            {
                var salesItem = new DTaxCNoteItem(_salesItem);
                ItemList.Add(salesItem);
            }
        }

        public string GetError()
        {
            return null;
        }

    }
    public class DTaxSaveCNoteResp : DTaxBaseResp
    {

        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("date")]
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; }
        [Newtonsoft.Json.JsonProperty("time")]
        [System.Text.Json.Serialization.JsonPropertyName("time")]
        public string Time { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_type_code")]
        public string ReceiptTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("sale_detail_url")]
        [System.Text.Json.Serialization.JsonPropertyName("sale_detail_url")]
        public string SaleDetailURL { get; set; }
        [Newtonsoft.Json.JsonProperty("serial_number")]
        [System.Text.Json.Serialization.JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_number")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_number")]
        public int ReceiptNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_number")]
        public int InvoiceNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_id")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_id")]
        public string CustomerID { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_name")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_tin")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_tin")]
        public string CustomerPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("internal_data")]
        [System.Text.Json.Serialization.JsonPropertyName("internal_data")]
        public string InternalData { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_signature")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_signature")]
        public string ReceiptSignature { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_url")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_url")]
        public string ETimsURL { get; set; }
        [Newtonsoft.Json.JsonProperty("offline_url")]
        [System.Text.Json.Serialization.JsonPropertyName("offline_url")]
        public string OfflineURL { get; set; }
        [Newtonsoft.Json.JsonProperty("status")]
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_phone_number")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_phone_number")]
        public string CustomerPhoneNo { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_email")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_email")]
        public string CustomerEmail { get; set; }

        [Newtonsoft.Json.JsonProperty("original_sale_id")]
        [System.Text.Json.Serialization.JsonPropertyName("original_sale_id")]
        public string OriginalSaleID { get; set; }
        [Newtonsoft.Json.JsonProperty("sales_tax_summary")]
        [System.Text.Json.Serialization.JsonPropertyName("sales_tax_summary")]
        public SalesTaxSummary SalesTaxSummary { get; set; }
        [Newtonsoft.Json.JsonProperty("item_list")]
        [System.Text.Json.Serialization.JsonPropertyName("item_list")]
        public List<DSaleListItem> ItemList { get; set; }

        public string GetCUNumber(ClientBranch clientBranch)
        {
            if (string.IsNullOrWhiteSpace(ReceiptSignature))
                return null;
            return $"{clientBranch.TaxClient.TaxNumber}{clientBranch.BranchCode}{ReceiptSignature}";
        }
        public string GetQRText()
        {
            if (string.IsNullOrWhiteSpace(ETimsURL))
                return null;
            return ETimsURL;
        }
    }

    public class DTaxCNoteItem
    {
        [Newtonsoft.Json.JsonProperty("item_bar_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_bar_code")]
        public string BarCode { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_quantity", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_quantity")]
        public decimal Package { get; set; }
        [Newtonsoft.Json.JsonProperty("unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("total_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_rate", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("item_description", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("item_description")]
        public string ItemDescription { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount")]
        public decimal TaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }

        public DTaxCNoteItem()
        {
        }

        public DTaxCNoteItem(SalesItem salesItem)
            : this()
        {
            BarCode = salesItem.ProductCode;
            Quantity = salesItem.Quantity;
            Package = salesItem.Package;
            UnitPrice = salesItem.TotalAmount / salesItem.Quantity;
            DiscountRate = salesItem.DiscountRate;
            DiscountAmount = salesItem.DiscountAmount;
            TaxableAmount = salesItem.TaxableAmount;
            TaxAmount = salesItem.TaxAmount;
            TotalAmount = salesItem.TotalAmount;
            ItemDescription = salesItem.Description;
        }
    }

}

using iTaxSuite.Library.Models.Entities;

namespace iTaxSuite.Library.Models.ViewModels
{
    public abstract class WebHookVModel
    {
        [Newtonsoft.Json.JsonProperty("event")]
        [System.Text.Json.Serialization.JsonPropertyName("event")]
        public string Event { get; set; }
    }

    public class ItemCallback : WebHookVModel
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ItemCBData CBData { get; set; }
    }
    public class ItemCBData
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("item_class_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_class_code")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_type_code")]
        public string ItemTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("item_name")]
        [System.Text.Json.Serialization.JsonPropertyName("item_name")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("origin_nation_code")]
        [System.Text.Json.Serialization.JsonPropertyName("origin_nation_code")]
        public string OriginNatCode { get; set; }
        [Newtonsoft.Json.JsonProperty("package_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("package_unit_code")]
        public string PkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity_unit_code")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity_unit_code")]
        public string QtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_type_code")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("default_unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("default_unit_price")]
        public decimal DefaultUnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_item_code")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_item_code")]
        public string EtimsItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("is_stock_item")]
        [System.Text.Json.Serialization.JsonPropertyName("is_stock_item")]
        public bool IsStockItem { get; set; }
        [Newtonsoft.Json.JsonProperty("running_balance")]
        [System.Text.Json.Serialization.JsonPropertyName("running_balance")]
        public decimal RunningBalance { get; set; }
    }

    public class SaleCallback : WebHookVModel
    {
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public SaleCBData CBData { get; set; }

        public string GetCUNumber(ClientBranch clientBranch)
        {
            if (CBData is null || string.IsNullOrWhiteSpace(CBData.ReceiptSignature))
                return null;
            return $"{clientBranch.TaxClient.TaxNumber}{clientBranch.BranchCode}{CBData.ReceiptSignature}";
        }
        public string GetQRText()
        {
            if (CBData is null || string.IsNullOrWhiteSpace(CBData.ETimsURL))
                return null;
            return CBData.ETimsURL;
        }
    }
    public class SaleCBData
    {
        [Newtonsoft.Json.JsonProperty("date")]
        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; }
        [Newtonsoft.Json.JsonProperty("time")]
        [System.Text.Json.Serialization.JsonPropertyName("time")]
        public string Time { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("original_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("original_invoice_number")]
        public string OriginalInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("serial_number")]
        [System.Text.Json.Serialization.JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_number")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_number")]
        public int ReceiptNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("internal_data")]
        [System.Text.Json.Serialization.JsonPropertyName("internal_data")]
        public string InternalData { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_signature")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_signature")]
        public string ReceiptSignature { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_url")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_url")]
        public string ETimsURL { get; set; }
        [Newtonsoft.Json.JsonProperty("sale_detail_url")]
        [System.Text.Json.Serialization.JsonPropertyName("sale_detail_url")]
        public string SaleDetailURL { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_pin")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_pin")]
        public string CustomerPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("customer_name")]
        [System.Text.Json.Serialization.JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("queue_status")]
        [System.Text.Json.Serialization.JsonPropertyName("queue_status")]
        public string QueueStatus { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_number")]
        public string InvoiceNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("sales_tax_summary")]
        [System.Text.Json.Serialization.JsonPropertyName("sales_tax_summary")]
        public SalesTaxSummary SalesTaxSummary { get; set; }
    }
    public class SalesTaxSummary
    {
        [Newtonsoft.Json.JsonProperty("taxable_amount_a")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount_a")]
        public double TaxableAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount_b")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount_b")]
        public double TaxableAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount_c")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount_c")]
        public double TaxableAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount_d")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount_d")]
        public double TaxableAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxable_amount_e")]
        [System.Text.Json.Serialization.JsonPropertyName("taxable_amount_e")]
        public double TaxableAmountE { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate_a")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate_a")]
        public double TaxRateA { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate_b")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate_b")]
        public double TaxRateB { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate_c")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate_c")]
        public double TaxRateC { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate_d")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate_d")]
        public double TaxRateD { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_rate_e")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_rate_e")]
        public double TaxRateE { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount_a")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount_a")]
        public double TaxAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount_b")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount_b")]
        public double TaxAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount_c")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount_c")]
        public double TaxAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount_d")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount_d")]
        public double TaxAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_amount_e")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_amount_e")]
        public double TaxAmountE { get; set; }
    }

}

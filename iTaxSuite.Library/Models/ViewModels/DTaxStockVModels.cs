using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.ViewModels
{
    public class DTaxCreateItemReq : DTaxBaseItem
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int _offset { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _pkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _qtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_type_code")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("default_unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("default_unit_price")]
        public double DefaultUnitPrice { get; set; } = 1;
        [Newtonsoft.Json.JsonProperty("stock_quantity", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("stock_quantity")]
        public double StockQuantity { get; set; }
        [Newtonsoft.Json.JsonProperty("callback_url", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("callback_url")]
        public string CallbackURL { get; set; }
        [Newtonsoft.Json.JsonProperty("item_bar_code", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("item_bar_code")]
        public string BarCode { get; set; }
        [Newtonsoft.Json.JsonProperty("levies", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("levies")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public List<string> Levies { get; set; }
        public DTaxCreateItemReq()
        {
            OriginNatCode = "KE";
        }

        public DTaxCreateItemReq(ClientBranch clientBranch, StockItem stockItem, Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Item item)
            : this()
        {
            _offset = stockItem.EtrSeqNumber;
            BarCode = stockItem.ProductCode;
            if (item.StockItem)
            {
                ItemTypeCode = "2";
                StockQuantity = (double)item.QuantityAvailable;
            }
            else
            {
                ItemTypeCode = "3";
            }
            ItemName = item.Description;
            PkgUnitCode = stockItem.Product.PackageUnit;
            QtyUnitCode = stockItem.Product.QuantityUnit;
            TaxTypeCode = "B";
            ItemClassCode = stockItem.Product.ItemClassCode;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/itemsync";

            string newItemClassCode = string.Empty;
            if (item.ItemOptionalFields != null && item.ItemOptionalFields.Count > 0)
            {
                var optTaxClass = item.ItemOptionalFields.FirstOrDefault(f => f.OptionalField
                    .Equals(GeneralConst.OPTFLD_TAXCLASSCODE, StringComparison.InvariantCultureIgnoreCase));
                if (optTaxClass != null && !string.IsNullOrWhiteSpace(optTaxClass.Value))
                {
                    newItemClassCode = optTaxClass.Value;
                    if (stockItem.Product.ItemClassCode != newItemClassCode)
                    {
                        newItemClassCode = stockItem.Product.ItemClassCode;
                    }
                }
            }
            if (newItemClassCode != ItemClassCode)
                ItemClassCode = newItemClassCode;
        }

        public DTaxCreateItemReq(ClientBranch clientBranch, StockItem stockItem, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Item item)
            : this()
        {
            _offset = stockItem.EtrSeqNumber;
            BarCode = stockItem.ProductCode;
            ItemTypeCode = "3";
            ItemName = item.Description;
            PkgUnitCode = stockItem.Product.PackageUnit;
            QtyUnitCode = stockItem.Product.QuantityUnit;
            TaxTypeCode = "B";
            ItemClassCode = stockItem.Product.ItemClassCode;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/itemsync";

            string newItemClassCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(item.CommodityCode))
            {
                newItemClassCode = item.CommodityCode;
            }
            if (newItemClassCode != ItemClassCode)
                ItemClassCode = newItemClassCode;
        }

        public DTaxCreateItemReq(ClientBranch clientBranch, StockItem stockItem, Sage.CA.SBS.ERP.Sage300.GL.WebApi.Models.Account account)
            : this()
        {
            _offset = stockItem.EtrSeqNumber;
            BarCode = stockItem.ProductCode;
            ItemTypeCode = "3";
            ItemName = account.Description;
            PkgUnitCode = stockItem.Product.PackageUnit;
            QtyUnitCode = stockItem.Product.QuantityUnit;
            TaxTypeCode = "B";
            ItemClassCode = stockItem.Product.ItemClassCode;
            if (!string.IsNullOrWhiteSpace(clientBranch.TaxClient.BaseCallback))
                CallbackURL = $"{clientBranch.TaxClient.BaseCallback}/{clientBranch.TaxClient.ExternalID}/itemsync";

            string newItemClassCode = string.Empty;
            if (account.AccountOptionalFields != null && account.AccountOptionalFields.Count > 0)
            {
                var optTaxClass = account.AccountOptionalFields.FirstOrDefault(f => f.OptionalField
                    .Equals(GeneralConst.OPTFLD_TAXCLASSCODE, StringComparison.InvariantCultureIgnoreCase));
                if (optTaxClass != null && !string.IsNullOrWhiteSpace(optTaxClass.Value))
                {
                    newItemClassCode = optTaxClass.Value;
                }
            }
            if (newItemClassCode != ItemClassCode)
                ItemClassCode = newItemClassCode;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ItemClassCode) && !string.IsNullOrWhiteSpace(PkgUnitCode) 
                && !string.IsNullOrWhiteSpace(QtyUnitCode) && !string.IsNullOrWhiteSpace(BarCode);
        }

    }

    public class DTaxCreateItemResp : DTaxBaseItemResp
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_type_code")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("default_unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("default_unit_price")]
        public double DefaultUnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_item_code")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_item_code")]
        public string EtimsItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("is_stock_item")]
        [System.Text.Json.Serialization.JsonPropertyName("is_stock_item")]
        public bool IsStockItem { get; set; }
        [Newtonsoft.Json.JsonProperty("stock_quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("stock_quantity")]
        public double StockQuantity { get; set; }
        [Newtonsoft.Json.JsonProperty("active")]
        [System.Text.Json.Serialization.JsonPropertyName("active")]
        public bool Active { get; set; }
        [Newtonsoft.Json.JsonProperty("status")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public DTaxSyncStatus Status { get; set; } = DTaxSyncStatus.PENDING;
        [Newtonsoft.Json.JsonProperty("item_bar_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_bar_code")]
        public string BarCode { get; set; }
        [Newtonsoft.Json.JsonProperty("levies")]
        [System.Text.Json.Serialization.JsonPropertyName("levies")]
        public List<string> Levies { get; set; }
    }

    public class DTaxSelectItemResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("pagination")]
        [System.Text.Json.Serialization.JsonPropertyName("pagination")]
        public DTaxPagination Pagination { get; set; }
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<DTaxItem> Items { get; set; }
    }
    public class DTaxItem : DTaxBaseItem
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("tax_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("tax_type_code")]
        public string TaxTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("default_unit_price")]
        [System.Text.Json.Serialization.JsonPropertyName("default_unit_price")]
        public double DefaultUnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("etims_item_code")]
        [System.Text.Json.Serialization.JsonPropertyName("etims_item_code")]
        public string EtimsItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("is_stock_item")]
        [System.Text.Json.Serialization.JsonPropertyName("is_stock_item")]
        public bool IsStockItem { get; set; }
        [Newtonsoft.Json.JsonProperty("stock_quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("stock_quantity")]
        public double StockQuantity { get; set; }
        [Newtonsoft.Json.JsonProperty("active")]
        [System.Text.Json.Serialization.JsonPropertyName("active")]
        public bool Active { get; set; }
        [Newtonsoft.Json.JsonProperty("status")]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public DTaxSyncStatus Status { get; set; } = DTaxSyncStatus.PENDING;
        [Newtonsoft.Json.JsonProperty("item_bar_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_bar_code")]
        public string BarCode { get; set; }
    }

    [NotMapped]
    public class DTaxStockAdjustReq
    {
        [Newtonsoft.Json.JsonProperty("item_id")]
        [System.Text.Json.Serialization.JsonPropertyName("item_id")]
        public string ItemID { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("action")]
        [System.Text.Json.Serialization.JsonPropertyName("action")]
        public string Action { get; set; }
        [Newtonsoft.Json.JsonProperty("movement_type")]
        [System.Text.Json.Serialization.JsonPropertyName("movement_type")]
        public string MovementType { get; set; }
        public DTaxStockAdjustReq()
        {
        }

        public DTaxStockAdjustReq(ClientBranch clientBranch, StockIORequest stockIORequest, StockItem stockItem)
            : this()
        {
            ItemID = stockItem.ExternalID;
            Quantity = stockIORequest.MoveQuantity;
            MovementType = stockIORequest.MovementType.GetEnumMemberValue();
            switch(stockIORequest.MovementType)
            {
                case StockMovementType.Import:
                case StockMovementType.Purchase:
                case StockMovementType.ReturnInwards:
                case StockMovementType.StockMovement:
                case StockMovementType.IncomingProcessing:
                case StockMovementType.IncomingAdjustment:
                    Action = DTaxStockAction.ADD.GetEnumMemberValue();
                    break;
                case StockMovementType.Sale:
                case StockMovementType.ReturnOutwards:
                case StockMovementType.OutgoingStock:
                case StockMovementType.Processing:
                case StockMovementType.Discarding:
                case StockMovementType.OutgoingAdjustment:
                    Action = DTaxStockAction.DEDUCT.GetEnumMemberValue();
                    break;
            }
            
        }

        public bool IsValid()
        {
            return true;
        }
    }
    public class DTaxStockAdjustResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("item_code")]
        [System.Text.Json.Serialization.JsonPropertyName("item_code")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("stock_quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("stock_quantity")]
        public double StockQuantity { get; set; }
    }

    // Purchases
    public class DTaxPurchaseResp : DTaxBaseResp
    {
        [Newtonsoft.Json.JsonProperty("pagination")]
        [System.Text.Json.Serialization.JsonPropertyName("pagination")]
        public DTaxPagination Pagination { get; set; }
        [Newtonsoft.Json.JsonProperty("meta")]
        [System.Text.Json.Serialization.JsonPropertyName("meta")]
        public DTaxMeta Meta { get; set; }
        [Newtonsoft.Json.JsonProperty("data")]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<DTaxPurchase> Purchases { get; set; }
    }
    public class DTaxMeta
    {
        [Newtonsoft.Json.JsonProperty("last_updated")]
        [System.Text.Json.Serialization.JsonPropertyName("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
    public class DTaxPurchase
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("registration_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("registration_type_code")]
        public string RegTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("invoice_number")]
        public int InvoiceNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("purchase_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("purchase_type_code")]
        public string PurchaseTypeCode { get; set; }
        [Newtonsoft.Json.JsonProperty("receipt_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("receipt_type_code")]
        public string ReceiptTypeCode { get; set; }         // 4.9. Sale Receipt Type
        [Newtonsoft.Json.JsonProperty("payment_type_code")]
        [System.Text.Json.Serialization.JsonPropertyName("payment_type_code")]
        public string PaymentTypeCode { get; set; }         // 4.10. Payment Method
        [Newtonsoft.Json.JsonProperty("purchase_status_code")]
        [System.Text.Json.Serialization.JsonPropertyName("purchase_status_code")]
        public string PurchaseStatusCode { get; set; }
        [Newtonsoft.Json.JsonProperty("purchase_date")]
        [System.Text.Json.Serialization.JsonPropertyName("purchase_date")]
        public string PurchaseDate { get; set; }
        [Newtonsoft.Json.JsonProperty("trader_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("trader_invoice_number")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonProperty("supplier_branch_id")]
        [System.Text.Json.Serialization.JsonPropertyName("supplier_branch_id")]
        public string SupplierBranchID { get; set; }
        [Newtonsoft.Json.JsonProperty("supplier_name")]
        [System.Text.Json.Serialization.JsonPropertyName("supplier_name")]
        public string SupplierName { get; set; }
        [Newtonsoft.Json.JsonProperty("supplier_invoice_number")]
        [System.Text.Json.Serialization.JsonPropertyName("supplier_invoice_number")]
        public int SupplierInvoiceNo { get; set; }
        public List<ItemList> item_list { get; set; }
    }
    public class ItemList
    {
        [Newtonsoft.Json.JsonProperty("id")]
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("quantity")]
        [System.Text.Json.Serialization.JsonPropertyName("quantity")]
        public double Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("price")]
        [System.Text.Json.Serialization.JsonPropertyName("price")]
        public decimal Price { get; set; }
        [Newtonsoft.Json.JsonProperty("supply_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("supply_amount")]
        public decimal SupplyAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_rate")]
        [System.Text.Json.Serialization.JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("discount_amount")]
        [System.Text.Json.Serialization.JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("item_id")]
        [System.Text.Json.Serialization.JsonPropertyName("item_id")]
        public string ItemID { get; set; }
    }

}

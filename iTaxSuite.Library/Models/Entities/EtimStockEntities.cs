using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("StockItem")]
    public class StockItem : BaseTransact
    {
        [Required]
        [StringLength(64)]
        public string ProductCode { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        public DateTime? LastStockSave { get; set; }
        public DateTime? LastMovement { get; set; }
        [StringLength(64)]
        public string TaxItemCode { get; set; }
        public int EtrSeqNumber { get; set; } = -1;
        public int StockCount { get; set; }
        public DateTime? CountTime { get; set; }
        //private string _cacheKey;
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        //public override string CacheKey => _cacheKey;
        public override string CacheKey => $"{BranchCode}:{ProductCode}";
        public virtual Product Product { get; set; }
        public virtual ClientBranch ClientBranch { get; set; }

        public StockItem()
        {
        }

        public StockItem(Product product, ClientBranch clientBranch)
            : this()
        {
            ProductCode = product.ProductCode;
            BranchCode = clientBranch.BranchCode;
            Product = product;
            ClientBranch = clientBranch;
            EtrSeqNumber = clientBranch.ProductSeq;
        }
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Product.ItemClassCode) && !string.IsNullOrWhiteSpace(TaxItemCode);
        }

        public EtimsTransact GetTransaction(ClientBranch clientBranch)
        {
            if (RecordStatus == RecordStatus.POST_OK || RecordStatus == RecordStatus.POST_FAIL
                || RecordStatus == RecordStatus.POST_DUPL || !IsValid())
                return null;

            var etimsTransact = new EtimsTransact()
            {
                BranchCode = BranchCode,
                ReqType = ETIMSReqType.CREATE_ITEMS,
                DocNumber = CacheKey,
                DocStamp = Product?.ProductData?.SourceStamp ?? DateTime.Now,
                SourceApp = "IC",
                ReqAddress = $"{clientBranch.EtrAddress}/items/saveItems",
                ReqKey = CacheKey,
                ParentKey = null,
                IsNewRequest = true,
                NextSeqNumber = -1
            };
            return etimsTransact;
        }

    }

    [Table("StockMovement")]
    public class StockMovement : BaseTransact
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MovementID { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(64)]
        public string DocNumber { get; set; }
        [Required]
        [NotMinValue]
        public DateTime DocDate { get; set; }
        [Required]
        public StockMovementType MovementType { get; set; }
        [Required]
        public SourceType SourceType { get; set; }
        [StringLength(128)]
        public string Description { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public override string CacheKey => $"{BranchCode}:{DocNumber}";
        public virtual StockMovData? StockMovData { get; set; }
        public virtual ClientBranch ClientBranch { get; set; }
        public StockMovement()
        {
        }
        public StockMovement(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice oeInvoice)
            : this()
        {
            MovementType = StockMovementType.Sale;
            SourceType = SourceType.Sage300CERP;
            BranchCode = clientBranch.BranchCode;
            DocNumber = $"{BranchCode}:{oeInvoice.InvoiceNumber}";
            DocDate = oeInvoice.InvoiceDate.Value;
            Description = oeInvoice.Description;
            CreatedBy = "Sys-Admin";
        }

        public StockMovement(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.PO.WebApi.Models.Invoice pInvoice, 
            StockIOSaveReq stockIOSaveReq)
            : this()
        {
            MovementType = StockMovementType.Purchase;
            SourceType = SourceType.Sage300CERP;
            BranchCode = clientBranch.BranchCode;
            DocNumber = $"{BranchCode}:{pInvoice.InvoiceNumber}";
            DocDate = pInvoice.InvoiceDate.Value;
            CreatedBy = "Sys-Admin";
        }

        public StockMovement(ClientBranch clientBranch, StockIORequest stockIORequest)
        {
            MovementType = stockIORequest.MovementType;
            SourceType = SourceType.MANUALENTRY;
            BranchCode = clientBranch.BranchCode;
            DocNumber = $"{BranchCode}:{stockIORequest.DocNumber}";
            DocDate = stockIORequest.DocDate;
            Description = stockIORequest.Description;
            CreatedBy = "Sys-Admin";
        }
    }
    public class MovementFilter : APDatedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        [StringLength(2)]
        public string BranchCode { get; set; }
        [StringLength(64)]
        public string DocNumber { get; set; }
        //public StockMovementType MovementType { get; set; }
    }
    [Table("StockMovData")]
    public class StockMovData : BaseDataEntity
    {
        [Key]
        [Required]
        public int MovementID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public StockMovement StockMovement { get; set; }
        public StockIOSaveReq StockIOSaveReq { get;set; }
        public StockMovData()
        {
        }
        public StockMovData(StockMovement stockMovement, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice oeInvoice, 
            StockIOSaveReq stockIOSaveReq)
            : this()
        {
            SourceStamp = oeInvoice.InvoiceDate.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(oeInvoice);
            StockIOSaveReq = stockIOSaveReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(stockIOSaveReq, new DecimalFormatConverter());
        }

        public StockMovData(StockMovement stockMovement, StockIORequest stockIORequest, StockIOSaveReq stockIOSaveReq)
        {
            SourceStamp = stockIORequest.DocDate;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(stockIORequest);
            StockIOSaveReq = stockIOSaveReq;
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(stockIOSaveReq, new DecimalFormatConverter());
        }

        public StockIOSaveReq GetEtimsRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StockIOSaveReq>(RequestPayload);
        }

    }

    [Table("Product")]
    public class Product : BaseTransact
    {
        [Key]
        [Required]
        [StringLength(64)]
        public string ProductCode { get; set; }
        [Required]
        public bool IsStockable { get; set; }
        [Required]
        [StringLength(128)]
        public string Description { get; set; }
        [StringLength(10)]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string _pkgUnitCode { get; set; }
        [Required]
        [StringLength(8)]
        public string PackageUnit { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string _qtyUnitCode { get; set; }
        [Required]
        [StringLength(8)]
        public string QuantityUnit { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public override string CacheKey => ProductCode;
        public virtual ProductData? ProductData { get; set; }
        public Product()
        {
            RecordStatus = RecordStatus.NONE;
        }
        public Product(Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Item item)
            : this()
        {
            ProductCode = item.ItemNumber;
            Description = item.Description;
            _pkgUnitCode = _qtyUnitCode = item.StockingUnitOfMeasure;
            IsStockable = item.StockItem;

            // Sort Item Class
            if (item.ItemOptionalFields != null && item.ItemOptionalFields.Count > 0)
            {
                var optTaxClass = item.ItemOptionalFields.FirstOrDefault(f => f.OptionalField
                    .Equals(GeneralConst.OPTFLD_TAXCLASSCODE, StringComparison.InvariantCultureIgnoreCase));
                if (optTaxClass != null && !string.IsNullOrWhiteSpace(optTaxClass.Value))
                {
                    ItemClassCode = optTaxClass.Value;
                }
            }
        }

        public void UpdateAttributes((string PkgUnitCode, string QtyUnitCode) unitCodes)
        {
            PackageUnit = unitCodes.PkgUnitCode;
            QuantityUnit = unitCodes.QtyUnitCode;
        }

        public bool IsValid()
        {
            return PackageUnit != ETIMSConst.NOUNIT_CODE && QuantityUnit != ETIMSConst.NOUNIT_CODE 
                && !string.IsNullOrWhiteSpace(ItemClassCode);
        }
    }

    [Table("ProductData")]
    public class ProductData : BaseDataEntity
    {
        [Key]
        [Required]
        [StringLength(64)]
        public string ProductCode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Product Product { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SaveItemReq SaveItemReq { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public SaveItemResp SaveItemResp { get; set; }
        public ProductData()
        {
        }

        public ProductData(ClientBranch clientBranch, StockItem stockItem, Sage.CA.SBS.ERP.Sage300.IC.WebApi.Models.Item item)
            : this()
        {
            // Sort Request details
            SourceStamp = item.DateLastMaintained.Value;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            SaveItemReq = new SaveItemReq(clientBranch, stockItem, item);
            RequestPayload = Newtonsoft.Json.JsonConvert.SerializeObject(SaveItemReq, new DecimalFormatConverter());
        }
        public SaveItemReq GetEtimsRequest()
        {
            if (string.IsNullOrWhiteSpace(RequestPayload))
                return null;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SaveItemReq>(RequestPayload);
        }
    }
       

}

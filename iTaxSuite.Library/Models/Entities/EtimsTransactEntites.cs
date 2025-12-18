using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("EtimsTransact")]
    public class EtimsTransact : BaseTransact
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EtimsTrxID { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string BranchCode { get; set; }
        [Required]
        public ETIMSReqType ReqType { get; set; }
        [Required]
        [StringLength(64)]
        public string DocNumber { get; set; }
        [Required]
        [NotMinValue]
        public DateTime DocStamp { get; set; }
        [Required]
        [StringLength(3)]
        public string SourceApp { get; set; }    // OE,AP,AR,IC
        [Required]
        [StringLength(256)]
        public string ReqAddress { get; set; }
        [Required]
        [StringLength(64)]
        public string ReqKey { get; set; }
        [StringLength(64)]
        public string ParentKey { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ReqPayload { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RespPayload { get; set; }
        [Required]
        public bool IsNewRequest { get; set; }
        public int NextSeqNumber { get; set; } = -1;
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{ReqType}:{DocNumber}:{DocStamp:s}";
        public virtual ClientBranch ClientBranch { get; set; }
    }
    public class TransactFilter : APDatedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        public ETIMSReqType ReqType { get; set; } = ETIMSReqType.NONE;
        [StringLength(2)]
        public string BranchCode { get; set; }
        [StringLength(64)]
        public string DocNumber { get; set; }
        [StringLength(64)]
        public string ParentKey { get; set; }
    }
    public class TransactKey
    {
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        public ETIMSReqType ReqType { get; set; } = ETIMSReqType.NONE;
        [StringLength(2)]
        public string BranchCode { get; set; }
        [StringLength(64)]
        public string DocNumber { get; set; }
        [StringLength(64)]
        public string ParentKey { get; set; }
    }

    public class SalesTrxView
    {
        public EtimsTransact SalesTransact { get; set; }
        public EtimsTransact StockTransact { get; set; }
        public SalesTrxView()
        {
        }
    }
}

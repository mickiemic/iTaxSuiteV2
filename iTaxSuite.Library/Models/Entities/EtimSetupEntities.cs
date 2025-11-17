using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.Entities
{
    [Table("ClientBranch")]
    public class ClientBranch : BaseTransact
    {
        [Required]
        [StringLength(16)]
        public string ClientCode { get; set; }
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(60)]
        public string BranchName { get; set; }
        [StringLength(13)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string Email { get; set; }
        [Required]
        [StringLength(128)]
        public string EtrAddress { get; set; }
        [StringLength(128)]
        public string SecAddress { get; set; }
        [Required]
        [StringLength(100)]
        public string EtrSerialNo { get; set; }
        public int ProductSeq { get; set; }
        public int SaleInvoiceSeq { get; set; }
        public int PurchInvoiceSeq { get; set; }
        public DateTime? InitializedAt { get; set; }
        [StringLength(512)]
        public string InitRequest { get; set; }
        [StringLength(2048)]
        public string InitResponse { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => BranchCode;
        public virtual TaxClient TaxClient { get; set; }
    }

    [Table("BranchCustomer")]
    public class BranchCustomer : BaseTransactData
    {
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(32)]
        public string CustNumber { get; set; }
        [Required]
        [StringLength(60)]
        public string CustName { get; set; }
        [Required]
        [StringLength(16)]
        public string CustTaxNumber { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
        [StringLength(128)]
        public string Description { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{CustNumber}";
        public virtual ClientBranch ClientBranch { get; set; }
    }

    [Table("BranchUser")]
    public class BranchUser : BaseTransactData
    {
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(32)]
        public string UserNumber { get; set; }
        [Required]
        [StringLength(60)]
        public string UserName { get; set; }
        [Required]
        [StringLength(256)]
        public string UserPass { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
        [StringLength(128)]
        public string Description { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => $"{BranchCode}:{UserNumber}";
        public virtual ClientBranch ClientBranch { get; set; }
    }

    [Table("BranchVendor")]
    public class BranchVendor : BaseEntity
    {
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(32)]
        public string VendorNumber { get; set; }
        [StringLength(60)]
        public string VendorName { get; set; }
        [StringLength(11)]
        public string VendorTaxNumber { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
        [StringLength(128)]
        public string Description { get; set; }
        public DateTime? SourceStamp { get; set; }
        [StringLength(4000)]
        public string SourcePayload { get; set; }

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CacheKey => $"{BranchCode}:{VendorNumber}";
        public virtual ClientBranch ClientBranch { get; set; }
    }

    [Table("Customer")]
    public class Customer : BaseTransactData
    {
        [Key]
        [Required]
        [StringLength(32)]
        public string CustomerNumber { get; set; }
        [Required]
        [StringLength(60)]
        public string CustomerName { get; set; }
        [StringLength(16)]
        public string PhoneNumber { get; set; }
        [StringLength(11)]
        public string TaxNumber { get; set; }
        [StringLength(2)]
        public string BranchNumber { get; set; }
        [StringLength(20)]
        public string TopMessage { get; set; }
        [StringLength(20)]
        public string BtmMessage { get; set; }
        [StringLength(2000)]
        public string ProcessMeta { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => CustomerNumber;
        public Customer()
        {
            RecordStatus = RecordStatus.QUEUEDOUT;
        }

        public Customer(Customer customer)
            : this()
        {
            CustomerNumber = customer.CustomerNumber;
            CustomerName = customer.CustomerName;
            PhoneNumber = customer.PhoneNumber;
            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
        }

        /*public CustomerKey GetCacheKey()
        {
            return new CustomerKey(this);
        }*/
    }

}

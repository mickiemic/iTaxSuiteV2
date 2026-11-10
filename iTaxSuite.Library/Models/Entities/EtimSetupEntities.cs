using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

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
        [StringLength(512)]
        public string ProcessMeta { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public override string CacheKey => BranchCode;
        public string BaseETRAddress
        {
            get
            {
                if (TaxClient is null)
                    return null;

                if (TaxClient.IsProduction)
                    return $"https://etims.kra.go.ke/common/link/etims/receipt/indexEtimsReceiptData?Data=";
                else
                    return $"https://etims-sbx.kra.go.ke/common/link/etims/receipt/indexEtimsReceiptData?Data=";
            }
        }
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
        [StringLength(128)]
        public string ExternalID { get; set; }
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
        [StringLength(128)]
        public string ExternalID { get; set; }
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
        [StringLength(128)]
        public string ExternalID { get; set; }
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
        [StringLength(128)]
        public string ExternalID { get; set; }
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

    [Table("ItemClassCode")]
    public class ItemClassCode : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(10)]
        public string ClassCode { get; set; }
        [Required]
        [StringLength(200)]
        public string ClassName { get; set; }
        [StringLength(5)]
        public string TaxTypeCode { get; set; }
        public int ClassLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsMajorTarget { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CacheKey => ClassCode;
    }

    [Table("TISystemCode")]
    public class TISystemCode : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(2)]
        public string CodeClass { get; set; }
        [StringLength(60)]
        public string ClassName { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [StringLength(1)]
        public bool IsActive { get; set; }
        [StringLength(60)]
        public string UserDefinedName1 { get; set; }
        [StringLength(60)]
        public string UserDefinedName2 { get; set; }
        [StringLength(60)]
        public string UserDefinedName3 { get; set; }
        public List<SystemCodeItem> SystemCodeItems { get; set; } = new();
    }
    [Table("SystemCodeItem")]
    public class SystemCodeItem : BaseEntity
    {
        [Required]
        [StringLength(2)]
        public string CodeClass { get; set; }
        [StringLength(5)]
        public string Code { get; set; }
        [StringLength(60)]
        public string CodeName { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        [StringLength(20)]
        public string UserDefinedCode1 { get; set; }
        [StringLength(20)]
        public string UserDefinedCode2 { get; set; }
        [StringLength(20)]
        public string UserDefinedCode3 { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TISystemCode TISystemCode { get; set; }
    }

    public enum TargetType { All = 0, Role = 1, SpecificUser = 2 }
    
    [Table("Notification")]
    public class Notification: BaseEntity
    {
        [Required]
        [StringLength(32)]
        public string NotificationID { get; set; }
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [StringLength(128)]
        public string ExternalID { get; set; }
        public TargetType TargetType { get; set; }
        public string TargetRoleId { get; set; }    // Identity role name(when TargetType = 1)
        public string TargetUserId { get; set; }    // Identity user Id  (when TargetType=2)
        [StringLength(256)]
        public string Title { get; set; }
        [StringLength(2048)]
        public string Message { get; set; }
        [StringLength(512)]
        public string LinkUrl { get; set; }
        public LogLevel Level { get; set; } = LogLevel.Information;
        [Required]
        public DateTime NotificationDate { get; set; } = DateTime.Now;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SourceStamp { get; set; }
        [StringLength(4000)]
        public string SourcePayload { get; set; }
        public virtual ClientBranch ClientBranch { get; set; }
        public List<NotificationReceiver> NotificationReceivers { get; set; } = new();
        public Notification()
        {
        }
        public Notification(NoticeItem notice)
            : this()
        {
            NotificationID = notice.NoticeNumber.ToString();
            Title = notice.Title;
            Message = notice.Content;
            LinkUrl = notice.DetailURL;
            if (DateTime.TryParseExact(notice.RegistrationDateTime, ETIMSConst.FMT_DATETIME, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                NotificationDate = result;
            }
            CreatedBy = notice.RegistrantName;
            SourceStamp = DateTime.Now;

            SourcePayload = Newtonsoft.Json.JsonConvert.SerializeObject(notice);
        }
    }
    [Table("NotificationReceiver")]
    public class NotificationReceiver : BaseEntity
    {
        [Required]
        [StringLength(32)]
        public string NotificationID { get; set; }
        [Required]
        [StringLength(2)]
        public string BranchCode { get; set; }
        [Required]
        [StringLength(450)]
        public string UserID { get; set; }
        [Required]
        public bool IsRead { get; set; } = false;
        [Required]
        public bool IsDismissed { get; set; } = false;
        public DateTime? ReadDate { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Notification Notification { get; set; }
        public NotificationReceiver()
        {
        }

        public NotificationReceiver(Notification notification, string userId, string branchCode)
            : this()
        {
            this.Notification = notification;
            NotificationID = notification.NotificationID;
            UserID = userId;
            BranchCode = branchCode;
        }
    }

}

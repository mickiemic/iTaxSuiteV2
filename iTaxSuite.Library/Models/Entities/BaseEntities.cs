using iTaxSuite.Library.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace iTaxSuite.Library.Models.Entities
{
    public abstract class BaseEntity
    {
        [ScaffoldColumn(false)]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [ScaffoldColumn(false)]
        [StringLength(40)]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CreatedBy { get; set; } = "SYS-ADMIN";

        [ScaffoldColumn(false)]
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? UpdatedOn { get; set; }

        [ScaffoldColumn(false)]
        [StringLength(40)]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string UpdatedBy { get; set; }

        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime LastModified
        {
            get
            {
                if (UpdatedOn.HasValue && UpdatedOn.Value != DateTime.MinValue)
                    return UpdatedOn.Value;
                else
                    return CreatedOn;
            }
        }
    }

    public enum RecordStatus
    {
        [EnumMember(Value = "NONE")]
        [Display(Name = "NONE")]
        NONE = 1,

        [EnumMember(Value = "QUEUED IN")]
        [Display(Name = "QUEUED IN")]
        QUEUEDIN = 1001,

        [EnumMember(Value = "QUEUED OUT")]
        [Display(Name = "QUEUED OUT")]
        QUEUEDOUT = 1051,

        [EnumMember(Value = "MANUAL OUT")]
        [Display(Name = "MANUAL OUT")]
        MANUALOUT = 3001,

        [EnumMember(Value = "POST OK")]
        [Display(Name = "POST OK")]
        POST_OK = 2001,

        [EnumMember(Value = "POST FAIL")]
        [Display(Name = "POST FAIL")]
        POST_FAIL = 2051,

        [EnumMember(Value = "POST DUPL")]
        [Display(Name = "POST DUPL")]
        POST_DUPL = 2011,

        [EnumMember(Value = "INVALID")]
        [Display(Name = "INVALID")]
        INVALID = 5001,

        [EnumMember(Value = "DEPENDS")]
        [Display(Name = "DEPENDS")]
        DEPENDS = 5051
    }
    public enum RecordStatusGroup
    {
        [EnumMember(Value = "ALL")]
        [Display(Name = "ALL")]
        ALL = 1,
        [EnumMember(Value = "SUCCESSFUL")]
        [Display(Name = "SUCCESSFUL")]
        SUCCESSFUL = 2,
        [EnumMember(Value = "FAILED")]
        [Display(Name = "FAILED")]
        FAILED = 3,
        [EnumMember(Value = "QUEUED")]
        [Display(Name = "QUEUED")]
        QUEUED = 4
    }

    public abstract class BaseTransact : BaseEntity
    {
        [Required]
        public RecordStatus RecordStatus { get; set; } = RecordStatus.NONE;
        [StringLength(1024)]
        public string Remark { get; set; }
        public int Tries { get; set; }
        public DateTime? LastTry { get; set; }
        public abstract string CacheKey { get; }
    }
    public abstract class BaseTransactData : BaseTransact
    {
        public DateTime? SourceStamp { get; set; }
        public string SourcePayload { get; set; }
        public DateTime? RequestTime { get; set; }
        [StringLength(4000)]
        public string RequestPayload { get; set; }
        public DateTime? ResponseTime { get; set; }
        [StringLength(4000)]
        public string ResponsePayload { get; set; }
    }
    public abstract class BaseDataEntity : BaseEntity
    {
        public DateTime? SourceStamp { get; set; }
        public string SourcePayload { get; set; }
        public DateTime? RequestTime { get; set; }
        [StringLength(4000)]
        public string RequestPayload { get; set; }
        public DateTime? ResponseTime { get; set; }
        [StringLength(4000)]
        public string ResponsePayload { get; set; }
    }
    public abstract class BaseCacheKey
    {
        public RecordStatus RecordStatus { get; set; } = RecordStatus.NONE;
    }

    [Table("TaxClient")]
    public class TaxClient : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(16)]
        public string ClientCode { get; set; }
        [Required]
        [StringLength(100)]
        public string ClientName { get; set; }
        [Required]
        [StringLength(32)]
        public string TaxNumber { get; set; }
        [Required]
        [StringLength(16)]
        public string SystemCode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [StringLength(1)]
        [Column(TypeName = "char(1)")]
        public string LockColumn { get; set; } = "X";
        public virtual ExtSystConfig ExtSystConfig { get; set; }
    }

    [Table("EntityAttribute")]
    public class EntityAttribute : BaseEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttributeID { get; set; }
        [Required]
        [StringLength(32)]
        public string EntityType { get; set; }
        [Required]
        [StringLength(32)]
        public string SearchKey { get; set; }
        [Required]
        [StringLength(32)]
        public string EntityKey { get; set; }
        [Required]
        [StringLength(32)]
        public string Title { get; set; }
        [StringLength(64)]
        public string ExtraKey { get; set; }
        [StringLength(64)]
        public string ExtraValue { get; set; }
        public int SortOrder { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CacheKey => $"{EntityType}:{SearchKey}";
    }
    public class AttributeFilter : APagedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        [StringLength(32)]
        public string SearchKey { get; set; }
    }

    [Table("SyncChannel")]
    public class SyncChannel : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(32)]
        public string ChannelId { get; set; }
        [Required]
        [StringLength(64, MinimumLength = 3)]
        public string ChannelName { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
        [Required]
        public int TriggerType { get; set; }
        [Required]
        [StringLength(64)]
        public string TriggerExpr { get; set; } = "0 0 0 1 1 *";
        public DateTime? LastAccess { get; set; }
        [StringLength(32)]
        public string KeyCol { get; set; }
        [StringLength(32)]
        public string UniqIDCol { get; set; }
        [StringLength(32)]
        public string DateCol { get; set; }
        [StringLength(256)]
        public string SyncTrackExpr { get; set; }
        public DateTime? LastTracked { get; set; }
        [NotMapped]     // Dynamic Tracker Object
        public SyncTracker Tracker { get; set; }

        public SyncChannel()
        {
            Tracker = new SyncTracker();
        }

        public void InitTracker()
        {
            if (!string.IsNullOrWhiteSpace(SyncTrackExpr))
            {
                Tracker = Newtonsoft.Json.JsonConvert.DeserializeObject<SyncTracker>(SyncTrackExpr);
            }
        }
        public DateTime GetMinDate()
        {
            var minDateTime = new DateTime(2024, 11, 1);
            if (string.IsNullOrWhiteSpace(DateCol) || Tracker is null || !Tracker.MinDateTime.HasValue)
            {
                return minDateTime;
            }
            else if (Tracker.MinDateTime.HasValue && Tracker.MinDateTime.Value >= minDateTime)
            {
                minDateTime = Tracker.MinDateTime.Value;
            }
            return minDateTime;
        }
        public void UpdateTracker(string strValue, bool IncrOffset = false)
        {
            Tracker.SetValue(strValue, IncrOffset);
        }
        public void UpdateTracker(long longValue, bool IncrOffset = false)
        {
            Tracker.SetValue(longValue, IncrOffset);
        }
        public void UpdateTracker(DateTime? dateValue, bool IncrOffset = false)
        {
            Tracker.SetValue(dateValue, IncrOffset);
        }
        public bool ResetTracker()
        {
            if (Tracker != null)
            {
                Tracker.OffSet = 0;
                Tracker.LastStamp = DateTime.Now;
                return true;
            }
            return false;
        }
        public void UpdateOffSet(int OffSet)
        {
            Tracker.OffSet = OffSet;
        }
        public void IncrOffSet(int OffSet)
        {
            Tracker.OffSet += OffSet;
        }
        public int OffSet => Tracker.OffSet;
    }
    public class SyncTracker
    {
        public string FieldName { get; set; }
        public bool IsNumeric { get; set; }
        public object Value { get; set; }
        public DateTime? MinDateTime { get; set; } = new DateTime(2024, 11, 1);
        public DateTime LastStamp { get; set; } = DateTime.Now;
        public int OffSet { get; set; }

        public SyncTracker() { }

        public SyncTracker(string fieldName, bool isNumeric, int offset = 0)
        {
            FieldName = fieldName;
            IsNumeric = isNumeric;
            OffSet = offset;
        }

        public void SetValue(string strValue, bool IncrOffset = false)
        {
            Value = strValue;
            LastStamp = DateTime.Now;
            if (IncrOffset)
                OffSet++;
        }
        public void SetValue(long longValue, bool IncrOffset = false)
        {
            Value = longValue;
            LastStamp = DateTime.Now;
            if (IncrOffset)
                OffSet++;
        }
        public void SetValue(DateTime? dateValue, bool IncrOffset = false)
        {
            if (!dateValue.HasValue)
                return;
            MinDateTime = dateValue;
            LastStamp = DateTime.Now;
            if (IncrOffset)
                OffSet++;
        }

        public string GetStrValue()
        {
            return (string)Value;
        }
        public long GetLongValue()
        {
            return (long)Value;
        }
        public int GetIntValue()
        {
            return (int)Value;
        }
    }

}

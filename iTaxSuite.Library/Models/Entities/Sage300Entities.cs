using iTaxSuite.Library.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTaxSuite.Library.Models.Entities
{
    public enum SourceType
    {
        Sage300CERP = 1001,
        SageX3ERP = 2001,
        MANUALENTRY = 9001
    }

    [Table("ExtSystConfig")]
    public class ExtSystConfig : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(16)]
        public string SystemCode { get; set; }
        [Required]
        public SourceType SystemType { get; set; }
        [Required]
        [StringLength(256)]
        public string ApiAddress { get; set; }
        [StringLength(256)]
        public string ExtApiAddress { get; set; }
        [StringLength(32)]
        public string Username { get; set; }
        [StringLength(256)]
        public string Password { get; set; }
        [StringLength(256)]
        public string AuthToken { get; set; }
        [Required]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [StringLength(1)]
        [Column(TypeName = "char(1)")]
        public string LockColumn { get; set; } = "X";
    }

    [Table("S300TaxAuthority")]
    public class S300TaxAuthority : BaseEntity
    {
        [Key]
        [Required]
        [StringLength(48, MinimumLength = 3)]
        public string AuthorityKey { get; set; }
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
        public bool Active { get; set; } = false;
        public Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority.TaxTypeEnum TaxType { get; set; }
        public Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority.TaxBaseEnum TaxBase { get; set; }
        public DateTime LastMaintained { get; set; }
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CacheKey => $"{AuthorityKey}";
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ObjData { get; set; }

        [NotMapped]
        public List<S300TaxClass> Classes { get; set; } = new();
        [NotMapped]
        public List<S300TaxRate> Rates { get; set; } = new();
        public S300TaxAuthority()
        {
        }

        public S300TaxAuthority(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority authority)
            : base()
        {
            AuthorityKey = authority.TaxAuthorityKey;
            Currency = authority.TaxReportingCurrency;
            TaxBase = authority.TaxBase;
            TaxType = authority.TaxType;
            LastMaintained = authority.LastMaintained.Value;
            authority.TaxClasses.ForEach(tClass =>
            {
                Classes.Add(new S300TaxClass(tClass));
            });
        }
        public class TaxAuthorityFilter : APagedFilter
        {
            public bool IsValid => true;
            [StringLength(48)]
            public string AuthorityKey { get; set; }
            [StringLength(3)]
            public string Currency { get; set; }
        }
    }
    public class S300TaxClassRates
    {
        public List<S300TaxClass> Classes { get; set; } = new();
        public List<S300TaxRate> Rates { get; set; } = new();
        public S300TaxClassRates()
        {
        }
        public S300TaxClassRates(S300TaxAuthority taxAuthority)
        {
            Classes = taxAuthority.Classes;
            Rates = taxAuthority.Rates;
        }
    }
    public class S300TaxAuthKey
    {
        [StringLength(48, MinimumLength = 3)]
        public string AuthorityKey { get; set; }
        public string CacheKey { get; set; }
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
        public bool Active { get; set; } = false;
        public Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority.TaxTypeEnum TaxType { get; set; }
        public Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxAuthority.TaxBaseEnum TaxBase { get; set; }
        public DateTime LastMaintained { get; set; }
        public List<S300TaxClass> Classes { get; set; } = new();
        public List<S300TaxRate> Rates { get; set; } = new();
        public S300TaxAuthKey()
        {
        }
        public S300TaxAuthKey(S300TaxAuthority taxAuthority)
            : this()
        {
            AuthorityKey = taxAuthority.AuthorityKey;
            CacheKey = taxAuthority.CacheKey;
            Currency = taxAuthority.Currency;
            Active = taxAuthority.Active;
            TaxType = taxAuthority.TaxType;
            TaxBase = taxAuthority.TaxBase;
            LastMaintained = taxAuthority.LastMaintained;
            if (!string.IsNullOrWhiteSpace(taxAuthority.ObjData))
            {
                var classRates = Newtonsoft.Json.JsonConvert.DeserializeObject<S300TaxClassRates>(taxAuthority.ObjData);
                Classes = classRates.Classes;
                Rates = classRates.Rates;
            }
        }
    }

}

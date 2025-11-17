using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace iTaxSuite.Library.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(8, MinimumLength = 6)]
        public string DistributorCode { get; set; }

        [StringLength(24)]
        public string FirstName { get; set; }
        [StringLength(24)]
        public string LastName { get; set; }

        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [StringLength(32)]
        public string EmployeeCode { get; set; }
        [StringLength(32)]
        public string SalesPersonKey { get; set; }

        [StringLength(1024)]
        public string ProcessMeta { get; set; }
        public DateTime LastLoginDateTime { get; set; }
    }
}

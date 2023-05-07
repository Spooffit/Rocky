using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        [NotMapped]
        public string StreetAddress { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        public string State { get; set; }
        [NotMapped]
        public string PostalCode { get; set; }

    }
}

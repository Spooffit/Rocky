using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models
{
    public class Product
    {
        public Product()
        {
            TempSqFt = 1;
        }
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDesc { get; set; }

        [Range(1, int.MaxValue)]
        public int Price { get; set; }

        [ValidateNever]
        public string Image { get; set; }

        [ValidateNever]
        [Display(Name = "Category Type")]
        public int CategoryId { get; set; }

        [ValidateNever]
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ValidateNever]
        [Display(Name = "Application Type")]
        public int ApplicationTypeId { get; set; }

        [ValidateNever]
        [ForeignKey("ApplicationTypeId")]
        public virtual ApplicationType ApplicationType { get; set; }

        [NotMapped]
        [Range(1,10000, ErrorMessage = "Sqft must be greater than 0.")]
        public virtual int TempSqFt { get; set; }
    }
}

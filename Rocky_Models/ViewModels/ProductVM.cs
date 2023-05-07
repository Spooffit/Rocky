using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rocky_Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategorySelectList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ApplicationTypeSelectList { get; set; }

    }
}

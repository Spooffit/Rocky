using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }

        public InquiryController
            (IInquiryHeaderRepository inquiryHeaderRepository, 
            IInquiryDetailRepository inquiryDetailRepository)
        {
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET - DETAILS
        public IActionResult Details(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inquiryHeaderRepository.FirstOrDefault(u => u.Id == id, includeProperties:"ApplicationUser"),
                InquiryDetail = _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == id, includeProperties:"Product"),
            };

            return View(InquiryVM);
        }

        // POST - DETAILS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            InquiryVM.InquiryDetail = _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            foreach (var detail in InquiryVM.InquiryDetail)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = detail.ProductId,
                };
                shoppingCartList.Add(shoppingCart);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            HttpContext.Session.Set(WC.SessionInquiryId, InquiryVM.InquiryHeader.Id);

            TempData[WC.Success] = "Inquiry has been added to cart successfully";

            return RedirectToAction("Index","Cart");
        }

        // POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inquiryHeaderRepository.FirstOrDefault(u => u.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquiryDetail> inquiryDetails = _inquiryDetailRepository.GetAll(u => u.InquiryHeaderId == InquiryVM.InquiryHeader.Id);

            _inquiryDetailRepository.RemoveRange(inquiryDetails);
            _inquiryHeaderRepository.Remove(inquiryHeader);

            _inquiryHeaderRepository.Save();
            TempData[WC.Success] = "Inquiry has been deleted successfully";

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetInquiryList() 
        {
            return Json(new { data = _inquiryHeaderRepository.GetAll() });
        }

        #endregion
    }
}

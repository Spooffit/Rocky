using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using Rocky_Utility.BrainTree;
using System.Security.Claims;
using System.Text;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IApplicationUserRepository _appUserRepository;

        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;

        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        private readonly IBrainTreeGate _brain;

        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }    
        public CartController
            (IProductRepository productRepository, 
            IApplicationUserRepository appUserRepository,
            IInquiryHeaderRepository inquiryHeaderRepository,
            IInquiryDetailRepository inquiryDetailRepository,
            IWebHostEnvironment env, 
            IEmailSender emailSender,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IBrainTreeGate brain)
        {
            _productRepository = productRepository;
            _appUserRepository = appUserRepository;
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
            _env = env;
            _emailSender = emailSender;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _brain = brain;
        }

        //GET - INDEX
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();

            IEnumerable<Product> productListTemp = _productRepository.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> productList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product productTemp = productListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                productTemp.TempSqFt = cartObj.SqFt;
                productList.Add(productTemp);
            }

            return View(productList);
        }

        //POST - INDEX
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product product in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = product.Id, SqFt = product.TempSqFt });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }

        //GET - SUMMARY
        public IActionResult Summary()
        {
            ApplicationUser applicationUser;

            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    InquiryHeader inquiryHeader = _inquiryHeaderRepository.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber,
                    };
                }
                else 
                {
                    applicationUser = new ApplicationUser();
                }

                var gateway = _brain.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                applicationUser = _appUserRepository.FirstOrDefault(u => u.Id == claim.Value);
            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();

            IEnumerable<Product> productList = _productRepository.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _productRepository.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }

        //POST - SUMMARY

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ActionName("Summary")]
        //public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM productUserVM)
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        //    if (User.IsInRole(WC.AdminRole))
        //    {
        //        OrderHeader orderHeader = new OrderHeader()
        //        {
        //            CreatedByUserId = claim.Value,
        //            FinalOrderTotal = ProductUserVM.ProductList.Sum(u => u.Price * u.TempSqFt),
        //            City = productUserVM.ApplicationUser.City,
        //            StreetAddress = productUserVM.ApplicationUser.StreetAddress,
        //            State = productUserVM.ApplicationUser.State,
        //            PostalCode = productUserVM.ApplicationUser.PostalCode,
        //            FullName = productUserVM.ApplicationUser.FullName,
        //            Email = productUserVM.ApplicationUser.Email,
        //            PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
        //            OrderDate = DateTime.Now,
        //            OrderStatus = WC.StatusPending,
        //        };
        //        _orderHeaderRepository.Add(orderHeader);
        //        _orderHeaderRepository.Save();

        //        foreach (var product in productUserVM.ProductList)
        //        {
        //            OrderDetail orderDetail = new OrderDetail()
        //            {
        //                OrderHeaderId = orderHeader.Id,
        //                PricePerSqFt = product.Price,
        //                Sqft = product.TempSqFt,
        //                ProductId = product.Id,
        //            };
        //            _orderDetailRepository.Add(orderDetail);
        //        }
        //        _orderDetailRepository.Save();

        //        string nonceFromTheClient = collection["payment_method_nonce"];

        //        var request = new TransactionRequest
        //        {
        //            Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
        //            PaymentMethodNonce = nonceFromTheClient,
        //            OrderId = orderHeader.Id.ToString(),
        //            Options = new TransactionOptionsRequest
        //            {
        //                SubmitForSettlement = true
        //            }
        //        };

        //        var gateway = _brain.GetGateway();

        //        Result<Transaction> result = gateway.Transaction.Sale(request);

        //        if(result.IsSuccess())
        //        {
        //            if (result.Target.ProcessorResponseText == "Approved")
        //            {
        //                orderHeader.TransactionId = result.Target.Id;
        //                orderHeader.OrderStatus = WC.StatusApproved;
        //            }
        //        }
        //        else
        //        {
        //            orderHeader.OrderStatus= WC.StatusCancelled;
        //        }
        //        _orderHeaderRepository.Save();

        //        return RedirectToAction(nameof(InquiryConfirmation), new {id = orderHeader.Id});
        //    }
        //    else 
        //    {
        //        var PathToTemplate = _env.WebRootPath + Path.DirectorySeparatorChar.ToString()
        //        + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

        //        var subject = "New Inquiry";
        //        string HtmlBody = "";

        //        using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
        //        {
        //            HtmlBody = sr.ReadToEnd();
        //        }

        //        // Name: { 0}
        //        // Email: { 1}
        //        // Phone: { 2}
        //        // Products { 3}

        //        StringBuilder productListSB = new StringBuilder();

        //        foreach (var product in productUserVM.ProductList)
        //        {
        //            productListSB.Append($" - Name: {product.Name} <span style='font-size:14px;'> (ID: {product.Id}) </span><br>");
        //        }

        //        string messageBody = string.Format(HtmlBody,
        //            productUserVM.ApplicationUser.FullName,
        //            productUserVM.ApplicationUser.Email,
        //            productUserVM.ApplicationUser.PhoneNumber,
        //            productListSB.ToString());

        //        await _emailSender.SendEmailAsync(productUserVM.ApplicationUser.Email, subject, messageBody);

        //        InquiryHeader inquiryHeader = new InquiryHeader()
        //        {
        //            ApplicationUserId = claim.Value,
        //            FullName = productUserVM.ApplicationUser.FullName,
        //            PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
        //            Email = productUserVM.ApplicationUser.Email,
        //            InquiryDate = DateTime.Now
        //        };

        //        _inquiryHeaderRepository.Add(inquiryHeader);
        //        _inquiryHeaderRepository.Save();

        //        foreach (var product in productUserVM.ProductList)
        //        {
        //            InquiryDetail inquiryDetail = new InquiryDetail()
        //            {
        //                InquiryHeaderId = inquiryHeader.Id,
        //                ProductId = product.Id,
        //            };
        //            _inquiryDetailRepository.Add(inquiryDetail);
        //        }
        //        _inquiryDetailRepository.Save();
        //    }

        //    return RedirectToAction(nameof(InquiryConfirmation));
        //}

        public IActionResult InquiryConfirmation(int id=0)
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            TempData[WC.Success] = "Item has been removed from cart successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product product in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = product.Id, SqFt = product.TempSqFt });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Clear();

            TempData[WC.Success] = "Items have been removed from cart successfully";
            return RedirectToAction("Index","Home");
        }
    }

}

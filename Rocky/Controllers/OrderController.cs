using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using Rocky_Utility.BrainTree;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class OrderController : Controller
    {

        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController
            (IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IBrainTreeGate brain)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _brain = brain;
        }

        public IActionResult Index(string searchName = null, string searchEmail = null, string searchPhone = null, string Status = null)
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHeaderList = _orderHeaderRepository.GetAll(),
                StatusList = WC.ListStatus.ToList().Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = x,
                    Value = x
                })
            };

            if(!string.IsNullOrEmpty(searchName)) 
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.FullName.ToLower().Contains(searchName.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchEmail))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()));
            }
            if (!string.IsNullOrEmpty(searchPhone))
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.PhoneNumber.ToLower().Contains(searchPhone.ToLower()));
            }
            if (!string.IsNullOrEmpty(Status) && Status != "--Order Status--")
            {
                orderListVM.OrderHeaderList = orderListVM.OrderHeaderList.Where(u => u.OrderStatus.ToLower().Contains(Status.ToLower()));
            }

            return View(orderListVM);
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == id),
                OrderDetail= _orderDetailRepository.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult StartProcessing()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusInProcess;

            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order Status has been updated successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = WC.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order has been shipped successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            var gateway = _brain.GetGateway();
            Transaction transaction = gateway.Transaction.Find(orderHeader.TransactionId);

            if(transaction.Status == TransactionStatus.AUTHORIZED || transaction.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT)
            {
                // no refund
                Result<Transaction> resultvoid = gateway.Transaction.Void(orderHeader.TransactionId);
            }
            else
            {
                // refund
                Result<Transaction> resultRefund = gateway.Transaction.Refund(orderHeader.TransactionId);

            }

            orderHeader.OrderStatus = WC.StatusRefunded;

            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order has been canceled successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderHeaderFromDb = _orderHeaderRepository.FirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);

            orderHeaderFromDb.FullName = OrderVM.OrderHeader.FullName;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeaderFromDb.Email = OrderVM.OrderHeader.Email;

            _orderHeaderRepository.Save();
            TempData[WC.Success] = "Order Details have been updated successfully";

            return RedirectToAction("Details","Order", new {id=orderHeaderFromDb.Id});
        }
    
    }
}

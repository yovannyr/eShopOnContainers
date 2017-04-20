using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebMVC.ViewModels;

namespace WebMVC.Controllers
{
    [Authorize]
    public class OrderManagementController : Controller
    {
        private IOrderingService _orderSvc;
        private readonly IIdentityParser<ApplicationUser> _appUserParser;
        public OrderManagementController(IOrderingService orderSvc, IIdentityParser<ApplicationUser> appUserParser)
        {
            _appUserParser = appUserParser;
            _orderSvc = orderSvc;
        }

        public async Task<IActionResult> Index()
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var vm = await _orderSvc.GetMyOrders(user);                        

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> OrderProcess(string orderId, string actionCode)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);

            if(OrderProcessAction.CheckStock.Code == actionCode)
            {
                await _orderSvc.CheckStockOrderProcess(order, Guid.NewGuid().ToString());
            }
            else if(OrderProcessAction.RecordPayment.Code == actionCode)
            {
                await _orderSvc.RecordPaymentOrderProcess(order, Guid.NewGuid().ToString());
            }
            else if (OrderProcessAction.Ship.Code == actionCode)
            {
                await _orderSvc.ShipOrderProcess(order, Guid.NewGuid().ToString());
            }
            else if (OrderProcessAction.Refund.Code == actionCode)
            {
                await _orderSvc.RefundOrderProcess(order, Guid.NewGuid().ToString());
            }
            else if (OrderProcessAction.Cancel.Code == actionCode)
            {
                await _orderSvc.CancelOrderProcess(order, Guid.NewGuid().ToString());
            }
            else if (OrderProcessAction.Complete.Code == actionCode)
            {
                await _orderSvc.CompletedOrderProcess(order, Guid.NewGuid().ToString());
            }

            return RedirectToAction("Index");
        }        
    }
}
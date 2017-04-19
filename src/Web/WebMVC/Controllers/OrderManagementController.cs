using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;

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

        public async Task<IActionResult> CheckStock(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.CheckStockOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RecordPayment(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.RecordPaymentOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Ship(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.ShipOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Refund(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.RefundOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Cancel(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.CancelOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Complete(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.CompletedOrderProcess(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }
    }
}
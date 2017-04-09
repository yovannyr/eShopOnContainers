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

        public async Task<IActionResult> Process(string orderId)
        {
            var user = _appUserParser.Parse(HttpContext.User);
            var order = await _orderSvc.GetOrder(user, orderId);
            await _orderSvc.CreateProcessOrder(order, Guid.NewGuid().ToString());
            return RedirectToAction("Index");
        }
    }
}
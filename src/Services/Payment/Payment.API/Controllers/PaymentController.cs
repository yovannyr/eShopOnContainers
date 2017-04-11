using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Model;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Payment.API.IntegrationEvents.Events;

namespace Payment.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IEventBus _eventBus;

        public PaymentController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }        

        // POST api/payment/1
        [Route("{orderId}")]
        [HttpPost]
        public void Post(int orderId)
        {
            // Fake Payment gateway
            // Send integration event to indicate that the payment is successfully done
            var evt = new OrderPaidIntegrationEvent(orderId, true);
            _eventBus.Publish(evt);
        }        
    }
}

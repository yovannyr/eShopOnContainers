using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;
using Microsoft.eShopOnContainers.Services.Ordering.API.Infrastructure.Services;
using Ordering.API.Application.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IOrderQueries _orderQueries;
        private readonly IIdentityService _identityService;

        public OrdersController(IMediator mediator, IOrderQueries orderQueries, IIdentityService identityService)
        {

            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        }

        [Route("{orderId:int}")]
        [HttpGet]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var order = await _orderQueries
                    .GetOrderAsync(orderId);

                return Ok(order);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderQueries
                .GetOrdersAsync();

            return Ok(orders);
        }

        [Route("cardtypes")]
        [HttpGet]
        public async Task<IActionResult> GetCardTypes()
        {
            var cardTypes = await _orderQueries
                .GetCardTypesAsync();

            return Ok(cardTypes);
        }

        [Route("new")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody]CreateOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;
            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestCreateOrder);
            }
            else
            {
                // If no x-requestid header is found we process the order anyway. This is just temporary to not break existing clients
                // that aren't still updated. When all clients were updated this could be removed.
                result = await _mediator.SendAsync(command);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("checkStock")]
        [HttpPut]
        public async Task<IActionResult> CheckStockOrderProcess([FromBody]CheckStockInventoryCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<CheckStockInventoryCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }            

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("recordPayment")]
        [HttpPut]
        public async Task<IActionResult> RecordPaymentOrderProcess([FromBody]RecordPaymentCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<RecordPaymentCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("ship")]
        [HttpPut]
        public async Task<IActionResult> ShipOrderProcess([FromBody]ShipOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<ShipOrderCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("refund")]
        [HttpPut]
        public async Task<IActionResult> RefundOrderProcess([FromBody]RefundOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<RefundOrderCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("cancel")]
        [HttpPut]
        public async Task<IActionResult> CancelOrderProcess([FromBody]CancelOrderCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<CancelOrderCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        [Route("complete")]
        [HttpPut]
        public async Task<IActionResult> CompleteOrderProcess([FromBody]CompleteOrderProcessCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;

            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestProcessOrder = new IdentifiedCommand<CompleteOrderProcessCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestProcessOrder);
            }

            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}



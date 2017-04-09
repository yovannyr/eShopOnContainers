using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Application.DomainEventHandlers.OrderCompleted
{
    public class OrderCompletedEventHandler
                   : IAsyncNotificationHandler<OrderCompletedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILoggerFactory _logger;
        public OrderCompletedEventHandler(
            IOrderRepository orderRepository, ILoggerFactory logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Domain Logic comment:
        // When the order is correctly finished in process ordering saga, 
        // then it updates the state of the order to completed status
        public async Task Handle(OrderCompletedEvent orderCompletedEvent)
        {
            // TODO:
            //var orderToUpdate = await _orderRepository.GetAsync(orderCompletedEvent.OrderId);
            //orderToUpdate.SetOrderStatusId(OrderStatus.Shipped.Id);
            //_orderRepository.Update(orderToUpdate);
            //await _orderRepository.UnitOfWork.SaveChangesAsync();

            _logger.CreateLogger(nameof(OrderCompletedEventHandler))
                .LogTrace($"Thre process Order with Id: {orderCompletedEvent.OrderId} has been successfully completed and is ready to ship");
        }
    }
}

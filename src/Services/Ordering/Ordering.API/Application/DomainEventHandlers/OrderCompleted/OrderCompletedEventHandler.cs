using Autofac;
using Autofac.Features.OwnedInstances;
using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure;
using Microsoft.Extensions.Logging;
using Ordering.Domain.Events;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Application.DomainEventHandlers.OrderCompleted
{
    public class OrderCompletedEventHandler
                   : IAsyncNotificationHandler<OrderCompletedEvent>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILoggerFactory _logger;

        public OrderCompletedEventHandler(
            ILifetimeScope lifetimeScope, ILoggerFactory logger)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Domain Logic comment:
        // When the order is correctly finished in process ordering saga, 
        // then it updates the state of the order to completed status
        public async Task Handle(OrderCompletedEvent orderCompletedEvent)
        {
            // A new lifetimescope must be created for OrderingContext since it is 
            // disposed when event is received
            using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var orderRepository = scope.Resolve<IOrderRepository>();
                var orderToUpdate = await orderRepository.GetAsync(orderCompletedEvent.OrderId);
                orderToUpdate.SetOrderStatusId(OrderStatus.Shipped.Id);
                orderRepository.Update(orderToUpdate);
                await orderRepository.UnitOfWork.SaveChangesAsync();
            }                

            _logger.CreateLogger(nameof(OrderCompletedEventHandler))
                .LogTrace($"Thre process Order with Id: {orderCompletedEvent.OrderId} has been successfully completed and is ready to ship");
        }
    }
}

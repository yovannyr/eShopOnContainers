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
                   : IAsyncNotificationHandler<OrderProcessCompletedEvent>
    {
        private readonly ILoggerFactory _logger;

        public OrderCompletedEventHandler(ILoggerFactory logger)
        {           
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderProcessCompletedEvent orderCompletedEvent)
        {
            // TODO: Notify user that the order has been completed and shipped

            _logger.CreateLogger(nameof(OrderCompletedEventHandler))
                .LogTrace($"Thre process Order with Id: {orderCompletedEvent.OrderId} has been successfully completed and shipped");

            await Task.FromResult(1);
        }
    }
}

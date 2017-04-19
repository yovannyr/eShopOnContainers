using Autofac.Features.OwnedInstances;
using MediatR;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure;
using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;
using Microsoft.Extensions.Options;
using Ordering.API.Application.Commands;
using Ordering.API.Application.IntegrationEvents.Events;
using Ordering.Domain.Events;
using Ordering.Domain.Exceptions;
using Ordering.Domain.SagaData;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Application.Sagas
{

        /// <summary>
        /// Saga for enforcing record payment, stock checking
        /// and order shipping
        /// Once successfully executed, the saga is 
        /// marked as completed
        /// </summary>
        public class OrderProcessSaga : Saga<Order>,
        IAsyncRequestHandler<CheckStockInventoryCommand, bool>,
        IAsyncRequestHandler<RecordPaymentCommand, bool>,
        IAsyncRequestHandler<ShipOrderCommand, bool>,
        IAsyncRequestHandler<CancelOrderCommand, bool>,
        IAsyncRequestHandler<RefundOrderCommand, bool>,
        IAsyncRequestHandler<CompleteOrderProcessCommand, bool>,
        IIntegrationEventHandler<OrderPaidIntegrationEvent>,
        IIntegrationEventHandler<StockCheckedIntegrationEvent>
    {
        private readonly IHttpClient _apiClient;
        private readonly IOptionsSnapshot<Settings> _settings;
        private readonly IMediator _mediator;
        private readonly Func<Owned<OrderingContext>> _dbContextFactory;

        public OrderProcessSaga(
            IHttpClient httpClient, IOptionsSnapshot<Settings> settings,
            Func<Owned<OrderingContext>> dbContextFactory, OrderingContext orderingContext,
            IMediator mediator) 
            : base(orderingContext)
        {
            _apiClient = httpClient;
            _settings = settings;
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
        }

        /// <summary>
        /// Entry point of the order process saga
        /// Handler which processes the command when
        /// administrator executes stock checking
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(CheckStockInventoryCommand command)
        {
            var result = true;
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId == OrderStatus.AwaitingCheckStock.Id)
            {
                // Call catalog api to check inventory stock 
                var catalogResponse = await _apiClient.PutAsync($"{_settings.Value.CatalogUrl}/api/v1/stock/stocktoremovefromproducts", command);
                result &= catalogResponse.IsSuccessStatusCode;
            }

            return result;
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes record payment
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(RecordPaymentCommand command)
        {
            var result = true;
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId == OrderStatus.AwaitingRecordPayment.Id)
            {
                // Call payment api to record the new payment
                var paymentResponse = await _apiClient.PostAsync($"{_settings.Value.PaymentUrl}/api/v1/payment/{command.OrderNumber}", command);
                result &= paymentResponse.IsSuccessStatusCode;
            }

            return result;
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes ship order
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(ShipOrderCommand command)
        {
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId == OrderStatus.AwaitingShipment.Id)
            {
                orderSaga.SetOrderStatusId(OrderStatus.Shipped.Id);                
            }            

            return await SaveChangesAsync();
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes cancel order
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(CancelOrderCommand command)
        {
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId != OrderStatus.Cancelled.Id)
            {
                // TODO: call catalog api to restock items ordered
                orderSaga.SetOrderStatusId(OrderStatus.Cancelled.Id);
            }

            return await SaveChangesAsync();
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes refund order
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(RefundOrderCommand command)
        {
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId != OrderStatus.Refunded.Id)
            {
                // TODO: do order refund
                orderSaga.SetOrderStatusId(OrderStatus.Refunded.Id);
            }

            return await SaveChangesAsync();
        }

        /// <summary>
        /// Handler which processes the command when
        /// administrator executes completed order
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(CompleteOrderProcessCommand command)
        {
            var orderSaga = FindSagaById(command.OrderNumber);
            CheckValidSagaId(orderSaga);

            if (orderSaga.OrderStatusId == OrderStatus.Shipped.Id)
            {
                // Send domain event to notify the customer that the order is shipped           
                var orderCompletedEvt = new OrderProcessCompletedEvent(orderSaga.Id);
                await _mediator.PublishAsync(orderCompletedEvt);

                orderSaga.SetOrderStatusId(OrderStatus.Completed.Id);
            }

            return await SaveChangesAsync();
        }

        /// <summary>
        /// Integration event handler which processes
        /// the event sent by catalog api when a new 
        /// stock checking is executed for an order
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task Handle(StockCheckedIntegrationEvent @event)
        {
            using (var ctx = _dbContextFactory().Value)
            {
                var orderSaga = FindSagaById(@event.OrderId, ctx);
                CheckValidSagaId(orderSaga);

                if (@event.IsSuccess)
                {
                    orderSaga.SetOrderStatusId(OrderStatus.AwaitingRecordPayment.Id);
                }

                await SaveChangesAsync(ctx);
            }
        }

        /// <summary>
        /// Integration event handler which processes 
        /// the event sent by record payment api when
        /// when a payment is executed for an order
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task Handle(OrderPaidIntegrationEvent @event)
        {
            // Managing an owned lifetimescope for OrderingContext since it is 
            // disposed when event is received
            using (var ctx = _dbContextFactory().Value)
            {
                var orderSaga = FindSagaById(@event.OrderId, ctx);
                CheckValidSagaId(orderSaga);

                if (@event.IsSuccess)
                {
                    orderSaga.SetOrderStatusId(OrderStatus.AwaitingShipment.Id);
                }

                await SaveChangesAsync(ctx);
            }
        }      

        private void CheckValidSagaId(Order orderSaga)
        {
            if (orderSaga is null)
            {
                throw new OrderingDomainException("Not able to process order saga event. Reason: no valid orderId");
            }
        }


        #region CommandHandlerIdentifiers

        public class CheckStockCommandIdentifiedHandler : IdentifierCommandHandler<CheckStockInventoryCommand, bool>
        {
            public CheckStockCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        public class RecordPaymentCommandIdentifiedHandler : IdentifierCommandHandler<RecordPaymentCommand, bool>
        {
            public RecordPaymentCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        public class ShipOrderCommandIdentifiedHandler : IdentifierCommandHandler<ShipOrderCommand, bool>
        {
            public ShipOrderCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        public class CancelOrderCommandIdentifiedHandler : IdentifierCommandHandler<CancelOrderCommand, bool>
        {
            public CancelOrderCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        public class RefundOrderCommandIdentifiedHandler : IdentifierCommandHandler<RefundOrderCommand, bool>
        {
            public RefundOrderCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        public class CompleteOrderProcessCommandIdentifiedHandler : IdentifierCommandHandler<CompleteOrderProcessCommand, bool>
        {
            public CompleteOrderProcessCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
            {
            }

            protected override bool CreateResultForDuplicateRequest()
            {
                return true;                // Ignore duplicate requests for processing order.
            }
        }

        #endregion

    }
}

using Autofac.Features.OwnedInstances;
using MediatR;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure;
using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;
using Microsoft.Extensions.Options;
using Ordering.API.Application.Commands;
using Ordering.API.IntegrationEvents.Events;
using Ordering.Domain.Events;
using Ordering.Domain.Exceptions;
using Ordering.Domain.SagaData;
using System;
using System.Threading.Tasks;

namespace Ordering.API.Application.Sagas
{
    public class OrderProcessCommandIdentifiedHandler : IdentifierCommandHandler<StartOrderProcessCommand, bool>
    {
        public OrderProcessCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;                // Ignore duplicate requests for processing order.
        }
    }

    /// <summary>
    /// Saga for enforcing payment and stock checking
    /// If both are successfully executed, the saga is 
    /// marked as completed and the order's state is 
    /// updated accordingly
    /// </summary>
    public class OrderProcessSaga : Saga<OrderSagaData>,
        IAsyncRequestHandler<StartOrderProcessCommand, bool>,
        IIntegrationEventHandler<OrderPaidIntegrationEvent>,
        IIntegrationEventHandler<StockCheckedIntegrationEvent>
    {
        private readonly IHttpClient _apiClient;
        private readonly IOptionsSnapshot<Settings> _settings;
        private readonly IMediator _mediator;
        private readonly Func<Owned<OrderingContext>> _dbContextFactory;

        public OrderProcessSaga(
            IHttpClient httpClient, IOptionsSnapshot<Settings> settings,
            Func<Owned<OrderingContext>> dbContextFactory, IMediator mediator) 
            : base()
        {
            _apiClient = httpClient;
            _settings = settings;
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
        }

        /// <summary>
        /// Entry point of the order process saga
        /// Handler which processes the command when
        /// user executes process order
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(StartOrderProcessCommand command)
        {
            bool result = true;
            using (var ctx = _dbContextFactory().Value)
            {
                if (!ExistSaga(command.OrderNumber, ctx))
                {
                    AddSaga(new OrderSagaData()
                    {
                        CorrelationId = command.OrderNumber,
                        Originator = nameof(StartOrderProcessCommand)
                    }, 
                    ctx);

                    await SaveChangesAsync(ctx);
                }
            }

            // Call catalog api to check inventory stock 
            _apiClient.Inst.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
            var catalogResponse = await _apiClient.PostAsync($"{_settings.Value.CatalogUrl}/api/v1/stock/stocktoremovefromproducts", command);
            result &= catalogResponse.IsSuccessStatusCode;

            // Call payment gateway api to create the payment
            _apiClient.Inst.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
            var paymentResponse = await _apiClient.PostAsync($"{_settings.Value.PaymentUrl}/api/v1/payment/{command.OrderNumber}", command);
            result &= paymentResponse.IsSuccessStatusCode;

            // Call catalog api to RemoveStock from items in the order
            // TO DO

            return result;
        }

        /// <summary>
        /// Integration event handler which processes 
        /// the event sent by payment gateway api when
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
                orderSaga.IsPaymentDone = @event.IsSuccess;
                UpdateSaga(orderSaga, ctx);                
                await CheckForSagaCompletionAsync(orderSaga, ctx);
                await SaveChangesAsync(ctx);
            }
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
                orderSaga.IsStockProvided = @event.IsSuccess;
                UpdateSaga(orderSaga, ctx);                
                await CheckForSagaCompletionAsync(orderSaga, ctx);
                await SaveChangesAsync(ctx);
            }
        }

        private async Task CheckForSagaCompletionAsync(OrderSagaData saga, OrderingContext ctx)
        {
            if(saga.IsPaymentDone && saga.IsStockProvided)
            {
                // Set saga as completed
                MarkSagaAsCompleted(saga);
                UpdateSaga(saga, ctx);

                // Send domain event to update order's state            
                var orderCompletedEvt = new OrderProcessCompletedEvent(saga.CorrelationId);
                await _mediator.PublishAsync(orderCompletedEvt);
            }            
        }

        private void CheckValidSagaId(OrderSagaData orderSaga)
        {
            if (orderSaga is null)
            {
                throw new OrderingDomainException("Not able to process order process saga event.Reason: no valid orderId");
            }
        }
    }
}

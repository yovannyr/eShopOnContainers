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
    public class ProcessOrderCommandIdentifiedHandler : IdentifierCommandHandler<OrderProcessCommand, bool>
    {
        public ProcessOrderCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;                // Ignore duplicate requests for processing order.
        }
    }

    /// <summary>
    /// Saga for enforcing payment and stock
    /// execution before shipping order
    /// </summary>
    public class OrderProcessSaga : Saga<OrderSagaData>,
        IAsyncRequestHandler<OrderProcessCommand, bool>,
        IIntegrationEventHandler<OrderPaidIntegrationEvent>,
        IIntegrationEventHandler<StockCheckedIntegrationEvent>
    {
        private readonly IHttpClient _apiClient;
        private readonly IOptionsSnapshot<Settings> _settings;
        private readonly IMediator _mediator;
        private Func<Owned<OrderingContext>> _dbContextFactory;

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
        /// Handler for processing ProcessOrder command
        /// Entry point of the ordering saga
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<bool> Handle(OrderProcessCommand command)
        {
            bool result = true;
            using (var ctx = _dbContextFactory().Value)
            {
                if (!ExistSaga(command.OrderNumber, ctx))
                {
                    AddSagaState(new OrderSagaData()
                    {
                        CorrelationId = command.OrderNumber,
                        Originator = nameof(OrderProcessCommand)
                    }, 
                    ctx);

                    await SaveChangesAsync(ctx);
                }
            }

            // Call catalog api to create a stock inventory
            _apiClient.Inst.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
            var catalogResponse = await _apiClient.PostAsync($"{_settings.Value.CatalogUrl}/api/v1/stock", command);
            result &= catalogResponse.IsSuccessStatusCode;

            // Call payment gateway api to create the payment
            _apiClient.Inst.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
            var paymentResponse = await _apiClient.PostAsync($"{_settings.Value.PaymentUrl}/api/v1/payment/{command.OrderNumber}", command);
            result &= paymentResponse.IsSuccessStatusCode;

            return result;
        }

        /// <summary>
        /// Integration event handler which processes 
        /// the bus event when the payment is done
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task Handle(OrderPaidIntegrationEvent @event)
        {
            // A new lifetimescope must be created for OrderingContext since it is 
            // disposed when event is received
            using (var ctx = _dbContextFactory().Value)
            {
                var orderSaga = FindById(@event.OrderId, ctx);
                CheckValidSagaId(orderSaga);
                orderSaga.IsPaymentDone = @event.IsSuccess;
                UpdateSagaState(orderSaga, ctx);                
                await CheckForSagaCompletionAsync(orderSaga, ctx);
                await SaveChangesAsync(ctx);
            }
        }

        /// <summary>
        /// Integration event handler which processes the bus
        /// event received when the inventory stock is checked
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task Handle(StockCheckedIntegrationEvent @event)
        {
            using (var ctx = _dbContextFactory().Value)
            {
                var orderSaga = FindById(@event.OrderId, ctx);
                CheckValidSagaId(orderSaga);
                orderSaga.IsStockProvided = @event.IsSuccess;
                UpdateSagaState(orderSaga, ctx);                
                await CheckForSagaCompletionAsync(orderSaga, ctx);
                await SaveChangesAsync(ctx);
            }
        }

        private async Task CheckForSagaCompletionAsync(OrderSagaData saga, OrderingContext ctx)
        {
            if(saga.IsPaymentDone && saga.IsStockProvided)
            {
                // Set saga as completed
                MarkAsCompleted(saga);
                UpdateSagaState(saga, ctx);

                // Send domain event to update order's state to complete and save                
                var orderCompletedEvt = new OrderCompletedEvent(saga.CorrelationId);
                await _mediator.PublishAsync(orderCompletedEvt);
            }            
        }

        private void CheckValidSagaId(OrderSagaData orderSaga)
        {
            if (orderSaga is null)
            {
                throw new OrderingDomainException("Not able to process Stock Requested event.Reason: no valid orderId");
            }
        }
    }
}

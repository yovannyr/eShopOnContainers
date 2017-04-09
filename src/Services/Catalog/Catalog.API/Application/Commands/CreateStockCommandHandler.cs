using Catalog.API.Infrastructure.Idempotency;
using Catalog.API.IntegrationEvents;
using Catalog.API.IntegrationEvents.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Application.Commands
{
    public class CreateStockCommandIdentifiedHandler : IdentifierCommandHandler<CreateStockCommand, bool>
    {
        public CreateStockCommandIdentifiedHandler(IMediator mediator, IRequestManager requestManager) : base(mediator, requestManager)
        {
        }

        protected override bool CreateResultForDuplicateRequest()
        {
            return true;                // Ignore duplicate requests for creating order.
        }
    }

    public class CreateStockCommandHandler
        : IAsyncRequestHandler<CreateStockCommand, bool>
    {
        private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;
        private readonly CatalogContext _catalogContext;
        public CreateStockCommandHandler(
            ICatalogIntegrationEventService catalogIntegrationEventService,
            CatalogContext catalogContext)
        {
            _catalogIntegrationEventService = catalogIntegrationEventService;
            _catalogContext = catalogContext;
        }

        public async Task<bool> Handle(CreateStockCommand message)
        {
            var ids = message.OrderItems.Select(i => i.ProductId);
            var productsToUpdate = _catalogContext.CatalogItems
                .Where(c => ids.Contains(c.Id))
                .ToList();

            // Remove number of products provided from stock
            productsToUpdate.ForEach((productToUpdate) => {
                foreach(var item in message.OrderItems) {
                    if(item.ProductId == productToUpdate.Id)
                    {
                        productToUpdate.RemoveStock(item.Units);
                        _catalogContext.Update(productToUpdate);
                        break;
                    }
                };
            });
            var result = await _catalogContext.SaveChangesAsync();
            var isSuccess = result > 0;

            // Send Integration event to ordering api in order to update order saga status            
            var evt = new StockCreatedIntegrationEvent(message.OrderNumber, isSuccess);
            await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(evt);
            await _catalogIntegrationEventService.PublishThroughEventBusAsync(evt);
            return isSuccess;
        }
    }
}

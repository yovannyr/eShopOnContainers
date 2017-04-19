using Catalog.API.IntegrationEvents;
using Catalog.API.IntegrationEvents.Events;
using Catalog.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;

        public StockController(
            CatalogContext Context,
            ICatalogIntegrationEventService catalogIntegrationEventService)
        {
            _catalogContext = Context;
            _catalogIntegrationEventService = catalogIntegrationEventService;
            ((DbContext)Context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        //PUT api/v1/[controller]/stocktoremovefromproducts
        [Route("stocktoremovefromproducts")]
        [HttpPut]
        public async Task<IActionResult> RemoveStockFromProducs([FromBody]OrderedItems orderedItems)
        {
            var ids = orderedItems.OrderItems.Select(i => i.ProductId);
            var productsToUpdate = _catalogContext.CatalogItems
                .Where(c => ids.Contains(c.Id))
                .ToList();

            // Remove number of products provided from stock
            productsToUpdate.ForEach((productToUpdate) => {
                foreach (var item in orderedItems.OrderItems)
                {
                    if (item.ProductId == productToUpdate.Id)
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
            var evt = new StockCheckedIntegrationEvent(orderedItems.OrderNumber, isSuccess);
            await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(evt);
            await _catalogIntegrationEventService.PublishThroughEventBusAsync(evt);

            if (isSuccess)
            {
                return Ok();
            }

            return BadRequest();
        }

        //PUT api/v1/[controller]/products/1/stocktoadd/3
        [Route("products/{productId}/stocktoadd/{quantity}")]
        [HttpPut]
        public async Task<IActionResult> AddStockToProduct(int productId, int quantity)
        {
            var productToUpdate = _catalogContext.CatalogItems
                .Where(x => x.Id == productId).SingleOrDefault();
            productToUpdate.AddStock(quantity);
            _catalogContext.Update(productToUpdate);
            var result = await _catalogContext.SaveChangesAsync();

            if (result > 0)
            {
                return Ok();
            }

            return BadRequest();
        }        
        
    }
}

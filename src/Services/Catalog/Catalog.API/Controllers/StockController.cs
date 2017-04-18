using Catalog.API.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly IOptionsSnapshot<Settings> _settings;
        private readonly IMediator _mediator;

        public StockController(
            CatalogContext Context, IOptionsSnapshot<Settings> settings, IMediator mediator)
        {
            _catalogContext = Context;
            _settings = settings;
            _mediator = mediator;

            ((DbContext)Context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        //PUT api/v1/[controller]/stocktoremovefromproducts
        [Route("stocktoremovefromproducts")]
        [HttpPut]
        public async Task<IActionResult> RemoveStockFromProducs([FromBody]RemoveStockFromProducsCommand command, [FromHeader(Name = "x-requestid")] string requestId)
        {
            bool result = false;
            if (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty)
            {
                var requestCreateStock = new IdentifiedCommand<RemoveStockFromProducsCommand, bool>(command, guid);
                result = await _mediator.SendAsync(requestCreateStock);
            }            

            if (result)
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

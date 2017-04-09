using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.IntegrationEvents.Events
{
    // Integration Events notes: 
    // An Event is “something that has happened in the past”, therefore its name has to be   
    // An Integration Event is an event that can cause side effects to other microsrvices, Bounded-Contexts or external systems.
    public class StockCreatedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; private set; }

        public bool IsSuccess { get; private set; }

        public StockCreatedIntegrationEvent(int orderId, bool result)
        {
            OrderId = orderId;
            IsSuccess = result;
        }
    }
}

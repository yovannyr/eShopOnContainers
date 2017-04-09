using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    public class OrderCompletedEvent
        : IAsyncNotification
    {        
        public int OrderId { get; private set; }

        public OrderCompletedEvent(int orderId)
        {
            OrderId = orderId;            
        }
    }
}

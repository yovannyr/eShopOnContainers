using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Events
{
    public class OrderProcessCompletedEvent
        : IAsyncNotification
    {        
        public int OrderId { get; private set; }

        public OrderProcessCompletedEvent(int orderId)
        {
            OrderId = orderId;            
        }
    }
}

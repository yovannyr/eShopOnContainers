using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ordering.API.Application.Commands
{
    public class RefundOrderCommand : IAsyncRequest<bool>
    {

        [DataMember]
        public int OrderNumber { get; private set; }

        public RefundOrderCommand(int orderNumber)
        {
            OrderNumber = orderNumber;
        }
    }
}

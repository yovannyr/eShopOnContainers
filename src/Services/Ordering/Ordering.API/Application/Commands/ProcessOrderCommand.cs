using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ordering.API.Application.Commands
{
    // DDD and CQRS patterns comment: Note that it is recommended to implement immutable Commands
    // In this case, its immutability is achieved by having all the setters as private
    // plus only being able to update the data just once, when creating the object through its constructor.
    // References on Immutable Commands:  
    // http://cqrs.nu/Faq
    // https://docs.spine3.org/motivation/immutability.html 
    // http://blog.gauffin.org/2012/06/griffin-container-introducing-command-support/
    // https://msdn.microsoft.com/en-us/library/bb383979.aspx

    [DataContract]
    public class ProcessOrderCommand
        : IAsyncRequest<bool>
    {

        [DataMember]
        public int OrderNumber { get; private set; }

        [DataMember]
        private readonly List<OrderItemDTO> _orderItems;

        [DataMember]
        public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

        public ProcessOrderCommand(int orderId)
        {
            OrderNumber = orderId;
            _orderItems = new List<OrderItemDTO>();
        }

        public class OrderItemDTO
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal Discount { get; set; }

            public int Units { get; set; }

            public string PictureUrl { get; set; }
        }
    }
}

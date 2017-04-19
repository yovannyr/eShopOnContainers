using System.Collections.Generic;

namespace Catalog.API.Model
{
    public class OrderedItems
    {
        public int OrderNumber { get; private set; }

        private readonly List<OrderItemDTO> _orderItems;

        public IEnumerable<OrderItemDTO> OrderItems => _orderItems;

        public void AddOrderItem(OrderItemDTO item)
        {
            _orderItems.Add(item);
        }

        public OrderedItems(int orderNumber)
        {
            OrderNumber = orderNumber;
            _orderItems = new List<OrderItemDTO>();
        }

        public class OrderItemDTO
        {
            public int ProductId { get; set; }

            public int Units { get; set; }
        }
    }
}

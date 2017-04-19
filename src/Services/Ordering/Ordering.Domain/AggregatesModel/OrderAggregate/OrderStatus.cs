namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate
{
    using global::Ordering.Domain.Exceptions;
    using Seedwork;
    using SeedWork;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class OrderStatus
        : Enumeration
    {
        /* customer started the checkout process, but did not complete it */
        public static OrderStatus Pending = new OrderStatus(1, nameof(Pending).ToLowerInvariant());
        /* customer has completed checkout process, but payment has yet to be confirmed */
        public static OrderStatus AwaitingPayment  = new OrderStatus(2, nameof(AwaitingPayment).ToLowerInvariant());
        /* customer has completed the payment but administrator has not yet record it */
        public static OrderStatus AwaitingRecordPayment = new OrderStatus(3, nameof(AwaitingRecordPayment).ToLowerInvariant());
        /* administrator has record de payment but he has not yet check the stock */
        public static OrderStatus AwaitingCheckStock = new OrderStatus(4, nameof(AwaitingCheckStock).ToLowerInvariant());
        /* stock has been successfully checked, but shipment has not been confirmed yet */
        public static OrderStatus AwaitingShipment = new OrderStatus(5, nameof(AwaitingShipment).ToLowerInvariant());
        /* administrator has cancelled an order, due to a stock inconsistency or other reasons.  */
        public static OrderStatus Cancelled = new OrderStatus(6, nameof(Cancelled).ToLowerInvariant());
        /* administrator has used the Refund action */
        public static OrderStatus Refunded = new OrderStatus(7, nameof(Refunded).ToLowerInvariant());
        /* order has been shipped, but receipt has not been confirmed */
        public static OrderStatus Shipped = new OrderStatus(8, nameof(Shipped).ToLowerInvariant());
        /*  order has been shipped/picked up, and receipt is confirmed */
        public static OrderStatus Completed = new OrderStatus(9, nameof(Completed).ToLowerInvariant());

        protected OrderStatus()
        {
        }

        public OrderStatus(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<OrderStatus> List()
        {
            return new[] { Pending,
                AwaitingPayment,
                AwaitingCheckStock,
                AwaitingRecordPayment,
                AwaitingShipment,
                Cancelled,
                Refunded,
                Shipped,
                Completed };
        }

        public static OrderStatus FromName(string name)
        {
            var state = List()
                .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (state == null)
            {
                throw new OrderingDomainException($"Possible values for OrderStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }

        public static OrderStatus From(int id)
        {
            var state = List().SingleOrDefault(s => s.Id == id);

            if (state == null)
            {
                throw new OrderingDomainException($"Possible values for OrderStatus: {String.Join(",", List().Select(s => s.Name))}");
            }

            return state;
        }
    }
}

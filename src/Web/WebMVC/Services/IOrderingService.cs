using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public interface IOrderingService
    {
        Task<List<Order>> GetMyOrders(ApplicationUser user);
        Task<Order> GetOrder(ApplicationUser user, string orderId);
        Task CreateOrder(Order order);
        Task CheckStockOrderProcess(Order order, string requestId);
        Task RecordPaymentOrderProcess(Order order, string requestId);
        Task ShipOrderProcess(Order order, string requestId);
        Task RefundOrderProcess(Order order, string requestId);
        Task CancelOrderProcess(Order order, string requestId);
        Task CompletedOrderProcess(Order order, string requestId);
        Order MapUserInfoIntoOrder(ApplicationUser user, Order order);
        void OverrideUserInfoIntoOrder(Order original, Order destination);
    }
}

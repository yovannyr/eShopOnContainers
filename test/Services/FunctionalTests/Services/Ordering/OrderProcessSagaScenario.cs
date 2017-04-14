using FunctionalTests.Extensions;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Newtonsoft.Json;
using Ordering.API.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace FunctionalTests.Services.Ordering
{
    public class OrderProcessSagaScenario : OrderingScenariosBase
    {
        [Fact]
        public async Task Create_order_and_check_state_not_completed()
        {
            using (var server = CreateServer())
            {
                var client = server.CreateIdempotentClient();

                // GIVEN an order is created              
                await client.PostAsync(Post.AddNewOrder, new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json"));

                var ordersResponse = await client.GetAsync(Get.Orders);
                var responseBody = await ordersResponse.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(responseBody);
                string orderId = orders.OrderByDescending(o => o.Date).First().OrderNumber;

                //WHEN we request the order bit its id
                var order = await client.GetAsync(Get.OrderBy(int.Parse(orderId)));
                var orderBody = await order.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Order>(orderBody);

                //THEN the requested order is returned 
                Assert.Equal(orderId, result.OrderNumber);
                Assert.Equal("inprocess", result.Status);
                Assert.Equal(1, result.OrderItems.Count);
                Assert.Equal(10, result.OrderItems[0].UnitPrice);
            }
        }

        [Fact]
        public async Task Create_process_order_and_check_state_completed()
        {
            using (var server = CreateServer())
            {
                var client = server.CreateIdempotentClient();
                bool continueLoop = true;
                var counter = 0;
                Order result = null;

                // GIVEN an process order is created 
                await client.PostAsync(Post.AddNewOrder, new StringContent(BuildOrder(), UTF8Encoding.UTF8, "application/json"));
                var ordersResponse = await client.GetAsync(Get.Orders);
                var responseBody = await ordersResponse.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Order>>(responseBody);
                string orderId = orders.OrderByDescending(o => o.Date).First().OrderNumber;
                // Create process order from given orderid
                client = server.CreateIdempotentClient();
                await client.PostAsync(Post.AddProcessOrder, new StringContent(BuildProcessOrder(orderId), UTF8Encoding.UTF8, "application/json"));

                //WHEN we request the order bit its id
                while (continueLoop && counter < 10)
                {                    
                    var order = await client.GetAsync(Get.OrderBy(int.Parse(orderId)));
                    var orderBody = await order.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<Order>(orderBody);
                    if (result.IsCompleted.HasValue)
                    {
                        continueLoop = false;
                    }
                    else
                    {
                        counter++;
                        await Task.Delay(1000);
                    }
                }
                //THEN the requested order is returned 
                Assert.Equal(orderId, result.OrderNumber);
                Assert.Equal("shipped", result.Status);
                Assert.Equal(1, result.OrderItems.Count);
                Assert.Equal(10, result.OrderItems[0].UnitPrice);
            }
        }

        string BuildOrder()
        {
            var order = new CreateOrderCommand(
                cardExpiration: DateTime.UtcNow.AddYears(1),
                cardNumber: "5145-555-5555",
                cardHolderName: "Jhon Senna",
                cardSecurityNumber: "232",
                cardTypeId: 1,
                city: "Redmon",
                country: "USA",
                state: "WA",
                street: "One way",
                zipcode: "zipcode",
                paymentId: 1,
                buyerId: 3
            );

            order.AddOrderItem(new CreateOrderCommand.OrderItemDTO()
            {
                ProductId = 1,
                Discount = 8M,
                UnitPrice = 10,
                Units = 1,
                ProductName = "Some name"
            });

            return JsonConvert.SerializeObject(order);
        }

        string BuildProcessOrder(string orderId)
        {
            var orderItems = new List<StartOrderProcessCommand.OrderItemDTO>() {
                new StartOrderProcessCommand.OrderItemDTO()
                {
                    ProductId = 1,
                    Units = 2,
                }
            };

            var order = new StartOrderProcessCommand(
                int.Parse(orderId),
                orderItems
            );            
            return JsonConvert.SerializeObject(order);
        }
    }    
}

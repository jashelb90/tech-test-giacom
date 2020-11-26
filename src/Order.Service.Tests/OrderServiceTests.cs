using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order.Model.Enums;
using Order.Model.Requests;
using Order.Utility;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private OrderContext _orderContext;

        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusCompletedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _orderContext = new OrderContext(options);
            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }
        
        [Test]
        public async Task GetOrderByStatusAsync_ReturnsOrderWithCorrectValues()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);
            
            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2, DateTime.Now, _orderStatusCompletedId);

            // Act
            var orders = await _orderService.GetOrdersByStatusAsync(OrderStatusEnum.completed);

            // Assert
            Assert.AreEqual(1, orders.Count());
            Assert.AreEqual(orderId2, orders.FirstOrDefault().Id);
            Assert.AreEqual(1.6m, orders.FirstOrDefault().TotalCost);
            Assert.AreEqual(1.8m, orders.FirstOrDefault().TotalPrice);
            
            // Act
            orders = await _orderService.GetOrdersByStatusAsync(OrderStatusEnum.created);

            // Assert
            Assert.AreEqual(1, orders.Count());
            Assert.AreEqual(orderId1, orders.FirstOrDefault().Id);
            Assert.AreEqual(1.6m, orders.FirstOrDefault().TotalCost);
            Assert.AreEqual(1.8m, orders.FirstOrDefault().TotalPrice);
        }
        
        [Test]
        public async Task UpdateOrderStatus_ReturnsOrderWithUpdatedStatus()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.UpdateOrderStatusAsync(orderId1, OrderStatusEnum.completed);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
            Assert.AreEqual(OrderStatusEnum.completed.GetDescription(), order.StatusName);
            
            // Act
            order = await _orderService.UpdateOrderStatusAsync(orderId1, OrderStatusEnum.created);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
            Assert.AreEqual(OrderStatusEnum.created.GetDescription(), order.StatusName);
        }
        
        [Test]
        public async Task CreateOrder_ReturnsOrderId()
        {
            // Arrange
            var createOrder = new CreateOrder
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItem>
                {
                    new CreateOrderItem
                    {
                        Cost = 5.5m,
                        Price = 10.5m,
                        Quantity = 3,
                        ProductId = new Guid(_orderProductEmailId),
                        ServiceId = new Guid(_orderServiceEmailId)
                    }
                }
            };

            // Act
            var orderGuid = await _orderService.CreateOrderAsync(createOrder);

            // Assert
            Assert.NotNull(orderGuid);
            Assert.AreEqual(typeof(Guid), orderGuid.GetType());

            var order = await _orderContext.Order.FindAsync(orderGuid.ToByteArray());
            
            Assert.IsNotNull(order);
            Assert.AreEqual(orderGuid.ToByteArray(), order.Id);
            Assert.AreEqual(OrderStatusEnum.created.GetDescription(), order.Status.Name);
            Assert.AreEqual(createOrder.CustomerId.ToByteArray(), order.CustomerId);
            Assert.AreEqual(createOrder.ResellerId.ToByteArray(), order.ResellerId);
            Assert.AreEqual(3, order.Items.FirstOrDefault().Quantity);
            Assert.AreEqual(orderGuid.ToByteArray(), order.Items.FirstOrDefault().OrderId);
            Assert.AreEqual(_orderProductEmailId, order.Items.FirstOrDefault().ProductId);
            Assert.AreEqual(_orderServiceEmailId, order.Items.FirstOrDefault().ServiceId);
        }
        
        [Test]
        public async Task GetOrderProfit_ReturnsProfitData()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2, DateTime.Now, _orderStatusCompletedId);
            
            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2, DateTime.Now.AddMonths(-1), _orderStatusCompletedId);
            
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 2, DateTime.Now.AddMonths(-2), _orderStatusCompletedId);

            // Act
            var profitDetails = await _orderService.GetOrderProfitAsync();

            // Assert
            Assert.NotNull(profitDetails);
            
            var profitDetailsList = profitDetails.ToList();
            
            Assert.AreEqual(3, profitDetailsList.Count());

            for (var i = 0; i < profitDetailsList.Count(); i++)
            {
                Assert.AreEqual(DateTime.Now.AddMonths(i * -1).Month, profitDetailsList[i].Month);
                Assert.AreEqual(DateTime.Now.AddMonths(i * -1).Year, profitDetailsList[i].Year);
                Assert.AreEqual(0.2m, profitDetailsList[i].Profit);
            }
        }
        
        private async Task AddOrder(Guid orderId, int quantity, DateTime createdDate = new DateTime(), byte[] orderStatusId = null)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = createdDate,
                StatusId = orderStatusId ?? _orderStatusCreatedId,
            });

            _orderContext.OrderItem.Add(new OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
           
            orderContext.OrderStatus.AddRange(new List<OrderStatus>
            {
                new OrderStatus
                {
                    Id = _orderStatusCreatedId,
                    Name = "Created",
                },
                new OrderStatus
                {
                    Id = _orderStatusCompletedId,
                    Name = "Completed",
                }
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}

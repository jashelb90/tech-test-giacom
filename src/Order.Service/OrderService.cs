using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Order.Model.Enums;
using Order.Model.Requests;
using Order.Model.Responses;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(OrderStatusEnum orderStatus)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(orderStatus);
            return orders;
        }

        public async Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, OrderStatusEnum orderStatus)
        {
            return await _orderRepository.UpdateOrderStatusAsync(orderId, orderStatus);
        }

        public async Task<Guid> CreateOrderAsync(CreateOrder createOrder)
        {
            return await _orderRepository.CreateOrderAsync(createOrder);
        }

        public Task<IEnumerable<OrderProfitDetail>> GetOrderProfitAsync()
        {
            return _orderRepository.GetOrderProfitAsync();
        }
    }
}

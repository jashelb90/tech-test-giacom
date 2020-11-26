using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Order.Model.Enums;
using Order.Model.Requests;
using Order.Model.Responses;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
        
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(OrderStatusEnum orderStatusEnum);
        
        Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, OrderStatusEnum orderStatusEnum);
        
        Task<Guid> CreateOrderAsync(CreateOrder createOrder);

        Task<IEnumerable<OrderProfitDetail>> GetOrderProfitAsync();
    }
}

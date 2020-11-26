using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order.Model.Enums;
using Order.Model.Requests;
using Order.Model.Responses;
using Order.Utility;
using Order.Utility.Exceptions;
using OrderItem = Order.Data.Entities.OrderItem;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orderEntities = await _orderContext.Order
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var orders = orderEntities.Select(x => new OrderSummary
            {
                Id = new Guid(x.Id),
                ResellerId = new Guid(x.ResellerId),
                CustomerId = new Guid(x.CustomerId),
                StatusId = new Guid(x.StatusId),
                StatusName = x.Status.Name,
                ItemCount = x.Items.Count,
                TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                CreatedDate = x.CreatedDate
            });

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order.SingleOrDefaultAsync(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes );
            if (order == null)
            {
                return null;
            }

            var orderDetail = new OrderDetail
            {
                Id = new Guid(order.Id),
                ResellerId = new Guid(order.ResellerId),
                CustomerId = new Guid(order.CustomerId),
                StatusId = new Guid(order.StatusId),
                StatusName = order.Status.Name,
                CreatedDate = order.CreatedDate,
                TotalCost = order.Items.Sum(x => x.Quantity * x.Product.UnitCost).Value,
                TotalPrice = order.Items.Sum(x => x.Quantity * x.Product.UnitPrice).Value,
                Items = order.Items.Select(x => new Model.OrderItem
                {
                    Id = new Guid(x.Id),
                    OrderId = new Guid(x.OrderId),
                    ServiceId = new Guid(x.ServiceId),
                    ServiceName = x.Service.Name,
                    ProductId = new Guid(x.ProductId),
                    ProductName = x.Product.Name,
                    UnitCost = x.Product.UnitCost,
                    UnitPrice = x.Product.UnitPrice,
                    TotalCost = x.Product.UnitCost * x.Quantity.Value,
                    TotalPrice = x.Product.UnitPrice * x.Quantity.Value,
                    Quantity = x.Quantity.Value
                })
            };

            return orderDetail;
        }
        
        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(OrderStatusEnum orderStatusEnum)
        {
            //Ideally I would have kept this as an IQueryable and returned the list after selecting the DTO however
            //I would have had to change the integration test methodology used so for the purpose of the tech test I will leave it as this
            var orderEntities = await _orderContext.Order
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            var orders = orderEntities
                .Where(p => p.Status.Name.Equals(orderStatusEnum.GetDescription())) 
                .Select(p => new OrderSummary
                {
                    Id = new Guid(p.Id),
                    ResellerId = new Guid(p.ResellerId),
                    CustomerId = new Guid(p.CustomerId),
                    StatusId = new Guid(p.StatusId),
                    StatusName = p.Status.Name,
                    ItemCount = p.Items.Count,
                    TotalCost = p.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = p.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = p.CreatedDate
                });

            return orders;
        }

        public async Task<OrderDetail> UpdateOrderStatusAsync(Guid orderId, OrderStatusEnum orderStatusEnum)
        {
            var orderIdBytes = orderId.ToByteArray();
            
            var order = await _orderContext.Order.SingleOrDefaultAsync(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes);
            
            if (order == null)
            {
                throw new EntityNotFoundException(typeof(Entities.Order), orderId);
            }

            var status =
                await _orderContext.OrderStatus.FirstOrDefaultAsync(
                    p => p.Name.Equals(orderStatusEnum.GetDescription()));
            
            if (status == null)
            {
                throw new EntityNotFoundException($"There is no such entity. Entity type: OrderStatus Name: {orderStatusEnum.GetDescription()}");
            }

            order.StatusId = status.Id;

            order = _orderContext.Update(order).Entity;

            await _orderContext.SaveChangesAsync();

            return new OrderDetail
            {
                Id = new Guid(order.Id),
                ResellerId = new Guid(order.ResellerId),
                CustomerId = new Guid(order.CustomerId),
                StatusId = new Guid(order.StatusId),
                StatusName = order.Status.Name,
                CreatedDate = order.CreatedDate,
                TotalCost = order.Items.Sum(x => x.Quantity * x.Product.UnitCost).Value,
                TotalPrice = order.Items.Sum(x => x.Quantity * x.Product.UnitPrice).Value,
                Items = order.Items.Select(x => new Model.OrderItem
                {
                    Id = new Guid(x.Id),
                    OrderId = new Guid(x.OrderId),
                    ServiceId = new Guid(x.ServiceId),
                    ServiceName = x.Service.Name,
                    ProductId = new Guid(x.ProductId),
                    ProductName = x.Product.Name,
                    UnitCost = x.Product.UnitCost,
                    UnitPrice = x.Product.UnitPrice,
                    TotalCost = x.Product.UnitCost * x.Quantity.Value,
                    TotalPrice = x.Product.UnitPrice * x.Quantity.Value,
                    Quantity = x.Quantity.Value
                })
            };
        }
        
        public async Task<Guid> CreateOrderAsync(CreateOrder createOrder)
        {
            var status =
                await _orderContext.OrderStatus.FirstOrDefaultAsync(
                    p => p.Name.Equals(OrderStatusEnum.created.GetDescription()));

            var orderId = Guid.NewGuid().ToByteArray();
            
            var order = new Entities.Order
            {
                Id = orderId,
                CustomerId = createOrder.CustomerId.ToByteArray(),
                ResellerId = createOrder.ResellerId.ToByteArray(),
                StatusId = status.Id,
                CreatedDate = DateTime.Now,
                Items = createOrder.Items.Select(p => new OrderItem
                {
                    Id = Guid.NewGuid().ToByteArray(),
                    ProductId = p.ProductId.ToByteArray(),
                    ServiceId = p.ServiceId.ToByteArray(),
                    OrderId = orderId,
                    Quantity = p.Quantity
                }).ToList()
            };
            
            await _orderContext.Order.AddAsync(order);

            await _orderContext.SaveChangesAsync();
            
            return new Guid(order.Id);
        }

        public async Task<IEnumerable<OrderProfitDetail>> GetOrderProfitAsync()
        {
            var orders = await _orderContext.Order
                .ToListAsync();
                
            return orders.Where(p => p.Status.Name.Equals(OrderStatusEnum.completed.GetDescription()))
                .Select(p => new
                {
                    p.CreatedDate.Month,
                    p.CreatedDate.Year,
                    TotalCost = p.Items.Sum(x => x.Quantity * x.Product.UnitCost).Value,
                    TotalPrice = p.Items.Sum(x => x.Quantity * x.Product.UnitPrice).Value
                })
                .GroupBy(p => new
                {
                    p.Year,
                    p.Month
                }, (key, group) => new OrderProfitDetail
                {
                    Month = key.Month,
                    Year = key.Year,
                    Profit = group.Sum(order => order.TotalPrice - order.TotalCost)
                })
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month);
        }
    }
}

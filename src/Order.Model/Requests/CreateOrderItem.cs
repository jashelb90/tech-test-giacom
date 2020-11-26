using System;
using Order.Utility.Attributes;

namespace Order.Model.Requests
{
    public class CreateOrderItem
    {
        [RequiredNotDefault]
        public Guid ProductId { get; set; }
        
        [RequiredNotDefault]
        public Guid ServiceId { get; set; }
        
        [RequiredNotDefault]
        public int Quantity { get; set; }
        
        [RequiredNotDefault]
        public decimal Cost { get; set; }
        
        [RequiredNotDefault]
        public decimal Price { get; set; }
    }
}
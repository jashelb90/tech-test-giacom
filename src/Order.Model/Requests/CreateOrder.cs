using System;
using System.Collections.Generic;
using Order.Utility.Attributes;

namespace Order.Model.Requests
{
    public class CreateOrder
    {
        [RequiredNotDefault]
        public Guid ResellerId { get; set; }
        
        [RequiredNotDefault]
        public Guid CustomerId { get; set; }

        [RequiredCollection]
        public IEnumerable<CreateOrderItem> Items { get; set; } = new List<CreateOrderItem>();
    }
}
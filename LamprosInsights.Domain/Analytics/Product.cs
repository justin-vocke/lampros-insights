using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Domain.Analytics
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = default!;

        public string Category { get; set; } = default!;

        public string SKU { get; set; } = default!;

        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}

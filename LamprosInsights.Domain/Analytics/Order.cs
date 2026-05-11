using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Domain.Analytics
{
    public class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = default!;

        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public Customer Customer { get; set; } = default!;

        public ICollection<OrderItem> OrderItems { get; set; } = [];

        public ICollection<Invoice> Invoices { get; set; } = [];
    }
}

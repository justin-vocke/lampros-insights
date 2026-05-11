using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Domain.Analytics
{
    public class Region
    {
        public int RegionId { get; set; }

        public string Name { get; set; } = default!;

        public ICollection<Customer> Customers { get; set; } = [];

        public ICollection<SalesRep> SalesReps { get; set; } = [];
    }
}

namespace LamprosInsights.Domain.Analytics
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string? Phone { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; }

        public int RegionId { get; set; }

        public int SalesRepId { get; set; }

        public Region Region { get; set; } = default!;

        public SalesRep SalesRep { get; set; } = default!;

        public ICollection<Order> Orders { get; set; } = [];
    }
}
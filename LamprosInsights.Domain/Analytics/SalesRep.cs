namespace LamprosInsights.Domain.Analytics
{
    public class SalesRep
    {
        public int SalesRepId { get; set; }

        public string FirstName { get; set; } = default!;

        public string LastName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public DateTime HireDate { get; set; }

        public int RegionId { get; set; }

        public Region Region { get; set; } = default!;

        public ICollection<Customer> Customers { get; set; } = [];
    }
}
namespace LamprosInsights.Domain.Analytics;

public class Invoice
{
    public int InvoiceId { get; set; }

    public int OrderId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    public decimal InvoiceAmount { get; set; }

    public string Status { get; set; } = default!;

    public Order Order { get; set; } = default!;

    public ICollection<Payment> Payments { get; set; } = [];
}
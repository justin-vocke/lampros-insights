namespace LamprosInsights.Domain.Analytics;

public class Payment
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = default!;

    public Invoice Invoice { get; set; } = default!;
}
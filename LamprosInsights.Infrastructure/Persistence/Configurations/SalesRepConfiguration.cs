using LamprosInsights.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LamprosInsights.Infrastructure.Persistence.Configurations;

public class SalesRepConfiguration : IEntityTypeConfiguration<SalesRep>
{
    public void Configure(EntityTypeBuilder<SalesRep> builder)
    {
        builder.HasKey(s => s.SalesRepId);

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.HireDate)
            .IsRequired();

        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.HasMany(s => s.Customers)
            .WithOne(c => c.SalesRep)
            .HasForeignKey(c => c.SalesRepId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

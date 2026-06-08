using LamprosInsights.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LamprosInsights.Infrastructure.Persistence.Configurations;

public class AnalyticsQueryConfiguration : IEntityTypeConfiguration<AnalyticsQueries>
{
    public void Configure(EntityTypeBuilder<AnalyticsQueries> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Question)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.GeneratedSql)
            .HasMaxLength(50);

        builder.Property(c => c.ExecutionTimeMs)
            .IsRequired();

        builder.Property(c => c.Success)
            .HasMaxLength(100);

        builder.Property(c => c.CreatedOn)
            .IsRequired();
    }
}

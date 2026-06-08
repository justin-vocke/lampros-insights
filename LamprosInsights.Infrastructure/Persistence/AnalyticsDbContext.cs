using LamprosInsights.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Infrastructure.Persistence
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(
            DbContextOptions<AnalyticsDbContext> options) 
            : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Region> Regions => Set<Region>();

        public DbSet<SalesRep> SalesReps => Set<SalesRep>();

        public DbSet<Invoice> Invoices => Set<Invoice>();

        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<AnalyticsQueries> AnalyticsQueries => Set<AnalyticsQueries>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AnalyticsDbContext).Assembly);
        }
    }
}

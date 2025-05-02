using Microsoft.EntityFrameworkCore;
using PaymentProcessing.Domain.Entities;
using PaymentProcessing.Domain.Model;
namespace PaymentProcessing.Infrastructure.Data
{
    public class EFDBContext : DbContext
    {
        public EFDBContext(DbContextOptions<EFDBContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<PaymentProvider> PaymentProviders => Set<PaymentProvider>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentTransaction>(builder =>
            {
                builder.HasKey(p => p.TransactionId);

                builder.Property(p => p.Amount).IsRequired();
                builder.Property(p => p.Currency).HasMaxLength(10).IsRequired();
                builder.Property(p => p.PayerEmail).HasMaxLength(255).IsRequired();
                builder.Property(p => p.Status).HasConversion<int>();
                builder.Property(p => p.TimestampUtc).HasConversion<DateTime>();

                builder.HasOne(p => p.PaymentMethod)
                    .WithMany(m => m.Transactions)
                    .HasForeignKey(p => p.PaymentMethodId);

                builder.HasOne(p => p.Provider)
                    .WithMany(pr => pr.Transactions)
                    .HasForeignKey(p => p.ProviderId);
            });

            modelBuilder.Entity<PaymentProvider>(builder =>
            {
                builder.HasKey(p => p.ProviderId);
                builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<PaymentMethod>(builder =>
            {
                builder.HasKey(p => p.PaymentMethodId);
                builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            });
            modelBuilder.Entity<PaymentTransaction>().HasIndex
                (t => new { t.ProviderId, t.Amount, t.Currency, t.PayerEmail, t.TimestampUtc });

            base.OnModelCreating(modelBuilder);
        }
    }
}

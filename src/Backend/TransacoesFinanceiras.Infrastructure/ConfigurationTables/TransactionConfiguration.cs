using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransacoesFinanceiras.Domain.Entity;

namespace TransacoesFinanceiras.Infrastructure.ConfigurationTables
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.TransactionId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.AccountId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.Operation)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(t => t.ReferenceId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(t => t.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Timestamp)
                .IsRequired();

            builder.Property(t => t.ErrorMessage)
                .HasMaxLength(500);

            builder.HasIndex(t => t.ReferenceId)
                .IsUnique();

            builder.HasIndex(t => t.AccountId);
            builder.HasIndex(t => t.Timestamp);
        }
    }
}

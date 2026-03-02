using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransacoesFinanceiras.Domain.Entity;

namespace TransacoesFinanceiras.Infrastructure.ConfigurationTables
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.AccountId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.ClientId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Balance)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.ReservedBalance)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.CreditLimit)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(a => a.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasMany(a => a.Transactions)
                .WithOne()
                .HasForeignKey("AccountId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(a => a.AvailableBalance);
        }
    }
}

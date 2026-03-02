using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransacoesFinanceiras.Domain.Entity;

namespace TransacoesFinanceiras.Infrastructure.ConfigurationTables
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.ClientId);

            builder.Property(c => c.ClientId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasMany(c => c.Accounts)
                .WithOne()
                .HasForeignKey("ClientId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Infrastructure.ConfigurationTables;

namespace TransacoesFinanceiras.Infrastructure.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());

            // Configurar concorrência
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .IsConcurrencyToken();

            modelBuilder.Entity<Account>()
                .Property(a => a.ReservedBalance)
                .IsConcurrencyToken();
        }
    }
}

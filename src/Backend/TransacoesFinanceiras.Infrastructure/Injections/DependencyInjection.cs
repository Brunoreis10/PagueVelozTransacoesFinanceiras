using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Exceptions.Exceptions;
using TransacoesFinanceiras.Infrastructure.Database;
using TransacoesFinanceiras.Infrastructure.Repository;

namespace TransacoesFinanceiras.Infrastructure.Injections
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(ResourceMessagesException.ER_023);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            return services;
        }
    }
}

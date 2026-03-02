using Microsoft.EntityFrameworkCore;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Infrastructure.Database;

namespace TransacoesFinanceiras.Infrastructure.Repository
{
    public class ClientRepository(AppDbContext context) : IClientRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Client?> GetByIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
        }

        public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            await _context.Clients.AddAsync(client, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

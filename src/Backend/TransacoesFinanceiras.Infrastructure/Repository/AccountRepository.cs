using Microsoft.EntityFrameworkCore;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Exceptions.Exceptions;
using TransacoesFinanceiras.Infrastructure.Database;

namespace TransacoesFinanceiras.Infrastructure.Repository
{
    public class AccountRepository(AppDbContext context) : IAccountRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Account?> GetByIdAsync(string accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);
        }

        public async Task<Account?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.ReferenceId == referenceId, cancellationToken);

            if (transaction == null)
                return null;

            return await GetByIdAsync(transaction.AccountId, cancellationToken);
        }

        public async Task<bool> ExistsReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AnyAsync(t => t.ReferenceId == referenceId, cancellationToken);
        }

        public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Update(account);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Retry com lock pessimista
                throw new InvalidOperationException(ResourceMessagesException.ER_024);
            }
        }

        public async Task<List<Account>> GetAccountsByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Where(a => a.ClientId == clientId)
                .Include(a => a.Transactions)
                .ToListAsync(cancellationToken);
        }
    }
}

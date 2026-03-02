using Microsoft.EntityFrameworkCore;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Infrastructure.Database;

namespace TransacoesFinanceiras.Infrastructure.Repository
{
    public class TransactionRepository(AppDbContext context) : ITransactionRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Transaction?> GetByIdAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
        }

        public async Task<List<Transaction>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<Transaction?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.ReferenceId == referenceId, cancellationToken);
        }

        public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .AnyAsync(t => t.ReferenceId == referenceId, cancellationToken);
        }
    }
}

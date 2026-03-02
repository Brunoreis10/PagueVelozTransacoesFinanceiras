using TransacoesFinanceiras.Domain.Entity;

namespace TransacoesFinanceiras.Domain.Repository
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(string transactionId, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
        Task<Transaction?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<bool> ExistsReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
    }
}

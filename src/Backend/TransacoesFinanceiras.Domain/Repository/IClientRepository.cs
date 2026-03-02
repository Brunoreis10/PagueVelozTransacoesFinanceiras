using TransacoesFinanceiras.Domain.Entity;

namespace TransacoesFinanceiras.Domain.Repository
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(string clientId, CancellationToken cancellationToken = default);
        Task AddAsync(Client client, CancellationToken cancellationToken = default);
        Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
    }
}

using TransacoesFinanceiras.Exceptions.Exceptions;

namespace TransacoesFinanceiras.Domain.Entity
{
    public class Client
    {
        public string ClientId { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public List<Account> Accounts { get; private set; } = new();

        private Client() { }

        public Client(string clientId, string name)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException(ResourceMessagesException.ER_004, nameof(clientId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(ResourceMessagesException.ER_019, nameof(name));

            ClientId = clientId;
            Name = name;
        }

        public void AddAccount(Account account)
        {
            ArgumentNullException.ThrowIfNull(account);

            Accounts.Add(account);
        }
    }
}

using TransacoesFinanceiras.Domain.Enums;
using TransacoesFinanceiras.Exceptions.Exceptions;

namespace TransacoesFinanceiras.Domain.Entity
{
    public class Account
    {
        public string AccountId { get; private set; } = string.Empty;
        public string ClientId { get; private set; } = string.Empty;
        public decimal Balance { get; private set; }
        public decimal ReservedBalance { get; private set; }
        public decimal CreditLimit { get; private set; }
        public StatusAccount Status { get; private set; }
        public List<Transaction> Transactions { get; private set; } = new();

        private Account() { }

        public Account(string accountId, string clientId, decimal initialBalance, decimal creditLimit)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentException(ResourceMessagesException.ER_003, nameof(accountId));

            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException(ResourceMessagesException.ER_004, nameof(clientId));

            if (initialBalance < 0)
                throw new ArgumentException(ResourceMessagesException.ER_005, nameof(initialBalance));

            if (creditLimit < 0)
                throw new ArgumentException(ResourceMessagesException.ER_006, nameof(creditLimit));

            AccountId = accountId;
            ClientId = clientId;
            Balance = initialBalance;
            ReservedBalance = 0;
            CreditLimit = creditLimit;
            Status = StatusAccount.Active;
        }

        public decimal AvailableBalance => Balance + CreditLimit;

        public void Credit(decimal amount, string transactionId, string referenceId, string? currency = "BRL")
        {
            if (amount <= 0)
                throw new InvalidOperationException(ResourceMessagesException.ER_007);

            if (Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_008);

            Balance += amount;

            var transaction = new Transaction(
                transactionId,
                AccountId,
                OperationTransaction.Credit,
                amount,
                currency ?? "BRL",
                referenceId
            );

            Transactions.Add(transaction);
        }

        public void Debit(decimal amount, string transactionId, string referenceId, string? currency = "BRL")
        {
            if (amount <= 0)
                throw new InvalidOperationException(ResourceMessagesException.ER_009);

            if (Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_008);

            if (AvailableBalance < amount)
                throw new InvalidOperationException(ResourceMessagesException.ER_010);

            Balance -= amount;

            var transaction = new Transaction(
                transactionId,
                AccountId,
                OperationTransaction.Debit,
                amount,
                currency ?? "BRL",
                referenceId
            );

            Transactions.Add(transaction);
        }

        public void Reserve(decimal amount, string transactionId, string referenceId, string? currency = "BRL")
        {
            if (amount <= 0)
                throw new InvalidOperationException(ResourceMessagesException.ER_011);

            if (Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_008);

            if (Balance < amount)
                throw new InvalidOperationException(ResourceMessagesException.ER_012);

            Balance -= amount;
            ReservedBalance += amount;

            var transaction = new Transaction(
                transactionId,
                AccountId,
                OperationTransaction.Reserve,
                amount,
                currency ?? "BRL",
                referenceId
            );

            Transactions.Add(transaction);
        }

        public void Capture(decimal amount, string transactionId, string referenceId, string? currency = "BRL")
        {
            if (amount <= 0)
                throw new InvalidOperationException(ResourceMessagesException.ER_013);

            if (Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_008);

            if (ReservedBalance < amount)
                throw new InvalidOperationException(ResourceMessagesException.ER_014);

            ReservedBalance -= amount;

            var transaction = new Transaction(
                transactionId,
                AccountId,
                OperationTransaction.Capture,
                amount,
                currency ?? "BRL",
                referenceId
            );

            Transactions.Add(transaction);
        }

        public void Reverse(string originalReferenceId, string newTransactionId, string newReferenceId, string? currency = "BRL")
        {
            var originalTransaction = Transactions
                .FirstOrDefault(t => t.ReferenceId == originalReferenceId && t.Status == StatusTransaction.Success);

            if (originalTransaction == null)
                throw new InvalidOperationException($"Transação original não encontrada: {originalReferenceId}");

            if (Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_008);

            decimal amount = originalTransaction.Amount;

            switch (originalTransaction.Operation)
            {
                case OperationTransaction.Credit:
                    if (Balance < amount)
                        throw new InvalidOperationException(ResourceMessagesException.ER_015);
                    Balance -= amount;
                    break;

                case OperationTransaction.Debit:
                    Balance += amount;
                    break;

                case OperationTransaction.Reserve:
                    Balance += amount;
                    ReservedBalance -= amount;
                    break;

                case OperationTransaction.Capture:
                    ReservedBalance += amount;
                    break;

                case OperationTransaction.Transfer:
                    break;

                default:
                    throw new InvalidOperationException($"Operação não pode ser revertida: {originalTransaction.Operation}");
            }

            var transaction = new Transaction(
                newTransactionId,
                AccountId,
                OperationTransaction.Reversal,
                amount,
                currency ?? "BRL",
                newReferenceId
            );

            Transactions.Add(transaction);
        }

        public void TransferTo(Account destinationAccount, decimal amount, string transactionId, string referenceId, string? currency = "BRL")
        {
            ArgumentNullException.ThrowIfNull(destinationAccount);

            if (amount <= 0)
                throw new InvalidOperationException(ResourceMessagesException.ER_016);

            if (Status != StatusAccount.Active || destinationAccount.Status != StatusAccount.Active)
                throw new InvalidOperationException(ResourceMessagesException.ER_017);

            if (AvailableBalance < amount)
                throw new InvalidOperationException(ResourceMessagesException.ER_018);

            Balance -= amount;
            destinationAccount.Balance += amount;

            var sourceTransaction = new Transaction(
                transactionId,
                AccountId,
                OperationTransaction.Transfer,
                amount,
                currency ?? "BRL",
                referenceId
            );

            var destinationTransaction = new Transaction(
                $"{transactionId}-DST",
                destinationAccount.AccountId,
                OperationTransaction.Transfer,
                amount,
                currency ?? "BRL",
                $"{referenceId}-DST"
            );

            Transactions.Add(sourceTransaction);
            destinationAccount.Transactions.Add(destinationTransaction);
        }

        public void Block()
        {
            Status = StatusAccount.Blocked;
        }

        public void Activate()
        {
            Status = StatusAccount.Active;
        }

        public void Deactivate()
        {
            Status = StatusAccount.Inactive;
        }
    }
}

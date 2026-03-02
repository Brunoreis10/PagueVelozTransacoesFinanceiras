using System.Transactions;
using TransacoesFinanceiras.Domain.Enums;
using TransacoesFinanceiras.Exceptions.Exceptions;

namespace TransacoesFinanceiras.Domain.Entity
{
    public class Transaction
    {
        public string TransactionId { get; private set; } = string.Empty;
        public string AccountId { get; private set; } = string.Empty;
        public OperationTransaction Operation { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "BRL";
        public string ReferenceId { get; private set; } = string.Empty;
        public StatusTransaction Status { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string? ErrorMessage { get; private set; }

        private Transaction() { }

        public Transaction(
            string transactionId,
            string accountId,
            OperationTransaction operation,
            decimal amount,
            string currency,
            string referenceId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException(ResourceMessagesException.ER_021, nameof(transactionId));

            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentException(ResourceMessagesException.ER_003, nameof(accountId));

            if (amount <= 0)
                throw new ArgumentException(ResourceMessagesException.ER_020, nameof(amount));

            if (string.IsNullOrWhiteSpace(referenceId))
                throw new ArgumentException(ResourceMessagesException.ER_022, nameof(referenceId));

            TransactionId = transactionId;
            AccountId = accountId;
            Operation = operation;
            Amount = amount;
            Currency = currency;
            ReferenceId = referenceId;
            Status = StatusTransaction.Success;
            Timestamp = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = StatusTransaction.Failed;
            ErrorMessage = errorMessage;
        }

        public void MarkAsPending()
        {
            Status = StatusTransaction.Pending;
        }
    }
}

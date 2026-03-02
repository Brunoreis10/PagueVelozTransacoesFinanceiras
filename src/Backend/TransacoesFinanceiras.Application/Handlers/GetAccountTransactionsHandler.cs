using MediatR;
using Microsoft.Extensions.Logging;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.Querys;
using TransacoesFinanceiras.Domain.Repository;

namespace TransacoesFinanceiras.Application.Handlers
{
    public class GetAccountTransactionsHandler : IRequestHandler<GetAccountTransactionsQuery, List<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<GetAccountTransactionsHandler> _logger;

        public GetAccountTransactionsHandler(
            ITransactionRepository transactionRepository,
            ILogger<GetAccountTransactionsHandler> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<List<TransactionDto>> Handle(GetAccountTransactionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Buscando transações da conta {AccountId}", request.AccountId);

            var transactions = await _transactionRepository.GetByAccountIdAsync(request.AccountId, cancellationToken);

            return transactions.Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                AccountId = t.AccountId,
                Operation = t.Operation,
                Amount = t.Amount,
                Currency = t.Currency,
                ReferenceId = t.ReferenceId,
                Status = t.Status,
                Timestamp = t.Timestamp,
                ErrorMessage = t.ErrorMessage
            }).ToList();
        }
    }
}

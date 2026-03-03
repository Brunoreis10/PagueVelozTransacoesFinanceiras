using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using TransacoesFinanceiras.Application.Commands;
using TransacoesFinanceiras.Application.DTOs.Responses;
using TransacoesFinanceiras.Domain.Enums;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Exceptions.Exceptions;

namespace TransacoesFinanceiras.Application.Handlers
{
    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponseDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<CreateTransactionCommandHandler> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public CreateTransactionCommandHandler(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ILogger<CreateTransactionCommandHandler> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<InvalidOperationException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Tentativa {RetryCount} após {TimeSpan}s. Erro: {Error}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception.Message);
                    });
        }

        public async Task<TransactionResponseDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Processando transação {Operation} para conta {AccountId} com reference {ReferenceId}",
                request.Dto.Operation,
                request.Dto.AccountId,
                request.Dto.ReferenceId);

            var existingTransaction = await _transactionRepository.GetByReferenceIdAsync(request.Dto.ReferenceId, cancellationToken);
            if (existingTransaction != null)
            {
                _logger.LogInformation("Transação com reference {ReferenceId} já existe. Retornando resultado existente.", request.Dto.ReferenceId);

                var account = await _accountRepository.GetByIdAsync(existingTransaction.AccountId, cancellationToken) ?? throw new InvalidOperationException($"Conta {existingTransaction.AccountId} não encontrada");
                return new TransactionResponseDto
                {
                    TransactionId = existingTransaction.TransactionId,
                    Status = existingTransaction.Status.ToString().ToLower(),
                    Balance = account.Balance,
                    ReservedBalance = account.ReservedBalance,
                    AvailableBalance = account.AvailableBalance,
                    Timestamp = existingTransaction.Timestamp,
                    ErrorMessage = existingTransaction.ErrorMessage
                };
            }

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var account = await _accountRepository.GetByIdAsync(request.Dto.AccountId, cancellationToken) ?? throw new InvalidOperationException($"Conta {request.Dto.AccountId} não encontrada");
                var processedTransactionId = $"{request.Dto.ReferenceId}-PROCESSED";
                try
                {
                    switch (request.Dto.Operation)
                    {
                        case OperationTransaction.Credit:
                            account.Credit(request.Dto.Amount, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            break;

                        case OperationTransaction.Debit:
                            account.Debit(request.Dto.Amount, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            break;

                        case OperationTransaction.Reserve:
                            account.Reserve(request.Dto.Amount, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            break;

                        case OperationTransaction.Capture:
                            account.Capture(request.Dto.Amount, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            break;

                        case OperationTransaction.Reversal:
                            if (string.IsNullOrWhiteSpace(request.Dto.OriginalReferenceId))
                                throw new InvalidOperationException(ResourceMessagesException.ER_001);
                            account.Reverse(request.Dto.OriginalReferenceId, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            break;

                        case OperationTransaction.Transfer:
                            if (string.IsNullOrWhiteSpace(request.Dto.DestinationAccountId))
                                throw new InvalidOperationException(ResourceMessagesException.ER_002);

                            var destinationAccount = await _accountRepository.GetByIdAsync(request.Dto.DestinationAccountId, cancellationToken);
                            if (destinationAccount == null)
                                throw new InvalidOperationException($"Conta de destino {request.Dto.DestinationAccountId} não encontrada");

                            account.TransferTo(destinationAccount, request.Dto.Amount, processedTransactionId, request.Dto.ReferenceId, request.Dto.Currency);
                            await _accountRepository.UpdateAsync(destinationAccount, cancellationToken);
                            break;

                        default:
                            throw new InvalidOperationException($"Operação não suportada: {request.Dto.Operation}");
                    }

                    var transaction = account.Transactions.Last();
                    await _transactionRepository.AddAsync(transaction, cancellationToken);
                    await _accountRepository.UpdateAsync(account, cancellationToken);

                    _logger.LogInformation(
                        "Transação {TransactionId} processada com sucesso. Status: {Status}",
                        transaction.TransactionId,
                        transaction.Status);

                    return new TransactionResponseDto
                    {
                        TransactionId = transaction.TransactionId,
                        Status = transaction.Status.ToString().ToLower(),
                        Balance = account.Balance,
                        ReservedBalance = account.ReservedBalance,
                        AvailableBalance = account.AvailableBalance,
                        Timestamp = transaction.Timestamp,
                        ErrorMessage = transaction.ErrorMessage
                    };
                }
                catch (InvalidOperationException ex) when (IsBusinessRuleViolation(ex))
                {
                    _logger.LogWarning("Regra de negócio violada para {ReferenceId}: {Error}", request.Dto.ReferenceId, ex.Message);

                    var failedTransaction = new Domain.Entity.Transaction(
                        processedTransactionId,
                        request.Dto.AccountId,
                        request.Dto.Operation,
                        request.Dto.Amount,
                        request.Dto.Currency,
                        request.Dto.ReferenceId
                    );
                    failedTransaction.MarkAsFailed(ex.Message);

                    await _transactionRepository.AddAsync(failedTransaction, cancellationToken);

                    return new TransactionResponseDto
                    {
                        TransactionId = failedTransaction.TransactionId,
                        Status = "failed",
                        Balance = account.Balance,
                        ReservedBalance = account.ReservedBalance,
                        AvailableBalance = account.AvailableBalance,
                        Timestamp = failedTransaction.Timestamp,
                        ErrorMessage = ex.Message
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar transação {ReferenceId}", request.Dto.ReferenceId);
                    throw;
                }
            });
        }

        private static bool IsBusinessRuleViolation(InvalidOperationException ex)
        {
            // Erros de regra de negócio que devem retornar "failed" sem propagar exceção
            var businessErrors = new[]
            {
                ResourceMessagesException.ER_010, // saldo insuficiente debit
                ResourceMessagesException.ER_012, // saldo insuficiente reserve
                ResourceMessagesException.ER_014, // saldo reservado insuficiente capture
                ResourceMessagesException.ER_015, // saldo insuficiente reversal credit
                ResourceMessagesException.ER_018, // saldo insuficiente transfer
                ResourceMessagesException.ER_008, // conta inativa/bloqueada
            };
            return businessErrors.Contains(ex.Message);
        }
    }
}

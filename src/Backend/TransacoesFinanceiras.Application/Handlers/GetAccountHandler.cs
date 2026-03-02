using MediatR;
using Microsoft.Extensions.Logging;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.Querys;
using TransacoesFinanceiras.Domain.Repository;

namespace TransacoesFinanceiras.Application.Handlers
{
    public class GetAccountHandler : IRequestHandler<GetAccountQuery, AccountDto?>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<GetAccountHandler> _logger;

        public GetAccountHandler(
            IAccountRepository accountRepository,
            ILogger<GetAccountHandler> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<AccountDto?> Handle(GetAccountQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Buscando conta {AccountId}", request.AccountId);

            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                _logger.LogWarning("Conta {AccountId} não encontrada", request.AccountId);
                return null;
            }

            return new AccountDto
            {
                AccountId = account.AccountId,
                ClientId = account.ClientId,
                Balance = account.Balance,
                ReservedBalance = account.ReservedBalance,
                CreditLimit = account.CreditLimit,
                AvailableBalance = account.AvailableBalance,
                Status = account.Status
            };
        }
    }
}

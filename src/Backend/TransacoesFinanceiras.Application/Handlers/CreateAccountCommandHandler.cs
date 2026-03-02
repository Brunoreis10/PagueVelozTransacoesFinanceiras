using MediatR;
using Microsoft.Extensions.Logging;
using TransacoesFinanceiras.Application.Commands;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Repository;

namespace TransacoesFinanceiras.Application.Handlers
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(
            IAccountRepository accountRepository,
            IClientRepository clientRepository,
            ILogger<CreateAccountCommandHandler> logger)
        {
            _accountRepository = accountRepository;
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Criando conta para cliente {ClientId}", request.Dto.ClientId);

            var client = await _clientRepository.GetByIdAsync(request.Dto.ClientId, cancellationToken);
            if (client == null)
            {
                client = new Client(request.Dto.ClientId, $"Cliente {request.Dto.ClientId}");
                await _clientRepository.AddAsync(client, cancellationToken);
            }

            var accountId = $"ACC-{Guid.NewGuid():N}"[..12].ToUpper();
            var account = new Account(
                accountId,
                request.Dto.ClientId,
                request.Dto.InitialBalance,
                request.Dto.CreditLimit
            );

            await _accountRepository.AddAsync(account, cancellationToken);
            client.AddAccount(account);
            await _clientRepository.UpdateAsync(client, cancellationToken);

            _logger.LogInformation("Conta {AccountId} criada com sucesso", accountId);

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

using MediatR;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.DTOs.Responses;

namespace TransacoesFinanceiras.Application.Commands
{
    public record CreateAccountCommand(CreateAccountDto Dto) : IRequest<AccountDto>;
    public record CreateTransactionCommand(CreateTransactionDto Dto) : IRequest<TransactionResponseDto>;
}

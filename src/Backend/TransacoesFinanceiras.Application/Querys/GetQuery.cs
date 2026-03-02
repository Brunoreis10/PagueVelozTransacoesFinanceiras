using MediatR;
using TransacoesFinanceiras.Application.DTOs;

namespace TransacoesFinanceiras.Application.Querys
{
    public record GetAccountQuery(string AccountId) : IRequest<AccountDto?>;

    public record GetAccountTransactionsQuery(string AccountId) : IRequest<List<TransactionDto>>;
}

using TransacoesFinanceiras.Domain.Enums;

namespace TransacoesFinanceiras.Application.DTOs
{
    public record AccountDto
    {
        public string AccountId { get; init; } = string.Empty;
        public string ClientId { get; init; } = string.Empty;
        public decimal Balance { get; init; }
        public decimal ReservedBalance { get; init; }
        public decimal CreditLimit { get; init; }
        public decimal AvailableBalance { get; init; }
        public StatusAccount Status { get; init; }
    }
}

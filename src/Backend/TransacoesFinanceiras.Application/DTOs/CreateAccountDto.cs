namespace TransacoesFinanceiras.Application.DTOs
{
    public record CreateAccountDto
    {
        public string ClientId { get; init; } = string.Empty;
        public decimal InitialBalance { get; init; }
        public decimal CreditLimit { get; init; }
    }
}

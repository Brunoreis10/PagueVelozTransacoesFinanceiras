using TransacoesFinanceiras.Domain.Enums;

namespace TransacoesFinanceiras.Application.DTOs
{
    public record CreateTransactionDto
    {
        public OperationTransaction Operation { get; init; }
        public string AccountId { get; init; } = string.Empty;
        public string? DestinationAccountId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = "BRL";
        public string ReferenceId { get; init; } = string.Empty;
        public string? OriginalReferenceId { get; init; }
        public Dictionary<string, object>? Metadata { get; init; }
    }
}

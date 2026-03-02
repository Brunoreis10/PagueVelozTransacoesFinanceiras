using TransacoesFinanceiras.Domain.Enums;

namespace TransacoesFinanceiras.Application.DTOs
{
    public record TransactionDto
    {
        public string TransactionId { get; init; } = string.Empty;
        public string AccountId { get; init; } = string.Empty;
        public OperationTransaction Operation { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string ReferenceId { get; init; } = string.Empty;
        public StatusTransaction Status { get; init; }
        public DateTime Timestamp { get; init; }
        public string? ErrorMessage { get; init; }
    }
}

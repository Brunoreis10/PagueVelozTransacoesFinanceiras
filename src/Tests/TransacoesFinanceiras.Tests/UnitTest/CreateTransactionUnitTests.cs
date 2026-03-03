using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransacoesFinanceiras.Application.Commands;
using TransacoesFinanceiras.Application.DTOs;
using TransacoesFinanceiras.Application.Handlers;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Enums;
using TransacoesFinanceiras.Domain.Repository;

namespace TransacoesFinanceiras.Tests.UnitTest
{
    public class CreateTransactionUnitTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock;
        private readonly CreateTransactionCommandHandler _handler;

        public CreateTransactionUnitTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _loggerMock = new Mock<ILogger<CreateTransactionCommandHandler>>();

            _handler = new CreateTransactionCommandHandler(
                _accountRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _loggerMock.Object);
        }

        private CreateTransactionCommand BuildCommand(
            string accountId,
            string referenceId,
            decimal amount,
            OperationTransaction operation,
            string? destinationAccountId = null,
            string? originalReferenceId = null) =>
            new(new CreateTransactionDto
            {
                AccountId = accountId,
                ReferenceId = referenceId,
                Amount = amount,
                Operation = operation,
                Currency = "BRL",
                DestinationAccountId = destinationAccountId,
                OriginalReferenceId = originalReferenceId
            });

        [Fact]
        public async Task Handle_Credit_WhenAccountExists_ShouldReturnUpdatedBalance()
        {
            // Arrange
            var account = new Account("ACC-001", "CLI-001", initialBalance: 500m, creditLimit: 0m);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-001", It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-001", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-001", "REF-001", 300m, OperationTransaction.Credit);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Balance.Should().Be(800m);
            result.Status.Should().Be("success");
            _accountRepositoryMock.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Debit_WhenAccountExists_ShouldReturnDeductedBalance()
        {
            // Arrange
            var account = new Account("ACC-002", "CLI-002", initialBalance: 1000m, creditLimit: 0m);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-002", It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-002", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-002", "REF-002", 400m, OperationTransaction.Debit);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Balance.Should().Be(600m);
            result.Status.Should().Be("success");
        }

        [Fact]
        public async Task Handle_Reserve_WhenApplied_ShouldReflectReservedBalance()
        {
            // Arrange
            var account = new Account("ACC-003", "CLI-003", initialBalance: 1000m, creditLimit: 0m);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-003", It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-003", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-003", "REF-003", 300m, OperationTransaction.Reserve);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Balance.Should().Be(700m);
            result.ReservedBalance.Should().Be(300m);
        }

        [Fact]
        public async Task Handle_Transfer_WhenDestinationAccountExists_ShouldTransferCorrectly()
        {
            // Arrange
            var origin = new Account("ACC-004", "CLI-004", initialBalance: 1000m, creditLimit: 0m);
            var destination = new Account("ACC-005", "CLI-005", initialBalance: 200m, creditLimit: 0m);

            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-004", It.IsAny<CancellationToken>())).ReturnsAsync(origin);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-005", It.IsAny<CancellationToken>())).ReturnsAsync(destination);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-004", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-004", "REF-004", 500m, OperationTransaction.Transfer, destinationAccountId: "ACC-005");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Balance.Should().Be(500m);
            _accountRepositoryMock.Verify(r => r.UpdateAsync(destination, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Transfer_WhenDestinationAccountMissing_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var origin = new Account("ACC-006", "CLI-006", initialBalance: 1000m, creditLimit: 0m);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-006", It.IsAny<CancellationToken>())).ReturnsAsync(origin);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-999", It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-005", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-006", "REF-005", 100m, OperationTransaction.Transfer, destinationAccountId: "ACC-999");

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*ACC-999*");
        }

        [Fact]
        public async Task Handle_Reversal_WhenOriginalReferenceIdMissing_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var account = new Account("ACC-007", "CLI-007", initialBalance: 1000m, creditLimit: 0m);
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-007", It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-006", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-007", "REF-006", 100m, OperationTransaction.Reversal, originalReferenceId: null);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_Idempotency_WhenTransactionAlreadyExists_ShouldReturnExistingResult()
        {
            // Arrange
            var existingAccount = new Account("ACC-008", "CLI-008", initialBalance: 700m, creditLimit: 0m);
            existingAccount.Credit(200m, "TXN-001-PROCESSED", "REF-DUP-001");
            var existingTransaction = existingAccount.Transactions.Last();

            _transactionRepositoryMock
                .Setup(r => r.GetByReferenceIdAsync("REF-DUP-001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTransaction);

            _accountRepositoryMock
                .Setup(r => r.GetByIdAsync(existingTransaction.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingAccount);

            var command = BuildCommand("ACC-008", "REF-DUP-001", 200m, OperationTransaction.Credit);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.TransactionId.Should().Be(existingTransaction.TransactionId);
            _accountRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenAccountNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _accountRepositoryMock.Setup(r => r.GetByIdAsync("ACC-GHOST", It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("REF-007", It.IsAny<CancellationToken>())).ReturnsAsync((Transaction?)null);

            var command = BuildCommand("ACC-GHOST", "REF-007", 100m, OperationTransaction.Credit);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*ACC-GHOST*");
        }
    }
}

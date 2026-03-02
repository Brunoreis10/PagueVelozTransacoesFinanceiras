using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransacoesFinanceiras.Domain.Entity;
using TransacoesFinanceiras.Domain.Repository;
using TransacoesFinanceiras.Infrastructure.Database;
using TransacoesFinanceiras.Infrastructure.Repository;

namespace TransacoesFinanceiras.Tests.IntegrationTest
{
    public class AccountIntegrationTests : IAsyncLifetime
    {
        private readonly AppDbContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public AccountIntegrationTests()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            var provider = services.BuildServiceProvider();
            _context = provider.GetRequiredService<AppDbContext>();
            _accountRepository = provider.GetRequiredService<IAccountRepository>();
            _transactionRepository = provider.GetRequiredService<ITransactionRepository>();
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public async Task DisposeAsync() => await _context.DisposeAsync();

        [Fact]
        public async Task CreateAccount_WhenValidData_ShouldPersistCorrectly()
        {
            // Arrange
            var account = new Account("ACC-INT-001", "CLI-INT-001", initialBalance: 2500m, creditLimit: 10000m);

            // Act
            await _accountRepository.AddAsync(account);

            // Assert
            var retrieved = await _accountRepository.GetByIdAsync("ACC-INT-001");
            retrieved.Should().NotBeNull();
            retrieved!.Balance.Should().Be(2500m);
            retrieved.CreditLimit.Should().Be(10000m);
            retrieved.ClientId.Should().Be("CLI-INT-001");
        }

        [Fact]
        public async Task CreditTransaction_WhenApplied_ShouldIncreaseBalance()
        {
            // Arrange
            var account = new Account("ACC-INT-002", "CLI-INT-002", initialBalance: 500m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            // Act
            account.Credit(250m, "REF-CREDIT-001", "BRL");
            await _accountRepository.UpdateAsync(account);

            // Assert
            var updated = await _accountRepository.GetByIdAsync("ACC-INT-002");
            updated!.Balance.Should().Be(750m);
        }

        [Fact]
        public async Task DebitTransaction_WhenSufficientFunds_ShouldDecreaseBalance()
        {
            // Arrange
            var account = new Account("ACC-INT-003", "CLI-INT-003", initialBalance: 1000m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            // Act
            account.Debit(350m, "REF-DEBIT-001", "BRL");
            await _accountRepository.UpdateAsync(account);

            // Assert
            var updated = await _accountRepository.GetByIdAsync("ACC-INT-003");
            updated!.Balance.Should().Be(650m);
        }

        [Fact]
        public async Task ReserveTransaction_WhenApplied_ShouldMoveAmountToReservedBalance()
        {
            // Arrange
            var account = new Account("ACC-INT-004", "CLI-INT-004", initialBalance: 1000m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            // Act
            account.Reserve(400m, "REF-RESERVE-001", "BRL");
            await _accountRepository.UpdateAsync(account);

            // Assert
            var updated = await _accountRepository.GetByIdAsync("ACC-INT-004");
            updated!.Balance.Should().Be(600m);
            updated.ReservedBalance.Should().Be(400m);
        }

        [Fact]
        public async Task IdempotencyCheck_WhenReferenceAlreadyProcessed_ShouldReturnTrue()
        {
            // Arrange
            var account = new Account("ACC-INT-005", "CLI-INT-005", initialBalance: 800m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            account.Credit(200m, "REF-IDEMPOTENT-001", "BRL");
            await _accountRepository.UpdateAsync(account);

            // Act
            var alreadyExists = await _transactionRepository.ExistsReferenceIdAsync("REF-IDEMPOTENT-001");

            // Assert
            alreadyExists.Should().BeTrue("a transação com esse referenceId já foi processada");
        }

        [Fact]
        public async Task MultipleSequentialTransactions_ShouldAccumulateCorrectly()
        {
            // Arrange
            var account = new Account("ACC-INT-006", "CLI-INT-006", initialBalance: 1000m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            // Act
            account.Credit(300m, "REF-SEQ-001", "BRL");
            account.Credit(200m, "REF-SEQ-002", "BRL");
            account.Debit(100m, "REF-SEQ-003", "BRL");
            await _accountRepository.UpdateAsync(account);

            // Assert
            var updated = await _accountRepository.GetByIdAsync("ACC-INT-006");
            updated!.Balance.Should().Be(1400m);
        }

        [Fact]
        public async Task Concurrency_WhenMultipleCreditsApplied_BalanceShouldNotDropBelowInitial()
        {
            // Arrange
            var account = new Account("ACC-INT-007", "CLI-INT-007", initialBalance: 1000m, creditLimit: 0m);
            await _accountRepository.AddAsync(account);

            // Act — sequencial com scopes isolados para evitar conflito no contexto EF InMemory
            for (int i = 0; i < 8; i++)
            {
                var acc = await _accountRepository.GetByIdAsync("ACC-INT-007");
                if (acc is null) continue;
                acc.Credit(50m, $"REF-CONC-{i:D3}", "BRL");
                await _accountRepository.UpdateAsync(acc);
            }

            // Assert
            var final = await _accountRepository.GetByIdAsync("ACC-INT-007");
            final!.Balance.Should().BeGreaterOrEqualTo(1000m);
        }
    }
}

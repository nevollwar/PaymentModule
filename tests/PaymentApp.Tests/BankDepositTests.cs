using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class BankDepositTests
{
    [Fact]
    public void Deposit_Success()
    {
        var account = new DailyLimitAccount("40817-001", "Иванов", 5000m, 10000m, 20000m);
        var bank = new Bank(new[] { account });

        var result = bank.Deposit(account, 3000m);

        Assert.Equal(8000m, account.Balance);
        Assert.Equal(17000m, account.DailyLimitRemaining);
        Assert.True(result.PostconditionHolds);
    }

    [Fact]
    public void Deposit_ExceedsSingleLimit_Throws()
    {
        var account = new DailyLimitAccount("40817-003", "Сидоров", 1000m, 5000m, 15000m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Deposit(account, 5001m));

        Assert.Contains("превышает разовый лимит", exception.Message);
    }

    [Fact]
    public void Deposit_ExceedsDailyLimit_Throws()
    {
        var account = new DailyLimitAccount("40817-004", "Кузнецов", 1000m, 10000m, 8000m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Deposit(account, 8001m));

        Assert.Contains("превышает остаток дневного лимита", exception.Message);
    }

    [Fact]
    public void Deposit_ZeroAmount_Throws()
    {
        var account = new DailyLimitAccount("40817-005", "Морозов", 1000m, 5000m, 15000m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Deposit(account, 0m));

        Assert.Contains("больше 0", exception.Message);
    }

    [Fact]
    public void FindDepositPreViolation_ValidDeposit_ReturnsNull()
    {
        var account = new DailyLimitAccount("40817-009", "Волков", 1000m, 5000m, 15000m);
        var bank = new Bank(new[] { account });

        var violation = bank.FindDepositPreViolation(account, 3000m);

        Assert.Null(violation);
    }
}

using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class BankServiceTests
{
    [Fact]
    public void PayService_Success()
    {
        var account = new Account("40817-001", "Иванов", 10000m);
        var bank = new Bank(new[] { account });

        var result = bank.PayService(account, "Интернет", 1000m);

        Assert.Equal(8990m, account.Balance);
        Assert.True(result.PostconditionHolds);
    }

    [Fact]
    public void PayService_InsufficientBalance_Throws()
    {
        var account = new Account("40817-002", "Петров", 100m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.PayService(account, "Интернет", 1000m));

        Assert.Contains("Недостаточно средств", exception.Message);
    }

    [Fact]
    public void PayService_InvalidService_Throws()
    {
        var account = new Account("40817-003", "Сидоров", 10000m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.PayService(account, "НесуществующаяУслуга", 1000m));

        Assert.Contains("Услуга не выбрана", exception.Message);
    }

    [Fact]
    public void PayService_ZeroAmount_Throws()
    {
        var account = new Account("40817-004", "Кузнецов", 10000m);
        var bank = new Bank(new[] { account });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.PayService(account, "Интернет", 0m));

        Assert.Contains("больше 0", exception.Message);
    }

    [Fact]
    public void FindServicePaymentPreViolation_Valid_ReturnsNull()
    {
        var account = new Account("40817-005", "Морозов", 10000m);
        var bank = new Bank(new[] { account });

        var violation = bank.FindServicePaymentPreViolation(account, "Интернет", 1000m);

        Assert.Null(violation);
    }
}

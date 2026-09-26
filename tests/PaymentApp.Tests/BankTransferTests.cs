using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class BankTransferTests
{
    [Fact]
    public void Transfer_Success()
    {
        var acc1 = new Account("40817-001", "Иванов", 10000m);
        var acc2 = new Account("40817-002", "Петров", 5000m);
        var bank = new Bank(new[] { acc1, acc2 });

        var result = bank.Transfer(acc1, acc2, 3000m);

        Assert.Equal(7000m, acc1.Balance);
        Assert.Equal(8000m, acc2.Balance);
        Assert.True(result.PostconditionHolds);
    }

    [Fact]
    public void Transfer_InsufficientFunds_Throws()
    {
        var acc1 = new Account("40817-003", "Сидоров", 1000m);
        var acc2 = new Account("40817-004", "Кузнецов", 5000m);
        var bank = new Bank(new[] { acc1, acc2 });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Transfer(acc1, acc2, 2000m));

        Assert.Contains("Недостаточно средств", exception.Message);
    }

    [Fact]
    public void Transfer_SameAccount_Throws()
    {
        var acc1 = new Account("40817-005", "Морозов", 10000m);
        var bank = new Bank(new[] { acc1 });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Transfer(acc1, acc1, 1000m));

        Assert.Contains("совпадает", exception.Message);
    }

    [Fact]
    public void Transfer_ZeroAmount_Throws()
    {
        var acc1 = new Account("40817-006", "Новиков", 10000m);
        var acc2 = new Account("40817-007", "Смирнов", 5000m);
        var bank = new Bank(new[] { acc1, acc2 });

        var exception = Assert.Throws<PreconditionException>(
            () => bank.Transfer(acc1, acc2, 0m));

        Assert.Contains("больше 0", exception.Message);
    }

    [Fact]
    public void FindTransferPreViolation_Valid_ReturnsNull()
    {
        var acc1 = new Account("40817-008", "Попов", 10000m);
        var acc2 = new Account("40817-009", "Волков", 5000m);
        var bank = new Bank(new[] { acc1, acc2 });

        var violation = bank.FindTransferPreViolation(acc1, acc2, 3000m);

        Assert.Null(violation);
    }
}

using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class AccountTests
{
    [Fact]
    public void CreateAccount_Success()
    {
        var account = new Account("40817-001", "Иванов", 10000m);

        Assert.Equal("40817-001", account.Number);
        Assert.Equal("Иванов", account.Owner);
        Assert.Equal(10000m, account.Balance);
    }

    [Fact]
    public void CreateAccount_WithZeroBalance_Success()
    {
        var account = new Account("40817-002", "Петров", 0m);

        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void CreateAccount_EmptyNumber_Throws()
    {
        var exception = Assert.Throws<PreconditionException>(
            () => new Account("", "Владелец", 1000m));
        
        Assert.Contains("Номер счёта не задан", exception.Message);
    }

    [Fact]
    public void CreateAccount_NegativeBalance_Throws()
    {
        var exception = Assert.Throws<PreconditionException>(
            () => new Account("40817-001", "Владелец", -100m));
        
        Assert.Contains("отрицательным", exception.Message);
    }
}

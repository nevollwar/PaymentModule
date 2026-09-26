using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class DailyLimitAccountTests
{
    [Fact]
    public void CreateDailyLimitAccount_Success()
    {
        var account = new DailyLimitAccount("40817-003", "Сидорова", 5000m, 10000m, 50000m);

        Assert.Equal(10000m, account.SingleDepositLimit);
        Assert.Equal(50000m, account.DailyLimitRemaining);
    }

    [Fact]
    public void CreateAccount_ZeroSingleLimit_Throws()
    {
        var exception = Assert.Throws<PreconditionException>(
            () => new DailyLimitAccount("40817-005", "Владелец", 1000m, 0m, 1000m));

        Assert.Contains("Разовый лимит", exception.Message);
    }

    [Fact]
    public void CreateAccount_NegativeDailyLimit_Throws()
    {
        var exception = Assert.Throws<PreconditionException>(
            () => new DailyLimitAccount("40817-007", "Владелец", 1000m, 5000m, -100m));

        Assert.Contains("отрицательным", exception.Message);
    }
}

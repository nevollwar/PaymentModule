using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class BankPreViolationTests
{
    [Fact]
    public void FindTransferPreViolation_AllBranches()
    {
        var a = new Account("A", "Иванов", 100m);
        var b = new Account("B", "Петров", 50m);
        var bank = new Bank(new[] { a, b });

        // null-счёт
        Assert.NotNull(bank.FindTransferPreViolation(null, b, 10m));
        Assert.NotNull(bank.FindTransferPreViolation(a, null, 10m));

        // чужой счёт
        var foreign = new Account("Z", "Чужой", 0m);
        Assert.NotNull(bank.FindTransferPreViolation(a, foreign, 10m));

        // сам себе
        Assert.NotNull(bank.FindTransferPreViolation(a, a, 10m));

        // невалидная сумма
        Assert.NotNull(bank.FindTransferPreViolation(a, b, 0m));
        Assert.NotNull(bank.FindTransferPreViolation(a, b, -1m));

        // недостаточно средств
        Assert.NotNull(bank.FindTransferPreViolation(a, b, 1000m));

        // всё хорошо
        Assert.Null(bank.FindTransferPreViolation(a, b, 50m));
    }

    [Fact]
    public void FindDepositPreViolation_AllBranches()
    {
        var acc = new DailyLimitAccount("A", "Иванов", 0m, 1000m, 5000m);
        var bank = new Bank(new[] { acc });

        // null
        Assert.NotNull(bank.FindDepositPreViolation(null, 100m));

        // чужой счёт
        var foreign = new DailyLimitAccount("Z", "Чужой", 0m, 1000m, 5000m);
        Assert.NotNull(bank.FindDepositPreViolation(foreign, 100m));

        // отрицательная сумма
        Assert.NotNull(bank.FindDepositPreViolation(acc, -1m));

        // превышение разового лимита
        Assert.NotNull(bank.FindDepositPreViolation(acc, 1001m));

        // всё хорошо
        Assert.Null(bank.FindDepositPreViolation(acc, 100m));
    }

    [Fact]
    public void FindServicePaymentPreViolation_AllBranches()
    {
        var a = new Account("A", "Иванов", 100m);
        var bank = new Bank(new[] { a });

        // null счёт
        Assert.NotNull(bank.FindServicePaymentPreViolation(null, "Интернет", 10m));

        // чужой счёт
        var foreign = new Account("Z", "Чужой", 100m);
        Assert.NotNull(bank.FindServicePaymentPreViolation(foreign, "Интернет", 10m));

        // услуга null / пустая / неизвестная
        Assert.NotNull(bank.FindServicePaymentPreViolation(a, null, 10m));
        Assert.NotNull(bank.FindServicePaymentPreViolation(a, "", 10m));
        Assert.NotNull(bank.FindServicePaymentPreViolation(a, "НетТакой", 10m));

        // невалидная сумма
        Assert.NotNull(bank.FindServicePaymentPreViolation(a, "Интернет", 0m));

        // недостаточно средств
        Assert.NotNull(bank.FindServicePaymentPreViolation(a, "Интернет", 1000m));

        // всё хорошо
        Assert.Null(bank.FindServicePaymentPreViolation(a, "Интернет", 10m));
    }

    [Fact]
    public void Bank_DuplicateAccountNumbers_Throws()
    {
        var a1 = new Account("SAME", "Иванов", 100m);
        var a2 = new Account("SAME", "Петров", 50m);
        Assert.Throws<PreconditionException>(() => new Bank(new[] { a1, a2 }));
    }
}
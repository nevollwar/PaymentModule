using PaymentApp.Domain;
using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.SQLite;

namespace PaymentApp.Tests;

public class PersistentBankTests
{
    private static SqliteDatabase SeedDb()
    {
        var db = new SqliteDatabase("Data Source=:memory:");
        db.Open();

        var acc = new SqliteAccountRepository(db);
        acc.Insert(new AccountEntity { Number = "40817-001", Owner = "Иванов", Balance = 10_000m });
        acc.Insert(new AccountEntity { Number = "40817-002", Owner = "Петров", Balance = 2_500m });

        return db;
    }

    [Fact]
    public void Transfer_SavesBalancesAndJournal()
    {
        using var db = SeedDb();
        var acc = new SqliteAccountRepository(db);
        var trx = new SqliteTransactionRepository(db);
        var bank = new PersistentBank(acc, trx);

        var from = bank.Accounts.Single(a => a.Number == "40817-001");
        var to = bank.Accounts.Single(a => a.Number == "40817-002");

        bank.Transfer(from, to, 1_000m);

        Assert.Equal(9_000m, acc.FindByNumber("40817-001")!.Balance);
        Assert.Equal(3_500m, acc.FindByNumber("40817-002")!.Balance);

        var journal = trx.FindAll();
        Assert.Single(journal);
        Assert.Equal(1_000m, journal[0].Amount);
    }

    [Fact]
    public void PayService_SavesJournalWithSupplier()
    {
        using var db = SeedDb();
        var acc = new SqliteAccountRepository(db);
        var trx = new SqliteTransactionRepository(db);
        var bank = new PersistentBank(acc, trx);

        var a = bank.Accounts.First(a => a.Number == "40817-001");
        bank.PayService(a, "Интернет", 1_000m);

        var journal = trx.FindAll();
        Assert.Single(journal);
        Assert.Equal("Поставщик: Интернет", journal[0].ToAccount);
        Assert.Equal(1_010m, journal[0].Amount);
    }

    [Fact]
    public void LoadAccounts_RestoresDailyLimit()
    {
        using var db = SeedDb();
        var acc = new SqliteAccountRepository(db);
        acc.Insert(new AccountEntity
        {
            Number = "D",
            Owner = "X",
            Balance = 0m,
            HasDailyLimit = true,
            SingleDepositLimit = 5_000m,
            DailyLimitRemaining = 15_000m
        });

        var bank = new PersistentBank(acc, new SqliteTransactionRepository(db));

        var restored = bank.Accounts.Single(a => a.Number == "D");
        Assert.IsType<DailyLimitAccount>(restored);
        Assert.Equal(5_000m, ((DailyLimitAccount)restored).SingleDepositLimit);
    }
}
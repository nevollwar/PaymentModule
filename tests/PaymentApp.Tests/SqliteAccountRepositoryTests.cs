using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.SQLite;

namespace PaymentApp.Tests;

public class SqliteAccountRepositoryTests
{
    private static (SqliteDatabase db, SqliteAccountRepository repo) New()
    {
        var db = new SqliteDatabase("Data Source=:memory:");
        db.Open();
        return (db, new SqliteAccountRepository(db));
    }

    private static AccountEntity Acc(string number = "A", decimal balance = 100m) => new()
    {
        Number = number,
        Owner = "Иванов",
        Balance = balance
    };

    [Fact]
    public void Insert_FindById_FindAll_RoundTrip()
    {
        var (db, repo) = New();
        using (db)
        {
            var saved = repo.Insert(Acc("A", 500m));
            var byId = repo.FindById(saved.Id);
            var all = repo.FindAll();

            Assert.NotNull(byId);
            Assert.Equal("A", byId!.Number);
            Assert.Equal(500m, byId.Balance);
            Assert.Single(all);
        }
    }

    [Fact]
    public void FindByNumber_Works_And_EmptyThrows()
    {
        var (db, repo) = New();
        using (db)
        {
            repo.Insert(Acc("ABC"));

            Assert.NotNull(repo.FindByNumber("ABC"));
            Assert.Null(repo.FindByNumber("NOPE"));
            Assert.Throws<ArgumentException>(() => repo.FindByNumber(""));
        }
    }

    [Fact]
    public void Update_ChangesBalance_Delete_Removes()
    {
        var (db, repo) = New();
        using (db)
        {
            var a = repo.Insert(Acc(balance: 100m));

            a.Balance = 999m;
            repo.Update(a);
            Assert.Equal(999m, repo.FindById(a.Id)!.Balance);

            Assert.True(repo.Delete(a.Id));
            Assert.Null(repo.FindById(a.Id));
            Assert.False(repo.Delete(9999));
        }
    }

    [Fact]
    public void Insert_DailyLimit_PersistsFields()
    {
        var (db, repo) = New();
        using (db)
        {
            var a = repo.Insert(new AccountEntity
            {
                Number = "D",
                Owner = "X",
                Balance = 0m,
                HasDailyLimit = true,
                SingleDepositLimit = 5000m,
                DailyLimitRemaining = 15000m
            });

            var fromDb = repo.FindById(a.Id)!;
            Assert.True(fromDb.HasDailyLimit);
            Assert.Equal(5000m, fromDb.SingleDepositLimit);
            Assert.Equal(15000m, fromDb.DailyLimitRemaining);
        }
    }

    [Fact]
    public void NullAndBadId_Throw()
    {
        var (db, repo) = New();
        using (db)
        {
            Assert.Throws<ArgumentNullException>(() => repo.Insert(null!));
            Assert.Throws<ArgumentNullException>(() => repo.Update(null!));
            Assert.Throws<ArgumentException>(() => repo.Update(Acc()));
        }
    }
}
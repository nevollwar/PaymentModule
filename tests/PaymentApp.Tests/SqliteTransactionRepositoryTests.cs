using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.SQLite;

namespace PaymentApp.Tests;

public class SqliteTransactionRepositoryTests
{
    private static (SqliteDatabase db, SqliteTransactionRepository repo) New()
    {
        var db = new SqliteDatabase("Data Source=:memory:");
        db.Open();
        return (db, new SqliteTransactionRepository(db));
    }

    private static TransactionEntity Trx(string from = "A", string to = "B", decimal amount = 10m) => new()
    {
        Timestamp = DateTime.UtcNow,
        FromAccount = from,
        ToAccount = to,
        Amount = amount
    };

    [Fact]
    public void Insert_FindById_FindAll()
    {
        var (db, repo) = New();
        using (db)
        {
            var t = repo.Insert(Trx(amount: 123.45m));
            var byId = repo.FindById(t.Id)!;

            Assert.Equal("A", byId.FromAccount);
            Assert.Equal(123.45m, byId.Amount);
            Assert.Single(repo.FindAll());
        }
    }

    [Fact]
    public void FindByAccountNumber_BothSides()
    {
        var (db, repo) = New();
        using (db)
        {
            repo.Insert(Trx("A", "B"));
            repo.Insert(Trx("C", "A"));
            repo.Insert(Trx("X", "Y"));

            Assert.Equal(2, repo.FindByAccountNumber("A").Count);
            Assert.Throws<ArgumentException>(() => repo.FindByAccountNumber(""));
        }
    }

    [Fact]
    public void Update_Delete()
    {
        var (db, repo) = New();
        using (db)
        {
            var t = repo.Insert(Trx(amount: 10m));

            t.Amount = 999m;
            repo.Update(t);
            Assert.Equal(999m, repo.FindById(t.Id)!.Amount);

            Assert.True(repo.Delete(t.Id));
            Assert.Null(repo.FindById(t.Id));
            Assert.False(repo.Delete(999));
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
            Assert.Throws<ArgumentException>(() => repo.Update(Trx()));
        }
    }
}
using PaymentApp.Domain.Database.SQLite;

namespace PaymentApp.Tests;

public class SqliteDatabaseTests
{
    private static SqliteDatabase NewDb()
    {
        var db = new SqliteDatabase("Data Source=:memory:");
        db.Open();
        return db;
    }

    [Fact]
    public void Ctor_EmptyDataSource_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SqliteDatabase(""));
    }

    [Fact]
    public void Execute_EmptyQuery_Throws()
    {
        using var db = NewDb();
        Assert.Throws<ArgumentException>(() => db.Execute(""));
    }

    [Fact]
    public void Insert_And_Query_RoundTrip()
    {
        using var db = NewDb();

        long id = db.Insert(
            "INSERT INTO accounts (number, owner, balance, has_daily_limit) VALUES ($0,$1,$2,$3)",
            "A", "B", "100", 0);

        Assert.Equal(1, id);

        var rows = db.Query("SELECT number FROM accounts WHERE id = $0", id).ToList();
        Assert.Single(rows);
        Assert.Equal("A", rows[0][0]);
    }

    [Fact]
    public void Execute_AfterDispose_Throws()
    {
        var db = NewDb();
        db.Dispose();

        Assert.Throws<NullReferenceException>(() => db.Execute("SELECT 1"));
    }
}
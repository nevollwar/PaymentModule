using PaymentApp.Domain;

namespace PaymentApp.Tests;

public class GuardTests
{
    [Fact]
    public void Requires_True_DoesNotThrow() => Guard.Requires(true, "ok");

    [Fact]
    public void Requires_False_Throws()
    {
        var ex = Assert.Throws<PreconditionException>(() => Guard.Requires(false, "boom"));
        Assert.Equal("boom", ex.Message);
    }
}
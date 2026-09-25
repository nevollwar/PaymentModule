using PaymentApp.Domain;

namespace PaymentApp.Tests
{
    public class TransferTests
    {
        private readonly Account ivanov = new Account("40817-001", "Иванов И.И.", 10_000m);
        private readonly Account petrov = new Account("40817-002", "Петров П.П.", 2_500m);
        private readonly Bank bank;

        public TransferTests()
        {
            bank = new Bank(new[] { ivanov, petrov });
        }

        [Fact]
        public void Transfer_WholeBalance_SourceBecomesZeroAndTotalPreserved()
        {
            decimal totalBefore = bank.TotalBalance;

            TransferResult result = bank.Transfer(ivanov, petrov, 10_000m);

            Assert.True(result.PostconditionHolds);
            Assert.Equal(0m, ivanov.Balance);
            Assert.Equal(12_500m, petrov.Balance);
            Assert.Equal(totalBefore, bank.TotalBalance);
            Assert.Single(bank.Journal);
        }

        [Fact]
        public void Transfer_MinimalAmount_OneKopeckMovedAndRecorded()
        {
            bank.Transfer(petrov, ivanov, 0.01m);

            Assert.Equal(2_499.99m, petrov.Balance);
            Assert.Equal(10_000.01m, ivanov.Balance);
            TransactionRecord record = Assert.Single(bank.Journal);
            Assert.Equal(("40817-002", "40817-001", 0.01m), (record.FromAccount, record.ToAccount, record.Amount));
        }

        [Fact]
        public void Transfer_AmountExceedsBalanceByOneKopeck_ThrowsAndStateUnchanged()
        {
            Assert.Null(bank.FindTransferPreViolation(petrov, ivanov, 2_500m));
            Assert.NotNull(bank.FindTransferPreViolation(petrov, ivanov, 2_500.01m));

            Assert.Throws<PreconditionException>(() => bank.Transfer(petrov, ivanov, 2_500.01m));

            Assert.Equal(2_500m, petrov.Balance);
            Assert.Equal(10_000m, ivanov.Balance);
            Assert.Empty(bank.Journal);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        public void Transfer_NonPositiveAmount_Throws(double amount)
        {
            Assert.Throws<PreconditionException>(() => bank.Transfer(ivanov, petrov, (decimal)amount));
            Assert.Empty(bank.Journal);
        }

        [Fact]
        public void Transfer_ToSameAccount_Throws()
        {
            Assert.Throws<PreconditionException>(() => bank.Transfer(ivanov, ivanov, 100m));
            Assert.Equal(10_000m, ivanov.Balance);
            Assert.Empty(bank.Journal);
        }
    }
}

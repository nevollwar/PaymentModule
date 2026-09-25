namespace PaymentApp.Domain
{
    /// <summary>
    /// Счёт с дневным лимитом пополнения.
    /// Хранит разовый лимит (максимум одного пополнения) и остаток дневного лимита.
    /// </summary>
    public class DailyLimitAccount : Account
    {
        /// <summary>Максимальная сумма одного пополнения.</summary>
        public decimal SingleDepositLimit { get; }

        /// <summary>Остаток дневного лимита пополнений (уменьшается после каждого успешного пополнения).</summary>
        public decimal DailyLimitRemaining { get; private set; }

        public DailyLimitAccount(
            string number,
            string owner,
            decimal initialBalance,
            decimal singleDepositLimit,
            decimal dailyLimitRemaining)
            : base(number, owner, initialBalance)
        {
            Guard.Requires(singleDepositLimit > 0, "Разовый лимит должен быть больше 0");
            Guard.Requires(dailyLimitRemaining >= 0, "Остаток дневного лимита не может быть отрицательным");
            Guard.Requires(dailyLimitRemaining <= singleDepositLimit * 100,
                "Начальный остаток дневного лимита выглядит некорректным");

            SingleDepositLimit = singleDepositLimit;
            DailyLimitRemaining = dailyLimitRemaining;
        }

        /// <summary>Уменьшает остаток дневного лимита после успешного пополнения.</summary>
        internal void ConsumeDailyLimit(decimal amount)
        {
            DailyLimitRemaining -= amount;
        }
    }
}

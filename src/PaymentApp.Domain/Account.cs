namespace PaymentApp.Domain
{
    /// <summary>
    /// Банковский счёт в платёжной системе
    /// </summary>
    public class Account
    {
        public string Number { get; }
        public string Owner { get; }
        public decimal Balance { get; private set; }

        public Account(string number, string owner, decimal initialBalance)
        {
            Guard.Requires(!string.IsNullOrWhiteSpace(number), "Номер счёта не задан");
            Guard.Requires(initialBalance >= 0, "Начальный баланс не может быть отрицательным");

            Number = number;
            Owner = owner;
            Balance = initialBalance;
        }

        // Изменять баланс могут только операции платёжной системы (Bank),
        // проверки контрактов выполняются там
        internal void Withdraw(decimal amount)
        {
            Balance -= amount;
        }

        internal void Deposit(decimal amount)
        {
            Balance += amount;
        }
    }
}

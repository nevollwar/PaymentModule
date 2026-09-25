using System.Diagnostics;

namespace PaymentApp.Domain
{
    /// <summary>
    /// Платёжная система: набор счетов и журнал операций
    /// </summary>
    public class Bank
    {
        private readonly List<Account> accounts;
        private readonly List<TransactionRecord> journal = new List<TransactionRecord>();

        public IReadOnlyList<Account> Accounts => accounts;
        public IReadOnlyList<TransactionRecord> Journal => journal;

        /// <summary>
        /// Сумма балансов системы — инвариант для переводов
        /// </summary>
        public decimal TotalBalance => accounts.Sum(a => a.Balance);

        public Bank(IEnumerable<Account> initialAccounts)
        {
            accounts = initialAccounts.ToList();
            Guard.Requires(accounts.Select(a => a.Number).Distinct().Count() == accounts.Count,
                "Номера счетов должны быть уникальными");
        }

        public static Bank CreateDemo()
        {
            return new Bank(new Account[]
            {
                new Account("40817-001", "Иванов И.И.", 10_000m),
                new Account("40817-002", "Петров П.П.", 2_500m),
                new DailyLimitAccount("40817-003", "Сидорова А.А.", 0m,
                    singleDepositLimit: 5_000m, dailyLimitRemaining: 15_000m)
            });
        }

        // Операция 2: Пополнение счёта с лимитом


        /// <summary>
        /// Проверка предусловий пополнения без побочных эффектов (для индикатора Pre).
        /// Возвращает описание первого нарушенного предусловия или null, если все выполнены.
        /// </summary>
        public string? FindDepositPreViolation(DailyLimitAccount? account, decimal amount)
        {
            if (account == null)
                return "счёт не выбран";

            if (!accounts.Contains(account))
                return "счёт не принадлежит платёжной системе";

            if (amount <= 0)
                return "сумма должна быть больше 0";

            if (amount > account.SingleDepositLimit)
                return $"сумма {amount:N2} превышает разовый лимит {account.SingleDepositLimit:N2}";

            if (amount > account.DailyLimitRemaining)
                return $"сумма {amount:N2} превышает остаток дневного лимита {account.DailyLimitRemaining:N2}";

            return null;
        }

        /// <summary>
        /// Пополнение счёта с дневным лимитом.
        /// Pre:  amount > 0; amount ≤ account.SingleDepositLimit; amount ≤ account.DailyLimitRemaining.
        /// Post: account.Balance' = account.Balance + amount;
        ///       account.DailyLimitRemaining' = account.DailyLimitRemaining − amount.
        /// </summary>
        public DepositResult Deposit(DailyLimitAccount account, decimal amount)
        {
            Guard.Requires(account != null, "Счёт не выбран");
            Guard.Requires(accounts.Contains(account!), "Счёт не принадлежит платёжной системе");
            Guard.Requires(amount > 0, "Сумма пополнения должна быть больше 0");
            Guard.Requires(amount <= account!.SingleDepositLimit,
                $"Сумма {amount:N2} превышает разовый лимит {account.SingleDepositLimit:N2}");
            Guard.Requires(amount <= account.DailyLimitRemaining,
                $"Сумма {amount:N2} превышает остаток дневного лимита {account.DailyLimitRemaining:N2}");

            decimal balanceBefore = account.Balance;
            decimal limitBefore = account.DailyLimitRemaining;

            account.Deposit(amount);
            account.ConsumeDailyLimit(amount);

            bool postconditionHolds =
                account.Balance == balanceBefore + amount &&
                account.DailyLimitRemaining == limitBefore - amount;

            Debug.Assert(postconditionHolds, "Нарушено постусловие пополнения");

            return new DepositResult(balanceBefore, account.Balance,
                limitBefore, account.DailyLimitRemaining, amount, postconditionHolds);
        }


        //  Оплата услуг с комиссией
      

        /// <summary>Справочник доступных услуг: название → ставка комиссии (0..1).</summary>
        public static readonly IReadOnlyDictionary<string, decimal> ServiceCommissions =
            new Dictionary<string, decimal>
            {
                { "Интернет",           0.01m },
                { "Мобильная связь",    0.02m },
                { "Коммунальные услуги",0.015m },
                { "Телевидение",        0.01m },
            };

        /// <summary>
        /// Проверка предусловий оплаты услуги без побочных эффектов (для индикатора Pre).
        /// </summary>
        public string? FindServicePaymentPreViolation(Account? account, string? serviceName, decimal amount)
        {
            if (account == null)
                return "счёт не выбран";

            if (!accounts.Contains(account))
                return "счёт не принадлежит платёжной системе";

            if (string.IsNullOrWhiteSpace(serviceName) || !ServiceCommissions.ContainsKey(serviceName))
                return "услуга не выбрана";

            if (amount <= 0)
                return "сумма должна быть больше 0";

            decimal commission = Math.Round(amount * ServiceCommissions[serviceName], 2);
            decimal total = amount + commission;

            if (account.Balance < total)
                return $"недостаточно средств: баланс {account.Balance:N2} < суммы+комиссии {total:N2}";

            return null;
        }

        /// <summary>
        /// Оплата услуги с комиссией.
        /// Pre:  услуга выбрана ∈ справочника; amount > 0; account.Balance ≥ amount + commission.
        /// Post: account.Balance' = account.Balance − (amount + commission); статус «Оплачено».
        /// </summary>
        public ServicePaymentResult PayService(Account account, string serviceName, decimal amount)
        {
            Guard.Requires(account != null, "Счёт не выбран");
            Guard.Requires(accounts.Contains(account!), "Счёт не принадлежит платёжной системе");
            Guard.Requires(!string.IsNullOrWhiteSpace(serviceName) && ServiceCommissions.ContainsKey(serviceName),
                "Услуга не выбрана или не найдена в справочнике");
            Guard.Requires(amount > 0, "Сумма платежа должна быть больше 0");

            decimal commission = Math.Round(amount * ServiceCommissions[serviceName], 2);
            decimal total = amount + commission;

            Guard.Requires(account!.Balance >= total,
                $"Недостаточно средств: баланс {account.Balance:N2} < суммы+комиссии {total:N2}");

            decimal balanceBefore = account.Balance;

            account.Withdraw(total);

            bool postconditionHolds = account.Balance == balanceBefore - total;

            Debug.Assert(postconditionHolds, "Нарушено постусловие оплаты услуги");

            return new ServicePaymentResult(serviceName, balanceBefore, account.Balance,
                amount, commission, total, postconditionHolds);
        }

        /// <summary>
        /// Проверка предусловий перевода без побочных эффектов (для индикатора Pre).
        /// Возвращает описание первого нарушенного предусловия или null, если все выполнены.
        /// </summary>
        public string? FindTransferPreViolation(Account? from, Account? to, decimal amount)
        {
            if (from == null || to == null)
            {
                return "не выбран счёт отправителя или получателя";
            }
            if (!accounts.Contains(from) || !accounts.Contains(to))
            {
                return "счёт не принадлежит платёжной системе";
            }
            if (from == to)
            {
                return "счёт-источник совпадает со счётом-получателем";
            }
            if (amount <= 0)
            {
                return "сумма перевода должна быть больше 0";
            }
            if (from.Balance < amount)
            {
                return $"недостаточно средств: баланс {from.Balance:N2} < суммы {amount:N2}";
            }
            return null;
        }

        /// <summary>
        /// Перевод со счёта на счёт.
        /// Pre:  from ≠ to; amount > 0; from.Balance ≥ amount.
        /// Post: from.Balance уменьшен на amount; to.Balance увеличен на amount;
        ///       в журнал добавлена запись; сумма балансов системы сохранена.
        /// </summary>
        public TransferResult Transfer(Account from, Account to, decimal amount)
        {
            Guard.Requires(from != null && to != null, "Не выбран счёт отправителя или получателя");
            Guard.Requires(accounts.Contains(from!) && accounts.Contains(to!), "Счёт не принадлежит платёжной системе");
            Guard.Requires(from != to, "Счёт-источник совпадает со счётом-получателем");
            Guard.Requires(amount > 0, "Сумма перевода должна быть больше 0");
            Guard.Requires(from!.Balance >= amount, "Недостаточно средств на счёте-источнике");

            decimal fromBefore = from.Balance;
            decimal toBefore = to!.Balance;
            decimal totalBefore = TotalBalance;
            int journalCountBefore = journal.Count;

            from.Withdraw(amount);
            to.Deposit(amount);
            TransactionRecord record = new TransactionRecord(DateTime.Now, from.Number, to.Number, amount);
            journal.Add(record);

            decimal totalAfter = TotalBalance;
            bool postconditionHolds =
                from.Balance == fromBefore - amount &&
                to.Balance == toBefore + amount &&
                journal.Count == journalCountBefore + 1 &&
                journal[^1] == record &&
                totalAfter == totalBefore;

            Debug.Assert(postconditionHolds, "Нарушено постусловие перевода");

            return new TransferResult(fromBefore, from.Balance, toBefore, to.Balance,
                totalBefore, totalAfter, record, postconditionHolds);
        }
    }
}

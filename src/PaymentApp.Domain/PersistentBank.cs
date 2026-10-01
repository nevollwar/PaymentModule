using System.Diagnostics;
using System.Globalization;
using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.Repositories;
using PaymentApp.Domain.Database.SQLite;

namespace PaymentApp.Domain
{
    /// <summary>
    /// Платёжная система с персистентностью через SQLite.
    /// Наследует всю доменную логику Bank и добавляет сохранение в БД
    /// после каждой операции, изменяющей состояние.
    /// </summary>
    public class PersistentBank : Bank
    {
        private readonly IAccountRepository accountRepository;
        private readonly ITransactionRepository transactionRepository;

        /// <summary>
        /// Создаёт PersistentBank из существующих репозиториев.
        /// Счета загружаются из БД.
        /// </summary>
        public PersistentBank(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
            : base(LoadAccounts(accountRepository))
        {
            this.accountRepository = accountRepository;
            this.transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Создаёт PersistentBank с SQLite базой данных по указанному пути.
        /// Если таблицы пусты — заполняет демо-данными.
        /// </summary>
        public static PersistentBank Create(string dbPath = "payment.db")
        {
            var db = new SqliteDatabase($"Data Source={dbPath}", new SqliteSchemaInitialier());
            db.Open();

            var accountRepo = new SqliteAccountRepository(db);
            var transactionRepo = new SqliteTransactionRepository(db);

            // Если счетов ещё нет — вставляем демо-данные
            if (!accountRepo.FindAll().Any())
            {
                SeedDemoData(accountRepo);
            }

            return new PersistentBank(accountRepo, transactionRepo);
        }

        //Перевод -  добавление сохранения в БД

        public override TransferResult Transfer(Account from, Account to, decimal amount)
        {
            TransferResult result = base.Transfer(from, to, amount);

            // Синхронизируем изменённые балансы в БД
            SaveAccount(from);
            SaveAccount(to);

            // Сохраняем запись журнала
            transactionRepository.Insert(new TransactionEntity
            {
                Timestamp = result.Record.Timestamp,
                FromAccount = result.Record.FromAccount,
                ToAccount = result.Record.ToAccount,
                Amount = result.Record.Amount
            });

            return result;
        }

        //Пополнение —  добавление сохранения в БД

        public override DepositResult Deposit(DailyLimitAccount account, decimal amount)
        {
            DepositResult result = base.Deposit(account, amount);

            // Синхронизируем баланс и дневной лимит в БД
            SaveAccount(account);

            return result;
        }

        //Оплата услуг — добавление сохранения в БД

        public override ServicePaymentResult PayService(Account account, string serviceName, decimal amount)
        {
            ServicePaymentResult result = base.PayService(account, serviceName, amount);

            // Синхронизируем баланс в БД
            SaveAccount(account);

            // Сохраняем запись об оплате в журнал
            transactionRepository.Insert(new TransactionEntity
            {
                Timestamp = DateTime.Now,
                FromAccount = account.Number,
                ToAccount = $"Поставщик: {serviceName}",
                Amount = result.TotalCharged
            });

            return result;
        }

        // Вспомогательные методы

        private void SaveAccount(Account account)
        {
            AccountEntity? existing = accountRepository.FindByNumber(account.Number);
            if (existing == null) return;

            existing.Balance = account.Balance;

            if (account is DailyLimitAccount dailyAccount)
            {
                existing.HasDailyLimit = true;
                existing.SingleDepositLimit = dailyAccount.SingleDepositLimit;
                existing.DailyLimitRemaining = dailyAccount.DailyLimitRemaining;
            }

            accountRepository.Update(existing);
        }

        private static List<Account> LoadAccounts(IAccountRepository repo)
        {
            return repo.FindAll()
                .Select(e =>
                {
                    if (e.HasDailyLimit)
                        return (Account)new DailyLimitAccount(
                            e.Number, e.Owner, e.Balance,
                            e.SingleDepositLimit ?? 0,
                            e.DailyLimitRemaining ?? 0);

                    return new Account(e.Number, e.Owner, e.Balance);
                })
                .ToList();
        }

        private static void SeedDemoData(IAccountRepository repo)
        {
            var demo = new Account[]
            {
                new Account("40817-001", "Иванов И.И.", 10_000m),
                new Account("40817-002", "Петров П.П.", 2_500m),
                new DailyLimitAccount("40817-003", "Сидорова А.А.", 0m,
                    singleDepositLimit: 5_000m, dailyLimitRemaining: 15_000m)
            };

            foreach (var acc in demo)
            {
                var entity = new AccountEntity
                {
                    Number = acc.Number,
                    Owner = acc.Owner,
                    Balance = acc.Balance,
                    HasDailyLimit = acc is DailyLimitAccount,
                    SingleDepositLimit = acc is DailyLimitAccount d ? d.SingleDepositLimit : null,
                    DailyLimitRemaining = acc is DailyLimitAccount d2 ? d2.DailyLimitRemaining : null
                };
                repo.Insert(entity);
            }
        }
    }
}

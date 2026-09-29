namespace PaymentApp.Domain.Database.SQLite
{
    /// <summary>
    /// Инициализатор схемы таблиц платёжного модуля для SQLite.
    /// Создаёт таблицы счетов и журнала операций, если они отсутствуют.
    /// </summary>
    public class SqliteSchemaInitialier : ISchemaInitializer
    {
        /// <summary>
        /// Создаёт таблицы 'accounts' и 'transactions' в базе данных.
        /// </summary>
        /// <param name="database">Экземпляр базы данных. Не может быть null</param>
        /// <exception cref="ArgumentNullException">Если database == null</exception>
        public void Initialize(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);

            const string createAccountsTableQuery = @"
                CREATE TABLE IF NOT EXISTS accounts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    number TEXT NOT NULL UNIQUE,
                    owner TEXT NOT NULL,
                    balance TEXT NOT NULL,
                    has_daily_limit INTEGER NOT NULL DEFAULT 0,
                    single_deposit_limit TEXT NULL,
                    daily_limit_remaining TEXT NULL
                );";

            const string createTransactionsTableQuery = @"
                CREATE TABLE IF NOT EXISTS transactions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL,
                    from_account TEXT NOT NULL,
                    to_account TEXT NOT NULL,
                    amount TEXT NOT NULL
                );";

            database.Execute(createAccountsTableQuery);
            database.Execute(createTransactionsTableQuery);
        }
    }
}
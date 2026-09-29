using System.Globalization;
using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.Repositories;

namespace PaymentApp.Domain.Database.SQLite
{
    /// <summary>
    /// Реализация репозитория счетов поверх базы данных SQLite.
    /// </summary>
    public class SqliteAccountRepository : DatabaseRepository, IAccountRepository
    {
        /// <summary>
        /// Конструктор для создания репозитория счетов.
        /// </summary>
        /// <param name="database">Экземпляр IDatabase. Не может быть null</param>
        public SqliteAccountRepository(IDatabase database) : base(database)
        {
        }

        /// <summary>
        /// Возвращает список всех счетов из базы данных.
        /// </summary>
        /// <returns>Список всех сущностей счетов</returns>
        public IReadOnlyList<AccountEntity> FindAll()
        {
            const string query = "SELECT * FROM accounts";
            var rows = Database.Query(query);
            var result = new List<AccountEntity>();

            foreach (var row in rows)
            {
                result.Add(MapRowToEntity(row));
            }

            return result;
        }

        /// <summary>
        /// Возвращает счёт по его первичному ключу ID.
        /// </summary>
        /// <param name="id">Уникальный ID сущности в БД</param>
        /// <returns>Сущность счёта или null, если запись не найдена</returns>
        public AccountEntity? FindById(long id)
        {
            const string query = "SELECT * FROM accounts WHERE id = $0 LIMIT 1";
            var rows = Database.Query(query, id);
            var firstRow = rows.FirstOrDefault();

            return firstRow != null ? MapRowToEntity(firstRow) : null;
        }

        /// <summary>
        /// Найти счёт по его уникальному номеру.
        /// </summary>
        /// <param name="number">Номер счёта</param>
        /// <returns>Найденный счёт или null, если не найден</returns>
        /// <exception cref="ArgumentException">Если номер счёта пуст</exception>
        public AccountEntity? FindByNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Номер счёта не может быть пустым", nameof(number));

            const string query = "SELECT * FROM accounts WHERE number = $0 LIMIT 1";
            var rows = Database.Query(query, number);
            var firstRow = rows.FirstOrDefault();

            return firstRow != null ? MapRowToEntity(firstRow) : null;
        }

        /// <summary>
        /// Вставить новый счёт в базу данных.
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        /// <returns>Сущность с присвоенным Id из базы данных</returns>
        /// <exception cref="ArgumentNullException">Если entity равен null</exception>
        public AccountEntity Insert(AccountEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            const string query = @"
                INSERT INTO accounts (number, owner, balance, has_daily_limit, single_deposit_limit, daily_limit_remaining)
                VALUES ($0, $1, $2, $3, $4, $5)";

            long generatedId = Database.Insert(query,
                entity.Number,
                entity.Owner,
                entity.Balance.ToString(CultureInfo.InvariantCulture),
                entity.HasDailyLimit ? 1 : 0,
                entity.SingleDepositLimit?.ToString(CultureInfo.InvariantCulture) ?? (object)DBNull.Value,
                entity.DailyLimitRemaining?.ToString(CultureInfo.InvariantCulture) ?? (object)DBNull.Value
            );

            entity.Id = generatedId;
            return entity;
        }

        /// <summary>
        /// Обновить существующий счёт в базе данных.
        /// </summary>
        /// <param name="entity">Сущность с обновлёнными полями</param>
        /// <returns>Обновлённая сущность</returns>
        /// <exception cref="ArgumentNullException">Если entity равен null</exception>
        /// <exception cref="ArgumentException">Если entity.Id равен 0</exception>
        public AccountEntity Update(AccountEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Id <= 0)
                throw new ArgumentException("Невозможно обновить сущность без валидного идентификатора", nameof(entity));

            const string query = @"
                UPDATE accounts
                SET number = $0,
                    owner = $1,
                    balance = $2,
                    has_daily_limit = $3,
                    single_deposit_limit = $4,
                    daily_limit_remaining = $5
                WHERE id = $6";

            Database.Execute(query,
                entity.Number,
                entity.Owner,
                entity.Balance.ToString(CultureInfo.InvariantCulture),
                entity.HasDailyLimit ? 1 : 0,
                entity.SingleDepositLimit?.ToString(CultureInfo.InvariantCulture) ?? (object)DBNull.Value,
                entity.DailyLimitRemaining?.ToString(CultureInfo.InvariantCulture) ?? (object)DBNull.Value,
                entity.Id
            );

            return entity;
        }

        /// <summary>
        /// Удалить счёт по его идентификатору.
        /// </summary>
        /// <param name="id">ID удаляемой записи</param>
        /// <returns>True, если строка была удалена; иначе False</returns>
        public bool Delete(long id)
        {
            const string query = "DELETE FROM accounts WHERE id = $0";
            int affected = Database.Execute(query, id);
            return affected > 0;
        }

        private static AccountEntity MapRowToEntity(object[] row)
        {
            return new AccountEntity
            {
                Id = Convert.ToInt64(row[0]),
                Number = Convert.ToString(row[1]) ?? string.Empty,
                Owner = Convert.ToString(row[2]) ?? string.Empty,
                Balance = decimal.Parse(Convert.ToString(row[3])!, CultureInfo.InvariantCulture),
                HasDailyLimit = Convert.ToInt64(row[4]) == 1,
                SingleDepositLimit = row[5] != DBNull.Value && row[5] != null
                    ? decimal.Parse(Convert.ToString(row[5])!, CultureInfo.InvariantCulture)
                    : null,
                DailyLimitRemaining = row[6] != DBNull.Value && row[6] != null
                    ? decimal.Parse(Convert.ToString(row[6])!, CultureInfo.InvariantCulture)
                    : null
            };
        }
    }
}
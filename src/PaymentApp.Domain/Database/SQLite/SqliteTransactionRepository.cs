using System.Globalization;
using PaymentApp.Domain.Database.Entities;
using PaymentApp.Domain.Database.Repositories;

namespace PaymentApp.Domain.Database.SQLite
{
    /// <summary>
    /// Реализация репозитория журнала транзакций поверх базы данных SQLite.
    /// </summary>
    public class SqliteTransactionRepository : DatabaseRepository, ITransactionRepository
    {
        /// <summary>
        /// Конструктор для создания репозитория транзакций.
        /// </summary>
        /// <param name="database">Экземпляр IDatabase. Не может быть null</param>
        public SqliteTransactionRepository(IDatabase database) : base(database)
        {
        }

        /// <summary>
        /// Возвращает все записи журнала транзакций.
        /// </summary>
        /// <returns>Список записей журнала</returns>
        public IReadOnlyList<TransactionEntity> FindAll()
        {
            const string query = "SELECT * FROM transactions ORDER BY timestamp DESC";
            var rows = Database.Query(query);
            var result = new List<TransactionEntity>();

            foreach (var row in rows)
            {
                result.Add(MapRowToEntity(row));
            }

            return result;
        }

        /// <summary>
        /// Возвращает транзакцию по её первичному ключу ID.
        /// </summary>
        /// <param name="id">Уникальный ID транзакции</param>
        /// <returns>Сущность транзакции или null, если не найдена</returns>
        public TransactionEntity? FindById(long id)
        {
            const string query = "SELECT * FROM transactions WHERE id = $0 LIMIT 1";
            var rows = Database.Query(query, id);
            var firstRow = rows.FirstOrDefault();

            return firstRow != null ? MapRowToEntity(firstRow) : null;
        }

        /// <summary>
        /// Получить список транзакций по счёту.
        /// </summary>
        /// <param name="accountNumber">Номер счёта</param>
        /// <returns>Список транзакций, упорядоченных по дате</returns>
        public IReadOnlyList<TransactionEntity> FindByAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Номер счёта не может быть пустым", nameof(accountNumber));

            const string query = @"
                SELECT * FROM transactions 
                WHERE from_account = $0 OR to_account = $1 
                ORDER BY timestamp DESC";

            var rows = Database.Query(query, accountNumber, accountNumber);
            var result = new List<TransactionEntity>();

            foreach (var row in rows)
            {
                result.Add(MapRowToEntity(row));
            }

            return result;
        }

        /// <summary>
        /// Добавить запись о транзакции в журнал базы данных.
        /// </summary>
        /// <param name="entity">Запись транзакции</param>
        /// <returns>Сущность с заполненным Id</returns>
        public TransactionEntity Insert(TransactionEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            const string query = @"
                INSERT INTO transactions (timestamp, from_account, to_account, amount)
                VALUES ($0, $1, $2, $3)";

            long generatedId = Database.Insert(query,
                entity.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                entity.FromAccount,
                entity.ToAccount,
                entity.Amount.ToString(CultureInfo.InvariantCulture)
            );

            entity.Id = generatedId;
            return entity;
        }

        /// <summary>
        /// Обновить запись транзакции в базе данных.
        /// </summary>
        /// <param name="entity">Сущность с обновлёнными полями</param>
        /// <returns>Обновлённая сущность</returns>
        public TransactionEntity Update(TransactionEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Id <= 0)
                throw new ArgumentException("Невозможно обновить транзакцию без валидного идентификатора", nameof(entity));

            const string query = @"
                UPDATE transactions
                SET timestamp = $0,
                    from_account = $1,
                    to_account = $2,
                    amount = $3
                WHERE id = $4";

            Database.Execute(query,
                entity.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                entity.FromAccount,
                entity.ToAccount,
                entity.Amount.ToString(CultureInfo.InvariantCulture),
                entity.Id
            );

            return entity;
        }

        /// <summary>
        /// Удалить транзакцию из журнала.
        /// </summary>
        /// <param name="id">ID удаляемой записи</param>
        /// <returns>True, если строка была удалена; иначе False</returns>
        public bool Delete(long id)
        {
            const string query = "DELETE FROM transactions WHERE id = $0";
            int affected = Database.Execute(query, id);
            return affected > 0;
        }

        private static TransactionEntity MapRowToEntity(object[] row)
        {
            return new TransactionEntity
            {
                Id = Convert.ToInt64(row[0]),
                Timestamp = DateTime.Parse(Convert.ToString(row[1])!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                FromAccount = Convert.ToString(row[2]) ?? string.Empty,
                ToAccount = Convert.ToString(row[3]) ?? string.Empty,
                Amount = decimal.Parse(Convert.ToString(row[4])!, CultureInfo.InvariantCulture)
            };
        }
    }
}
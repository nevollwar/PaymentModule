using PaymentApp.Domain.Database.Entities;

namespace PaymentApp.Domain.Database.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для управления журналом транзакций.
    /// </summary>
    public interface ITransactionRepository : IRepository<TransactionEntity>
    {
        /// <summary>
        /// Получить список всех транзакций, в которых участвовал счёт (как отправитель или получатель).
        /// </summary>
        /// <param name="accountNumber">Номер счёта для фильтрации</param>
        /// <returns>Список записей транзакций</returns>
        IReadOnlyList<TransactionEntity> FindByAccountNumber(string accountNumber);
    }
}
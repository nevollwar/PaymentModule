using PaymentApp.Domain.Database.Entities;

namespace PaymentApp.Domain.Database.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для управления счетами в базе данных.
    /// </summary>
    public interface IAccountRepository : IRepository<AccountEntity>
    {
        /// <summary>
        /// Найти счёт по его уникальному номеру.
        /// </summary>
        /// <param name="number">Номер счёта. Не может быть пустым или null</param>
        /// <returns>Найденный счёт или null, если счёт не найден</returns>
        AccountEntity? FindByNumber(string number);
    }
}
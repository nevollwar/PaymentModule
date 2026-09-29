using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database
{
    /// <summary>
    /// Интерфейс для работы с соединением к БД.
    /// </summary>
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Открыть соединение с БД
        /// </summary>
        void Open();

        /// <summary>
        /// Закрыть (освободить) соединение с БД.
        /// </summary>
        void Close();

        /// <summary>
        /// Выполнить команду INSERT/UPDATE/DELETE без результата.
        /// <returns>Число затронутых строк.</returns>
        /// </summary>
        int Execute(string query, params object[] args);

        /// <summary>
        /// Создаёт единичную запись в бд с возвратом идентификатора вставленной записи.
        /// <returns>Идентификатор вставленной записи</returns>
        /// </summary>
        long Insert(string query, params object[] args);

        /// <summary>
        /// Выполнить запрос, к БД, вовращающий строки (SELECT) 
        /// <returns>Возвращённые записи из БД</returns>
        /// </summary>
        IEnumerable<object[]> Query(string query, params object[] args);
    }
}

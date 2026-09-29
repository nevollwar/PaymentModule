using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database
{
    /// <summary>
    /// Базовый класс репозитория внутри реляционной базы данных.
    /// </summary>
    public abstract class DatabaseRepository
    {
        protected IDatabase Database { get; }

        /// <summary>
        /// Конструктор для создания репозитория
        /// </summary>
        /// <param name="database">Класс реализующий интерфейс IDatabase. Не может быть null</param>
        /// <exception cref="ArgumentNullException">Если аргумент database == null </exception>
        public DatabaseRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);

            Database = database;
        }
    }
}

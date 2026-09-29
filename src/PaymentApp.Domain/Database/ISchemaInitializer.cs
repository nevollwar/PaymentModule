using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database
{
    /// <summary>
    /// Интерфейс стратегии начальной инициализации структуры базы данных.
    /// </summary>
    public interface ISchemaInitializer
    {
        /// <summary>
        /// Выполняет инициализацию схемы таблиц и первичных данных в БД.
        /// </summary>
        /// <param name="database">Соединение с базой данных, в которой накатывается схема</param>
        void Initialize(IDatabase database);
    }
}
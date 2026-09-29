using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database.SQLite
{
    /// <summary>
    /// Реализация <see cref="IDatabase"/> поверх SQLite.
    /// Управляет соединением и выполнением команд.
    /// </summary>
    public class SqliteDatabase : IDatabase
    {
        private readonly string filePath;
        private SqliteConnection connection;

        /// <summary>
        /// Конструктор для создания подключения к базе данных SQLite.
        /// </summary>
        /// <param name="filePath">Путь к файлу базы данных. Не может быть null или пустой</param>
        /// <exception cref="ArgumentException">Если аргумент connectionString == null или пустой</exception>
        public SqliteDatabase(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Строка подключения не может быть пустой или равной null")

            this.filePath = filePath;
        }

        /// <summary>
        /// Закрывает текущее соединение с базой данных, если оно открыто.
        /// </summary>
        public void Close()
        {
            if (connection != null)
                connection.Close();
        }

        /// <summary>
        /// Освобождает ресурсы соединения с базой данных.
        /// </summary>
        public void Dispose()
        {
            if (connection != null)
                connection.Dispose();

            connection = null;
        }

        /// <summary>
        /// Выполняет команду, не возвращающую строк (INSERT/UPDATE/DELETE).
        /// </summary>
        /// <param name="query">Текст SQL-запроса с параметрами вида $0, $1, ... Не может быть null или пустым</param>
        /// <param name="args">Значения параметров запроса в порядке их следования</param>
        /// <returns>Число строк, затронутых командой</returns>
        /// <exception cref="InvalidOperationException">Если подключение к базе данных не инициализировано</exception>
        /// <exception cref="ArgumentException">Если аргумент query == null или пустой</exception>
        public int Execute(string query, params object[] args)
        {
            if (connection == null)
                throw new NullReferenceException("Подключение к базе данных не инициализировано");

            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Запрос не может быть пустым или равным null");

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = query;

            for (int i = 0; i < args.Length; i++)
                command.Parameters.AddWithValue($"${i}", args[i] ?? DBNull.Value);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создаёт единичную запись в бд с возвратом идентификатора вставленной записи.
        /// </summary>
        /// <param name="query">Текст SQL-запроса INSERT с параметрами вида $0, $1, ... Не может быть null или пустым</param>
        /// <param name="args">Значения параметров запроса в порядке их следования</param>
        /// <returns>Идентификатор вставленной записи</returns>
        /// <exception cref="InvalidOperationException">Если подключение к базе данных не инициализировано</exception>
        /// <exception cref="ArgumentException">Если аргумент query == null или пустой</exception>
        public long Insert(string query, params object[] args)
        {
            if (connection == null)
                throw new NullReferenceException("Подключение к базе данных не инициализировано");

            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Запрос не может быть пустым или равным null");

            using SqliteCommand command = connection.CreateCommand();

            // После выполнения SQL на INSERT получить индекс последней вставки.
            command.CommandText = query + "; SELECT last_insert_rowid();";

            for (int i = 0; i < args.Length; i++)
                command.Parameters.AddWithValue($"${i}", args[i] ?? DBNull.Value);

            // Выполнить операцию, а затем вернуть id.
            object? result = command.ExecuteScalar();
            return Convert.ToInt64(result);
        }

        /// <summary>
        /// Открывает соединение с базой данных, если оно ещё не открыто.
        /// </summary>
        public void Open()
        {
            if (connection == null)
                connection = new SqliteConnection(filePath);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
        }

        /// <summary>
        /// Выполняет команду, возвращающую строки (SELECT).
        /// </summary>
        /// <param name="query">Текст SQL-запроса с параметрами вида $0, $1, ... Не может быть null или пустым</param>
        /// <param name="args">Значения параметров запроса в порядке их следования</param>
        /// <returns>Последовательность строк, каждая строка — массив значений колонок</returns>
        /// <exception cref="InvalidOperationException">Если подключение к базе данных не инициализировано</exception>
        /// <exception cref="ArgumentException">Если аргумент query == null или пустой</exception>
        public IEnumerable<object[]> Query(string query, params object[] args)
        {
            if (connection == null)
                throw new NullReferenceException("Подключение к базе данных не инициализировано");

            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Запрос не может быть пустым или равным null");

            using var command = connection.CreateCommand();
            command.CommandText = query;

            for (int i = 0; i < args.Length; i++)
                command.Parameters.AddWithValue($"${i}", args[i] ?? DBNull.Value);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new object[reader.FieldCount];
                reader.GetValues(row);
                yield return row;
            }
        }
    }
}

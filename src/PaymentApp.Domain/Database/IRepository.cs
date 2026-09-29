using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database
{
    /// <summary>
    /// Базовый интерфейс для работы с определённой сущностью в БД
    /// </summary>
    /// <typeparam name="T">Тип обратываемой сущности</typeparam>
    public interface IRepository<T> where T : Entity
    {
        /// <summary>
        /// Возвращает список всех сущностей репозитория
        /// </summary>
        /// <returns>Все элементы из репозитория типа T</returns>
        public IReadOnlyList<T> FindAll();

        
        /// <summary>
        /// Возвращает сущность по её ID
        /// </summary>
        /// <param name="id">Уникальный ID сущности</param>
        /// <returns>Сущнось типа optional T</returns>
        public T? FindById(long id);

        /// <summary>
        /// Вставить сущность в базу данных
        /// </summary>
        /// <param name="entity">Сущность для вставки (id игнорируется, по дефолту должно быть 0)</param>
        public T Insert(T entity);

        /// <summary>
        /// Обновить поля сущности в базе данных
        /// </summary>
        /// <param name="entity">Сущность для обновления (id > 0)</param>
        public T Update(T entity);

        /// <summary>
        /// Удалить сущность из базы данных
        /// </summary>
        /// <param name="id">ID сущности для удаления (id > 0)</param>
        /// <returns>Возвращает true если запись существовала</returns>
        public bool Delete(long id);


    }
}

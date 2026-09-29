using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentApp.Domain.Database
{
    /// <summary>
    /// Базовая сущность, которая может храниться в репозитории.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Уникальный идентификатор сущности.
        /// </summary>
        public long Id { get; set; }
    }
}

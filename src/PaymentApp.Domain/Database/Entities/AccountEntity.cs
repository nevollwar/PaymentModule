namespace PaymentApp.Domain.Database.Entities
{
    /// <summary>
    /// Сущность банковского счёта для хранения в базе данных.
    /// Поддерживает как стандартные счета, так и счета с дневным лимитом.
    /// </summary>
    public class AccountEntity : Entity
    {
        /// <summary>
        /// Уникальный номер счёта.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// ФИО владельца счёта.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Текущий баланс счёта.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Признак наличия лимитов пополнения (true для DailyLimitAccount).
        /// </summary>
        public bool HasDailyLimit { get; set; }

        /// <summary>
        /// Максимальная сумма разового пополнения (null, если лимит отсутствует).
        /// </summary>
        public decimal? SingleDepositLimit { get; set; }

        /// <summary>
        /// Остаток суточного лимита пополнения (null, если лимит отсутствует).
        /// </summary>
        public decimal? DailyLimitRemaining { get; set; }
    }
}
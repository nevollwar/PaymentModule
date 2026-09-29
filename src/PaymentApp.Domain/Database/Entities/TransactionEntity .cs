namespace PaymentApp.Domain.Database.Entities
{
    /// <summary>
    /// Сущность записи журнала транзакций для хранения в базе данных.
    /// </summary>
    public class TransactionEntity : Entity
    {
        /// <summary>
        /// Дата и время совершения операции.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Номер счёта отправителя / списания.
        /// </summary>
        public string FromAccount { get; set; } = string.Empty;

        /// <summary>
        /// Номер счёта получателя (или название поставщика услуг при оплате).
        /// </summary>
        public string ToAccount { get; set; } = string.Empty;

        /// <summary>
        /// Сумма транзакции.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
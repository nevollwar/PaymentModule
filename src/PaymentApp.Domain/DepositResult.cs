namespace PaymentApp.Domain
{
    /// <summary>
    /// Состояние счёта до и после операции пополнения —
    /// нужно, чтобы показать выполнение постусловий в UI.
    /// </summary>
    public record DepositResult(
        decimal BalanceBefore,
        decimal BalanceAfter,
        decimal DailyLimitBefore,
        decimal DailyLimitAfter,
        decimal Amount,
        bool PostconditionHolds
    );
}

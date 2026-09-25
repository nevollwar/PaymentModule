namespace PaymentApp.Domain
{
    /// <summary>
    /// Запись журнала операций
    /// </summary>
    public record TransactionRecord(
        DateTime Timestamp,
        string FromAccount,
        string ToAccount,
        decimal Amount
        );

    /// <summary>
    /// Состояние до и после перевода — нужно, чтобы показать выполнение постусловия
    /// </summary>
    public record TransferResult(
        decimal FromBalanceBefore,
        decimal FromBalanceAfter,
        decimal ToBalanceBefore,
        decimal ToBalanceAfter,
        decimal TotalBefore,
        decimal TotalAfter,
        TransactionRecord Record,
        bool PostconditionHolds
        );
}

namespace PaymentApp.Domain
{
    /// <summary>
    /// Результат операции «Оплата услуг с комиссией».
    /// </summary>
    public record ServicePaymentResult(
        string ServiceName,
        decimal BalanceBefore,
        decimal BalanceAfter,
        decimal Amount,
        decimal Commission,
        decimal TotalCharged,
        bool PostconditionHolds
    );
}

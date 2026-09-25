namespace PaymentApp.Domain
{
    /// <summary>
    /// Модель данных для модального окна «Показать контракт»
    /// </summary>
    public record ContractInfo(
        string OperationName,
        string Preconditions,
        string Postconditions,
        string EffectsAndExceptions,
        string ValidExample,
        string InvalidExample
        );
}

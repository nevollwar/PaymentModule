namespace PaymentApp.Domain
{
    /// <summary>
    /// Класс для проверки контрактов (предусловий)
    /// </summary>
    public static class Guard
    {
        public static void Requires(bool condition, string message = "Нарушено предусловие")
        {
            if (!condition)
            {
                throw new PreconditionException(message);
            }
        }
    }

    public class PreconditionException: Exception
    {
        public PreconditionException(string message) : base(message) { }
    }
}
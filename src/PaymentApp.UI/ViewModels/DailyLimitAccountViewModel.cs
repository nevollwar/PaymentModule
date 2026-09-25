using PaymentApp.Domain;
using PaymentApp.UI.Mvvm;

namespace PaymentApp.UI.ViewModels
{
    /// <summary>
    /// Обёртка над DailyLimitAccount для отображения в UI.
    /// Дополнительно показывает лимиты.
    /// </summary>
    public class DailyLimitAccountViewModel : ViewModelBase
    {
        public DailyLimitAccount Model { get; }

        public string Number => Model.Number;
        public string Owner => Model.Owner;
        public decimal Balance => Model.Balance;
        public decimal SingleDepositLimit => Model.SingleDepositLimit;
        public decimal DailyLimitRemaining => Model.DailyLimitRemaining;

        public string DisplayName =>
            $"{Number} — {Owner} ({Balance:N2} ₽)";

        public string LimitInfo =>
            $"Разовый лимит: {SingleDepositLimit:N2} ₽ | Остаток дневного: {DailyLimitRemaining:N2} ₽";

        public DailyLimitAccountViewModel(DailyLimitAccount model)
        {
            Model = model;
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(DailyLimitRemaining));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(LimitInfo));
        }
    }
}

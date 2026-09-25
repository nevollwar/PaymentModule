using System.Collections.ObjectModel;
using System.Globalization;
using PaymentApp.Domain;

namespace PaymentApp.UI.ViewModels
{
    /// <summary>
    /// Операция 3: Оплата услуг с комиссией
    /// </summary>
    public class ServicePaymentOperationViewModel : OperationViewModelBase
    {
        private readonly Bank bank;

        public override string Title => "Оплата услуг с комиссией";

        public override ContractInfo Contract { get; } = new ContractInfo(
            "Оплата услуг с комиссией",

            // Предусловия
            "1) account ≠ null (счёт выбран и принадлежит системе)\n" +
            "2) serviceName ∈ справочника услуг (услуга выбрана)\n" +
            "3) amount > 0\n" +
            "4) account.Balance ≥ amount + commission,\n" +
            "   где commission = round(amount × rate, 2)\n" +
            "   (ставки: Интернет 1%, Мобильная связь 2%, ЖКХ 1.5%, ТВ 1%)",

            // Постусловия
            "1) account.Balance' = account.Balance − (amount + commission)\n" +
            "2) Статус операции = «Оплачено»",

            // Эффекты и исключения
            "Эффекты: баланс счёта уменьшается на (amount + commission), " +
            "где commission = round(amount × ставка_услуги, 2); " +
            "остальные счета не изменяются.\n" +
            "Исключения: при нарушении любого предусловия Guard.Requires выбрасывает " +
            "PreconditionException — состояние не меняется, кнопка «Выполнить» заблокирована. " +
            "Нарушение постусловия ловит Debug.Assert, индикатор Post становится красным.",

            // Валидный пример
            "Иванов И.И. (баланс 10 000,00 ₽): оплата «Коммунальные услуги» на 2 000,00 ₽. " +
            "commission = round(2 000,00 × 0,015, 2) = 30,00; total = 2 030,00. " +
            "Pre: 10 000,00 ≥ 2 030,00 ✓. " +
            "После: баланс 7 970,00 ₽, статус «Оплачено» → Post выполнено.",

            // Невалидный пример
            "Петров П.П. (баланс 2 500,00 ₽): оплата «Мобильная связь» на 2 500,00 ₽. " +
            "commission = round(2 500,00 × 0,02, 2) = 50,00; total = 2 550,00. " +
            "Pre нарушено (2 500,00 < 2 550,00): индикатор Pre красный, кнопка недоступна, " +
            "PreconditionException, баланс не меняется. " +
            "Аналогично отклоняются сумма 0,00 и пустая услуга."
        );

        public ObservableCollection<AccountViewModel> Accounts { get; }
        public IReadOnlyList<string> Services { get; }

        private AccountViewModel? selectedAccount;
        public AccountViewModel? SelectedAccount
        {
            get => selectedAccount;
            set
            {
                if (SetField(ref selectedAccount, value))
                    OnInputChanged();
            }
        }

        private string? selectedService;
        public string? SelectedService
        {
            get => selectedService;
            set
            {
                if (SetField(ref selectedService, value))
                    OnInputChanged();
            }
        }

        private string amountText = "";
        public string AmountText
        {
            get => amountText;
            set
            {
                if (SetField(ref amountText, value))
                    OnInputChanged();
            }
        }

        private string commissionPreviewText = "—";
        public string CommissionPreviewText
        {
            get => commissionPreviewText;
            set => SetField(ref commissionPreviewText, value);
        }

        private string lastResultText = "Оплата ещё не выполнялась.";
        public string LastResultText
        {
            get => lastResultText;
            set => SetField(ref lastResultText, value);
        }

        public ServicePaymentOperationViewModel(Bank bank)
        {
            this.bank = bank;
            Accounts = new ObservableCollection<AccountViewModel>(
                bank.Accounts.Select(a => new AccountViewModel(a)));
            Services = Bank.ServiceCommissions.Keys.ToList();
            CheckPreconditions();
        }

        public override void CheckPreconditions()
        {
            bool parsed = TryParseAmount(AmountText, out decimal amount);

            // Обновляем превью комиссии
            if (parsed && amount > 0 && SelectedService != null &&
                Bank.ServiceCommissions.TryGetValue(SelectedService, out decimal rate))
            {
                decimal commission = Math.Round(amount * rate, 2);
                CommissionPreviewText = $"{commission:N2} ₽  (итого: {amount + commission:N2} ₽)";
            }
            else
            {
                CommissionPreviewText = "—";
            }

            string? violation = parsed
                ? bank.FindServicePaymentPreViolation(SelectedAccount?.Model, SelectedService, amount)
                : "сумма должна быть числом (например, 1500 или 99,90)";

            IsPreValid = violation == null;
            StatusMessage = violation == null
                ? "Все предусловия выполнены — можно оплатить услугу."
                : "Предусловие нарушено: " + violation;
        }

        public override void Execute()
        {
            TryParseAmount(AmountText, out decimal amount);
            ServicePaymentResult result = bank.PayService(SelectedAccount!.Model, SelectedService!, amount);

            foreach (AccountViewModel account in Accounts)
                account.Refresh();

            LastResultText =
                $"Услуга:    {result.ServiceName}\n" +
                $"Сумма:     {result.Amount:N2} ₽\n" +
                $"Комиссия:  {result.Commission:N2} ₽\n" +
                $"Списано:   {result.TotalCharged:N2} ₽\n" +
                $"Баланс:    {result.BalanceBefore:N2} → {result.BalanceAfter:N2}\n" +
                $"Статус:    Оплачено ✓";

            OnInputChanged();

            if (!result.PostconditionHolds)
                throw new InvalidOperationException("постусловие оплаты услуги не выполнено");
        }

        private void OnInputChanged()
        {
            IsPostValid = null;
            CheckPreconditions();
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            string normalized = (text ?? "").Trim().Replace(',', '.');
            return decimal.TryParse(normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out amount);
        }
    }
}

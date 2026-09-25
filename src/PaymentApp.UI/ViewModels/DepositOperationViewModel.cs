using System.Collections.ObjectModel;
using System.Globalization;
using PaymentApp.Domain;

namespace PaymentApp.UI.ViewModels
{
    /// <summary>
    /// Операция 2: Пополнение счёта с лимитом
    /// </summary>
    public class DepositOperationViewModel : OperationViewModelBase
    {
        private readonly Bank bank;

        public override string Title => "Пополнение счёта с лимитом";

        public override ContractInfo Contract { get; } = new ContractInfo(
          "Пополнение счёта с лимитом",

    // Предусловия
                "1) account ≠ null (счёт выбран и принадлежит системе)\n" +
          "2) amount > 0\n" +
          "3) amount ≤ account.SingleDepositLimit  (не превышает разовый лимит)\n" +
          "4) amount ≤ account.DailyLimitRemaining (не превышает остаток дневного лимита)",

    // Постусловия
                "1) account.Balance' = account.Balance + amount\n" +
          "2) account.DailyLimitRemaining' = account.DailyLimitRemaining − amount",

    // Эффекты и исключения
                "Эффекты: баланс выбранного счёта увеличивается ровно на amount; " +
          "остаток дневного лимита уменьшается ровно на amount; " +
          "остальные счета не изменяются.\n" +
          "Исключения: при нарушении любого предусловия Guard.Requires выбрасывает " +
          "PreconditionException — состояние счёта не меняется, кнопка «Выполнить» в UI " +
          "заблокирована. Нарушение постусловия ловит Debug.Assert, индикатор Post становится красным.",

    // Валидный пример
                "Сидорова А.А. (баланс 0,00 ₽; разовый лимит 5 000,00; дневной остаток 15 000,00): " +
          "пополняем на 3 000,00. Pre: 3 000,00 > 0 ✓; 3 000,00 ≤ 5 000,00 ✓; 3 000,00 ≤ 15 000,00 ✓. " +
          "После операции: баланс 3 000,00, дневной остаток 12 000,00 → Post выполнено.",

    // Невалидный пример
                "Сидорова А.А. (дневной остаток 1 000,00): пополняем на 1 500,00. " +
          "Pre нарушено (1 500,00 > 1 000,00 = остаток дневного лимита): " +
          "индикатор Pre красный, кнопка недоступна, PreconditionException, баланс не меняется. " +
          "Аналогично отклоняются сумма 0,00 и сумма, превышающая разовый лимит 5 000,00."
        );

        /// <summary>Только счета с дневным лимитом поддерживают операцию пополнения.</summary>
        public ObservableCollection<DailyLimitAccountViewModel> LimitAccounts { get; }

        private DailyLimitAccountViewModel? selectedAccount;
        public DailyLimitAccountViewModel? SelectedAccount
        {
            get => selectedAccount;
            set
            {
                if (SetField(ref selectedAccount, value))
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

        private string lastResultText = "Пополнение ещё не выполнялось.";
        public string LastResultText
        {
            get => lastResultText;
            set => SetField(ref lastResultText, value);
        }

        public DepositOperationViewModel(Bank bank)
        {
            this.bank = bank;
            LimitAccounts = new ObservableCollection<DailyLimitAccountViewModel>(
              bank.Accounts
                .OfType<DailyLimitAccount>()
                .Select(a => new DailyLimitAccountViewModel(a)));
            CheckPreconditions();
        }

        public override void CheckPreconditions()
        {
            string? violation = TryParseAmount(AmountText, out decimal amount)
              ? bank.FindDepositPreViolation(SelectedAccount?.Model, amount)
              : "сумма должна быть числом (например, 1500 или 99,90)";

            IsPreValid = violation == null;
            StatusMessage = violation == null
              ? "Все предусловия выполнены — можно пополнить счёт."
              : "Предусловие нарушено: " + violation;
        }

        public override void Execute()
        {
            TryParseAmount(AmountText, out decimal amount);
            DepositResult result = bank.Deposit(SelectedAccount!.Model, amount);

            SelectedAccount.Refresh();

            LastResultText =
              $"Баланс:       {result.BalanceBefore:N2} → {result.BalanceAfter:N2} (+{result.Amount:N2})\n" +
              $"Дневной лимит: {result.DailyLimitBefore:N2} → {result.DailyLimitAfter:N2} (−{result.Amount:N2})";

            OnInputChanged();

            if (!result.PostconditionHolds)
                throw new InvalidOperationException("постусловие пополнения не выполнено");
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
using System.Collections.ObjectModel;
using System.Globalization;
using PaymentApp.Domain;

namespace PaymentApp.UI.ViewModels
{
    /// <summary>
    /// Операция 1: перевод со счёта на счёт
    /// </summary>
    public class TransferOperationViewModel : OperationViewModelBase
    {
        private readonly Bank bank;

        public override string Title => "Перевод со счёта на счёт";

        public override ContractInfo Contract { get; } = new ContractInfo(
            "Перевод со счёта на счёт",
            "1) from ≠ null ∧ to ≠ null (оба счёта выбраны и принадлежат системе)\n" +
            "2) from ≠ to\n" +
            "3) amount > 0\n" +
            "4) from.Balance ≥ amount",
            "1) from.Balance' = from.Balance − amount\n" +
            "2) to.Balance' = to.Balance + amount\n" +
            "3) |Journal'| = |Journal| + 1, последняя запись = (from, to, amount)\n" +
            "4) Σ Balance' = Σ Balance (инвариант «сумма балансов системы»)",
            "Эффекты: меняются балансы ровно двух счетов (источника и получателя), " +
            "в журнал операций добавляется одна запись; остальные счета не изменяются.\n" +
            "Исключения: при нарушении любого предусловия Guard.Requires выбрасывает " +
            "PreconditionException, состояние системы не меняется (в UI кнопка «Выполнить» недоступна). " +
            "Нарушение постусловия ловит Debug.Assert, индикатор Post становится красным.",
            "Иванов И.И. (баланс 10 000,00) → Петров П.П. (2 500,00), сумма 10 000,00 — ровно весь баланс. " +
            "Pre выполнено (10 000,00 ≥ 10 000,00). После перевода: Иванов 0,00, Петров 12 500,00, " +
            "сумма балансов системы 12 500,00 не изменилась, в журнале +1 запись → Post выполнено.",
            "Петров П.П. (баланс 2 500,00) → Сидорова А.А., сумма 2 500,01 — на копейку больше баланса. " +
            "Pre не выполнено (2 500,00 < 2 500,01): индикатор Pre красный, кнопка недоступна, " +
            "Guard.Requires выбрасывает PreconditionException, балансы и журнал не меняются. " +
            "Аналогично отклоняются сумма 0,00 и перевод на тот же самый счёт.");

        public ObservableCollection<AccountViewModel> Accounts { get; }
        public ObservableCollection<string> JournalLines { get; } = new ObservableCollection<string>();

        public string TotalBalanceText => $"{bank.TotalBalance:N2} ₽";

        private AccountViewModel? fromAccount;
        public AccountViewModel? FromAccount
        {
            get => fromAccount;
            set
            {
                if (SetField(ref fromAccount, value))
                {
                    OnInputChanged();
                }
            }
        }

        private AccountViewModel? toAccount;
        public AccountViewModel? ToAccount
        {
            get => toAccount;
            set
            {
                if (SetField(ref toAccount, value))
                {
                    OnInputChanged();
                }
            }
        }

        private string amountText = "";
        public string AmountText
        {
            get => amountText;
            set
            {
                if (SetField(ref amountText, value))
                {
                    OnInputChanged();
                }
            }
        }

        private string lastResultText = "Перевод ещё не выполнялся.";
        public string LastResultText
        {
            get => lastResultText;
            set => SetField(ref lastResultText, value);
        }

        public TransferOperationViewModel(Bank bank)
        {
            this.bank = bank;
            Accounts = new ObservableCollection<AccountViewModel>(bank.Accounts.Select(a => new AccountViewModel(a)));
            CheckPreconditions();
        }

        public override void CheckPreconditions()
        {
            string? violation = TryParseAmount(AmountText, out decimal amount)
                ? bank.FindTransferPreViolation(FromAccount?.Model, ToAccount?.Model, amount)
                : "сумма должна быть числом (например, 1500 или 99,90)";

            IsPreValid = violation == null;
            StatusMessage = violation == null
                ? "Все предусловия выполнены — можно выполнить перевод."
                : "Предусловие нарушено: " + violation;
        }

        public override void Execute()
        {
            TryParseAmount(AmountText, out decimal amount);
            TransferResult result = bank.Transfer(FromAccount!.Model, ToAccount!.Model, amount);

            foreach (AccountViewModel account in Accounts)
            {
                account.Refresh();
            }
            OnPropertyChanged(nameof(TotalBalanceText));

            TransactionRecord record = result.Record;
            JournalLines.Insert(0, $"{record.Timestamp:HH:mm:ss}  {record.FromAccount} → {record.ToAccount}: {record.Amount:N2} ₽");

            LastResultText =
                $"Отправитель: {result.FromBalanceBefore:N2} → {result.FromBalanceAfter:N2} (−{amount:N2})\n" +
                $"Получатель: {result.ToBalanceBefore:N2} → {result.ToBalanceAfter:N2} (+{amount:N2})\n" +
                $"Сумма балансов системы: {result.TotalBefore:N2} → {result.TotalAfter:N2}";

            // Балансы изменились — предусловия на текущем вводе нужно пересчитать
            CheckPreconditions();

            if (!result.PostconditionHolds)
            {
                throw new InvalidOperationException("постусловие перевода не выполнено");
            }
        }

        private void OnInputChanged()
        {
            // Результат прошлого запуска к новому вводу не относится
            IsPostValid = null;
            CheckPreconditions();
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            // Принимаем и запятую, и точку в качестве разделителя дробной части
            string normalized = (text ?? "").Trim().Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out amount);
        }
    }
}

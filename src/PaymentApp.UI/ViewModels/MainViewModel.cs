using System.Collections.ObjectModel;
using System.Linq;
using PaymentApp.Domain;
using PaymentApp.UI.Mvvm;

namespace PaymentApp.UI.ViewModels
{
    /// <summary>
    /// Главная модель представления приложения.
    /// Управляет списком операций и подключает базу данных SQLite.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private OperationViewModelBase? selectedOperation;

        public ObservableCollection<OperationViewModelBase> Operations { get; }

        public OperationViewModelBase? SelectedOperation
        {
            get => selectedOperation;
            set
            {
                if (SetField(ref selectedOperation, value))
                {
                    // При смене операции сразу пересчитываем предусловия
                    selectedOperation?.CheckPreconditions();
                }
            }
        }

        /// <summary>
        /// Полноценный банк с подключением к реальной базе данных SQLite (payment.db)
        /// </summary>
        public Bank Bank { get; }

        public MainViewModel()
        {
            // Инициализация персистентного банка:
            // Если файла payment.db нет, он создаётся, накатывается схема таблиц и первичные счета.
            Bank = PersistentBank.Create("payment.db");

            Operations = new ObservableCollection<OperationViewModelBase>
            {
                new TransferOperationViewModel(Bank),
                new DepositOperationViewModel(Bank),
                new ServicePaymentOperationViewModel(Bank)
            };

            // Выбираем первую операцию по умолчанию при запуске
            SelectedOperation = Operations.First();
        }
    }
}
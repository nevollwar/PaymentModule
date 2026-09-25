using System.Collections.ObjectModel;
using PaymentApp.Domain;
using PaymentApp.UI.Mvvm;

namespace PaymentApp.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private OperationViewModelBase? selectedOperation;

        public ObservableCollection<OperationViewModelBase> Operations { get; set; }

        public OperationViewModelBase? SelectedOperation
        {
            get => selectedOperation;
            set => SetField(ref selectedOperation, value);
        }

        // Общая платёжная система для всех операций
        public Bank Bank { get; } = Bank.CreateDemo();

        public MainViewModel()
        {
            Operations = new ObservableCollection<OperationViewModelBase>();
            // Операция 1: Перевод со счёта на счёт
            Operations.Add(new TransferOperationViewModel(Bank));
            // Операция 2: Пополнение счёта с лимитом
            Operations.Add(new DepositOperationViewModel(Bank));
            // Операция 3: Оплата услуг с комиссией
            Operations.Add(new ServicePaymentOperationViewModel(Bank));
        }
    }
}
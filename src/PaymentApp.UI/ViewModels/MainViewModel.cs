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
            // Сюда Чел 2, Чел 3 и Чел 4 добавят свои операции:
            Operations.Add(new TransferOperationViewModel(Bank));
            // Operations.Add(new DepositOperationViewModel());
            // Operations.Add(new ServicePaymentViewModel());
        }
    }
}
using System;
using System.Windows.Input;
using PaymentApp.Domain;
using PaymentApp.UI.Mvvm;

namespace PaymentApp.UI.ViewModels
{
    public abstract class OperationViewModelBase : ViewModelBase
    {
        public abstract string Title { get; }
        public abstract ContractInfo Contract { get; }

        private bool isPreValid;
        public bool IsPreValid
        {
            get => isPreValid;
            set => SetField(ref isPreValid, value);
        }

        private bool? isPostValid;
        public bool? IsPostValid
        {
            get => isPostValid;
            set => SetField(ref isPostValid, value);
        }

        private string statusMessage = "Заполните параметры операции";
        public string StatusMessage
        {
            get => statusMessage;
            set => SetField(ref statusMessage, value);
        }

        public ICommand ExecuteCommand { get; }

        protected OperationViewModelBase()
        {
            ExecuteCommand = new RelayCommand(RunOperation, () => IsPreValid);
        }

        // Челы 2, 3, 4 будут переопределять эти два метода под свои операции
        public abstract void CheckPreconditions();
        public abstract void Execute();

        private void RunOperation()
        {
            CheckPreconditions();
            if (!IsPreValid)
            {
                return;
            }

            try
            {
                Execute();
                IsPostValid = true;
                StatusMessage = "Успешно: операция выполнена, постусловия истинны.";
            }
            catch (Exception ex)
            {
                IsPostValid = false;
                StatusMessage = "Ошибка выполнения: " + ex.Message;
            }
        }
    }
}
using System.Windows;
using PaymentApp.Domain;

namespace PaymentApp.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для модального окна спецификации контракта ContractWindow.xaml
    /// </summary>
    public partial class ContractWindow : Window
    {
        public ContractWindow(ContractInfo contract)
        {
            InitializeComponent();

            // Заполняем данные по спецификации контракта
            OperationTitleText.Text = contract.OperationName;
            PreText.Text = contract.Preconditions;
            PostText.Text = contract.Postconditions;
            EffectsText.Text = contract.EffectsAndExceptions;
            ValidExampleText.Text = contract.ValidExample;
            InvalidExampleText.Text = contract.InvalidExample;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
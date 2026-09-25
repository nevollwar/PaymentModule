using System.Windows;
using PaymentApp.UI.ViewModels;
using PaymentApp.UI.Views;

namespace PaymentApp.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowContractButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel? viewModel = DataContext as MainViewModel;
            if (viewModel?.SelectedOperation == null)
            {
                MessageBox.Show("Сначала выберите операцию из списка слева.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ContractWindow contractWindow = new ContractWindow(viewModel.SelectedOperation.Contract);
            contractWindow.Owner = this;
            contractWindow.ShowDialog();
        }
    }
}
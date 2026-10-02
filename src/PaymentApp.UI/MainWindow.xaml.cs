using System.Windows;
using PaymentApp.UI.ViewModels;
using PaymentApp.UI.Views;

namespace PaymentApp.UI
{
    /// <summary>
    /// Логика взаимодействия для главного окна MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// Открытие диалогового окна с контрактом выбранной операции
        /// </summary>
        private void ShowContractButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel? viewModel = DataContext as MainViewModel;
            if (viewModel?.SelectedOperation == null)
            {
                MessageBox.Show("Сначала выберите операцию в меню слева.",
                                "Информация",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            ContractWindow contractWindow = new ContractWindow(viewModel.SelectedOperation.Contract)
            {
                Owner = this
            };
            contractWindow.ShowDialog();
        }
    }
}
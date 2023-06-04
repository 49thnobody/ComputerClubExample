using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ComputerClub context;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            context = new ComputerClub();
            DataContext = context;
        }

        private void RemoveComputer(object sender, RoutedEventArgs e)
        {
            var selectedComputer = lb_computers.SelectedItem as Computer;
            if (selectedComputer == null)
            {
                MessageBox.Show("Компьютер не выбран");
                return;
            }

            if (!context.RemoveComputer(selectedComputer))
            {
                MessageBox.Show("Компьютер сейчас занят! Нельзя его убрать");
            }

            lb_computers.ItemsSource = null;
            lb_computers.ItemsSource = context.Computers;
        }

        private void ServeClient(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrEmpty(tb_clientLastName.Text))
                errors.AppendLine("Фамилия не указана");
            if (string.IsNullOrEmpty(tb_clientName.Text))
                errors.AppendLine("Имя не указано");
            var selectedComputer = cb_clientComputer.SelectedItem as Computer;
            if (selectedComputer == null)
                errors.AppendLine("Компьютер не выбран");
            if (string.IsNullOrEmpty(tb_clientTime.Text))
                errors.AppendLine("Время не указано");
            if (string.IsNullOrEmpty(tb_clientBudget.Text))
                errors.AppendLine("Бюджет не указан");

            if (tb_clientTime.Text.Any(c => !char.IsDigit(c)))
                errors.AppendLine("Время указано некорректно, ожидается целое число");
            if (tb_clientBudget.Text.Any(c => !char.IsDigit(c)))
                errors.AppendLine("Бюджет указан некорректно, ожидается целое число");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (!context.ServeClient(new Client(tb_clientLastName.Text, tb_clientName.Text, Convert.ToInt32(tb_clientBudget.Text), Convert.ToInt32(tb_clientTime.Text)), selectedComputer))
            {
                MessageBox.Show("У клиента не хватило денег");
                return;
            }

            lb_computers.ItemsSource = null;
            lb_computers.ItemsSource = context.Computers;
        }

        private void AddComputer(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrEmpty(tb_computerInventoryNumber.Text))
                errors.AppendLine("Инвентарный номер не указан");
            if (string.IsNullOrEmpty(tb_computerPrice.Text))
                errors.AppendLine("Стоимость за час не указана");

            if (tb_computerPrice.Text.Any(c => !char.IsDigit(c)))
                errors.AppendLine("Стоимость указана некорректно, ожидается целое число");
            var result = context.AddComputer(new Computer(tb_computerInventoryNumber.Text, Convert.ToInt32(tb_computerPrice.Text)));
            if (result != "")
            {
                MessageBox.Show(result);
                return;
            }

            lb_computers.ItemsSource = null;
            lb_computers.ItemsSource = context.Computers;
        }

        private void FixComputer(object sender, RoutedEventArgs e)
        {
            var selectedComputer = lb_computers.SelectedItem as Computer;
            if (selectedComputer == null)
            {
                MessageBox.Show("Компьютер не выбран");
                return;
            }

            var result = context.FixComputer(selectedComputer);
            if (result != "")
            {
                MessageBox.Show(result);
                return;
            }

            lb_computers.ItemsSource = null;
            lb_computers.ItemsSource = context.Computers;
        }
    }
}

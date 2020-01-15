using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OperaWebDriver operaWebDriver = new OperaWebDriver();
        Task task;
        /// <summary>
        /// Инициализация программы и загрузка данных
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            operaWebDriver.InitializeWebDriver();
        }


        /// <summary>
        /// Добавление новых аккаунтов из файла
        /// </summary>
        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.AddNewAccounts();
        }


        /// <summary>
        /// Добавление новых реферальных кодов
        /// </summary>
        private void Refferals_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.AddNewRefferals();
        }

        /// <summary>
        /// Запуск автоматической регистрации
        /// </summary>
        private void StartReg_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.StartAutoReg();

        }


        /// <summary>
        /// Закрытие драйвера при выходе из прогаммы
        /// </summary>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            operaWebDriver.Quit();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}

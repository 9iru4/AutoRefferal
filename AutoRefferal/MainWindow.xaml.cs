﻿using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyWebDriver operaWebDriver = new MyWebDriver();
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;
        /// <summary>
        /// Инициализация программы и загрузка данных
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            operaWebDriver.InitializeWebDriver("1b2c6c99b521dAed531d16449226396d");
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
            operaWebDriver.InitializeOperaDriver();
            token = cancelTokenSource.Token;
            Task task = new Task(() => operaWebDriver.StartAutoReg(token));
            task.Start();
        }

        /// <summary>
        /// Закрытие драйвера при выходе из прогаммы
        /// </summary>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            operaWebDriver.Quit();
        }

        /// <summary>
        /// Остановка выполенения
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
        }
    }
}

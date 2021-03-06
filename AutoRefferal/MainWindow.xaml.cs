﻿using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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
            operaWebDriver.InitializeWebDriver();
            SetSettings();
            LoadAll();
        }

        /// <summary>
        /// Загрузка всех файлов
        /// </summary>
        public void LoadAll()
        {
            LoadRefferals();
            LoadProxies();
            LoadAccounts();
        }

        /// <summary>
        /// Загрузка реффералов в юи
        /// </summary>
        public void LoadRefferals()
        {
            Dispatcher.Invoke(
                () =>
                {
                    RefferalsDataGrid.ItemsSource = null;
                    RefferalsDataGrid.ItemsSource = operaWebDriver.refferals;
                    RefferalsDataGrid.Items.Refresh();
                });
        }

        /// <summary>
        /// Загрузка прокси в юи
        /// </summary>
        public void LoadProxies()
        {
            Dispatcher.Invoke(
                () =>
                {
                    ProxiesDataGrid.ItemsSource = null;
                    ProxiesDataGrid.ItemsSource = operaWebDriver.myProxies;
                    ProxiesDataGrid.Items.Refresh();
                });
        }

        /// <summary>
        /// Загрузка аккаунтов в юи
        /// </summary>
        public void LoadAccounts()
        {
            Dispatcher.Invoke(
                   () =>
                   {
                       AccountsDataGrid.ItemsSource = null;
                       AccountsDataGrid.ItemsSource = operaWebDriver.accounts;
                       AccountsDataGrid.Items.Refresh();
                   });
        }

        /// <summary>
        /// Загузка настроек в юи
        /// </summary>
        public void SetSettings()
        {
            PathToOperaBrowser.Text = operaWebDriver.settings.OperaPath;
            SMSApiKey.Text = operaWebDriver.settings.SmsApiKey;
            SelectedBrowser.SelectedValue = operaWebDriver.settings.SelectedBrowser;
            HiddenMode.IsChecked = operaWebDriver.settings.HiddenMode;
        }

        /// <summary>
        /// Запуск автоматической регистрации
        /// </summary>
        private async void StartRegButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeProgramState(true);
            token = cancelTokenSource.Token;
            Task<AutoRegState> task = new Task<AutoRegState>(() => { return operaWebDriver.StartAutoReg(token); });
            task.Start();
            await Task.WhenAll(task);
            LoadAll();
            switch (task.Result)
            {
                case AutoRegState.StoppedByUser:
                    MyMessageBox.Show("Программа остановелна по требованию пользователя.", this);
                    break;
                case AutoRegState.NotEnoughProxies:
                    MyMessageBox.Show("Прокси закончились.", this);
                    break;
                case AutoRegState.NotEnoughAccounts:
                    MyMessageBox.Show("Аккаунты закончились.", this);
                    break;
                case AutoRegState.NotEnoughRefferals:
                    MyMessageBox.Show("Рефералы закончились.", this);
                    break;
                case AutoRegState.StoppedByException:
                    MyMessageBox.Show("Произошла неизвестная ошибка, она записана в лог файл. Сообщите разработчику об этой ошибке любым удобным для вас способом.", this);
                    break;
                case AutoRegState.SMSServiceCrashed:
                    MyMessageBox.Show("Проблемы с сервером для получения номеров.", this);
                    break;
            }
            ChangeProgramState(false);
            cancelTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Изменение состояния кнопок
        /// </summary>
        /// <param name="state">Да-программа запущена, нет - готова к запуску</param>
        public void ChangeProgramState(bool state)
        {
            if (state)
            {
                StopButton.IsEnabled = true;
                StartRegButton.IsEnabled = false;
            }
            else
            {
                StopButton.IsEnabled = false;
                StartRegButton.IsEnabled = true;
            }
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
            StopButton.IsEnabled = false;
        }

        /// <summary>
        /// Добавление новых рефералов
        /// </summary>
        private void AddNewRefferalsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                operaWebDriver.AddNewRefferals(openFileDialog.FileName);
                LoadRefferals();
            }
        }

        /// <summary>
        /// Удалить всех рефералов
        /// </summary>
        private void DeleteAllRefferalsButton_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.refferals = new List<Refferal>();
            Refferal.SaveRefferals(operaWebDriver.refferals);
            LoadRefferals();
        }

        /// <summary>
        /// Добавить прокси
        /// </summary>
        private void AddNewProxiesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                operaWebDriver.AddNewProxies(openFileDialog.FileName);
                LoadProxies();
            }
        }

        /// <summary>
        /// Удалить все прокси
        /// </summary>
        private void DeleteAllProxiesButton_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.myProxies = new List<MyProxy>();
            MyProxy.SaveProxies(operaWebDriver.myProxies);
            LoadProxies();
        }

        /// <summary>
        /// Добавление новых аккаунтов
        /// </summary>
        private void AddNewAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                operaWebDriver.AddNewAccounts(openFileDialog.FileName);
                LoadAccounts();
            }
        }

        /// <summary>
        /// Удалить все аккаунты
        /// </summary>
        private void DeleteAllAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.accounts = new System.Collections.Generic.List<Account>();
            Account.SaveAccounts(operaWebDriver.accounts);
            LoadAccounts();
        }

        /// <summary>
        /// Сохранить рефералов
        /// </summary>
        private void RefferalsSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var reff = RefferalsDataGrid.ItemsSource as List<Refferal>;
            operaWebDriver.refferals = reff;
            Refferal.SaveRefferals(reff);
            LoadRefferals();
        }

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.settings = new MySettings(PathToOperaBrowser.Text, SMSApiKey.Text, "", SelectedBrowser.SelectedValue.ToString(), (bool)HiddenMode.IsChecked);
            operaWebDriver.settings.SaveSettings(operaWebDriver.settings);
            operaWebDriver.SetApiKey();
            MyMessageBox.Show("Настройки успешно сохранены.", this);
        }

        /// <summary>
        /// Сохранить прокси
        /// </summary>
        private void ProxiesSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var prox = ProxiesDataGrid.ItemsSource as List<MyProxy>;
            operaWebDriver.myProxies = prox;
            MyProxy.SaveProxies(prox);
            LoadProxies();
        }

        /// <summary>
        /// Удалить выбранный аккаунт
        /// </summary>
        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var a = AccountsDataGrid.SelectedItem as Account;
            operaWebDriver.accounts.Remove(a);
            Account.SaveAccounts(operaWebDriver.accounts);
            LoadAccounts();
        }

        /// <summary>
        /// Удалить выбранный прокси
        /// </summary>
        private void DeleteProxy_Click(object sender, RoutedEventArgs e)
        {
            var p = ProxiesDataGrid.SelectedItem as MyProxy;
            operaWebDriver.myProxies.Remove(p);
            MyProxy.SaveProxies(operaWebDriver.myProxies);
            LoadProxies();
        }

        /// <summary>
        /// Удалить выбранного реферала
        /// </summary>
        private void DeleteRefferal_Click(object sender, RoutedEventArgs e)
        {
            var r = RefferalsDataGrid.SelectedItem as Refferal;
            operaWebDriver.refferals.Remove(r);
            Refferal.SaveRefferals(operaWebDriver.refferals);
            LoadRefferals();
        }

    }
}

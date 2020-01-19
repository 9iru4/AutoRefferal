using MahApps.Metro.Controls;
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
            RefferalsDataGrid.ItemsSource = null;
            RefferalsDataGrid.ItemsSource = operaWebDriver.refferals;
        }

        /// <summary>
        /// Загрузка прокси в юи
        /// </summary>
        public void LoadProxies()
        {
            ProxiesDataGrid.ItemsSource = null;
            ProxiesDataGrid.ItemsSource = operaWebDriver.myProxies;
        }

        /// <summary>
        /// Загрузка аккаунтов в юи
        /// </summary>
        public void LoadAccounts()
        {
            AccountsDataGrid.ItemsSource = null;
            AccountsDataGrid.ItemsSource = operaWebDriver.accounts;
        }

        /// <summary>
        /// Загузка настроек в юи
        /// </summary>
        public void SetSettings()
        {
            PathToOperaBrowser.Text = operaWebDriver.settings.OperaPath;
            SMSApiKey.Text = operaWebDriver.settings.SmsApiKey;
            SelectedBrowser.SelectedValue = operaWebDriver.settings.SelectedBrowser;
        }

        /// <summary>
        /// Запуск автоматической регистрации
        /// </summary>
        private void StartRegButton_Click(object sender, RoutedEventArgs e)
        {
            token = cancelTokenSource.Token;
            Task task = new Task(() => { operaWebDriver.StartAutoReg(token); LoadAll(); });
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
            operaWebDriver.settings = new MySettings(PathToOperaBrowser.Text, SMSApiKey.Text, "", SelectedBrowser.SelectedValue.ToString());
            operaWebDriver.settings.SaveSettings(operaWebDriver.settings);
            MessageBox.Show("Настройки успешно сохранены.");
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

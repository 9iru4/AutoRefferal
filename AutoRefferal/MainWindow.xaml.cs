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
        }

        /// <summary>
        /// Загузка настроек в юи
        /// </summary>
        public void SetSettings()
        {
            SMSApiKey.Text = operaWebDriver.settings.SmsApiKey;
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
        /// Сохранить настройки
        /// </summary>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            operaWebDriver.settings = new MySettings("", SMSApiKey.Text, "", "Chrome", (bool)HiddenMode.IsChecked);
            operaWebDriver.settings.SaveSettings(operaWebDriver.settings);
            operaWebDriver.SetApiKey();
            MyMessageBox.Show("Настройки успешно сохранены.", this);
        }


    }
}

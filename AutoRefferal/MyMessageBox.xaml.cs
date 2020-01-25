using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : MetroWindow, IDisposable
    {
        /// <summary>
        /// Инициализация окна
        /// </summary>
        /// <param name="message">Текст для отображения</param>
        public MyMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        /// <summary>
        /// Нажатие кнопки ок
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Закрытие окна
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Отображение окна с текстом
        /// </summary>
        /// <param name="message">Текст для отображения</param>
        public static void Show(string message)
        {
            using (MyMessageBox mmb = new MyMessageBox(message))
            {
                mmb.ShowDialog();
            }
        }


    }
}

using MahApps.Metro.Controls;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : MetroWindow
    {
        /// <summary>
        /// Создание окна с текстом.
        /// </summary>
        /// <param name="text">Текст для отображения</param>
        public MyMessageBox(string text)
        {
            InitializeComponent();
            MessageText.Text = text;
        }

        /// <summary>
        /// Нажатие на клавишу ок
        /// </summary>
        private void OkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}

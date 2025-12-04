using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBDGalleryApp
{

    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;

        public SettingsWindow(MainWindow mw)
        {
            InitializeComponent();
            mainWindow = mw;
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = WindowState.Maximized;
            mainWindow.WindowStyle = WindowStyle.None;
            mainWindow.ResizeMode = ResizeMode.NoResize;
        }

        private void Windowed_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            mainWindow.ResizeMode = ResizeMode.CanResize;
        }

        private void PseudoFullScreen_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.WindowState = WindowState.Maximized;
            mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            mainWindow.ResizeMode = ResizeMode.NoResize;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

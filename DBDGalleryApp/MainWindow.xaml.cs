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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // 画面の読み込み
            InitializeComponent();
        }

        // 画面設定ウィンドウを開く
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow(this); // MainWindow を渡す
            settings.Owner = this;      // ← ここで所有者を設定
            settings.ShowDialog();      // モーダルで開く
        }

        // ウィンドウを閉じる
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ギャラリー画面を開く
        private void GalleryButton_Click(object sender, RoutedEventArgs e)
        {
            // ホーム画面を隠す
            HomeGrid.Visibility = Visibility.Collapsed;

            // ContentAreaに表示されている要素を削除
            ContentArea.Children.Clear();
            // ギャラリー画面を表すUserControlのインスタンスを作成
            var gallery = new GalleryView();
            // ギャラリー画面の戻るボタンが押されたときの処理
            gallery.BackRequested += () =>
            {
                ContentArea.Children.Clear();
                HomeGrid.Visibility = Visibility.Visible; // ホーム画面を再表示
            };
            ContentArea.Children.Add(gallery);
        }
    }
}
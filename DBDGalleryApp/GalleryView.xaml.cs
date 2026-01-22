using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DBDGalleryApp
{
    public class GalleryItem
    {
        public string name { get; set; }
        public string detail { get; set; }
        public List<string> tags { get; set; }
        public string img { get; set; }
        public string camp { get; set; }
    }


    public partial class GalleryView : UserControl
    {
        public event Action BackRequested;

        // JSONで読み込んだすべてのアイテムの保存
        private List<GalleryItem> items;  // ← string から変更
        // 現在のページ番号
        private int currentPage = 0;
        // 1ページに表示するアイテム数
        private int itemsPerPage = 15; // 3行×5列

        private string currentType = "S";

        private List<string> selectedTags = new List<string>();

        public GalleryView()
        {
            InitializeComponent();
            LoadItems();
            CreateTagButtons();
            DisplayPage();
        }

        // JSONファイルの読み込み
        private void LoadItems()
        {
            string path = "GalleryData.json";  // ファイル名

            string json = File.ReadAllText(path);
            items = JsonSerializer.Deserialize<List<GalleryItem>>(json);
        }

        private void CreateTagButtons()
        {
            TagPanel.Children.Clear();

            // camp が currentType のアイテムだけからタグを抽出
            var tagsForCurrentType = items
                .Where(i => i.camp == currentType && i.tags != null)
                .SelectMany(i => i.tags)
                .Distinct()
                .OrderBy(t => t);

            foreach (var tag in tagsForCurrentType)
            {
                var btn = new System.Windows.Controls.Primitives.ToggleButton
                {
                    Content = $"#{tag}",
                    Tag = tag,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                btn.Checked += TagToggle_Changed;
                btn.Unchecked += TagToggle_Changed;

                TagPanel.Children.Add(btn);
            }
        }

        // ページの処理と表示
        private void DisplayPage()
        {
            IEnumerable<GalleryItem> filtered = items.Where(i => i.camp == currentType);

            // タグフィルタ（複数 AND）
            if (selectedTags.Any())
            {
                filtered = filtered.Where(i =>
                    i.tags != null &&
                    selectedTags.All(tag => i.tags.Contains(tag))
                );
            }

            var filteredList = filtered.ToList();

            // 既存のアイテムをクリア
            ItemGrid.Items.Clear();

            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, filteredList.Count);

            for (int i = start; i < end; i++)
            {
                var item = filteredList[i];

                var img = new Image
                {
                    Width = 128,
                    Height = 128,
                    Stretch = Stretch.Uniform,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                    Tag = item
                };

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"imgs/{item.img}", UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();


                img.Source = bitmap;
                img.SnapsToDevicePixels = true;
                img.UseLayoutRounding = true;


                // マウスイベント
                img.MouseEnter += Item_MouseEnter;
                img.MouseLeave += Item_MouseLeave;
                img.MouseMove += Item_MouseMove;

                ItemGrid.Items.Add(img);
            }

            // ★ ページ数表示（ここが正しい位置）
            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling((double)filteredList.Count / itemsPerPage)
            );

            PageLabel.Content = $"[{currentPage + 1}/{totalPages}]";
        }

        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            var item = img.Tag as GalleryItem;

            PopupTitle.Text = item.name;
            PopupDetail.Text = item.detail;

            HoverPopup.IsOpen = true;
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            // カーソル移動に追従させたい場合は何もしなくてOK
            // Placement="MousePoint" が自動で処理します
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverPopup.IsOpen = false;
        }



        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            int filteredCount = items.Count(i => i.camp == currentType);
            int maxPage = (int)Math.Ceiling(filteredCount / (double)itemsPerPage);

            if (currentPage > 0)
            {
                currentPage--;
                DisplayPage();
            }
            else { 
                currentPage = maxPage - 1;
                DisplayPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int filteredCount = items.Count(i => i.camp == currentType);
            int maxPage = (int)Math.Ceiling(filteredCount / (double)itemsPerPage);

            if (currentPage + 1 < maxPage)
            {
                currentPage++;
                DisplayPage();
            }else
            {
                currentPage = 0;
                DisplayPage();
            }
        }

        private void TagToggle_Changed(object sender, RoutedEventArgs e)
        {
            var tbtn = sender as System.Windows.Controls.Primitives.ToggleButton;
            string tag = tbtn.Tag.ToString();

            if (tbtn.IsChecked == true)
            {
                if (!selectedTags.Contains(tag))
                    selectedTags.Add(tag);
            }
            else
            {
                selectedTags.Remove(tag);
            }

            UpdateTagSearchBoxText();

            currentPage = 0;
            DisplayPage();
        }

        private void UpdateTagSearchBoxText()
        {
            if (selectedTags.Count == 0)
            {
                SelectedTagText.Text = "タグ選択";
            }
            else
            {
                SelectedTagText.Text = string.Join(" ", selectedTags.Select(t => $"#{t}"));
            }
        }

        private void TagSearchBox_Click(object sender, MouseButtonEventArgs e)
        {
            TagPopup.IsOpen = true;
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }

        private void BtnShowS_Click(object sender, RoutedEventArgs e)
        {
            currentType = "S";
            selectedTags.Clear();
            UpdateTagSearchBoxText();

            CreateTagButtons();
            currentPage = 0;
            DisplayPage();
        }

        private void BtnShowK_Click(object sender, RoutedEventArgs e)
        {
            currentType = "K";
            selectedTags.Clear();
            UpdateTagSearchBoxText();

            CreateTagButtons();
            currentPage = 0;
            DisplayPage();
        }
    }
}

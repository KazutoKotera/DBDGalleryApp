using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Text.Json;
using System.IO;

namespace DBDGalleryApp
{
    public class GalleryItem
    {
        public string name { get; set; }
        public string hint { get; set; }
        public string detail { get; set; }
        public List<string> tags { get; set; }
        public string img { get; set; }
    }


    public partial class GalleryView : UserControl
    {
        public event Action BackRequested;

        private List<GalleryItem> items;  // ← string から変更
        private int currentPage = 0;
        private int itemsPerPage = 15; // 3行×5列

        public GalleryView()
        {
            InitializeComponent();
            LoadItems();
            DisplayPage();
        }

        private void LoadItems()
        {
            string path = "GalleryData.json";  // ファイル名

            if (!File.Exists(path))
            {
                MessageBox.Show("GalleryData.json が見つかりません");
                items = new List<GalleryItem>();
                return;
            }

            string json = File.ReadAllText(path);
            items = JsonSerializer.Deserialize<List<GalleryItem>>(json);
        }

        private void DisplayPage()
        {
            ItemGrid.Items.Clear();

            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, items.Count);

            for (int i = start; i < end; i++)
            {
                var item = items[i];

                // ボタンには name を表示（画像がある場合は後で画像へ変更）
                var btn = new Button
                {
                    Content = item.name,
                    Margin = new Thickness(5)
                };

                btn.Click += (s, e) =>
                    MessageBox.Show($"{item.name}\n\n{item.detail}");

                ItemGrid.Items.Add(btn);
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                DisplayPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * itemsPerPage < items.Count)
            {
                currentPage++;
                DisplayPage();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }
    }
}

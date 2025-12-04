using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Text.Json;
using System.IO;

namespace DBDGalleryApp
{
    public partial class GalleryView : UserControl
    {
        public event Action BackRequested;

        private List<string> items; // データアイテム（例として文字列）
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
            // サンプルデータ 1～300 を作成
            items = new List<string>();
            for (int i = 1; i <= 300; i++)
                items.Add($"Item {i}");
        }

        private void DisplayPage()
        {
            ItemGrid.Items.Clear();
            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, items.Count);

            for (int i = start; i < end; i++)
            {
                var btn = new Button { Content = items[i], Margin = new Thickness(5) };
                btn.Click += (s, e) => MessageBox.Show($"選択: {items[i]}");
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

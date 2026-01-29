using System;
using System.Collections.Generic;

// ↓JSONファイルの読み込み
using System.IO;

// ↓Where / Select / Any などの絞り込み
using System.Linq;

// ↓JSONファイルをクラスに変換
using System.Text.Json;

// ↓↓↓↓↓WPFのUI関連
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
// ↓画像読み込み
using System.Windows.Media.Imaging;

namespace DBDGalleryApp
{
    // JSONデータのクラス定義
    public class GalleryItem
    {
        // 名前
        public string name { get; set; }

        // 説明文
        public string detail { get; set; }

        // 検索・分類用タグ
        public List<string> tags { get; set; }

        // 画像ファイル名
        public string img { get; set; }

        // 陣営（S or K）
        public string camp { get; set; }
    }

    // ギャラリー画面のクラス定義
    public partial class GalleryView : UserControl
    {
        // 戻るボタンが押されたときのイベント
        public event Action BackRequested;

        // JSONで読み込んだ全データ
        private List<GalleryItem> items;

        // 現在のページ番号
        private int currentPage = 0;

        // 1ページに表示するアイテム数
        private int itemsPerPage = 15;

        // 現在の表示タイプ（S or K）
        private string currentType = "S";

        // 検索テキスト
        private string searchText = "";

        // 選択されているタグ
        private List<string> selectedTags = new List<string>();


        // コンストラクタ
        public GalleryView()
        {
            // XAMLの読み込み
            InitializeComponent();

            // JSONからのデータ読み込み
            LoadItems();

            // タグボタン生成
            CreateTagButtons();

            // 最初のページ表示
            DisplayPage();
        }


        // JSONからのデータ読み込み
        private void LoadItems()
        {
            // pathにJSONファイルの名前を入れる
            string path = "GalleryData.json";

            // GalleryItemのリスト変換
            string json = File.ReadAllText(path);
            items = JsonSerializer.Deserialize<List<GalleryItem>>(json);
        }


        // タグボタン生成
        private void CreateTagButtons()
        {
            // タグのパネルの中身を全削除
            TagPanel.Children.Clear();

            // tagsForCurrentTypeにJSONから読み込んだ全アイテムを入れる
            var tagsForCurrentType = items
                // SかKで絞り込み 万が一、SかKが入っていないデータがある場合は除外
                .Where(i => i.camp == currentType && i.tags != null)
                // 全タグを取り出し、重複もまとめてリストに変換する
                .SelectMany(i => i.tags)
                // 重複したタグを除外
                .Distinct()
                // タグを並び替え
                .OrderBy(t => t);

            // タグごとにボタンを作る
            foreach (var tag in tagsForCurrentType)
            {
                // ボタンの生成
                var btn = new System.Windows.Controls.Primitives.ToggleButton
                {
                    // ボタンに表示されるテキスト
                    Content = $"#{tag}",
                    // ボタンにタグを紐づける
                    Tag = tag,
                    // ボタン同士の隙間
                    Margin = new Thickness(5),
                    // ボタン内の余白
                    Padding = new Thickness(10)
                };

                // クリックイベントの登録
                btn.Checked += TagToggle_Changed;
                btn.Unchecked += TagToggle_Changed;

                // タグパネルにボタンを追加
                TagPanel.Children.Add(btn);
            }
        }

        // ページの処理と表示
        private void DisplayPage()
        {
            // フィルタリング処理
            IEnumerable<GalleryItem> filtered = items.Where(i => i.camp == currentType);

            // タグフィルタ（AND検索）
            if (selectedTags.Any())
            {
                // 選択されたタグすべてを含むアイテムに絞り込み
                filtered = filtered.Where(i =>
                    i.tags != null &&
                    selectedTags.All(tag => i.tags.Contains(tag))
                );
            }

            // テキスト検索フィルタ(空文字、スペースのみ以外)
            if (!string.IsNullOrWhiteSpace(searchText))
                {
                    // nameかdetailかtagsに検索したテキストが含まれているかどうか
                    filtered = filtered.Where(i =>
                        (i.name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (i.detail?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (i.tags?.Any(t => t.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ?? false)
                    );
                }

            // フィルタ後のリストを作成
            var filteredList = filtered.ToList();

            // 表示エリアを空にする
            ItemGrid.Items.Clear();

            // 現在のページに応じたアイテムを表示
            int start = currentPage * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, filteredList.Count);

            // アイテムの生成と表示
            for (int i = start; i < end; i++)
            {
                var item = filteredList[i];

                // 画像の生成
                var img = new Image
                {
                    Width = 128,
                    Height = 128,
                    Stretch = Stretch.Uniform,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                    Tag = item
                };

                // 画像の読み込み
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

                // アイテムグリッドに画像を追加
                ItemGrid.Items.Add(img);

            }

            // ページ数表示
            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling((double)filteredList.Count / itemsPerPage)
            );

            PageLabel.Content = $"[{currentPage + 1}/{totalPages}]";
        }


        // 検索ボックスの文字が変わるたびに呼ばれるイベント
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 入力された文字を保存
            searchText = SearchBox.Text.Trim();
            // ページを先頭に戻す
            currentPage = 0;
            // DisplayPageを呼ぶ
            DisplayPage();
        }


        // 表示された画像にマウスカーソルが乗った際に呼ばれるイベント
        private void Item_MouseEnter(object sender, MouseEventArgs e)
        {
            // sender(イベントを発生させたUI要素)をImageに変換 
            var img = sender as Image;
            // Imageに紐づけられたデータを取り出す
            var item = img.Tag as GalleryItem;

            // ポップアップに情報をセットして表示
            PopupTitle.Text = item.name;
            PopupDetail.Text = item.detail;

            // ポップアップを表示
            HoverPopup.IsOpen = true;
        }


        // 画像上でマウスが離れた際に呼ばれるイベント
        private void Item_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverPopup.IsOpen = false;
        }


        // <矢印ボタンが押された際に呼ばれるイベント
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            // フィルタ後のアイテム数を取得
            int filteredCount = items.Count(i => i.camp == currentType);
            // 最大ページ数を計算
            int maxPage = (int)Math.Ceiling(filteredCount / (double)itemsPerPage);

            // ページを戻す
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

        // >矢印ボタンが押された際に呼ばれるイベント
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            // フィルタ後のアイテム数を取得
            int filteredCount = items.Count(i => i.camp == currentType);
            // 最大ページ数を計算
            int maxPage = (int)Math.Ceiling(filteredCount / (double)itemsPerPage);

            // ページを進める
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


        // タグボタンが押された際に呼ばれるイベント
        private void TagToggle_Changed(object sender, RoutedEventArgs e)
        {
            // senderを変換
            var tbtn = sender as System.Windows.Controls.Primitives.ToggleButton;
            // タグを取得
            string tag = tbtn.Tag.ToString();

            // 選択されたタグリストを更新
            if (tbtn.IsChecked == true)
            {
                if (!selectedTags.Contains(tag))
                    selectedTags.Add(tag);
            }
            else
            {
                selectedTags.Remove(tag);
            }

            // タグ選択ボックスのテキスト更新
            UpdateTagSearchBoxText();

            // ページを先頭に戻して表示更新
            currentPage = 0;
            // DisplayPageを呼ぶ
            DisplayPage();
        }


        // タグ選択ボックスのテキスト更新
        private void UpdateTagSearchBoxText()
        {
            // 選択されたタグを表示
            if (selectedTags.Count == 0)
            {
                SelectedTagText.Text = "タグ選択";
            }
            else
            {
                SelectedTagText.Text = string.Join(" ", selectedTags.Select(t => $"#{t}"));
            }
        }


        // タグ選択ボックスがクリックされた際に呼ばれるイベント
        private void TagSearchBox_Click(object sender, MouseButtonEventArgs e)
        {
            TagPopup.IsOpen = true;
        }


        // 戻るボタンが押された際に呼ばれるイベント
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }


        // S陣営ボタンが押された際に呼ばれるイベント
        private void BtnShowS_Click(object sender, RoutedEventArgs e)
        {
            currentType = "S";
            selectedTags.Clear();
            UpdateTagSearchBoxText();

            CreateTagButtons();
            currentPage = 0;
            DisplayPage();
        }


        // K陣営ボタンが押された際に呼ばれるイベント
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

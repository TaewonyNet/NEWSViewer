using MahApps.Metro.Controls;
using NEWSViewer.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<CategoryData> CategoryDatas = new ObservableCollection<CategoryData>();

        public ObservableCollection<ArticleData> _ArticleDatas = new ObservableCollection<ArticleData>();

        public ObservableCollection<ArticleData> ArticleDatas = new ObservableCollection<ArticleData>();

        public List<CategoryData> SearchQueue = new List<CategoryData>();

        public int ArticleMaxCount = 40;

        public int ArticleMaxDay = 7;

        private bool isSearchRun;

        public bool IsSearchRun
        {
            get { return isSearchRun; }
            set
            {
                isSearchRun = value;
                Button_Search.Dispatcher.Invoke(() =>
                {
                    Button_Search.Content = isSearchRun ? "검색중" : "검색시작";
                });
            }
        }

        bool Searching = false;

        Timer Timer { get; set; }

        public const string CategoryAll = "전체";

        public Config Config { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Button_ExportCategory.Click += Button_ExportCategory_Click;
            Button_ImportCategory.Click += Button_ImportCategory_Click;
            Button_ReadAll.Click += Button_ReadAll_Click;
            Button_Search.Click += Button_Search_Click;
            Button_Option.Click += Button_Option_Click;
            Button_Folder.Click += Button_Folder_Click;
            Button_CategoryAddFolder.Click += Button_CategoryAddFolder_Click;
            Button_CategoryAdd.Click += Button_CategoryAdd_Click;
            Button_CategoryModify.Click += Button_CategoryModify_Click;
            Button_CategoryDelete.Click += Button_CategoryDelete_Click;
            TreeView_Category.SelectedItemChanged += TreeView_Category_SelectedItemChanged;
            ListView_Acticle.SelectionChanged += ListView_Acticle_SelectionChanged;
            ListView_Acticle.MouseDoubleClick += ListView_Acticle_MouseDoubleClick;
            ListView_Acticle.KeyDown += ListView_Acticle_KeyDown;
            GridSplitter_Horizontal.MouseUp += GridSplitter_Horizontal_MouseUp;
            GridSplitter_Horizontal.MouseDoubleClick += GridSplitter_Horizontal_MouseDoubleClick;
            GridSplitter_Vertical.MouseUp += GridSplitter_Vertical_MouseUp;
            GridSplitter_Vertical.MouseDoubleClick += GridSplitter_Vertical_MouseDoubleClick;
            SizeChanged += MainWindow_SizeChanged;
            Closed += MainWindow_Closed;

            TreeView_Category.Items.Clear();
            TreeView_Category.ItemsSource = CategoryDatas;
            ListView_Acticle.Items.Clear();
            ListView_Acticle.ItemsSource = ArticleDatas;
            Global.Instance.PopupLayer = Grid_Popup;

            //search();

            Init();

            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Interval = TimeSpan.FromSeconds(0.1).TotalMilliseconds;
            Timer.Start();

            Configuration.UseLocalApplicationData = true;
            Configuration.Self = new Configuration();
            var config = Configuration.Self.GetXMLToSerialize();
            if (config == null)
            {
                config = new Config();
                Save(config);
            }
            else
            {
                this.Width = config.Width;
                this.Height = config.Height;
                this.WindowState = Converters.ConvertType<WindowState>(config.WindowState);
                this.ColumnDefinition_With.Width = new GridLength(config.ColumnDefinition_With);
                this.RowDefinition_Height.Height = new GridLength(config.RowDefinition_Height);
            }
            Config = config;
        }

        private void ListView_Acticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ListView_Acticle_MouseDoubleClick(sender, null);
            }
        }

        private void GridSplitter_Vertical_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Save(Config);
        }

        private void GridSplitter_Horizontal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Save(Config);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Save(Config);
        }

        private void Save(Config config)
        {
            config.Width = this.Width;
            config.Height = this.Height;
            config.WindowState = (int)this.WindowState;
            config.ColumnDefinition_With = this.ColumnDefinition_With.Width.Value;
            config.RowDefinition_Height = this.RowDefinition_Height.Height.Value;
            Configuration.Self.SerializeToXML(config);
        }

        private void GridSplitter_Vertical_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ColumnDefinition_With.Width = new GridLength(200);
        }

        private void GridSplitter_Horizontal_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.RowDefinition_Height.Height = new GridLength(120);
        }

        private void ListView_Acticle_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListView_Acticle.SelectedItem is ArticleData)
            {
                var art = ListView_Acticle.SelectedItem as ArticleData;
                Process.Start(art.Data.Link);
                art.Data.IsRead = true;
                art.Data.ReadDate = DateTime.Now;
                SqlManager.Instance.InsertT_ARTICLE(new[] { art.Data });
            }
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (SearchQueue.Count > 0 && Searching == false)
            {
                Searching = true;

                var cate = SearchQueue[0];
                SearchQueue.RemoveAt(0);

                if (string.IsNullOrEmpty(cate.Data.SearchText) == false)
                {
                    List<T_ARTICLE> list = new List<T_ARTICLE>();
                    DateTime betime = DateTime.Now.AddDays(-ArticleMaxDay);
                    string search = cate.Data.SearchText;
                    if (string.IsNullOrEmpty(cate.Data.NoSearchText) == false)
                    {
                        foreach (var s in cate.Data.NoSearchText.Split(' '))
                        {
                            if (string.IsNullOrEmpty(s) == false)
                            {
                                search += " -" + s;
                            }
                        }
                    }
                    int page = 1;
                    var barts = SqlManager.Instance.SelectT_ARTICLE(cate.Data.CategorySeq);
                    do
                    {
                        if (cate.Data.IsSearchTitle == true)
                        {
                            var arts = await Global.Instance.WebDownloadManager.NaverSearchStock(search, page);
                            list.AddRange(arts);
                        }
                        else
                        {
                            var arts = await Global.Instance.WebDownloadManager.NaverSearch(search, page);
                            list.AddRange(arts);
                        }
                        page++;
                    }
                    while (list.Count <= ArticleMaxCount && list.Min(f => f.InfoTime) > betime);

                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (barts.FirstOrDefault(f => f.Link == list[i].Link) != null)
                        {
                            list.RemoveAt(i);
                        }
                        else
                        {
                            list[i].CategorySeq = cate.Data.CategorySeq;
                            list[i].RegDate = DateTime.Now;
                            list[i].ModDate = DateTime.Now;
                        }
                    }
                    SqlManager.Instance.InsertT_ARTICLE(list);
                    cate.Count = SqlManager.Instance.SelectCountT_ARTICLE(cate.Data.CategorySeq);
                }
                ProgressBar_Search.Dispatcher.Invoke(() =>
                {
                    ProgressBar_Search.Value += 1;
                    if (ProgressBar_Search.Value  == ProgressBar_Search.Maximum)
                    {
                        IsSearchRun = false;
                    }
                });

                Searching = false;
            }
        }

        private void ListView_Acticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ArticleData)
            {
                var data = e.AddedItems[0] as ArticleData;
                TextBlock_Press.Text = data.Data.Press;
                TextBlock_InfoTime.Text = data.Data.InfoTime.ToString("yyyy-MM-dd HH:mm:ss");
                TextBlock title = data.GetHighlightText(data.Data.Title);
                title.TextWrapping = TextWrapping.WrapWithOverflow;
                RichTextBox_ArticleHeader.Content = title;
                TextBlock desc = data.GetHighlightText(data.Data.Description);
                desc.TextWrapping = TextWrapping.WrapWithOverflow;
                RichTextBox_Article.Content = desc;
                if (string.IsNullOrEmpty(data.Data.ImageUrl) == false)
                {
                    Image_Article.Source = new BitmapImage(new Uri(data.Data.ImageUrl));
                }
                else
                {
                    Image_Article.Source = null;
                }
                Grid_ArticleContent.UpdateLayout();
            }
        }

        private void Button_ExportCategory_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_ImportCategory_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_ReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (ArticleDatas.Count > 0)
            {
                List<T_ARTICLE> list = new List<T_ARTICLE>();
                foreach(var art in ArticleDatas.Where(f=>f.Data.IsRead == false))
                {
                    art.Data.IsRead = true;
                    art.Data.ReadDate = DateTime.Now;
                    list.Add(art.Data);
                }
                SqlManager.Instance.InsertT_ARTICLE(list);
                ListView_Acticle.Items.Refresh();
            }
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            if (IsSearchRun == false)
            {
                SearchQueue.Clear();
                foreach (var folder in CategoryDatas)
                {
                    foreach (var cate in folder.Children)
                    {
                        SearchQueue.Add(cate);
                    }
                }
                ProgressBar_Search.Maximum = SearchQueue.Count;
                ProgressBar_Search.Value = 0;
                Searching = false;
                IsSearchRun = true;
            }
            else
            {
                SearchQueue.Clear();
                Searching = false;
                IsSearchRun = false;
            }
        }

        private void Button_Option_Click(object sender, RoutedEventArgs e)
        {
            OptionSetting uc = new OptionSetting();
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                //var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                //uc.Data.Data = d;
                //CategoryDatas.Add(uc.Data);
            };
        }

        private void TreeView_Category_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Button_CategoryAdd.IsEnabled = false;
            Button_CategoryModify.IsEnabled = false;
            Button_CategoryDelete.IsEnabled = false;
            var data = e.NewValue as CategoryData;
            if ((data != null) && (data.Data.Category != CategoryAll))
            {
                if (string.IsNullOrEmpty(data.Data.UpCategory) == true)
                {
                    Button_CategoryAdd.IsEnabled = true;
                }
                Button_CategoryModify.IsEnabled = true;
                Button_CategoryDelete.IsEnabled = true;

                var list = SqlManager.Instance.SelectT_ARTICLE(data.Data.CategorySeq);
                _ArticleDatas.Clear();
                foreach (var item in list)
                {
                    _ArticleDatas.Add(new ArticleData(item, data.Data.SearchText));
                }
            }
            else
            {
                var list = SqlManager.Instance.SelectT_ARTICLE();
                _ArticleDatas.Clear();
                foreach (var item in list)
                {
                    _ArticleDatas.Add(new ArticleData(item));
                }
            }

            for (int i = _ArticleDatas.Count - 1; i >= 0; i--)
            {
                var art = _ArticleDatas[i];
                var list = _ArticleDatas.Where(f => f != art && f.Data.Title == art.Data.Title);
                for (int j = list.Count() - 1; j >= 0; j--)
                {
                    var sart = list.ElementAt(j);
                    _ArticleDatas.Remove(sart);
                    sart.Data.IsDelete = true;
                    sart.Data.ModDate = DateTime.Now;
                }
            }

            ArticleDatas.Clear();
            foreach (var art in _ArticleDatas)
            {
                ArticleDatas.Add(art);
            }
        }

        private void Init()
        {
            var cates = SqlManager.Instance.SelectT_CATEGORY();
            if (cates.Count == 0)
            {
                T_CATEGORY cate = new T_CATEGORY()
                {
                    Category = CategoryAll,
                    SearchText = "",
                    NoSearchText = "",
                    Number = -1,
                    RegDate = DateTime.Now,
                    ModDate = DateTime.Now,
                    UpCategory = null,
                };
                SqlManager.Instance.InsertT_CATEGORY(cate);
                cates.Add(cate);
            }

            foreach (var cate in cates.OrderBy(f => f.UpCategory))
            {
                CategoryData data = new CategoryData(cate);
                if (string.IsNullOrEmpty(cate.UpCategory) == true)
                {
                    CategoryDatas.Add(data);
                }
                else
                {
                    var up = CategoryDatas.FirstOrDefault(f => f.Data.Category == cate.UpCategory);
                    if (up != null)
                    {
                        up.Children.Add(data);
                        data.Count = SqlManager.Instance.SelectCountT_ARTICLE(data.Data.CategorySeq);
                    }
                }
            }
            TreeView_Category.Items.Refresh();
        }
        private void Button_CategoryAddFolder_Click(object sender, RoutedEventArgs e)
        {
            CategoryAdd uc = new CategoryAdd(null);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                CategoryDatas.Add(uc.Data);
            };
        }

        private void Button_CategoryAdd_Click(object sender, RoutedEventArgs e)
        {
            CategoryData data = TreeView_Category.SelectedItem as CategoryData;
            if (data == null)
            {
                Global.Instance.Alert("항목을 선택 후 추가해주세요.");
                return;
            }
            CategoryAdd uc = new CategoryAdd(null, data.Data.Category);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                data.Children.Add(uc.Data);
            };
        }

        private void Button_CategoryModify_Click(object sender, RoutedEventArgs e)
        {
            CategoryData data = TreeView_Category.SelectedItem as CategoryData;
            if (data == null)
            {
                Global.Instance.Alert("항목을 선택 후 수정해주세요.");
                return;
            }
            CategoryAdd uc = new CategoryAdd(data, data.Data.UpCategory);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                SqlManager.Instance.UpdateT_CATEGORY(uc.Data.Data);
            };
        }

        private void Button_CategoryDelete_Click(object sender, RoutedEventArgs e)
        {
            CategoryData data = TreeView_Category.SelectedItem as CategoryData;
            if (data == null)
            {
                Global.Instance.Alert("항목을 선택 후 삭제해주세요.");
                return;
            }
            else if (data.Children.Count > 0)
            {
                Global.Instance.Alert("하위항목이 있으면 삭제할 수 없습니다.");
                return;
            }
            else if (data.Data.CategorySeq == 1)
            {
                Global.Instance.Alert("기본항목을 삭제할 수 없습니다.");
                return;
            }
            var confirm = Global.Instance.Confirm("해당 항목을 정말 삭제하겠습니까?");
            confirm.OKClicked = (s) =>
            {
                if (CategoryDatas.IndexOf(data) > -1)
                {
                    CategoryDatas.Remove(data);
                }
                else
                {
                    var parant = CategoryDatas.FirstOrDefault(f => f.Children.IndexOf(data) > -1);
                    if (parant != null)
                    {
                        parant.Children.Remove(data);
                    }
                }
                SqlManager.Instance.DeleteT_CATEGORY(data.Data);
            };
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Global.Instance.WebDownloadManager.Dispose();
            App.Current.Shutdown();
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.BasePath);
        }

        public async void search()
        {
            List<T_ARTICLE> list = new List<T_ARTICLE>();
            var l1 = await Global.Instance.WebDownloadManager.NaverSearch("특징주");
            list.AddRange(l1);
            var l2 = await Global.Instance.WebDownloadManager.NaverSearchStock("특징주");
            list.AddRange(l2);

            SqlManager.Instance.InsertT_ARTICLE(list);

            var acts = SqlManager.Instance.SelectT_ARTICLE();
            if (acts != null)
            {

            }
            else
            {

            }

        }
    }
}

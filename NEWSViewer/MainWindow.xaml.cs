using MahApps.Metro.Controls;
using Microsoft.Win32;
using NEWSViewer.Compositions;
using NEWSViewer.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public ObservableCollection<ArticleData> ArticleDatas = new ObservableCollection<ArticleData>();

        public List<CategoryData> SearchQueue = new List<CategoryData>();

        private bool isSearchRun;

        public bool IsSearchRun
        {
            get { return isSearchRun; }
            set
            {
                isSearchRun = value;
                Button_Search.Dispatcher.Invoke(() =>
                {
                    Button_Search.Content = isSearchRun ? "검색중지" : "검색시작";
                    TextBlock_Search.Text = "";
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

            Hyperlink_Contact.NavigateUri = new Uri(string.Format("mailto:kingtw@nate.com?subject=[{0}]{1}&body={2}",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "프로그램 문의사항", "여기에 내용을 적어주세요."));

            Button_ExportCategory.Click += Button_ExportCategory_Click;
            Button_ImportCategory.Click += Button_ImportCategory_Click;
            Button_ReadAll.Click += Button_ReadAll_Click;
            Button_ReadDelete.Click += Button_ReadDelete_Click;
            Button_BeforeDelete.Click += Button_BeforeDelete_Click;
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
            Button_ArticleLink.Click += Button_ArticleLink_Click;
            GridSplitter_Horizontal.MouseUp += GridSplitter_Horizontal_MouseUp;
            GridSplitter_Horizontal.MouseDoubleClick += GridSplitter_Horizontal_MouseDoubleClick;
            GridSplitter_Vertical.MouseUp += GridSplitter_Vertical_MouseUp;
            GridSplitter_Vertical.MouseDoubleClick += GridSplitter_Vertical_MouseDoubleClick;
            SizeChanged += MainWindow_SizeChanged;
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            TreeView_Category.Items.Clear();
            TreeView_Category.ItemsSource = CategoryDatas;
            ListView_Acticle.Items.Clear();
            ListView_Acticle.ItemsSource = ArticleDatas;

            // 리스트뷰 가상화
            //  VirtualizingStackPanel.IsVirtualizing = "True"
            //  ScrollViewer.CanContentScroll = "True"
            //  VirtualizingStackPanel.VirtualizationMode = "Recycling"
            //  ScrollViewer.IsDeferredScrollingEnabled = "True">
            //<ListView.ItemsPanel>
            //    <ItemsPanelTemplate>
            //        <VirtualizingStackPanel/>
            //    </ItemsPanelTemplate>
            //</ListView.ItemsPanel>
            var view = ListView_Acticle;
            view.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
            view.SetValue(ScrollViewer.CanContentScrollProperty, true);
            view.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
            view.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, true);
            view.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
            // 리스트뷰 가상화
            var treeview = TreeView_Category;
            treeview.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
            treeview.SetValue(ScrollViewer.CanContentScrollProperty, true);
            treeview.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
            treeview.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, true);
            treeview.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));

            Global.Instance.PopupLayer = Grid_Popup;

            //search();

            Init();

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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedTreeViewItem(CategoryDatas.First());

            Timer = new Timer();
            Timer.Elapsed += Timer_Elapsed;
            Timer.Interval = TimeSpan.FromSeconds(0.05).TotalMilliseconds;
            Timer.Start();

            Timer uiTimer = new Timer();
            uiTimer.Elapsed += UiTimer_Elapsed;
            uiTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            uiTimer.Start();
        }

        private void UiTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ProgressBar_Search.Dispatcher.Invoke(() =>
                {
                    if (IsSearchRun == true)
                    {
                        ProgressBar_Search.Value = ProgressBar_Search.Maximum - SearchQueue.Count;
                        TextBlock_Search.Text = LastSearchText;

                        if (ProgressBar_Search.Value == ProgressBar_Search.Maximum)
                        {
                            TextBlock_Search.Text = "재검색 " +
                            LastRefreshTime.Subtract(DateTime.Now).ToString().Remove(8);
                        }
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_BeforeDelete_Click(object sender, RoutedEventArgs e)
        {
            var selart = ListView_Acticle.SelectedItem as ArticleData;
            if (selart == null)
            {
                Global.Instance.Alert("기사를 선택 해 주세요.");
                return;
            }
            int idx = ArticleDatas.IndexOf(selart);
            List<T_ARTICLE> uparts = new List<T_ARTICLE>();
            for (int i = ArticleDatas.Count - 1; i >= idx; i--)
            {
                var art = ArticleDatas[i];
                art.Data.IsDelete = true;
                ArticleDatas.Remove(art);
                uparts.Add(art.Data);
            }
            SqlManager.Instance.UpdateT_ARTICLE(uparts);
            var cate = TreeView_Category.SelectedItem as CategoryData;
            if (cate != null)
            {
                cate.Count = ArticleDatas.Count(f => f.Data.IsRead == false);
                cate.Refresh();
            }
        }

        private void Button_ReadDelete_Click(object sender, RoutedEventArgs e)
        {
            ReadArticle(ArticleDatas, false, Global.Instance.ReadAutoDeleteDay);
        }

        private void Button_ArticleLink_Click(object sender, RoutedEventArgs e)
        {
            if ((sender is Button) && ((sender as Button).Tag != null))
            {
                Process.Start((sender as Button).Tag.ToString());
            }
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
                ReadArticle(new[] { art }, true, Global.Instance.ReadAutoDeleteDay);
            }
        }

        DateTime LastRefreshTime = DateTime.Now;

        string LastSearchText = string.Empty;

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsSearchRun)
            {
                if (Searching == false && DateTime.Now.Subtract(LastRefreshTime).TotalSeconds > 0)
                {
                    Searching = true;
                    if (SearchQueue.Count > 0)
                    {
                        var cate = SearchQueue[0];
                        SearchQueue.RemoveAt(0);

                        if (string.IsNullOrEmpty(cate.Data.SearchText) == false)
                        {
                            LastSearchText = cate.Data.SearchText;
                            List<T_ARTICLE> list = new List<T_ARTICLE>();
                            DateTime betime = DateTime.Now.AddDays(-Global.Instance.CrawlerOnceDay);
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
                                try
                                {
                                    var arts = await Global.Instance.WebDownloadManager.NaverNewsSearch(
                                        search,
                                        cate.Data.IsSearchTitle,
                                        betime,
                                        DateTime.Now,
                                        page);
                                    if (arts != null && arts.Item1 != null && arts.Item1.Count > 0)
                                    {
                                        list.AddRange(arts.Item1);
                                        if (arts.Item2 == page)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    page++;
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("WebDownloadManager {0}", ex);
                                }
                            }
                            while (list.Count > 0 && list.Count <= Global.Instance.CrawlerOnceCount && list.Min(f => f.InfoTime) > betime);

                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                if ((barts.FirstOrDefault(f => f.Link == list[i].Link) != null) ||
                                    (list[i].InfoTime <= betime))
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
                            cate.Refresh();
                        }
                        if (SearchQueue.Count == 0)
                        {
                            LastRefreshTime = DateTime.Now.AddSeconds(Global.Instance.ReSearchTimeSec);
                        }
                    }
                    else
                    {
                        SearchStart();
                    }
                    Searching = false;
                }
            }
        }

        private void ListView_Acticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ArticleData)
            {
                Button_ArticleLink.Visibility = Visibility.Visible;
                var art = e.AddedItems[0] as ArticleData;
                Button_ArticleLink.Tag = art.Data.Link;
                TextBlock_Press.Text = art.Data.Press;
                TextBlock_InfoTime.Text = art.Data.InfoTime.ToString("yyyy-MM-dd HH:mm:ss");
                TextBlock title = art.GetHighlightText(art.Data.Title);
                title.TextWrapping = TextWrapping.WrapWithOverflow;
                RichTextBox_ArticleHeader.Content = title;
                TextBlock desc = art.GetHighlightText(art.Data.Description);
                desc.TextWrapping = TextWrapping.WrapWithOverflow;
                RichTextBox_Article.Content = desc;
                if (string.IsNullOrEmpty(art.Data.ImageUrl) == false)
                {
                    Image_Article.Source = new BitmapImage(new Uri(art.Data.ImageUrl));
                }
                else
                {
                    Image_Article.Source = null;
                }
                if (Global.Instance.PreviewRead == true)
                {
                    ReadArticle(new[] { art }, true, Global.Instance.ReadAutoDeleteDay);
                }
                Grid_ArticleContent.UpdateLayout();
            }
        }

        private void Button_ExportCategory_Click(object sender, RoutedEventArgs e)
        {
            CSVHelper csv = new CSVHelper();
            if (csv.SaveFileDialog() == true)
            {
                List<object[]> rows = new List<object[]>();
                rows.Add(new object[] {
                    "폴더", "검색어", "제외어", "제목만(제목만:1, 전체검색:0)"//, "폴더키(백업용 없어도됨)", "키(백업용 없어도됨)"
                });
                foreach (var folder in CategoryDatas)
                {
                    foreach (var cate in folder.Children.OrderBy(f => f.Data.SearchText))
                    {
                        rows.Add(new object[]
                        {
                            folder.Data.Category,
                            cate.Data.SearchText,
                            cate.Data.NoSearchText,
                            cate.Data.IsSearchTitle ? 1:0
                            //folder.Data.CategorySeq,
                            //cate.Data.CategorySeq
                        });
                    }
                }
                csv.WriteFile(rows);
            }
        }

        private void Button_ImportCategory_Click(object sender, RoutedEventArgs e)
        {
            CSVHelper csv = new CSVHelper();
            if (csv.OpenFileDialog() == true)
            {
                List<string[]> rows = new List<string[]>();
                csv.ReadFile(rows);
                //"폴더", "검색어", "제외어", "제목만(제목만:1, 전체검색:0)"
                for (int i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Length > 2)
                    {
                        var folder = CategoryDatas.FirstOrDefault(f => f.Data.Category == row[0].Trim());
                        if (folder == null)
                        {
                            T_CATEGORY data = new T_CATEGORY()
                            {
                                Category = row[0],
                                RegDate = DateTime.Now,
                                ModDate = DateTime.Now,
                            };
                            data = SqlManager.Instance.InsertT_CATEGORY(data);
                            folder = new CategoryData(data);
                            CategoryDatas.Add(folder);
                        }
                        var cate = folder.Children.FirstOrDefault(f => f.Data.SearchText == row[1]);
                        var nosearch = row.Length > 2 ? row[2] : string.Empty;
                        var istitle = row.Length > 3 ? row[3].Trim() == "1" : false;
                        if ((cate == null) ||
                            (cate.Data.NoSearchText != nosearch) ||
                            (cate.Data.IsSearchTitle != istitle))
                        {
                            T_CATEGORY data = new T_CATEGORY()
                            {
                                UpCategorySeq = folder.Data.CategorySeq,
                                SearchText = row[1],
                                NoSearchText = nosearch,
                                IsSearchTitle = istitle,
                                RegDate = DateTime.Now,
                                ModDate = DateTime.Now,
                            };
                            data = SqlManager.Instance.InsertT_CATEGORY(data);
                            if (cate == null)
                            {
                                cate = new CategoryData(data);
                                folder.Children.Add(cate);
                            }
                            else
                            {
                                cate.Data = data;
                            }
                        }
                        folder.Refresh();
                        cate.Refresh();
                    }
                }
                //TreeView_Category.Items.Refresh();
            }
        }

        private void Button_ReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (ArticleDatas.Count > 0)
            {
                ReadArticle(ArticleDatas, true, Global.Instance.ReadAutoDeleteDay);
                //ListView_Acticle.Items.Refresh();
            }
        }

        private void ReadArticle(IEnumerable<ArticleData> datas, bool? isread, int readautodelday)
        {
            if (datas == null || datas.Count() == 0)
            {
                return;
            }
            var arts = datas.ToArray();
            if (isread != null)
            {
                arts = arts.Where(f => f.Data.IsRead == !isread.Value).ToArray();
            }
            List<T_ARTICLE> uparts = new List<T_ARTICLE>();
            for (int i = arts.Length - 1; i >= 0; i--)
            {
                var art = arts[i];
                if (art.Data.IsRead == false)
                {
                    art.Data.IsRead = true;
                    art.Data.ReadDate = DateTime.Now;
                }
                if (Global.Instance.ReadAutoDeleteDay == 0)
                {
                    art.Data.IsDelete = true;
                    ArticleDatas.Remove(art);
                }
                else
                {
                    art.Refresh();
                }
                uparts.Add(art.Data);
            }
            SqlManager.Instance.UpdateT_ARTICLE(uparts);
            if (Global.Instance.ReadAutoDeleteDay > 0)
            {
                SqlManager.Instance.DeleteT_ARTICLE(DateTime.Now.AddDays(Global.Instance.ReadAutoDeleteDay));
            }

            var cate = TreeView_Category.SelectedItem as CategoryData;
            if (cate.Data.UpCategorySeq == null)
            {
                var counts = SqlManager.Instance.SelectCountT_ARTICLE(cate.Children.Select(f => f.Data.CategorySeq));
                foreach (var scate in cate.Children)
                {
                    var count = counts.FirstOrDefault(f => f.CategorySeq == scate.Data.CategorySeq);
                    scate.Count = count != null ? count.Count : 0;
                    scate.Refresh();
                }
            }
            else
            {
                cate.Count = SqlManager.Instance.SelectCountT_ARTICLE(cate.Data.CategorySeq);
                cate.Refresh();
            }
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            if (IsSearchRun == false)
            {
                SearchStart();
            }
            else
            {
                SearchQueue.Clear();
                Searching = false;
                IsSearchRun = false;
            }
        }

        private void SearchStart()
        {
            SearchQueue.Clear();
            foreach (var folder in CategoryDatas)
            {
                foreach (var cate in folder.Children)
                {
                    SearchQueue.Add(cate);
                }
            }
            ProgressBar_Search.Dispatcher.Invoke(() => {
                ProgressBar_Search.Maximum = SearchQueue.Count;
                ProgressBar_Search.Value = 0;
            });
            Searching = false;
            IsSearchRun = true;
            LastRefreshTime = DateTime.Now;
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
            List<ArticleData> arts = new List<ArticleData>();
            if (data != null)
            {
                if (data.Data.Category == CategoryAll)
                {
                    // 전체
                    var list = SqlManager.Instance.SelectT_ARTICLE();
                    foreach (var item in list)
                    {
                        arts.Add(new ArticleData(item));
                    }
                }
                else
                {
                    if (data.Data.UpCategorySeq != null)
                    {
                        // 검색어
                        Button_CategoryAdd.IsEnabled = true;
                        var list = SqlManager.Instance.SelectT_ARTICLE(data.Data.CategorySeq);
                        foreach (var item in list)
                        {
                            arts.Add(new ArticleData(item, data.Data.SearchText));
                        }
                    }
                    else
                    {
                        // 폴더
                        Button_CategoryAdd.IsEnabled = true;
                        var list = SqlManager.Instance.SelectT_ARTICLE(
                            data.Children.Select(f => f.Data.CategorySeq)
                            );
                        foreach (var item in list)
                        {
                            arts.Add(new ArticleData(item, data.Data.SearchText));
                        }
                    }
                    Button_CategoryModify.IsEnabled = true;
                    Button_CategoryDelete.IsEnabled = true;
                }

                List<T_ARTICLE> uparts = new List<T_ARTICLE>();
                // 동일제목 삭제
                for (int i = arts.Count - 1; i >= 0; i--)
                {
                    var art = arts[i];
                    var list = arts.Where(f => f != art && f.Data.Title == art.Data.Title && f.Data.CategorySeq == art.Data.CategorySeq);

                    for (int j = list.Count() - 1; j >= 0; j--)
                    {
                        var sart = list.ElementAt(j);
                        arts.Remove(sart);
                        sart.Data.IsDelete = true;
                        sart.Data.ModDate = DateTime.Now;
                        uparts.Add(sart.Data);
                        i--;
                    }
                }
                SqlManager.Instance.UpdateT_ARTICLE(uparts);

                if ((data.Data.Category != CategoryAll)
                    && (data.Data.UpCategorySeq != null))
                {
                    data.Count = arts.Count;
                }
            }

            ArticleDatas.Clear();
            foreach (var art in arts.OrderByDescending(f => f.Data.InfoTime))
            {
                ArticleDatas.Add(art);
                //art.Refresh();
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
                    UpCategorySeq = null,
                };
                SqlManager.Instance.InsertT_CATEGORY(cate);
                cates.Add(cate);
            }

            var counts = SqlManager.Instance.SelectCountT_ARTICLE(cates.Select(f => f.CategorySeq));
            foreach (var cate in cates.OrderBy(f => f.UpCategorySeq))
            {
                CategoryData data = new CategoryData(cate);
                if (cate.UpCategorySeq == null)
                {
                    CategoryDatas.Add(data);
                }
                else
                {
                    var up = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == cate.UpCategorySeq);
                    if (up != null)
                    {
                        up.Children.Add(data);
                        var count = counts.FirstOrDefault(f => f.CategorySeq == data.Data.CategorySeq);
                        data.Count = count != null ? count.Count : 0;
                    }
                }
                data.Refresh();
            }
#if DEBUG
#endif
            //TreeView_Category.Items.Refresh();
            //var tvi = TreeView_Category.ItemContainerGenerator.ContainerFromItem(CategoryDatas.First())
            //          as TreeViewItem;
            //if (tvi != null)
            //{
            //    tvi.IsSelected = true;
            //}

            SearchStart();

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) => {

            }));
        }

        private void SelectedTreeViewItem(CategoryData data)
        {
            CategoryData sel = TreeView_Category.SelectedItem as CategoryData;
            if (sel == data)
            {
                return;
            }
            CategoryData seled = CategoryDatas.FirstOrDefault(f =>
                                            f.IsSelected == true ||
                                            f.Children.FirstOrDefault(f2 => f2.IsSelected == true) != null);
            CategoryData subseled = null;
            if (seled != null)
            {
                seled.IsSelected = false;
                subseled = seled.Children.FirstOrDefault(f2 => f2.IsSelected == true);
                if (subseled != null)
                {
                    subseled.IsSelected = false;
                }
            }
            data.IsSelected = true;
        }

        private void Button_CategoryAddFolder_Click(object sender, RoutedEventArgs e)
        {
            CategoryAdd uc = new CategoryAdd(null);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                var before = CategoryDatas.FirstOrDefault(f => f.Data.Category == uc.Data.Data.Category);
                if (before != null)
                {
                    Global.Instance.Alert("이미 등록된 폴더명입니다.");
                    SelectedTreeViewItem(before);
                    return;
                }
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                CategoryDatas.Add(uc.Data);
                uc.Data.Refresh();
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
            else if (data.Data.Category == CategoryAll)
            {
                Global.Instance.Alert("기본항목에는 추가할 수 없습니다.");
                return;
            }
            CategoryAdd uc = new CategoryAdd(null, data.Data.CategorySeq);
            if (data.Data.UpCategorySeq != null)
            {
                data = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == data.Data.UpCategorySeq);
            }
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                var before = data.Children.FirstOrDefault(f => f.Data.SearchText == uc.Data.Data.SearchText);
                if (before != null)
                {
                    Global.Instance.Alert("이미 등록된 검색어입니다.");
                    SelectedTreeViewItem(before);
                    return;
                }
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                data.Children.Add(uc.Data);
                uc.Data.Refresh();
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
            CategoryAdd uc = new CategoryAdd(data, data.Data.UpCategorySeq);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                if (data.Data.UpCategorySeq == null)
                {
                    var before = CategoryDatas.FirstOrDefault(f => f.Data != uc.Data.Data && f.Data.Category == uc.Data.Data.Category);
                    if (before != null)
                    {
                        Global.Instance.Alert("이미 등록된 검색어입니다.");
                        SelectedTreeViewItem(before);
                        return;
                    }
                }
                else
                {
                    var before = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == uc.Data.Data.UpCategorySeq);
                    if (before != null)
                    {
                        before = before.Children.FirstOrDefault(f => f.Data.SearchText == uc.Data.Data.SearchText);
                        if (before != null)
                        {
                            Global.Instance.Alert("이미 등록된 검색어입니다.");
                            SelectedTreeViewItem(before);
                            return;
                        }
                    }
                }
                SqlManager.Instance.UpdateT_CATEGORY(uc.Data.Data);
                uc.Data.Refresh();
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
            else if (data.Data.Category == CategoryAll)
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
            Application.Current.Shutdown();
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.BasePath);
        }
    }
}

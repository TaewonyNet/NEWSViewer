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
using WpfScreenHelper;

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
            Button_EditCategory.Click += Button_EditCategory_Click;
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
            Button_SearchCategory.Click += Button_SearchCategory_Click;
            TextBox_Search.KeyDown += TextBox_Search_KeyDown;

            MenuItem_KeywordDelete.Click += MenuItem_KeywordDelete_Click;
            MenuItem_KeywordView.Click += MenuItem_KeywordView_Click;
            MenuItem_ReadAll.Click += Button_ReadAll_Click;
            MenuItem_ReadDelete.Click += Button_ReadDelete_Click;
            MenuItem_BeforeDelete.Click += Button_BeforeDelete_Click;
            MenuItem_Search.Click += Button_Search_Click;

            MenuItem_CategoryAddFolder.Click += Button_CategoryAddFolder_Click;
            MenuItem_CategoryAdd.Click += Button_CategoryAdd_Click;
            MenuItem_CategoryModify.Click += Button_CategoryModify_Click;
            MenuItem_CategoryDelete.Click += Button_CategoryDelete_Click;

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
            //treeview.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
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
                this.Top = config.Top;
                this.Left = config.Left;
            }

            Config = config;
        }

        private void MenuItem_KeywordView_Click(object sender, RoutedEventArgs e)
        {
            var selart = ListView_Acticle.SelectedItem as ArticleData;
            if (selart == null)
            {
                Global.Instance.Alert("기사를 선택 해 주세요.");
                return;
            }
            CategoryData before = null;
            foreach (var cate in CategoryDatas)
            {
                before = cate.Items.FirstOrDefault(f => f.Data.CategorySeq == selart.Data.CategorySeq);
                if (before != null)
                {
                    SelectedTreeViewItem(before);
                    return;
                }
            }
        }

        private void MenuItem_KeywordDelete_Click(object sender, RoutedEventArgs e)
        {
            var selart = ListView_Acticle.SelectedItem as ArticleData;
            if (selart == null)
            {
                Global.Instance.Alert("기사를 선택 해 주세요.");
                return;
            }
            var confirm = Global.Instance.Confirm("해당 항목을 정말 삭제하겠습니까?");
            confirm.OKClicked = (s) =>
            {
                CategoryData before = null;
                foreach (var cate in CategoryDatas)
                {
                    before = cate.Items.FirstOrDefault(f => f.Data.CategorySeq == selart.Data.CategorySeq);
                    if (before != null)
                    {
                        SqlManager.Instance.DeleteT_CATEGORY(before.Data);
                        cate.Items.Remove(before);
                        return;
                    }
                }
            };
        }

        private void TextBox_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_SearchCategory_Click(sender, e);
            }
        }

        private void Button_SearchCategory_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox_Search.Text) == false)
            {
                var search = TextBox_Search.Text.Trim();
                var list = CategoryDatas.SelectMany(f => f.Items.Select(f2 => f2.Data.SearchText)).ToList();
                var index = 0;
                if ((TreeView_Category.SelectedItem is CategoryData)
                    && ((TreeView_Category.SelectedItem as CategoryData).Data.UpCategorySeq != null))
                {
                    var txt = (TreeView_Category.SelectedItem as CategoryData).Data.SearchText;
                    index = list.IndexOf(txt);
                }
                var sel = list.Skip(index + 1).FirstOrDefault(f =>
                  f.IndexOf(search) > -1);
                if (sel == null)
                {
                    sel = list.Take(index).FirstOrDefault(f =>
                  f.IndexOf(search) > -1);
                }
                if (sel == null)
                {
                    Global.Instance.Alert("검색어를 찾을 수 없습니다.");
                }
                else
                {
                    CategoryData before = null;
                    foreach (var cate in CategoryDatas)
                    {
                        before = cate.Items.FirstOrDefault(f => f.Data.SearchText == sel);
                        if (before != null)
                        {
                            before.IsSelected = true;
                            SelectedTreeViewItem(before);
                            return;
                        }
                    }
                    if (before == null)
                    {

                    }
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedTreeViewItem(CategoryDatas.First());

            SearchStart();

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
                        TimeSpan time = LastRefreshTime.Subtract(DateTime.Now);
                        if ((ProgressBar_Search.Value == ProgressBar_Search.Maximum)
                            && (time.TotalSeconds > 0))
                        {
                            TextBlock_Search.Text = "재검색 " + time.ToString().Remove(8);
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

            var confirm = Global.Instance.Confirm("해당 항목을 정말 삭제하겠습니까?");
            confirm.OKClicked = (s) =>
            {
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
            };
        }

        private void Button_ReadDelete_Click(object sender, RoutedEventArgs e)
        {
            List<T_ARTICLE> uparts = new List<T_ARTICLE>();
            for (int i = ArticleDatas.Count - 1; i >= 0; i--)
            {
                var art = ArticleDatas[i];
                if (art.Data.IsRead == true)
                {
                    art.Data.IsDelete = true;
                    ArticleDatas.Remove(art);
                    uparts.Add(art.Data);
                }
            }
            SqlManager.Instance.UpdateT_ARTICLE(uparts);
            var cate = TreeView_Category.SelectedItem as CategoryData;
            if (cate != null)
            {
                cate.Count = ArticleDatas.Count(f => f.Data.IsRead == false);
                cate.Refresh();
            }
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
            config.Top = this.Top;
            config.Left = this.Left;
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
                ReadArticle(new[] { art }, true);
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
                //if (string.IsNullOrEmpty(art.Data.ImageUrl) == false)
                //{
                //    Image_Article.Source = new BitmapImage(new Uri(art.Data.ImageUrl));
                //}
                //else
                //{
                //    Image_Article.Source = null;
                //}
                if (Global.Instance.PreviewRead == true)
                {
                    ReadArticle(new[] { art }, true);
                }
                Grid_ArticleContent.UpdateLayout();
            }
        }

        private void Button_ExportCategory_Click(object sender, RoutedEventArgs e)
        {
            CSVHelper csv = new CSVHelper();
            if (csv.SaveFileDialog() == true)
            {
                List<string[]> rows = new List<string[]>();
                rows.Add(new string[] {
                    "폴더", "검색어", "제외어", "제목만(제목만:1, 전체검색:0)"//, "폴더키(백업용 없어도됨)", "키(백업용 없어도됨)"
                }); 
                ExportCategory(rows);
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
                rows.RemoveAt(0);
                ImportCategory(rows);
            }
        }
        
        private void Button_EditCategory_Click(object sender, RoutedEventArgs e)
        {
            List<string[]> rows = new List<string[]>();
            CategoryEdit uc = new CategoryEdit(ExportCategory(rows));
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                ImportCategory(uc.Data.ToList());
            };
        }

        private List<string[]> ExportCategory(List<string[]> rows)
        {
            foreach (var folder in CategoryDatas)
            {
                foreach (var cate in folder.Items.OrderBy(f => f.Data.SearchText))
                {
                    rows.Add(new string[]
                    {
                        folder.Data.Category,
                        cate.Data.SearchText,
                        cate.Data.NoSearchText,
                        cate.Data.IsSearchTitle ? "1":"0"
                        //folder.Data.CategorySeq,
                        //cate.Data.CategorySeq
                    });
                }
            }

            return rows;
        }

        private void ImportCategory(List<string[]> rows)
        {
            List<string> samelist = new List<string>();
            DateTime dt = DateTime.Now;
            List<T_CATEGORY> list = new List<T_CATEGORY>();
            //"폴더", "검색어", "제외어", "제목만(제목만:1, 전체검색:0)"
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.Length > 0 && string.IsNullOrWhiteSpace(row[0]) == false)
                {
                    var data = new T_CATEGORY()
                    {
                        Category = row[0].Trim(),
                        IsSearchTitle = row.Length > 3 ? row[3].Trim() == "1" : true,
                        Type = null,
                        SearchText = row.Length > 1 ? row[1].Trim() : string.Empty,
                        NoSearchText = row.Length > 2 ? row[2].Trim() : string.Empty,
                        UpCategorySeq = null,
                        Number = 0,
                        RegDate = dt,
                        ModDate = dt,
                    };
                    var same = list.FirstOrDefault(f => f.SearchText == data.SearchText);
                    if (same != null)
                    {
                        samelist.Add(string.Format("{0}>{1}", data.Category, data.SearchText));
                    }
                    else
                    {
                        list.Add(data);
                    }
                }
            }
            var upcates = list.Select(f => f.Category).Distinct().ToList();
            var delcates = CategoryDatas.Where(f =>
                (upcates.IndexOf(f.Data.Category) == -1) && f.Data.Category != CategoryAll).ToArray();
            for (int i = delcates.Length - 1; i >= 0; i--)
            {
                foreach (var search in delcates[i].Items)
                {
                    SqlManager.Instance.DeleteT_CATEGORY(search.Data);
                }
                CategoryDatas.Remove(delcates[i]);
                SqlManager.Instance.DeleteT_CATEGORY(delcates[i].Data);
            }
            for (int i = 0; i < upcates.Count; i++)
            {
                var cate = CategoryDatas.FirstOrDefault(f => f.Data.Category == upcates[i]);
                if (cate == null)
                {
                    T_CATEGORY data = new T_CATEGORY()
                    {
                        Category = upcates[i],
                        IsSearchTitle = false,
                        Type = null,
                        SearchText = null,
                        NoSearchText = null,
                        UpCategorySeq = null,
                        Number = 0,
                        RegDate = dt,
                        ModDate = dt,
                    };
                    cate = new CategoryData(SqlManager.Instance.InsertT_CATEGORY(data));
                    CategoryDatas.Add(cate);
                }
                var texts = list.Where(f => f.Category == cate.Data.Category).ToList();
                var delsearchs = cate.Items.Where(f => texts.FirstOrDefault(f2 => f2.SearchText == f.Data.SearchText) == null).ToArray();
                for (int j = delsearchs.Length - 1; j >= 0; j--)
                {
                    cate.Items.Remove(delsearchs[j]);
                    SqlManager.Instance.DeleteT_CATEGORY(delsearchs[j].Data);
                }
                for (int j = 0; j < texts.Count; j++)
                {
                    var text = texts[j];
                    var search = cate.Items.FirstOrDefault(f => f.Data.SearchText == text.SearchText);
                    if (search == null)
                    {
                        T_CATEGORY data = new T_CATEGORY()
                        {
                            Category = null,
                            IsSearchTitle = text.IsSearchTitle,
                            Type = null,
                            SearchText = text.SearchText,
                            NoSearchText = text.NoSearchText,
                            UpCategorySeq = cate.Data.CategorySeq,
                            Number = 0,
                            RegDate = dt,
                            ModDate = dt,
                        };
                        search = new CategoryData(SqlManager.Instance.InsertT_CATEGORY(data))
                        {
                            Count = 0,
                        };
                        //cate.Items.Add(search);
                        CategoryAddItem(cate, search);
                    }
                    else
                    {
                        search.Data.Category = null;
                        search.Data.SearchText = text.SearchText;
                        search.Data.NoSearchText = text.NoSearchText;
                        search.Data.IsSearchTitle = text.IsSearchTitle;
                        search.Data.ModDate = dt;
                        search.Refresh();
                        SqlManager.Instance.UpdateT_CATEGORY(search.Data);
                    }
                }
            }
            if (samelist.Count > 0)
            {
                Global.Instance.Alert(string.Format("중복항목 {0}개를 제거하였습니다.\n{1}", samelist.Count, string.Join(",", samelist.Take(10).ToArray())));
            }
            SelectedTreeViewItem(CategoryDatas.First());
        }

        private void Button_ReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (ArticleDatas.Count > 0)
            {
                ReadArticle(ArticleDatas, true);
                //ListView_Acticle.Items.Refresh();
            }
        }

        private void ReadArticle(IEnumerable<ArticleData> datas, bool? isread)
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
                SqlManager.Instance.DeleteT_ARTICLE(DateTime.Now.AddDays(-Global.Instance.ReadAutoDeleteDay));
            }

            var cate = TreeView_Category.SelectedItem as CategoryData;
            if (cate.Data.UpCategorySeq == null)
            {
                var counts = SqlManager.Instance.SelectCountT_ARTICLE(cate.Items.Select(f => f.Data.CategorySeq));
                foreach (var scate in cate.Items)
                {
                    var count = counts.FirstOrDefault(f => f.CategorySeq == scate.Data.CategorySeq);
                    var newcount = count != null ? count.Count : 0;
                    if (scate.Count != newcount)
                    {
                        scate.Refresh();
                    }
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
                foreach (var cate in folder.Items)
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
                Application.Current.Resources["NoReadColor"] = new SolidColorBrush(Global.Instance.NoReadColor);
                Application.Current.Resources["TitleFontSize"] = Global.Instance.TitleFontSize;
                Application.Current.Resources["ListItemMargin"] = new Thickness()
                {
                    Top = Global.Instance.ListItemMargin,
                    Bottom = Global.Instance.ListItemMargin,
                };

                //var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                //uc.Data.Data = d;
                //CategoryDatas.Add(uc.Data);
            };
        }

        private void TreeView_Category_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue is CategoryData)
            {
                List<T_ARTICLE> uparts = new List<T_ARTICLE>();
                for (int i = ArticleDatas.Count - 1; i >= 0; i--)
                {
                    var art = ArticleDatas[i];
                    if (art.Data.IsRead == true)
                    {
                        art.Data.IsDelete = true;
                        uparts.Add(art.Data);
                    }
                }
                SqlManager.Instance.UpdateT_ARTICLE(uparts);
            }
            (Resources["Menu_CategoryAdd"] as MenuItem).IsEnabled = false;
            (Resources["Menu_CategoryModify"] as MenuItem).IsEnabled = false;
            (Resources["Menu_CategoryDelete"] as MenuItem).IsEnabled = false;
            var data = e.NewValue as CategoryData;
            if (e.NewValue == null)
            {
                return;
            }
            List<ArticleData> arts = new List<ArticleData>();
            if (data != null)
            {
                if (data.Data.Category == CategoryAll)
                {
                    // 전체
                    var list = SqlManager.Instance.SelectT_ARTICLE();

                    var cates = CategoryDatas.SelectMany(f => f.Items.Select(f2 => f2.Data));
                    foreach (var item in list)
                    {
                        var cate = cates.FirstOrDefault(f => f.CategorySeq == item.CategorySeq);
                        if (cate != null)
                        {
                            arts.Add(new ArticleData(item, cate.SearchText));
                        }
                        else
                        {
                            SqlManager.Instance.DeleteT_ARTICLE(item);
                        }
                    }
                }
                else
                {
                    if (data.Data.UpCategorySeq != null)
                    {
                        // 검색어
                        (Resources["Menu_CategoryAdd"] as MenuItem).IsEnabled = true;
                        var list = SqlManager.Instance.SelectT_ARTICLE(data.Data.CategorySeq);
                        foreach (var item in list)
                        {
                            arts.Add(new ArticleData(item, data.Data.SearchText));
                        }
                    }
                    else
                    {
                        // 폴더
                        (Resources["Menu_CategoryAdd"] as MenuItem).IsEnabled = true;
                        var list = SqlManager.Instance.SelectT_ARTICLE(
                            data.Items.Select(f => f.Data.CategorySeq)
                            );
                        foreach (var item in list)
                        {
                            var cate = data.Items.FirstOrDefault(f => f.Data.CategorySeq == item.CategorySeq);
                            arts.Add(new ArticleData(item, cate.Data.SearchText));
                        }
                    }
                    (Resources["Menu_CategoryModify"] as MenuItem).IsEnabled = true;
                    (Resources["Menu_CategoryDelete"] as MenuItem).IsEnabled = true;
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
            if (data != null)
            {
                if (data.Data.UpCategorySeq == null)
                {
                    TextBlock_SelectedCategory.Text = data.Data.Category;
                }
                else
                {
                    var upcate = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == data.Data.UpCategorySeq);
                    TextBlock_SelectedCategory.Text = upcate.Data.Category + " > " + data.Data.SearchText;
                }
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
            foreach (var cate in cates)
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
                        up.Items.Add(data);
                        var count = counts.FirstOrDefault(f => f.CategorySeq == data.Data.CategorySeq);
                        data.Count = count != null ? count.Count : 0;
                    }
                    else
                    {

                    }
                }
                data.Refresh();
            }
            //TreeView_Category.Items.Refresh();
            //var tvi = TreeView_Category.ItemContainerGenerator.ContainerFromItem(CategoryDatas.First())
            //          as TreeViewItem;
            //if (tvi != null)
            //{
            //    tvi.IsSelected = true;
            //}

            Application.Current.Resources["NoReadColor"] = new SolidColorBrush(Global.Instance.NoReadColor);
            Application.Current.Resources["TitleFontSize"] = Global.Instance.TitleFontSize;
            Application.Current.Resources["ListItemMargin"] = new Thickness()
            {
                Top = Global.Instance.ListItemMargin,
                Bottom = Global.Instance.ListItemMargin,
            };

#if DEBUG
#else             
#endif

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
                                            f.Items.FirstOrDefault(f2 => f2.IsSelected == true) != null);
            CategoryData subseled = null;
            if (seled != null)
            {
                seled.IsSelected = false;
                subseled = seled.Items.FirstOrDefault(f2 => f2.IsSelected == true);
                if (subseled != null)
                {
                    subseled.IsSelected = false;
                }
            }
            data.IsSelected = true;
            var upcate = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == data.Data.UpCategorySeq);
            if (upcate != null)
            {
                if (upcate.IsExpanded == false)
                {
                    upcate.IsExpanded = true;
                }
                TreeView_Category.UpdateLayout();

                //var item = TreeView_Category.ItemContainerGenerator.ContainerFromItem(upcate);
                //if (item != null)
                //{
                //    var idx = (item as TreeViewItem).Items.IndexOf(data);
                //    (item as TreeViewItem).BringIntoView(new Rect(100, 100, 100, 100));
                //}
            }
            else
            {
                //var item = TreeView_Category.ItemContainerGenerator.ContainerFromItem(data);
                //if (item != null)
                //{
                //    (item as TreeViewItem).BringIntoView();
                //}
            }
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                //(item.DataContext as CategoryData).View = item;
                item.BringIntoView();
                e.Handled = true;
            }
        }

        private void Button_CategoryAddFolder_Click(object sender, RoutedEventArgs e)
        {
            CategoryAdd uc = new CategoryAdd(null, null, CategoryDatas);
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
            if (data.Data.UpCategorySeq != null)
            {
                data = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == data.Data.UpCategorySeq);
            }
            CategoryAdd uc = new CategoryAdd(null, data.Data.CategorySeq, CategoryDatas);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKAndClicked = (s) => {
                CategoryData before = null;
                foreach (var cate in CategoryDatas)
                {
                    before = cate.Items.FirstOrDefault(f => (f.Data != uc.Data.Data) &&
                    (f.Data.SearchText == uc.Data.Data.SearchText));
                    if (before != null)
                    {
                        Global.Instance.Alert(string.Format("이미 등록된 검색어입니다.\n{0}>{1}", cate.Data.Category, before.Data.SearchText));
                        return;
                    }
                }
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                //data.Items.Add(uc.Data);
                data.Items.Remove(uc.Data);
                CategoryAddItem(uc.UpCategory, uc.Data);
            };
            uc.OnOKClicked = (s) =>
            {
                CategoryData before = null;
                foreach (var cate in CategoryDatas)
                {
                    before = cate.Items.FirstOrDefault(f => (f.Data != uc.Data.Data) &&
                    (f.Data.SearchText == uc.Data.Data.SearchText));
                    if (before != null)
                    {
                        Global.Instance.Alert(string.Format("이미 등록된 검색어입니다.\n{0}>{1}", cate.Data.Category, before.Data.SearchText));
                        SelectedTreeViewItem(before);
                        return;
                    }
                }
                var d = SqlManager.Instance.InsertT_CATEGORY(uc.Data.Data);
                uc.Data.Data = d;
                //data.Items.Add(uc.Data);
                data.Items.Remove(uc.Data);
                CategoryAddItem(uc.UpCategory, uc.Data);
                SelectedTreeViewItem(uc.Data);
                uc.Data.Refresh();
            };
        }

        private static void CategoryAddItem(CategoryData parent, CategoryData data)
        {
            int? pre = null;
            for (int i = 0; i < parent.Items.Count; i++)
            {
                if (string.CompareOrdinal(parent.Items[i].Data.SearchText, data.Data.SearchText) > 0)
                {
                    pre = i;
                    break;
                }
            }
            if (pre != null)
            {
                parent.Items.Insert(pre.Value, data);
            }
            else
            {
                parent.Items.Add(data);
            }
        }

        private void Button_CategoryModify_Click(object sender, RoutedEventArgs e)
        {
            CategoryData data = TreeView_Category.SelectedItem as CategoryData;
            if (data == null)
            {
                Global.Instance.Alert("항목을 선택 후 수정해주세요.");
                return;
            }
            var upcate = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == data.Data.UpCategorySeq);
            CategoryAdd uc = new CategoryAdd(data, data.Data.UpCategorySeq, CategoryDatas);
            PopupWindow window = new PopupWindow(uc);
            uc.OnOKClicked = (s) =>
            {
                if (data.Data.UpCategorySeq == null)
                {
                    var before = CategoryDatas.FirstOrDefault(f => f.Data != uc.Data.Data && f.Data.Category == uc.Data.Data.Category);
                    if (before != null)
                    {
                        Global.Instance.Alert("이미 등록된 폴더명입니다.");
                        SelectedTreeViewItem(before);
                        return;
                    }
                }
                else
                {
                    CategoryData before = null;
                    foreach (var cate in CategoryDatas)
                    {
                        before = cate.Items.FirstOrDefault(f => (f.Data != uc.Data.Data) &&
                        (f.Data.SearchText == uc.Data.Data.SearchText));
                        if (before != null)
                        {
                            Global.Instance.Alert(string.Format("이미 등록된 검색어입니다.\n{0}>{1}", cate.Data.Category, before.Data.SearchText));
                            SelectedTreeViewItem(before);
                            return;
                        }
                    }

                    upcate.Items.Remove(uc.Data);
                    CategoryAddItem(uc.UpCategory, uc.Data);
                    SelectedTreeViewItem(uc.Data);
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
            else if (data.Items.Count > 0)
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
                    var parant = CategoryDatas.FirstOrDefault(f => f.Items.IndexOf(data) > -1);
                    if (parant != null)
                    {
                        parant.Items.Remove(data);
                    }
                }
                SqlManager.Instance.DeleteT_CATEGORY(data.Data);
            };
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Save(Config);
            IsSearchRun = false;
            Searching = false;
            SearchQueue.Clear();

            SqlManager.Instance.DeleteT_ARTICLE(true);

            Global.Instance.WebDownloadManager.Dispose();
            Application.Current.Shutdown();
            System.Environment.Exit(0);
        }

        private void Button_Folder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Global.BasePath);
        }
    }
}

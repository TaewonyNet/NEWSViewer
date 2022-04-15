using NEWSViewer.Compositions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaewonyNet.Common.Interfaces;

namespace NEWSViewer.Controls
{
    /// <summary>
    /// CategoryAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CategoryAdd : UserControl, IWindow
    {

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        public IDelegate OnOKClicked;

        public IDelegate OnOKAndClicked;

        public CategoryData Data { get; set; }

        public int? UpCategorySeq { get; set; }

        public CategoryData UpCategory { get; set; }
        public PopupWindow Window { get; set; }
        public bool? DialogResult { get; set; }

        public ObservableCollection<CategoryData> CategoryDatas { get; private set; }

        public CategoryAdd(CategoryData data, int? upCategory, ObservableCollection<CategoryData> categories)
        {
            InitializeComponent();

            Button_OK.Click += Button_OK_Click;
            Button_OK_AND.Click += Button_OK_AND_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Loaded += CategoryAdd_Loaded;
            UpCategorySeq = upCategory;
            Data = data;
            CategoryDatas = new ObservableCollection<CategoryData>();
            foreach(var cate in categories.Skip(1))
            {
                CategoryDatas.Add(cate);
            }

        }

        private void Button_OK_AND_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Category.Text) && string.IsNullOrEmpty(TextBox_SearchText.Text))
            {
                Global.Instance.Alert("카테고리명 또는 검색어중 하나이상을 입력해야합니다.");
                return;
            }

            Data.Data.Category = TextBox_Category.Text.Trim();
            Data.Data.SearchText = TextBox_SearchText.Text.Trim();
            Data.Data.NoSearchText = TextBox_NoSearchText.Text.Trim();
            Data.Data.IsSearchTitle = CheckBox_SearchTitle.IsChecked.Value;
            Data.Data.ModDate = DateTime.Now;
            if (UpCategorySeq != null)
            {
                Data.Count = 0;
                UpCategory = ComboBox_Category.SelectedItem as CategoryData;
                Data.Data.UpCategorySeq = UpCategory.Data.CategorySeq;
            }

            if (OnOKAndClicked != null)
            {
                OnOKAndClicked.Invoke(this);
            }
            UpCategorySeq = Data.Data.UpCategorySeq;
            Data = null;
            CategoryAdd_Loaded(null, null);
        }

        private void CategoryAdd_Loaded(object sender, RoutedEventArgs e)
        {
            if (Data == null)
            {
                Data = new CategoryData(new T_CATEGORY());
                Data.Data.RegDate = DateTime.Now;
                Data.Data.Number = 0;
                Data.Data.IsSearchTitle = true;
                if (UpCategorySeq != null)
                {
                    Button_OK_AND.Visibility = Visibility.Visible;
                }
            }
            else
            {
            }
            if (UpCategorySeq == null)
            {
                TextBox_Category.SelectAll();
                TextBox_Category.Focus();
                ComboBox_Category.Visibility = Visibility.Collapsed;
                TextBlock_SearchText.Visibility = Visibility.Collapsed;
                TextBox_SearchText.Visibility = Visibility.Collapsed;
                TextBlock_NoSearchText.Visibility = Visibility.Collapsed;
                TextBox_NoSearchText.Visibility = Visibility.Collapsed;
                CheckBox_SearchTitle.Visibility = Visibility.Collapsed;
                TextBox_Category.KeyDown += (s, a) =>
                 {
                     if (a.Key == Key.Enter)
                     {
                         Button_OK_Click(sender, e);
                     }
                 };
            }
            else
            {
                TextBox_SearchText.SelectAll();
                TextBox_SearchText.Focus();
                ComboBox_Category.SelectedItem = CategoryDatas.FirstOrDefault(f => f.Data.CategorySeq == UpCategorySeq);
                TextBox_Category.Visibility = Visibility.Collapsed;
                TextBox_SearchText.KeyDown += (s, a) =>
                {
                    if (a.Key == Key.Enter)
                    {
                        Button_OK_Click(sender, e);
                    }
                };
                TextBox_NoSearchText.KeyDown += (s, a) =>
                {
                    if (a.Key == Key.Enter)
                    {
                        Button_OK_Click(sender, e);
                    }
                };
            }
            TextBox_Category.Text = Data.Data.Category;
            TextBox_SearchText.Text = Data.Data.SearchText;
            TextBox_NoSearchText.Text = Data.Data.NoSearchText;
            CheckBox_SearchTitle.IsChecked = Data.Data.IsSearchTitle;
            ComboBox_Category.ItemsSource = CategoryDatas;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Category.Text) && string.IsNullOrEmpty(TextBox_SearchText.Text))
            {
                Global.Instance.Alert("카테고리명 또는 검색어중 하나이상을 입력해야합니다.");
                return;
            }

            Data.Data.Category = TextBox_Category.Text.Trim();
            Data.Data.SearchText = TextBox_SearchText.Text.Trim();
            Data.Data.NoSearchText = TextBox_NoSearchText.Text.Trim();
            Data.Data.IsSearchTitle = CheckBox_SearchTitle.IsChecked.Value;
            Data.Data.ModDate = DateTime.Now;
            if (UpCategorySeq != null)
            {
                Data.Count = 0;
                //Data.Data.UpCategorySeq = UpCategorySeq;
                UpCategory = ComboBox_Category.SelectedItem as CategoryData;
                Data.Data.UpCategorySeq = UpCategory.Data.CategorySeq;
            }

            DialogResult = true;
            if (OnOKClicked != null)
            {
                OnOKClicked.Invoke(this);
            }
            Window.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
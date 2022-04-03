using NEWSViewer.Compositions;
using System;
using System.Collections.Generic;
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

        public CategoryData Data { get; set; }

        public string UpCategory { get; set; }
        public PopupWindow Window { get; set; }
        public bool? DialogResult { get; set; }

        public CategoryAdd(CategoryData data, string upCategory = "")
        {
            InitializeComponent();

            Button_OK.Click += Button_OK_Click;
            Button_Cancel.Click += Button_Cancel_Click;

            if (data == null)
            {
                data = new CategoryData(new T_CATEGORY());
                data.Data.RegDate = DateTime.Now;
                data.Data.Number = 0;
            }
            else
            {
            }
            if (string.IsNullOrEmpty(upCategory) == true)
            {
                TextBlock_SearchText.Visibility = Visibility.Collapsed;
                TextBox_SearchText.Visibility = Visibility.Collapsed;
                TextBlock_NoSearchText.Visibility = Visibility.Collapsed;
                TextBox_NoSearchText.Visibility = Visibility.Collapsed;
                CheckBox_SearchTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextBlock_Category.Visibility = Visibility.Collapsed;
                TextBox_Category.Visibility = Visibility.Collapsed;
            }
            UpCategory = upCategory;
            Data = data;

            TextBox_Category.Text = Data.Data.Category;
            TextBox_SearchText.Text = Data.Data.SearchText;
            TextBox_NoSearchText.Text = Data.Data.NoSearchText;
            CheckBox_SearchTitle.IsChecked = Data.Data.IsSearchTitle;
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
            if (string.IsNullOrEmpty(UpCategory) == false)
            {
                Data.Count = 0;
                Data.Data.UpCategory = UpCategory;
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

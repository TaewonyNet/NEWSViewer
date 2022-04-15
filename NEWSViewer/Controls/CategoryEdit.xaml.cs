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
    /// CategoryEdit.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CategoryEdit : UserControl, IWindow
    {
        public ObservableCollection<string[]> Data { get; set; }
        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        public IDelegate OnOKClicked;

        public PopupWindow Window { get; set; }
        public bool? DialogResult { get; set; }

        public CategoryEdit(List<string[]> datas)
        {
            InitializeComponent();

            Button_CategoryAdd.Click += Button_CategoryAdd_Click;
            Button_Search.Click += Button_Search_Click;
            TextBox_Search.KeyDown += TextBox_Search_KeyDown;
            Button_OK.Click += Button_OK_Click;
            Button_Cancel.Click += Button_Cancel_Click;
            Data = new ObservableCollection<string[]>();
            foreach (var data in datas)
            {
                Data.Add(data);
            }
            DataGrid_Category.ItemsSource = Data;
        }

        private void TextBox_Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Search_Click(sender, e);
            }
        }

        private void Button_CategoryAdd_Click(object sender, RoutedEventArgs e)
        {
            var cate = new string[] { "", "", "", "1" };
            Data.Add(cate);
            DataGrid_Category.ScrollIntoView(cate);
        }

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox_Search.Text) == false)
            {
                var search = TextBox_Search.Text.Trim();
                var index = DataGrid_Category.SelectedIndex;
                var sel = Data.Skip(index + 1).FirstOrDefault(f =>
                  (f[0].IndexOf(search) > -1) || (f[1].IndexOf(search) > -1) || (f[2].IndexOf(search) > -1));
                if (sel == null)
                {
                    sel = Data.Take(index).FirstOrDefault(f =>
                   (f[0].IndexOf(search) > -1) || (f[1].IndexOf(search) > -1) || (f[2].IndexOf(search) > -1));
                }
                if (sel == null)
                {
                    Global.Instance.Alert("검색어를 찾을 수 없습니다.");
                }
                else
                {
                    DataGrid_Category.SelectedItem = sel;
                    DataGrid_Category.ScrollIntoView(sel);
                }
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
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

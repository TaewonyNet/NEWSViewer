using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NEWSViewer
{
    public class CategoryData
    {
        public T_CATEGORY Data { get; set; }

        public ObservableCollection<CategoryData> Children { get; set; }

        public int? Count { get; set; }

        public CategoryData()
        {
            //Loaded += CategoryData_Loaded;
            Children = new ObservableCollection<CategoryData>();
        }

        private void CategoryData_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (Data != null)
            //{
            //    this.Header = string.IsNullOrEmpty(Data.Category) == false ? Data.Category : Data.SearchText;
            //}
        }

        public CategoryData(T_CATEGORY data) : this()
        {
            Data = data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class CategoryData : NotifyPropertyBase//TreeViewItem 
    {
        public T_CATEGORY Data { get; set; }

        public new ObservableCollection<CategoryData> Items { get; set; }

        public int? Count { get; set; }

        public CategoryData()
        {
            //Loaded += CategoryData_Loaded;
            Items = new ObservableCollection<CategoryData>();
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

        public void Refresh()
        {
            //this.UpdateLayout();
            NotifyPropertyChanged("Data");
            NotifyPropertyChanged("Children");
            NotifyPropertyChanged("Count");
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                NotifyPropertyChanged();
            }
        }
        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyPropertyChanged();
            }
        }
    }
}

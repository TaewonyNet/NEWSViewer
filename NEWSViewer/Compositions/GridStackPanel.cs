using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer.Compositions
{
    public class GridStackPanel : Grid
    {
        public static int GetUpperEmptyRows(DependencyObject obj)
        {
            return (int)obj.GetValue(UpperEmptyRowsProperty);
        }

        public static void SetUpperEmptyRows(DependencyObject obj, int value)
        {
            obj.SetValue(UpperEmptyRowsProperty, value);
        }

        public static readonly DependencyProperty UpperEmptyRowsProperty =
            DependencyProperty.RegisterAttached("UpperEmptyRows", typeof(int), typeof(GridStackPanel), null);



        public GridStackPanel()
        {
            Loaded += GridStackPanel_Loaded;
        }

        private void GridStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            SetChildrenRow();
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            //SetChildrenRow();
            base.OnChildDesiredSizeChanged(child);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            //SetChildrenRow();
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private void SetChildrenRow()
        {
            try
            {
                Dictionary<int, List<UIElement>> list = new Dictionary<int, List<UIElement>>();
                foreach (UIElement element in Children)
                {
                    int column = Converters.ConvertType<int>(element.GetValue(Grid.ColumnProperty));
                    if (list.ContainsKey(column) == false)
                    {
                        list.Add(column, new List<UIElement>());
                    }
                    list[column].Add(element);
                }

                int maxrow = 0;
                foreach (var row in list)
                {
                    int i = 0;
                    foreach (UIElement element in row.Value)
                    {
                        int upper = Converters.ConvertType<int>(element.GetValue(GridStackPanel.UpperEmptyRowsProperty));
                        element.SetValue(Grid.RowProperty, (int)(i + upper));
                        i = i + upper + 1;
                        if (maxrow < i)
                        {
                            maxrow = i;
                        }
                    }
                }

                RowDefinitions.Clear();
                for (int i = 0; i < maxrow; i++)
                {
                    RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}

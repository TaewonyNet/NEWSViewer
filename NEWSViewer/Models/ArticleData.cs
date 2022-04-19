using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class TextTag : FrameworkElement
    {
        public FormattedText FormattedText { get; set; }

        public TextTag(FormattedText text)
        {
            FormattedText = text;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(FormattedText, new Point(0, 0));
            base.OnRender(drawingContext);
        }
    }

    public class ArticleData : NotifyPropertyBase
    {
        public T_ARTICLE Data { get; set; }

        public TextBlock Title { get; set; }

        //public TextTag Title2 { get; set; }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                Title = GetHighlightText(Data.Title, Global.Instance.TitleFontSize);
                //Title2 = new TextTag(GetHighlightText2(Data.Title));
            }
        }

        public void Refresh()
        {
            NotifyPropertyChanged("Data");
            NotifyPropertyChanged("Title");
            NotifyPropertyChanged("SearchText");
        }

        public FormattedText GetHighlightText2(string content)
        {
            FormattedText formattedText = new FormattedText(
                content,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Fonts.SystemTypefaces.First(),
                12,
                new SolidColorBrush(Colors.Black)
                );
            if (Data != null)
            {
                int pos = 0;
                if (string.IsNullOrWhiteSpace(_searchText) == false)
                {
                    var text = GetSearchText(content, _searchText);
                    foreach (var t in text)
                    {
                        if (t.Item2 == true)
                        {
                            formattedText.SetForegroundBrush(
                                new SolidColorBrush(Global.Instance.HighlightColor),
                                pos,
                                t.Item1.Length);
                            formattedText.SetFontWeight(
                                FontWeights.Bold,
                                pos,
                                t.Item1.Length);
                        }
                        else
                        {
                        }
                        pos += t.Item1.Length;
                    }
                }
            }
            return formattedText;
        }

        public TextBlock GetHighlightText(string content, double fontsize = 12)
        {
            TextBlock textBlock = new TextBlock();
            //Binding binding = new Binding
            //{
            //    Source = Application.Current.Resources,
            //    Mode = BindingMode.OneWay,
            //    Path = new PropertyPath("TitleFontSize"),
            //};
            //textBlock.SetBinding(TextBlock.FontSizeProperty, binding);
            textBlock.SetValue(TextBlock.FontSizeProperty, double.Parse(Application.Current.Resources["TitleFontSize"].ToString()));
            if (Data != null) 
            {
                if (string.IsNullOrWhiteSpace(_searchText) == false)
                {
                    var text = GetSearchText(content, _searchText);
                    foreach (var t in text)
                    {
                        if (t.Item2 == true)
                        {
                            textBlock.Inlines.Add(new Run(t.Item1)
                            {
                                FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush(Global.Instance.HighlightColor),
                            });
                        }
                        else
                        {
                            textBlock.Inlines.Add(t.Item1);
                        }
                    }
                }
                else
                {
                    textBlock.Inlines.Add(content);
                }
            }
            return textBlock;
        }

        public static Regex regexreplace = new Regex(@"[^가-힣0-9a-zA-Z ]");

        public static List<Tuple<string, bool>> GetSearchText(string text, string saerch)
        {
            saerch = saerch.Trim();
            //saerch = regexreplace.Replace(saerch, "");
            List<Tuple<string, bool>> result = new List<Tuple<string, bool>>();
            var t = text;
            while (string.IsNullOrEmpty(saerch) == false && t.Length > 0)
            {
                var i = t.IndexOf(saerch);
                if (i > -1)
                {
                    result.Add(new Tuple<string, bool>(t.Remove(i), false));
                    result.Add(new Tuple<string, bool>(saerch, true));
                    t = t.Remove(0, i + saerch.Length);
                }
                else
                {
                    result.Add(new Tuple<string, bool>(t, false));
                    t = string.Empty;
                }
            }
            return result;
        }

        public ArticleData()
        {
            Title = new TextBlock();
        }

        public ArticleData(T_ARTICLE data, string searchText = "") : this()
        {
            Data = data;
            SearchText = searchText;
        }

    }
}

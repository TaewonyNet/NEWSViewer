using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NEWSViewer
{
    public class ArticleData
    {
        public T_ARTICLE Data { get; set; }

        public TextBlock Title { get; set; }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                Title = GetHighlightText(Data.Title);
            }
        }

        public TextBlock GetHighlightText(string content)
        {
            TextBlock textBlock = new TextBlock();
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

        public List<Tuple<string, bool>> GetSearchText(string text, string saerch)
        {
            List<Tuple<string, bool>> result = new List<Tuple<string, bool>>();
            var t = text;
            while (t.Length > 0)
            {
                var i = t.IndexOf(_searchText);
                if (i > -1)
                {
                    result.Add(new Tuple<string, bool>(t.Remove(i), false));
                    result.Add(new Tuple<string, bool>(_searchText, true));
                    t = t.Remove(0, i + _searchText.Length);
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

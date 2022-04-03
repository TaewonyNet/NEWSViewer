using NEWSViewer.Compositions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class Global : SingletonBase<Global>
    {
        public WebDownloadManager WebDownloadManager { get; private set; }

        private Global()
        {

        }

        public static string BasePath { get; set; }

        public async Task Init()
        {
            BasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
            Log.UseLocalApplicationData = true;
            Log.KeepFileOpen = true;
            Log.AutoFlush = false;
            Log.OpenFileFlushTimeout = 60;
#if DEBUG
            Log.KeepFileOpen = false;
            Log.AutoFlush = true;
            Log.OpenFileFlushTimeout = 0;
#endif
            var datapath = System.IO.Path.Combine(BasePath, "data");
            WebDownloadManager = new WebDownloadManager(2, 100, 60, datapath);
        }



        /// <summary>
        /// 팝업 윈도우
        /// </summary>
        public Panel PopupLayer { get; set; }

        /// <summary>
        /// 알림 메세지를 보여준다.
        /// </summary>
        /// <param name="message"></param>
        public Alert Alert(string message)
        {
            if (PopupLayer != null)
            {
                Alert alert = new Alert(message);
                PopupWindow popup = new PopupWindow(alert);
                return alert;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 컴폼 메세지를 보여준다.
        /// </summary>
        /// <param name="message"></param>
        public Confirm Confirm(string message)
        {
            if (PopupLayer != null)
            {
                Confirm alert = new Confirm(message);
                PopupWindow popup = new PopupWindow(alert);
                return alert;
            }
            else
            {
                return null;
            }
        }

        private Color _HighlightColor = Colors.Red;

        public Color HighlightColor
        {
            get { return _HighlightColor; }
            set { _HighlightColor = value; }
        }

        private Color _ArticleColor = Colors.White;

        public Color ArticleColor
        {
            get { return _ArticleColor; }
            set { _ArticleColor = value; }
        }

        private Color _ArticleHighlightColor = (Color)ColorConverter.ConvertFromString("#FFFFA77E");

        public Color ArticleHighlightColor
        {
            get { return _ArticleHighlightColor; }
            set { _ArticleHighlightColor = value; }
        }

        public static string OptionHighlightColor = "HighlightColor";
        public static string OptionArticleColor = "ArticleColor";
        public static string OptionArticleHighlightColor = "ArticleHighlightColor";
    }
}

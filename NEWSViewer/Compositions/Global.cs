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
            Configuration.UseLocalApplicationData = true;
            Configuration.Self = new Configuration();
            var datapath = System.IO.Path.Combine(BasePath, "data");
            var logpath = System.IO.Path.Combine(BasePath, "logs");
            WebDownloadManager = new WebDownloadManager(2, 4, 3, datapath);
            GetOption();

            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((o) => {
                DeleteLog(datapath, logpath);
            }));
        }

        private static void DeleteLog(string datapath, string logpath)
        {
            DateTime deldate = DateTime.Now.AddDays(-Global.Instance.ReadAutoDeleteDay - 3);

            foreach (var path in new[] { datapath, logpath })
            {
                foreach (var file in new System.IO.DirectoryInfo(path).GetFiles())
                {
                    if (file.CreationTime < deldate)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("ClearData data Error {0} {1}", file.Name, ex);
                        }
                    }
                }
            }
            SqlManager.Instance.DeleteT_ARTICLE(DateTime.Now.AddDays(-Global.Instance.ReadAutoDeleteDay));
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

        public void SetOption()
        {
            List<T_OPTION> options = new List<T_OPTION>();
            foreach (var key in OptionKeys)
            {
                string value = null;
                var f = this.GetType().GetField(key);
                var t = f.FieldType;
                value = f.GetValue(this).ToString();
                options.Add(new T_OPTION
                {
                    Key = key,
                    Value = value,
                });
            }
            SqlManager.Instance.InsertT_OPTION(options);
        }

        public void GetOption()
        {
            var options = SqlManager.Instance.SelectT_OPTION();
            if (options == null || options.Count == 0)
            {
                SetOption();
            }
            foreach (var option in options)
            {
                var f = this.GetType().GetField(option.Key);
                if (f != null)
                {
                    var t = f.FieldType;
                    if (t == typeof(Color))
                    {
                        f.SetValue(this, ColorConverter.ConvertFromString(option.Value));
                    }
                    else
                    {
                        f.SetValue(this, Convert.ChangeType(option.Value, t));
                    }
                }
            }
        }

        public string[] OptionKeys = new[] {
            "NoReadColor" ,"ReadColor", "HighlightColor", "CrawlerOnceCount", "CrawlerOnceDay", "ReSearchTimeSec", "ReadAutoDeleteDay", "WebPageCacheSec", "PreviewRead", "TitleFontSize",
            "ListItemMargin"
        };
        public Color NoReadColor = (Color)ColorConverter.ConvertFromString("#E7FFBF");
        public Color ReadColor = Colors.White;
        public Color HighlightColor = Colors.Red;
        public int CrawlerOnceCount = 50;
        public int CrawlerOnceDay = 7;
        public int ReSearchTimeSec = 300;
        public int ReadAutoDeleteDay = 1;
        public int WebPageCacheSec = 60;
        public bool PreviewRead = true;
        public int TitleFontSize = 12;
        public int ListItemMargin = 3;
    }
}

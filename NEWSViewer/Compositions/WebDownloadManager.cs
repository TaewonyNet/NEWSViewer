using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer.Compositions
{
    public class WebDownloadManager : WebDownloadHelper
    {
        public double Cache = TimeSpan.FromSeconds(10).TotalDays;

        public const string url_naver_search = "https://m.search.naver.com/search.naver?where=m_news&query={0}&sort=1&sm=tab_smr&nso=so:dd,p:all,a:all&start={1}";

        public const string url_naver_search_stock = "https://m.search.naver.com/search.naver?where=m_news&query={0}&sm=tab_opt&sort=1&photo=0&field=1&nso=so:dd,p:all&start={1}";

        public WebDownloadManager(int threadcount = 2, int maxaction = 85, int period = 60, string path = "") : base(threadcount, maxaction, period)
        {
            string dir = path;
            if (string.IsNullOrEmpty(dir) == true)
            {
                dir = System.IO.Path.Combine(Environment.CurrentDirectory, "Data");
            }
            if (System.IO.Directory.Exists(dir) == false)
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            DataDirectory = dir;
        }

        public async Task<List<T_ARTICLE>> NaverSearch(string text, int page = 1)
        {
            var url = string.Format(url_naver_search, text, page * 15 + 1);
            var bt = await WebClinetDownloadAsync(url, null, Cache);
            var html = Encoding.UTF8.GetString(bt);
            return GetItems(html);
        }

        public async Task<List<T_ARTICLE>> NaverSearchStock(string text, int page = 1)
        {
            var url = string.Format(url_naver_search_stock, text, page * 15 + 1);
            var bt = await WebClinetDownloadAsync(url, null, Cache);
            var html = Encoding.UTF8.GetString(bt);
            return GetItems(html);
        }

        public List<T_ARTICLE> GetItems(string html)
        {
            List<T_ARTICLE> result = new List<T_ARTICLE>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var section = doc.DocumentNode.SelectSingleNode("//section[@class=\"sc sp_nnews _prs_nws\"]");
            foreach (var news_wrap in section.SelectNodes("//div[@class=\"news_wrap\"]"))
            {
                try
                {
                    T_ARTICLE act = new T_ARTICLE();
                    var info_press = news_wrap.SelectSingleNode(".//a[@class=\"info press\"]");
                    act.Press = HtmlEntity.DeEntitize(info_press.InnerText.Trim());
                    act.PressLink = info_press.Attributes["href"].Value.Trim();
                    var info = news_wrap.SelectSingleNode(".//span[@class=\"info\"]");
                    string datestring = info.InnerText.Trim();
                    var imin = datestring.IndexOf("분");
                    var ihour = datestring.IndexOf("시간");
                    var iday = datestring.IndexOf("일");
                    DateTime date = DateTime.Now;
                    if (imin > -1)
                    {
                        act.InfoTime = date.AddMinutes(-int.Parse(datestring.Remove(imin)));
                    }
                    else if (ihour > -1)
                    {
                        act.InfoTime = date.AddHours(-int.Parse(datestring.Remove(ihour)));
                    }
                    else if (iday > -1)
                    {
                        act.InfoTime = date.AddDays(-int.Parse(datestring.Remove(iday)));
                    }
                    else
                    {
                        act.InfoTime = DateTime.Parse(datestring);
                    }
                    var news_tit = news_wrap.SelectSingleNode(".//a[@class=\"news_tit\"]");
                    act.Link = news_tit.Attributes["href"].Value.Trim();
                    act.Title = HtmlEntity.DeEntitize(news_tit.InnerText.Trim());
                    var news_dsc = news_wrap.SelectSingleNode(".//div[@class=\"news_dsc\"]");
                    var img = news_dsc.SelectSingleNode(".//img[@src]");
                    if (img != null)
                    {
                        act.ImageUrl = img.Attributes["src"].Value.Trim();
                    }
                    act.Description = HtmlEntity.DeEntitize(news_dsc.InnerText.Trim());
                    result.Add(act);
                }
                catch(Exception ex)
                {
                    Log.Error("Error {0}", ex);
                }
            }
            //var sp_page = doc.DocumentNode.SelectSingleNode("//[@class=\"sp_page\"]");
            return result;
        }
    }
}

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Compositions;
using TaewonyNet.Common.Interfaces;
using TaewonyNet.Common.Models;

namespace NEWSViewer.Compositions
{
    public class WebDownloadManager : WebDownloadHelper
    {
        public double Cache = TimeSpan.FromSeconds(60).TotalDays;

        //public const string url_naver_search_desc_m = "https://m.search.naver.com/search.naver?where=m_news&query={0}&sm=tab_opt&sort=1&photo=0&field=1&nso=so:dd,p:all&start={1}";
        //public const string url_naver_search_m = "https://m.search.naver.com/search.naver?where=m_news&query={0}&sort=1&sm=tab_smr&nso=so:dd,p:all,a:all&start={1}";

        //https://search.naver.com/search.naver?where=news&sm=tab_pge&query=%ED%8A%B9%EC%A7%95%EC%A3%BC&sort=1&photo=0&field=1&pd=3&ds=2022.04.01&de=2022.04.05&mynews=0&office_type=0&office_section_code=0&news_office_checked=&nso=so:dd,p:from20220401to20220405,a:t&start=41 네이버 뉴스 검색 제목위주
        //https://search.naver.com/search.naver?where=news&sm=tab_pge&query=%ED%8A%B9%EC%A7%95%EC%A3%BC&sort=1&photo=0&field=0&pd=3&ds=2022.04.01&de=2022.04.05&mynews=0&office_type=0&office_section_code=0&news_office_checked=&nso=so:dd,p:from20220401to20220405,a:t&start=41 네이버 뉴스 검색 

        public string[] file_naver_news_search = {
            "https://search.naver.com/search.naver?where=news&sm=tab_pge&query=",
            "&sort=1&photo=0&field=", "&pd=3&ds=",
            "&de=", "&mynews=0&office_type=0&office_section_code=0&news_office_checked=&office_category=0&service_area=0&nso=so:dd,p:from",
            "to", ",a:t&start=",
        };
        public const string url_naver_news_search = "https://search.naver.com/search.naver?where=news&sm=tab_pge&query={0}&sort=1&photo=0&field={1}&pd=3&ds={2:yyyy.MM.dd}&de={3:yyyy.MM.dd}&mynews=0&office_type=0&office_section_code=0&news_office_checked=&office_category=0&service_area=0&nso=so:dd,p:from{2:yyyyMMdd}to{3:yyyyMMdd},a:t&start={4}";


        public WebDownloadManager(int threadcount, int maxaction, int period, string path, int waittimesec = 0) : base(threadcount, maxaction, period, waittimesec)
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

            OnCacheFileName = new IDelegate<string, string>(GetFileName);
        }

        private string GetFileName(object sender, string args)
        {
            if ((args.IndexOf(file_naver_news_search[1]) > -1) 
                && (args.IndexOf(file_naver_news_search[4]) > -1))
            {
                foreach(var part in file_naver_news_search)
                {
                    args = args.Replace(part, " ");
                }
                return System.IO.Path.Combine(DataDirectory, "file_naver_news_search" + args + ".dat");
            }
            return System.IO.Path.Combine(DataDirectory, System.Web.HttpUtility.UrlEncode(args) + ".dat");
        }

        public async Task<Tuple<List<T_ARTICLE>, int>> NaverNewsSearch(string text, bool titleonly, DateTime dtstart, DateTime dtend, int page = 1)
        {
            var url = string.Format(url_naver_news_search, System.Web.HttpUtility.UrlEncode(text), titleonly ? 1 : 0, dtstart, dtend, (page - 1) * 10 + 1);
            var bt = await WebClinetDownloadAsync(url, null, TimeSpan.FromSeconds(Global.Instance.WebPageCacheSec).TotalDays);
            if (bt != null)
            {
                var html = Encoding.UTF8.GetString(bt);
                return GetItems(html);
            }
            else
            {
                Log.Error("NaverNewsSearch Url:{0} Error:{1}", url, "Download Failed");
                return null;
            }
        }

        public Tuple<List<T_ARTICLE>, int> GetItems(string html)
        {
            int nextpage = 0;
            List<T_ARTICLE> arts = new List<T_ARTICLE>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var section = doc.DocumentNode.SelectSingleNode("//div[@class=\"group_news\"]");
            if (section!= null)
            {
                foreach (var news_wrap in section.SelectNodes(".//div[@class=\"news_area\"]"))
                {
                    try
                    {
                        T_ARTICLE act = new T_ARTICLE();
                        var info_press = news_wrap.SelectSingleNode(".//*[@class=\"info press\"]");
                        act.Press = HtmlEntity.DeEntitize(info_press.InnerText.Trim());
                        var info_press_href = news_wrap.SelectSingleNode(".//*[@class=\"info press\"][@href]");
                        if (info_press_href != null)
                        {
                            act.PressLink = info_press_href.Attributes["href"].Value.Trim();
                        }
                        var info = news_wrap.SelectNodes(".//span[@class=\"info\"]").Last();
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
                        act.Title = HtmlEntity.DeEntitize(news_tit.InnerText.Trim());
                        act.Link = news_tit.Attributes["href"].Value.Trim();
                        var news_dsc = news_wrap.SelectSingleNode(".//div[@class=\"news_dsc\"]");
                        act.Description = HtmlEntity.DeEntitize(news_dsc.InnerText.Trim());
                        var img = news_wrap.SelectSingleNode("//a[@target=\"_blank\"]/img");
                        if (img != null)
                        {
                            act.ImageUrl = img.Attributes["src"].Value.Trim();
                        }
                        arts.Add(act);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error {0}", ex);
                    }
                }
                var pages = doc.DocumentNode.SelectNodes("//div[@class=\"sc_page_inner\"]/a");
                if (pages != null)
                {
                    int page = 0;
                    if (int.TryParse(pages.Last().InnerText, out page) == true)
                    {
                        nextpage = page;
                    }
                }
            }
            return new Tuple<List<T_ARTICLE>, int>(arts, nextpage);
        }
    }
}

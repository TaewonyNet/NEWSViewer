using System;
using System.Collections.Generic;
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
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AwesomeWebClient client = new AwesomeWebClient();
            var bt = client.DownloadData("https://m.search.naver.com/search.naver?where=m_news&query=%ED%8A%B9%EC%A7%95%EC%A3%BC&sort=1&sm=tab_smr&nso=so:dd,p:all,a:all");
            if (bt != null)
            {

            }
        }
    }
}

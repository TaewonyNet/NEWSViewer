using NEWSViewer.Compositions;
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
using TaewonyNet.Common.Interfaces;

namespace NEWSViewer
{
    /// <summary>
    /// Alert.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Alert : UserControl
    {
        PopupWindow window { get; set; }
        /// <summary>
        /// 닫기이벤트
        /// </summary>
        public IDelegate CloseEvent;
        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        public IDelegate OKClicked;
        /// <summary>
        /// 로드
        /// </summary>
        public IDelegate LoadEvent;

        private bool isClose = false;

        public Alert(string message)
        {
            InitializeComponent();

            Loaded += Alert_Loaded;
            Button_OK.Click += Button_OK_Click;
            Button_OK.Focus();

            Label_message.Content = message;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (OKClicked != null)
            {
                OKClicked(this);
            }
            window.Close();
        }

        private void Alert_Loaded(object sender, RoutedEventArgs e)
        {
            if (window == null)
            {
                window = Utilitys.GetParentControl<PopupWindow>(this);
                window.IsOpen = true;
                window.IsModal = true;
                window.ClosingFinished += Window_ClosingFinished;
                if (LoadEvent != null)
                {
                    LoadEvent.Invoke(this);
                }
            }
        }

        private void Window_ClosingFinished(object sender, RoutedEventArgs e)
        {
            if (isClose == false)
            {
                if (CloseEvent != null)
                {
                    CloseEvent.Invoke(this);
                }
                isClose = true;
            }
        }
    }
}

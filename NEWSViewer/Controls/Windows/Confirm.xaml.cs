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
    /// Confirm.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Confirm : UserControl
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

        public bool IsOK = false;

        private bool isClose = false;

        public Confirm(string message)
        {
            InitializeComponent();

            Loaded += Alert_Loaded;
            Button_Yes.Click += Button_OK_Click;
            Button_No.Click += Button_No_Click;

            Label_message.Content = message;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            IsOK = true;
            if (OKClicked != null)
            {
                OKClicked(this);
            }
            window.Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
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
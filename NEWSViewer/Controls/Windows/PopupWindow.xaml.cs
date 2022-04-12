using MahApps.Metro.SimpleChildWindow;
using NEWSViewer.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// PopupWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PopupWindow : ChildWindow
    {
        public UserControl Page
        {
            get { return (UserControl)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register("Page", typeof(UserControl), typeof(PopupWindow), new UIPropertyMetadata(new PropertyChangedCallback(PageChanged)));

        private static void PageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is PopupWindow)
                && (e.NewValue != null))
            {
                PopupWindow window = d as PopupWindow;
                //window.Page = (UserControl)e.NewValue;
            }
        }
        /// <summary>
        /// 닫기이벤트
        /// </summary>
        public IDelegate OnCloseEvent;
        /// <summary>
        /// 로드
        /// </summary>
        public IDelegate OnLoadEvent;

        public Panel Panel { get; set; }

        public bool isClosed = false;

        public new string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
                base.Title = value;
            }
        }

        public static new readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PopupWindow), new UIPropertyMetadata(new PropertyChangedCallback(TitleChanged)));

        public static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d is PopupWindow)
                && (e.NewValue != null))
            {
                PopupWindow window = d as PopupWindow;
                if ((e.NewValue == null)
                    || (string.IsNullOrEmpty(e.NewValue.ToString()) == true))
                {
                    window.Title = Application.Current.MainWindow.Title;
                }
                else
                {
                    window.Title = e.NewValue.ToString();
                }
            }
        }

        public PopupWindow(UserControl uc) : this(uc, Global.Instance.PopupLayer)
        {

        }

        public PopupWindow(UserControl uc, Panel panel)
        {
            InitializeComponent();
            Loaded += PopupWindow_Loaded;
            ClosingFinished += PopupWindow_ClosingFinished;

            Page = uc;
            Panel = panel;
            Panel.Children.Add(this);
        }

        private void PopupWindow_ClosingFinished(object sender, RoutedEventArgs e)
        {
            if (isClosed == false)
            {
                if (OnCloseEvent != null)
                {
                    OnCloseEvent.Invoke(Page);
                }
                Panel.Children.Remove(this);
                isClosed = true;
            }
        }

        public override void EndInit()
        {
            base.EndInit();
        }

        private void PopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (OnLoadEvent != null)
            {
                OnLoadEvent.Invoke(Page);
            }
            ContentsControl_Main.Content = Page;

            Page.Loaded += Page_Loaded;
            Page.Unloaded += Page_Unloaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is IWindow)
            {
                var window = sender as IWindow;
                window.Window = this;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}

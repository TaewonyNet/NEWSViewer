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

namespace NEWSViewer.Controls
{
    /// <summary>
    /// OptionSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OptionSetting : UserControl, IWindow
    {

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        public IDelegate OnOKClicked;

        Global global { get; set; }

        public OptionSetting()
        {
            InitializeComponent();

            Button_OK.Click += Button_OK_Click;
            Button_Cancel.Click += Button_Cancel_Click;

            global = Global.Instance;

            NumericUpDown_CrawlerMaxaction.Value = global.CrawlerMaxaction;
            NumericUpDown_CrawlerPeriod.Value = global.CrawlerPeriod;
            ColorPicker_NoReadColor.SelectedColor = global.NoReadColor;
            ColorPicker_ReadColor.SelectedColor = global.ReadColor;
            ColorPicker_HighlightColor.SelectedColor = global.HighlightColor;
            NumericUpDown_CrawlerOnceCount.Value = global.CrawlerOnceCount;
            NumericUpDown_CrawlerOnceDay.Value = global.CrawlerOnceDay;
            NumericUpDown_ReSearchTimeSec.Value = global.ReSearchTimeSec;
            NumericUpDown_ReadAutoDeleteDay.Value = global.ReadAutoDeleteDay;
            NumericUpDown_WebPageCacheSec.Value = global.WebPageCacheSec;
            NumericUpDown_TitleFontSize.Value = global.TitleFontSize;
            NumericUpDown_ListItemMargin.Value = global.ListItemMargin;
            CheckBox_PreviewRead.IsChecked = global.PreviewRead;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            global.CrawlerMaxaction = (int)NumericUpDown_CrawlerMaxaction.Value;
            global.CrawlerPeriod = (int)NumericUpDown_CrawlerPeriod.Value;
            global.NoReadColor = ColorPicker_NoReadColor.SelectedColor.Value;
            global.ReadColor = ColorPicker_ReadColor.SelectedColor.Value;
            global.HighlightColor= ColorPicker_HighlightColor.SelectedColor.Value;
            global.CrawlerOnceCount = (int)NumericUpDown_CrawlerOnceCount.Value.Value;
            global.CrawlerOnceDay = (int)NumericUpDown_CrawlerOnceDay.Value.Value;
            global.ReSearchTimeSec = (int)NumericUpDown_ReSearchTimeSec.Value.Value;
            global.ReadAutoDeleteDay = (int)NumericUpDown_ReadAutoDeleteDay.Value.Value;
            global.WebPageCacheSec = (int)NumericUpDown_WebPageCacheSec.Value.Value;
            global.TitleFontSize = (int)NumericUpDown_TitleFontSize.Value.Value;
            global.ListItemMargin = (int)NumericUpDown_ListItemMargin.Value.Value;
            global.PreviewRead = CheckBox_PreviewRead.IsChecked.Value;
            global.SetOption();

            DialogResult = true;
            if (OnOKClicked != null)
            {
                OnOKClicked.Invoke(this);
            }

            Window.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        public PopupWindow Window { get; set; }
        public bool? DialogResult { get; set; }
    }
}

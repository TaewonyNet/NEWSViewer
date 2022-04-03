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

        public OptionSetting()
        {
            InitializeComponent();
        }

        public PopupWindow Window { get; set; }
        public bool? DialogResult { get; set; }
    }
}

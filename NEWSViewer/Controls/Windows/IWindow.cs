using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWSViewer.Controls
{
    interface IWindow
    {
        PopupWindow Window { get; set; }

        bool? DialogResult { get; set; }
    }
}

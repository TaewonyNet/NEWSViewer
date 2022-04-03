using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer
{
    public class Configuration : Configuration<Config>
    {
        public Configuration() : base()
        {
        }
    }

    public class Config
    {

        /// <summary>
        /// Width
        /// </summary>
        [XmlElement("Width")]
        public double Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [XmlElement("Height")]
        public double Height { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [XmlElement("WindowState")]
        public int WindowState { get; set; }

        /// <summary>
        /// ColumnDefinition_With
        /// </summary>
        [XmlElement("ColumnDefinition_With")]
        public double ColumnDefinition_With { get; set; }

        /// <summary>
        /// RowDefinition_Height
        /// </summary>
        [XmlElement("RowDefinition_Height")]
        public double RowDefinition_Height { get; set; }

    }
}

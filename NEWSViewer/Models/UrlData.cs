using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Interfaces;

namespace NEWSViewer
{
    public class UrlData
    {
        public IDelegate<Tuple<UrlData, byte[]>> Delegate { get; set; }

        public object Data { get; set; }

        public byte[] FormData { get; set; }

        public string Url { get; set; }

        public string FileName { get; set; }

        public Dictionary<string, string> Header { get; set; }
    }
}

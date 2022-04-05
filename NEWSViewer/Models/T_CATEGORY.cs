using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWSViewer
{
    public class T_CATEGORY
    {
        /// <summary>
        /// 카테고리 키
        /// </summary>
        public int CategorySeq { get; set; }

        public string Category { get; set; }

        public bool IsSearchTitle { get; set; }

        public string Type { get; set; }

        public string SearchText { get; set; }

        public string NoSearchText { get; set; }

        public int? UpCategorySeq { get; set; }

        public int Number { get; set; }

        public DateTime RegDate { get; set; }

        public DateTime ModDate { get; set; }
    }
}

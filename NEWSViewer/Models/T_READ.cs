using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWSViewer
{
    public class T_READ
    {
        /// <summary>
        /// 링크(키)
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 읽기여부
        /// </summary>
        public bool IsRead { get; set; }
        /// <summary>
        /// 삭제여부
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 등록날짜
        /// </summary>
        public DateTime RegDate { get; set; }
        /// <summary>
        /// 수정날짜
        /// </summary>
        public DateTime ModDate { get; set; }
    }
}

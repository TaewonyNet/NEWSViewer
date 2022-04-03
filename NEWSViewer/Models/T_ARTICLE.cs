using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEWSViewer
{
    public class T_ARTICLE
    {
        /// <summary>
        /// 검색위치
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 카테고리 키
        /// </summary>
        public int CategorySeq { get; set; }
        /// <summary>
        /// 언론사
        /// </summary>
        public string Press { get; set; }
        /// <summary>
        /// 언론사링크
        /// </summary>
        public string PressLink { get; set; }
        /// <summary>
        /// 날짜
        /// </summary>
        public DateTime InfoTime { get; set; }
        /// <summary>
        /// 제목
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 링크(키)
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 내용
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 이미지
        /// </summary>
        public string ImageUrl { get; set; }
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
        /// 읽은날짜
        /// </summary>
        public DateTime? ReadDate { get; set; }
        /// <summary>
        /// 수정날짜
        /// </summary>
        public DateTime ModDate { get; set; }
    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlHTML.Model
{
   public class ImgForSCGS
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 图片类目
        /// </summary>
        public string UrlCat { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgSrc { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CTime { get; set; }
        /// <summary>
        /// 排序值
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 图片说明
        /// </summary>
        public string ImgDes { get; set; }
    }
}

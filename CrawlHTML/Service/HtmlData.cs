using Common;
using CrawlHTML.Model;
using Dapper;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlHTML.Service
{
    /// <summary>
    /// 图片写入数据库
    /// </summary>
   public class HtmlData
    {
        static Logger logger = LogManager.GetLogger("素材公社");
        public static ConcurrentQueue<ImgForSCGS> copyNeedDb = new ConcurrentQueue<ImgForSCGS>();

        public static void Do()
        {
            {
                Thread t = new Thread(() => {
                    List<ImgForSCGS> list = new List<ImgForSCGS>();
                    while (true) {
                        if (copyNeedDb.TryDequeue(out ImgForSCGS item)) {
                            list.Add(item);
                            try {
                                if (list.Count % 1000 == 0) {
                                    using (var dbMysql = SqlHelper.Instance.DataBaseConn(SqlHelper.DBNameEnumType.Web)) {
                                        dbMysql.Execute(@"insert ignore into imgforscgs (UrlCat,ImgSrc,CTime,Sort,ImgDes) values(@UrlCat,@ImgSrc,now(),@Sort,@ImgDes)",list);
                                    }
                                    list.Clear();
                                }
                            } catch (Exception ex) {

                                logger.Trace($"图片添加失败：{ex}");
                            }
                        } else {
                            if (list.Count != 0) {
                                using (var dbMysql = SqlHelper.Instance.DataBaseConn(SqlHelper.DBNameEnumType.Web)) {
                                    dbMysql.Execute(@"insert ignore into imgforscgs (UrlCat,ImgSrc,CTime,Sort,ImgDes) values(@UrlCat,@ImgSrc,now(),@Sort,@ImgDes)",list);
                                }
                                list.Clear();
                                logger.Trace($"图片写入数据库完成");
                            }
                            Thread.Sleep(30 * 1000);
                        }
                    }
                }) {
                    IsBackground = true
                };
                t.Start();
            }
        }
    }
}

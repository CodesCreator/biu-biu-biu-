using Common;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrawlHTML.Service
{
    /// <summary>
    /// 素材公社图片
    /// </summary>
    public class CrawlPage
    {
        static Logger logger = LogManager.GetLogger("素材公社图片");
        public static ConcurrentQueue<PageRequestObj> requestQuery = new ConcurrentQueue<PageRequestObj>();
        public static int DataDownCount = 0;
        private static Timer newTimer = null;
        /// <summary>
        /// 数据库操作的数据
        /// </summary>
        private static readonly string FromDes = "素材公社图片";
        /// <summary>
        /// 执行间隔
        /// </summary>
        public static readonly int dueTime = 3600 * 1000;

        public static bool DoneOnce = false;
        private static ManualResetEvent createEvent = new ManualResetEvent(false);
        public static int Page = 100;

        public static void Start()
        {
            //读取默认的页数
            using (var httpClient=new HttpClient()) {
                string url = "https://www.tooopen.com/img";
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(HtmlCode(url));
                var root = doc.DocumentNode;
                //var catelist = root.SelectNodes("//ul[@class='c-fix']//li//a");
                var catelist = root.SelectNodes("//div[@class='c-fix list-com']//div//a");
                if (catelist != null && catelist.Count != 0) {
                    int i = 0; int sort = 0;
                    foreach (var item in catelist) {
                        i++; sort++;
                        HtmlDocument docu = new HtmlDocument();
                                    docu.LoadHtml(item.OuterHtml);
                                    var nod = docu.DocumentNode;
                                    HtmlData.copyNeedDb.Enqueue(new Model.ImgForSCGS {
                                        UrlCat=sort.ToString(),
                                        //UrlCat= rot.SelectSingleNode("//ul[@class='c-fix']//li//a").InnerText,
                                        ImgSrc = nod.SelectSingleNode("//img")?.Attributes["src"]?.Value,
                                    Sort =sort,
                                        ImgDes= nod.SelectSingleNode("//img")?.Attributes["alt"]?.Value
                                });

                        //string src = item.Attributes["href"].Value;
                        //if (src.Contains("_")) {
                        //    HtmlDocument docs = new HtmlDocument();
                        //    docs.LoadHtml(HtmlCode(src));
                        //    var rot = docs.DocumentNode;
                        //    var itemlist = rot.SelectNodes("//a[@class='pic']");
                        //    if (itemlist != null && itemlist.Count != 0) {
                        //        foreach (var it in itemlist) {
                        //            sort++;
                        //            HtmlDocument docu = new HtmlDocument();
                        //            docu.LoadHtml(it.OuterHtml);
                        //            var nod = docu.DocumentNode;
                        //            HtmlData.copyNeedDb.Enqueue(new Model.ImgForSCGS {
                        //                UrlCat= rot.SelectSingleNode("//ul[@class='c-fix']//li//a").InnerText,
                        //                ImgSrc = nod.SelectSingleNode("//img")?.Attributes["src"]?.Value,
                        //            Sort =sort,
                        //                ImgDes= nod.SelectSingleNode("//img")?.Attributes["alt"]?.Value
                        //        });
                        //        }
                        //    }
                        //} else {
                        //    continue;
                        //}
                    }
                }
            }
            //每一个小时产生一次请求
            newTimer = new Timer(o => {
                logger.Trace($"开始，图片采集");
                if (requestQuery.Count == 0) {
                    Page = 100;
                    for (int i = 1; i < Page; i++) {
                        requestQuery.Enqueue(new PageRequestObj(FromDes,1,1) {
                            Page = i
                        });
                    }
                    createEvent.Set();
                } else {
                    logger.Error($"上一次的采集未完成");
                }
            },null,1000,dueTime);
            {
                Thread t = new Thread(() => {
                    createEvent.WaitOne();
                    while (true) {
                        s:
                        if (DataDownCount > 30) {
                            Thread.Sleep(1000);
                            goto s;
                        }
                        if (requestQuery.TryDequeue(out PageRequestObj obj)) {
                            Interlocked.Increment(ref DataDownCount);
                            var dic = new Dictionary<string,string>();
                            dic.Add("page",obj.Page.ToString());
                            CJClient.NOCookieClient.PostAsync($"https://www.tooopen.com/img",CJClient.Content(dic)).ContinueWith((x,y) => {
                                if (x.Status == TaskStatus.RanToCompletion && x.Result.StatusCode == System.Net.HttpStatusCode.OK) {
                                    x.Result.Content.ReadAsStringAsync().ContinueWith((xx,yy) => {
                                        var jsonResult = xx.Result;
                                        //判断是否请求成功,是否有数据
                                        if (xx.Status == TaskStatus.RanToCompletion) {
                                            var jarray = JArray.Parse(jsonResult);
                                            if (jarray.Count > 0) {
                                                try {
                                                    foreach (var item in jarray) {
                                                        HtmlData.copyNeedDb.Enqueue(new Model.ImgForSCGS {
                                                            UrlCat = item["UrlCat"]?.Value<string>(),
                                                            ImgSrc = item["ImgSrc"]?.Value<string>(),
                                                            Sort = item["Sort"].Value<int>(),
                                                            ImgDes = item["ImgDes"]?.Value<string>()
                                                        });
                                                    }
                                                } catch (Exception ex) {
                                                    ErrorUtils.ErrorException(ex,"素材公社图片采集,数据解析失败:\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(yy),logger);
                                                } finally {
                                                    Interlocked.Decrement(ref DataDownCount);
                                                }
                                            } else {
                                                logger.Trace("素材公社图片采集,最后一页:" + Newtonsoft.Json.JsonConvert.SerializeObject(yy));
                                            }
                                        } else {
                                            Interlocked.Decrement(ref DataDownCount);
                                            logger.Trace("素材公社图片采集,请求完成,数据解析,xx.Status:",xx.Status);
                                            ErrorUtils.ErrorException(xx.Exception,"素材公社图片采集,请求完成,数据解析:\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(yy),logger);
                                        }
                                    },y);
                                }
                            },dic);
                        } else {
                            Thread.Sleep(500 * 1000);
                        }
                    }
                });
                t.IsBackground = true;
                t.Start();
            }
        }
        /// <summary>
        /// 解析页面
        /// </summary>
        public static string HtmlCode(string url)
        {
            string htmlCode;
            Uri uri = new Uri(url);
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.ContentType = "application/json";
            myHttpWebRequest.Timeout = 50000;
            myHttpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36";
            myHttpWebRequest.Method = "GET";
            myHttpWebRequest.Headers.Add("Accept-Encoding","gzip, deflate");
            HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
            //获取目标网站的编码格式
            string contentype = response.Headers["Content-Type"];
            Regex regex = new Regex("charset\\s*=\\s*[\\W]?\\s*([\\w-]+)",RegexOptions.IgnoreCase);
            if (response.ContentEncoding.ToLower() == "gzip") {
                using (Stream streamReceive = response.GetResponseStream()) {
                    using (var zipStream = new System.IO.Compression.GZipStream(streamReceive,System.IO.Compression.CompressionMode.Decompress)) {
                        //using (StreamReader sr = new StreamReader(zipStream,Encoding.Default)) {
                        //匹配编码格式
                        if (regex.IsMatch(contentype)) {
                            Encoding ending = Encoding.GetEncoding(regex.Match(contentype).Groups[1].Value.Trim());
                            using (StreamReader sr = new System.IO.StreamReader(zipStream,ending)) {
                                htmlCode = sr.ReadToEnd();
                            }
                        } else {
                            using (StreamReader sr = new System.IO.StreamReader(zipStream,Encoding.UTF8)) {
                                htmlCode = sr.ReadToEnd();
                            }
                        }
                        //}
                    }
                }
            } else {
                using (Stream streamReceive = response.GetResponseStream()) {
                    using (StreamReader sr = new StreamReader(streamReceive,Encoding.UTF8)) {
                        htmlCode = sr.ReadToEnd();
                    }
                }
            }
            return htmlCode;
        }
    }

    public class PageRequestObj : BaseRequestObj
    {
        public PageRequestObj(string fromDes,int catSort,int imgSort) : base(fromDes,catSort,imgSort)
        {
        }
        public int Page { get; set; }
    }

    public class BaseRequestObj
    {
        public BaseRequestObj(string fromDes, int catSort,int imgSort)
        {
            FromDes = fromDes;
            UrlCat = catSort;
            ImgSrc = imgSort;
        }
        public int UrlCat { get; set; }
        public int ImgSrc { get; set; }
        public string FromDes { get; set; }
    }
}

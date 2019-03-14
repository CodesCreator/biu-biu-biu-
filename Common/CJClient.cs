using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
   public static class CJClient
    {
        private static HttpClient __c;
        private static object __l = new object();

        private static long __count;

        /// <summary>
        /// 复用连接需要
        /// </summary>
        public static HttpClient NOCookieClient {
            //每一万个请求获取一个新的队形
            get {
                if (__c == null || __count % 10000 == 0) {
                    lock (__l) {
                        if (__c == null || __count % 10000 == 0) {
                            var handler = new HttpClientHandler() {
                                AutomaticDecompression = DecompressionMethods.GZip,
                                AllowAutoRedirect = false,
                                UseCookies = false,
                            };
                            var c = new HttpClient(handler);
                            c.DefaultRequestHeaders.Connection.Add("keep-alive");
                            c.Timeout = TimeSpan.FromSeconds(20);
                            __c = c;
                            Interlocked.Increment(ref __count);
                            return __c;
                        } else {
                            Interlocked.Increment(ref __count);
                            return __c;
                        }
                    }
                } else {
                    Interlocked.Increment(ref __count);
                    return __c;
                }
            }
        }

        private static HttpClient __cc;
        /// <summary>
        /// 复用连接需要
        /// </summary>
        public static HttpClient AutoRedirectClient {
            //每一万个请求获取一个新的队形
            get {
                if (__cc == null || __count % 10000 == 0) {
                    lock (__l) {
                        if (__cc == null || __count % 10000 == 0) {
                            var handler = new HttpClientHandler() {
                                AutomaticDecompression = DecompressionMethods.GZip,
                                AllowAutoRedirect = true,
                                UseCookies = false,
                            };
                            var c = new HttpClient(handler);
                            c.DefaultRequestHeaders.Connection.Add("keep-alive");
                            c.Timeout = TimeSpan.FromSeconds(20);
                            __cc = c;
                            Interlocked.Increment(ref __count);
                            return __cc;
                        } else {
                            Interlocked.Increment(ref __count);
                            return __cc;
                        }
                    }
                } else {
                    Interlocked.Increment(ref __count);
                    return __cc;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static CookieContainer CookieClientCookieContainer = new CookieContainer();

        private static HttpClient __ccc;
        /// <summary>
        /// 
        /// </summary>
        public static HttpClient CookieClient {
            //每一万个请求获取一个新的队形
            get {
                if (__ccc == null || __count % 10000 == 0) {
                    lock (__l) {
                        if (__ccc == null || __count % 10000 == 0) {
                            var handler = new HttpClientHandler() {
                                AutomaticDecompression = DecompressionMethods.GZip,
                                AllowAutoRedirect = false,
                                UseCookies = true,
                                CookieContainer = CookieClientCookieContainer,
                            };
                            var c = new HttpClient(handler);
                            c.DefaultRequestHeaders.Connection.Add("keep-alive");
                            c.Timeout = TimeSpan.FromSeconds(20);
                            __ccc = c;
                            Interlocked.Increment(ref __count);
                            return __ccc;
                        } else {
                            Interlocked.Increment(ref __count);
                            return __ccc;
                        }
                    }
                } else {
                    Interlocked.Increment(ref __count);
                    return __ccc;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static HttpContent Content(Dictionary<string,string> dic)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(dic);
            return content;
        }
    }
}

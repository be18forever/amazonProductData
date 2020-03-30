using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon
{
   public class Common
    {

        /// <summary>
        /// 微软为我们提供两个关于HTTP请求HttpWebRequest，HttpWebResponse对象，方便我们发送请求获取数据。C# HTTP请求代码：
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string RequestAction(RequestOptions options)
        {
            string result = string.Empty;
            IWebProxy proxy = GetProxy();
            var request = (HttpWebRequest)WebRequest.Create(options.Uri);
            request.Accept = options.Accept;
            //在使用curl做POST的时候, 当要POST的数据大于1024字节的时候, curl并不会直接就发起POST请求, 而是会分为俩步,
            //发送一个请求, 包含一个Expect: 100 -continue, 询问Server使用愿意接受数据
            //接收到Server返回的100 - continue应答以后, 才把数据POST给Server
            //并不是所有的Server都会正确应答100 -continue, 比如lighttpd, 就会返回417 “Expectation Failed”, 则会造成逻辑出错.
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
            if (!string.IsNullOrEmpty(options.XHRParams)) { request.AllowWriteStreamBuffering = true; } else { request.AllowWriteStreamBuffering = false; }; //禁止缓冲加快载入速度
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
            request.ContentType = options.ContentType;//定义文档类型及编码
            request.AllowAutoRedirect = options.AllowAutoRedirect;//禁止自动跳转
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36";//设置User-Agent，伪装成Google Chrome浏览器
            request.Timeout = options.Timeout;//定义请求超时时间为5秒
            request.KeepAlive = options.KeepAlive;//启用长连接
            if (!string.IsNullOrEmpty(options.Referer)) request.Referer = options.Referer;//返回上一级历史链接
            request.Method = options.Method;//定义请求方式为GET
            if (proxy != null) request.Proxy = proxy;//设置代理服务器IP，伪装请求地址
            if (!string.IsNullOrEmpty(options.RequestCookies)) request.Headers[HttpRequestHeader.Cookie] = options.RequestCookies;
            request.ServicePoint.ConnectionLimit = options.ConnectionLimit;//定义最大连接数
            if (options.WebHeader != null && options.WebHeader.Count > 0) request.Headers.Add(options.WebHeader);//添加头部信息
            if (!string.IsNullOrEmpty(options.XHRParams))//如果是POST请求，加入POST数据
            {
                byte[] buffer = Encoding.UTF8.GetBytes(options.XHRParams);
                if (buffer != null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                ////获取请求响应
                //foreach (Cookie cookie in response.Cookies)
                //    options.CookiesContainer.Add(cookie);//将Cookie加入容器，保存登录状态
                if (response.ContentEncoding.ToLower().Contains("gzip"))//解压
                {
                    using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))//解压
                {
                    using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    using (Stream stream = response.GetResponseStream())//原始
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            request.Abort();
            return result;
        }
        /// <summary>
        /// 微软NET框架也为了我们提供一个使用代理IP 的System.Net.WebProxy对象
        /// </summary>
        /// <returns></returns>
        private static System.Net.WebProxy GetProxy()
        {
            System.Net.WebProxy webProxy = null;
            try
            {
                // 代理链接地址加端口
                string proxyHost = "192.168.1.1";
                string proxyPort = "9030";

                // 代理身份验证的帐号跟密码
                //string proxyUser = "xxx";
                //string proxyPass = "xxx";

                // 设置代理服务器
                webProxy = new System.Net.WebProxy();
                // 设置代理地址加端口
                webProxy.Address = new Uri(string.Format("{0}:{1}", proxyHost, proxyPort));
                // 如果只是设置代理IP加端口，例如192.168.1.1:80，这里直接注释该段代码，则不需要设置提交给代理服务器进行身份验证的帐号跟密码。
                //webProxy.Credentials = new System.Net.NetworkCredential(proxyUser, proxyPass);
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取代理信息异常", DateTime.Now.ToString(), ex.Message);
            }
            return webProxy;
        }
        /// <summary>
        /// 自定义传参对象，当然无论传入或者传出的对象都是你们根据自己实际业务需求定义的
        /// </summary>
        public class RequestOptions
        {
            /// <summary>
            /// 请求方式，GET或POST
            /// </summary>
            public string Method { get; set; }
            /// <summary>
            /// URL
            /// </summary>
            public Uri Uri { get; set; }
            /// <summary>
            /// 上一级历史记录链接
            /// </summary>
            public string Referer { get; set; }
            /// <summary>
            /// 超时时间（毫秒）
            /// </summary>
            public int Timeout = 15000;
            /// <summary>
            /// 启用长连接
            /// </summary>
            public bool KeepAlive = true;
            /// <summary>
            /// 禁止自动跳转
            /// </summary>
            public bool AllowAutoRedirect = false;
            /// <summary>
            /// 定义最大连接数
            /// </summary>
            public int ConnectionLimit = int.MaxValue;
            /// <summary>
            /// 请求次数
            /// </summary>
            public int RequestNum = 3;
            /// <summary>
            /// 可通过文件上传提交的文件类型
            /// </summary>
            public string Accept = "*/*";
            /// <summary>
            /// 内容类型
            /// </summary>
            public string ContentType = "application/x-www-form-urlencoded";
            /// <summary>
            /// 实例化头部信息
            /// </summary>
            private WebHeaderCollection header = new WebHeaderCollection();
            /// <summary>
            /// 头部信息
            /// </summary>
            public WebHeaderCollection WebHeader
            {
                get { return header; }
                set { header = value; }
            }
            /// <summary>
            /// 定义请求Cookie字符串
            /// </summary>
            public string RequestCookies { get; set; }
            /// <summary>
            /// 异步参数数据
            /// </summary>
            public string XHRParams { get; set; }
        }
        //----------------------------------------------------------------------------------------








    }
}

using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Amazon.Common;

namespace Amazon
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var html = GetWebClient("https://www.baidu.com");
           var httpHelper = new HttpHelper();
           var html= httpHelper.getUrlRespHtml_multiTry("https://www.amazon.com/Best-Sellers/zgbs");

            //*[@id="zg_browseRoot"]/ul/li[2]/a
            List<string> catalog = new List<string>();
            //*[@id="zg_browseRoot"]/ul/li[2]/a
            List<string> productUrl = new List<string>();

            HtmlDocument htmlDoc = htmlToHtmlDoc(html);

            var htmlnodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"zg_browseRoot\"]/ul/li/a");
            foreach (var item in htmlnodes)
            {
                var link = item.GetAttributes("href").FirstOrDefault().Value;
                catalog.Add(link);
            }

            //具体产品链接
            var productHtml = httpHelper.getUrlRespHtml_multiTry(catalog[3]);

            HtmlDocument productListDoc = htmlToHtmlDoc(productHtml);

            var productNodes = productListDoc.DocumentNode.SelectNodes("//*[@id='zg-ordered-list']/li/span/div/span/a[1]");
            foreach (var productLink in productNodes)
            {
                var link2 = "https://www.amazon.com/"+productLink.GetAttributes("href").FirstOrDefault().Value;
                productUrl.Add(link2);
            }

            //具体页面
            var productDetail = httpHelper.getUrlRespHtml_multiTry(productUrl[0]);

            



            var b = 2;
        }

        /// <summary>
        /// 获取网页源代码方法
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="charSet">指定编码，如果为空，则自动判断</param>
        /// <param name="out_str">网页源代码</param>
        public static string GetHtml(string url, string charSet)
        {
            string strWebData = string.Empty;
            try
            {
                WebClient myWebClient = new WebClient(); //创建WebClient实例
                byte[] myDataBuffer = myWebClient.DownloadData(url);
                strWebData = System.Text.Encoding.Default.GetString(myDataBuffer);
                //获取网页字符编码描述信息 
                if (string.IsNullOrEmpty(charSet))
                {
                    Match charSetMatch = Regex.Match(strWebData, "<meta([^>]*)charset=(\")?(.*)?\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string webCharSet = charSetMatch.Groups[3].Value.Trim().ToLower();
                    if (webCharSet != "gb2312")
                    {
                        webCharSet = "utf-8";
                    }
                    if (System.Text.Encoding.GetEncoding(webCharSet) != System.Text.Encoding.Default)
                    {
                        strWebData = System.Text.Encoding.GetEncoding(webCharSet).GetString(myDataBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return strWebData;
        }
        private string GetWebClient(string url)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Accept-Encoding", "gzip, deflate");
            string sUrl = url;
            byte[] byteArray = client.DownloadData(sUrl);

            // 处理　gzip
            string sContentEncoding = client.ResponseHeaders["Content-Encoding"];
            if (sContentEncoding == "gzip")
            {
             
                MemoryStream ms = new MemoryStream(byteArray);
                MemoryStream msTemp = new MemoryStream();
                int count = 0;
                GZipStream gzip =
                new GZipStream(ms, CompressionMode.Decompress);
                byte[] buf = new byte[1000];

                while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                {
                    msTemp.Write(buf, 0, count);
                }

                byteArray = msTemp.ToArray();
            }
            // end-gzip

            string sHtml = Encoding.GetEncoding(936).GetString(byteArray);
            return sHtml;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
        public HtmlAgilityPack.HtmlDocument htmlToHtmlDoc(string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            //http://www.crifan.com/htmlagilitypack_html_tag_form_option_no_child_via_sibling_get_innertext/
            //make some html tag: form/option, has child
            HtmlNode.ElementsFlags.Remove("form");
            HtmlNode.ElementsFlags.Remove("option");

            htmlDoc.LoadHtml(html);

            return htmlDoc;
        }
    }
}

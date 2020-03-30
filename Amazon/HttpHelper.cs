

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Amazon
{
  
    public  class HttpHelper
    {
        CookieCollection curCookies = null;

        public string getUrlRespHtml_multiTry
                                    (string url,
                                    Dictionary<string, string> headerDict = null,
                                    string charset = null,
                                    Dictionary<string, string> postDict = null,
                                    int timeout = 20 * 1000,/* defaul use timeout to 20*1000ms */
                                    string postDataStr = "",
                                    int maxTryNum = 5)
        {
            string respHtml = "";

            for (int tryIdx = 0; tryIdx < maxTryNum; tryIdx++)
            {
                respHtml = getUrlRespHtml(url, headerDict, charset, postDict, timeout, postDataStr);
                if (respHtml != "")
                    break;
            }

            return respHtml;
        }

        public string getUrlRespHtml(string url,
                                    Dictionary<string, string> headerDict,
                                    string charset,
                                    Dictionary<string, string> postDict,
                                    int timeout,
                                    string postDataStr)
        {
            string respHtml = "";

            //HttpWebResponse resp = getUrlResponse(url, headerDict, postDict, timeout);
            HttpWebResponse resp = getUrlResponse(url, headerDict, postDict, timeout, postDataStr);

            //long realRespLen = resp.ContentLength;
            if (resp != null)
            {
                StreamReader sr;
                if ((charset != null) && (charset != ""))
                {
                    Encoding htmlEncoding = Encoding.GetEncoding(charset);
                    sr = new StreamReader(resp.GetResponseStream(), htmlEncoding);
                }
                else
                {
                    sr = new StreamReader(resp.GetResponseStream());
                }

                //respHtml = sr.ReadToEnd();
                while (!sr.EndOfStream)
                {
                    respHtml = respHtml + sr.ReadLine();
                }

                sr.Close();

                resp.Close();
            }

            return respHtml;
        }
        public HttpWebResponse getUrlResponse(string url,
                            Dictionary<string, string> headerDict,
                            Dictionary<string, string> postDict)
        {
            return getUrlResponse(url, headerDict, postDict, 0, "");
        }


        public HttpWebResponse getUrlResponse(string url,
                                Dictionary<string, string> headerDict,
                                Dictionary<string, string> postDict,
                                int timeout,
                                string postDataStr)
        {
//#if USE_GETURLRESPONSE_BW
//        HttpWebResponse localCurResp = null;
//        getUrlResponse_bw(url, headerDict, postDict, timeout, postDataStr);
//        while (bNotCompleted_resp)
//        {
//            System.Windows.Forms.Application.DoEvents();
//        }
//        localCurResp = gCurResp;

//        //clear
//        gCurResp = null;

//        return localCurResp;
//#else
            return _getUrlResponse(url, headerDict, postDict, timeout, postDataStr); ;

        }

        //indicate background worker complete or not
        bool bNotCompleted_resp = true;
        //store response of http request
        private HttpWebResponse gCurResp = null;


        private void getUrlResponse_bw(string url,
                                        Dictionary<string, string> headerDict,
                                        Dictionary<string, string> postDict,
                                        int timeout,
                                        string postDataStr)
        {
            // Create a background thread
            BackgroundWorker bgwGetUrlResp = new BackgroundWorker();
            bgwGetUrlResp.DoWork += new DoWorkEventHandler(bgwGetUrlResp_DoWork);
            bgwGetUrlResp.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwGetUrlResp_RunWorkerCompleted);

            //init
            bNotCompleted_resp = true;

            // run in another thread
            object paraObj = new object[] { url, headerDict, postDict, timeout, postDataStr };
            bgwGetUrlResp.RunWorkerAsync(paraObj);
        }
        private void bgwGetUrlResp_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] paraObj = (object[])e.Argument;
            string url = (string)paraObj[0];
            Dictionary<string, string> headerDict = (Dictionary<string, string>)paraObj[1];
            Dictionary<string, string> postDict = (Dictionary<string, string>)paraObj[2];
            int timeout = (int)paraObj[3];
            string postDataStr = (string)paraObj[4];

            e.Result = _getUrlResponse(url, headerDict, postDict, timeout, postDataStr);
        }


        private void bgwGetUrlResp_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.

            // Check to see if an error occurred in the
            // background process.
            if (e.Error != null)
            {
                //MessageBox.Show(e.Error.Message);
                return;
            }

            // Check to see if the background process was cancelled.
            if (e.Cancelled)
            {
                //MessageBox.Show("Cancelled ...");
            }
            else
            {
                bNotCompleted_resp = false;

                // Everything completed normally.
                // process the response using e.Result
                //MessageBox.Show("Completed...");
                gCurResp = (HttpWebResponse)e.Result;
            }
        }
        public HttpWebResponse _getUrlResponse(string url,
                                    Dictionary<string, string> headerDict,
                                    Dictionary<string, string> postDict,
                                    int timeout,
                                    string postDataStr)
        {
            //CookieCollection parsedCookies;

            HttpWebResponse resp = null;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.AllowAutoRedirect = true;
            req.Accept = "*/*";

            //req.ContentType = "text/plain";

            //const string gAcceptLanguage = "en-US"; // zh-CN/en-US
            //req.Headers["Accept-Language"] = gAcceptLanguage;

            req.KeepAlive = true;

            //IE8
            const string gUserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E";
            //IE9
            //const string gUserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)"; // x64
            //const string gUserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"; // x86
            //const string gUserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)";
            //Chrome
            //const string gUserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.99 Safari/533.4";
            //Mozilla Firefox
            //const string gUserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; rv:1.9.2.6) Gecko/20100625 Firefox/3.6.6";
            req.UserAgent = gUserAgent;

            req.Headers["Accept-Encoding"] = "gzip, deflate";
            req.AutomaticDecompression = DecompressionMethods.GZip;

            req.Proxy = null;

            if (timeout > 0)
            {
                req.Timeout = timeout;
            }

            if (curCookies != null)
            {
                req.CookieContainer = new CookieContainer();
                req.CookieContainer.PerDomainCapacity = 40; // following will exceed max default 20 cookie per domain
                req.CookieContainer.Add(curCookies);
            }

            if (headerDict != null)
            {
                foreach (string header in headerDict.Keys)
                {
                    string headerValue = "";
                    if (headerDict.TryGetValue(header, out headerValue))
                    {
                        // following are allow the caller overwrite the default header setting
                        if (header.ToLower() == "referer")
                        {
                            req.Referer = headerValue;
                        }
                        else if (header.ToLower() == "allowautoredirect")
                        {
                            bool isAllow = false;
                            if (bool.TryParse(headerValue, out isAllow))
                            {
                                req.AllowAutoRedirect = isAllow;
                            }
                        }
                        else if (header.ToLower() == "accept")
                        {
                            req.Accept = headerValue;
                        }
                        else if (header.ToLower() == "keepalive")
                        {
                            bool isKeepAlive = false;
                            if (bool.TryParse(headerValue, out isKeepAlive))
                            {
                                req.KeepAlive = isKeepAlive;
                            }
                        }
                        else if (header.ToLower() == "accept-language")
                        {
                            req.Headers["Accept-Language"] = headerValue;
                        }
                        else if (header.ToLower() == "useragent")
                        {
                            req.UserAgent = headerValue;
                        }
                        else if (header.ToLower() == "content-type")
                        {
                            req.ContentType = headerValue;
                        }
                        else
                        {
                            req.Headers[header] = headerValue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (postDict != null || postDataStr != "")
            {
                req.Method = "POST";
                if (req.ContentType == null)
                {
                    req.ContentType = "application/x-www-form-urlencoded";
                }

                if (postDict != null)
                {
                    postDataStr = quoteParas(postDict);
                }

                //byte[] postBytes = Encoding.GetEncoding("utf-8").GetBytes(postData);
                byte[] postBytes = Encoding.UTF8.GetBytes(postDataStr);
                req.ContentLength = postBytes.Length;

                Stream postDataStream = req.GetRequestStream();
                postDataStream.Write(postBytes, 0, postBytes.Length);
                postDataStream.Close();
            }
            else
            {
                req.Method = "GET";
            }

            //may timeout, has fixed in:
            //http://www.crifan.com/fixed_problem_sometime_httpwebrequest_getresponse_timeout/
            try
            {
                resp = (HttpWebResponse)req.GetResponse();
                updateLocalCookies(resp.Cookies, ref curCookies);
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    resp = null;
                }
            }


            return resp;
        }

        public void updateLocalCookies(CookieCollection cookiesToUpdate, ref CookieCollection localCookies)
        {
            updateLocalCookies(cookiesToUpdate, ref localCookies, null);
        }
        public void updateLocalCookies(CookieCollection cookiesToUpdate, ref CookieCollection localCookies, object omitUpdateCookies)
        {
            if (cookiesToUpdate.Count > 0)
            {
                if (localCookies == null)
                {
                    localCookies = cookiesToUpdate;
                }
                else
                {
                    foreach (Cookie newCookie in cookiesToUpdate)
                    {
                        if (isContainCookie(newCookie, omitUpdateCookies))
                        {
                            // need omit process this
                        }
                        else
                        {
                            addCookieToCookies(newCookie, ref localCookies);
                        }
                    }
                }
            }
        }//updateLocalCookies

        //check whether the cookies contains the ckToCheck cookie
        //support:
        //ckTocheck is Cookie/string
        //cookies is Cookie/string/CookieCollection/string[]
        public bool isContainCookie(object ckToCheck, object cookies)
        {
            bool isContain = false;

            if ((ckToCheck != null) && (cookies != null))
            {
                string ckName = "";
                Type type = ckToCheck.GetType();

                //string typeStr = ckType.ToString();

                //if (ckType.FullName == "System.string")
                if (type.Name.ToLower() == "string")
                {
                    ckName = (string)ckToCheck;
                }
                else if (type.Name == "Cookie")
                {
                    ckName = ((Cookie)ckToCheck).Name;
                }

                if (ckName != "")
                {
                    type = cookies.GetType();

                    // is single Cookie
                    if (type.Name == "Cookie")
                    {
                        if (ckName == ((Cookie)cookies).Name)
                        {
                            isContain = true;
                        }
                    }
                    // is CookieCollection
                    else if (type.Name == "CookieCollection")
                    {
                        foreach (Cookie ck in (CookieCollection)cookies)
                        {
                            if (ckName == ck.Name)
                            {
                                isContain = true;
                                break;
                            }
                        }
                    }
                    // is single cookie name string
                    else if (type.Name.ToLower() == "string")
                    {
                        if (ckName == (string)cookies)
                        {
                            isContain = true;
                        }
                    }
                    // is cookie name string[]
                    else if (type.Name.ToLower() == "string[]")
                    {
                        foreach (string name in ((string[])cookies))
                        {
                            if (ckName == name)
                            {
                                isContain = true;
                                break;
                            }
                        }
                    }
                }
            }

            return isContain;
        }//isContainCookie
         //add singel cookie to cookies, default no overwrite domain
        public void addCookieToCookies(Cookie toAdd, ref CookieCollection cookies)
        {
            addCookieToCookies(toAdd, ref cookies, false);
        }

        public void addCookieToCookies(Cookie toAdd, ref CookieCollection cookies, bool overwriteDomain)
        {
            bool found = false;

            if (cookies.Count > 0)
            {
                foreach (Cookie originalCookie in cookies)
                {
                    if (originalCookie.Name == toAdd.Name)
                    {
                        // !!! for different domain, cookie is not same,
                        // so should not set the cookie value here while their domains is not same
                        // only if it explictly need overwrite domain
                        if ((originalCookie.Domain == toAdd.Domain) ||
                            ((originalCookie.Domain != toAdd.Domain) && overwriteDomain))
                        {
                            //here can not force convert CookieCollection to HttpCookieCollection,
                            //then use .remove to remove this cookie then add
                            // so no good way to copy all field value
                            originalCookie.Value = toAdd.Value;

                            originalCookie.Domain = toAdd.Domain;

                            originalCookie.Expires = toAdd.Expires;
                            originalCookie.Version = toAdd.Version;
                            originalCookie.Path = toAdd.Path;

                            //following fields seems should not change
                            //originalCookie.HttpOnly = toAdd.HttpOnly;
                            //originalCookie.Secure = toAdd.Secure;

                            found = true;
                            break;
                        }
                    }
                }
            }

            if (!found)
            {
                if (toAdd.Domain != "")
                {
                    // if add the null domain, will lead to follow req.CookieContainer.Add(cookies) failed !!!
                    cookies.Add(toAdd);
                }
            }

        }//addCookieToCookies

        //quote the input dict values
        //note: the return result for first para no '&'
        public string quoteParas(Dictionary<string, string> paras, bool spaceToPercent20 = true)
        {
            string quotedParas = "";
            bool isFirst = true;
            string val = "";
            foreach (string para in paras.Keys)
            {
                if (paras.TryGetValue(para, out val))
                {
                    string encodedVal = "";
                    if (spaceToPercent20)
                    {
                        //encodedVal = HttpUtility.UrlPathEncode(val);
                        //encodedVal = Uri.EscapeDataString(val);
                        //encodedVal = Uri.EscapeUriString(val);
                        encodedVal = HttpUtility.UrlEncode(val).Replace("+", "%20");
                    }
                    else
                    {
                        encodedVal = HttpUtility.UrlEncode(val); //space to +
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                        quotedParas += para + "=" + encodedVal;
                    }
                    else
                    {
                        quotedParas += "&" + para + "=" + encodedVal;
                    }
                }
                else
                {
                    break;
                }
            }

            return quotedParas;
        }
    }
}

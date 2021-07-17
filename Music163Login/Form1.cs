using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Music163Login
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WYMusicParmsHelp wYMusic = new WYMusicParmsHelp();
            string longin = wYMusic.GetLoginParms("你的手机号", "你的密码");
            string encseckey = "581da1e7696670758d709d77bf49265dce4e32ec3724767d00948a72f6b908e9c66b8cfe521ed52a42f9fd865934ce4bcfeb4768de56b7608d069084f9254ba1c34ef29ea3274b97cb5207fd83a2dfdf70d0fa9c5bba9ccd1fa4ec22710ab8e5e305574d561bc1f86a826be2d919d62adc66956a09a7be2c3ba941fddb0d189d";
            //string encseckey = wYMusic.GetEncSecKey();
            var result = Music163LoginAsync(longin, encseckey);
            
        }

        public async Task<string> Music163LoginAsync(string loginParms, string EncSecKey)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };
            string outhtml = string.Empty;
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>() {
                {"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36" },
                {"Accept-Encoding","gzip, deflate" },
                {"Accept","*/*" },
                {"Referer","http://music.163.com/" }
            };

                //Cookie: os=pc; osver=Microsoft-Windows-10-Professional-build-10586-64bit; appver=2.0.3.131777; channel=netease; __remember_me=true;
                CookieContainer ckc = new CookieContainer();
                ckc.Add(new Cookie("os", "pc", "", "music.163.com"));
                ckc.Add(new Cookie("osver", "Microsoft-Windows-10-Professional-build-10586-64bit", "", "music.163.com"));
                ckc.Add(new Cookie("appver", "2.0.3.131777", "", "music.163.com"));
                ckc.Add(new Cookie("channel", "netease", "", "music.163.com"));
                ckc.Add(new Cookie("__remember_me", "true", "", "music.163.com"));
                HttpClientHandler hch = new HttpClientHandler();
                hch.CookieContainer = ckc;
                using (HttpClient hc = new HttpClient(hch))
                {
                    foreach (var item in dic)
                    {
                        hc.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                    }

                    string postdata = "params=" + System.Web.HttpUtility.UrlEncode(loginParms) + "&encSecKey=" + System.Web.HttpUtility.UrlEncode(EncSecKey);

                    StringContent sc = new StringContent(postdata);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    sc.Headers.ContentType.CharSet = "utf-8";

                    using (var result = await hc.PostAsync("https://music.163.com/weapi/login/cellphone", sc))
                    {
                        result.EnsureSuccessStatusCode();
                        var ckarray = result.Headers.GetValues("Set-Cookie");
                        if (result.ToString().ToLower().Contains("gzip"))
                        {
                            using (Stream HttpResponseStream = await result.Content.ReadAsStreamAsync())
                            {
                                using (var gzipStream = new GZipStream(HttpResponseStream, CompressionMode.Decompress))
                                {
                                    using (var streamReader = new StreamReader(gzipStream, Encoding.UTF8))
                                    {
                                        outhtml = streamReader.ReadToEnd();
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (HttpContent HttpContent = result.Content)
                            {
                                outhtml = await HttpContent.ReadAsStringAsync();
                            }
                        }
                    }
                    List<Cookie> cks = hch.CookieContainer.GetCookies(new UriBuilder("music.163.com").Uri).Cast<Cookie>().ToList();

                }
            }
            catch (Exception ex)
            {

            }
            
            return outhtml;
        }
    }
}

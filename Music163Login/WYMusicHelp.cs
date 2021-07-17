using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Music163Login
{
    /// <summary>
    /// 获取网易云音乐请求参数
    /// </summary>
    public class WYMusicParmsHelp
    {
        private string randomstr = "96b1096e6328c37e";
        
        public string Randomstr { get => randomstr; set => randomstr = value; }

        /// <summary>
        /// 获取网易云音乐辅助类
        /// </summary>
        /// <param name="rdmstr">16位随机数</param>
        public WYMusicParmsHelp(string rdmstr = "96b1096e6328c37e")
        {
            this.Randomstr = rdmstr;
        }

        /// <summary>
        /// 生成网易云客户端登录参数
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="pwd"></param>
        /// <param name="country_code"></param>
        /// <returns></returns>
        public string GetLoginParms(string phone, string pwd, string country_code = "86")
        {
            string password = PWD_MD5(pwd);
            string d = @"{""phone"": """ + phone + @""", ""countrycode"": """ + country_code + @""", ""password"": """ + password + @""", ""rememberLogin"": ""true""}";
            return GetOtherParms(d); 
        }

        /// <summary>
        /// 生成网易云客户端EncSecKey参数
        /// </summary>
        /// <returns></returns>
        public string GetEncSecKey()
        {
            //string i = GetRandomStr(null, 16);
            //string i = "96b1096e6328c37e";//i研究之后你会发现就是一个长度为16的随机字符串,可以采用直接固定方式
            string i = this.Randomstr;
            string e1 = "010001";
            string f = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7";
            // EncSecKey可以采用直接固定方式，采用固定方式注意16的随机字符串求对应的params参数
            string EncSecKey = GetEncSecKey(i, e1, f); //此步涉及到求次幂值，计算缓慢，建议取个10-20组值固定下来用
            Console.WriteLine(EncSecKey);
            return EncSecKey;
        }

        /// <summary>
        /// 生成网易云请求参数
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="pwd"></param>
        /// <param name="country_code"></param>
        /// <param name="jsonstr">JSON请求参数</param>
        /// <returns></returns>
        public string GetOtherParms(string jsonstr)
        {
            string d = jsonstr;
            string g = "0CoJUm6Qyw8W8jud";
            string iv1 = "0102030405060708";
            //string i = GetRandomStr(null, 16);
            //string i = "96b1096e6328c37e";//i研究之后你会发现就是一个长度为16的随机字符串,可以采用直接固定方式
            string i = this.Randomstr; //注意GetLoginParms()和GetEncSecKey()需保持统一
            string wyparams = Encrypt(d, g, iv1);
            wyparams = Encrypt(wyparams, i, iv1);
            Console.WriteLine(wyparams);
            return wyparams;

        }
        /// <summary>
        /// 网易云密码MD5算法
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public string PWD_MD5(string pwd)
        {
            byte[] by = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(pwd));
            StringBuilder sb = new StringBuilder();
            foreach (var item in by)
            {
                sb.Append(item.ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 网易云音乐params请求参数 <para>AES加密 - CipherMode.CBC</para>
        /// </summary>
        /// <param name="toEncrypt">加密字符串</param>
        /// <param name="key">加密key</param>
        /// <param name="iv">偏移量</param>
        /// <param name="Padding">填充方式</param>
        /// <returns></returns>
        public string Encrypt(string toEncrypt, string key, string iv, PaddingMode Padding = PaddingMode.PKCS7)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            //rDel.Padding = PaddingMode.Zeros;
            rDel.Padding = Padding;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="toDecrypt">解密字符串</param>
        /// <param name="解密">加密key</param>
        /// <param name="iv">偏移量</param>
        /// <param name="Padding">填充方式</param>
        /// <returns></returns>
        public string Decrypt(string toDecrypt, string key, string iv, PaddingMode Padding = PaddingMode.PKCS7)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            //rDel.Padding = PaddingMode.Zeros;
            rDel.Padding = Padding;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        /// <summary>
        /// 网易云音乐encSecKey请求参数<para>网易云Rsa算法，非常规rsa算法</para>
        /// </summary>
        /// <param name="randomstr">16位随机字符串</param>
        /// <param name="pubkey">常量010001</param>
        /// <param name="modulus">常量<para>00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7</para></param>
        public string GetEncSecKey(string randomstr, string pubkey= "010001", string modulus= "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7")
        {
            char[] c = randomstr.ToCharArray();
            Array.Reverse(c);//字符串倒序
            string ar = new string(c);
            byte[] bt = UTF8Encoding.UTF8.GetBytes(ar);
            string str = string.Empty;
            foreach (var item in bt)
            {
                str += Convert.ToString(item, 16);
            }
            //long、int值太小需要应用System.Numerics.BigInteger（任意大带符号整数）
            System.Numerics.BigInteger text = System.Numerics.BigInteger.Parse(str, System.Globalization.NumberStyles.HexNumber); //16进制
            int etext = Convert.ToInt32(pubkey, 16);//16进制
            System.Numerics.BigInteger mtext = System.Numerics.BigInteger.Parse(modulus, System.Globalization.NumberStyles.HexNumber);//16进制
            //此步计算缓慢,  建议直接抓取10-20组encSecKey请求参数固定
            System.Numerics.BigInteger result1 = System.Numerics.BigInteger.Pow(text, etext) % mtext;//System.Numerics.BigInteger.Pow求幂次方-乘方 

            return zfill(result1.ToString("x"), 256);//补0
        }
        /// <summary>
        /// 截取长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private String zfill(String str, int size)
        {
            while (str.Length < size)
            {
                str = "0" + str;
            }
            return str;
        }

        private static Random random = new Random();



        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomStr(string chars, int length)
        {
            if (string.IsNullOrEmpty(chars))
            {
                chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghizklmnopqrstuvwxyz0123456789";
            }
            //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

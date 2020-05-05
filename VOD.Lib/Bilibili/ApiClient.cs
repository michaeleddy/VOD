using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VOD.Lib.Libs;

namespace VOD.Lib.Bilibili
{
    public sealed class ApiClient
    {
        public const string build = "5520400";
        public static long GetTimeSpan => Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0, 0)).TotalSeconds);
        public static ApiKeyInfo AndroidKey { get; } = new ApiKeyInfo("4409e2ce8ffd12b8", "59b43e04ad6965f34319062b478f83dd");
        public static string AccessKey
        {
            get
            {
                return "accesskey".GetConfig();
            }
        }
        public static long UserId
        {
            get
            {
                return "uid".GetConfig().ToInt64();
            }
        }
        public static string GetSign(string url, ApiKeyInfo apiKeyInfo = null)
        {
            try
            {
                if (apiKeyInfo == null)
                    apiKeyInfo = AndroidKey;
                string str = url.Substring(url.IndexOf("?", 4) + 1);
                List<string> list = str.Split('&').ToList();
                list.Sort();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string str1 in list)
                {
                    stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                    stringBuilder.Append(str1);
                }
                stringBuilder.Append(apiKeyInfo.Secret);
                return stringBuilder.ToString().ToMD5().ToLower();
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("GetSign", ex);
                return string.Empty;
            }
        }
    }
    public sealed class ApiKeyInfo
    {
        public ApiKeyInfo(string key, string secret)
        {
            Appkey = key;
            Secret = secret;
        }
        public string Appkey { get; set; }
        public string Secret { get; set; }
    }
}
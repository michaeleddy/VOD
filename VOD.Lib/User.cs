﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLCrypto;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VOD.Lib.Bilibili;
using VOD.Lib.Libs;
using VOD.Lib.Models;

namespace VOD.Lib
{
    public sealed class User
    {
        private HttpClientEx HttpClient { get; }
        public User(HttpClientEx httpClientEx)
        {
            HttpClient = httpClientEx;
        }
        public async Task<bool> Login(string UserName, string Password)
        {
            try
            {
                if ("accesskey".GetConfig().IsNotEmpty()) return true;
                string url = "https://passport.bilibili.com/api/oauth2/login";
                var encryptedPassword = Uri.EscapeDataString(await GetEncryptedPassword(Password));
                string data = $"appkey={ApiClient.AndroidKey.Appkey}&build={ApiClient.build}&mobi_app=android&password={encryptedPassword}&platform=android&ts={ApiClient.GetTimeSpan}&username={Uri.EscapeDataString(UserName)}";
                data += "&sign=" + ApiClient.GetSign(data);
                var results = await HttpClient.PostResults(url, data);
                LoginModel model = JsonConvert.DeserializeObject<LoginModel>(results);
                if (model.Code == 0)
                {
                    var hr2 = await HttpClient.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.Data.Access_Token + "&appkey=422fd9d7289a1dd9&platform=wp"));
                    hr2.EnsureSuccessStatusCode();
                    "accesskey".SaveConfig(model.Data.Access_Token);
                    "uid".SaveConfig(model.Data.Mid.ToString());
                    url = string.Format("http://app.bilibili.com/x/v2/space?access_key={0}&appkey={1}&platform=wp&ps=10&ts={2}000&vmid={3}&build=5250000&mobi_app=android", ApiClient.AccessKey, ApiClient.AndroidKey.Appkey, ApiClient.GetTimeSpan, model.Data.Mid);
                    url += "&sign=" + ApiClient.GetSign(url);
                    string jsonStr = await HttpClient.GetResults(new Uri(url));
                    UserInfoModel infoModel = JsonConvert.DeserializeObject<UserInfoModel>(jsonStr);
                    "name".SaveConfig(infoModel.data.Card.Name);
                    if ("roomid".GetConfig().IsEmpty())
                        "roomid".SaveConfig(infoModel.data.live.roomid);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("Login", ex);
            }
            return false;
        }
        public async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String;
            try
            {
                string url = "https://passport.bilibili.com/api/oauth2/getKey";
                string content = $"appkey={ApiClient.AndroidKey.Appkey}&mobi_app=android&platform=android&ts={ApiClient.GetTimeSpan}";
                content += "&sign=" + ApiClient.GetSign(content);
                string stringAsync = await HttpClient.PostResults(url, content);
                JObject jObjects = JObject.Parse(stringAsync);
                string hash = jObjects["data"]["hash"].ToString();
                string key = jObjects["data"]["key"].ToString();
                string hashPass = string.Concat(hash, passWord);
                string publicKey = Regex.Match(key, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(publicKey);
                var asymmetricKeyAlgorithmProvider = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(PCLCrypto.AsymmetricAlgorithm.RsaPkcs1);
                var cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(numArray, 0);
                var buffer = WinRTCrypto.CryptographicEngine.Encrypt(cryptographicKey, Encoding.UTF8.GetBytes(hashPass), null);
                base64String = Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("GetEncryptedPassword", ex);
                base64String = passWord;
            }
            return base64String;
        }
    }
}
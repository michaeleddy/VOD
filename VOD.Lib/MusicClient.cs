﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using VOD.Lib.Libs;
using VOD.Lib.Models;

namespace VOD.Lib
{
    public sealed class MusicClient
    {
        private HttpClient HttpClient { get; set; }
        private List<string> SourceName { get; }
        private const string MusicList = "http://193.112.40.149:5050/search/song?key={0}page=1&limit=10&vendor={1}";
        private const string MusicUrl = "http://193.112.40.149:5050/get/song?id={0}&vendor={1}";
        public MusicClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            SourceName = new List<string>
            {
                "netease",
                "qq",
                "xiami"
            };
        }
        public async Task<MusicModel> GetSongList(string[] sv)
        {
            MusicModel model = new MusicModel();
            try
            {
                sv = GetSongInfo(sv);
                if (sv.IsEmptyArray())
                {
                    model.ErrorMsg = "点歌的格式不正确！";
                    return model;
                }
                string key = HttpUtility.UrlEncode(string.Format("{0} {1}", sv[0], sv[1]));
                string vendor = (sv.Count() == 3 ? SourceName.Exists(x => x == sv[2].ToLower()) ? sv[2].ToLower() : SourceName.FirstOrDefault() : "netease").ReplaceSpace();
                string url = string.Format(MusicList, key, vendor);
                if (url.IsNotEmpty())
                {
                    var jsonStr = await HttpClient.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<MusicResultModel>(jsonStr);
                    if (result.Success)
                    {
                        var info = result.Results.FirstOrDefault(x => !x.NeedPay);
                        if (info != null)
                        {
                            string songUrl = string.Empty;
                            if (vendor != "xiami")
                            {
                                url = string.Format(MusicUrl, info.Id, vendor);
                                jsonStr = await HttpClient.GetStringAsync(url);
                                var urlInfo = JsonConvert.DeserializeObject<MusicUrlModel>(jsonStr);
                                songUrl = urlInfo.Results.Url;
                            }
                            else
                                songUrl = info.Plus.File;
                            return new MusicModel
                            {
                                SongId = Guid.NewGuid(),
                                Singer = info.Artist,
                                SongName = info.Name,
                                SongUrl = songUrl
                            };
                        }
                        else
                        {
                            model.ErrorMsg = "从音乐服务商获取数据的数据都是有权限限制的！";
                            return model;
                        }
                    }
                    else
                    {
                        model.ErrorMsg = "从音乐服务商获取数据失败！";
                        return model;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("GetSongList", ex);
                model.ErrorMsg = ex.Message;
            }
            return model;
        }
        private string[] GetSongInfo(string[] vs)
        {
            List<string> vss = new List<string>();
            try
            {
                if (vs != null && vs.Length > 0)
                {
                    foreach (var v in vs)
                    {
                        if (v.ReplaceSpace().IsNotEmpty())
                            vss.Add(v);
                    }
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("GetSongInfo", ex);
            }
            return vss.ToArray();
        }
    }
}
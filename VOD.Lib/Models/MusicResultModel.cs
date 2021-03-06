﻿using System.Collections.Generic;

namespace VOD.Lib.Models
{
    public sealed class MusicResultModel
    {
        public bool Success { get; set; }
        public List<MusicInfoModel> Results { get; set; }
    }
    public sealed class MusicInfoModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Cover { get; set; }
        public bool NeedPay { get; set; }
        public UrlModel Plus { get; set; }
    }
    public sealed class MusicUrlModel
    {
        public bool Success { get; set; }
        public UrlModel Results { get; set; }
    }
    public sealed class UrlModel
    {
        public string File { get; set; }
        public string Url { get; set; }
    }
}
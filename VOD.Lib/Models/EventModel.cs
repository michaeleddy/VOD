﻿using System;

namespace VOD.Lib.Models
{
    public sealed class EventModel
    {
        /// <summary>
        /// 歌曲信息
        /// </summary>
        public MusicModel MusicInfo { get; set; }
        /// <summary>
        /// 打印文字
        /// </summary>
        public string PrintMsg { get; set; }
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 是否发送弹幕
        /// </summary>
        public bool Send { get; set; } = true;
    }
}
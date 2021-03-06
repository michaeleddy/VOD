﻿using System;

namespace VOD.Lib.Models
{
    public sealed class MusicModel
    {
        /// <summary>
        /// 歌手名
        /// </summary>
        public string Singer { get; set; }
        /// <summary>
        /// 歌曲ID
        /// </summary>
        public Guid SongId { get; set; }
        /// <summary>
        /// 歌曲名
        /// </summary>
        public string SongName { get; set; }
        /// <summary>
        /// 歌曲地址
        /// </summary>
        public string SongUrl { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 点歌人
        /// </summary>
        public string SongChoser { get; set; }
        public string ItemContent
        {
            get
            {
                return string.Format("{0}点歌{1}", SongChoser, SongName);
            }
        }
    }
}
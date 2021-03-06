﻿using System.Collections.Generic;

namespace VOD.Lib.Models
{
    public sealed class LoginModel
    {
        public int TimeStamp { get; set; }
        public int Code { get; set; }
        public LoginDataModel Data { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
    }
    public sealed class LoginDataModel
    {
        public int Status { get; set; }
        public TokenInfo TokenInfo { get; set; }
        public CookieInfo CookieInfo { get; set; }
        public List<string> Sso { get; set; }
        public string Url { get; set; }
        public long Mid { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public int Expires_In { get; set; }
    }
    public class TokenInfo
    {
        public long Mid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
    public sealed class CookieInfo
    {
        public List<Cookies> Cookies { get; set; }
        public List<string> Domains { get; set; }
    }
    public sealed class Cookies
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int HttpOnly { get; set; }
        public int Expires { get; set; }
    }
    public sealed class UserInfoModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public UserInfoModel data { get; set; }
        public UserCardInfo Card { get; set; }
        public UserInfoModel live { get; set; }
        public int liveStatus { get; set; }
        public string roomid { get; set; }
    }
    public sealed class UserCardInfo
    {
        public string Name { get; set; }
    }
}
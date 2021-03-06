﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VOD.Lib;
using VOD.Lib.Bilibili;
using VOD.Lib.Libs;
using VOD.Lib.Models;

namespace VOD.Wpf
{
    public sealed class Danmaku : IDisposable
    {
        private int RoomId { get; }
        private string[] DefaultHosts { get; }
        private string ChatHost = "chat.bilibili.com";
        private int ChatPort { get; set; } = 2243;
        private TcpClient TcpClient { get; }
        private MusicClient MusicClient { get; }
        private Stream NetStream { get; set; }
        private bool Connected { get; set; } = false;
        private HttpClientEx HttpClient { get; }
        public delegate void PlaySongEvent(object sender, EventModel e);
        public event PlaySongEvent PlaySongEvt;
        public delegate void PrintEvent(object sender, EventModel e);
        public event PrintEvent PrintEvt;
        public Danmaku()
        {
            RoomId = "roomid".GetConfig().ToInt32();
            TcpClient = new TcpClient();
            HttpClient = new HttpClientEx();
            MusicClient = new MusicClient(HttpClient.BaseHttpClient);
            DefaultHosts = new string[] { "livecmt-2.bilibili.com", "livecmt-1.bilibili.com" };
        }
        public async Task SendDanmu(string text)
        {
            try
            {
                string sendText = $"cid={RoomId}&mid={ApiClient.UserId}&msg={text}&rnd={ApiClient.GetTimeSpan}&mode=1&pool=0&type=json&color=16777215&fontsize=25&playTime=0.0";
                var url = $"https://api.live.bilibili.com/api/sendmsg?access_key={ApiClient.AccessKey}&actionKey=appkey&appkey={ApiClient.AndroidKey.Appkey}&build={ApiClient.build}&device=android&mobi_app=android&platform=android&ts={ApiClient.GetTimeSpan}";
                url += "&sign=" + ApiClient.GetSign(url);
                string result = await HttpClient.PostResults(url, sendText);
                JObject jb = JObject.Parse(result);
                var model = new EventModel
                {
                    PrintMsg = "发送弹幕结果:" + ((int)jb["code"] == 0),
                    Send = false
                };
                PrintEvt?.Invoke(this, model);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("SendDanmu", ex);
            }
        }
        public async Task<bool> ConnectAsync()
        {
            try
            {
                var token = string.Empty;
                try
                {
                    var req = await HttpClient.GetStringAsync(new Uri("https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id=" + RoomId));
                    var roomobj = JObject.Parse(req);
                    token = roomobj["data"]["token"].ToString();
                    ChatHost = roomobj["data"]["host"].ToString();
                    ChatPort = roomobj["data"]["port"].Value<int>();
                    if (string.IsNullOrEmpty(ChatHost))
                        throw new Exception();
                }
                catch
                {
                    ChatHost = DefaultHosts[new Random().Next(DefaultHosts.Length)];
                }
                var ipaddrss = await Dns.GetHostAddressesAsync(ChatHost);
                var random = new Random();
                var idx = random.Next(ipaddrss.Length);
                await TcpClient.ConnectAsync(ipaddrss[idx], ChatPort);
                NetStream = Stream.Synchronized(TcpClient.GetStream());
                if (await SendJoinChannel(RoomId, token))
                {
                    Connected = true;
                    _ = HeartbeatLoop();
                    _ = ReceiveMessageLoop();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("ConnectAsync", ex);
                return false;
            }
        }
        private async Task ReceiveMessageLoop()
        {
            try
            {
                var stableBuffer = new byte[16];
                var buffer = new byte[4096];
                while (this.Connected)
                {
                    await NetStream.ReadBAsync(stableBuffer, 0, 16);
                    var protocol = DanmakuProtocol.FromBuffer(stableBuffer);
                    if (protocol.PacketLength < 16)
                        throw new NotSupportedException("协议失败: (L:" + protocol.PacketLength + ")");
                    var payloadlength = protocol.PacketLength - 16;
                    if (payloadlength == 0) continue;
                    buffer = new byte[payloadlength];
                    await NetStream.ReadBAsync(buffer, 0, payloadlength);
                    if (protocol.Version == 2 && protocol.Action == 5)
                    {
                        using (var ms = new MemoryStream(buffer, 2, payloadlength - 2))
                        using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                        {
                            var headerbuffer = new byte[16];
                            try
                            {
                                while (true)
                                {
                                    await deflate.ReadBAsync(headerbuffer, 0, 16);
                                    var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                    payloadlength = protocol_in.PacketLength - 16;
                                    var danmakubuffer = new byte[payloadlength];
                                    await deflate.ReadBAsync(danmakubuffer, 0, payloadlength);
                                    ProcessDanmaku(protocol.Action, danmakubuffer);
                                }
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        ProcessDanmaku(protocol.Action, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("ReceiveMessageLoop", ex);
                Disconnect();
            }
        }
        private async void ProcessDanmaku(int action, byte[] buffer)
        {
            switch (action)
            {
                case 5:
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        try
                        {
                            var dama = new DanmakuModel(json, 2);
                            switch (dama.MsgType)
                            {
                                case MsgTypeEnum.Comment:
                                    {
                                        var model = new EventModel();
                                        if (dama.CommentText.StartsWith("#点歌"))
                                        {
                                            var values = dama.CommentText.Replace("#点歌", "").Split(' ');
                                            var sv = values.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                            var musicInfo = await MusicClient.GetSongList(sv);
                                            musicInfo.SongChoser = dama.UserName;
                                            if (musicInfo.ErrorMsg.IsNotEmpty())
                                            {
                                                model.PrintMsg = string.Format("用户：{0} 点歌：{1} 失败,可能原因：{2}", dama.UserName, dama.CommentText, musicInfo.ErrorMsg);
                                                PrintEvt?.Invoke(this, model);
                                            }
                                            else
                                            {
                                                model.PrintMsg = string.Format("用户：{0} 点歌：{1} 成功，加入队列中", dama.UserName, dama.CommentText);
                                                model.MusicInfo = musicInfo;
                                                PlaySongEvt?.Invoke(this, model);
                                            }
                                        }
                                        else
                                        {
                                            model.PrintMsg = string.Format("用户：{0} 发送了一条弹幕：{1}", dama.UserName, dama.CommentText);
                                            PrintEvt?.Invoke(this, model);
                                        }
                                        break;
                                    }
                                case MsgTypeEnum.GiftSend:
                                    {
                                        var model = new EventModel
                                        {
                                            PrintMsg = string.Format("谢谢{0}赠送的{1}个{2}，{3}表示灰常喜欢，么么哒~", dama.UserName, dama.GiftCount, dama.GiftName, "name".GetConfig()),
                                            DateTime = DateTime.Now
                                        };
                                        PlaySongEvt?.Invoke(this, model);
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.LogError("ProcessDanmaku", ex);
                        }
                        break;
                    }
            }
        }
        private async Task HeartbeatLoop()
        {

            try
            {
                while (Connected)
                {
                    await SendHeartbeatAsync();
                    await Task.Delay(30000);
                }
            }
            catch { Disconnect(); }
        }
        public void Disconnect()
        {
            if (Connected)
            {
                Connected = false;
                TcpClient.Close();
                NetStream = null;
            }
        }
        private async Task SendHeartbeatAsync()
        {
            await SendSocketDataAsync(2);
        }
        Task SendSocketDataAsync(int action, string body = "")
        {
            return SendSocketDataAsync(0, 16, 2, action, 1, body);
        }
        async Task SendSocketDataAsync(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            try
            {
                var playload = Encoding.UTF8.GetBytes(body);
                if (packetlength == 0)
                    packetlength = playload.Length + 16;
                var buffer = new byte[packetlength];
                using (var ms = new MemoryStream(buffer))
                {
                    var b = EndianBitConverter.BigEndian.GetBytes(buffer.Length);
                    await ms.WriteAsync(b, 0, 4);
                    b = EndianBitConverter.BigEndian.GetBytes(magic);
                    await ms.WriteAsync(b, 0, 2);
                    b = EndianBitConverter.BigEndian.GetBytes(ver);
                    await ms.WriteAsync(b, 0, 2);
                    b = EndianBitConverter.BigEndian.GetBytes(action);
                    await ms.WriteAsync(b, 0, 4);
                    b = EndianBitConverter.BigEndian.GetBytes(param);
                    await ms.WriteAsync(b, 0, 4);
                    if (playload.Length > 0)
                        await ms.WriteAsync(playload, 0, playload.Length);
                    await NetStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("SendSocketDataAsync", ex);
            }
        }
        private async Task<bool> SendJoinChannel(int channelId, string token)
        {
            try
            {
                var packetModel = new { roomid = channelId, uid = 0, protover = 2, token, platform = "danmuji" };
                var playload = JsonConvert.SerializeObject(packetModel);
                await SendSocketDataAsync(7, playload);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("SendJoinChannel", ex);
                return false;
            }
        }
        public void Dispose()
        {
            try
            {
                Disconnect();
                HttpClient.Dispose();
            }
            catch { }
        }
    }
}
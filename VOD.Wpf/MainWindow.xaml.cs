using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VOD.Lib;
using VOD.Lib.Libs;
using VOD.Lib.Models;
using VOD.Wpf.Dialogs;

namespace VOD.Wpf
{
    public partial class MainWindow : Window
    {
        private User User { get; }
        private DispatcherTimer Timer { get; }
        private Danmaku Danmu { get; }
        private HttpClientEx HttpClient { get; }
        private const string BreakLine = "\r\n";
        private static object LockObj { get; } = new object();
        public MainWindow()
        {
            InitializeComponent();

            Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            Timer.Tick += Timer_Tick;
            HttpClient = new HttpClientEx();
            User = new User(HttpClient);
            Danmu = new Danmaku();
            Danmu.PlaySongEvt += Danmu_PlaySongEvt;
            Danmu.PrintEvt += Danmu_PrintEvt;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            string currentTime = string.Format("{0}:{1}", player.Position.Minutes, player.Position.Seconds.ToString("00"));
            txtCurrent.Content = currentTime;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string user = "user".GetConfig(), passwd = "passwd".GetConfig();
                if (user.IsEmpty() || passwd.IsEmpty())
                {
                    Setting setting = new Setting();
                    if (setting.ShowDialog() == true)
                        await Login(user, passwd);
                    else
                    {
                        MessageBox.Show("需要登录后才能使用哦qaq~", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        this.Close();
                        Application.Current.Shutdown();
                    }
                }
                else
                    await Login(user, passwd);
                player.MediaOpened += Player_MediaOpened;
                player.MediaEnded += Player_MediaEnded;
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("Window_Loaded", ex);
            }
        }
        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            btnNext_Click(sender, e);
            timeSlider.Value = 0;
            txtCurrent.Content = "0:00";
        }
        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            Timer.Start();
            timeSlider.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
            string totalTime = string.Format("{0}:{1}", player.NaturalDuration.TimeSpan.Minutes, player.NaturalDuration.TimeSpan.Seconds.ToString("00"));
            txtTotal.Content = totalTime;
        }
        private async Task Login(string user, string passwd)
        {
            var result = await User.Login(user, passwd);
            if (result)
                await Danmu.ConnectAsync();
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Danmu.PrintEvt -= Danmu_PrintEvt;
                Danmu.PlaySongEvt -= Danmu_PlaySongEvt;
                Danmu.Dispose();
                HttpClient.Dispose();
            }
            catch { }
        }
        private void Danmu_PlaySongEvt(object sender, EventModel e)
        {
            try
            {
                lock (LockObj)
                {
                    songList.Items.Add(e.MusicInfo);
                    if (btnState.Content.ToString() == "播放")
                    {
                        player.Source = new Uri(e.MusicInfo.SongUrl, UriKind.RelativeOrAbsolute);
                        player.Tag = e.MusicInfo.SongId;
                        player.Play();
                        btnState.Content = "暂停";
                    }
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("Danmu_PlaySongEvt", ex);
            }
        }
        private async void Danmu_PrintEvt(object sender, EventModel e)
        {
            try
            {
                int totalline = printBox.GetLineIndexFromCharacterIndex(printBox.Text.Length);
                if (totalline > 1000)
                {
                    var length = printBox.GetLineLength(500);
                    printBox.Text.Remove(0, length);
                }
                string msg = string.Format("{0},时间:{1}", e.PrintMsg, e.DateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                if (e.Send) await Danmu.SendDanmu(msg);
                printBox.Text += string.Format("{0}{1}", msg, BreakLine);
                printBox.ScrollToLine(printBox.GetLineIndexFromCharacterIndex(printBox.SelectionStart));
                printBox.Focus();
                printBox.Select(printBox.Text.Length, 0);
                printBox.ScrollToLine(printBox.GetLineIndexFromCharacterIndex(printBox.SelectionStart));
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("Danmu_PrintEvt", ex);
            }
        }
        private void btnState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (songList.Items.Count > 0)
                {
                    if (btnState.Content.ToString() == "播放")
                    {
                        player.Play();
                        btnState.Content = "暂停";
                    }
                    else
                    {
                        player.Pause();
                        btnState.Content = "播放";
                    }
                }
                else
                {
                    MessageBox.Show("没有歌曲！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("btnState_Click", ex);
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Guid.TryParse(player.Tag.ToString(), out Guid songId))
                {
                    var item = songList.Items.Find(x => x.SongId == songId);
                    if (item != null)
                        songList.Items.Remove(item);
                }
                var next = songList.Items.Get(0);
                if (next != null)
                {
                    player.Source = new Uri(next.SongUrl, UriKind.RelativeOrAbsolute);
                    player.Tag = next.SongId;
                    player.Play();
                    btnState.Content = "暂停";
                }
                else
                {
                    player.Pause();
                    btnState.Content = "播放";
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("btnNext_Click", ex);
            }
        }
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog() { Filter = "音乐文件(*.mp3)|*.mp3" };
                if (openFileDialog.ShowDialog() == true)
                {
                    EventModel model = new EventModel
                    {
                        MusicInfo = new MusicModel
                        {
                            SongChoser = "name".GetConfig(),
                            SongUrl = openFileDialog.FileName,
                            SongName = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                            SongId = Guid.NewGuid()
                        }
                    };
                    Danmu_PlaySongEvt(sender, model);
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.LogError("btnOpen_Click", ex);
            }
        }
        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
        private void CancelItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            setting.ShowDialog();
        }
        private void timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Position = TimeSpan.FromSeconds(e.NewValue);
        }
    }
}
using System.Windows;
using VOD.Lib;
using VOD.Lib.Models;
using ApplicationForm = System.Windows.Forms.Application;

namespace VOD.Wpf.Dialogs
{
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool restartApp = false;
            if (txtAccount.Text.IsNotEmpty())
            {
                var value = "user".SaveConfig(txtAccount.Text);
                if (restartApp == false && value == SaveState.Update)
                    restartApp = true;
            }
            if (txtPasswd.Password.IsNotEmpty())
            {
                var value = "passwd".SaveConfig(txtPasswd.Password);
                if (restartApp == false && value == SaveState.Update)
                    restartApp = true;
            }
            if (txtRoomId.Text.IsNotEmpty())
            {
                var value = "roomid".SaveConfig(txtRoomId.Text);
                if (restartApp == false && value == SaveState.Update)
                    restartApp = true;
            }
            if (restartApp == true)
            {
                "uid".SaveConfig(string.Empty);
                "accesskey".SaveConfig(string.Empty);
            }
            string msg = restartApp ? "保存成功，需要重新启动客户端！若没有配置直播间ID将自动获取账号对应的直播间ID。" : "保存成功！";
            if (MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK && restartApp)
            {
                this.DialogResult = true;
                this.Close();
                ApplicationForm.Restart();
                Application.Current.Shutdown();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtAccount.Text = "user".GetConfig();
            txtPasswd.Password = "passwd".GetConfig();
            txtRoomId.Text = "roomid".GetConfig();
        }
    }
}
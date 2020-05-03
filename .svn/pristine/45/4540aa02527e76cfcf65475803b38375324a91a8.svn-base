using System.Windows;
using VOD.Lib;
using VOD.Lib.Models;

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
            if (restartApp == true)
            {
                "uid".SaveConfig(string.Empty);
                "roomid".SaveConfig(string.Empty);
                "accesskey".SaveConfig(string.Empty);
            }
            string msg = restartApp ? "保存成功，需要重新启动客户端！" : "保存成功！";
            if (MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Information) == MessageBoxResult.OK && restartApp)
            {
                this.DialogResult = true;
                this.Close();
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtAccount.Text = "user".GetConfig();
            txtPasswd.Password = "passwd".GetConfig();
        }
    }
}
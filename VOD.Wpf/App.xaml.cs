using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VOD.Lib.Libs;

namespace VOD.Wpf
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; ;
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogManager.Instance.LogError("Task线程内未捕获异常", e.Exception);
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.Instance.LogError("非UI线程未捕获异常", e.ExceptionObject as Exception);
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.Instance.LogError("UI线程未捕获异常", e.Exception);
        }
    }
}
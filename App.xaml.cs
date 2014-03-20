using System.Windows;
using GalaSoft.MvvmLight.Threading;
using SyncDateTime.Properties;
using SyncDateTime.Logging;

namespace SyncDateTime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logger.SyncLogger.Info("Start application");

        }
    }
}

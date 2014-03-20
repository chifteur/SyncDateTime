using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDateTime.Logging
{
    public class Logger
    {
        private static ILog _log;
        private static AsyncFileAppender _roller;

        public static void Setup()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            //patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ConversionPattern = "%date [%thread] %-5level - %message%newline";
            patternLayout.ActivateOptions();

            _roller = new AsyncFileAppender();
            _roller.AppendToFile = false;
            var myTemp = System.Windows.Forms.Application.LocalUserAppDataPath;//Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _roller.File = Path.Combine(myTemp, "SyncDateTime\\Logs\\SyncLog.txt");
            _roller.Layout = patternLayout;
            _roller.MaxSizeRollBackups = 5;
            _roller.MaximumFileSize = "100MB";
            _roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            _roller.StaticLogFileName = true;
            _roller.ActivateOptions();
            
            //Add this appender for every logger
            hierarchy.Root.AddAppender(_roller);
            hierarchy.Root.Level = Level.All;

            //var coreLogger = hierarchy.GetLogger("SyncLogger") as log4net.Repository.Hierarchy.Logger;
            //coreLogger.Level = Level.All;
            //coreLogger.Parent = hierarchy.Root;
            //coreLogger.AddAppender(roller); 

            
            hierarchy.Configured = true;

            _log = LogManager.GetLogger("SyncLogger");
            _log.Info("Setup log4net");
        }

        public static ILog SyncLogger
        {
            get
            {
                if (_log == null)
                    Setup();
                return _log;
            }
        }

        internal static void Flush()
        {
            if (_roller != null)
                _roller.Flush();
        }
    }
}

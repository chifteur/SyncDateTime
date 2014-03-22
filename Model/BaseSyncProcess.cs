using SyncDateTime.Logging;
using SyncDateTime.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SyncDateTime.Model
{
    public class BaseSyncProcess
    {

        private List<string> _files = new List<string>();  // List that will hold the files and subfiles in path
        private List<string> _folders = new List<string>(); // List that hold direcotries that cannot be accessed
        private Action<string,int> _callback;
        private int _skipDirectory;

        public int Count { get; private set; }

        public void Run(DataOption option, Action<string,int> logCallBack)
        {
            _skipDirectory = option.SourcePath.Length;
            // because we don't want it to be prefixed by a slash
            // if source like "C:\MyFolder", rather than "C:\MyFolder\"
            if (!option.SourcePath.EndsWith("" + Path.DirectorySeparatorChar)) 
                _skipDirectory++;

            _callback = logCallBack;
       
            Task.Factory.StartNew(() => RunAsync(option), TaskCreationOptions.LongRunning);//.ContinueWith((t)=>callBack(true));
        }

        private void RunAsync(DataOption option)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            var _stopWatch = new Stopwatch();
            Logger.SyncLogger.InfoFormat("Start sync source:{0}, target:{1}", option.SourcePath, option.TargetPath);
            //First build the source list
            _stopWatch.Restart();
            DirectoryInfo di = new DirectoryInfo(option.SourcePath);
            FullDirList(di, "*");

            Logger.SyncLogger.InfoFormat("Find {0} files, and {1} folders in source", _files.Count, _folders.Count);

            Count = _files.Count + _folders.Count;


            DateTime newDT;
            //now for each files and folder
            FileInfo sourcefile;
            FileInfo targetfile;
            foreach (var file in _files)
            {
                sourcefile = new FileInfo(Path.Combine(option.SourcePath, file));
                targetfile = new FileInfo(Path.Combine(option.TargetPath, file));
                Count--;
                if (sourcefile.Exists && targetfile.Exists)
                {
                    if (option.CreateDate)
                    {
                        newDT = File.GetCreationTime(sourcefile.FullName);
                        try
                        {

                            File.SetCreationTime(targetfile.FullName, newDT);
                            Callback(String.Format("Set CreationTime file {0:g} : {1}", newDT, targetfile.FullName));
                        }
                        catch (Exception ex)
                        {
                            Callback("Error :" + ex.Message);
                        }
                    }

                    if (option.ModDateTime)
                    {
                        newDT = File.GetLastWriteTime(sourcefile.FullName);
                        try
                        {

                            File.SetLastWriteTime(targetfile.FullName, newDT);
                            Callback(String.Format("Set LastWriteTime file {0:g} : {1}", newDT, targetfile.FullName));
                        }
                        catch (Exception ex)
                        {
                            Callback("Error :" + ex.Message);
                        }
                    }
                }
                else
                    Logger.SyncLogger.WarnFormat("File {0} doesn't exist, unabled to sync with source.", targetfile.FullName);
            }

            DirectoryInfo sourcedir;
            DirectoryInfo targetdir;
            foreach (var dir in _folders)
            {
                sourcedir = new DirectoryInfo(Path.Combine(option.SourcePath, dir));
                targetdir = new DirectoryInfo(Path.Combine(option.TargetPath, dir));
                Count--;
                if (sourcedir.Exists && targetdir.Exists)
                {
                    if (option.CreateDate)
                    {
                        newDT = Directory.GetCreationTime(sourcedir.FullName);
                        try
                        {
                            Directory.SetCreationTime(targetdir.FullName, newDT);
                            Callback(String.Format("Set CreationTime folder {0:g} : {1}", newDT, targetdir.FullName));
                        }
                        catch (Exception ex)
                        {
                            Callback("Error :" + ex.Message);
                        }
                    }

                    if (option.ModDateTime)
                    {
                        newDT = Directory.GetLastWriteTime(sourcedir.FullName);
                        try
                        {
                            Directory.SetLastWriteTime(targetdir.FullName, newDT);
                            Callback(String.Format("Set LastWriteTime folder {0:g} : {1}", newDT, targetdir.FullName));
                        }
                        catch (Exception ex)
                        {
                            Callback("Error :" + ex.Message);
                        }
                    }
                }
                else
                    Logger.SyncLogger.WarnFormat("Folder {0} doesn't exist, unabled to sync with source.", targetdir.FullName);


            }
            _stopWatch.Stop();
            Logger.SyncLogger.InfoFormat("End. Sync {0} items, in {1}", _files.Count + _folders.Count, _stopWatch.Elapsed);
            Logger.Flush();

        }

        private void Callback(string p)
        {
            Logger.SyncLogger.Info(p);
            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(_callback, System.Windows.Threading.DispatcherPriority.Background,p,Count);
        }

        private void FullDirList(DirectoryInfo dir, string searchPattern)
        {
            try
            {
                foreach (FileInfo f in dir.GetFiles(searchPattern))
                {
                    //Console.WriteLine("File {0}", f.FullName);
                    _files.Add(f.FullName.Substring(_skipDirectory));
                }
            }
            catch
            {
                Callback(string.Format("Error :Directory {0}  \n could not be accessed!!!!", dir.FullName));
                return;  // We alredy got an error trying to access dir so dont try to access it again
            }

            // process each directory
            // If I have been able to see the files in the directory I should also be able 
            // to look at its directories so I dont think I should place this in a try catch block
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                _folders.Add(d.FullName.Substring(_skipDirectory));
                FullDirList(d, searchPattern);
            }

        }

    }
}

using SyncDateTime.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public void Run(string source, string target, Action<string,int> logCallBack, Action<bool> callBack)
        {
            _skipDirectory = source.Length;
            // because we don't want it to be prefixed by a slash
            // if source like "C:\MyFolder", rather than "C:\MyFolder\"
            if (!source.EndsWith("" + Path.DirectorySeparatorChar)) 
                _skipDirectory++;

            _callback = logCallBack;
            //var task = Task.Run(() => RunAsync(source, target));           
            Task.Factory.StartNew(() => RunAsync(source, target),TaskCreationOptions.LongRunning);//.ContinueWith((t)=>callBack(true));
        }

        private void RunAsync(string source, string target)
        {
            //First build the source list
            DirectoryInfo di = new DirectoryInfo(source);
            FullDirList(di, "*");

            Count = _files.Count + _folders.Count;

            DateTime newDT;
            //now for each files and folder
            FileInfo sourcefile;
            FileInfo targetfile;
            foreach (var file in _files)
            {
                sourcefile = new FileInfo(Path.Combine(source, file));
                targetfile = new FileInfo(Path.Combine(target, file));
                Count--;
                if (sourcefile.Exists && targetfile.Exists)
                {
                    newDT = File.GetCreationTime(sourcefile.FullName);
                    try
                    {
                        
                        File.SetCreationTime(targetfile.FullName, newDT);
                        Callback(String.Format("Set file {0:g} : {1}", newDT, targetfile.FullName));
                    }
                    catch (Exception ex)
                    {
                        Callback("Error :" + ex.Message);
                    }
                }
            }

            DirectoryInfo sourcedir;
            DirectoryInfo targetdir;
            foreach (var dir in _folders)
            {
                sourcedir = new DirectoryInfo(Path.Combine(source, dir));
                targetdir = new DirectoryInfo(Path.Combine(target, dir));
                Count--;
                if (sourcedir.Exists && targetdir.Exists)
                {
                    newDT = Directory.GetCreationTime(sourcedir.FullName);
                    try
                    {
                        Directory.SetCreationTime(targetdir.FullName, newDT);
                        Callback(String.Format("Set folder {0:g} : {1}", newDT, targetdir.FullName));
                    }
                    catch (Exception ex)
                    {
                        Callback("Error :" + ex.Message);
                    }


                }
            }
        }

        private void Callback(string p)
        {
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

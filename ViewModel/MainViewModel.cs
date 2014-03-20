using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SyncDateTime.Logging;
using SyncDateTime.Messages;
using SyncDateTime.Model;
using SyncDateTime.Properties;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SyncDateTime.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Variables

        private readonly IDataService _dataService;
        private bool _Busy;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        //public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private int _Restant = 0;
        private List<string> _LogResult;
        private string _sLogResult;
        #endregion


        #region Properties

        public int Restant
        {
            get { return _Restant; }
            set
            {
                if (_Restant == value) return;
                _Restant = value;
                RaisePropertyChanged("Restant");
            }
        }

        public string SourceFolder
        {
            get { return Settings.Default.SourceFolder; }
            set
            {
                if (Settings.Default.SourceFolder == value) return;
                Settings.Default.SourceFolder = value;
                RaisePropertyChanged("SourceFolder");
            }
        }

        public string TargetFolder
        {
            get { return Settings.Default.TargetFolder; }
            set
            {
                if (Settings.Default.TargetFolder == value) return;
                Settings.Default.TargetFolder = value;
                RaisePropertyChanged("TargetFolder");
            }
        }

        public string LogResult
        {
            get
            {
                return _sLogResult;
                //var res = new StringBuilder();
                //foreach (var str in _LogResult)
                //{
                //    res.AppendLine(str);
                //}
                //return res.ToString();
            }
            set
            {
                _sLogResult = value;
                //_LogResult.Add(value);
                //if (_LogResult.Count > 100)
                //    _LogResult.RemoveAt(0);
                RaisePropertyChanged("LogResult");
            }
        }

        #region Commands
        public ICommand SelectFolder { get; private set; }
        public ICommand SwitchFolder { get; private set; }
        public ICommand SynchFolder { get; private set; }        
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _LogResult = new List<string>();
            _dataService = dataService;

            SelectFolder = new RelayCommand<EnumWichFolder>(SelectFolderExecute);
            SwitchFolder = new RelayCommand(() =>
            {
                var save = TargetFolder;
                TargetFolder = SourceFolder;
                SourceFolder = save;
            });

            SynchFolder = new RelayCommand(SyncFolderExecute, SyncFolderCanExecute);
        }

        #endregion

        #region Methods

        private void SelectFolderExecute(EnumWichFolder value)
        {
            var msg = new SelectFolderMessage(SelectFolderResult);
            msg.Folder = value;
            switch (value)
            {
                case EnumWichFolder.Source:
                    msg.Path = SourceFolder;
                    break;
                case EnumWichFolder.Target:
                    msg.Path = TargetFolder;
                    break;
            }
            Messenger.Default.Send(msg);
        }

        private void SelectFolderResult(string newPath, EnumWichFolder folder)
        {
            switch(folder)
            {
                case EnumWichFolder.Source:
                    SourceFolder = newPath;
                    break;
                case EnumWichFolder.Target:
                    TargetFolder = newPath;
                    break;
            }
        }

        private bool SyncFolderCanExecute()
        {
            return !_Busy;
        }

        private void SyncFolderExecute()
        {
            if (!_Busy)
            {
                _Busy = true;
                _LogResult.Clear();
                _dataService.SyncFolder((e, i) =>
                {
                    LogResult = e;                    
                    Restant = i;
                    if (i < 2)
                        _Busy = false;
                }, (b) => { });
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed            
            base.Cleanup();
        }
        #endregion
    }
}
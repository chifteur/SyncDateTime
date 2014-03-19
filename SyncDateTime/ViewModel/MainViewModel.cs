using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SyncDateTime.Messages;
using SyncDateTime.Model;
using SyncDateTime.Properties;
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

        private string _welcomeTitle = string.Empty;
        private StringBuilder _LogResult;
        #endregion


        #region Properties
        ///// <summary>
        ///// Gets the WelcomeTitle property.
        ///// Changes to that property's value raise the PropertyChanged event. 
        ///// </summary>
        //public string WelcomeTitle
        //{
        //    get { return _welcomeTitle; }
        //    set
        //    {
        //        if (_welcomeTitle == value) return;
        //        _welcomeTitle = value;
        //        RaisePropertyChanged(WelcomeTitlePropertyName);
        //    }
        //}

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
            get { return _LogResult.ToString(); }
            set
            {
                _LogResult.AppendLine(value);
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
            _LogResult = new StringBuilder();
            _dataService = dataService;
            //_dataService.GetData(
            //    (item, error) =>
            //    {
            //        if (error != null)
            //        {
            //            // Report error here
            //            return;
            //        }

            //        WelcomeTitle = item.Title;
            //    });

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
            _Busy = true;
            _dataService.SyncFolder((e) => LogResult = e, (b) => _Busy = false);
        }

        public override void Cleanup()
        {
            // Clean up if needed            
            base.Cleanup();
        }
        #endregion
    }
}
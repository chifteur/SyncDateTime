using System.Windows;
using SyncDateTime.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using SyncDateTime.Messages;

namespace SyncDateTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

            Messenger.Default.Register<SelectFolderMessage>(
                this,
                msg =>
                {
                    var dialog = new System.Windows.Forms.FolderBrowserDialog();
                    dialog.Reset();
                    dialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
                    dialog.SelectedPath = msg.Path;
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Send callback
                        msg.ProcessCallback(dialog.SelectedPath);
                    }
                });
        }
    }
}
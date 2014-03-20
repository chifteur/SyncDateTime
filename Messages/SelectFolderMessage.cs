using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDateTime.Messages
{
    class SelectFolderMessage : MessageBase
    {
        private Action<string, EnumWichFolder> _callback;
        
       

        public EnumWichFolder Folder { get; set; }
        public string  Path { get; set; }

        public SelectFolderMessage(Action<string, EnumWichFolder> callback)
            : base()
        {
            _callback = callback;
        }

        public void ProcessCallback(string newfolder)
        {
            _callback(newfolder, Folder);
        }
    }

    enum EnumWichFolder
    {
        Source,
        Target
    }
}

using System;
using SyncDateTime.Model;

namespace SyncDateTime.Design
{
    public class DesignDataService : IDataService
    {


        public void SyncFolder(Action<string,int> callLog, Action<bool> callback)
        {
            //throw new NotImplementedException();
            callLog("Design Time", 100);
        }
    }
}
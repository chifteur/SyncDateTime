using System;
using SyncDateTime.Model;

namespace SyncDateTime.Design
{
    public class DesignDataService : IDataService
    {


        public void SyncFolder(DataOption option, Action<string, int> callLog)
        {
            //throw new NotImplementedException();
            callLog("Design Time", 0);
        }
    }
}
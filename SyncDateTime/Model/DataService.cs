using SyncDateTime.Properties;
using System;

namespace SyncDateTime.Model
{
    public class DataService : IDataService
    {
        //public void GetData(Action<DataItem, Exception> callback)
        //{
        //    // Use this to connect to the actual data service

        //    var item = new DataItem("Welcome to MVVM Light");
        //    callback(item, null);
        //}

        public DataService()
        {
            //
        }

        public void SyncFolder(Action<string,int> callLog,Action<bool> callback)
        {
            //
            try
            {
                var pro = new BaseSyncProcess();
                pro.Run(Settings.Default.SourceFolder, Settings.Default.TargetFolder, callLog, callback);               
            }
            catch (Exception ex)
            {
                callLog("Error :"+ex.Message,0);
                callback(false);
            }
        }
    }
}
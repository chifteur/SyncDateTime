﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncDateTime.Model
{
    public interface IDataService
    {
        void SyncFolder(Action<string,int> callLog, Action<bool> callback);
    }
}
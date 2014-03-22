using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDateTime.Model
{
    public class DataOption
    {
        #region Properties
        public bool CreateDate { get; set; }
        public bool ModDateTime { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public bool IsValide { get { return CreateDate || ModDateTime; } }
        #endregion

        #region methods
        public DataOption Clone()
        {
            var n = new DataOption();
            n.CreateDate = this.CreateDate;
            n.ModDateTime = this.ModDateTime;
            n.SourcePath = this.SourcePath;
            n.TargetPath = this.TargetPath;

            return n;
        }
        #endregion


        
    }
}

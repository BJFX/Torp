using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;

namespace LOUV.Torp.BaseType
{
    public class CommGPS
    {
        public string Comm { get; set; }
        public int DataRate { get; set; }
        public int GPSPort { get; set; }
    }

    public class CommNet
    {
        public int CmdPort { get; set; }
        public int DataPort { get; set; }
        public int BroadPort { get; set; }
        public int RecvPort { get; set; }

    }


    public class Buoy
    {
        public string Name
        {
            get { return "浮标-" + id.ToString("D2"); }
        }
        public int id { get; set; }
        public string IP { get; set; }
        public string Memo { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
    }

    [Serializable]
    public class InitialData
    {
        public Hashtable BuyoArray;
    }
}

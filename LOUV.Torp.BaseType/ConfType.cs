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
    }

    public class CommNet
    {
        public string IP { get; set; }
        public int CmdPort { get; set; }
        public int DataPort { get; set; }
        public int BroadPort { get; set; }
        public int RecvPort { get; set; }

    }
    public struct Offset{
        public double Lat;
        public double Lng;
        public static Offset operator -(Offset off)
        {
            var newoff = new Offset();
            newoff.Lat = -off.Lat;
            newoff.Lng = -off.Lng;
            return newoff;
        }
    }
    public class MapCfg
    {
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public string Title { get; set; }
        //public string AccessMode { get; set; }
        public string MapType { get; set; }
        public Offset MapOffset { get; set; }  

    }

    public class Buoy
    {
        public string Name
        {
            get { return "浮标-" + id.ToString("D2"); }
        }
        public int id { get; set; }
        public string Memo { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Range { get; set; }
    }

    public class Object
    {

    }

    [Serializable]
    public class InitialData
    {
        public Hashtable BuyoArray;
    }
}

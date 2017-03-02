using GMap.NET;
using LOUV.Torp.Utility;
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

    public class Setup
    {
        public int UseProfile { get; set; }
        public float AcouVel { get; set; }
        public float Offset { get; set; }
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
    [Serializable]
    public class Buoy
    {
        public string Name
        {
            get { return "浮标-" + id.ToString("D2"); }
        }
        public string Time
        {
            get { return gps.UTCTime.ToShortTimeString(); }
        }
        public int id { get; set; }

        public GpsInfo gps { get; set; }
        public LiteRange liteRange { get; set; }
        public TeleRange teleRange { get; set; }
        public string Memo { get; set; }

        public float Range { get; set; }

        public Buoy(int id)
        {
            id = id;
        }
        public Buoy()
        {
        }
 
    }
    [Serializable]
    public class GpsInfo
    {
        public DateTime UTCTime;
        public float Longitude;
        public float Latitude;
    }
    [Serializable]
    public class LiteRange
    {
        public double RelativePara;
        public UInt16 RecvGain;
        public Int32 PeakPosition;
    }
    [Serializable]
    public class TeleRange
    {
        public Int32 SamplingStart;
        public float RecvDelay;
        public byte ModemStyle;
        public Int16 Crc;
        public float Dopple;
        public UInt16 MsgLength;
        public byte[] Msg;

        public string Message
        {
            get
            {
                var da = new DateTime(BitConverter.ToInt64(Msg, 0));
                var buf = new byte[MsgLength - 4];
                Buffer.BlockCopy(Msg, 4, buf, 0, MsgLength - 4);
                return da.ToShortTimeString() + ":" + Util.ConvertCharToHex(buf);
            }
        }//utc time +0xFF
    }
    public class Target
    {
        public string Name { get; set; }
        public string Memo { get; set; }

        public string Time
        {
            get { return UTCTime.ToShortTimeString(); }
        }

        public DateTime UTCTime{ get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }

        public Target(string name="目标")
        {
            Name = name;
        }
    }

    [Serializable]
    public class InitialData
    {
        public Hashtable buoy;
        public Hashtable info;
    }
}

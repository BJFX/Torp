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
        public List<string> IP { get; set; }
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
        public int TimeOut { get; set; }
        public int ValidInterval { get; set; }
        public double SonarDepth { get; set; }
        public int PreAdjust { get; set; }
    }
    

    public class MapCfg
    {
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public string Title { get; set; }
        public string AccessMode { get; set; }
        public string MapType { get; set; }
        public Offset MapOffset { get; set; } 

    }
    [Serializable]
    public class Buoy
    {
        public string Name
        {
            get { return "浮标-" + Id.ToString("D2"); }
        }
        public string Time
        {
            get
            {
                if (gps == null)
                    return DateTime.UtcNow.ToLongTimeString();
                return gps.UTCTime.ToLongTimeString();
            }
        }
        public int Id { get; set; }
        //public string Memo { get; set; }
        public float Range { get; set; }

//        public float Longitude
//        {
//            get { return gps.Longitude; }
//        }
//
//        public float Latitude
//        {
//            get { return gps.Latitude; }
//        }
        public GpsInfo gps { get; set; }
        public LiteRange liteRange { get; set; }
        public TeleRange teleRange { get; set; }
        public string IP { get; set; }

        public Buoy(int id=0)
        {
            Id = id;
            gps = new GpsInfo();
        }
 
    }
    [Serializable]
    public class GpsInfo
    {
        public DateTime UTCTime;
        public float Longitude { get; set; }
        public float Latitude{ get; set; }
        public GpsInfo()
        {
            UTCTime = DateTime.UtcNow;
            Longitude = 0;
            Latitude = 0;
        }
    }
    [Serializable]
    public class LiteRange
    {
        public float RelativePara1 { get; set; }
        public float RelativePara2 { get; set; }
        public UInt16 RecvGain { get; set; }
        public Int32 PeakPosition { get; set; }

        public LiteRange()
        {
            RelativePara1 = 0;
            RelativePara2 = 0;
            RecvGain = 0;
            PeakPosition = 0;
        }
    }
    [Serializable]
    public class TeleRange
    {
        public static int pressureadjust =377;
        public TeleRange()
        {
            SamplingStart = 0;
            RecvDelay = 0;
            ModemStyle = 0;
            Crc = 1;//error
            Dopple = 0;
            MsgLength = 19;
            Msg = null;
            ba = null;
        }
        public Int32 SamplingStart { get; set; }
        public float RecvDelay { get; set; }
        public byte ModemStyle { get; set; }
        public Int16 Crc { get; set; }
        public float Dopple { get; set; }
        public UInt16 MsgLength { get; set; }
        public byte[] Msg;
        public BitArray ba;

        public string MessageType
        {
            get
            {
                if (ba == null)
                    return "";
                if(Util.GetIntValueFromBit(ba,0,3)==2)
                {
                    return "AUV->浮标";
                }
                if(Util.GetIntValueFromBit(ba, 0, 3) == 6)
                {
                    return "AUV->回应浮标";
                }
                return "不支持的消息类型";
            }
        }
        public string MessageTime
        {
            get
            {
                if (ba == null)
                    return "";
                var secs = Util.GetIntValueFromBit(ba, 3, 17);
                var time = DateTime.UtcNow.Date.AddSeconds(secs);
                return time.ToLongTimeString();
            }
        }
        public string Presure
        {
            get
            {
                if (ba == null)
                    return "";

                var presure = (Util.GetIntValueFromBit(ba, 20, 12)- pressureadjust)* 0.133935287;
                
                return presure.ToString();
            }
        }
        public string Message
        {
            get
            {
                if (Msg == null)
                    return "";
                var buf = new byte[MsgLength - 4];
                Buffer.BlockCopy(Msg, 4, buf, 0, MsgLength - 4);
                return Util.ConvertCharToHex(buf);
            }
        }
    }
    public class Target
    {
        public string Name { get; set; }
        public string Status { get; set; }

        public string Time
        {
            get { return UTCTime.ToLongTimeString(); }
        }

        public DateTime UTCTime{ get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Depth { get; set; }
        public Target(string name="目标")
        {
            Name = name;
            Status = "无定位";
        }
    }

    [Serializable]
    public class InitialData
    {
        public Hashtable buoy;
        //public Hashtable info;
    }

    public class Locate2D
    {
        public DateTime Time;
        public double Lng;
        public double Lat;
        //public double Z;//Positive, Depth; 
        public double Range;
        public Locate2D(DateTime time, double lng =0, double lat =0,  double range = 0)
        {
            Time = time;
            Lng = lng;
            Lat = lat;
            //Z = z;
            Range = range;
        }
    }
    public class Locate3D
    {
        public DateTime Time;
        public double centerLng;
        public double centerLat;
        public double X;
        public double Y;
        public double Z;//Positive, Depth; 
        //public double Range;
        public Locate3D(DateTime time, double x = 0, double y = 0, double z = 0)
        {
            Time = time;
            X = x;
            Y = y;
            Z = z;
        }
    }
}

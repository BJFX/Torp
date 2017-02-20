using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMEA0183;
using LOUV.Torp.Utility;
namespace LOUV.Torp.MonProtocol
{
    public class GpsInfo
    {
        public DateTime UTCTime;
        public float Longitude;
        public float Latitude;
    }

    public class LiteRange
    {
        public double RelativePara;
        public UInt16 RecvGain;
        public Int32 PeakPosition;
    }

    public class TeleRange
    {
        public Int32 SamplingStart;
        public float RecvDelay;
        public byte ModemStyle;
        public Int16 Crc;
        public float Dopple;
        public UInt16  MsgLength;
        public byte[] Msg;

        public string Message
        {
            get
            {
                var da = new DateTime(BitConverter.ToInt64(Msg, 0));
                var buf = new byte[MsgLength - 4];
                Buffer.BlockCopy(Msg,4,buf,0,MsgLength - 4);
                return da.ToShortTimeString()+":"+Util.ConvertCharToHex(buf);
            }
        }//utc time +0xFF
    }

    public class PulseRange
    {
        public LiteRange range;
        public GpsInfo gps;
    }

    public class CommRange
    {
        public LiteRange range;
        public GpsInfo gps;
        public TeleRange telerange;
    }
    public class MonProtocol
    {
        public static GpsInfo ParseGps(byte[] buffer)
        {
            if (GPS.Parse(Encoding.Default.GetString(buffer)))
            {
                GpsInfo info = new GpsInfo();
                info.UTCTime = GPS.UTCTime;
                info.Latitude = GPS.Latitude;
                info.Longitude = GPS.Longitude;
                return info;
            }
            return null;
        }

        public static LiteRange ParsePulseRange(byte[] buffer)
        {
            var range = new LiteRange();
            range.RelativePara = BitConverter.ToDouble(buffer, 0);
            range.RecvGain = BitConverter.ToUInt16(buffer, 8);
            range.PeakPosition = BitConverter.ToInt32(buffer, 10);

            return range;
        }
        public static TeleRange ParseTeleRange(byte[] buffer,UInt16 msglength)
        {
            var range = new TeleRange();
   
            range.SamplingStart = BitConverter.ToInt32(buffer, 0);
            range.RecvDelay = BitConverter.ToSingle(buffer, 4);
            range.ModemStyle = (byte)BitConverter.ToChar(buffer, 8);
            range.Crc = BitConverter.ToInt16(buffer, 9);
            range.Dopple = BitConverter.ToSingle(buffer, 11);
            range.MsgLength = msglength;
            range.Msg = new byte[msglength];
            Buffer.BlockCopy(buffer,13,range.Msg,0,msglength);
            return range;
        }
    }
}

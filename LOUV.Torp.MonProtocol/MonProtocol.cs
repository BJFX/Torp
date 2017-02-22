using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMEA0183;
using LOUV.Torp.Utility;
using LOUV.Torp.BaseType;
namespace LOUV.Torp.MonProtocol
{
    
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

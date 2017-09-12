using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMEA0183;
using LOUV.Torp.Utility;
using LOUV.Torp.BaseType;
using System.Collections;

namespace LOUV.Torp.MonProtocol
{
    public class MonProtocol
    {
        public static float Velocity{ get; set; }
        public static float FixedOffset { get; set; }
        public static GpsInfo ParseGps(byte[] buffer)
        {
            if (GPS.Parse(Encoding.Default.GetString(buffer)))
            {
                GpsInfo info = new GpsInfo()
                {
                    UTCTime = GPS.UTCTime,
                    Latitude = GPS.Latitude,
                    Longitude = GPS.Longitude
                };
                return info;
            }
            return null;
        }

        public static LiteRange ParsePulseRange(byte[] buffer,bool lite)
        {
            var range = new LiteRange()
            {
                RelativePara1 = BitConverter.ToSingle(buffer, 0),
                RelativePara2 = BitConverter.ToSingle(buffer, 4),
                RecvGain = BitConverter.ToUInt16(buffer, 8),
                PeakPosition = BitConverter.ToInt32(buffer, 10),   
            };
            if (!lite)
            {
                range.SampleStartTime = BitConverter.ToInt32(buffer, 198);
                range.ID = (byte)BitConverter.ToChar(buffer, 202);
            }
            return range;
        }
        public static TeleRange ParseTeleRange(byte[] buffer,UInt16 msglength)
        {
            var range = new TeleRange()
            {
                SamplingStart = BitConverter.ToInt32(buffer, 0),
                RecvDelay = BitConverter.ToSingle(buffer, 4),
                ModemStyle = (byte)BitConverter.ToChar(buffer, 8),
                Crc = BitConverter.ToInt16(buffer, 9),
                Dopple = BitConverter.ToSingle(buffer, 11),
                ID = (byte)BitConverter.ToChar(buffer, 240),
                MsgLength = msglength,
                Msg = new byte[msglength]
            };
            Buffer.BlockCopy(buffer,17,range.Msg,0,msglength);
            byte[] msghead = new byte[4];
            Buffer.BlockCopy(range.Msg, 0, msghead, 0, 4);
            range.ba = new BitArray(msghead);
            return range;
        }
        //
        public static float CalDistanceByLite(LiteRange range)
        {
            if(range==null)
            {
                return float.Epsilon;
            }
            if (range.PeakPosition < 0)
                range.PeakPosition += 160000;
            var distance = (float)(range.PeakPosition * Velocity / 80000.0  + FixedOffset);
            return distance <0?0: distance;
        }
        public static float CalDistanceByTele(LiteRange literange,TeleRange telerange)
        {
            if (telerange == null&& literange==null)
            {
                return float.Epsilon;
            }
            if(telerange.Crc==0)
            {
                var distance = telerange.RecvDelay * Velocity + FixedOffset;
                return distance < 0 ? 0 : distance;
            }
            else
            {
                return CalDistanceByLite(literange);
            }
        }
    }
}

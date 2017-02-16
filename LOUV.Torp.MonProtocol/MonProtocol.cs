using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMEA0183;
namespace LOUV.Torp.MonProtocol
{
    public class GpsInfo
    {
        public DateTime UTCTime;
        public float Longitude;
        public float Latitude;
    }
    public class MonProtocol
    {
        public static GpsInfo ParseGps(byte[] buffer)
        {
            byte[] buf = new byte[1030];
            Buffer.BlockCopy(buffer, 2, buf, 0, 1030);
            if (GPS.Parse(Encoding.Default.GetString(buf)))
            {
                GpsInfo info = new GpsInfo();
                info.UTCTime = GPS.UTCTime;
                info.Latitude = GPS.Latitude;
                info.Longitude = GPS.Longitude;
                return info;
            }
            return null;
        }

        public static GpsInfo ParseRange(byte[] buffer)
        {
            GpsInfo info = new GpsInfo();
            if (GPS.Parse(Encoding.Default.GetString(buffer)))
            {
                info.UTCTime = GPS.UTCTime;
                info.Latitude = GPS.Latitude;
                info.Longitude = GPS.Longitude;
            }
            return info;
        }
        public static GpsInfo ParseTeleRange(byte[] buffer)
        {
            GpsInfo info = new GpsInfo();
            if (GPS.Parse(Encoding.Default.GetString(buffer)))
            {
                info.UTCTime = GPS.UTCTime;
                info.Latitude = GPS.Latitude;
                info.Longitude = GPS.Longitude;
            }
            return info;
        }
    }
}

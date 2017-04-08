using GMap.NET;
using System;
using System.Collections;

namespace LOUV.Torp.Utility
{
    public class Util
    {
        static double R = 6378137; // WGS-84;
        public static string ConvertCharToHex(byte[] str)
        {
            string data = "";
            for (int i = 0; i < str.Length; i++)
            {
                string s = Convert.ToString(str[i], 16);
                if (s.Length == 1)
                {
                    s = "0" + s;
                }
                data += s;
            }

            return data.ToUpper();

        }
        static public int GetIntValueFromBit(BitArray data, int startindex,int bitlen)
        {
            int[] value = new int[1];
            BitArray ba = new BitArray(bitlen);
            for (int i = 0; i < bitlen; i++)
            {
                ba[i] = data[startindex+i];
            }

            ba.CopyTo(value, 0);
            return value[0];
        }
        public static double CalcDistance(PointLatLng start, PointLatLng end)
        {
            double pidiv180 = Math.PI / 180;
            double a = Math.Pow(Math.Sin((start.Lat - end.Lat) / 2 * pidiv180), 2)
                + Math.Cos(start.Lat * pidiv180) * Math.Cos(end.Lat * pidiv180)
                * Math.Pow(Math.Sin((start.Lng - end.Lng) / 2 * pidiv180), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        //only used in north earth
        public static void GetReltXY(PointLatLng p, PointLatLng center, out double x, out double y)
        {
            PointLatLng pxcenter = new PointLatLng(center.Lat, p.Lng);
            PointLatLng pycenter = new PointLatLng(p.Lat,center.Lng);
            var absx = CalcDistance(p, pycenter);
            var absy = CalcDistance(p, pxcenter);
            x = -absx;
            y = -absy;
            if (p.Lng > center.Lng)
                x = absx;          
            if (p.Lat > center.Lat)
                y = absy;
        }

        public static double LongOffset(double LonLocal, double LatLocal, double x)
        {
            var LonTarget = LonLocal + x / (111.3 * 1000 * Math.Abs(Math.Cos(LatLocal / 180 * Math.PI)));
            if (LonTarget > 180)
            {
                LonTarget -= 360;
            }
            else if (LonTarget < -180)
            {
                LonTarget += 360;
            }
            return LonTarget;
        }
        public static double LatOffset(double LatLocal, double y)
        {
            var LatTarget = LatLocal + y / (111.3 * 1000);
            if (LatTarget > 90)
            {
                LatTarget = 180 - LatTarget;
            }
            else if (LatTarget < -90)
            {
                LatTarget = -180 - LatTarget;
            }
            return LatTarget;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.BaseType;
using System.Diagnostics;
using GMap.NET;

namespace LOUV.Torp.MonProtocol
{
    public class TriangleLocate
    {

        private static double x1, y1, z1,range1, x2, y2, z2,range2, x3, y3, z3,range3;

        public static SortedList<int,Locate2D> Buoys = new SortedList<int,Locate2D>(3);
        public static void Init()
        {
            x1 = y1 = z1 = x2 = y2 = z2 = x3 = y3 = z3 = range1 = range2 = range3 = 0;
        }
        //validate the input 3 buoy position
        //if their utctime is not updated then remove old data and init all local data.
        //return true when data is updated and assign them to local viarable.
        //need add lock when call this function
        //if count of buoy less then 3, return false and do nothing
        public static bool Valid(int timeout)
        {
            if (Buoys.Count < 3)
                return false;
            var nowtime = DateTime.UtcNow;
            for(int i= Buoys.Count-1; i>=0; i--)
            {
                if(Math.Abs(nowtime.Subtract(Buoys.Values[i].Time).TotalSeconds)>timeout)
                {
                    Buoys.Remove(i);
                }
                
            }
            if (Buoys.Count < 3)
                return false;
            //find latest 3 data
            if(Buoys.Count==4)
            {
                int indexOld = 1;
                DateTime oldtime = Buoys[indexOld].Time;
                for(int i=1;i<=Buoys.Count;i++)
                {
                    if (Buoys[i].Time < oldtime)
                    {
                        oldtime = Buoys[i].Time;
                        indexOld = i;
                    }
                }
                Buoys.Remove(indexOld);
            }
            var center = new PointLatLng((Buoys.Values[0].Lat + Buoys.Values[1].Lat + Buoys.Values[2].Lat) / 3,
                (Buoys.Values[0].Lng + Buoys.Values[1].Lng + Buoys.Values[2].Lng) / 3);
            var buoy1 = new PointLatLng(Buoys.Values[0].Lat, Buoys.Values[0].Lng);
            Utility.Util.GetReltXY(buoy1,center,out x1,out y1);
            
            z1 = 0;
            var buoy2 = new PointLatLng(Buoys.Values[1].Lat, Buoys.Values[1].Lng);
            Utility.Util.GetReltXY(buoy2, center, out x2, out y2);
            z2 = 0;
            var buoy3 = new PointLatLng(Buoys.Values[2].Lat, Buoys.Values[2].Lng);
            Utility.Util.GetReltXY(buoy3, center, out x3, out y3);
            z3 = 0;
            range1 = Buoys.Values[0].Range;
            range2 = Buoys.Values[1].Range;
            range3 = Buoys.Values[2].Range;
            return true;
        }
        public static bool CalTargetLocation(out Locate3D position)
        {
            double x, y = 0;
            
            Matrix m = new Matrix();
            Input i1 = new Input()
            {
                x = x1 ,
                y = y1 ,
                z = z1 ,
                r = range1 ,
            };
            Input i2 = new Input()
            {
                x = x2 ,
                y = y2 ,
                z = z2 ,
                r = range2 ,
            };
            Input i3 = new Input()
            {
                x = x3 ,
                y = y3 ,
                z = z3 ,
                r = range3 ,
            };
            double[] D = new double[3];
            MatrixLocate.InitMatrix(ref m, i1, i2, i3, ref D);
            if(MatrixLocate.locate(m, D, out x, out y)!=1)
            {
                position = new Locate3D(DateTime.Now);
                return false;
            }

            double z = Math.Sqrt(range1 * range1 - (y - y1) * (y - y1) - (x - x1) * (x - x1));
            position = new Locate3D(DateTime.Now, x, y, z);
            return true;
        }
    }
}

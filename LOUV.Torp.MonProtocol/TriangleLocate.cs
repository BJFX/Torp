using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.BaseType;
using System.Diagnostics;
namespace LOUV.Torp.MonProtocol
{
    public class TriangleLocate
    {

        private static double x1, y1, z1,range1, x2, y2, z2,range2, x3, y3, z3,range3;

        public static SortedList<int,Locate3D> Buoys = new SortedList<int,Locate3D>(3);
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
            for(int i= Buoys.Count; i>0; i--)
            {
                if(Math.Abs(nowtime.Subtract(Buoys[i].Time).TotalSeconds)>timeout)
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
            x1 = Buoys.Values[0].X;
            y1 = Buoys.Values[0].Y;
            z1 = Buoys.Values[0].Z;
            x2 = Buoys.Values[1].X;
            y2 = Buoys.Values[1].Y;
            z2 = Buoys.Values[1].Z;
            x3 = Buoys.Values[2].X;
            y3 = Buoys.Values[2].Y;
            z3 = Buoys.Values[2].Z;
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
                x = x1,
                y = y1,
                z = z1,
                r = range1,
            };
            Input i2 = new Input()
            {
                x = x2,
                y = y2,
                z = z2,
                r = range2,
            };
            Input i3 = new Input()
            {
                x = x3,
                y = y3,
                z = z3,
                r = range3,
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LOUV.Torp.BaseType;
namespace LOUV.Torp.MonProtocol
{
    public class TriangleLocate
    {
        private static int N = 3;
        private static int N2 = N * N;//jacobi
        private static double Eps = 0.0001F;
        private static double x1, y1, z1, x2, y2, z2, x3, y3, z3;
        public static SortedList<int,Locate3D> Buoys = new SortedList<int,Locate3D>(3);
        public static void Init()
        {
            x1 = y1 = z1 = x2 = y2 = z2 = x3 = y3 = z3 = 0;
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
                if(Math.Abs(nowtime.Subtract(Buoys[i].Time).TotalSeconds)>timeout)
                {
                    Buoys.RemoveAt(i);
                }
                
            }
            if (Buoys.Count < 3)
                return false;
            //find latest 3 data
            if(Buoys.Count==4)
            {
                int indexOld = 0;
                DateTime oldtime = Buoys[indexOld].Time;
                for(int i=1;i<Buoys.Count;i++)
                {
                    if (Buoys[i].Time < oldtime)
                    {
                        oldtime = Buoys[i].Time;
                        indexOld = i;
                    }
                }
                Buoys.RemoveAt(indexOld);
            }
            x1 = Buoys[0].X;
            y1 = Buoys[0].Y;
            z1 = Buoys[0].Z;
            x2 = Buoys[1].X;
            y2 = Buoys[1].Y;
            z2 = Buoys[1].Z;
            x3 = Buoys[2].X;
            y3 = Buoys[2].Y;
            z3 = Buoys[2].Z;
            return true;
        }
        public static bool CalTargetLocation(out Locate3D position)
        {
            position = new Locate3D(DateTime.Now);
            return false;
        }
    }
}

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
        public static bool Valid(int timeout,ref PointLatLng center)
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
            center = new PointLatLng((Buoys.Values[0].Lat + Buoys.Values[1].Lat + Buoys.Values[2].Lat) / 3,
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

        public static bool CalTargetByMatrix(out Locate3D position)
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
            position = new Locate3D(DateTime.Now);
            if (MatrixLocate.locate(m, D, out x, out y)!=1)
            {
                return false;
            }
            var diff = range1 * range1 - (y - y1) * (y - y1) - (x - x1) * (x - x1);
            if (diff < 0)
                return false;
            double z = Math.Sqrt(diff);
            position = new Locate3D(DateTime.Now, x, y, z);
            return true;
        }



        public static double CalTargetByApproach(out Locate3D position)
        {

        }
        #region method used in Approach
        private double[] solveLongBaseEquation(double[] buoy1, double[] buoy2, double[] buoy3, double[] buoy4, int buoyNum, double[] dis)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            double distanceTemp = 0;
            double distanceMin = float.MaxValue;

            double[] buoySphere1 = degree2Sphere(buoy1);
            double[] buoySphere2 = degree2Sphere(buoy2);
            double[] buoySphere3 = degree2Sphere(buoy3);
            double[] buoySphere4 = degree2Sphere(buoy4);

            double[] disTemp = new double[4];

            double[] test = new double[3] { 0, 0, earthRadius - 0.02 };
            double[] testSphere = new double[3] { 0, 0, earthRadius - 0.02 };
            double[] output = new double[3];

            stopwatch.Start();
            for (test[0] = 0.5; test[0] < 180; test[0] += 1)
            {
                for (test[1] = 0.5; test[1] < 360; test[1] += 1)
                {
                    testSphere[0] = test[0] / 180 * Math.PI;
                    testSphere[1] = test[1] / 180 * Math.PI;
                    disTemp[0] = sphere2Distance(buoySphere1, testSphere) - dis[0];
                    disTemp[1] = sphere2Distance(buoySphere2, testSphere) - dis[1];
                    disTemp[2] = sphere2Distance(buoySphere3, testSphere) - dis[2];
                    disTemp[3] = sphere2Distance(buoySphere4, testSphere) - dis[3];
                    distanceTemp = disTemp[0] * disTemp[0] + disTemp[1] * disTemp[1] + disTemp[2] * disTemp[2];
                    if (buoyNum >= 4)
                    {
                        distanceTemp = distanceTemp + disTemp[3] * disTemp[3];
                    }
                    if (distanceTemp < distanceMin)
                    {
                        distanceMin = distanceTemp;
                        Array.Copy(test, output, 3);
                    }
                }
            }
            stopwatch.Stop();
            double precision = 1;

            for (int i = 0; i < 8; i++)
            {
                precision = precision / 10;
                double depthPrecision = precision / 10;
                if (depthPrecision < 0.0001)
                    depthPrecision = 0.0001;
                double min0 = Math.Floor((output[0] - precision) / precision / 10) * precision * 10;
                double max0 = Math.Ceiling((output[0] + precision) / precision / 10) * precision * 10;

                double min1 = Math.Floor((output[1] - precision) / precision / 10) * precision * 10;
                double max1 = Math.Ceiling((output[1] + precision) / precision / 10) * precision * 10;

                double min2 = Math.Floor((output[2] - depthPrecision) / depthPrecision / 10) * depthPrecision * 10;
                double max2 = Math.Ceiling((output[2] + depthPrecision) / depthPrecision / 10) * depthPrecision * 10;

                distanceMin = float.MaxValue;
                for (test[0] = min0; test[0] < max0; test[0] += precision)
                {
                    for (test[1] = min1; test[1] < max1; test[1] += precision)
                    {
                        for (test[2] = min2; test[2] < max2; test[2] += depthPrecision)
                        {
                            testSphere[0] = test[0] / 180 * Math.PI;
                            testSphere[1] = test[1] / 180 * Math.PI;
                            testSphere[2] = test[2];
                            disTemp[0] = sphere2Distance(buoySphere1, testSphere) - dis[0];
                            disTemp[1] = sphere2Distance(buoySphere2, testSphere) - dis[1];
                            disTemp[2] = sphere2Distance(buoySphere3, testSphere) - dis[2];
                            disTemp[3] = sphere2Distance(buoySphere4, testSphere) - dis[3];
                            distanceTemp = disTemp[0] * disTemp[0] + disTemp[1] * disTemp[1] + disTemp[2] * disTemp[2];
                            if (buoyNum >= 4)
                            {
                                distanceTemp = distanceTemp + disTemp[3] * disTemp[3];
                            }
                            if (distanceTemp < distanceMin)
                            {
                                distanceMin = distanceTemp;
                                Array.Copy(test, output, 3);
                            }
                        }
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine("{0}", stopwatch.Elapsed);

            double[] res = new double[4];


            res[0] = 90 - output[0];
            if (output[1] > 180)
                res[1] = output[1] - 360;
            else
                res[1] = output[1];

            res[2] = output[2];
            res[3] = Math.Sqrt(distanceMin) * 1000;
            return res;
        }
        private double[] degree2Sphere(double[] degree)
        {

            double[] res = new double[3];
            res[0] = (90.0 - degree[0]) / 180 * Math.PI;
            if (degree[1] < 0.0)
            {
                res[1] = (degree[1] + 360.0) / 180 * Math.PI; ;
            }
            else
            {
                res[1] = degree[1] / 180 * Math.PI;
            }
            res[2] = degree[2];
            return res;

        }

        private double[] sphere2Decare(double[] sphere)
        {
            double[] xyz1 = new double[3];

            xyz1[0] = sphere[2] * Math.Sin(sphere[0]) * Math.Cos(sphere[1]);
            xyz1[1] = sphere[2] * Math.Sin(sphere[0]) * Math.Sin(sphere[1]);
            xyz1[2] = sphere[2] * Math.Cos(sphere[0]);

            return xyz1;
        }

        private double decare2Distance(double[] decare1, double[] decare2)
        {
            double a = decare1[0] - decare2[0];
            double b = decare1[1] - decare2[1];
            double c = decare1[2] - decare2[2];

            return Math.Sqrt(a * a + b * b + c * c);
        }
        private double sphere2Distance(double[] degree1, double[] degree2)
        {
            double[] xyz1 = new double[3];
            double[] xyz2 = new double[3];

            xyz1[0] = degree1[2] * Math.Sin(degree1[0]) * Math.Cos(degree1[1]);
            xyz1[1] = degree1[2] * Math.Sin(degree1[0]) * Math.Sin(degree1[1]);
            xyz1[2] = degree1[2] * Math.Cos(degree1[0]);

            xyz2[0] = degree2[2] * Math.Sin(degree2[0]) * Math.Cos(degree2[1]);
            xyz2[1] = degree2[2] * Math.Sin(degree2[0]) * Math.Sin(degree2[1]);
            xyz2[2] = degree2[2] * Math.Cos(degree2[0]);

            double straightLineDistance = Math.Sqrt((xyz1[0] - xyz2[0]) * (xyz1[0] - xyz2[0]) + (xyz1[1] - xyz2[1]) * (xyz1[1] - xyz2[1]) + (xyz1[2] - xyz2[2]) * (xyz1[2] - xyz2[2]));
            return straightLineDistance;
        }
        #endregion
    }
}

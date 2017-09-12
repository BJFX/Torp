using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using LOUV.Torp.BaseType;
using System.Diagnostics;
using GMap.NET;

namespace LOUV.Torp.MonProtocol
{
    [TestFixture]
    public class TriangleLocateTest
    {
        Matrix m;
        Input i1, i2, i3;
        double[] D;
        double x1, y1, z1, range1, x2, y2, z2, range2, x3, y3, z3, range3;
        [SetUp]
        public void Init()
        {
            TriangleLocate locate = new TriangleLocate();
            var lpoint1 = new Locate2D(DateTime.UtcNow, 116.3187, 39.98544, 41.09);
            locate.Buoys.Add(1, lpoint1);
            var lpoint2 = new Locate2D(DateTime.UtcNow, 116.3184, 39.98542, 22.23);
            locate.Buoys.Add(2, lpoint2);
            var lpoint3 = new Locate2D(DateTime.UtcNow, 116.3184, 39.98556, 23.64);
            locate.Buoys.Add(4, lpoint3);
            var center = new PointLatLng((locate.Buoys.Values[0].Lat + locate.Buoys.Values[1].Lat + locate.Buoys.Values[2].Lat) / 3,
                (locate.Buoys.Values[0].Lng + locate.Buoys.Values[1].Lng + locate.Buoys.Values[2].Lng) / 3);
            var buoy1 = new PointLatLng(locate.Buoys.Values[0].Lat, locate.Buoys.Values[0].Lng);
            Utility.Util.GetReltXY(buoy1, center, out x1, out y1);

            z1 = 0;
            var buoy2 = new PointLatLng(locate.Buoys.Values[1].Lat, locate.Buoys.Values[1].Lng);
            Utility.Util.GetReltXY(buoy2, center, out x2, out y2);
            z2 = 0;
            var buoy3 = new PointLatLng(locate.Buoys.Values[2].Lat, locate.Buoys.Values[2].Lng);
            Utility.Util.GetReltXY(buoy3, center, out x3, out y3);
            z3 = 0;
            range1 = locate.Buoys.Values[0].Range;
            range2 = locate.Buoys.Values[1].Range;
            range3 = locate.Buoys.Values[2].Range;
            m = new Matrix();
             i1 = new Input()
            {
                x = x1,
                y = y1,
                z = z1,
                r = range1,
            };
             i2 = new Input()
            {
                x = x2,
                y = y2,
                z = z2,
                r = range2,
            };
             i3 = new Input()
            {
                x = x3,
                y = y3,
                z = z3,
                r = range3,
            };
            D = new double[3];
            MatrixLocate.InitMatrix(ref m, i1, i2, i3, ref D);
        }
        [Test]
        public void CalTargetLocationTest()
        {
            double x, y = 0;
            if (MatrixLocate.locate(m, D, out x, out y)==1)
            {
                Assert.Warn("x:{0:F06},y:{1:F06}",x,y);
                if (Math.Abs(x) > range1 || Math.Abs(y) > range1)
                    Assert.Fail("can not locate object");
                double z = Math.Sqrt(range1 * range1 - (y - y1) * (y - y1) - (x - x1) * (x - x1));
                string log = "x:"+x.ToString("F06")+",y:"+y.ToString("F06")+",z:"+z.ToString("F06");
                Assert.Warn(log);
                Assert.Pass("locate passed!");
            }
            Assert.Fail();
        }
    }
}

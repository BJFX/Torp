using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using LOUV.Torp.BaseType;
using System.Diagnostics;
namespace LOUV.Torp.MonProtocol
{
    [TestFixture]
    public class TriangleLocateTest
    {
        Matrix m;
        Input i1, i2, i3;
        double[] D;
        [SetUp]
        public void Init()
        {
            
             m = new Matrix();
             i1 = new Input()
            {
                x = 0,
                y = 0,
                z = 1,
                r = 1.714,
            };
             i2 = new Input()
            {
                x = 0,
                y = 1,
                z = 1,
                r = 1.732,
            };
             i3 = new Input()
            {
                x = 1,
                y = 1,
                z = 1,
                r = 1.414,
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
                double z = Math.Sqrt(1.414 * 1.414 - (y - 0) * (y - 0) - (x - 0) * (x - 0)) + 1;
                Debug.WriteLine("x: {0:f6},y: {1:f6},z: {2:f6}", x, y, z);
                Assert.Pass("locate passed!");
            }
            Assert.Fail();
        }
    }
}

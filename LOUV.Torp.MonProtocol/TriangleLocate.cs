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
        private static float Eps = 0.0001F;
        public static List<Locate3D> Buoys = new List<Locate3D>(3);
        public static void Init()
        {
            Buoys.Clear();
        }
        public static Locate3D CalTargetLocation()
        {
            return new Locate3D(DateTime.UtcNow);
        }
    }
}

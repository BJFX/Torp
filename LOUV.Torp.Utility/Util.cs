using System;
using System.Collections;

namespace LOUV.Torp.Utility
{
    public class Util
    {
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
            for (int i = startindex; i < startindex+bitlen; i++)
            {
                ba[i] = data[i];
            }

            ba.CopyTo(value, 0);
            return value[0];
        }
    }
}

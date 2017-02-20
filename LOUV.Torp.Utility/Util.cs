using System;

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
    }
}

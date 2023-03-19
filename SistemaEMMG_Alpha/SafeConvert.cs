using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaEMMG_Alpha
{
    public static class SafeConvert
    {
        public static double ToDouble(string s)
        {
            double result;
            if (Double.TryParse(s, out result))
            {
                return result;
            } else
            {
                return 0.0;
            }
        }
        public static int ToInt32(string s)
        {
            int result;
            if (Int32.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        public static long ToInt64(string s)
        {
            long result;
            if (Int64.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
        public static bool ToBoolean(string s)
        {
            bool result;
            if (Boolean.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }
        public static bool ToBoolean(bool? b)
        {
            return Convert.ToBoolean(b);
        }

        public static string ToString(double d)
        {
            return Convert.ToString(d);
        }
    }
}

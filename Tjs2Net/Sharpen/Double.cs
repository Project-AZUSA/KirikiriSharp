using System;

namespace Tjs2.Sharpen
{
    class Double
    {
        public static Number ValueOf(double value)
        {
            return new Number(value);
        }

        public static bool IsNaN(double value)
        {
            return double.IsNaN(value);
        }

        public static bool IsInfinite(double value)
        {
            return double.IsInfinity(value);
        }

        public static int Compare(double lhs, double rhs)
        {
            double delta = lhs - rhs;
            if (Math.Abs(delta) < 0.000001)
            {
                return 0;
            }

            return (delta > 0) ? 1 : -1;
        }

        public const double NegativeInfinity = double.NegativeInfinity;

        public static long DoubleToLongBits(double val)
        {
            return BitConverter.DoubleToInt64Bits(val);
        }

        public static long DoubleToRawLongBits(double val)
        {
            return BitConverter.DoubleToInt64Bits(val);
        }

        public static double LongBitsToDouble(long val)
        {
            return BitConverter.Int64BitsToDouble(val);
        }
    }
}

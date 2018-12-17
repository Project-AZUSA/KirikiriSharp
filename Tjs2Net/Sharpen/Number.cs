namespace Tjs2.Sharpen
{
    public class Number
    {
        private object mValue;

        public Number(double value)
        {
            mValue = value;
        }

        public Number(float value)
        {
            mValue = value;
        }

        public Number(int value)
        {
            mValue = value;
        }

        public Number(long value)
        {
            mValue = value;
        }

        public Number(short value)
        {
            mValue = value;
        }

        public Number(byte value)
        {
            mValue = value;
        }

        public static implicit operator double(Number x)
        {
            return (double)x.mValue;
        }

        public static implicit operator float(Number x)
        {
            return (float)x.mValue;
        }

        public static implicit operator int(Number x)
        {
            return (int)x.mValue;
        }

        public static implicit operator long(Number x)
        {
            return (long)x.mValue;
        }

        public static implicit operator short(Number x)
        {
            return (short)x.mValue;
        }

        public static implicit operator byte(Number x)
        {
            return (byte)x.mValue;
        }

        public bool IsDouble()
        {
            return mValue is double;
        }

        public bool IsInt()
        {
            return mValue is int;
        }
    }
}
namespace Tjs2.Sharpen
{
    public class LongBuffer : ByteBuffer
    {
        public static LongBuffer Wrap(long[] data)
        {
            LongBuffer buf = new LongBuffer(new byte[data.Length * 8], 0, data.Length * 8);
            for (int i = 0; i < data.Length; i++)
            {
                buf.PutLong(data[i]);
            }
            return buf;
        }

        public LongBuffer() { }

        protected LongBuffer(byte[] buf, int start, int len)
            :base(buf, start, len)
        {
        }

        public void Put(int pos, long val)
        {
            int len = (pos * 8);
            Position(len);
            PutLong(val);
        }
    }
}

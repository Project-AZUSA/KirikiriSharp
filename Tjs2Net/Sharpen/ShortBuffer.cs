namespace Tjs2.Sharpen
{
    public class ShortBuffer : ByteBuffer
    {
        public static ShortBuffer Wrap(short[] data)
        {
            ShortBuffer buf = new ShortBuffer(new byte[data.Length * 2], 0, data.Length * 2);
            buf.Put(data);
            return buf;
        }

        public ShortBuffer() { }

        protected ShortBuffer(byte[] buf, int start, int len)
            :base(buf, start, len)
        {
        }

        public void Put(short val)
        {
            this.PutShort(val);
        }

        public void Put(short[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                this.PutShort(data[i]);
            }
        }
    }
}

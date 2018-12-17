namespace Tjs2.Sharpen
{
    public class IntBuffer : ByteBuffer
    {
        public static IntBuffer Wrap(int[] data)
        {
            IntBuffer buf = new IntBuffer(new byte[data.Length * 4], 0, data.Length * 4);
            for (int i = 0; i < data.Length; i++)
            {
                buf.PutInt(data[i]);
            }
            return buf;
        }

        public IntBuffer() { }

        protected IntBuffer(byte[] buf, int start, int len)
            :base(buf, start, len)
        {
        }

        public void Put(int val)
        {
            this.PutInt(val);
        }
    }
}

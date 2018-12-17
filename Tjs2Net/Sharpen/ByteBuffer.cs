namespace Tjs2.Sharpen
{
    public class ByteBuffer
	{
		private byte[] buffer;
		private DataConverter c;
		private int capacity;
		private int index;
		private int limit;
		private int mark;
		private int offset;
		private ByteOrder order;

		public ByteBuffer ()
		{
			this.c = DataConverter.BigEndian;
		}

		protected ByteBuffer (byte[] buf, int start, int len)
		{
			this.buffer = buf;
			this.offset = 0;
			this.limit = start + len;
			this.index = start;
			this.mark = start;
			this.capacity = buf.Length;
			this.c = DataConverter.BigEndian;
		}

		public static ByteBuffer Allocate (int size)
		{
			return new ByteBuffer (new byte[size], 0, size);
		}

		public static ByteBuffer AllocateDirect (int size)
		{
			return Allocate (size);
		}

		public byte[] Array ()
		{
			return buffer;
		}

		public int ArrayOffset ()
		{
			return offset;
		}

		public int Capacity ()
		{
			return capacity;
		}

		private void CheckGetLimit (int inc)
		{
			if ((index + inc) > limit) {
				throw new BufferUnderflowException ();
			}
		}

		private void CheckPutLimit (int inc)
		{
			if ((index + inc) > limit) {
				throw new BufferUnderflowException ();
			}
		}

		public void Clear ()
		{
			index = offset;
			limit = offset + capacity;
		}

		public void Flip ()
		{
			limit = index;
			index = offset;
		}

		public byte Get ()
		{
			CheckGetLimit (1);
			return buffer[index++];
		}

        public byte Get(int i)
        {
            return buffer[i];
        }

		public void Get (byte[] data)
		{
			Get (data, 0, data.Length);
		}

		public void Get (byte[] data, int start, int len)
		{
			CheckGetLimit (len);
			for (int i = 0; i < len; i++) {
				data[i + start] = buffer[index++];
			}
		}

		public int GetInt ()
		{
			CheckGetLimit (4);
			int num = c.GetInt32 (buffer, index);
			index += 4;
			return num;
		}

		public short GetShort ()
		{
			CheckGetLimit (2);
			short num = c.GetInt16 (buffer, index);
			index += 2;
			return num;
		}

		public bool HasArray ()
		{
			return true;
		}

		public int Limit ()
		{
			return (limit - offset);
		}

		public void Limit (int newLimit)
		{
			limit = newLimit;
		}

		public void Mark ()
		{
			mark = index;
		}

		public void Order (ByteOrder order)
		{
			this.order = order;
			if (order == ByteOrder.BIG_ENDIAN) {
				c = DataConverter.BigEndian;
			} else {
				c = DataConverter.LittleEndian;
			}
		}

		public int Position ()
		{
			return (index - offset);
		}

		public void Position (int pos)
		{
			if ((pos < offset) || (pos > limit)) {
				throw new BufferUnderflowException ();
			}
			index = pos + offset;
		}

		public void Put (byte[] data)
		{
			Put (data, 0, data.Length);
		}

        public void Put(ByteBuffer buf)
        {
            Put(buf.buffer, buf.ArrayOffset(), buf.Capacity());
        }

		public void Put (byte data)
		{
			CheckPutLimit (1);
			buffer[index++] = data;
		}

		public void Put (byte[] data, int start, int len)
		{
			CheckPutLimit (len);
			for (int i = 0; i < len; i++) {
				buffer[index++] = data[i + start];
			}
		}

		public void PutInt (int i)
		{
			Put (c.GetBytes (i));
		}

		public void PutShort (short i)
		{
			Put (c.GetBytes (i));
		}

        public void PutLong(long i)
        {
            Put(c.GetBytes(i));
        }

        public void PutChar(char ch)
        {
            this.Put((byte)ch);
        }

		public int Remaining ()
		{
			return (limit - index);
		}

		public void Reset ()
		{
			index = mark;
		}

		public ByteBuffer Slice ()
		{
			ByteBuffer b = Wrap (buffer, index, buffer.Length - index);
			b.offset = index;
			b.limit = limit;
			b.order = order;
			b.c = c;
			b.capacity = limit - index;
			return b;
		}

		public static ByteBuffer Wrap (byte[] buf)
		{
			return new ByteBuffer (buf, 0, buf.Length);
		}

		public static ByteBuffer Wrap (byte[] buf, int start, int len)
		{
			return new ByteBuffer (buf, start, len);
		}

        public static ByteOrder NativeOrder()
        {
            // TODO: default win32 order
            return ByteOrder.BIG_ENDIAN;
        }

        public ByteBuffer Duplicate()
        {
            ByteBuffer buf = new ByteBuffer();
            buf.buffer = new byte[this.buffer.Length];
            this.buffer.CopyTo(buf.buffer, 0);

            buf.c = this.c;
            buf.capacity = this.capacity;
            buf.index = this.index;
            buf.limit = this.limit;
            buf.mark = this.mark;
            buf.offset = this.offset;
            buf.order = this.order;
            return buf;
        }

        public LongBuffer AsLongBuffer()
        {
            LongBuffer buf = new LongBuffer();
            buf.buffer = new byte[this.buffer.Length];
            this.buffer.CopyTo(buf.buffer, 0);

            buf.c = this.c;
            buf.capacity = this.capacity;
            buf.index = this.index;
            buf.limit = this.limit;
            buf.mark = this.mark;
            buf.offset = this.offset;
            buf.order = this.order;
            return buf;
        }

        public ShortBuffer AsShortBuffer()
        {
            ShortBuffer buf = new ShortBuffer();
            buf.buffer = new byte[this.buffer.Length];
            this.buffer.CopyTo(buf.buffer, 0);

            buf.c = this.c;
            buf.capacity = this.capacity;
            buf.index = this.index;
            buf.limit = this.limit;
            buf.mark = this.mark;
            buf.offset = this.offset;
            buf.order = this.order;
            return buf;
        }

        public IntBuffer AsIntBuffer()
        {
            IntBuffer buf = new IntBuffer();
            buf.buffer = new byte[this.buffer.Length];
            this.buffer.CopyTo(buf.buffer, 0);

            buf.c = this.c;
            buf.capacity = this.capacity;
            buf.index = this.index;
            buf.limit = this.limit;
            buf.mark = this.mark;
            buf.offset = this.offset;
            buf.order = this.order;
            return buf;
        }
	}
}

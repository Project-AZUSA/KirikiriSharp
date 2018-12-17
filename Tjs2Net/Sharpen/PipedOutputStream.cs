namespace Tjs2.Sharpen
{
    internal class PipedOutputStream : OutputStream
	{
		PipedInputStream ips;

		public PipedOutputStream ()
		{
		}

		public PipedOutputStream (PipedInputStream iss) : this()
		{
			Attach (iss);
		}

        public void Dispose()
        {
            Dispose(true);
        }

		public override void Dispose(bool disposing)
		{
		    if (disposing)
		    {
		        ips.Dispose();
		        base.Dispose(true);
		    }
		}

		internal void Attach (PipedInputStream iss)
		{
			ips = iss;
		}

		public override void Write (int b)
		{
			ips.Write (b);
		}

		public override void Write (byte[] b, int offset, int len)
		{
			ips.Write (b, offset, len);
		}
	}
}

using System.IO;

namespace Tjs2.Sharpen
{
    internal class BufferedInputStream : InputStream
	{
		public BufferedInputStream (InputStream s)
		{
			BaseStream = s.GetWrappedStream ();
			base.Wrapped = new BufferedStream (BaseStream);
		}

		public BufferedInputStream (InputStream s, int bufferSize)
		{
			BaseStream = s.GetWrappedStream ();
			base.Wrapped = new BufferedStream (BaseStream, bufferSize);
		}
	}
}

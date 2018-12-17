using System.IO.Compression;

namespace Tjs2.Sharpen
{
    internal class GZIPOutputStream : OutputStream
	{
		public GZIPOutputStream (OutputStream os)
		{
			Wrapped = new GZipStream (os, CompressionMode.Compress);
		}
	}
}

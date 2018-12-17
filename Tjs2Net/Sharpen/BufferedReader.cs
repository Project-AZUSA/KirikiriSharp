using System.IO;

namespace Tjs2.Sharpen
{
    internal class BufferedReader : StreamReader
	{
		public BufferedReader (InputStreamReader r) : base(r.BaseStream)
		{
		}
	}
}

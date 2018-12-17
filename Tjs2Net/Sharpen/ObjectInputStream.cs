using System;
using System.IO;

namespace Tjs2.Sharpen
{
    internal class ObjectInputStream : InputStream
	{
		private BinaryReader reader;

		public ObjectInputStream (InputStream s)
		{
			this.reader = new BinaryReader (s.GetWrappedStream ());
		}

		public int ReadInt ()
		{
			return this.reader.ReadInt32 ();
		}

		public object ReadObject ()
		{
			throw new NotImplementedException ();
		}
	}
}

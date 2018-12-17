/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class TjsSilentException : Exception
	{
		private const long serialVersionUID = 51839351639123183L;

		public TjsSilentException()
		{
		}

		public TjsSilentException(string msg) : base(msg)
		{
		}
	}
}

/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class VariantException : TjsException
	{
		private const long serialVersionUID = -3605064460238917638L;

		public VariantException()
		{
		}

		public VariantException(string msg) : base(msg)
		{
		}
	}
}

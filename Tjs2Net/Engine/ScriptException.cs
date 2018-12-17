/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class ScriptException : Exception
	{
		private const long serialVersionUID = 61567993834917176L;

		public ScriptException()
		{
		}

		public ScriptException(string msg) : base(msg)
		{
		}
	}
}

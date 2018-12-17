/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class CompileException : TjsScriptError
	{
		/// <summary>TODO 一度 CompileException を呼んでいるところを见直した方が良い</summary>
		private const long serialVersionUID = 560827963479780060L;

		public CompileException(string msg) : base(msg, null, 0)
		{
		}

		public CompileException(string msg, SourceCodeAccessor accessor, int pos) : base(
			msg, accessor, pos)
		{
		}
	}
}

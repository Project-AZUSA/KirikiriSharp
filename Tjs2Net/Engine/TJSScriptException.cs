/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class TjsScriptException : TjsScriptError
	{
		private const long serialVersionUID = 5461681283822047431L;

		private Variant mValue;

		public TjsScriptException(string Msg, ScriptBlock block, int pos, Variant val) : 
			base(Msg, block, pos)
		{
			mValue = val;
		}

		public TjsScriptException(TjsScriptException @ref) : base(@ref)
		{
			mValue = @ref.mValue;
		}

		public virtual Variant GetValue()
		{
			return mValue;
		}
	}
}

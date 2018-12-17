/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class TjsScriptError : TjsException
	{
		private const long serialVersionUID = -1727870605938357683L;

		private const int MAX_TRACE_TEXT_LEN = 1500;

		private SourceCodeAccessor mAccessor;

		private int mPosition;

		private string mTrace;

		public virtual SourceCodeAccessor GetAccessor()
		{
			return mAccessor;
		}

		public virtual int GetPosition()
		{
			return mPosition;
		}

		public virtual int GetSourceLine()
		{
			return mAccessor.SrcPosToLine(mPosition) + 1;
		}

		public string GetBlockName()
		{
			string name = mAccessor.GetName();
			return name != null ? name : string.Empty;
		}

		public string GetTrace()
		{
			return mTrace;
		}

		public virtual bool AddTrace(ScriptBlock block, int srcpos)
		{
			int len = mTrace.Length;
			if (len >= MAX_TRACE_TEXT_LEN)
			{
				return false;
			}
			if (len != 0)
			{
				mTrace += " <-- ";
			}
			mTrace += block.GetLineDescriptionString(srcpos);
			return true;
		}

		public virtual bool AddTrace(string data)
		{
			int len = mTrace.Length;
			if (len >= MAX_TRACE_TEXT_LEN)
			{
				return false;
			}
			if (len != 0)
			{
				mTrace += " <-- ";
			}
			mTrace += data;
			return true;
		}

		public TjsScriptError(string Msg, SourceCodeAccessor accessor, int pos) : base(Msg
			)
		{
			mAccessor = accessor;
			mPosition = pos;
			mTrace = string.Empty;
		}

		public TjsScriptError(TjsScriptError @ref)
		{
			mAccessor = @ref.mAccessor;
			mPosition = @ref.mPosition;
			mTrace = @ref.mTrace;
		}

		public virtual bool AddTrace(InterCodeObject context, int codepos)
		{
			int len = mTrace.Length;
			if (len >= MAX_TRACE_TEXT_LEN)
			{
				return false;
			}
			if (len != 0)
			{
				mTrace += " <-- ";
			}
			mTrace += context.GetPositionDescriptionString(codepos);
			return true;
		}
	}
}

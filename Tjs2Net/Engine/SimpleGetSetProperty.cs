/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class SimpleGetSetProperty : Dispatch
	{
		private Variant mValue;

		public SimpleGetSetProperty(Variant value) : base()
		{
			mValue = value;
		}

		public override int PropGet(int flag, string membername, Variant result, Dispatch2
			 objthis)
		{
			if (membername != null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			if (result != null)
			{
				result.Set(mValue);
			}
			return Error.S_OK;
		}

		public override int PropSet(int flag, string membername, Variant param, Dispatch2
			 objThis)
		{
			if (membername != null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			mValue.Set(param);
			return Error.S_OK;
		}
	}
}

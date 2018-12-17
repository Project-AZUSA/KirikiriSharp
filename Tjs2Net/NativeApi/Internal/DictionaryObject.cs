/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi.Internal
{
	public class DictionaryObject : CustomObject
	{
		private static Variant VoidVal;

		public static void Initialize()
		{
			VoidVal = new Variant();
		}

		public static void FinalizeApplication()
		{
			VoidVal = null;
		}

		public DictionaryObject() : base()
		{
			mCallFinalize = false;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropGet(int flag, string membername, Variant result, Dispatch2
			 objthis)
		{
			int hr = base.PropGet(flag, membername, result, objthis);
			if (hr == Error.E_MEMBERNOTFOUND && (flag & Interface.MEMBERMUSTEXIST) == 0)
			{
				if (result != null)
				{
					result.Clear();
				}
				// returns void
				return Error.S_OK;
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int CreateNew(int flag, string membername, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objthis)
		{
			int hr = base.CreateNew(flag, membername, result, param, objthis);
			if (hr == Error.E_MEMBERNOTFOUND && (flag & Interface.MEMBERMUSTEXIST) == 0)
			{
				return Error.E_INVALIDTYPE;
			}
			// call operation for void
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int Operation(int flag, string membername, Variant result, Variant
			 param, Dispatch2 objthis)
		{
			int hr = base.Operation(flag, membername, result, param, objthis);
			if (hr == Error.E_MEMBERNOTFOUND && (flag & Interface.MEMBERMUSTEXIST) == 0)
			{
				// value not found -> create a value, do the operation once more
				hr = base.PropSet(Interface.MEMBERENSURE, membername, VoidVal, objthis);
				if (hr < 0)
				{
					return hr;
				}
				hr = base.Operation(flag, membername, result, param, objthis);
			}
			return hr;
		}
	}
}

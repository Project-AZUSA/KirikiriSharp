/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi
{
	public abstract class NativeClassMethod : Dispatch
	{
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOf(int flag, string membername, string classname, Dispatch2
			 objthis)
		{
			if (membername == null)
			{
				if ("Function".Equals(classname))
				{
					return Error.S_TRUE;
				}
			}
			return base.IsInstanceOf(flag, membername, classname, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int FuncCall(int flag, string membername, Variant result, Variant
			[] param, Dispatch2 objthis)
		{
			if (membername != null)
			{
				return base.FuncCall(flag, membername, result, param, objthis);
			}
			if (objthis == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			if (result != null)
			{
				result.Clear();
			}
			return Process(result, param, objthis);
		}

		// override this instead of FuncCall
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal abstract int Process(Variant result, Variant[] param, Dispatch2
			 objthis);
	}
}

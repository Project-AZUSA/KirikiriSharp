/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine.Translate
{
	public abstract class NativeConvertedFunction : NativeConvertedClassBase
	{
		public NativeConvertedFunction(Tjs owner) : base(owner)
		{
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
			return Process(result, param, objthis);
		}

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

		// override this instead of FuncCall
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal abstract int Process(Variant result, Variant[] param, Dispatch2
			 objthis);
	}
}

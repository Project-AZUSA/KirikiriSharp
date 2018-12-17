/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine.Translate
{
	public abstract class NativeConvertedClassConstructor : NativeConvertedClassMethod
	{
		public NativeConvertedClassConstructor(Tjs owner) : base(owner)
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
			if (result != null)
			{
				result.Clear();
			}
			return Process(result, param, objthis);
		}
	}
}

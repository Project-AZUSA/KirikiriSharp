/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi
{
	public class NativeInstanceObject : NativeInstance
	{
		// TJS constructor
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int Construct(Variant[] param, Dispatch2 tjsObj)
		{
			return Error.S_OK;
		}

		// called before destruction
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Invalidate()
		{
		}

		// must destruct itself
		public virtual void Destruct()
		{
		}
	}
}

/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi
{
	public interface NativeInstance
	{
		// TJS constructor
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int Construct(Variant[] param, Dispatch2 tjsObj);

		// called before destruction
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		void Invalidate();

		// must destruct itself
		void Destruct();
	}
}

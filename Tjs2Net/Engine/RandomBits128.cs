/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public interface RandomBits128
	{
		// retrives 128-bits (16bytes) random bits for random seed.
		// this can be override application-specified routine, otherwise
		// TJS2 uses current time as a random seed.
		void GetRandomBits128(ByteBuffer buf, int offset);
	}
}

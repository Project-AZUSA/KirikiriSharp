/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public interface TextWriteStreamInterface
	{
		/// <exception cref="TjsException"></exception>
		void Write(string val);

		/// <exception cref="TjsException"></exception>
		void Destruct();
	}
}

/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public interface StorageInterface
	{
		/// <exception cref="TjsException"></exception>
		string ReadText(string name, string modestr);

		/// <exception cref="TjsException"></exception>
		TextWriteStreamInterface CreateTextWriteStream(string name, string modestr);

		/// <exception cref="TjsException"></exception>
		BinaryStream CreateBinaryWriteStream(string name);
	}
}

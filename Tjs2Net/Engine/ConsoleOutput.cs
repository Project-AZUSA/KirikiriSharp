/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public interface IConsoleOutput
	{
		void ExceptionPrint(string msg);

		void Print(string msg);
	}
}

/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class Logger
	{
		public static void Log(string tag, string message)
		{
			System.Console.Out.WriteLine(tag + " : " + message);
		}

		// Log.v( tag, message );	// for android
		public static void Log(string message)
		{
			System.Console.Out.WriteLine(message);
		}
		// Log.v( "Logger", message );	// for android
	}
}

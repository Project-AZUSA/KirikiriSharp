using System;
using Tjs2.Engine;

namespace Tjs2.Sharper
{
    public class DebugConsoleOutput : IConsoleOutput
    {
        public void ExceptionPrint(string msg)
        {
            Console.Write("Error:");
            Console.WriteLine(msg);
        }

        public void Print(string msg)
        {
            Console.Write("OUT:");
            Console.WriteLine(msg);
        }
    }
}

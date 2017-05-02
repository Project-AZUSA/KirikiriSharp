using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tjs2;
using Tjs2.Engine;
using Tjs2.Sharper;

namespace Tjs2Executor
{
    class Program
    {
        private static int _count = 0;
        private static readonly DebugConsoleOutput ConsoleOutput = new DebugConsoleOutput();
        static void Main(string[] args)
        {
            Console.WriteLine("Krkr# Tjs2 Disassembler by Ulysses");
            if (args.Length <= 0)
            {
                args = new[] { Directory.GetCurrentDirectory() };
            }

            Tjs.mStorage = null;
            Tjs.Initialize();
            Tjs scriptEngine = new Tjs();
            Tjs.SetConsoleOutput(ConsoleOutput);

            Dispatch2 dsp = scriptEngine.GetGlobal();
            TjsByteCodeLoader loader = new TjsByteCodeLoader();

            foreach (string s in args)
            {
                if (Directory.Exists(s)) //disasm dir
                {
                    var list = Directory.EnumerateFiles(s, "*.tjs").Where(ss => ss.ToLowerInvariant().EndsWith(".tjs")).ToList();
                    var initScript = list.FirstOrDefault(n => n.Contains("startup.tjs"));
                    if (!string.IsNullOrWhiteSpace(initScript))
                    {
                        Dump(loader, scriptEngine, initScript);
                        list.Remove(initScript);
                    }

                    initScript = list.FirstOrDefault(n => n.Contains("initialize.tjs"));
                    if (!string.IsNullOrWhiteSpace(initScript))
                    {
                        Dump(loader, scriptEngine, initScript);
                        list.Remove(initScript);
                    }

                    foreach (var scripts in list)
                    {
                        Dump(loader, scriptEngine, scripts);
                    }
                }
                else if (File.Exists(s) && s.ToLowerInvariant().EndsWith(".tjs"))
                {
                    Dump(loader, scriptEngine, s);
                }
            }

            Console.WriteLine($"All Done! {_count} files processed.");
            Console.ReadLine();
            scriptEngine.Shutdown();
            Tjs.FinalizeApplication();
        }

    }
}

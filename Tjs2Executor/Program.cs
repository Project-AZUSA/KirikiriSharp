using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tjs2;
using Tjs2.Engine;
using Tjs2.Sharper;

// Still In Dev
namespace Tjs2Executor
{
    class Program
    {
        private static int _count = 0;
        private static readonly DebugConsoleOutput ConsoleOutput = new DebugConsoleOutput();
        static void Main(string[] args)
        {
            Console.WriteLine("Krkr# Tjs2 Executor by Ulysses");
            if (args.Length <= 0)
            {
                //args = new[] {"frameview.tjs"};
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
                        Execute(loader, scriptEngine, initScript);
                        list.Remove(initScript);
                    }

                    initScript = list.FirstOrDefault(n => n.Contains("initialize.tjs"));
                    if (!string.IsNullOrWhiteSpace(initScript))
                    {
                        Execute(loader, scriptEngine, initScript);
                        list.Remove(initScript);
                    }

                    foreach (var scripts in list)
                    {
                        var ret = Execute(loader, scriptEngine, scripts);
                        Console.WriteLine($"ret: {ret}");
                    }
                }
                else if (File.Exists(s) && s.ToLowerInvariant().EndsWith(".tjs"))
                {
                    var ret = Execute(loader, scriptEngine, s);
                    Console.WriteLine($"ret: {ret}");
                }
            }

            Console.WriteLine($"All Done! {_count} files processed.");
            Console.ReadLine();
            scriptEngine.Shutdown();
            Tjs.FinalizeApplication();
        }

        static Variant Execute(TjsByteCodeLoader loader, Tjs engine, string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                TjsBinaryStream stream = new TjsBinaryStream(fs);
                try
                {
                    var ret = new Variant();
                    Dispatch2 dsp = engine.GetGlobal();
                    engine.LoadByteCode(ret, dsp, Path.GetFileNameWithoutExtension(path), stream);
                    //dsp.PropGet(0, "FrameView", ret, dsp);
                    //var obj = ret.AsObject();
                    //var r = obj.FuncCall(0, null, ret, null, obj);
                    //r = obj.FuncCall(0, "FrameView", ret, null, obj);
                    return ret;
                }
                catch (TjsScriptError ex)
                {
                    Console.WriteLine(ex);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Execute {path} failed.");
                    _count--;
                }
            }
            return null;
        }
    }
}

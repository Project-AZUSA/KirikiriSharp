using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tjs2.Engine;
using Tjs2.Sharpen;

namespace Tjs2.Sharper
{
    public class FileLogOutput : IConsoleOutput, IDisposable
    {
        private readonly FileStream _fs;
        private readonly TextWriter _tw;
        public FileLogOutput(string path)
        {
            _fs = new FileStream(path, FileMode.OpenOrCreate);
            _tw = new OutputStreamWriter(_fs, Encoding.UTF8);
        }

        public void ExceptionPrint(string msg)
        {
            _tw.WriteLine($"Error: {msg}");
        }

        public void Print(string msg)
        {
            _tw.WriteLine(msg);
        }

        public void Dispose()
        {
            _tw.Close();
            _fs.Close();
        }
    }
}

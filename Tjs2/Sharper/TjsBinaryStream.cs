using System;
using System.IO;
using Tjs2.Engine;
using Tjs2.Sharpen;

namespace Tjs2.Sharper
{
    public class TjsBinaryStream : BinaryStream, IDisposable
    {
        public string Path { get; set; }
        public Stream BaseStream { get; }
        public TjsBinaryStream(Stream stream)
        {
            BaseStream = stream;
        }

        public override long Seek(long offset, int whence)
        {
            SeekOrigin origin;
            switch (whence)
            {
                case BinaryStream.SEEK_CUR:
                    origin = SeekOrigin.Current;
                    break;
                case SEEK_END:
                    origin = SeekOrigin.End;
                    break;
                default:
                    origin = SeekOrigin.Begin;
                    break;
            }
            return BaseStream.Seek(offset, origin);
        }

        public override int Read(ByteBuffer buffer)
        {
            return BaseStream.Read(buffer.Array(), 0, buffer.Limit());
        }

        public override int Read(byte[] buffer)
        {
            return BaseStream.Read(buffer, 0, buffer.Length);
        }

        public override int Read(byte[] b, int off, int len)
        {
            return BaseStream.Read(b, off, len);
        }

        public override int Write(ByteBuffer buffer)
        {
            BaseStream.Write(buffer.Array(), 0, buffer.Limit());
            return buffer.Limit();
        }

        public override int Write(byte[] buffer)
        {
            BaseStream.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        public override void Write(byte[] b, int off, int len)
        {
            BaseStream.Write(b, off, len);
        }

        /// <summary>
        /// Write a byte, ingore first 24bit
        /// </summary>
        /// <param name="b"></param>
        public override void Write(int b)
        {
            BaseStream.WriteByte((byte)b);
        }

        public override void Close()
        {
            BaseStream.Close();
        }

        public override InputStream GetInputStream()
        {
            return BaseStream;
        }

        public override OutputStream GetOutputStream()
        {
            return BaseStream;
        }

        public override string GetFilePath()
        {
            if (!string.IsNullOrEmpty(Path))
            {
                return Path;
            }
            if (BaseStream is FileStream fs)
            {
                return fs.Name;
            }
            return "";
        }

        public void Dispose()
        {
            BaseStream.Dispose();
        }

        public override long GetSize()
        {
            return BaseStream.Length;
        }
    }
}

using System;
using System.IO;

namespace Tjs2.Sharpen
{
    internal class WrappedSystemStream : Stream
	{
		
		public InputStream InputStream { get; }
        public OutputStream OutputStream { get; }

        private int _position;
        private int _markedPosition;

		public WrappedSystemStream (InputStream ist)
		{
			InputStream = ist;
		}

		public WrappedSystemStream (OutputStream ost)
		{
			OutputStream = ost;
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InputStream?.Dispose();
                OutputStream?.Dispose();
            }
        }

		public override void Flush()
		{
			OutputStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int res = InputStream.Read(buffer, offset, count);
		    if (res == -1) return 0;
		    _position += res;
		    return res;
		}

		public override int ReadByte ()
		{
			int res = InputStream.Read ();
			if (res != -1)
				_position++;
			return res;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
				return Position = offset;
            throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			OutputStream.Write (buffer, offset, count);
			_position += count;
		}

		public override void WriteByte (byte value)
		{
			OutputStream.Write (value);
			_position++;
		}

		public override bool CanRead => InputStream != null;

        public override bool CanSeek => true;

        public override bool CanWrite => OutputStream != null;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        internal void OnMark (int nb)
		{
			_markedPosition = _position;
			InputStream.Mark (nb);
		}
		
		public override long Position {
			get {
				if (InputStream != null && InputStream.CanSeek ())
					return InputStream.Position;
                return _position;
			}
			set {
				if (value == _position)
					return;
				if (value == _markedPosition)
					InputStream.Reset();
				else if (InputStream != null && InputStream.CanSeek()) {
					InputStream.Position = value;
				}
				else
					throw new NotSupportedException();
			}
		}
	}
}

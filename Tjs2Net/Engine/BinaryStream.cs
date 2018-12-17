/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	/// <summary>バイナリストリーム读み书きクラス</summary>
	public abstract class BinaryStream
	{
		/// <summary>读み迂みモード</summary>
		public const int READ = 0;

		/// <summary>书き迂みモード</summary>
		public const int WRITE = 1;

		/// <summary>追记モード</summary>
		public const int APPEND = 2;

		/// <summary>更新モード</summary>
		public const int UPDATE = 3;

		/// <summary>アクセスモードマスク</summary>
		public const int ACCESS_MASK = unchecked((int)(0x0f));

		/// <summary>先头からのシーク</summary>
		public const int SEEK_SET = 0;

		/// <summary>现在位置からのシーク</summary>
		public const int SEEK_CUR = 1;

		/// <summary>终端位置からのシーク</summary>
		public const int SEEK_END = 2;

		/// <summary>
		/// シークする
		/// エラー时、位置は变更されない
		/// </summary>
		/// <param name="offset">基准位置からのオフセット</param>
		/// <param name="whence">基准位置、SEEK_SET, SEEK_CUR, SEEK_END のいずれかを指定</param>
		/// <returns>シーク后の现在位置</returns>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		public abstract long Seek(long offset, int whence);

		/// <summary>returns actually read size</summary>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		public abstract int Read(ByteBuffer buffer);

		/// <summary>returns actually read size</summary>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		public abstract int Read(byte[] buffer);

		/// <summary>ストリームからの读み迂み</summary>
		/// <param name="b">读み迂み先byte配列</param>
		/// <param name="off">配列オフセット</param>
		/// <param name="len">读み迂みサイズ</param>
		/// <returns>实际に读み迂んだ长さ。-1 の时ファイル终端</returns>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		public abstract int Read(byte[] b, int off, int len);

		/// <summary>returns actually written size</summary>
		public abstract int Write(ByteBuffer buffer);

		/// <summary>returns actually written size</summary>
		public abstract int Write(byte[] buffer);

		public abstract void Write(byte[] b, int off, int len);

		/// <summary>
		/// 1 バイトが出力ストリームに书き迂まれます。
		/// 书き迂まれるバイトは、引数 b の下位 8 ビットです。
		/// b の上位 24 ビットは无视されます。
		/// </summary>
		/// <param name="b"></param>
		public abstract void Write(int b);

		/// <summary>close stream</summary>
		public abstract void Close();

		/// <summary>
		/// the default behavior is raising a exception
		/// if error, raises exception
		/// </summary>
		/// <exception cref="TjsException"></exception>
		public virtual void SetEndOfStorage()
		{
			throw new TjsException(Error.WriteError);
		}

		/// <summary>should re-implement for higher performance</summary>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		public virtual long GetSize()
		{
			long orgpos = GetPosition();
			long size = Seek(0, SEEK_END);
			Seek(orgpos, SEEK_SET);
			return size;
		}

		/// <exception cref="TjsException"></exception>
		public virtual long GetPosition()
		{
			return Seek(0, SEEK_CUR);
		}

		/// <exception cref="TjsException"></exception>
		public virtual void SetPosition(long pos)
		{
			if (pos != Seek(pos, SEEK_SET))
			{
				throw new TjsException(Error.SeekError);
			}
		}

		/// <exception cref="TjsException"></exception>
		public virtual void ReadBuffer(ByteBuffer buffer)
		{
			if (Read(buffer) != -1)
			{
				throw new TjsException(Error.ReadError);
			}
			buffer.Flip();
		}

		/// <exception cref="TjsException"></exception>
		public virtual void WriteBuffer(ByteBuffer buffer)
		{
			if (Write(buffer) != -1)
			{
				throw new TjsException(Error.WriteError);
			}
		}

		public abstract InputStream GetInputStream();

		public abstract OutputStream GetOutputStream();

		/// <summary>アーカイブ内のファイルかどうか判定する</summary>
		/// <returns></returns>
		public virtual bool IsArchive()
		{
			return false;
		}

		public abstract string GetFilePath();

        //public virtual FileDescriptor GetFileDescriptor()
        //{
        //    return null;
        //}

		public virtual long GetFileOffset()
		{
			return 0;
		}
	}
}

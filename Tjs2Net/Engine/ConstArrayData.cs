/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	/// <summary>
	/// TJS2 バイトコード书き出し、读み迂みで Variant 型を分离し、固有型で保持するためのクラス
	/// 读み迂みはより效率的に处理できるように别クラスにした方がいいか
	/// 读み迂んだ ByteBuffer を直接处理するような
	/// </summary>
	public class ConstArrayData
	{
		private AList<byte> mByte;

		private AList<short> mShort;

		private AList<int> mInteger;

		private AList<long> mLong;

		private AList<double> mDouble;

		private AList<string> mString;

		private AList<ByteBuffer> mByteBuffer;

		private Dictionary<byte, int> mByteHash;

		private Dictionary<short, int> mShortHash;

		private Dictionary<int, int> mIntegerHash;

		private Dictionary<long, int> mLongHash;

		private Dictionary<double, int> mDoubleHash;

		private Dictionary<string, int> mStringHash;

		private Dictionary<ByteBuffer, int> mByteBufferHash;

		private const byte TYPE_VOID = 0;

		private const byte TYPE_OBJECT = 1;

		private const byte TYPE_INTER_OBJECT = 2;

		private const byte TYPE_STRING = 3;

		private const byte TYPE_OCTET = 4;

		private const byte TYPE_REAL = 5;

		private const byte TYPE_BYTE = 6;

		private const byte TYPE_SHORT = 7;

		private const byte TYPE_INTEGER = 8;

		private const byte TYPE_LONG = 9;

		private const byte TYPE_INTER_GENERATOR = 10;

		private const byte TYPE_UNKNOWN = unchecked((byte)(-1));

		public ConstArrayData()
		{
			// 保持したかどうか判定するためのハッシュ
			// temporary
			mByte = new AList<byte>();
			mShort = new AList<short>();
			mInteger = new AList<int>();
			mLong = new AList<long>();
			mDouble = new AList<double>();
			mString = new AList<string>();
			mByteBuffer = new AList<ByteBuffer>();
			mByteHash = new Dictionary<byte, int>();
			mShortHash = new Dictionary<short, int>();
			mIntegerHash = new Dictionary<int, int>();
			mLongHash = new Dictionary<long, int>();
			mDoubleHash = new Dictionary<double, int>();
			mStringHash = new Dictionary<string, int>();
			mByteBufferHash = new Dictionary<ByteBuffer, int>();
		}

		private int PutByteBuffer(ByteBuffer val)
		{
			int index = mByteBufferHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mByteBuffer.Count);
				mByteBuffer.AddItem(val);
				mByteBufferHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		public int PutString(string val)
		{
			int index = mStringHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mString.Count);
				mString.AddItem(val);
				mStringHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		private int PutByte(byte b)
		{
			byte val = (b);
			int index = mByteHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mByte.Count);
				mByte.AddItem(val);
				mByteHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		private int PutShort(short b)
		{
			short val = (b);
			int index = mShortHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mShort.Count);
				mShort.AddItem(val);
				mShortHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		private int PutInteger(int b)
		{
			int val = Sharpen.Extensions.ValueOf(b);
			int index = mIntegerHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mInteger.Count);
				mInteger.AddItem(val);
				mIntegerHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		private int PutLong(long b)
		{
			long val = Sharpen.Extensions.ValueOf(b);
			int index = mLongHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mLong.Count);
				mLong.AddItem(val);
				mLongHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		private int PutDouble(double b)
		{
			double val = (b);
			int index = mDoubleHash.Get(val);
			if (index == null)
			{
				index = Sharpen.Extensions.ValueOf(mDouble.Count);
				mDouble.AddItem(val);
				mDoubleHash.Put(val, index);
				return index;
			}
			else
			{
				return index;
			}
		}

		public byte GetType(Variant v)
		{
			object o = v.ToJavaObject();
			if (o == null)
			{
				return TYPE_VOID;
			}
			else
			{
				if (o is string)
				{
					return TYPE_STRING;
				}
				else
				{
					if (o is int)
					{
						int val = ((int)o);
						if (val >= byte.MinValue && val <= byte.MaxValue)
						{
							return TYPE_BYTE;
						}
						else
						{
							if (val >= short.MinValue && val <= short.MaxValue)
							{
								return TYPE_SHORT;
							}
							else
							{
								return TYPE_INTEGER;
							}
						}
					}
					else
					{
						if (o is double)
						{
							return TYPE_REAL;
						}
						else
						{
							if (o is VariantClosure)
							{
								VariantClosure clo = (VariantClosure)o;
								Dispatch2 dsp = clo.mObject;
								if (dsp is InterCodeObject)
								{
									return TYPE_INTER_OBJECT;
								}
								else
								{
									return TYPE_OBJECT;
								}
							}
							else
							{
								if (o is InterCodeGenerator)
								{
									return TYPE_INTER_GENERATOR;
								}
								else
								{
									if (o is ByteBuffer)
									{
										return TYPE_OCTET;
									}
									else
									{
										if (o is long)
										{
											long val = ((long)o);
											if (val >= byte.MinValue && val <= byte.MaxValue)
											{
												return TYPE_BYTE;
											}
											else
											{
												if (val >= short.MinValue && val <= short.MaxValue)
												{
													return TYPE_SHORT;
												}
												else
												{
													if (val >= int.MinValue && val <= int.MaxValue)
													{
														return TYPE_INTEGER;
													}
													else
													{
														return TYPE_LONG;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return TYPE_UNKNOWN;
		}

		public int PutVariant(Variant v, ScriptBlock block)
		{
			object o = v.ToJavaObject();
			int type = GetType(v);
			switch (type)
			{
				case TYPE_VOID:
				{
					return 0;
				}

				case TYPE_OBJECT:
				{
					// 常に0
					VariantClosure clo = (VariantClosure)o;
					if (clo.mObject == null && clo.mObjThis == null)
					{
						return 0;
					}
					else
					{
						// null の VariantClosure は受け入れる
						return -1;
					}
				}

				case TYPE_INTER_OBJECT:
				{
					// その他は入れない。Dictionary と Array は保存できるようにした方がいいが……
					VariantClosure clo = (VariantClosure)o;
					Dispatch2 dsp = clo.mObject;
					return block.GetObjectIndex((InterCodeObject)dsp);
				}

				case TYPE_STRING:
				{
					return PutString(((string)o));
				}

				case TYPE_OCTET:
				{
					return PutByteBuffer((ByteBuffer)o);
				}

				case TYPE_REAL:
				{
					return PutDouble(((Number)o));
				}

				case TYPE_BYTE:
				{
					return PutByte(((Number)o));
				}

				case TYPE_SHORT:
				{
					return PutShort(((Number)o));
				}

				case TYPE_INTEGER:
				{
					return PutInteger(((Number)o));
				}

				case TYPE_LONG:
				{
					return PutLong(((Number)o));
				}

				case TYPE_UNKNOWN:
				{
					return -1;
				}
			}
			return -1;
		}

		public int PutVariant(Variant v, Compiler block)
		{
			object o = v.ToJavaObject();
			int type = GetType(v);
			switch (type)
			{
				case TYPE_VOID:
				{
					return 0;
				}

				case TYPE_OBJECT:
				{
					// 常に0
					VariantClosure clo = (VariantClosure)o;
					if (clo.mObject == null && clo.mObjThis == null)
					{
						return 0;
					}
					else
					{
						// null の VariantClosure は受け入れる
						return -1;
					}
				}

				case TYPE_INTER_OBJECT:
				{
					// その他は入れない。Dictionary と Array は保存できるようにした方がいいが……
					VariantClosure clo = (VariantClosure)o;
					Dispatch2 dsp = clo.mObject;
					return block.GetObjectIndex((InterCodeObject)dsp);
				}

				case TYPE_STRING:
				{
					return PutString(((string)o));
				}

				case TYPE_OCTET:
				{
					return PutByteBuffer((ByteBuffer)o);
				}

				case TYPE_REAL:
				{
					return PutDouble(((Number)o));
				}

				case TYPE_BYTE:
				{
					return PutByte(((Number)o));
				}

				case TYPE_SHORT:
				{
					return PutShort(((Number)o));
				}

				case TYPE_INTEGER:
				{
					return PutInteger(((Number)o));
				}

				case TYPE_LONG:
				{
					return PutLong(((Number)o));
				}

				case TYPE_INTER_GENERATOR:
				{
					return block.GetCodeIndex((InterCodeGenerator)o);
				}

				case TYPE_UNKNOWN:
				{
					return -1;
				}
			}
			return -1;
		}

		public ByteBuffer ExportBuffer()
		{
			int size = 0;
			// string
			int stralllen = 0;
			int count = mString.Count;
			for (int i = 0; i < count; i++)
			{
				int len = mString[i].Length;
				len = ((len + 1) / 2) * 2;
				stralllen += len * 2;
			}
			stralllen = ((stralllen + 3) / 4) * 4;
			// アライメント
			size += stralllen + count * 4 + 4;
			// byte buffer
			int bytealllen = 0;
			count = mByteBuffer.Count;
			for (int i_1 = 0; i_1 < count; i_1++)
			{
				int len = mByteBuffer[i_1].Capacity();
				len = ((len + 3) / 4) * 4;
				bytealllen += len;
			}
			bytealllen = ((bytealllen + 3) / 4) * 4;
			// アライメント
			size += bytealllen + count * 4 + 4;
			// byte
			count = mByte.Count;
			count = ((count + 3) / 4) * 4;
			// アライメント
			size += count + 4;
			// short
			count = mShort.Count * 2;
			count = ((count + 3) / 4) * 4;
			// アライメント
			size += count + 4;
			// int
			size += mInteger.Count * 4 + 4;
			// long
			size += mLong.Count * 8 + 4;
			// double
			size += mDouble.Count * 8 + 4;
			ByteBuffer buf = ByteBuffer.Allocate(size);
			buf.Order(ByteOrder.LITTLE_ENDIAN);
			buf.Clear();
			// byte write
			count = mByte.Count;
			buf.PutInt(count);
			for (int i_2 = 0; i_2 < count; i_2++)
			{
				buf.Put(mByte[i_2]);
			}
			count = (((count + 3) / 4) * 4) - count;
			// アライメント差分
			for (int i_3 = 0; i_3 < count; i_3++)
			{
				buf.Put(unchecked((byte)0));
			}
			// short write
			count = mShort.Count;
			buf.PutInt(count);
			for (int i_4 = 0; i_4 < count; i_4++)
			{
				buf.PutShort(mShort[i_4]);
			}
			count *= 2;
			count = (((count + 3) / 4) * 4) - count;
			// アライメント差分
			for (int i_5 = 0; i_5 < count; i_5++)
			{
				buf.Put(unchecked((byte)0));
			}
			// int write
			count = mInteger.Count;
			buf.PutInt(count);
			for (int i_6 = 0; i_6 < count; i_6++)
			{
				buf.PutInt(mInteger[i_6]);
			}
			// long write
			count = mLong.Count;
			buf.PutInt(count);
			for (int i_7 = 0; i_7 < count; i_7++)
			{
				buf.PutLong(mLong[i_7]);
			}
			// double write
			count = mDouble.Count;
			buf.PutInt(count);
			for (int i_8 = 0; i_8 < count; i_8++)
			{
				buf.PutLong(Double.DoubleToRawLongBits(mDouble[i_8]));
			}
			// string write
			count = mString.Count;
			buf.PutInt(count);
			for (int i_9 = 0; i_9 < count; i_9++)
			{
				string str = mString[i_9];
				int len = str.Length;
				buf.PutInt(len);
				int s = 0;
				for (; s < len; s++)
				{
					buf.PutChar(str[s]);
				}
				if ((len % 2) == 1)
				{
					// アライメント差分
					buf.PutChar((char)0);
				}
			}
			// byte buffer write
			count = mByteBuffer.Count;
			buf.PutInt(count);
			for (int i_10 = 0; i_10 < count; i_10++)
			{
				ByteBuffer by = mByteBuffer[i_10];
				int cap = by.Capacity();
				buf.PutInt(cap);
				for (int b = 0; b < cap; b++)
				{
					buf.Put(by.Get(b));
				}
				cap = ((cap + 3) / 4) * 4 - cap;
				// アライメント差分
				for (int b_1 = 0; b_1 < cap; b_1++)
				{
					buf.Put(unchecked((byte)0));
				}
			}
			buf.Flip();
			return buf;
		}
	}
}

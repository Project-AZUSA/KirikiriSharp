/*
 * TJS2 CSharp
 */

using System;
using System.Text;

namespace Tjs2.Engine
{
	public class StringStream
	{
		private const int BUFFER_CAPACITY = 1024;

		private readonly string mString;

		private readonly char[] mText;

		private int mOffset;

		private bool mEOF;

		private int mStringStatus;

		private IntVector mLineVector;

		private IntVector mLineLengthVector;

		private int mLineOffset;

		private const int CARRIAGE_RETURN = 13;

		private const int LINE_FEED = 10;

		private const int TAB = unchecked((int)(0x09));

		private const int SPACE = unchecked((int)(0x20));

		public const int NOT_COMMENT = 0;

		public const int UNCLOSED_COMMENT = -1;

		public const int CONTINUE = 1;

		public const int ENDED = 2;

		public const int NONE = 0;

		public const int DELIMITER = 1;

		public const int AMPERSAND = 2;

		public const int DOLLAR = 3;

		public StringStream(string str)
		{
			//private static final String TAG = "StringStream";
			//private static final boolean LOGD = false;
			mString = str;
			mText = new char[mString.Length];
			Sharpen.Runtime.GetCharsForString(str, 0, mString.Length, mText, 0);
		}

		//mOffset = 0;
		//mEOF = false;
		public void UngetC()
		{
			if (mOffset > 0)
			{
				mOffset--;
			}
		}

		public int GetC()
		{
			int retval = -1;
			if (mOffset < mText.Length)
			{
				retval = mText[mOffset];
				mOffset++;
			}
			else
			{
				mEOF = true;
			}
			return retval;
		}

		public void IncOffset()
		{
			if (mOffset < mText.Length)
			{
				mOffset++;
			}
			else
			{
				mEOF = true;
			}
		}

		public int PeekC()
		{
			int retval = -1;
			if (mOffset < mText.Length)
			{
				retval = mText[mOffset];
			}
			return retval;
		}

		public int PeekC(int offset)
		{
			int retval = -1;
			if ((mOffset + offset) < mText.Length)
			{
				retval = mText[mOffset + offset];
			}
			return retval;
		}

		// 改行コードを无视して取得する
		public int Next()
		{
			int retval = -1;
			if (mOffset < mText.Length)
			{
				retval = mText[mOffset];
				mOffset++;
				while (retval == CARRIAGE_RETURN || retval == LINE_FEED)
				{
					if (mOffset < mText.Length)
					{
						retval = mText[mOffset];
						mOffset++;
					}
					else
					{
						retval = -1;
						mEOF = true;
						break;
					}
				}
				return retval;
			}
			else
			{
				mEOF = true;
			}
			return retval;
		}

		public bool IsEOF()
		{
			return mEOF;
		}

		public void SkipSpace()
		{
			if (mOffset < mText.Length)
			{
				int c = mText[mOffset];
				mOffset++;
				bool skipToLast = false;
				while (c == CARRIAGE_RETURN || c == LINE_FEED || c == TAB || c == SPACE)
				{
					if (mOffset < mText.Length)
					{
						c = mText[mOffset];
						mOffset++;
					}
					else
					{
						skipToLast = true;
						break;
					}
				}
				if (mOffset > 0 && skipToLast == false)
				{
					mOffset--;
				}
			}
			if (mOffset >= mText.Length)
			{
				mEOF = true;
			}
		}

		public void SkipReturn()
		{
			if (mOffset < mText.Length)
			{
				int c = mText[mOffset];
				mOffset++;
				bool skipToLast = false;
				while (c == CARRIAGE_RETURN || c == LINE_FEED)
				{
					if (mOffset < mText.Length)
					{
						c = mText[mOffset];
						mOffset++;
					}
					else
					{
						skipToLast = true;
						break;
					}
				}
				if (mOffset > 0 && skipToLast == false)
				{
					mOffset--;
				}
			}
			if (mOffset >= mText.Length)
			{
				mEOF = true;
			}
		}

		/// <exception cref="CompileException"></exception>
		public int SkipComment()
		{
			int offset = mOffset;
			if ((offset + 1) < mText.Length)
			{
				if (mText[offset] != '/')
				{
					return NOT_COMMENT;
				}
				if (mText[offset + 1] == '/')
				{
					// ラインコメント
					mOffset += 2;
					int c = mText[mOffset];
					mOffset++;
					while (c != CARRIAGE_RETURN && c != LINE_FEED)
					{
						if (mOffset < mText.Length)
						{
							c = mText[mOffset];
							mOffset++;
						}
						else
						{
							break;
						}
					}
					if (mOffset < mText.Length)
					{
						if (c == CARRIAGE_RETURN)
						{
							if (mText[mOffset] == LINE_FEED)
							{
								mOffset++;
							}
						}
					}
					else
					{
						mEOF = true;
						return ENDED;
					}
					SkipSpace();
					if (mOffset >= mText.Length)
					{
						mEOF = true;
						return ENDED;
					}
					return CONTINUE;
				}
				else
				{
					if (mText[offset + 1] == '*')
					{
						// ブロックコメント
						mOffset += 2;
						int level = 0;
						while (true)
						{
							if ((mOffset + 1) < mText.Length)
							{
								if (mText[mOffset] == '/' && mText[mOffset + 1] == '*')
								{
									// コメントのネスト
									level++;
								}
								else
								{
									if (mText[mOffset] == '*' && mText[mOffset + 1] == '/')
									{
										if (level == 0)
										{
											mOffset += 2;
											break;
										}
										level--;
									}
								}
								mOffset++;
							}
							else
							{
								throw new CompileException(Error.UnclosedComment);
							}
						}
						if (mOffset >= mText.Length)
						{
							mEOF = true;
							return ENDED;
						}
						SkipSpace();
						if (mOffset >= mText.Length)
						{
							mEOF = true;
							return ENDED;
						}
						return CONTINUE;
					}
				}
			}
			return NOT_COMMENT;
		}

		public int GetOffset()
		{
			return mOffset;
		}

		public void SetOffset(int offset)
		{
			if (offset < mText.Length)
			{
				mOffset = offset;
			}
			else
			{
				mOffset = mText.Length;
				mEOF = true;
			}
		}

		public bool EqualString(string value)
		{
			int count = value.Length;
			if ((mText.Length - mOffset) >= count)
			{
				int offset = mOffset;
				for (int i = 0; i < count; i++)
				{
					if (mText[offset + i] != value[i])
					{
						return false;
					}
				}
				mOffset += count;
				return true;
			}
			else
			{
				return false;
			}
		}

		public static int UnescapeBackSlash(int ch)
		{
			switch (ch)
			{
				case 'a':
				{
					return unchecked((int)(0x07));
				}

				case 'b':
				{
					return unchecked((int)(0x08));
				}

				case 'f':
				{
					return unchecked((int)(0x0c));
				}

				case 'n':
				{
					return unchecked((int)(0x0a));
				}

				case 'r':
				{
					return unchecked((int)(0x0d));
				}

				case 't':
				{
					return unchecked((int)(0x09));
				}

				case 'v':
				{
					return unchecked((int)(0x0b));
				}

				default:
				{
					return ch;
				}
			}
		}

		public int CountOctetTail()
		{
			if (mOffset < mText.Length)
			{
				int offset = mOffset;
				while ((offset + 1) < mText.Length)
				{
					if (mText[offset] == '%' && mText[offset + 1] == '>')
					{
						break;
					}
					offset++;
				}
				return offset - mOffset;
			}
			else
			{
				return 0;
			}
		}

		/// <exception cref="CompileException"></exception>
		public string ReadString(int delimiter, bool embexpmode)
		{
			if (mOffset < mText.Length)
			{
				StringBuilder str = new StringBuilder(BUFFER_CAPACITY);
				mStringStatus = NONE;
				try
				{
					while (mOffset < mText.Length)
					{
						int c = mText[mOffset];
						mOffset++;
						while (c == CARRIAGE_RETURN || c == LINE_FEED)
						{
							c = mText[mOffset];
							mOffset++;
						}
						if (c == '\\')
						{
							// escape
							c = mText[mOffset];
							mOffset++;
							while (c == CARRIAGE_RETURN || c == LINE_FEED)
							{
								c = mText[mOffset];
								mOffset++;
							}
							if (c == 'x' || c == 'X')
							{
								// hex
								//							int num = 0;
								int code = 0;
								int count = 0;
								while (count < 4)
								{
									c = mText[mOffset];
									mOffset++;
									while (c == CARRIAGE_RETURN || c == LINE_FEED)
									{
										c = mText[mOffset];
										mOffset++;
									}
									int n = -1;
									if (c >= '0' && c <= '9')
									{
										n = c - '0';
									}
									else
									{
										if (c >= 'a' && c <= 'f')
										{
											n = c - 'a' + 10;
										}
										else
										{
											if (c >= 'A' && c <= 'F')
											{
												n = c - 'A' + 10;
											}
										}
									}
									if (n == -1)
									{
										mOffset--;
										break;
									}
									code <<= 4;
									// *16
									code += n;
									count++;
								}
								str.Append((char)code);
							}
							else
							{
								if (c == '0')
								{
									// octal
									//							int num;
									int code = 0;
									while (true)
									{
										c = mText[mOffset];
										mOffset++;
										while (c == CARRIAGE_RETURN || c == LINE_FEED)
										{
											c = mText[mOffset];
											mOffset++;
										}
										int n = -1;
										if (c >= '0' && c <= '7')
										{
											n = c - '0';
										}
										if (n == -1)
										{
											mOffset--;
											break;
										}
										code <<= 3;
										// * 8
										code += n;
									}
									str.Append((char)code);
								}
								else
								{
									str.Append((char)UnescapeBackSlash(c));
								}
							}
						}
						else
						{
							if (c == delimiter)
							{
								if (mOffset >= mText.Length)
								{
									mStringStatus = DELIMITER;
									break;
								}
								int offset = mOffset;
								SkipSpace();
								c = mText[mOffset];
								mOffset++;
								while (c == CARRIAGE_RETURN || c == LINE_FEED)
								{
									c = mText[mOffset];
									mOffset++;
								}
								if (c == delimiter)
								{
								}
								else
								{
									// sequence of 'A' 'B' will be combined as 'AB'
									mStringStatus = DELIMITER;
									mOffset = offset;
									break;
								}
							}
							else
							{
								if (embexpmode == true && c == '&')
								{
									if (mOffset >= mText.Length)
									{
										break;
									}
									mStringStatus = AMPERSAND;
									break;
								}
								else
								{
									if (embexpmode == true && c == '$')
									{
										// '{' must be placed immediately after '$'
										int offset = mOffset;
										c = mText[mOffset];
										mOffset++;
										while (c == CARRIAGE_RETURN || c == LINE_FEED)
										{
											c = mText[mOffset];
											mOffset++;
										}
										if (mOffset >= mText.Length)
										{
											break;
										}
										if (c == '{')
										{
											if (mOffset >= mText.Length)
											{
												break;
											}
											mStringStatus = DOLLAR;
											break;
										}
										else
										{
											mOffset = offset;
											str.Append((char)c);
										}
									}
									else
									{
										str.Append((char)c);
									}
								}
							}
						}
					}
				}
				catch (IndexOutOfRangeException)
				{
					mEOF = true;
					if (mStringStatus == NONE)
					{
						throw new CompileException(Error.StringParseError);
					}
				}
				if (mStringStatus == NONE)
				{
					throw new CompileException(Error.StringParseError);
				}
				return str.ToString();
			}
			return null;
		}

		public int GetStringStatus()
		{
			return mStringStatus;
		}

		public string Substring(int beginIndex, int endIndex)
		{
			return Sharpen.Runtime.Substring(mString, beginIndex, endIndex);
		}

		private void GenerateLineVector()
		{
			mLineVector = new IntVector();
			mLineLengthVector = new IntVector();
			int count = mText.Length;
			int lastCR = 0;
			int i;
			for (i = 0; i < count; i++)
			{
				int c = mText[i];
				if (c == CARRIAGE_RETURN || c == LINE_FEED)
				{
					mLineVector.Add(lastCR);
					mLineLengthVector.Add(i - lastCR);
					lastCR = i + 1;
					if ((i + 1) < count)
					{
						c = mText[i + 1];
						if (c == CARRIAGE_RETURN || c == LINE_FEED)
						{
							i++;
							lastCR = i + 1;
						}
					}
				}
			}
			if (i != lastCR)
			{
				mLineVector.Add(lastCR);
				mLineLengthVector.Add(i - lastCR);
			}
		}

		public int GetSrcPosToLine(int pos)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			// 2分法によって位置を求める
			int s = 0;
			int e = mLineVector.Size();
			while (true)
			{
				if ((e - s) <= 1)
				{
					return s + mLineOffset;
				}
				int m = s + (e - s) / 2;
				if (mLineVector.Get(m) > pos)
				{
					e = m;
				}
				else
				{
					s = m;
				}
			}
		}

		public int GetLineToSrcPos(int pos)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			return mLineVector.Get(pos);
		}

		public string GetLine(int line)
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			int start = mLineVector.Get(line);
			int length = mLineLengthVector.Get(line);
			return Sharpen.Runtime.Substring(mString, start, start + length);
		}

		public int GetMaxLine()
		{
			if (mLineVector == null)
			{
				GenerateLineVector();
			}
			return mLineVector.Size();
		}
	}
}

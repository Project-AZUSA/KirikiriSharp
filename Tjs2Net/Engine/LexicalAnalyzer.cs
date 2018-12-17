/*
 * TJS2 CSharp
 */

using System;
using System.Text;
using Tjs2.Sharpen;
using Double = Tjs2.Sharpen.Double;

namespace Tjs2.Engine
{
	public class LexicalAnalyzer
	{
		private bool mIsFirst;

		private bool mIsExprMode;

		private bool mResultNeeded;

		private bool mRegularExpression;

		private bool mBareWord;

		private bool mDicFunc;

		private int mValue;

		private int mPrevToken;

		private int mPrevPos;

		private int mNestLevel;

		private int mIfLevel;

		private const int BUFFER_CAPACITY = 1024;

		private const int CR = 13;

		private const int LF = 10;

		private const int TAB = unchecked((int)(0x09));

		private const int SPACE = unchecked((int)(0x20));

		private const int NOT_COMMENT = 0;

		private const int CONTINUE = 1;

		private const int ENDED = 2;

		private const int NONE = 0;

		private const int DELIMITER = 1;

		private const int AMPERSAND = 2;

		private const int DOLLAR = 3;

		private LongQue mRetValDeque;

		private AList<EmbeddableExpressionData> mEmbeddableExpressionDataStack;

		private AList<object> mValues;

		//private static final String TAG = "Lexer";
		//private static final boolean LOGD = false;
		// dicfunc quick-hack
		//private static final int CARRIAGE_RETURN = 13;
		//private static final int LINE_FEED = 10;
		//private static final int UNCLOSED_COMMENT = -1;
		// String Status
		// 下位がtoken,上位がvalue index
		internal virtual Variant GetValue(int idx)
		{
			return new Variant(mValues[idx]);
		}

		internal virtual string GetString(int idx)
		{
			object ret = mValues[idx];
			if (ret is string)
			{
				return (string)ret;
			}
			else
			{
				return null;
			}
		}

		private Compiler mBlock;

		private char[] mText;

		private int mCurrent;

		private int mStringStatus;

		private static StringBuilder mWorkBuilder;

		public static void Initialize()
		{
			mWorkBuilder = new StringBuilder(BUFFER_CAPACITY);
		}

		public static void FinalizeApplication()
		{
			mWorkBuilder = null;
		}

		public LexicalAnalyzer(Compiler block, string script, bool isexpr, bool resultneeded
			)
		{
			mRetValDeque = new LongQue();
			mEmbeddableExpressionDataStack = new AList<EmbeddableExpressionData>();
			mValues = new AList<object>();
			mBlock = block;
			mIsExprMode = isexpr;
			mResultNeeded = resultneeded;
			mPrevToken = -1;
			int scriptLen = script.Length;
			if (mIsExprMode)
			{
				mText = new char[scriptLen + 2];
				Sharpen.Runtime.GetCharsForString(script, 0, scriptLen, mText, 0);
				mText[scriptLen] = ';';
				mText[scriptLen + 1] = (char)0;
			}
			else
			{
				//mStream = new StringStream(script+";");
				if (script.StartsWith("#!") == true)
				{
					// #! を // に置换
					mText = new char[scriptLen + 1];
					Sharpen.Runtime.GetCharsForString(script, 2, scriptLen, mText, 2);
					mText[0] = mText[1] = '/';
					mText[scriptLen] = (char)0;
				}
				else
				{
					//mStream = new StringStream( "//" + script.substring(2));
					mText = new char[scriptLen + 1];
					Sharpen.Runtime.GetCharsForString(script, 0, scriptLen, mText, 0);
					mText[scriptLen] = (char)0;
				}
			}
			//mStream = new StringStream(script);
			if (CompileState.mEnableDicFuncQuickHack)
			{
				//----- dicfunc quick-hack
				//mDicFunc = false; // デフォルト值なので入れる必要なし
				//if( mIsExprMode && (script.startsWith("[") == true || script.startsWith("%[") == true) ) {
				char c = script[0];
				if (mIsExprMode && (c == '[' || (c == '%' && script[1] == '[')))
				{
					mDicFunc = true;
				}
			}
			//mIfLevel = 0;
			//mPrevPos = 0;
			//mNestLevel = 0;
			mIsFirst = true;
			//mRegularExpression = false;
			//mBareWord = false;
			PutValue(null);
		}

		/// <exception cref="CompileException"></exception>
		private int SkipComment()
		{
			char[] ptr = mText;
			int cur = mCurrent;
			if (ptr[cur] != '/')
			{
				return NOT_COMMENT;
			}
			char c = ptr[cur + 1];
			if (c == '/')
			{
				// line comment
				cur += 2;
				c = ptr[cur];
				while ((c != 0) && (c != CR && c != LF))
				{
					cur++;
					c = ptr[cur];
				}
				if (c != 0 && c == CR)
				{
					cur++;
					c = ptr[cur];
					if (c == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				//if( c == 0 ) return ENDED;
				//skipSpace();
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				mCurrent = cur;
				if (c == 0)
				{
					return ENDED;
				}
				return CONTINUE;
			}
			else
			{
				if (c == '*')
				{
					// ブロックコメント
					cur += 2;
					c = ptr[cur];
					if (c == 0)
					{
						mCurrent = cur;
						throw new CompileException(Error.UnclosedComment, mBlock, cur);
					}
					int level = 0;
					while (true)
					{
						if (c == '/' && ptr[cur + 1] == '*')
						{
							// コメントのネスト
							level++;
						}
						else
						{
							if (c == '*' && ptr[cur + 1] == '/')
							{
								if (level == 0)
								{
									cur += 2;
									c = ptr[cur];
									break;
								}
								level--;
							}
						}
						//if( !next() ) throw new CompileException( Error.UnclosedComment, mBlock, mCurrent );
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c == 0)
						{
							mCurrent = cur;
							throw new CompileException(Error.UnclosedComment, mBlock, cur);
						}
					}
					//if( mText[mCurrent] == 0 ) return ENDED;
					//skipSpace();
					while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
					{
						cur++;
						c = ptr[cur];
					}
					mCurrent = cur;
					if (c == 0)
					{
						return ENDED;
					}
					return CONTINUE;
				}
			}
			return NOT_COMMENT;
		}

		private const int TJS_IEEE_D_SIGNIFICAND_BITS = 52;

		private const int TJS_IEEE_D_EXP_MIN = -1022;

		private const int TJS_IEEE_D_EXP_MAX = 1023;

		private const long TJS_IEEE_D_EXP_BIAS = 1023;

		private bool ParseExtractNumber(int basebits)
		{
			bool point_found = false;
			bool exp_found = false;
			//int offset = mCurrent;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
				if (c == '.' && point_found == false && exp_found == false)
				{
					point_found = true;
					//next();
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				else
				{
					if ((c == 'p' || c == 'P') && exp_found == false)
					{
						exp_found = true;
						//next();
						cur++;
						c = ptr[cur];
						//if( c == CR && ptr[cur+1] == LF ) { cur++; c = ptr[cur]; }
						//skipSpace();
						while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
						{
							cur++;
							c = ptr[cur];
						}
						if (c == '+' || c == '-')
						{
							//next();
							cur++;
							c = ptr[cur];
							//if( c == CR && ptr[cur+1] == LF ) { cur++; c = ptr[cur]; }
							//skipSpace();
							while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
							{
								cur++;
								c = ptr[cur];
							}
						}
					}
					else
					{
						if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
						{
							if (basebits == 3)
							{
								if (c < '0' || c > '7')
								{
									break;
								}
							}
							else
							{
								if (basebits == 1)
								{
									if (c != '0' && c != '1')
									{
										break;
									}
								}
							}
							//next();
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
						}
						else
						{
							break;
						}
					}
				}
			}
			//mCurrent = offset;
			return point_found || exp_found;
		}

		// base
		// 16进数 : 4
		// 2进数 : 1
		// 8进数 : 3
		private double ParseNonDecimalReal(bool sign, int basebits)
		{
			long main = 0;
			int exp = 0;
			int numsignif = 0;
			bool pointpassed = false;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
				if (c == '.')
				{
					pointpassed = true;
				}
				else
				{
					if (c == 'p' || c == 'P')
					{
						//next();
						cur++;
						c = ptr[cur];
						//if( c == CR && ptr[cur+1] == LF ) { cur++; c = ptr[cur]; }
						//skipSpace();
						while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
						{
							cur++;
							c = ptr[cur];
						}
						bool biassign = false;
						if (c == '+')
						{
							biassign = false;
							//next();
							cur++;
							c = ptr[cur];
							//if( c == CR && ptr[cur+1] == LF ) { cur++; c = ptr[cur]; }
							//skipSpace();
							while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
							{
								cur++;
								c = ptr[cur];
							}
						}
						if (c == '-')
						{
							biassign = true;
							//next();
							cur++;
							c = ptr[cur];
							//if( c == CR && ptr[cur+1] == LF ) { cur++; c = ptr[cur]; }
							//skipSpace();
							while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
							{
								cur++;
								c = ptr[cur];
							}
						}
						int bias = 0;
						while (c >= '0' && c <= '9')
						{
							bias *= 10;
							bias += c - '0';
							//next();
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
						}
						if (biassign)
						{
							bias = -bias;
						}
						exp += bias;
						break;
					}
					else
					{
						int n = -1;
						if (basebits == 4)
						{
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
									else
									{
										break;
									}
								}
							}
						}
						else
						{
							if (basebits == 3)
							{
								if (c >= '0' && c <= '7')
								{
									n = c - '0';
								}
								else
								{
									break;
								}
							}
							else
							{
								if (basebits == 1)
								{
									if (c == '0' || c == '1')
									{
										n = c - '0';
									}
									else
									{
										break;
									}
								}
							}
						}
						if (numsignif == 0)
						{
							int b = basebits - 1;
							while (b >= 0)
							{
								if (((1 << b) & n) != 0)
								{
									break;
								}
								b--;
							}
							b++;
							if (b != 0)
							{
								// n is not zero
								numsignif = b;
								main |= ((long)n << (64 - numsignif));
								if (pointpassed)
								{
									exp -= (basebits - b + 1);
								}
								else
								{
									exp = b - 1;
								}
							}
							else
							{
								// n is zero
								if (pointpassed)
								{
									exp -= basebits;
								}
							}
						}
						else
						{
							// append to main
							if ((numsignif + basebits) < 64)
							{
								numsignif += basebits;
								main |= ((long)n << (64 - numsignif));
							}
							if (pointpassed == false)
							{
								exp += basebits;
							}
						}
					}
				}
				//next();
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
			}
			mCurrent = cur;
			main = (long)(((ulong)main) >> (64 - 1 - TJS_IEEE_D_SIGNIFICAND_BITS));
			if (main == 0)
			{
				return (0.0);
			}
			main &= ((1L << TJS_IEEE_D_SIGNIFICAND_BITS) - 1L);
			if (exp < TJS_IEEE_D_EXP_MIN)
			{
				return (0.0);
			}
			if (exp > TJS_IEEE_D_EXP_MAX)
			{
				if (sign)
				{
					return (double.NegativeInfinity);
				}
				else
				{
					return (double.PositiveInfinity);
				}
			}
			// compose IEEE double
			//double d = Double.longBitsToDouble(0x8000000000000000L | ((exp + TJS_IEEE_D_EXP_BIAS) << 52) | main);
			double d = Double.LongBitsToDouble((((long)exp + TJS_IEEE_D_EXP_BIAS) << 52) | main
				);
			if (sign)
			{
				d = -d;
			}
			return (d);
		}

		private int ParseNonDecimalInteger16(bool sign)
		{
			long v = 0;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
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
						else
						{
							break;
						}
					}
				}
				v <<= 4;
				v += n;
				//next();
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
			}
			mCurrent = cur;
			if (sign)
			{
				return Sharpen.Extensions.ValueOf((int)-v);
			}
			else
			{
				return Sharpen.Extensions.ValueOf((int)v);
			}
		}

		private int ParseNonDecimalInteger8(bool sign)
		{
			long v = 0;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
				int n = -1;
				if (c >= '0' && c <= '7')
				{
					n = c - '0';
				}
				else
				{
					break;
				}
				v <<= 3;
				v += n;
				//next();
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
			}
			mCurrent = cur;
			if (sign)
			{
				return Sharpen.Extensions.ValueOf((int)-v);
			}
			else
			{
				return Sharpen.Extensions.ValueOf((int)v);
			}
		}

		private int ParseNonDecimalInteger2(bool sign)
		{
			long v = 0;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
				if (c == '1')
				{
					v <<= 1;
					v++;
				}
				else
				{
					if (c == '0')
					{
						v <<= 1;
					}
					else
					{
						break;
					}
				}
				//next();
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
			}
			mCurrent = cur;
			if (sign)
			{
				return Sharpen.Extensions.ValueOf((int)-v);
			}
			else
			{
				return Sharpen.Extensions.ValueOf((int)v);
			}
		}

		private Number ParseNonDecimalNumber(bool sign, int @base)
		{
			bool is_real = ParseExtractNumber(@base);
			if (is_real)
			{
				return new Number(ParseNonDecimalReal(sign, @base));
			}
			else
			{
				switch (@base)
				{
					case 4:
					{
						return new Number(ParseNonDecimalInteger16(sign));
					}

					case 3:
					{
						return new Number(ParseNonDecimalInteger8(sign));
					}

					case 1:
					{
						return new Number(ParseNonDecimalInteger2(sign));
					}
				}
			}
			return null;
		}

		// @return : Integer or Double or null
		private Number ParseNumber()
		{
			int num = 0;
			bool sign = false;
			bool skipNum = false;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			if (c == '+')
			{
				sign = false;
				//if( !next() ) return null;
				cur++;
				c = ptr[cur];
				//if( !skipSpace() ) return null;
				//c = mText[mCurrent];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				if (c == 0)
				{
					mCurrent = cur;
					return null;
				}
			}
			else
			{
				if (c == '-')
				{
					sign = true;
					//if( !next() ) return null;
					cur++;
					c = ptr[cur];
					//if( !skipSpace() ) return null;
					//c = mText[cur];
					while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
					{
						cur++;
						c = ptr[cur];
					}
					if (c == 0)
					{
						mCurrent = cur;
						return null;
					}
				}
			}
			if (c > '9')
			{
				// 't', 'f', 'N', 'I' は '9' より大きい
				if (c == 't' && ptr[cur + 1] == 'r' && ptr[cur + 2] == 'u' && ptr[cur + 3] == 'e')
				{
					cur += 4;
					mCurrent = cur;
                    return new Number(1);
				}
				else
				{
					if (c == 'f' && ptr[cur + 1] == 'a' && ptr[cur + 2] == 'l' && ptr[cur + 3] == 's'
						 && ptr[cur + 4] == 'e')
					{
						cur += 5;
						mCurrent = cur;
                        return new Number(0);
					}
					else
					{
						if (c == 'N' && ptr[cur + 1] == 'a' && ptr[cur + 2] == 'N')
						{
							cur += 3;
							mCurrent = cur;
							return new Number(double.NaN);
						}
						else
						{
							if (c == 'I' && ptr[cur + 1] == 'n' && ptr[cur + 2] == 'f' && ptr[cur + 3] == 'i'
								 && ptr[cur + 4] == 'n' && ptr[cur + 5] == 'i' && ptr[cur + 6] == 't' && ptr[cur
								 + 7] == 'y')
							{
								cur += 8;
								mCurrent = cur;
								if (sign)
								{
                                    return new Number(double.NegativeInfinity);
								}
								else
								{
                                    return new Number(double.PositiveInfinity);
								}
							}
						}
					}
				}
			}
			// 10进数以外か调べる
			if (c == '0')
			{
				//if( !next() ) {
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
				if (c == 0)
				{
					mCurrent = cur;
                    return new Number(0);
				}
				if (c == 'x' || c == 'X')
				{
					// hexadecimal
					//if( !next() ) return null;
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
					mCurrent = cur;
					if (c == 0)
					{
						return null;
					}
					return ParseNonDecimalNumber(sign, 4);
				}
				else
				{
					if (c == 'b' || c == 'B')
					{
						// binary
						//if( !next() ) return null;
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						mCurrent = cur;
						if (c == 0)
						{
							return null;
						}
						return ParseNonDecimalNumber(sign, 1);
					}
					else
					{
						if (c == '.')
						{
							skipNum = true;
						}
						else
						{
							if (c == 'e' || c == 'E')
							{
								skipNum = true;
							}
							else
							{
								if (c == 'p' || c == 'P')
								{
									// 2^n exp
									mCurrent = cur;
									return null;
								}
								else
								{
									if (c >= '0' && c <= '7')
									{
										// octal
										mCurrent = cur;
										return ParseNonDecimalNumber(sign, 3);
									}
								}
							}
						}
					}
				}
			}
			if (skipNum == false)
			{
				while (c != 0)
				{
					if (c < '0' || c > '9')
					{
						break;
					}
					num = num * 10 + (c - '0');
					//next();
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
			}
			if (c == '.' || c == 'e' || c == 'E')
			{
				double figure = 1.0;
				int dec = 0;
				if (c == '.')
				{
					do
					{
						//next();
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c < '0' || c > '9')
						{
							break;
						}
						dec = dec * 10 + (c - '0');
						figure *= 10;
					}
					while (c != 0);
				}
				bool expSign = false;
				int expValue = 0;
				if (c == 'e' || c == 'E')
				{
					//next();
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
					if (c == '-')
					{
						expSign = true;
						//next();
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
					}
					while (c != 0)
					{
						if (c < '0' || c > '9')
						{
							break;
						}
						expValue = expValue * 10 + (c - '0');
						//next();
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
					}
				}
                double number = (double)num + ((double)dec / figure);
				if (expValue != 0)
				{
					if (expSign == false)
					{
						number *= Math.Pow(10, expValue);
					}
					else
					{
						number /= Math.Pow(10, expValue);
					}
				}
				if (sign)
				{
					number = -number;
				}
				mCurrent = cur;
                return new Number(number);
			}
			else
			{
				if (sign)
				{
					num = -num;
				}
				mCurrent = cur;
                return new Number(num);
			}
		}

		/// <exception cref="CompileException"></exception>
		private string ReadString(int delimiter, bool embexpmode)
		{
			mStringStatus = NONE;
			int cur = mCurrent;
			char[] ptr = mText;
			StringBuilder str = mWorkBuilder;
			str.Delete(0, str.Length);
			while (ptr[cur] != 0)
			{
				char c = ptr[cur];
				if (c == '\\')
				{
					// escape
					// Next
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
					if (c == 0)
					{
						break;
					}
					if (c == 'x' || c == 'X')
					{
						// hex
						// starts with a "\x", be parsed while characters are
						// recognized as hex-characters, but limited of size of char.
						// on Windows, \xXXXXX will be parsed to UNICODE 16bit characters.
						// Next
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c == 0)
						{
							break;
						}
						int code = 0;
						int count = 0;
						while (count < 4)
						{
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
									else
									{
										break;
									}
								}
							}
							code <<= 4;
							// *16
							code += n;
							count++;
							// Next
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								break;
							}
						}
						if (c == 0)
						{
							break;
						}
						str.Append((char)code);
					}
					else
					{
						if (c == '0')
						{
							// octal
							// Next
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								break;
							}
							int code = 0;
							while (true)
							{
								int n = -1;
								if (c >= '0' && c <= '7')
								{
									n = c - '0';
								}
								else
								{
									break;
								}
								code <<= 3;
								// * 8
								code += n;
								// Next
								cur++;
								c = ptr[cur];
								if (c == CR && ptr[cur + 1] == LF)
								{
									cur++;
									c = ptr[cur];
								}
								if (c == 0)
								{
									break;
								}
							}
							str.Append((char)code);
						}
						else
						{
							switch (c)
							{
								case 'a':
								{
									//str.append( (char)unescapeBackSlash(c) );
									c = (char)unchecked((int)(0x07));
									break;
								}

								case 'b':
								{
									c = (char)unchecked((int)(0x08));
									break;
								}

								case 'f':
								{
									c = (char)unchecked((int)(0x0c));
									break;
								}

								case 'n':
								{
									c = (char)unchecked((int)(0x0a));
									break;
								}

								case 'r':
								{
									c = (char)unchecked((int)(0x0d));
									break;
								}

								case 't':
								{
									c = (char)unchecked((int)(0x09));
									break;
								}

								case 'v':
								{
									c = (char)unchecked((int)(0x0b));
									break;
								}
							}
							str.Append((char)c);
							// Next
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
						}
					}
				}
				else
				{
					if (c == delimiter)
					{
						// Next
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c == 0)
						{
							mStringStatus = DELIMITER;
							break;
						}
						int offset = cur;
						// skipSpace();
						while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
						{
							cur++;
							c = ptr[cur];
						}
						if (c == delimiter)
						{
							// sequence of 'A' 'B' will be combined as 'AB'
							// Next
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
						}
						else
						{
							cur = offset;
							mStringStatus = DELIMITER;
							break;
						}
					}
					else
					{
						if (embexpmode == true && c == '&')
						{
							// Next
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
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
								int offset = cur;
								// Next
								cur++;
								c = ptr[cur];
								if (c == CR && ptr[cur + 1] == LF)
								{
									cur++;
									c = ptr[cur];
								}
								if (c == 0)
								{
									break;
								}
								if (c == '{')
								{
									// Next
									cur++;
									c = ptr[cur];
									if (c == CR && ptr[cur + 1] == LF)
									{
										cur++;
										c = ptr[cur];
									}
									if (c == 0)
									{
										break;
									}
									mStringStatus = DOLLAR;
									break;
								}
								else
								{
									cur = offset;
									c = ptr[cur];
									str.Append((char)c);
									// Next
									cur++;
									c = ptr[cur];
									if (c == CR && ptr[cur + 1] == LF)
									{
										cur++;
										c = ptr[cur];
									}
								}
							}
							else
							{
								str.Append((char)c);
								// Next
								cur++;
								c = ptr[cur];
								if (c == CR && ptr[cur + 1] == LF)
								{
									cur++;
									c = ptr[cur];
								}
							}
						}
					}
				}
			}
			mCurrent = cur;
			if (mStringStatus == NONE)
			{
				throw new CompileException(Error.StringParseError, mBlock, mCurrent);
			}
			return str.ToString();
		}

		public virtual void SetStartOfRegExp()
		{
			mRegularExpression = true;
		}

		public virtual void SetNextIsBareWord()
		{
			mBareWord = true;
		}

		/// <exception cref="CompileException"></exception>
		private string ParseRegExp()
		{
			bool ok = false;
			bool lastbackslash = false;
			StringBuilder str = mWorkBuilder;
			str.Delete(0, str.Length);
			StringBuilder flag = null;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (c != 0)
			{
				if (c == '\\')
				{
					str.Append((char)c);
					if (lastbackslash == true)
					{
						lastbackslash = false;
					}
					else
					{
						lastbackslash = true;
					}
				}
				else
				{
					if (c == '/' && lastbackslash == false)
					{
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c == 0)
						{
							ok = true;
							break;
						}
						//int flagMask = 0;
						if (flag == null)
						{
							flag = new StringBuilder(BUFFER_CAPACITY);
						}
						else
						{
							flag.Delete(0, flag.Length);
						}
						while (c >= 'a' && c <= 'z')
						{
							flag.Append((char)c);
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								break;
							}
						}
						str.Insert(0, "//");
						string flgStr = flag.ToString();
						str.Insert(2, flgStr);
						str.Insert(2 + flgStr.Length, "/");
						ok = true;
						break;
					}
					else
					{
						lastbackslash = false;
						str.Append((char)c);
					}
				}
				cur++;
				c = ptr[cur];
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
			}
			mCurrent = cur;
			if (!ok)
			{
				throw new CompileException(Error.StringParseError);
			}
			return str.ToString();
		}

		// 渡されたByteBufferを切り诘めた、新しいByteBufferを作る
		private ByteBuffer CompactByteBuffer(ByteBuffer b)
		{
			int count = b.Position();
			ByteBuffer ret = ByteBuffer.Allocate(count);
			b.Position(0);
			for (int i = 0; i < count; i++)
			{
				ret.Put(b.Get());
			}
			ret.Position(0);
			return ret;
		}

		/// <exception cref="CompileException"></exception>
		private ByteBuffer ParseOctet()
		{
			// parse a octet literal;
			// syntax is:
			// <% xx xx xx xx xx xx ... %>
			// where xx is hexadecimal 8bit(octet) binary representation.
			char[] ptr = mText;
			int cur = mCurrent + 1;
			//mCurrent++;
			char c = ptr[cur];
			// skip %
			if (c == CR && ptr[cur + 1] == LF)
			{
				cur++;
				c = ptr[cur];
			}
			bool leading = true;
			byte oct = 0;
			// int count = mStream.countOctetTail() / 2 + 1;
			int count = 0;
			if (c != 0)
			{
				int offset = cur;
				//while( (ptr[offset] == '%' && ptr[offset+1] == '>') == false ) offset++;
				int len = ptr.Length;
				while ((offset + 1) < len)
				{
					if (ptr[offset] == '%' && ptr[offset + 1] == '>')
					{
						break;
					}
					offset++;
				}
				count = offset - cur;
			}
			count = count / 2 + 1;
			ByteBuffer buffer = ByteBuffer.Allocate(count);
			while (c != 0)
			{
				if (c == '/')
				{
					if (SkipComment() == ENDED)
					{
						mCurrent = cur;
						throw new CompileException(Error.StringParseError, mBlock, cur);
					}
				}
				c = ptr[cur];
				int n = cur + 1;
				int next = ptr[n];
				if (next == CR && ptr[n + 1] == LF)
				{
					n++;
					c = ptr[n];
				}
				if (c == '%' && next == '>')
				{
					cur = n;
					if (ptr[cur] != 0)
					{
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
					}
					if (!leading)
					{
						buffer.Put(oct);
					}
					mCurrent = cur;
					return CompactByteBuffer(buffer);
				}
				//int num = getHexNum(c);
				int num;
				if (c >= '0' && c <= '9')
				{
					num = c - '0';
				}
				else
				{
					if (c >= 'a' && c <= 'f')
					{
						num = c - 'a' + 10;
					}
					else
					{
						if (c >= 'A' && c <= 'F')
						{
							num = c - 'A' + 10;
						}
						else
						{
							num = -1;
						}
					}
				}
				if (num != -1)
				{
					if (leading)
					{
						oct = unchecked((byte)num);
						leading = false;
					}
					else
					{
						oct <<= 4;
						oct += (byte)num;
						buffer.Put(oct);
						leading = true;
					}
				}
				if (leading == false && c == ',')
				{
					buffer.Put(oct);
					leading = true;
				}
				cur = n;
			}
			mCurrent = cur;
			throw new CompileException(Error.StringParseError, mBlock, cur);
		}

		//private String parseString( int delimiter ) throws CompileException {
		//	return readString(delimiter,false);
		//}
		/// <exception cref="CompileException"></exception>
		private int ParsePPExpression(string script)
		{
			PreprocessorExpressionParser parser = new PreprocessorExpressionParser(mBlock.GetTJS
				(), script);
			return parser.Parse();
		}

		/// <exception cref="CompileException"></exception>
		private int ProcessPPStatement()
		{
			// process pre-prosessor statements.
			// int offset = mCurrent;
			// mCurrent++; // skip '@'
			int cur = mCurrent + 1;
			// skip '@'
			char[] ptr = mText;
			char c = ptr[cur];
			if (c == 's' && ptr[cur + 1] == 'e' && ptr[cur + 2] == 't')
			{
				// set statemenet
				mBlock.NotifyUsingPreProcessror();
				cur += 3;
				c = ptr[cur];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				if (c == 0)
				{
					mCurrent = cur;
					throw new CompileException(Error.PPError, mBlock, cur);
				}
				if (c != '(')
				{
					mCurrent = cur;
					throw new CompileException(Error.PPError, mBlock, cur);
				}
				cur++;
				c = ptr[cur];
				// next '('
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
				StringBuilder script = mWorkBuilder;
				script.Delete(0, script.Length);
				int plevel = 0;
				while (c != 0 && (plevel != 0 || c != ')'))
				{
					if (c == '(')
					{
						plevel++;
					}
					else
					{
						if (c == ')')
						{
							plevel--;
						}
					}
					script.Append((char)c);
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				if (c != 0)
				{
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				ParsePPExpression(script.ToString());
				//skipSpace();
				c = ptr[cur];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				mCurrent = cur;
				if (c == 0)
				{
					return ENDED;
				}
				return CONTINUE;
			}
			if (c == 'i' && ptr[cur + 1] == 'f')
			{
				// if statement
				mBlock.NotifyUsingPreProcessror();
				cur += 2;
				//if( !skipSpace() ) throw new CompileException( Error.PPError, mBlock, mCurrent );
				c = ptr[cur];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				if (c == 0)
				{
					mCurrent = cur;
					throw new CompileException(Error.PPError, mBlock, cur);
				}
				if (c != '(')
				{
					mCurrent = cur;
					throw new CompileException(Error.PPError, mBlock, cur);
				}
				cur++;
				c = ptr[cur];
				// next '('
				if (c == CR && ptr[cur + 1] == LF)
				{
					cur++;
					c = ptr[cur];
				}
				StringBuilder script = mWorkBuilder;
				script.Delete(0, script.Length);
				int plevel = 0;
				while (c != 0 && (plevel != 0 || c != ')'))
				{
					if (c == '(')
					{
						plevel++;
					}
					else
					{
						if (c == ')')
						{
							plevel--;
						}
					}
					script.Append((char)c);
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				if (c != 0)
				{
					cur++;
					c = ptr[cur];
					if (c == CR && ptr[cur + 1] == LF)
					{
						cur++;
						c = ptr[cur];
					}
				}
				int ret = ParsePPExpression(script.ToString());
				if (ret == 0)
				{
					mCurrent = cur;
					return SkipUntilEndif();
				}
				mIfLevel++;
				//skipSpace();
				c = ptr[cur];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				mCurrent = cur;
				if (c == 0)
				{
					return ENDED;
				}
				return CONTINUE;
			}
			if (c == 'e' && ptr[cur + 1] == 'n' && ptr[cur + 2] == 'd' && ptr[cur + 3] == 'i'
				 && ptr[cur + 4] == 'f')
			{
				// endif statement
				cur += 5;
				mIfLevel--;
				if (mIfLevel < 0)
				{
					mCurrent = cur;
					throw new CompileException(Error.PPError, mBlock, cur);
				}
				//skipSpace();
				c = ptr[cur];
				while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
				{
					cur++;
					c = ptr[cur];
				}
				mCurrent = cur;
				if (c == 0)
				{
					return ENDED;
				}
				return CONTINUE;
			}
			// mCurrent = offset;
			return NOT_COMMENT;
		}

		/// <exception cref="CompileException"></exception>
		private int SkipUntilEndif()
		{
			int exl = mIfLevel;
			mIfLevel++;
			char[] ptr = mText;
			int cur = mCurrent;
			char c = ptr[cur];
			while (true)
			{
				if (c == '/')
				{
					// comment
					mCurrent = cur;
					int ret = SkipComment();
					cur = mCurrent;
					switch (ret)
					{
						case ENDED:
						{
							mCurrent = cur;
							throw new CompileException(Error.PPError, mBlock, cur);
						}

						case CONTINUE:
						{
							c = ptr[cur];
							break;
						}

						case NOT_COMMENT:
						{
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								mCurrent = cur;
								throw new CompileException(Error.PPError, mBlock, cur);
							}
							break;
						}
					}
				}
				else
				{
					if (c == '@')
					{
						cur++;
						c = ptr[cur];
						bool skipp = false;
						if (c == 'i' && ptr[cur + 1] == 'f')
						{
							mIfLevel++;
							cur += 2;
							c = ptr[cur];
							skipp = true;
						}
						else
						{
							if (c == 's' && ptr[cur + 1] == 'e' && ptr[cur + 2] == 't')
							{
								cur += 3;
								c = ptr[cur];
								skipp = true;
							}
							else
							{
								if (c == 'e' && ptr[cur + 1] == 'n' && ptr[cur + 2] == 'd' && ptr[cur + 3] == 'i'
									 && ptr[cur + 4] == 'f')
								{
									cur += 5;
									c = ptr[cur];
									mIfLevel--;
									if (mIfLevel == exl)
									{
										// skip ended
										//skipSpace();
										while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
										{
											cur++;
											c = ptr[cur];
										}
										mCurrent = cur;
										if (c == 0)
										{
											return ENDED;
										}
										return CONTINUE;
									}
								}
							}
						}
						//else {}
						if (skipp)
						{
							while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								mCurrent = cur;
								throw new CompileException(Error.PPError, mBlock, cur);
							}
							if (c != '(')
							{
								mCurrent = cur;
								throw new CompileException(Error.PPError, mBlock, cur);
							}
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							int plevel = 0;
							while (c != 0 && (plevel > 0 || c != ')'))
							{
								if (c == '(')
								{
									plevel++;
								}
								else
								{
									if (c == ')')
									{
										plevel--;
									}
								}
								cur++;
								c = ptr[cur];
								if (c == CR && ptr[cur + 1] == LF)
								{
									cur++;
									c = ptr[cur];
								}
							}
							if (c == 0)
							{
								mCurrent = cur;
								throw new CompileException(Error.PPError, mBlock, cur);
							}
							cur++;
							c = ptr[cur];
							if (c == CR && ptr[cur + 1] == LF)
							{
								cur++;
								c = ptr[cur];
							}
							if (c == 0)
							{
								mCurrent = cur;
								throw new CompileException(Error.PPError, mBlock, cur);
							}
						}
					}
					else
					{
						cur++;
						c = ptr[cur];
						if (c == CR && ptr[cur + 1] == LF)
						{
							cur++;
							c = ptr[cur];
						}
						if (c == 0)
						{
							mCurrent = cur;
							throw new CompileException(Error.PPError, mBlock, cur);
						}
					}
				}
			}
		}

		private static string EscapeC(char b)
		{
            int c = b;
			switch (c)
			{
				case unchecked((int)(0x07)):
				{
					return ("\\a");
				}

				case unchecked((int)(0x08)):
				{
					return ("\\b");
				}

				case unchecked((int)(0x0c)):
				{
					return ("\\f");
				}

				case unchecked((int)(0x0a)):
				{
					return ("\\n");
				}

				case unchecked((int)(0x0d)):
				{
					return ("\\r");
				}

				case unchecked((int)(0x09)):
				{
					return ("\\t");
				}

				case unchecked((int)(0x0b)):
				{
					return ("\\v");
				}

				case '\\':
				{
					return ("\\\\");
				}

				case '\'':
				{
					return ("\\\'");
				}

				case '\"':
				{
					return ("\\\"");
				}

				default:
				{
					if (c < unchecked((int)(0x20)))
					{
						StringBuilder ret = mWorkBuilder;
						ret.Delete(0, ret.Length);
						ret.Append("\\x");
						ret.Append(Sharpen.Extensions.ToHexString((int)c));
						return ret.ToString();
					}
                    return c.ToString();
				}
			}
		}

		private int PutValue(object val)
		{
			mValues.AddItem(val);
			return mValues.Count - 1;
		}

		/// <exception cref="CompileException"></exception>
		private int GetToken()
		{
			char[] ptr = mText;
			int c = ptr[mCurrent];
			if (c == 0)
			{
				return 0;
			}
			if (mRegularExpression == true)
			{
				mRegularExpression = false;
				mCurrent = mPrevPos;
				//next(); // 最初の'/'を读み飞ばし
				mCurrent++;
				if (mText[mCurrent] == CR && mText[mCurrent + 1] == LF)
				{
					mCurrent++;
				}
				string pattern = ParseRegExp();
				mValue = PutValue(pattern);
				return Token.T_REGEXP;
			}
			bool retry;
			do
			{
				retry = false;
				mPrevPos = mCurrent;
				c = ptr[mCurrent];
				switch (c)
				{
					case 0:
					{
						return 0;
					}

					case '>':
				    {
				        mCurrent++;
				        c = ptr[mCurrent];
				        if (c == '>')
				        {
				            // >>
				            mCurrent++;
				            c = ptr[mCurrent];
				            if (c == '>')
				            {
				                // >>>
				                mCurrent++;
				                c = ptr[mCurrent];
				                if (c == '=')
				                {
				                    // >>>=
				                    mCurrent++;
				                    return Token.T_RBITSHIFTEQUAL;
				                }
				                else
				                {
				                    return Token.T_RBITSHIFT;
				                }
				            }
				            else
				            {
				                if (c == '=')
				                {
				                    // >>=
				                    mCurrent++;
				                    return Token.T_RARITHSHIFTEQUAL;
				                }
				                else
				                {
				                    // >>
				                    return Token.T_RARITHSHIFT;
				                }
				            }
				        }
				        else
				        {
				            if (c == '=')
				            {
				                // >=
				                mCurrent++;
				                return Token.T_GTOREQUAL;
				            }
				            return Token.T_GT;
				        }
				    }

				    case '<':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '<')
						{
							// <<
							mCurrent++;
							c = ptr[mCurrent];
							if (c == '=')
							{
								// <<=
								mCurrent++;
								return Token.T_LARITHSHIFTEQUAL;
							}
							else
							{
								// <<
								return Token.T_LARITHSHIFT;
							}
						}
						else
						{
							if (c == '-')
							{
								// <-
								mCurrent++;
								c = ptr[mCurrent];
								if (c == '>')
								{
									// <->
									mCurrent++;
									return Token.T_SWAP;
								}
								else
								{
									// <
									mCurrent--;
									return Token.T_LT;
								}
							}
							else
							{
								if (c == '=')
								{
									// <=
									mCurrent++;
									return Token.T_LTOREQUAL;
								}
								else
								{
									if (c == '%')
									{
										// '<%'   octet literal
										ByteBuffer buffer = ParseOctet();
										mValue = PutValue(buffer);
										return Token.T_CONSTVAL;
									}
									else
									{
										// <
										return Token.T_LT;
									}
								}
							}
						}
					}

					case '=':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							// ===
							mCurrent++;
							c = ptr[mCurrent];
							if (c == '=')
							{
								mCurrent++;
								return Token.T_DISCEQUAL;
							}
							else
							{
								// ==
								return Token.T_EQUALEQUAL;
							}
						}
						else
						{
							if (c == '>')
							{
								// =>
								mCurrent++;
								return Token.T_COMMA;
							}
							else
							{
								// =
								return Token.T_EQUAL;
							}
						}
					}

					case '!':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							mCurrent++;
							c = ptr[mCurrent];
							if (c == '=')
							{
								// !==
								mCurrent++;
								return Token.T_DISCNOTEQUAL;
							}
							else
							{
								// !=
								return Token.T_NOTEQUAL;
							}
						}
						else
						{
							// !
							return Token.T_EXCRAMATION;
						}
					}

					case '&':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '&')
						{
							mCurrent++;
							c = ptr[mCurrent];
							if (c == '=')
							{
								// &&=
								mCurrent++;
								return Token.T_LOGICALANDEQUAL;
							}
							else
							{
								// &&
								return Token.T_LOGICALAND;
							}
						}
						else
						{
							if (c == '=')
							{
								// &=
								mCurrent++;
								return Token.T_AMPERSANDEQUAL;
							}
							else
							{
								// &
								return Token.T_AMPERSAND;
							}
						}
					}

					case '|':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '|')
						{
							mCurrent++;
							c = ptr[mCurrent];
							if (c == '=')
							{
								// ||=
								mCurrent++;
								return Token.T_LOGICALOREQUAL;
							}
							else
							{
								// ||
								return Token.T_LOGICALOR;
							}
						}
						else
						{
							if (c == '=')
							{
								// |=
								mCurrent++;
								return Token.T_VERTLINEEQUAL;
							}
							else
							{
								// |
								return Token.T_VERTLINE;
							}
						}
					}

					case '.':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c >= '0' && c <= '9')
						{
							// number
							mCurrent--;
							//mCurrent--;
							Number o = ParseNumber();
							if (o != null)
							{
								if (o.IsInt())
								{
									mValue = PutValue((int)o);
								}
								else
								{
									if (o.IsDouble())
									{
										mValue = PutValue((double)o);
									}
									else
									{
										mValue = PutValue(null);
									}
								}
							}
							else
							{
								mValue = PutValue(null);
							}
							return Token.T_CONSTVAL;
						}
						else
						{
							if (c == '.')
							{
								mCurrent++;
								c = ptr[mCurrent];
								if (c == '.')
								{
									// ...
									mCurrent++;
									return Token.T_OMIT;
								}
								else
								{
									// .
									mCurrent--;
									//mCurrent--;
									return Token.T_DOT;
								}
							}
							else
							{
								// .
								return Token.T_DOT;
							}
						}
					}

					case '+':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '+')
						{
							// ++
							mCurrent++;
							return Token.T_INCREMENT;
						}
						else
						{
							if (c == '=')
							{
								// +=
								mCurrent++;
								return Token.T_PLUSEQUAL;
							}
							else
							{
								// +
								return Token.T_PLUS;
							}
						}
					}

					case '-':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '-')
						{
							// --
							mCurrent++;
							return Token.T_DECREMENT;
						}
						else
						{
							if (c == '=')
							{
								mCurrent++;
								return Token.T_MINUSEQUAL;
							}
							else
							{
								// -=
								// -
								return Token.T_MINUS;
							}
						}
					}

					case '*':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							// *=
							mCurrent++;
							return Token.T_ASTERISKEQUAL;
						}
						else
						{
							// *
							return Token.T_ASTERISK;
						}
					}

					case '/':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '/' || c == '*')
						{
							mCurrent--;
							int comment = SkipComment();
							if (comment == CONTINUE)
							{
								retry = true;
								break;
							}
							else
							{
								if (comment == ENDED)
								{
									return 0;
								}
							}
						}
						if (c == '=')
						{
							// /=
							mCurrent++;
							return Token.T_SLASHEQUAL;
						}
						else
						{
							// /
							return Token.T_SLASH;
						}
					}

					case '\\':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							// \=
							mCurrent++;
							return Token.T_BACKSLASHEQUAL;
						}
						else
						{
							// \
							return Token.T_BACKSLASH;
						}
					}

					case '%':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							// %=
							mCurrent++;
							return Token.T_PERCENTEQUAL;
						}
						else
						{
							// %
							return Token.T_PERCENT;
						}
					}

					case '^':
					{
						mCurrent++;
						c = ptr[mCurrent];
						if (c == '=')
						{
							// ^=
							mCurrent++;
							return Token.T_CHEVRONEQUAL;
						}
						else
						{
							// ^
							return Token.T_CHEVRON;
						}
					}

					case '[':
					{
						mNestLevel++;
						mCurrent++;
						return Token.T_LBRACKET;
					}

					case ']':
					{
						mNestLevel--;
						mCurrent++;
						return Token.T_RBRACKET;
					}

					case '(':
					{
						mNestLevel++;
						mCurrent++;
						return Token.T_LPARENTHESIS;
					}

					case ')':
					{
						mNestLevel--;
						mCurrent++;
						return Token.T_RPARENTHESIS;
					}

					case '~':
					{
						mCurrent++;
						return Token.T_TILDE;
					}

					case '?':
					{
						mCurrent++;
						return Token.T_QUESTION;
					}

					case ':':
					{
						mCurrent++;
						return Token.T_COLON;
					}

					case ',':
					{
						mCurrent++;
						return Token.T_COMMA;
					}

					case ';':
					{
						mCurrent++;
						return Token.T_SEMICOLON;
					}

					case '{':
					{
						mNestLevel++;
						mCurrent++;
						return Token.T_LBRACE;
					}

					case '}':
					{
						mNestLevel--;
						mCurrent++;
						return Token.T_RBRACE;
					}

					case '#':
					{
						mCurrent++;
						return Token.T_SHARP;
					}

					case '$':
					{
						mCurrent++;
						return Token.T_DOLLAR;
					}

					case '\'':
					case '\"':
					{
						// literal string
						//String str = parseString(c);
						//next();
						mCurrent++;
						if (mText[mCurrent] == CR && mText[mCurrent + 1] == LF)
						{
							mCurrent++;
						}
						string str = ReadString(c, false);
						mValue = PutValue(str);
						return Token.T_CONSTVAL;
					}

					case '@':
					{
						// embeddable expression in string (such as @"this can be embeddable like &variable;")
						int org = mCurrent;
						//if( !next() ) return 0;
						mCurrent++;
						c = ptr[mCurrent];
						if (c == CR && ptr[mCurrent + 1] == LF)
						{
							mCurrent++;
							c = ptr[mCurrent];
						}
						//if( !skipSpace() ) return 0;
						while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
						{
							mCurrent++;
							c = ptr[mCurrent];
						}
						if (c == 0)
						{
							return 0;
						}
						//c = ptr[mCurrent];
						if (c == '\'' || c == '\"')
						{
							EmbeddableExpressionData data = new EmbeddableExpressionData();
							data.mState = EmbeddableExpressionData.START;
							data.mWaitingNestLevel = mNestLevel;
							data.mDelimiter = c;
							data.mNeedPlus = false;
							//if( !next() ) return 0;
							mCurrent++;
							c = ptr[mCurrent];
							if (c == CR && ptr[mCurrent + 1] == LF)
							{
								mCurrent++;
								c = ptr[mCurrent];
							}
							if (c == 0)
							{
								return 0;
							}
							mEmbeddableExpressionDataStack.AddItem(data);
							return -1;
						}
						else
						{
							mCurrent = org;
						}
						switch (ProcessPPStatement())
						{
							case CONTINUE:
							{
								// possible pre-procesor statements
								retry = true;
								break;
							}

							case ENDED:
							{
								return 0;
							}

							case NOT_COMMENT:
							{
								mCurrent = org;
                                break;
							}
						}
						break;
					}

					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					{
						Number o = ParseNumber();
						if (o != null)
						{
							if (o.IsInt())
							{
								mValue = PutValue((int)o);
							}
							else
							{
								if (o.IsDouble())
								{
									mValue = PutValue((double)o);
								}
								else
								{
									throw new CompileException(Error.NumberError, mBlock, mCurrent);
								}
							}
						}
						else
						{
							throw new CompileException(Error.NumberError, mBlock, mCurrent);
						}
						return Token.T_CONSTVAL;
					}
				}
			}
			while (retry);
			// switch(c)
			if ((((c & 0xFF00) != 0) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) == false && c != '_')
			{
				//if( isWAlpha(c) == false && c != '_' ) {
				string str = Error.InvalidChar.Replace("%1", EscapeC((char)c));
				throw new CompileException(str, mBlock, mCurrent);
			}
			int oldC = c;
			int offset = mCurrent;
			int nch = 0;
			while ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || (c >= '0' 
				&& c <= '9') || ((c & unchecked((int)(0xFF00))) != 0))
			{
				//while( isWDigit(c) || isWAlpha(c) || c == '_' || c > 0x0100 || /*c == CARRIAGE_RETURN || */c == LINE_FEED ) {
				mCurrent++;
				c = ptr[mCurrent];
				nch++;
			}
			if (nch == 0)
			{
				string str = Error.InvalidChar.Replace("%1", EscapeC((char)oldC));
				throw new CompileException(str, mBlock, mCurrent);
			}
			string str_1 = new string(ptr, offset, nch);
			int retnum;
			if (mBareWord)
			{
				retnum = -1;
				mBareWord = false;
			}
			else
			{
				retnum = ReservedWordToken.GetToken(str_1);
			}
			if (retnum == -1)
			{
				// not a reserved word
				mValue = PutValue(str_1);
				return Token.T_SYMBOL;
			}
			switch (retnum)
			{
				case Token.T_FALSE:
				{
					mValue = PutValue(Sharpen.Extensions.ValueOf(0));
					return Token.T_CONSTVAL;
				}

				case Token.T_NULL:
				{
					mValue = PutValue(new VariantClosure(null));
					return Token.T_CONSTVAL;
				}

				case Token.T_TRUE:
				{
					mValue = PutValue(Sharpen.Extensions.ValueOf(1));
					return Token.T_CONSTVAL;
				}

				case Token.T_NAN:
				{
					mValue = PutValue((double.NaN));
					return Token.T_CONSTVAL;
				}

				case Token.T_INFINITY:
				{
					mValue = PutValue((double.PositiveInfinity));
					return Token.T_CONSTVAL;
				}
			}
			return retnum;
		}

		public int GetValue()
		{
			return mValue;
		}

		/// <exception cref="CompileException"></exception>
		public int GetNext()
		{
			if (mIsFirst)
			{
				mIsFirst = false;
				if (mIsExprMode && mResultNeeded)
				{
					mValue = 0;
					return Token.T_RETURN;
				}
			}
			int n = 0;
			mValue = 0;
			do
			{
				if (mRetValDeque.IsEmpty() != true)
				{
					long pair = mRetValDeque.Pop_front();
					mValue = (int)((long)(((ulong)pair) >> 32));
					mPrevToken = (int)(pair & unchecked((long)(0xffffffffL)));
					return mPrevToken;
				}
				try
				{
					if (mEmbeddableExpressionDataStack.Count == 0)
					{
						//skipSpace();
						char c = mText[mCurrent];
						while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
						{
							mCurrent++;
							c = mText[mCurrent];
						}
						n = GetToken();
						if (CompileState.mEnableDicFuncQuickHack)
						{
							// dicfunc quick-hack
							if (mDicFunc)
							{
								if (n == Token.T_PERCENT)
								{
									// push "function { return %"
									mRetValDeque.Push_back(Token.T_FUNCTION);
									// value index は 0 なので无视
									mRetValDeque.Push_back(Token.T_LBRACE);
									mRetValDeque.Push_back(Token.T_RETURN);
									mRetValDeque.Push_back(Token.T_PERCENT);
									n = -1;
								}
								else
								{
									if (n == Token.T_LBRACKET && mPrevToken != Token.T_PERCENT)
									{
										// push "function { return ["
										mRetValDeque.Push_back(Token.T_FUNCTION);
										// value index は 0 なので无视
										mRetValDeque.Push_back(Token.T_LBRACE);
										mRetValDeque.Push_back(Token.T_RETURN);
										mRetValDeque.Push_back(Token.T_LBRACKET);
										n = -1;
									}
									else
									{
										if (n == Token.T_RBRACKET)
										{
											// push "] ; } ( )"
											mRetValDeque.Push_back(Token.T_RBRACKET);
											// value index は 0 なので无视
											mRetValDeque.Push_back(Token.T_SEMICOLON);
											mRetValDeque.Push_back(Token.T_RBRACE);
											mRetValDeque.Push_back(Token.T_LPARENTHESIS);
											mRetValDeque.Push_back(Token.T_RPARENTHESIS);
											n = -1;
										}
									}
								}
							}
						}
					}
					else
					{
						// embeddable expression mode
						EmbeddableExpressionData data = mEmbeddableExpressionDataStack[mEmbeddableExpressionDataStack
							.Count - 1];
						switch (data.mState)
						{
							case EmbeddableExpressionData.START:
							{
								mRetValDeque.Push_back(Token.T_LPARENTHESIS);
								// value index は 0 なので无视
								n = -1;
								data.mState = EmbeddableExpressionData.NEXT_IS_STRING_LITERAL;
								break;
							}

							case EmbeddableExpressionData.NEXT_IS_STRING_LITERAL:
							{
								string str = ReadString(data.mDelimiter, true);
								int res = mStringStatus;
								if (mStringStatus == DELIMITER)
								{
									// embeddable expression mode ended
									if (str.Length > 0)
									{
										if (data.mNeedPlus)
										{
											mRetValDeque.Push_back(Token.T_PLUS);
										}
									}
									// value index は 0 なので无视
									if (str.Length > 0 || data.mNeedPlus == false)
									{
										int v = PutValue(str);
										mRetValDeque.Push_back(Token.T_CONSTVAL | (v << 32));
									}
									mRetValDeque.Push_back(Token.T_RPARENTHESIS);
									// value index は 0 なので无视
									mEmbeddableExpressionDataStack.Remove(mEmbeddableExpressionDataStack.Count - 1);
									n = -1;
									break;
								}
								else
								{
									// c is next to ampersand mark or '${'
									if (str.Length > 0)
									{
										if (data.mNeedPlus)
										{
											mRetValDeque.Push_back(Token.T_PLUS);
										}
										// value index は 0 なので无视
										int v = PutValue(str);
										mRetValDeque.Push_back(Token.T_CONSTVAL | (v << 32));
										data.mNeedPlus = true;
									}
									if (data.mNeedPlus == true)
									{
										mRetValDeque.Push_back(Token.T_PLUS);
									}
									// value index は 0 なので无视
									mRetValDeque.Push_back(Token.T_STRING);
									// value index は 0 なので无视
									mRetValDeque.Push_back(Token.T_LPARENTHESIS);
									data.mState = EmbeddableExpressionData.NEXT_IS_EXPRESSION;
									if (res == AMPERSAND)
									{
										data.mWaitingToken = Token.T_SEMICOLON;
									}
									else
									{
										if (res == DOLLAR)
										{
											data.mWaitingToken = Token.T_RBRACE;
											mNestLevel++;
										}
									}
									n = -1;
									break;
								}
							}

							case EmbeddableExpressionData.NEXT_IS_EXPRESSION:
							{
								//skipSpace();
								char c = mText[mCurrent];
								while (c != 0 && c <= SPACE && (c == CR || c == LF || c == TAB || c == SPACE))
								{
									mCurrent++;
									c = mText[mCurrent];
								}
								n = GetToken();
								if (n == data.mWaitingToken && mNestLevel == data.mWaitingNestLevel)
								{
									// end of embeddable expression mode
									mRetValDeque.Push_back(Token.T_RPARENTHESIS);
									// value index は 0 なので无视
									data.mNeedPlus = true;
									data.mState = EmbeddableExpressionData.NEXT_IS_STRING_LITERAL;
									n = -1;
								}
								break;
							}
						}
					}
					if (n == 0)
					{
						if (mIfLevel != 0)
						{
							throw new CompileException(Error.PPError, mBlock, mCurrent);
						}
					}
				}
				catch (CompileException e)
				{
					Sharpen.Runtime.PrintStackTrace(e);
					mBlock.Error(e.Message);
					return 0;
				}
			}
			while (n < 0);
			mPrevToken = n;
			return n;
		}

		public virtual int GetCurrentPosition()
		{
			return mCurrent;
		}
	}
}

/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using System.Text;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class Lexer : LexBase
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

		private const int LINE_FEED = 10;

		private Queue<TokenPair> mRetValDeque;

		private AList<EmbeddableExpressionData> mEmbeddableExpressionDataStack;

		private AList<object> mValues;

		//private static final String TAG = "Lexer";
		//private static final boolean LOGD = false;
		// dicfunc quick-hack
		//private static final int CARRIAGE_RETURN = 13;
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

		private static Dictionary<string, int> mReservedWordHash;

		private static void InitReservedWordsHashTable()
		{
			if (mReservedWordHash == null)
			{
				mReservedWordHash = new Dictionary<string, int>();
			}
			if (mReservedWordHash.Count > 0)
			{
				return;
			}
			mReservedWordHash.Put("break", Token.T_BREAK);
			mReservedWordHash.Put("continue", Token.T_CONTINUE);
			mReservedWordHash.Put("const", Token.T_CONST);
			mReservedWordHash.Put("catch", Token.T_CATCH);
			mReservedWordHash.Put("class", Token.T_CLASS);
			mReservedWordHash.Put("case", Token.T_CASE);
			mReservedWordHash.Put("debugger", Token.T_DEBUGGER);
			mReservedWordHash.Put("default", Token.T_DEFAULT);
			mReservedWordHash.Put("delete", Token.T_DELETE);
			mReservedWordHash.Put("do", Token.T_DO);
			mReservedWordHash.Put("extends", Token.T_EXTENDS);
			mReservedWordHash.Put("export", Token.T_EXPORT);
			mReservedWordHash.Put("enum", Token.T_ENUM);
			mReservedWordHash.Put("else", Token.T_ELSE);
			mReservedWordHash.Put("function", Token.T_FUNCTION);
			mReservedWordHash.Put("finally", Token.T_FINALLY);
			mReservedWordHash.Put("false", Token.T_FALSE);
			mReservedWordHash.Put("for", Token.T_FOR);
			mReservedWordHash.Put("global", Token.T_GLOBAL);
			mReservedWordHash.Put("getter", Token.T_GETTER);
			mReservedWordHash.Put("goto", Token.T_GOTO);
			mReservedWordHash.Put("incontextof", Token.T_INCONTEXTOF);
			mReservedWordHash.Put("invalidate", Token.T_INVALIDATE);
			mReservedWordHash.Put("instanceof", Token.T_INSTANCEOF);
			mReservedWordHash.Put("isvalid", Token.T_ISVALID);
			mReservedWordHash.Put("import", Token.T_IMPORT);
			mReservedWordHash.Put("int", Token.T_INT);
			mReservedWordHash.Put("in", Token.T_IN);
			mReservedWordHash.Put("if", Token.T_IF);
			mReservedWordHash.Put("null", Token.T_NULL);
			mReservedWordHash.Put("new", Token.T_NEW);
			mReservedWordHash.Put("octet", Token.T_OCTET);
			mReservedWordHash.Put("protected", Token.T_PROTECTED);
			mReservedWordHash.Put("property", Token.T_PROPERTY);
			mReservedWordHash.Put("private", Token.T_PRIVATE);
			mReservedWordHash.Put("public", Token.T_PUBLIC);
			mReservedWordHash.Put("return", Token.T_RETURN);
			mReservedWordHash.Put("real", Token.T_REAL);
			mReservedWordHash.Put("synchronized", Token.T_SYNCHRONIZED);
			mReservedWordHash.Put("switch", Token.T_SWITCH);
			mReservedWordHash.Put("static", Token.T_STATIC);
			mReservedWordHash.Put("setter", Token.T_SETTER);
			mReservedWordHash.Put("string", Token.T_STRING);
			mReservedWordHash.Put("super", Token.T_SUPER);
			mReservedWordHash.Put("typeof", Token.T_TYPEOF);
			mReservedWordHash.Put("throw", Token.T_THROW);
			mReservedWordHash.Put("this", Token.T_THIS);
			mReservedWordHash.Put("true", Token.T_TRUE);
			mReservedWordHash.Put("try", Token.T_TRY);
			mReservedWordHash.Put("void", Token.T_VOID);
			mReservedWordHash.Put("var", Token.T_VAR);
			mReservedWordHash.Put("while", Token.T_WHILE);
			mReservedWordHash.Put("NaN", Token.T_NAN);
			mReservedWordHash.Put("Infinity", Token.T_INFINITY);
			mReservedWordHash.Put("with", Token.T_WITH);
		}

		public Lexer(Compiler block, string script, bool isexpr, bool resultneeded) : base
			()
		{
			InitReservedWordsHashTable();
			mRetValDeque = new Queue<TokenPair>();
			mEmbeddableExpressionDataStack = new AList<EmbeddableExpressionData>();
			mValues = new AList<object>();
			mBlock = block;
			mIsExprMode = isexpr;
			mResultNeeded = resultneeded;
			mPrevToken = -1;
			if (mIsExprMode)
			{
				mStream = new StringStream(script + ";");
			}
			else
			{
				if (script.StartsWith("#!") == true)
				{
					// #! を // に置换
					mStream = new StringStream("//" + Sharpen.Runtime.Substring(script, 2));
				}
				else
				{
					mStream = new StringStream(script);
				}
			}
			if (CompileState.mEnableDicFuncQuickHack)
			{
				//----- dicfunc quick-hack
				mDicFunc = false;
				if (mIsExprMode && (script.StartsWith("[") == true || script.StartsWith("%[") == 
					true))
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
			StringBuilder str = new StringBuilder(BUFFER_CAPACITY);
			StringBuilder flag = new StringBuilder(BUFFER_CAPACITY);
			int c = mStream.Next();
			while (c != -1)
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
						c = mStream.Next();
						if (c == -1)
						{
							ok = true;
							break;
						}
						//int flagMask = 0;
						flag.Delete(0, flag.Length);
						while (c >= 'a' && c <= 'z')
						{
							flag.Append((char)c);
							c = mStream.Next();
							if (c == -1)
							{
								break;
							}
						}
						mStream.UngetC();
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
				c = mStream.Next();
			}
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
			ByteBuffer ret = ByteBuffer.AllocateDirect(count);
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
			bool leading = true;
			byte cur = 0;
			int count = mStream.CountOctetTail() / 2 + 1;
			ByteBuffer buffer = ByteBuffer.AllocateDirect(count);
			int c = mStream.PeekC();
			while (c != -1)
			{
				if (mStream.SkipComment() == StringStream.ENDED)
				{
					throw new CompileException(Error.StringParseError);
				}
				c = mStream.GetC();
				int next = mStream.PeekC();
				if (c == '%' && next == '>')
				{
					mStream.IncOffset();
					if (!leading)
					{
						buffer.Put(cur);
					}
					return CompactByteBuffer(buffer);
				}
				int num = GetHexNum(c);
				if (num != -1)
				{
					if (leading)
					{
						cur = unchecked((byte)num);
						leading = false;
					}
					else
					{
						cur <<= 4;
						cur += (byte)num;
						buffer.Put(cur);
						leading = true;
					}
				}
				if (leading == false && c == ',')
				{
					buffer.Put(cur);
					leading = true;
				}
			}
			throw new CompileException(Error.StringParseError);
		}

		/// <exception cref="CompileException"></exception>
		private string ParseString(int delimiter)
		{
			return mStream.ReadString(delimiter, false);
		}

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
			int offset = mStream.GetOffset();
			int c = mStream.GetC();
			if (c == 's' && mStream.EqualString("et") == true)
			{
				// set statemenet
				mBlock.NotifyUsingPreProcessror();
				mStream.SkipSpace();
				c = mStream.GetC();
				if (c == -1 || c != '(')
				{
					throw new CompileException(Error.PPError);
				}
				c = mStream.Next();
				StringBuilder script = new StringBuilder(BUFFER_CAPACITY);
				int plevel = 0;
				while (c != -1 && (plevel != 0 || c != ')'))
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
					c = mStream.GetC();
				}
				if (c != -1)
				{
					mStream.UngetC();
				}
				ParsePPExpression(script.ToString());
				if (mStream.IsEOF())
				{
					return StringStream.ENDED;
				}
				return StringStream.CONTINUE;
			}
			if (c == 'i' && mStream.EqualString("f") == true)
			{
				// if statement
				mBlock.NotifyUsingPreProcessror();
				mStream.SkipSpace();
				c = mStream.GetC();
				if (c == -1 || c != '(')
				{
					throw new CompileException(Error.PPError);
				}
				c = mStream.Next();
				StringBuilder script = new StringBuilder(BUFFER_CAPACITY);
				int plevel = 0;
				while (c != -1 && (plevel != 0 || c != ')'))
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
					c = mStream.GetC();
				}
				//			if( c != -1 ) mStream.ungetC();
				int ret = ParsePPExpression(script.ToString());
				if (ret == 0)
				{
					return SkipUntilEndif();
				}
				mIfLevel++;
				if (mStream.IsEOF())
				{
					return StringStream.ENDED;
				}
				return StringStream.CONTINUE;
			}
			if (c == 'e' && mStream.EqualString("ndif") == true)
			{
				// endif statement
				mIfLevel--;
				if (mIfLevel < 0)
				{
					throw new CompileException(Error.PPError);
				}
				if (mStream.IsEOF())
				{
					return StringStream.ENDED;
				}
				return StringStream.CONTINUE;
			}
			mStream.SetOffset(offset);
			return StringStream.NOT_COMMENT;
		}

		/// <exception cref="CompileException"></exception>
		private int SkipUntilEndif()
		{
			int exl = mIfLevel;
			mIfLevel++;
			int c = mStream.GetC();
			while (true)
			{
				if (c == '/')
				{
					// comment
					mStream.UngetC();
					int ret = mStream.SkipComment();
					switch (ret)
					{
						case StringStream.ENDED:
						{
							throw new CompileException(Error.PPError);
						}

						case StringStream.CONTINUE:
						{
							break;
						}

						case StringStream.NOT_COMMENT:
						{
							c = mStream.Next();
							if (mStream.IsEOF())
							{
								throw new CompileException(Error.PPError);
							}
							break;
						}
					}
				}
				else
				{
					if (c == '@')
					{
						//c = mStream.getC();
						bool skipp = false;
						if (mStream.EqualString("if"))
						{
							mIfLevel++;
							skipp = true;
						}
						else
						{
							if (mStream.EqualString("set"))
							{
								skipp = true;
							}
							else
							{
								if (mStream.EqualString("endif"))
								{
									mIfLevel--;
									if (mIfLevel == exl)
									{
										// skip ended
										mStream.SkipSpace();
										if (mStream.IsEOF())
										{
											return StringStream.ENDED;
										}
										return StringStream.CONTINUE;
									}
								}
								else
								{
									c = mStream.GetC();
								}
							}
						}
						if (skipp)
						{
							mStream.SkipSpace();
							if (mStream.IsEOF())
							{
								throw new CompileException(Error.PPError);
							}
							c = mStream.GetC();
							if (c != '(')
							{
								throw new CompileException(Error.PPError);
							}
							c = mStream.Next();
							int plevel = 0;
							while (c != -1 && (plevel > 0 || c != ')'))
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
								c = mStream.Next();
							}
							if (c == -1)
							{
								throw new CompileException(Error.PPError);
							}
							mStream.SkipSpace();
							if (mStream.IsEOF())
							{
								throw new CompileException(Error.PPError);
							}
						}
					}
					else
					{
						c = mStream.GetC();
						if (c == -1)
						{
							throw new CompileException(Error.PPError);
						}
					}
				}
			}
		}

		private int GetHexNum(int c)
		{
			if (c >= 'a' && c <= 'f')
			{
				return c - 'a' + 10;
			}
			if (c >= 'A' && c <= 'F')
			{
				return c - 'A' + 10;
			}
			if (c >= '0' && c <= '9')
			{
				return c - '0';
			}
			return -1;
		}

		private int PutValue(object val)
		{
			mValues.AddItem(val);
			return mValues.Count - 1;
		}

		/// <exception cref="CompileException"></exception>
		private int GetToken()
		{
			if (mStream.IsEOF() == true)
			{
				return 0;
			}
			if (mRegularExpression == true)
			{
				mRegularExpression = false;
				mStream.SetOffset(mPrevPos);
				mStream.SkipSpace();
				mStream.Next();
				// 最初の'/'を读み飞ばし
				string pattern = ParseRegExp();
				mValue = PutValue(pattern);
				return Token.T_REGEXP;
			}
			int c;
			bool retry;
			do
			{
				retry = false;
				mStream.SkipSpace();
				mPrevPos = mStream.GetOffset();
				c = mStream.GetC();
				switch (c)
				{
					case 0:
					case -1:
					{
						return 0;
					}

					case '>':
					{
						c = mStream.GetC();
						if (c == '>')
						{
							// >>
							c = mStream.GetC();
							if (c == '>')
							{
								// >>>
								c = mStream.GetC();
								if (c == '=')
								{
									// >>>=
									return Token.T_RBITSHIFTEQUAL;
								}
								else
								{
									mStream.UngetC();
									return Token.T_RBITSHIFT;
								}
							}
							else
							{
								if (c == '=')
								{
									// >>=
									return Token.T_RARITHSHIFTEQUAL;
								}
								else
								{
									// >>
									mStream.UngetC();
									return Token.T_RARITHSHIFT;
								}
							}
						}
						else
						{
							if (c == '=')
							{
								// >=
								return Token.T_GTOREQUAL;
							}
							else
							{
								mStream.UngetC();
								return Token.T_GT;
							}
						}
					}

					case '<':
					{
						c = mStream.GetC();
						if (c == '<')
						{
							// <<
							c = mStream.GetC();
							if (c == '=')
							{
								// <<=
								return Token.T_LARITHSHIFTEQUAL;
							}
							else
							{
								// <<
								mStream.UngetC();
								return Token.T_LARITHSHIFT;
							}
						}
						else
						{
							if (c == '-')
							{
								// <-
								c = mStream.GetC();
								if (c == '>')
								{
									// <->
									return Token.T_SWAP;
								}
								else
								{
									// <
									mStream.UngetC();
									mStream.UngetC();
									return Token.T_LT;
								}
							}
							else
							{
								if (c == '=')
								{
									// <=
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
										mStream.UngetC();
										return Token.T_LT;
									}
								}
							}
						}
					}

					case '=':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							// ===
							c = mStream.GetC();
							if (c == '=')
							{
								return Token.T_DISCEQUAL;
							}
							else
							{
								// ==
								mStream.UngetC();
								return Token.T_EQUALEQUAL;
							}
						}
						else
						{
							if (c == '>')
							{
								// =>
								return Token.T_COMMA;
							}
							else
							{
								// =
								mStream.UngetC();
								return Token.T_EQUAL;
							}
						}
					}

					case '!':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							c = mStream.GetC();
							if (c == '=')
							{
								// !==
								return Token.T_DISCNOTEQUAL;
							}
							else
							{
								// !=
								mStream.UngetC();
								return Token.T_NOTEQUAL;
							}
						}
						else
						{
							// !
							mStream.UngetC();
							return Token.T_EXCRAMATION;
						}
					}

					case '&':
					{
						c = mStream.GetC();
						if (c == '&')
						{
							c = mStream.GetC();
							if (c == '=')
							{
								// &&=
								return Token.T_LOGICALANDEQUAL;
							}
							else
							{
								// &&
								mStream.UngetC();
								return Token.T_LOGICALAND;
							}
						}
						else
						{
							if (c == '=')
							{
								// &=
								return Token.T_AMPERSANDEQUAL;
							}
							else
							{
								// &
								mStream.UngetC();
								return Token.T_AMPERSAND;
							}
						}
					}

					case '|':
					{
						c = mStream.GetC();
						if (c == '|')
						{
							c = mStream.GetC();
							if (c == '=')
							{
								// ||=
								return Token.T_LOGICALOREQUAL;
							}
							else
							{
								// ||
								mStream.UngetC();
								return Token.T_LOGICALOR;
							}
						}
						else
						{
							if (c == '=')
							{
								// |=
								return Token.T_VERTLINEEQUAL;
							}
							else
							{
								// |
								mStream.UngetC();
								return Token.T_VERTLINE;
							}
						}
					}

					case '.':
					{
						c = mStream.GetC();
						if (c >= '0' && c <= '9')
						{
							// number
							mStream.UngetC();
							mStream.UngetC();
							object o = ParseNumber();
							if (o != null)
							{
								if (((Number)o).IsInt())
								{
									mValue = PutValue((int)o);
								}
								else
								{
									if (((Number)o).IsDouble())
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
								c = mStream.GetC();
								if (c == '.')
								{
									// ...
									return Token.T_OMIT;
								}
								else
								{
									// .
									mStream.UngetC();
									mStream.UngetC();
									return Token.T_DOT;
								}
							}
							else
							{
								// .
								mStream.UngetC();
								return Token.T_DOT;
							}
						}
					}

					case '+':
					{
						c = mStream.GetC();
						if (c == '+')
						{
							// ++
							return Token.T_INCREMENT;
						}
						else
						{
							if (c == '=')
							{
								// +=
								return Token.T_PLUSEQUAL;
							}
							else
							{
								// +
								mStream.UngetC();
								return Token.T_PLUS;
							}
						}
					}

					case '-':
					{
						c = mStream.GetC();
						if (c == '-')
						{
							// --
							return Token.T_DECREMENT;
						}
						else
						{
							if (c == '=')
							{
								return Token.T_MINUSEQUAL;
							}
							else
							{
								// -=
								// -
								mStream.UngetC();
								return Token.T_MINUS;
							}
						}
					}

					case '*':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							// *=
							return Token.T_ASTERISKEQUAL;
						}
						else
						{
							// *
							mStream.UngetC();
							return Token.T_ASTERISK;
						}
					}

					case '/':
					{
						c = mStream.GetC();
						if (c == '/' || c == '*')
						{
							mStream.UngetC();
							mStream.UngetC();
							int comment = mStream.SkipComment();
							if (comment == StringStream.CONTINUE)
							{
								retry = true;
								break;
							}
							else
							{
								if (comment == StringStream.ENDED)
								{
									return 0;
								}
							}
						}
						if (c == '=')
						{
							// /=
							return Token.T_SLASHEQUAL;
						}
						else
						{
							// /
							mStream.UngetC();
							return Token.T_SLASH;
						}
					}

					case '\\':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							// \=
							return Token.T_BACKSLASHEQUAL;
						}
						else
						{
							// \
							mStream.UngetC();
							return Token.T_BACKSLASH;
						}
					}

					case '%':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							// %=
							return Token.T_PERCENTEQUAL;
						}
						else
						{
							// %
							mStream.UngetC();
							return Token.T_PERCENT;
						}
					}

					case '^':
					{
						c = mStream.GetC();
						if (c == '=')
						{
							// ^=
							return Token.T_CHEVRONEQUAL;
						}
						else
						{
							// ^
							mStream.UngetC();
							return Token.T_CHEVRON;
						}
					}

					case '[':
					{
						mNestLevel++;
						return Token.T_LBRACKET;
					}

					case ']':
					{
						mNestLevel--;
						return Token.T_RBRACKET;
					}

					case '(':
					{
						mNestLevel++;
						return Token.T_LPARENTHESIS;
					}

					case ')':
					{
						mNestLevel--;
						return Token.T_RPARENTHESIS;
					}

					case '~':
					{
						return Token.T_TILDE;
					}

					case '?':
					{
						return Token.T_QUESTION;
					}

					case ':':
					{
						return Token.T_COLON;
					}

					case ',':
					{
						return Token.T_COMMA;
					}

					case ';':
					{
						return Token.T_SEMICOLON;
					}

					case '{':
					{
						mNestLevel++;
						return Token.T_LBRACE;
					}

					case '}':
					{
						mNestLevel--;
						return Token.T_RBRACE;
					}

					case '#':
					{
						return Token.T_SHARP;
					}

					case '$':
					{
						return Token.T_DOLLAR;
					}

					case '\'':
					case '\"':
					{
						// literal string
						string str = ParseString(c);
						mValue = PutValue(str);
						return Token.T_CONSTVAL;
					}

					case '@':
					{
						// embeddable expression in string (such as @"this can be embeddable like &variable;")
						int offset = mStream.GetOffset();
						mStream.SkipSpace();
						c = mStream.Next();
						if (c == '\'' || c == '\"')
						{
							EmbeddableExpressionData data = new EmbeddableExpressionData();
							data.mState = EmbeddableExpressionData.START;
							data.mWaitingNestLevel = mNestLevel;
							data.mDelimiter = c;
							data.mNeedPlus = false;
							if (mStream.IsEOF())
							{
								return 0;
							}
							mEmbeddableExpressionDataStack.AddItem(data);
							return -1;
						}
						else
						{
							mStream.SetOffset(offset);
						}
						switch (ProcessPPStatement())
						{
							case StringStream.CONTINUE:
							{
								// possible pre-procesor statements
								retry = true;
								break;
							}

							case StringStream.ENDED:
							{
								return 0;
							}

							case StringStream.NOT_COMMENT:
							{
								mStream.SetOffset(offset);
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
						mStream.UngetC();
						object o = ParseNumber();
						if (o != null)
						{
							if (o is int)
							{
								mValue = PutValue((int)o);
							}
							else
							{
								if (o is double)
								{
									mValue = PutValue((double)o);
								}
								else
								{
									throw new CompileException(Error.NumberError);
								}
							}
						}
						else
						{
							throw new CompileException(Error.NumberError);
						}
						return Token.T_CONSTVAL;
					}
				}
			}
			while (retry);
			// switch(c)
			if (IsWAlpha(c) == false && c != '_')
			{
				string str = Error.InvalidChar.Replace("%1", EscapeC((char)c));
				throw new CompileException(str);
			}
			int oldC = c;
			int offset_1 = mStream.GetOffset() - 1;
			int nch = 0;
			while (IsWDigit(c) || IsWAlpha(c) || c == '_' || c > unchecked((int)(0x0100)) || 
				c == LINE_FEED)
			{
				c = mStream.GetC();
				nch++;
			}
			if (nch == 0)
			{
                string str = Error.InvalidChar.Replace("%1", EscapeC((char)oldC));
				throw new CompileException(str);
			}
			else
			{
				mStream.UngetC();
			}
			string str_1 = mStream.Substring(offset_1, offset_1 + nch);
			int strLen = str_1.Length;
			StringBuilder symStr = new StringBuilder(BUFFER_CAPACITY);
			for (int i = 0; i < strLen; i++)
			{
				char t = str_1[i];
				if (t != LINE_FEED)
				{
					symStr.Append(t);
				}
			}
			str_1 = symStr.ToString();
			int retnum;
			if (mBareWord)
			{
				retnum = -1;
			}
			else
			{
				int id = mReservedWordHash.Get(str_1);
				if (id != null)
				{
					retnum = id;
				}
				else
				{
					retnum = -1;
				}
			}
			mBareWord = false;
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

		public virtual int GetValue()
		{
			return mValue;
		}

		/// <exception cref="CompileException"></exception>
		public virtual int GetNext()
		{
			//if( LOGD ) Log.v(TAG,"getNext");
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
				if (mRetValDeque.Count > 0)
				{
					TokenPair pair = mRetValDeque.Dequeue();
					mValue = pair.value;
					mPrevToken = pair.token;
					return pair.token;
				}
				try
				{
					if (mEmbeddableExpressionDataStack.Count == 0)
					{
						mStream.SkipSpace();
						n = GetToken();
						if (CompileState.mEnableDicFuncQuickHack)
						{
							// dicfunc quick-hack
							if (mDicFunc)
							{
								if (n == Token.T_PERCENT)
								{
									// push "function { return %"
									mRetValDeque.Enqueue(new TokenPair(Token.T_FUNCTION, 0));
                                    mRetValDeque.Enqueue(new TokenPair(Token.T_LBRACE, 0));
                                    mRetValDeque.Enqueue(new TokenPair(Token.T_RETURN, 0));
                                    mRetValDeque.Enqueue(new TokenPair(Token.T_PERCENT, 0));
									n = -1;
								}
								else
								{
									if (n == Token.T_LBRACKET && mPrevToken != Token.T_PERCENT)
									{
										// push "function { return ["
                                        mRetValDeque.Enqueue(new TokenPair(Token.T_FUNCTION, 0));
                                        mRetValDeque.Enqueue(new TokenPair(Token.T_LBRACE, 0));
                                        mRetValDeque.Enqueue(new TokenPair(Token.T_RETURN, 0));
                                        mRetValDeque.Enqueue(new TokenPair(Token.T_LBRACKET, 0));
										n = -1;
									}
									else
									{
										if (n == Token.T_RBRACKET)
										{
											// push "] ; } ( )"
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_RBRACKET, 0));
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_SEMICOLON, 0));
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_RBRACE, 0));
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_LPARENTHESIS, 0));
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_RPARENTHESIS, 0));
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
								mRetValDeque.Enqueue(new TokenPair(Token.T_LPARENTHESIS, 0));
								n = -1;
								data.mState = EmbeddableExpressionData.NEXT_IS_STRING_LITERAL;
								break;
							}

							case EmbeddableExpressionData.NEXT_IS_STRING_LITERAL:
							{
								string str = mStream.ReadString(data.mDelimiter, true);
								int res = mStream.GetStringStatus();
								if (res == StringStream.DELIMITER)
								{
									if (str.Length > 0)
									{
										if (data.mNeedPlus)
										{
                                            mRetValDeque.Enqueue(new TokenPair(Token.T_PLUS, 0));
										}
									}
									if (str.Length > 0 || data.mNeedPlus == false)
									{
										int v = PutValue(str);
                                        mRetValDeque.Enqueue(new TokenPair(Token.T_CONSTVAL, v));
									}
                                    mRetValDeque.Enqueue(new TokenPair(Token.T_RPARENTHESIS, 0));
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
											mRetValDeque.Enqueue(new TokenPair(Token.T_PLUS, 0));
										}
										int v = PutValue(str);
										mRetValDeque.Enqueue(new TokenPair(Token.T_CONSTVAL, v));
										data.mNeedPlus = true;
									}
									if (data.mNeedPlus == true)
									{
										mRetValDeque.Enqueue(new TokenPair(Token.T_PLUS, 0));
									}
									mRetValDeque.Enqueue(new TokenPair(Token.T_STRING, 0));
									mRetValDeque.Enqueue(new TokenPair(Token.T_LPARENTHESIS, 0));
									data.mState = EmbeddableExpressionData.NEXT_IS_EXPRESSION;
									if (res == StringStream.AMPERSAND)
									{
										data.mWaitingToken = Token.T_SEMICOLON;
									}
									else
									{
										if (res == StringStream.DOLLAR)
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
								mStream.SkipSpace();
								n = GetToken();
								if (n == data.mWaitingToken && mNestLevel == data.mWaitingNestLevel)
								{
									// end of embeddable expression mode
									mRetValDeque.Enqueue(new TokenPair(Token.T_RPARENTHESIS, 0));
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
							throw new CompileException(Error.PPError);
						}
					}
				}
				catch (CompileException e)
				{
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
			int offset = mStream.GetOffset();
			return offset > 0 ? offset - 1 : 0;
		}
		//public int lineToSrcPos(int line) {
		//	return mStream.getLineToSrcPos(line);
		//}
	}
}

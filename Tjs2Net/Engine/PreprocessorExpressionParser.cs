/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class PreprocessorExpressionParser : LexBase
	{
		private const int BUFFER_CAPACITY = 1024;

		private AList<string> mIDs;

		private int mValue;

		private int mUnlex;

		private int mUnValue;

		private bool mIsUnlex;

		private Tjs mTJS;

		public PreprocessorExpressionParser(Tjs tjs, string script) : base(script)
		{
			//	private int mResult;
			mIDs = new AList<string>();
			mTJS = tjs;
		}

		//mIsUnlex = false;
		private int GetNext()
		{
			if (mIsUnlex)
			{
				mIsUnlex = false;
				mValue = mUnValue;
				return mUnlex;
			}
			mStream.SkipSpace();
			int c = mStream.GetC();
			switch (c)
			{
				case '(':
				{
					return Token.PT_LPARENTHESIS;
				}

				case ')':
				{
					return Token.PT_RPARENTHESIS;
				}

				case ',':
				{
					return Token.PT_COMMA;
				}

				case '=':
				{
					c = mStream.GetC();
					if (c == '=')
					{
						return Token.PT_EQUALEQUAL;
					}
					else
					{
						mStream.UngetC();
						return Token.PT_EQUAL;
					}
				}

				case '!':
				{
					c = mStream.GetC();
					if (c == '=')
					{
						return Token.PT_NOTEQUAL;
					}
					else
					{
						mStream.UngetC();
						return Token.PT_EXCLAMATION;
					}
				}

				case '|':
				{
					c = mStream.GetC();
					if (c == '|')
					{
						return Token.PT_LOGICALOR;
					}
					else
					{
						mStream.UngetC();
						return Token.PT_VERTLINE;
					}
				}

				case '&':
				{
					c = mStream.GetC();
				    if (c == '&')
				        return Token.PT_LOGICALAND;
				    mStream.UngetC();
				    return Token.PT_AMPERSAND;
				}
				case '^':
					return Token.PT_CHEVRON;
				case '+':
					return Token.PT_PLUS;
				case '-':
					return Token.PT_MINUS;
				case '*':
					return Token.PT_ASTERISK;
				case '/':
					return Token.PT_SLASH;
				case '%':
					return Token.PT_PERCENT;

				case '<':
				{
					c = mStream.GetC();
					if (c == '=')
						return Token.PT_LTOREQUAL;
					mStream.UngetC();
					return Token.PT_LT;
				}

				case '>':
				{
					c = mStream.GetC();
					if (c == '=')
					{
						return Token.PT_GTOREQUAL;
					}
					else
					{
						mStream.UngetC();
						return Token.PT_GT;
					}
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
					// number
					mStream.UngetC();
					Number num = ParseNumber();
					if (num == null)
						return Token.PT_ERROR;
					mValue = num;
					return Token.PT_NUM;
				}

				case -1:
					return 0;
			}

			if (IsWAlpha(c) == false && c != '_')
				return Token.PT_ERROR;

			var str = new StringBuilder(BUFFER_CAPACITY);
			while ((IsWAlpha(c) || IsWDigit(c) || c == '_') && c != -1)
			{
				str.Append((char)c);
				c = mStream.GetC();
			}
			mStream.UngetC();
			mIDs.AddItem(str.ToString());
			mValue = mIDs.Count - 1;
			return Token.PT_SYMBOL;
		}

		private string GetString(int index) => mIDs[index];

	    private void UnLex(int lex, int value)
		{
			mUnlex = lex;
			mUnValue = value;
			mIsUnlex = true;
		}

		// 单项演算子と括弧
		/// <exception cref="CompileException"></exception>
		private int Expr1()
		{
			int let = GetNext();
			int result = 0;
			int flag = 1;
			bool neg = false;
			switch (let)
			{
				case Token.PT_EXCLAMATION:
				{
					// !
					neg = true;
					let = GetNext();
					break;
				}

				case Token.PT_PLUS:
				{
					// +
					let = GetNext();
					break;
				}

				case Token.PT_MINUS:
				{
					// -
					flag = -1;
					let = GetNext();
					break;
				}
			}
			if (let == Token.PT_NUM)
			{
				result = mValue * flag;
				if (neg)
					result = result != 0 ? 0 : 1;
			}
			else
			{
				if (let == Token.PT_SYMBOL)
				{
					int tmp = mValue;
					let = GetNext();
					UnLex(let, mValue);
					if (let == Token.PT_EQUAL)
					{
						// 代入规则
						result = tmp;
					}
					else
					{
						result = mTJS.GetPpValue(GetString(tmp)) * flag;
						if (neg)
						{
							result = result != 0 ? 0 : 1;
						}
					}
				}
				else
				{
					if (let == Token.PT_LPARENTHESIS)
					{
						// (？
						result = Expression() * flag;
						if (neg)
						{
							result = result != 0 ? 0 : 1;
						}
						let = GetNext();
						if (let != Token.PT_RPARENTHESIS)
						{
							// )
							throw new CompileException(Error.NotFoundPreprocessorRPARENTHESISError);
						}
					}
					else
					{
						UnLex(let, mValue);
					}
				}
			}
			return result;
		}

		// / * %
		/// <exception cref="CompileException"></exception>
		private int Expr2()
		{
			int result = Expr1();
			int let = GetNext();
			int tmp;
			while (let == Token.PT_ASTERISK || let == Token.PT_SLASH || let == Token.PT_PERCENT
				)
			{
				switch (let)
				{
					case Token.PT_ASTERISK:
					{
						// *
						result *= Expr1();
						break;
					}

					case Token.PT_SLASH:
					{
						// /
						tmp = Expr1();
						if (tmp == 0)
						{
							throw new CompileException(Error.PreprocessorZeroDiv);
						}
						result /= tmp;
						break;
					}

					case Token.PT_PERCENT:
					{
						// %
						result /= Expr1();
						break;
					}
				}
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// + -
		/// <exception cref="CompileException"></exception>
		private int Expr3()
		{
			int result = Expr2();
			int let = GetNext();
			while (let == Token.PT_PLUS || let == Token.PT_MINUS)
			{
				switch (let)
				{
					case Token.PT_PLUS:
					{
						result += Expr2();
						break;
					}

					case Token.PT_MINUS:
					{
						result -= Expr2();
                        break;
					}
				}
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// < > <= >=
		/// <exception cref="CompileException"></exception>
		private int Expr4()
		{
			int result = Expr3();
			int let = GetNext();
			while (let == Token.PT_LT || let == Token.PT_GT || let == Token.PT_LTOREQUAL || let
				 == Token.PT_GTOREQUAL)
			{
				int tmp = Expr3();
				switch (let)
				{
					case Token.PT_LT:
					{
						// <
						result = result < tmp ? 1 : 0;
						break;
					}

					case Token.PT_GT:
					{
						// >
						result = result > tmp ? 1 : 0;
						break;
					}

					case Token.PT_LTOREQUAL:
					{
						// <=
						result = result <= tmp ? 1 : 0;
						break;
					}

					case Token.PT_GTOREQUAL:
					{
						// >=
						result = result >= tmp ? 1 : 0;
						break;
					}
				}
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// != ==
		/// <exception cref="CompileException"></exception>
		private int Expr5()
		{
			int result = Expr4();
			int let = GetNext();
			while (let == Token.PT_NOTEQUAL || let == Token.PT_EQUALEQUAL)
			{
				int tmp = Expr4();
				switch (let)
				{
					case Token.PT_NOTEQUAL:
					{
						// !=
						result = result != tmp ? 1 : 0;
						break;
					}

					case Token.PT_EQUALEQUAL:
					{
						// ==
						result = result == tmp ? 1 : 0;
						break;
					}
				}
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// &
		/// <exception cref="CompileException"></exception>
		private int Expr6()
		{
			int result = Expr5();
			int let = GetNext();
			while (let == Token.PT_AMPERSAND)
			{
				result = result & Expr5();
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// ^
		/// <exception cref="CompileException"></exception>
		private int Expr7()
		{
			int result = Expr6();
			int let = GetNext();
			while (let == Token.PT_CHEVRON)
			{
				result = result ^ Expr6();
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// |
		/// <exception cref="CompileException"></exception>
		private int Expr8()
		{
			int result = Expr7();
			int let = GetNext();
			while (let == Token.PT_VERTLINE)
			{
				result = result | Expr7();
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// &&
		/// <exception cref="CompileException"></exception>
		private int Expr9()
		{
			int result = Expr8();
			int let = GetNext();
			while (let == Token.PT_LOGICALAND)
			{
				result = (result != 0) && (Expr8() != 0) ? 1 : 0;
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// ||
		/// <exception cref="CompileException"></exception>
		private int Expr10()
		{
			int result = Expr9();
			int let = GetNext();
			while (let == Token.PT_LOGICALOR)
			{
				result = (result != 0) || (Expr9() != 0) ? 1 : 0;
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// =
		/// <exception cref="CompileException"></exception>
		private int Expr11()
		{
			int result = Expr10();
			int let = GetNext();
			while (let == Token.PT_EQUAL)
			{
				int tmp = Expr10();
				mTJS.SetPpValue(GetString(result), tmp);
				result = tmp;
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		// ,
		/// <exception cref="CompileException"></exception>
		private int Expr12()
		{
			int result = Expr11();
			int let = GetNext();
			while (let == Token.PT_COMMA)
			{
				result = Expr11();
				let = GetNext();
			}
			UnLex(let, mValue);
			return result;
		}

		/// <exception cref="CompileException"></exception>
		private int Expression()
		{
			return Expr12();
		}

		/// <exception cref="CompileException"></exception>
		public virtual int Parse()
		{
			int result = Expression();
			return result;
		}
	}
}

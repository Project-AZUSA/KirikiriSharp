/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using System.Text;
using Tjs2.Engine.Translate;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class Compiler : SourceCodeAccessor
	{
		private const bool LOGD = false;

		private static readonly string TAG = "Compiler";

		private static readonly string ERROR_TAG = "Syntax error";

		private Tjs mOwner;

		private bool mUsingPreProcessor;

		private int mCompileErrorCount;

		private LexicalAnalyzer mLexicalAnalyzer;

		private InterCodeObject mTopLevelObject;

		private AList<InterCodeObject> mInterCodeObjectList;

		/// <summary>现在のコンテキスト</summary>
		private InterCodeGenerator mInterCodeGenerator;

		/// <summary>トップレベルコンテキスト</summary>
		private InterCodeGenerator mTopLevelGenerator;

		/// <summary>コンテキストスタック</summary>
		private Stack<InterCodeGenerator> mGeneratorStack;

		/// <summary>コードリスト</summary>
		private AList<InterCodeGenerator> mInterCodeGeneratorList;

		// 以下の3つは实行时には不要のはず、ScriptBlock もコンパイル时にいるものと实行时にいるもので分离した方がいいかな
		// 以下の2つは实行时に必要、TopLevel は不要になるけど。
		// InterCodeGenerator は、インターフェイスを作った方がいいかな？
		// TJS2 バイトコードのみじゃなくて、他のコードジェネレーターも作りやすいように。
		public virtual void NotifyUsingPreProcessror()
		{
			mUsingPreProcessor = true;
		}

		public virtual bool IsUsingPreProcessor()
		{
			return mUsingPreProcessor;
		}

		private bool mIsUnlex;

		private int mUnlexToken;

		private int mUnlexValue;

		private int mToken;

		private int mValue;

		private ExprNode mNode;

		private string mFirstError;

		private int mFirstErrorPos;

		private string mName;

		private int mLineOffset;

		private string mScript;

		private ScriptLineData mLineData;

		// 以下の4つは实行时にいるかな、名前以外はエラー発生时に必要になるだけだろうけど。
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private void PushContextStack(string name, int type)
		{
			InterCodeGenerator ctx = new InterCodeGenerator(mInterCodeGenerator, name, this, 
				type);
			if (mInterCodeGenerator == null)
			{
				if (mTopLevelGenerator != null)
				{
					throw new TjsException(Engine.Error.InternalError);
				}
				mTopLevelGenerator = ctx;
			}
			mGeneratorStack.Push(ctx);
			mInterCodeGenerator = ctx;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private void PopContextStack()
		{
			mInterCodeGenerator.Commit();
			mGeneratorStack.Pop();
			if (mGeneratorStack.Count >= 1)
			{
				mInterCodeGenerator = mGeneratorStack.Peek();
			}
			else
			{
				mInterCodeGenerator = null;
			}
		}

		public Compiler(Tjs owner)
		{
			mOwner = owner;
			// Java で初期值となる初期化は省略
			//mScript = null;
			//mName = null;
			//mInterCodeContext = null;
			//mTopLevelContext = null;
			//mLexicalAnalyzer = null;
			//mUsingPreProcessor = false;
			//mLineOffset = 0;
			//mCompileErrorCount = 0;
			//mNode = null;
			mGeneratorStack = new Stack<InterCodeGenerator>();
			mInterCodeGeneratorList = new AList<InterCodeGenerator>();
			mInterCodeObjectList = new AList<InterCodeObject>();
		}

		~Compiler()
		{
			if (mTopLevelObject != null)
			{
				mTopLevelObject = null;
			}
			if (mGeneratorStack != null)
			{
				while (mGeneratorStack.Count > 0)
				{
					mGeneratorStack.Pop();
				}
			}
			if (mLexicalAnalyzer != null)
			{
				mLexicalAnalyzer = null;
			}
			if (mScript != null)
			{
				mScript = null;
			}
			if (mName != null)
			{
				mName = null;
			}
            //try
            //{
            //    base.Finalize();
            //}
            //catch
            //{
            //}
		}

		public virtual LexicalAnalyzer GetLexicalAnalyzer()
		{
			return mLexicalAnalyzer;
		}

		public virtual int SrcPosToLine(int pos)
		{
			return mLineData.GetSrcPosToLine(pos);
		}

		private void Unlex(int token, int value)
		{
			mIsUnlex = true;
			mUnlexToken = token;
			mUnlexValue = value;
		}

		private void Unlex()
		{
			mIsUnlex = true;
			mUnlexToken = mToken;
			mUnlexValue = mValue;
		}

		/// <exception cref="CompileException"></exception>
		private int Lex()
		{
			if (mIsUnlex)
			{
				mIsUnlex = false;
				mToken = mUnlexToken;
				mValue = mUnlexValue;
				return mUnlexToken;
			}
			else
			{
				mToken = mLexicalAnalyzer.GetNext();
				mValue = mLexicalAnalyzer.GetValue();
				return mToken;
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Parse(string script, bool isexpr, bool resultneeded)
		{
			mCompileErrorCount = 0;
			mLexicalAnalyzer = new LexicalAnalyzer(this, script, isexpr, resultneeded);
			mLineData = new ScriptLineData(script, mLineOffset);
			Program();
			mLexicalAnalyzer = null;
			if (mCompileErrorCount > 0)
			{
				throw new TjsScriptError(mFirstError, this, mFirstErrorPos);
			}
		}

		/// <exception cref="CompileException"></exception>
		public virtual void Error(string msg)
		{
			if (mCompileErrorCount == 0)
			{
				mFirstError = msg;
				mFirstErrorPos = mLexicalAnalyzer.GetCurrentPosition();
			}
			mCompileErrorCount++;
			string str = Engine.Error.SyntaxError.Replace("%1", msg);
			//int line = 1+mLexicalAnalyzer.getCurrentLine();
			int line = 1 + SrcPosToLine(mLexicalAnalyzer.GetCurrentPosition());
			string message = "Line (" + line + ") : " + str;
			Logger.Log(ERROR_TAG, message);
			if (mCompileErrorCount > 20)
			{
				throw new CompileException(Engine.Error.TooManyErrors, this, mFirstErrorPos
					);
			}
		}

		// ----------------------------- parser
		// const_dic_elm
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstDicElm()
		{
			int token = Lex();
			Variant var;
			if (token == Token.T_CONSTVAL)
			{
				string name = mLexicalAnalyzer.GetString(mValue);
				token = Lex();
				if (token != Token.T_COMMA)
				{
					Error(Engine.Error.ConstDicDelimiterError);
					Unlex();
					// syntax error
					return null;
				}
				token = Lex();
				switch (token)
				{
					case Token.T_MINUS:
					{
						token = Lex();
						if (token != Token.T_CONSTVAL)
						{
							Error(Engine.Error.ConstDicValueError);
							Unlex();
							// syntax error
							return null;
						}
						var = mLexicalAnalyzer.GetValue(mValue);
						var.ChangeSign();
						mInterCodeGenerator.GetCurrentNode().AddDictionaryElement(name, var);
						return null;
					}

					case Token.T_PLUS:
					{
						token = Lex();
						if (token != Token.T_CONSTVAL)
						{
							Error(Engine.Error.ConstDicValueError);
							Unlex();
							// syntax error
							return null;
						}
						var = mLexicalAnalyzer.GetValue(mValue);
						var.ToNumber();
						mInterCodeGenerator.GetCurrentNode().AddDictionaryElement(name, var);
						return null;
					}

					case Token.T_CONSTVAL:
					{
						mInterCodeGenerator.GetCurrentNode().AddDictionaryElement(name, mLexicalAnalyzer.
							GetValue(mValue));
						return null;
					}

					case Token.T_VOID:
					{
						mInterCodeGenerator.GetCurrentNode().AddDictionaryElement(name, new Variant());
						return null;
					}

					case Token.T_LPARENTHESIS:
					{
						Unlex();
						ExprNode node = ExprConstInlineArrayOrDic();
						mInterCodeGenerator.GetCurrentNode().AddDictionaryElement(name, node.GetValue());
						return null;
					}

					default:
					{
						Error(Engine.Error.ConstDicValueError);
						Unlex();
						break;
					}
				}
			}
			Unlex();
			return null;
		}

		// const_dic_elm_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstDicElmList()
		{
			int token;
			do
			{
				ExprConstDicElm();
				token = Lex();
			}
			while (token == Token.T_COMMA);
			Unlex();
			return null;
		}

		// const_inline_dic
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstInlineDic()
		{
			ExprNode node = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
			Dispatch2 dsp = Tjs.CreateDictionaryObject();
			node.SetValue(new Variant(dsp, dsp));
			mInterCodeGenerator.PushCurrentNode(node);
			ExprConstDicElmList();
			node = mInterCodeGenerator.GetCurrentNode();
			mInterCodeGenerator.PopCurrentNode();
			int token = Lex();
			if (token == Token.T_RBRACKET)
			{
				return node;
			}
			else
			{
				Error(Engine.Error.DicRBRACKETError);
				Unlex();
			}
			// error
			return null;
		}

		// const_array_elm
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstArrayElm()
		{
			int token = Lex();
			Variant var;
			switch (token)
			{
				case Token.T_MINUS:
				{
					token = Lex();
					if (token != Token.T_CONSTVAL)
					{
						Error(Engine.Error.ConstArrayValueError);
						Unlex();
						// syntax error
						return null;
					}
					var = mLexicalAnalyzer.GetValue(mValue);
					var.ChangeSign();
					mInterCodeGenerator.GetCurrentNode().AddArrayElement(var);
					return null;
				}

				case Token.T_PLUS:
				{
					token = Lex();
					if (token != Token.T_CONSTVAL)
					{
						Error(Engine.Error.ConstArrayValueError);
						Unlex();
						// syntax error
						return null;
					}
					var = mLexicalAnalyzer.GetValue(mValue);
					var.ToNumber();
					mInterCodeGenerator.GetCurrentNode().AddArrayElement(var);
					return null;
				}

				case Token.T_CONSTVAL:
				{
					mInterCodeGenerator.GetCurrentNode().AddArrayElement(mLexicalAnalyzer.GetValue(mValue
						));
					return null;
				}

				case Token.T_VOID:
				{
					mInterCodeGenerator.GetCurrentNode().AddArrayElement(new Variant());
					return null;
				}

				case Token.T_LPARENTHESIS:
				{
					Unlex();
					ExprNode node = ExprConstInlineArrayOrDic();
					mInterCodeGenerator.GetCurrentNode().AddArrayElement(node.GetValue());
					return null;
				}
			}
			Unlex();
			return null;
		}

		// const_array_elm_list, const_array_elm_list_opt
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstArrayElmList()
		{
			int token;
			do
			{
				ExprConstArrayElm();
				token = Lex();
			}
			while (token == Token.T_COMMA);
			Unlex();
			return null;
		}

		// const_inline_array
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstInlineArray()
		{
			ExprNode node = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
			Dispatch2 dsp = Tjs.CreateArrayObject();
			node.SetValue(new Variant(dsp, dsp));
			mInterCodeGenerator.PushCurrentNode(node);
			ExprConstArrayElmList();
			node = mInterCodeGenerator.GetCurrentNode();
			mInterCodeGenerator.PopCurrentNode();
			int token = Lex();
			if (token == Token.T_RBRACKET)
			{
				return node;
			}
			else
			{
				Error(Engine.Error.ArrayRBRACKETError);
				Unlex();
			}
			// error
			return null;
		}

		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprConstInlineArrayOrDic()
		{
			int token = Lex();
			if (token == Token.T_LPARENTHESIS)
			{
				token = Lex();
				if (token != Token.T_CONST)
				{
					Error(Engine.Error.ConstDicArrayStringError);
					Unlex();
				}
				token = Lex();
				if (token != Token.T_RPARENTHESIS)
				{
					Error(Engine.Error.ConstDicArrayStringError);
					Unlex();
				}
				token = Lex();
				if (token == Token.T_PERCENT)
				{
					// may be dic
					token = Lex();
					if (token == Token.T_LBRACKET)
					{
						return ExprConstInlineDic();
					}
					else
					{
						Error(Engine.Error.ConstDicLBRACKETError);
						Unlex();
					}
				}
				else
				{
					if (token == Token.T_LBRACKET)
					{
						// may be array
						return ExprConstInlineArray();
					}
					else
					{
						Error(Engine.Error.ConstArrayLBRACKETError);
						Unlex();
					}
				}
			}
			else
			{
				if (token == Token.T_CAST_CONST)
				{
					token = Lex();
					if (token == Token.T_PERCENT)
					{
						// may be dic
						token = Lex();
						if (token == Token.T_LBRACKET)
						{
							return ExprConstInlineDic();
						}
						else
						{
							Error(Engine.Error.ConstDicLBRACKETError);
							Unlex();
						}
					}
					else
					{
						if (token == Token.T_LBRACKET)
						{
							// may be array
							return ExprConstInlineArray();
						}
						else
						{
							Error(Engine.Error.ConstArrayLBRACKETError);
							Unlex();
						}
					}
				}
			}
			return null;
		}

		// dic_dummy_elm_opt
		// dic_elm
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprDicElm()
		{
			ExprNode node = null;
			int nodeCount = mInterCodeGenerator.GetNodeToDeleteVectorCount();
			ExprNode node0 = ExprExprNoComma();
			int token = Lex();
			if (token == Token.T_COMMA)
			{
				node = ExprExprNoComma();
				return mInterCodeGenerator.MakeNP2(Token.T_DICELM, node0, node);
			}
			else
			{
				if (token == Token.T_COLON)
				{
					int curNodeCount = mInterCodeGenerator.GetNodeToDeleteVectorCount();
					if (nodeCount == (curNodeCount - 1))
					{
						if (node0.OpCode == Token.T_SYMBOL)
						{
							node0.OpCode = Token.T_CONSTVAL;
							node = ExprExprNoComma();
							return mInterCodeGenerator.MakeNP2(Token.T_DICELM, node0, node);
						}
					}
					Error(Engine.Error.DicError);
					Unlex();
				}
				else
				{
					// error
					Error(Engine.Error.DicDelimiterError);
					Unlex();
				}
			}
			// error
			return null;
		}

		// dic_elm_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprDicElmList()
		{
			int token = Lex();
			if (token == Token.T_RBRACKET)
			{
				Unlex();
			}
			else
			{
				Unlex();
				ExprNode node = ExprDicElm();
				mInterCodeGenerator.GetCurrentNode().Add(node);
				token = Lex();
				while (token == Token.T_COMMA)
				{
					token = Lex();
					if (token == Token.T_RBRACKET)
					{
						break;
					}
					Unlex();
					node = ExprDicElm();
					mInterCodeGenerator.GetCurrentNode().Add(node);
					token = Lex();
				}
				Unlex();
			}
			return null;
		}

		// inline_dic
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprInlineDic()
		{
			int token = Lex();
			if (token == Token.T_PERCENT)
			{
				token = Lex();
				if (token == Token.T_LBRACKET)
				{
					ExprNode node = mInterCodeGenerator.MakeNP0(Token.T_INLINEDIC);
					mInterCodeGenerator.PushCurrentNode(node);
					ExprDicElmList();
					//				exprDicDummyElmOpt();
					token = Lex();
					if (token == Token.T_RBRACKET)
					{
						node = mInterCodeGenerator.GetCurrentNode();
						mInterCodeGenerator.PopCurrentNode();
						return node;
					}
					else
					{
						Error(Engine.Error.DicRBRACKETError);
						Unlex();
					}
				}
				else
				{
					// error
					Error(Engine.Error.DicLBRACKETError);
					Unlex();
				}
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// array_elm
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprArrayElm()
		{
			int token = Lex();
			if (token == Token.T_COMMA || token == Token.T_RBRACKET)
			{
				Unlex();
				return mInterCodeGenerator.MakeNP1(Token.T_ARRAYARG, null);
			}
			else
			{
				Unlex();
				ExprNode node = ExprExprNoComma();
				return mInterCodeGenerator.MakeNP1(Token.T_ARRAYARG, node);
			}
		}

		// array_elm_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprArrayElmList()
		{
			int token;
			do
			{
				ExprNode node = ExprArrayElm();
				mInterCodeGenerator.GetCurrentNode().Add(node);
				token = Lex();
			}
			while (token == Token.T_COMMA);
			Unlex();
			return null;
		}

		// inline_array
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprInlineArray()
		{
			int token = Lex();
			if (token == Token.T_LBRACKET)
			{
				ExprNode node = mInterCodeGenerator.MakeNP0(Token.T_INLINEARRAY);
				mInterCodeGenerator.PushCurrentNode(node);
				ExprArrayElmList();
				token = Lex();
				if (token == Token.T_RBRACKET)
				{
					node = mInterCodeGenerator.GetCurrentNode();
					mInterCodeGenerator.PopCurrentNode();
					return node;
				}
				else
				{
					Error(Engine.Error.ArrayRBRACKETError);
					Unlex();
				}
			}
			else
			{
				// error
				Unlex();
			}
			return null;
		}

		// call_arg
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCallArg()
		{
			int token = Lex();
			if (token == Token.T_RPARENTHESIS)
			{
				Unlex();
				// empty
				return null;
			}
			else
			{
				//		} else if( token == Token.T_ASTERISK ) {
				//			return mInterCodeGenerator.makeNP1( Token.T_EXPANDARG, null );
				Unlex();
				ExprNode node = ExprExprNoComma();
				if (node != null)
				{
					token = Lex();
					if (token == Token.T_ASTERISK)
					{
						return mInterCodeGenerator.MakeNP1(Token.T_EXPANDARG, node);
					}
					else
					{
						if (token == Token.T_ASTERISK_RPARENTHESIS)
						{
							Unlex(Token.T_RPARENTHESIS, 0);
							return mInterCodeGenerator.MakeNP1(Token.T_EXPANDARG, node);
						}
						else
						{
							if (token == Token.T_ASTERISK_COMMA)
							{
								Unlex(Token.T_COMMA, 0);
								return mInterCodeGenerator.MakeNP1(Token.T_EXPANDARG, node);
							}
							else
							{
								Unlex();
							}
						}
					}
				}
				else
				{
					token = Lex();
					if (token == Token.T_ASTERISK)
					{
						return mInterCodeGenerator.MakeNP1(Token.T_EXPANDARG, null);
					}
					else
					{
						if (token == Token.T_ASTERISK_RPARENTHESIS)
						{
							Unlex(Token.T_RPARENTHESIS, 0);
							return mInterCodeGenerator.MakeNP1(Token.T_EXPANDARG, null);
						}
						else
						{
							Unlex();
						}
					}
				}
				return node;
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCallArgList2(ExprNode node)
		{
			int token;
			node = mInterCodeGenerator.MakeNP1(Token.T_ARG, node);
			do
			{
				ExprNode n2 = ExprCallArg();
				node = mInterCodeGenerator.MakeNP2(Token.T_ARG, n2, node);
				token = Lex();
			}
			while (token == Token.T_COMMA);
			Unlex();
			return node;
		}

		// call_arg_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCallArgList()
		{
			int token = Lex();
			if (token == Token.T_OMIT)
			{
				return mInterCodeGenerator.MakeNP0(Token.T_OMIT);
			}
			else
			{
				Unlex();
				ExprNode node = ExprCallArg();
				token = Lex();
				if (token == Token.T_COMMA)
				{
					return ExprCallArgList2(node);
				}
				else
				{
					Unlex();
					return mInterCodeGenerator.MakeNP1(Token.T_ARG, node);
				}
			}
		}

		// func_call_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprFuncCallExpr(ExprNode node)
		{
			bool newExpression = false;
			if (node == null)
			{
				node = ExprPriorityExpr();
				newExpression = true;
			}
			if (node != null && newExpression == false)
			{
				int token = Lex();
				if (token != Token.T_LPARENTHESIS)
				{
					Error(Engine.Error.NotFoundFuncCallLPARENTHESISError);
					Unlex();
				}
				ExprNode n2 = ExprCallArgList();
				token = Lex();
				if (token != Token.T_RPARENTHESIS)
				{
					Error(Engine.Error.NotFoundFuncCallRPARENTHESISError);
					Unlex();
				}
				node = mInterCodeGenerator.MakeNP2(Token.T_LPARENTHESIS, node, n2);
			}
			return node;
		}

		// factor_expr
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprFactorExpr()
		{
			int token = Lex();
			ExprNode node = null;
			switch (token)
			{
				case Token.T_CONSTVAL:
				{
					node = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
					node.SetValue(mLexicalAnalyzer.GetValue(mValue));
					return node;
				}

				case Token.T_SYMBOL:
				{
					node = mInterCodeGenerator.MakeNP0(Token.T_SYMBOL);
					node.SetValue(new Variant(mLexicalAnalyzer.GetString(mValue)));
					return node;
				}

				case Token.T_THIS:
				{
					return mInterCodeGenerator.MakeNP0(Token.T_THIS);
				}

				case Token.T_SUPER:
				{
					return mInterCodeGenerator.MakeNP0(Token.T_SUPER);
				}

				case Token.T_FUNCTION:
				{
					Unlex();
					return ExprFuncExprDef();
				}

				case Token.T_GLOBAL:
				{
					return mInterCodeGenerator.MakeNP0(Token.T_GLOBAL);
				}

				case Token.T_VOID:
				{
					return mInterCodeGenerator.MakeNP0(Token.T_VOID);
				}

				case Token.T_LBRACKET:
				{
					// [
					Unlex();
					return ExprInlineArray();
				}

				case Token.T_PERCENT:
				{
					// %
					Unlex();
					return ExprInlineDic();
				}

				case Token.T_CAST_CONST:
				{
					// (const)
					Unlex();
					return ExprConstInlineArrayOrDic();
				}

				case Token.T_LPARENTHESIS:
				{
					// (
					token = Lex();
					if (token == Token.T_CONST)
					{
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Error(Engine.Error.NotFoundRPARENTHESISError);
							Unlex();
						}
						// syntax error
						Unlex(Token.T_CAST_CONST, 0);
						return ExprConstInlineArrayOrDic();
					}
					else
					{
						Unlex();
						mNode = Expr();
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Error(Engine.Error.NotFoundRPARENTHESISError);
							Unlex();
						}
						// syntax error
						Unlex(Token.T_CAST_EXPR, 0);
						return null;
					}
				}

				case Token.T_SLASHEQUAL:
				{
					// /=
					mLexicalAnalyzer.SetStartOfRegExp();
					token = Lex();
					if (token == Token.T_REGEXP)
					{
						node = mInterCodeGenerator.MakeNP0(Token.T_REGEXP);
						node.SetValue(mLexicalAnalyzer.GetValue(mValue));
						return node;
					}
					else
					{
						// 正规表现がない
						Error(Engine.Error.NotFoundRegexError);
						Unlex();
					}
					break;
				}

				case Token.T_SLASH:
				{
					// /
					mLexicalAnalyzer.SetStartOfRegExp();
					token = Lex();
					if (token == Token.T_REGEXP)
					{
						node = mInterCodeGenerator.MakeNP0(Token.T_REGEXP);
						node.SetValue(mLexicalAnalyzer.GetValue(mValue));
						return node;
					}
					else
					{
						// 正规表现がない
						Error(Engine.Error.NotFoundRegexError);
						Unlex();
					}
					break;
				}
			}
			Unlex();
			return null;
		}

		// priority_expr'
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprPriorityExpr1()
		{
			ExprNode node = ExprFactorExpr();
			if (node == null)
			{
				int token = Lex();
				if (token == Token.T_CAST_EXPR)
				{
					// (expr)
					node = mNode;
					mNode = null;
					return node;
				}
				else
				{
					if (token == Token.T_DOT)
					{
						mLexicalAnalyzer.SetNextIsBareWord();
						token = Lex();
						if (token == Token.T_SYMBOL)
						{
							ExprNode n2 = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
							n2.SetValue(mLexicalAnalyzer.GetValue(mValue));
							return mInterCodeGenerator.MakeNP1(Token.T_WITHDOT, n2);
						}
						else
						{
							Error(Engine.Error.NotFoundSymbolAfterDotError);
							Unlex();
						}
					}
					else
					{
						Unlex();
					}
				}
			}
			return node;
		}

		// priority_expr
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprPriorityExpr()
		{
			ExprNode node = ExprPriorityExpr1();
			//if( node != null ) {
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_LBRACKET || token == Token.T_DOT || token == Token.T_INCREMENT
					 || token == Token.T_DECREMENT || token == Token.T_EXCRAMATION || token == Token
					.T_LPARENTHESIS)
				{
					switch (token)
					{
						case Token.T_LBRACKET:
						{
							// [
							ExprNode n2 = Expr();
							token = Lex();
							if (token == Token.T_RBRACKET)
							{
								// ]
								node = mInterCodeGenerator.MakeNP2(Token.T_LBRACKET, node, n2);
							}
							else
							{
								// ] がない
								Error(Engine.Error.NotFoundDicOrArrayRBRACKETError);
								Unlex();
							}
							break;
						}

						case Token.T_DOT:
						{
							// .
							mLexicalAnalyzer.SetNextIsBareWord();
							token = Lex();
							if (token == Token.T_SYMBOL)
							{
								ExprNode n2 = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
								n2.SetValue(mLexicalAnalyzer.GetValue(mValue));
								node = mInterCodeGenerator.MakeNP2(Token.T_DOT, node, n2);
							}
							else
							{
								Error(Engine.Error.NotFoundSymbolAfterDotError);
								Unlex();
							}
							break;
						}

						case Token.T_INCREMENT:
						{
							// ++
							node = mInterCodeGenerator.MakeNP1(Token.T_POSTINCREMENT, node);
							break;
						}

						case Token.T_DECREMENT:
						{
							node = mInterCodeGenerator.MakeNP1(Token.T_POSTDECREMENT, node);
							break;
						}

						case Token.T_EXCRAMATION:
						{
							// !
							node = mInterCodeGenerator.MakeNP1(Token.T_EVAL, node);
							break;
						}

						case Token.T_LPARENTHESIS:
						{
							// (
							Unlex();
							node = ExprFuncCallExpr(node);
							break;
						}
					}
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// incontextof_expr
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprIncontextOfExpr()
		{
			ExprNode node = ExprPriorityExpr();
			int token = Lex();
			if (token == Token.T_INCONTEXTOF)
			{
				ExprNode n2 = ExprIncontextOfExpr();
				return mInterCodeGenerator.MakeNP2(Token.T_INCONTEXTOF, node, n2);
			}
			else
			{
				Unlex();
				return node;
			}
		}

		// unary_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprUnaryExpr()
		{
			int token = Lex();
			if (token == Token.T_LPARENTHESIS)
			{
				// ( の时、先读みしてトークンを切り替える
				token = Lex();
				switch (token)
				{
					case Token.T_INT:
					{
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Unlex();
							ExprNode n1 = ExprUnaryExpr();
							ExprNode retnode = mInterCodeGenerator.MakeNP1(Token.T_INT, n1);
							token = Lex();
							if (token != Token.T_RPARENTHESIS)
							{
								Error(Engine.Error.NotFoundRPARENTHESISError);
								Unlex();
							}
							// syntax error
							return retnode;
						}
						else
						{
							Unlex(Token.T_CAST_INT, 0);
						}
						break;
					}

					case Token.T_REAL:
					{
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Unlex();
							ExprNode n1 = ExprUnaryExpr();
							ExprNode retnode = mInterCodeGenerator.MakeNP1(Token.T_REAL, n1);
							token = Lex();
							if (token != Token.T_RPARENTHESIS)
							{
								Error(Engine.Error.NotFoundRPARENTHESISError);
								Unlex();
							}
							// syntax error
							return retnode;
						}
						else
						{
							Unlex(Token.T_CAST_REAL, 0);
						}
						break;
					}

					case Token.T_STRING:
					{
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Unlex();
							ExprNode n1 = ExprUnaryExpr();
							ExprNode retnode = mInterCodeGenerator.MakeNP1(Token.T_STRING, n1);
							token = Lex();
							if (token != Token.T_RPARENTHESIS)
							{
								Error(Engine.Error.NotFoundRPARENTHESISError);
								Unlex();
							}
							// syntax error
							return retnode;
						}
						else
						{
							Unlex(Token.T_CAST_STRING, 0);
						}
						break;
					}

					case Token.T_CONST:
					{
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Error(Engine.Error.NotFoundRPARENTHESISError);
							Unlex();
						}
						// syntax error
						Unlex(Token.T_CAST_CONST, 0);
						break;
					}

					default:
					{
						Unlex();
						mNode = Expr();
						token = Lex();
						if (token != Token.T_RPARENTHESIS)
						{
							Error(Engine.Error.NotFoundRPARENTHESISError);
							Unlex();
						}
						// syntax error
						Unlex(Token.T_CAST_EXPR, 0);
						break;
					}
				}
			}
			else
			{
				Unlex();
			}
			ExprNode node = ExprIncontextOfExpr();
			if (node == null)
			{
				token = Lex();
				switch (token)
				{
					case Token.T_EXCRAMATION:
					{
						// !
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_EXCRAMATION, node);
					}

					case Token.T_TILDE:
					{
						// ~
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_TILDE, node);
					}

					case Token.T_DECREMENT:
					{
						// --
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_DECREMENT, node);
					}

					case Token.T_INCREMENT:
					{
						// ++
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_INCREMENT, node);
					}

					case Token.T_NEW:
					{
						node = ExprFuncCallExpr(null);
						if (node != null)
						{
							node.OpCode = (Token.T_NEW);
						}
						return node;
					}

					case Token.T_INVALIDATE:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_INVALIDATE, node);
					}

					case Token.T_ISVALID:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_ISVALID, node);
					}

					case Token.T_DELETE:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_DELETE, node);
					}

					case Token.T_TYPEOF:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_TYPEOF, node);
					}

					case Token.T_SHARP:
					{
						// #
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_SHARP, node);
					}

					case Token.T_DOLLAR:
					{
						// $
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_DOLLAR, node);
					}

					case Token.T_PLUS:
					{
						// +
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_UPLUS, node);
					}

					case Token.T_MINUS:
					{
						// -
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_UMINUS, node);
					}

					case Token.T_AMPERSAND:
					{
						// &
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_IGNOREPROP, node);
					}

					case Token.T_ASTERISK:
					{
						// *
						node = ExprUnaryExpr();
						if (node == null)
						{
							Unlex(Token.T_ASTERISK_RPARENTHESIS, 0);
							return null;
						}
                        return mInterCodeGenerator.MakeNP1(Token.T_PROPACCESS, node);
					}

					case Token.T_CAST_INT:
					{
						// (int)
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_INT, node);
					}

					case Token.T_CAST_REAL:
					{
						// (real)
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_REAL, node);
					}

					case Token.T_CAST_STRING:
					{
						// (string)
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_STRING, node);
					}

					case Token.T_INT:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_INT, node);
					}

					case Token.T_REAL:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_REAL, node);
					}

					case Token.T_STRING:
					{
						node = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP1(Token.T_STRING, node);
					}

					default:
					{
						Unlex();
						break;
					}
				}
			}
			else
			{
				token = Lex();
				switch (token)
				{
					case Token.T_ISVALID:
					{
						return mInterCodeGenerator.MakeNP1(Token.T_ISVALID, node);
					}

					case Token.T_INSTANCEOF:
					{
						ExprNode n2 = ExprUnaryExpr();
						return mInterCodeGenerator.MakeNP2(Token.T_INSTANCEOF, node, n2);
					}

					default:
					{
						Unlex();
						break;
					}
				}
			}
			return node;
		}

		// mul_div_expr, mul_div_expr_and_asterisk
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprMulDivExpr()
		{
			ExprNode node = ExprUnaryExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_PERCENT || token == Token.T_SLASH || token == Token.T_BACKSLASH
					 || token == Token.T_ASTERISK)
				{
					ExprNode n2 = ExprUnaryExpr();
					if (n2 == null)
					{
						token = Lex();
						if (token == Token.T_RPARENTHESIS)
						{
							Unlex(Token.T_ASTERISK_RPARENTHESIS, 0);
							return node;
						}
						else
						{
							if (token == Token.T_COMMA)
							{
								Unlex(Token.T_ASTERISK_COMMA, 0);
								return node;
							}
							else
							{
								Error(Engine.Error.NotFoundAsteriskAfterError);
							}
						}
						break;
					}
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// add_sub_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprAddSubExpr()
		{
			ExprNode node = ExprMulDivExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_PLUS || token == Token.T_MINUS)
				{
					ExprNode n2 = ExprMulDivExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// shift_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprShiftExpr()
		{
			ExprNode node = ExprAddSubExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_RARITHSHIFT || token == Token.T_LARITHSHIFT || token == Token
					.T_RBITSHIFT)
				{
					n2 = ExprAddSubExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// compare_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCompareExpr()
		{
			ExprNode node = ExprShiftExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_LT || token == Token.T_GT || token == Token.T_LTOREQUAL ||
					 token == Token.T_GTOREQUAL)
				{
					ExprNode n2 = ExprShiftExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// identical_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprIdenticalExpr()
		{
			ExprNode node = ExprCompareExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_NOTEQUAL || token == Token.T_EQUALEQUAL || token == Token
					.T_DISCNOTEQUAL | token == Token.T_DISCEQUAL)
				{
					ExprNode n2 = ExprCompareExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// and_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprAndExpr()
		{
			ExprNode node = ExprIdenticalExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_AMPERSAND)
				{
					// &
					n2 = ExprIdenticalExpr();
					node = mInterCodeGenerator.MakeNP2(Token.T_AMPERSAND, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// exclusive_or_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprExclusiveOrExpr()
		{
			ExprNode node = ExprAndExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_CHEVRON)
				{
					// ^
					n2 = ExprAndExpr();
					node = mInterCodeGenerator.MakeNP2(Token.T_CHEVRON, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// inclusive_or_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprInclusiveOrExpr()
		{
			ExprNode node = ExprExclusiveOrExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_VERTLINE)
				{
					// |
					n2 = ExprExclusiveOrExpr();
					node = mInterCodeGenerator.MakeNP2(Token.T_VERTLINE, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// logical_and_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprLogicalAndExpr()
		{
			ExprNode node = ExprInclusiveOrExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_LOGICALAND)
				{
					// &&
					n2 = ExprInclusiveOrExpr();
					node = mInterCodeGenerator.MakeNP2(Token.T_LOGICALAND, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// logical_or_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprLogicalOrExpr()
		{
			ExprNode node = ExprLogicalAndExpr();
			if (node != null)
			{
				int token = Lex();
				ExprNode n2;
				while (token == Token.T_LOGICALOR)
				{
					// ||
					n2 = ExprLogicalAndExpr();
					node = mInterCodeGenerator.MakeNP2(Token.T_LOGICALOR, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// cond_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCondExpr()
		{
			ExprNode node = ExprLogicalOrExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_QUESTION)
				{
					// ?
					ExprNode n2 = ExprCondExpr();
					token = Lex();
					if (token != Token.T_COLON)
					{
						Error(Engine.Error.NotFound3ColonError);
						Unlex();
					}
					ExprNode n3 = ExprCondExpr();
					node = mInterCodeGenerator.MakeNP3(Token.T_QUESTION, node, n2, n3);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// assign_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprAssignExpr()
		{
			ExprNode node = ExprCondExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_SWAP || token == Token.T_EQUAL || token == Token.T_AMPERSANDEQUAL
					 || token == Token.T_VERTLINEEQUAL || token == Token.T_CHEVRONEQUAL || token == 
					Token.T_MINUSEQUAL || token == Token.T_PLUSEQUAL || token == Token.T_PERCENTEQUAL
					 || token == Token.T_SLASHEQUAL || token == Token.T_BACKSLASHEQUAL || token == Token
					.T_ASTERISKEQUAL || token == Token.T_LOGICALOREQUAL || token == Token.T_LOGICALANDEQUAL
					 || token == Token.T_RARITHSHIFTEQUAL || token == Token.T_LARITHSHIFTEQUAL || token
					 == Token.T_RBITSHIFTEQUAL)
				{
					ExprNode n2 = ExprAssignExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		//  comma_expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCommaExpr()
		{
			ExprNode node = ExprAssignExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_COMMA)
				{
					ExprNode n2 = ExprAssignExpr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// expr
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode Expr()
		{
			ExprNode node = ExprCommaExpr();
			if (node != null)
			{
				int token = Lex();
				while (token == Token.T_IF)
				{
					ExprNode n2 = Expr();
					node = mInterCodeGenerator.MakeNP2(token, node, n2);
					token = Lex();
				}
				Unlex();
			}
			return node;
		}

		// expr_no_comma
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprExprNoComma()
		{
			return ExprAssignExpr();
		}

		// throw
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprThrow()
		{
			// throw は消化济み
			ExprNode node = Expr();
			int token = Lex();
			if (token == Token.T_SEMICOLON)
			{
				mInterCodeGenerator.ProcessThrowCode(node);
			}
			else
			{
				Error(Engine.Error.NotFoundSemicolonAfterThrowError);
				Unlex();
			}
			return null;
		}

		// catch
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprCatch()
		{
			int token = Lex();
			if (token == Token.T_CATCH)
			{
				token = Lex();
				if (token == Token.T_LPARENTHESIS)
				{
					// (
					token = Lex();
					if (token == Token.T_RPARENTHESIS)
					{
						// )
						mInterCodeGenerator.EnterCatchCode(null);
					}
					else
					{
						if (token == Token.T_SYMBOL)
						{
							int value = mValue;
							token = Lex();
							if (token != Token.T_RPARENTHESIS)
							{
								// )
								Error(Engine.Error.NotFoundRPARENTHESISAfterCatchError);
								Unlex();
							}
							mInterCodeGenerator.EnterCatchCode(mLexicalAnalyzer.GetString(value));
						}
						else
						{
							Error(Engine.Error.NotFoundRPARENTHESISAfterCatchError);
							Unlex();
						}
					}
				}
				else
				{
					Unlex();
					mInterCodeGenerator.EnterCatchCode(null);
				}
			}
			else
			{
				Error(Engine.Error.NotFoundCatchError);
				Unlex();
			}
			return null;
		}

		// try
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprTry()
		{
			// try は消化济み
			mInterCodeGenerator.EnterTryCode();
			ExprBlockOrStatment();
			ExprCatch();
			ExprBlockOrStatment();
			mInterCodeGenerator.ExitTryCode();
			return null;
		}

		// case
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprCase()
		{
			int token = Lex();
			if (token == Token.T_CASE)
			{
				ExprNode node = Expr();
				token = Lex();
				if (token != Token.T_COLON)
				{
					Error(Engine.Error.NotFoundCaseColonError);
					Unlex();
				}
				mInterCodeGenerator.ProcessCaseCode(node);
			}
			else
			{
				if (token == Token.T_DEFAULT)
				{
					token = Lex();
					if (token != Token.T_COLON)
					{
						Error(Engine.Error.NotFoundDefaultColonError);
						Unlex();
					}
					mInterCodeGenerator.ProcessCaseCode(null);
				}
				else
				{
					Error(Engine.Error.NotFoundCaseOrDefaultError);
					Unlex();
				}
			}
			// ここに来ることはないはず
			return null;
		}

		// with
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprWith()
		{
			// with は消化济み
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundWithLPARENTHESISError);
				Unlex();
			}
			ExprNode node = Expr();
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundWithRPARENTHESISError);
				Unlex();
			}
			mInterCodeGenerator.EnterWithCode(node);
			ExprBlockOrStatment();
			mInterCodeGenerator.ExitWidthCode();
			return null;
		}

		// switch
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprSwitch()
		{
			// switch は消化济み
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundSwitchLPARENTHESISError);
				Unlex();
			}
			ExprNode node = Expr();
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundSwitchRPARENTHESISError);
				Unlex();
			}
			mInterCodeGenerator.EnterSwitchCode(node);
			ExprBlock();
			mInterCodeGenerator.ExitSwitchCode();
			return null;
		}

		// a return statement.
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprReturn()
		{
			// return は消化济み
			int token = Lex();
			if (token == Token.T_SEMICOLON)
			{
				mInterCodeGenerator.ReturnFromFunc(null);
			}
			else
			{
				Unlex();
				ExprNode node = Expr();
				token = Lex();
				if (token != Token.T_SEMICOLON)
				{
					Error(Engine.Error.NotFoundSemicolonAfterReturnError);
					Unlex();
				}
				mInterCodeGenerator.ReturnFromFunc(node);
			}
			return null;
		}

		// extends_list, extends_name
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprExtendsList()
		{
			ExprNode node = ExprExprNoComma();
			mInterCodeGenerator.CreateExtendsExprCode(node, false);
			int token = Lex();
			if (token == Token.T_COMMA)
			{
				ExprExtendsList();
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// class_extender
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprClassExtender()
		{
			ExprNode node = ExprExprNoComma();
			int token = Lex();
			if (token == Token.T_COMMA)
			{
				mInterCodeGenerator.CreateExtendsExprCode(node, false);
				ExprExtendsList();
			}
			else
			{
				mInterCodeGenerator.CreateExtendsExprCode(node, true);
				Unlex();
			}
			return null;
		}

		// class_def
		/// <exception cref="TjsException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprClassDef()
		{
			int token = Lex();
			if (token == Token.T_CLASS)
			{
				token = Lex();
				if (token != Token.T_SYMBOL)
				{
					Error(Engine.Error.NotFoundSymbolAfterClassError);
					Unlex();
				}
				PushContextStack(mLexicalAnalyzer.GetString(mValue), ContextType.CLASS);
				token = Lex();
				if (token == Token.T_EXTENDS)
				{
					ExprClassExtender();
				}
				else
				{
					Unlex();
				}
				ExprBlock();
				PopContextStack();
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// property_handler_getter
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprPropertyGetter()
		{
			int token = Lex();
			if (token == Token.T_LPARENTHESIS)
			{
				// (
				token = Lex();
				if (token != Token.T_RPARENTHESIS)
				{
					// )
					Error(Engine.Error.NotFoundPropGetRPARENTHESISError);
					Unlex();
				}
			}
			else
			{
				Unlex();
			}
			PushContextStack("(getter)", ContextType.PROPERTY_GETTER);
			mInterCodeGenerator.EnterBlock();
			ExprBlock();
			mInterCodeGenerator.ExitBlock();
			PopContextStack();
			return null;
		}

		// property_handler_setter
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprPropertySetter()
		{
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundPropSetLPARENTHESISError);
				Unlex();
			}
			token = Lex();
			if (token != Token.T_SYMBOL)
			{
				Error(Engine.Error.NotFoundPropSetSymbolError);
				Unlex();
			}
			int value = mValue;
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundPropSetRPARENTHESISError);
				Unlex();
			}
			PushContextStack("(setter)", ContextType.PROPERTY_SETTER);
			mInterCodeGenerator.EnterBlock();
			mInterCodeGenerator.SetPropertyDeclArg(mLexicalAnalyzer.GetString(value));
			ExprBlock();
			mInterCodeGenerator.ExitBlock();
			PopContextStack();
			return null;
		}

		// property_handler_def_list
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprPropertyHandlerDefList()
		{
			int token = Lex();
			if (token == Token.T_SETTER)
			{
				ExprPropertySetter();
				token = Lex();
				if (token == Token.T_GETTER)
				{
					ExprPropertyGetter();
				}
				else
				{
					Unlex();
				}
			}
			else
			{
				if (token == Token.T_GETTER)
				{
					ExprPropertyGetter();
					token = Lex();
					if (token == Token.T_SETTER)
					{
						ExprPropertySetter();
					}
					else
					{
						Unlex();
					}
				}
				else
				{
					Error(Engine.Error.NotFoundPropError);
					Unlex();
				}
			}
			return null;
		}

		// property_def
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprPropertyDef()
		{
			int token = Lex();
			if (token == Token.T_PROPERTY)
			{
				token = Lex();
				if (token != Token.T_SYMBOL)
				{
					Error(Engine.Error.NotFoundSymbolAfterPropError);
					Unlex();
				}
				int value = mValue;
				token = Lex();
				if (token != Token.T_LBRACE)
				{
					Error(Engine.Error.NotFoundLBRACEAfterPropError);
					Unlex();
				}
				PushContextStack(mLexicalAnalyzer.GetString(value), ContextType.PROPERTY);
				ExprPropertyHandlerDefList();
				token = Lex();
				if (token != Token.T_RBRACE)
				{
					Error(Engine.Error.NotFoundRBRACEAfterPropError);
					Unlex();
				}
				PopContextStack();
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// func_decl_arg_collapse, func_decl_arg, func_decl_arg_at_least_one, func_decl_arg_list
		// exprFuncDeclArgs にまとめてしまってる
		// func_decl_arg_opt +
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprFuncDeclArgs()
		{
			int token = Lex();
			if (token == Token.T_SYMBOL)
			{
				int value = mValue;
				token = Lex();
				if (token == Token.T_EQUAL)
				{
					// symbol = ???
					ExprNode node = ExprExprNoComma();
					mInterCodeGenerator.AddFunctionDeclArg(mLexicalAnalyzer.GetString(value), node);
					token = Lex();
					if (token == Token.T_COMMA)
					{
						ExprFuncDeclArgs();
					}
					else
					{
						Unlex();
					}
				}
				else
				{
					if (token == Token.T_ASTERISK)
					{
						// symbol *
						mInterCodeGenerator.AddFunctionDeclArgCollapse(mLexicalAnalyzer.GetString(value));
					}
					else
					{
						// symbol
						mInterCodeGenerator.AddFunctionDeclArg(mLexicalAnalyzer.GetString(value), null);
						if (token == Token.T_COMMA)
						{
							ExprFuncDeclArgs();
						}
						else
						{
							Unlex();
						}
					}
				}
			}
			else
			{
				if (token == Token.T_ASTERISK)
				{
					mInterCodeGenerator.AddFunctionDeclArgCollapse(null);
				}
				else
				{
					Unlex();
				}
			}
			return null;
		}

		// func_decl_arg_opt
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprFuncDeclArgOpt()
		{
			int token = Lex();
			if (token == Token.T_LPARENTHESIS)
			{
				ExprFuncDeclArgs();
				token = Lex();
				if (token != Token.T_RPARENTHESIS)
				{
					// )
					Error(Engine.Error.NotFoundFuncDeclRPARENTHESISError);
					Unlex();
				}
			}
			else
			{
				// empty
				Unlex();
			}
			return null;
		}

		// func_def
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private ExprNode ExprFunctionDef()
		{
			int token = Lex();
			if (token == Token.T_FUNCTION)
			{
				token = Lex();
				if (token != Token.T_SYMBOL)
				{
					Error(Engine.Error.NotFoundFuncDeclSymbolError);
					Unlex();
				}
				PushContextStack(mLexicalAnalyzer.GetString(mValue), ContextType.FUNCTION);
				mInterCodeGenerator.EnterBlock();
				ExprFuncDeclArgOpt();
				ExprBlock();
				mInterCodeGenerator.ExitBlock();
				PopContextStack();
			}
			else
			{
				Unlex();
				// ここに来ることはないはず
				throw new TjsException(Engine.Error.InternalError);
			}
			return null;
		}

		// func_expr_def
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		private ExprNode ExprFuncExprDef()
		{
			ExprNode node = null;
			int token = Lex();
			if (token == Token.T_FUNCTION)
			{
				PushContextStack("(anonymous)", ContextType.EXPR_FUNCTION);
				mInterCodeGenerator.EnterBlock();
				ExprFuncDeclArgOpt();
				ExprBlock();
				mInterCodeGenerator.ExitBlock();
				Variant v = new Variant(mInterCodeGenerator);
				PopContextStack();
				node = mInterCodeGenerator.MakeNP0(Token.T_CONSTVAL);
				node.SetValue(v);
			}
			else
			{
				Unlex();
				throw new TjsException(Engine.Error.InternalError);
			}
			return node;
		}

		// variable_id
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprVariableId()
		{
			int token = Lex();
			if (token == Token.T_SYMBOL)
			{
				int value = mValue;
				token = Lex();
				if (token == Token.T_EQUAL)
				{
					ExprNode node = ExprExprNoComma();
					mInterCodeGenerator.InitLocalVariable(mLexicalAnalyzer.GetString(value), node);
				}
				else
				{
					Unlex();
					mInterCodeGenerator.AddLocalVariable(mLexicalAnalyzer.GetString(value));
				}
			}
			else
			{
				Error(Engine.Error.NotFoundSymbolAfterVarError);
				Unlex();
			}
			return null;
		}

		// variable_id_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprVariableIdList()
		{
			ExprVariableId();
			int token = Lex();
			if (token == Token.T_COMMA)
			{
				ExprVariableIdList();
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// variable_def_inner
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprVariableDefInner()
		{
			// 现在のバージョンではconstを明确に区别してない
			int token = Lex();
			if (token == Token.T_VAR)
			{
				ExprVariableIdList();
			}
			else
			{
				if (token == Token.T_CONST)
				{
					ExprVariableIdList();
				}
				else
				{
					Unlex();
					throw new TjsException(Engine.Error.InternalError);
				}
			}
			return null;
		}

		// variable_def
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprVariableDef()
		{
			ExprVariableDefInner();
			int token = Lex();
			if (token != Token.T_SEMICOLON)
			{
				Error(Engine.Error.NotFoundVarSemicolonError);
				Unlex();
			}
			return null;
		}

		// for_third_clause
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprForThridClause()
		{
			int token = Lex();
			if (token == Token.T_RPARENTHESIS)
			{
				Unlex();
				mInterCodeGenerator.SetForThirdExprCode(null);
			}
			else
			{
				Unlex();
				ExprNode node = Expr();
				mInterCodeGenerator.SetForThirdExprCode(node);
			}
			return null;
		}

		// for_second_clause
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprForSecondClause()
		{
			int token = Lex();
			if (token == Token.T_SEMICOLON)
			{
				Unlex();
				mInterCodeGenerator.CreateForExprCode(null);
			}
			else
			{
				Unlex();
				ExprNode node = Expr();
				mInterCodeGenerator.CreateForExprCode(node);
			}
			return null;
		}

		// for_first_clause
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprForFirstClause()
		{
			int token = Lex();
			if (token == Token.T_VAR || token == Token.T_CONST)
			{
				Unlex();
				mInterCodeGenerator.EnterForCode(true);
				ExprVariableDefInner();
			}
			else
			{
				if (token == Token.T_SEMICOLON)
				{
					Unlex();
					mInterCodeGenerator.EnterForCode(false);
				}
				else
				{
					Unlex();
					ExprNode node = Expr();
					mInterCodeGenerator.EnterForCode(false);
					mInterCodeGenerator.CreateExprCode(node);
				}
			}
			return null;
		}

		// for
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprFor()
		{
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundForLPARENTHESISError);
				Unlex();
			}
			ExprForFirstClause();
			token = Lex();
			if (token != Token.T_SEMICOLON)
			{
				Error(Engine.Error.NotFoundForSemicolonError);
				Unlex();
			}
			ExprForSecondClause();
			token = Lex();
			if (token != Token.T_SEMICOLON)
			{
				Error(Engine.Error.NotFoundForSemicolonError);
				Unlex();
			}
			ExprForThridClause();
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				Error(Engine.Error.NotFoundForRPARENTHESISError);
				Unlex();
			}
			ExprBlockOrStatment();
			mInterCodeGenerator.ExitForCode();
			return null;
		}

		// if, if_else
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprIf()
		{
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundIfLPARENTHESISError);
				Unlex();
			}
			mInterCodeGenerator.EnterIfCode();
			ExprNode node = Expr();
			mInterCodeGenerator.CrateIfExprCode(node);
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundIfRPARENTHESISError);
				Unlex();
			}
			ExprBlockOrStatment();
			mInterCodeGenerator.ExitIfCode();
			token = Lex();
			if (token == Token.T_ELSE)
			{
				mInterCodeGenerator.EnterElseCode();
				ExprBlockOrStatment();
				mInterCodeGenerator.ExitElseCode();
			}
			else
			{
				Unlex();
			}
			return null;
		}

		// do_while
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprDo()
		{
			mInterCodeGenerator.EnterWhileCode(true);
			ExprBlockOrStatment();
			int token = Lex();
			if (token != Token.T_WHILE)
			{
				Error(Engine.Error.NotFoundDoWhileError);
				Unlex();
			}
			token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundDoWhileLPARENTHESISError);
				Unlex();
			}
			ExprNode node = Expr();
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundDoWhileRPARENTHESISError);
				Unlex();
			}
			mInterCodeGenerator.CreateWhileExprCode(node, true);
			token = Lex();
			if (token != Token.T_SEMICOLON)
			{
				// ;
				Error(Engine.Error.NotFoundDoWhileSemicolonError);
				Unlex();
			}
			mInterCodeGenerator.ExitWhileCode(true);
			return null;
		}

		// while
		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprWhile()
		{
			mInterCodeGenerator.EnterWhileCode(false);
			int token = Lex();
			if (token != Token.T_LPARENTHESIS)
			{
				// (
				Error(Engine.Error.NotFoundWhileLPARENTHESISError);
				Unlex();
			}
			ExprNode node = Expr();
			token = Lex();
			if (token != Token.T_RPARENTHESIS)
			{
				// )
				Error(Engine.Error.NotFoundWhileRPARENTHESISError);
				Unlex();
			}
			mInterCodeGenerator.CreateWhileExprCode(node, false);
			ExprBlockOrStatment();
			mInterCodeGenerator.ExitWhileCode(false);
			return null;
		}

		// block
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprBlock()
		{
			int token = Lex();
			if (token != Token.T_LBRACE)
			{
				Error(Engine.Error.NotFoundLBRACEAfterBlockError);
				Unlex();
			}
			// error
			mInterCodeGenerator.EnterBlock();
			ExprDefList();
			mInterCodeGenerator.ExitBlock();
			token = Lex();
			if (token != Token.T_RBRACE)
			{
				Error(Engine.Error.NotFoundRBRACEAfterBlockError);
				Unlex();
			}
			// error
			return null;
		}

		// statement
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprStatement()
		{
			int token = Lex();
			ExprNode node = null;
			switch (token)
			{
				case Token.T_IF:
				{
					// if or if else
					node = ExprIf();
					break;
				}

				case Token.T_WHILE:
				{
					node = ExprWhile();
					break;
				}

				case Token.T_DO:
				{
					node = ExprDo();
					break;
				}

				case Token.T_FOR:
				{
					node = ExprFor();
					break;
				}

				case Token.T_BREAK:
				{
					token = Lex();
					if (token != Token.T_SEMICOLON)
					{
						Error(Engine.Error.NotFoundBreakSemicolonError);
						Unlex();
					}
					mInterCodeGenerator.DoBreak();
					break;
				}

				case Token.T_CONTINUE:
				{
					token = Lex();
					if (token != Token.T_SEMICOLON)
					{
						Error(Engine.Error.NotFoundContinueSemicolonError);
						Unlex();
					}
					mInterCodeGenerator.DoContinue();
					break;
				}

				case Token.T_DEBUGGER:
				{
					token = Lex();
					if (token != Token.T_SEMICOLON)
					{
						Error(Engine.Error.NotFoundBebuggerSemicolonError);
						Unlex();
					}
					mInterCodeGenerator.DoDebugger();
					break;
				}

				case Token.T_VAR:
				case Token.T_CONST:
				{
					Unlex();
					node = ExprVariableDef();
					break;
				}

				case Token.T_FUNCTION:
				{
					Unlex();
					node = ExprFunctionDef();
					break;
				}

				case Token.T_PROPERTY:
				{
					Unlex();
					node = ExprPropertyDef();
					break;
				}

				case Token.T_CLASS:
				{
					Unlex();
					node = ExprClassDef();
					break;
				}

				case Token.T_RETURN:
				{
					node = ExprReturn();
					break;
				}

				case Token.T_SWITCH:
				{
					node = ExprSwitch();
					break;
				}

				case Token.T_WITH:
				{
					node = ExprWith();
					break;
				}

				case Token.T_CASE:
				case Token.T_DEFAULT:
				{
					Unlex();
					node = ExprCase();
					break;
				}

				case Token.T_TRY:
				{
					node = ExprTry();
					break;
				}

				case Token.T_THROW:
				{
					node = ExprThrow();
					break;
				}

				case Token.T_SEMICOLON:
				{
					// ignore
					break;
				}

				default:
				{
					Unlex();
					node = Expr();
					token = Lex();
					if (token != Token.T_SEMICOLON)
					{
						Error(Engine.Error.NotFoundSemicolonOrTokenTypeError);
						Unlex();
					}
					mInterCodeGenerator.CreateExprCode(node);
					break;
				}
			}
			return node;
		}

		// block_or_statement
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprBlockOrStatment()
		{
			int token = Lex();
			if (token == Token.T_LBRACE)
			{
				// block expression
				mInterCodeGenerator.EnterBlock();
				ExprDefList();
				token = Lex();
				if (token != Token.T_RBRACE)
				{
					Error(Engine.Error.NotFoundBlockRBRACEError);
					Unlex();
				}
				mInterCodeGenerator.ExitBlock();
			}
			else
			{
				Unlex();
				ExprStatement();
			}
			return null;
		}

		// def_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private ExprNode ExprDefList()
		{
			int token = Lex();
			while (token > 0 && token != Token.T_RBRACE)
			{
				Unlex();
				ExprBlockOrStatment();
				token = Lex();
			}
			Unlex();
			return null;
		}

		// program, global_list
		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		/// <exception cref="TjsException"></exception>
		private void Program()
		{
			PushContextStack("global", ContextType.TOP_LEVEL);
			int token;
			do
			{
				ExprDefList();
				token = Lex();
				if (token > 0)
				{
					Error(Engine.Error.EndOfBlockError);
					Unlex();
				}
			}
			while (token > 0);
			PopContextStack();
		}

		public virtual Tjs GetTJS()
		{
			return mOwner;
		}

		public virtual string GetName()
		{
			return mName;
		}

		public virtual void SetName(string name, int lineofs)
		{
			mName = null;
			if (name != null)
			{
				mLineOffset = lineofs;
				mName = name;
			}
		}

		public virtual int GetLineOffset()
		{
			return mLineOffset;
		}

		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual ScriptBlock DoCompile(string text, bool isexpression, bool isresultneeded
			)
		{
			if (text == null)
			{
				return null;
			}
			if (text.Length == 0)
			{
				return null;
			}
			mScript = text;
			// ラインリスト生成はここで行わない
			Parse(text, isexpression, isresultneeded);
			// InterCodeObject を生成する
			ScriptBlock ret;
			ret = GenerateInterCodeObjects();
			return ret;
		}

		private const int MEMBERENSURE = unchecked((int)(0x00000200));

		private const int IGNOREPROP = unchecked((int)(0x00000800));

		// create a member if not exists
		// ignore property invoking
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private ScriptBlock GenerateInterCodeObjects()
		{
			// dumpClassStructure();
			ScriptBlock block = new ScriptBlock(mOwner, mName, mLineOffset, mScript, mLineData
				);
			mInterCodeObjectList.Clear();
			// 1st. pass, まずはInterCodeObjectを作る
			int count = mInterCodeGeneratorList.Count;
			for (int i = 0; i < count; i++)
			{
				InterCodeGenerator gen = mInterCodeGeneratorList[i];
				mInterCodeObjectList.AddItem(gen.CreteCodeObject(block));
			}
			Variant val = new Variant();
			// 2nd. pass, 次にInterCodeObject内のリンクを解决する
			for (int i_1 = 0; i_1 < count; i_1++)
			{
				InterCodeGenerator gen = mInterCodeGeneratorList[i_1];
				InterCodeObject obj = mInterCodeObjectList[i_1];
				gen.CreateSecond(obj);
				gen.DateReplace(this);
				// DaraArray の中の InterCodeGenerator を InterCodeObject に差し替える
				//obj.dateReplace( this ); // DaraArray の中の InterCodeGenerator を InterCodeObject に差し替える
				AList<InterCodeGenerator.Property> p = gen.GetProp();
				if (p != null)
				{
					int pcount = p.Count;
					for (int j = 0; j < pcount; j++)
					{
						InterCodeGenerator.Property prop = p[j];
						val.Set(GetCodeObject(GetCodeIndex(prop.Value)));
						obj.mParent.PropSet(MEMBERENSURE | IGNOREPROP, prop.Name, val, obj.mParent);
					}
					p.Clear();
				}
			}
			mTopLevelObject = GetCodeObject(GetCodeIndex(mTopLevelGenerator));
			block.SetObjects(mTopLevelObject, mInterCodeObjectList);
			// 解放してしまう
			mInterCodeGenerator = null;
			mTopLevelGenerator = null;
			mGeneratorStack = null;
			mInterCodeGeneratorList.Clear();
			mInterCodeGeneratorList = null;
			mInterCodeObjectList.Clear();
			mInterCodeObjectList = null;
			return block;
		}

		public virtual string GetLineDescriptionString(int pos)
		{
			// get short description, like "mainwindow.tjs(321)"
			// pos is in character count from the first of the script
			StringBuilder builer = new StringBuilder(512);
			int line = SrcPosToLine(pos) + 1;
			if (mName != null)
			{
				builer.Append(mName);
			}
			else
			{
				builer.Append("anonymous@");
				builer.Append(ToString());
			}
			builer.Append('(');
			builer.Append(line.ToString());
			builer.Append(')');
			return builer.ToString();
		}

		public virtual int LineToSrcPos(int line)
		{
			// assumes line is added by LineOffset
			line -= mLineOffset;
			return mLineData.GetLineToSrcPos(line);
		}

		public virtual string GetLine(int line)
		{
			// note that this function DOES matter LineOffset
			line -= mLineOffset;
			return mLineData.GetLine(line);
		}

		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Compile(string text, bool isexpression, bool isresultneeded, 
			BinaryStream output)
		{
			if (text == null)
			{
				return;
			}
			if (text.Length == 0)
			{
				return;
			}
			mScript = text;
			Parse(text, isexpression, isresultneeded);
			// ここでバイトコード出力する
			//if( mName != null && mName.endsWith(".tjs") ) {
			//	String filename = mName.substring(0,mName.length()-4) + ".tjb";
			//	exportByteCode(filename);
			//}
			ExportByteCode(output);
		}

		/// <exception cref="CompileException"></exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void ToJavaCode(string text, bool isexpression, bool isresultneeded
			)
		{
			if (text == null)
			{
				return;
			}
			if (text.Length == 0)
			{
				return;
			}
			mScript = text;
			Parse(text, isexpression, isresultneeded);
			AList<JavaCodeIntermediate> clazz = new AList<JavaCodeIntermediate>();
			int count = mInterCodeGeneratorList.Count;
			for (int i = 0; i < count; i++)
			{
				InterCodeGenerator v = mInterCodeGeneratorList[i];
				if (v != null)
				{
					int type = v.GetContextType();
					AList<string> codes;
					switch (type)
					{
						case ContextType.TOP_LEVEL:
						{
							break;
						}

						case ContextType.FUNCTION:
						{
							InterCodeGenerator parent = v.GetParent();
							if (parent.GetContextType() == ContextType.CLASS)
							{
								string parentName = parent.GetName();
								codes = v.ToJavaCode(0, 0);
								int ccount = clazz.Count;
								bool inserted = false;
								for (int j = 0; j < ccount; j++)
								{
									JavaCodeIntermediate ci = clazz[j];
									if (ci.GetName().Equals(parentName))
									{
										ci.AddMember(new JavaCodeIntermediate.ClosureCode(v.GetName(), type, codes));
										inserted = true;
										break;
									}
								}
								if (inserted == false)
								{
									JavaCodeIntermediate ci = new JavaCodeIntermediate(parentName);
									ci.AddMember(new JavaCodeIntermediate.ClosureCode(v.GetName(), type, codes));
									clazz.AddItem(ci);
								}
							}
							break;
						}

						case ContextType.EXPR_FUNCTION:
						{
							break;
						}

						case ContextType.PROPERTY:
						{
							InterCodeGenerator parent = v.GetParent();
							if (parent.GetContextType() == ContextType.CLASS)
							{
								string parentName = parent.GetName();
								int ccount = clazz.Count;
								bool inserted = false;
								for (int j = 0; j < ccount; j++)
								{
									JavaCodeIntermediate ci = clazz[j];
									if (ci.GetName().Equals(parentName))
									{
										ci.AddProperty(v.GetName(), new JavaCodeIntermediate.Property(v.GetName()));
										inserted = true;
										break;
									}
								}
								if (inserted == false)
								{
									JavaCodeIntermediate ci = new JavaCodeIntermediate(parentName);
									ci.AddProperty(v.GetName(), new JavaCodeIntermediate.Property(v.GetName()));
									clazz.AddItem(ci);
								}
							}
							break;
						}

						case ContextType.PROPERTY_SETTER:
						case ContextType.PROPERTY_GETTER:
						{
							InterCodeGenerator parent = v.GetParent();
							if (parent.GetContextType() == ContextType.PROPERTY)
							{
								InterCodeGenerator parentparent = parent.GetParent();
								if (parentparent.GetContextType() == ContextType.CLASS)
								{
									string propName = parent.GetName();
									string className = parentparent.GetName();
									int ccount = clazz.Count;
									bool inserted = false;
									JavaCodeIntermediate target = null;
									for (int j = 0; j < ccount; j++)
									{
										JavaCodeIntermediate ci = clazz[j];
										if (ci.GetName().Equals(className))
										{
											target = ci;
											inserted = true;
											break;
										}
									}
									if (inserted == false)
									{
										JavaCodeIntermediate ci = new JavaCodeIntermediate(className);
										target = ci;
										clazz.AddItem(ci);
									}
									JavaCodeIntermediate.Property prop = target.GetProperty(propName);
									if (prop == null)
									{
										prop = new JavaCodeIntermediate.Property(propName);
										target.AddProperty(propName, prop);
									}
									codes = v.ToJavaCode(0, 0);
									if (type == ContextType.PROPERTY_SETTER)
									{
										prop.SetSetter(codes);
									}
									else
									{
										prop.SetGetter(codes);
									}
								}
							}
							break;
						}

						case ContextType.CLASS:
						{
							JavaCodeIntermediate ci = new JavaCodeIntermediate(v.GetName());
							codes = v.ToJavaCode(0, 0);
							ci.SetInitializer(codes);
							clazz.AddItem(ci);
							break;
						}

						case ContextType.SUPER_CLASS_GETTER:
						{
							break;
						}
					}
				}
			}
			int ccount_1 = clazz.Count;
			for (int j_1 = 0; j_1 < ccount_1; j_1++)
			{
				JavaCodeIntermediate ci = clazz[j_1];
				ci.Write();
			}
		}

		private const int FILE_TAG_SIZE = 8;

		private const int TAG_SIZE = 4;

		private const int CHUNK_SIZE_LEN = 4;

		public static readonly byte[] FILE_TAG = new byte[] { (byte)('T'), (byte)('J'), (
			byte)('S'), (byte)('2'), (byte)('1'), (byte)('0'), (byte)('0'), 0 };

		/// <exception cref="TjsException"></exception>
		private void ExportByteCode(BinaryStream output)
		{
			byte[] filetag = FILE_TAG;
			byte[] codetag = new byte[] { (byte)('T'), (byte)('J'), (byte)('S'), (byte)('2') };
			byte[] objtag = new byte[] { (byte)('O'), (byte)('B'), (byte)('J'), (byte)('S') };
			byte[] datatag = new byte[] { (byte)('D'), (byte)('A'), (byte)('T'), (byte)('A') };
			int count = mInterCodeGeneratorList.Count;
			AList<ByteBuffer> objarray = new AList<ByteBuffer>(count * 2);
			ConstArrayData constarray = new ConstArrayData();
			int objsize = 0;
			for (int i = 0; i < count; i++)
			{
				InterCodeGenerator obj = mInterCodeGeneratorList[i];
				ByteBuffer buf = obj.ExportByteCode(this, constarray);
				objarray.AddItem(buf);
				objsize += buf.Capacity() + TAG_SIZE + CHUNK_SIZE_LEN;
			}
			// tag + size
			objsize += TAG_SIZE + CHUNK_SIZE_LEN + 4 + 4;
			// OBJS tag + size + toplevel + count
			ByteBuffer dataarea = constarray.ExportBuffer();
			int datasize = dataarea.Capacity() + TAG_SIZE + CHUNK_SIZE_LEN;
			// DATA tag + size
			int filesize = objsize + datasize + FILE_TAG_SIZE + CHUNK_SIZE_LEN;
			// TJS2 tag + file size
			byte[] filesizearray = new byte[] { unchecked((byte)(filesize & unchecked((int)(0xff
				)))), unchecked((byte)(((int)(((uint)filesize) >> 8)) & unchecked((int)(0xff))))
				, unchecked((byte)(((int)(((uint)filesize) >> 16)) & unchecked((int)(0xff)))), unchecked(
				(byte)(((int)(((uint)filesize) >> 24)) & unchecked((int)(0xff)))) };
			byte[] datasizearray = new byte[] { unchecked((byte)(datasize & unchecked((int)(0xff
				)))), unchecked((byte)(((int)(((uint)datasize) >> 8)) & unchecked((int)(0xff))))
				, unchecked((byte)(((int)(((uint)datasize) >> 16)) & unchecked((int)(0xff)))), unchecked(
				(byte)(((int)(((uint)datasize) >> 24)) & unchecked((int)(0xff)))) };
			byte[] objsizearray = new byte[] { unchecked((byte)(objsize & unchecked((int)(0xff
				)))), unchecked((byte)(((int)(((uint)objsize) >> 8)) & unchecked((int)(0xff)))), 
				unchecked((byte)(((int)(((uint)objsize) >> 16)) & unchecked((int)(0xff)))), unchecked(
				(byte)(((int)(((uint)objsize) >> 24)) & unchecked((int)(0xff)))) };
			byte[] objcountarray = new byte[] { unchecked((byte)(count & unchecked((int)(0xff
				)))), unchecked((byte)(((int)(((uint)count) >> 8)) & unchecked((int)(0xff)))), unchecked(
				(byte)(((int)(((uint)count) >> 16)) & unchecked((int)(0xff)))), unchecked((byte)
				(((int)(((uint)count) >> 24)) & unchecked((int)(0xff)))) };
			int toplevel = -1;
			if (mTopLevelGenerator != null)
			{
				toplevel = GetCodeIndex(mTopLevelGenerator);
			}
			byte[] toparray = new byte[] { unchecked((byte)(toplevel & unchecked((int)(0xff))
				)), unchecked((byte)(((int)(((uint)toplevel) >> 8)) & unchecked((int)(0xff)))), 
				unchecked((byte)(((int)(((uint)toplevel) >> 16)) & unchecked((int)(0xff)))), unchecked(
				(byte)(((int)(((uint)toplevel) >> 24)) & unchecked((int)(0xff)))) };
			output.Write(filetag);
			output.Write(filesizearray);
			output.Write(datatag);
			output.Write(datasizearray);
			output.Write(dataarea);
			output.Write(objtag);
			output.Write(objsizearray);
			output.Write(toparray);
			output.Write(objcountarray);
			for (int i_1 = 0; i_1 < count; i_1++)
			{
				ByteBuffer buf = objarray[i_1];
				int size = buf.Capacity();
				byte[] bufsizearray = new byte[] { unchecked((byte)(size & unchecked((int)(0xff))
					)), unchecked((byte)(((int)(((uint)size) >> 8)) & unchecked((int)(0xff)))), unchecked(
					(byte)(((int)(((uint)size) >> 16)) & unchecked((int)(0xff)))), unchecked((byte)(
					((int)(((uint)size) >> 24)) & unchecked((int)(0xff)))) };
				output.Write(codetag);
				output.Write(bufsizearray);
				output.Write(buf);
			}
			output.Close();
			output = null;
			objarray.Clear();
			objarray = null;
			constarray = null;
			dataarea = null;
		}

		public virtual void Add(InterCodeGenerator gen)
		{
			mInterCodeGeneratorList.AddItem(gen);
		}

		public virtual void Remove(InterCodeGenerator gen)
		{
			mInterCodeGeneratorList.Remove(gen);
		}

		// for generat code
		/// <summary>位置を确定するために使う</summary>
		public virtual int GetCodeIndex(InterCodeGenerator gen)
		{
			return mInterCodeGeneratorList.IndexOf(gen);
		}

		public virtual int GetObjectIndex(InterCodeObject gen)
		{
			return mInterCodeObjectList.IndexOf(gen);
		}

		public virtual InterCodeObject GetCodeObject(int index)
		{
			if (index >= 0 && index < mInterCodeObjectList.Count)
			{
				return mInterCodeObjectList[index];
			}
			else
			{
				return null;
			}
		}

		public virtual string GetScript()
		{
			return mScript;
		}

		public int GetMaxLine()
		{
			return mLineData.GetMaxLine();
		}

		public virtual int CodePosToSrcPos(int codepos)
		{
			return 0;
		}
		// allways 0, 基本的に使われない
	}
}

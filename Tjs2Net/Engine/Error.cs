/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class Error
	{
		public static readonly string InternalError = "(InternalError) 内部エラーが発生しました";

		public static readonly string Warning = "(Warning) 警告: ";

		public static readonly string WarnEvalOperator = "(WarnEvalOperator) グローバルでない场所で后置 ! 演算子が使われています(この演算子の举动はTJS2 version 2.4.1 で变わりましたのでご注意ください)";

		public static readonly string NarrowToWideConversionError = "(NarrowToWideConversionError) ANSI 文字列を UNICODE 文字列に变换できません。现在のコードページで解释できない文字が含まれてます。正しいデータが指定されているかを确认してください。データが破损している可能性もあります";

		public static readonly string VariantConvertError = "(VariantConvertError) %1 から %2 へ型を变换できません";

		public static readonly string VariantConvertErrorToObject = "(VariantConvertErrorToObject) %1 から Object へ型を变换できません。Object 型が要求される文脉で Object 型以外の值が渡されるとこのエラーが発生します";

		public static readonly string IDExpected = "(IDExpected) 识别子を指定してください";

		public static readonly string SubstitutionInBooleanContext = "(SubstitutionInBooleanContext) 论理值が求められている场所で = 演算子が使用されています(== 演算子の间违いですか？代入した上でゼロと值を比较したい场合は、(A=B) != 0 の形式を使うことをお劝めします)";

		public static readonly string CannotModifyLHS = "(CannotModifyLHS) 不正な代入か不正な式の操作です";

		public static readonly string InsufficientMem = "(InsufficientMem) メモリが足りません";

		public static readonly string CannotGetResult = "(CannotGetResult) この式からは值を得ることができません";

		public static readonly string NullAccess = "(NullAccess) null オブジェクトにアクセスしようとしました";

		public static readonly string MemberNotFound = "(MemberNotFound) メンバ \"%1\" が见つかりません";

		public static readonly string MemberNotFoundNoNameGiven = "(MemberNotFoundNoNameGiven) メンバが见つかりません";

		public static readonly string NotImplemented = "(NotImplemented) 呼び出そうとした机能は未实装です";

		public static readonly string InvalidParam = "(InvalidParam) 不正な引数です";

		public static readonly string BadParamCount = "(BadParamCount) 引数の数が不正です";

		public static readonly string InvalidType = "(InvalidType) 关数ではないかプロパティの种类が违います";

		public static readonly string SpecifyDicOrArray = "(SpecifyDicOrArray) Dictionary または Array クラスのオブジェクトを指定してください";

		public static readonly string SpecifyArray = "(SpecifyArray) Array クラスのオブジェクトを指定してください";

		public static readonly string StringDeallocError = "(StringDeallocError) 文字列メモリブロックを解放できません";

		public static readonly string StringAllocError = "(StringAllocError) 文字列メモリブロックを确保できません";

		public static readonly string MisplacedBreakContinue = "(MisplacedBreakContinue) \"break\" または \"continue\" はここに书くことはできません";

		public static readonly string MisplacedCase = "(MisplacedCase) \"case\" はここに书くことはできません";

		public static readonly string MisplacedReturn = "(MisplacedReturn) \"return\" はここに书くことはできません";

		public static readonly string StringParseError = "(StringParseError) 文字列定数/正规表现/オクテット即值が终わらないままスクリプトの终端に达しました";

		public static readonly string NumberError = "(NumberError) 数值として解释できません";

		public static readonly string UnclosedComment = "(UnclosedComment) コメントが终わらないままスクリプトの终端に达しました";

		public static readonly string InvalidChar = "(InvalidChar) 不正な文字です : \'%1\'";

		public static readonly string Expected = "(Expected) %1 がありません";

		public static readonly string SyntaxError = "(SyntaxError) 文法エラーです(%1)";

		public static readonly string PPError = "(PPError) 条件コンパイル式にエラーがあります";

		public static readonly string CannotGetSuper = "(CannotGetSuper) スーパークラスが存在しないかスーパークラスを特定できません";

		public static readonly string InvalidOpecode = "(InvalidOpecode) 不正な VM コードです";

		public static readonly string RangeError = "(RangeError) 值が范围外です";

		public static readonly string AccessDenyed = "(AccessDenyed) 读み迂み专用あるいは书き迂み专用プロパティに对して行えない操作をしようとしました";

		public static readonly string NativeClassCrash = "(NativeClassCrash) 实行コンテキストが违います";

		public static readonly string InvalidObject = "(InvalidObject) オブジェクトはすでに无效化されています";

		public static readonly string CannotOmit = "(CannotOmit) \"...\" は关数外では使えません";

		public static readonly string CannotParseDate = "(CannotParseDate) 不正な日付文字列の形式です";

		public static readonly string InvalidValueForTimestamp = "(InvalidValueForTimestamp) 不正な日付?时刻です";

		public static readonly string ExceptionNotFound = "(ExceptionNotFound) \"Exception\" が存在しないため例外オブジェクトを作成できません";

		public static readonly string InvalidFormatString = "(InvalidFormatString) 不正な书式文字列です";

		public static readonly string DivideByZero = "(DivideByZero) 0 で除算をしようとしました";

		public static readonly string NotReconstructiveRandomizeData = "(NotReconstructiveRandomizeData) 乱数系列を初期化できません(おそらく不正なデータが渡されました)";

		public static readonly string Symbol = "(Symbol) 识别子";

		public static readonly string CallHistoryIsFromOutOfTJS2Script = "(CallHistoryIsFromOutOfTJS2Script) [TJSスクリプト管理外]";

		public static readonly string NObjectsWasNotFreed = "(NObjectsWasNotFreed) 合计 %1 个のオブジェクトが解放されていません";

		public static readonly string ObjectCreationHistoryDelimiter = "(ObjectCreationHistoryDelimiter) \n                     ";

		public static readonly string ObjectWasNotFreed = "(ObjectWasNotFreed) オブジェクト %1 [%2] が解放されていません。オブジェクト作成时の呼び出し履历は以下の通りです:\n                     %3";

		public static readonly string GroupByObjectTypeAndHistory = "(GroupByObjectTypeAndHistory) オブジェクトのタイプとオブジェクト作成时の履历による分类";

		public static readonly string GroupByObjectType = "(GroupByObjectType) オブジェクトのタイプによる分类";

		public static readonly string ObjectCountingMessageGroupByObjectTypeAndHistory = "%1 个 : [%2]\n                     %3";

		public static readonly string ObjectCountingMessageTJSGroupByObjectType = "(ObjectCountingMessageTJSGroupByObjectType) %1 个 : [%2]";

		public static readonly string WarnRunningCodeOnDeletingObject = "(WarnRunningCodeOnDeletingObject) %4: 削除中のオブジェクト %1[%2] 上でコードが实行されています。このオブジェクトの作成时の呼び出し履历は以下の通りです:\n                     %3";

		public static readonly string WriteError = "(WriteError) 书き迂みエラーが発生しました";

		public static readonly string ReadError = "(ReadError) 读み迂みエラーが発生しました。ファイルが破损している可能性や、デバイスからの读み迂みに失败した可能性があります";

		public static readonly string SeekError = "(SeekError) シークエラーが発生しました。ファイルが破损している可能性や、デバイスからの读み迂みに失败した可能性があります";

		public static readonly string TooManyErrors = "(TooManyErrors) Too many errors";

		public static readonly string ConstDicDelimiterError = "(ConstDicDelimiterError) 定数辞书(const Dictionary)で要素名と值の区切りが不正です";

		public static readonly string ConstDicValueError = "(ConstDicValueError) 定数辞书(const Dictionary)の要素值が不正です";

		public static readonly string ConstArrayValueError = "(ConstArrayValueError) 定数配列(const Array)の要素值が不正です";

		public static readonly string ConstDicArrayStringError = "(ConstDicArrayStringError) 定数辞书もしくは配列で(const)文字が不正です";

		public static readonly string ConstDicLBRACKETError = "(ConstDicLBRACKETError) 定数辞书(const Dictionary)で(const)%の后に\"[\"がありません";

		public static readonly string ConstArrayLBRACKETError = "(ConstArrayLBRACKETError) 定数配列(const Array)で(const)の后に\"[\"がありません";

		public static readonly string DicDelimiterError = "(DicDelimiterError) 辞书(Dictionary)で要素名と值の区切りが不正です";

		public static readonly string DicError = "(DicError) 辞书(Dictionary)が不正です";

		public static readonly string DicLBRACKETError = "(DicLBRACKETError) 辞书(Dictionary)で%の后に\"[\"がありません";

		public static readonly string DicRBRACKETError = "(DicRBRACKETError) 辞书(Dictionary)の终端に\"]\"がありません";

		public static readonly string ArrayRBRACKETError = "(ArrayRBRACKETError) 配列(Array)の终端に\"]\"がありません";

		public static readonly string NotFoundRegexError = "(NotFoundRegexError) 正规表现が要求される文脉で正规表现がありません";

		public static readonly string NotFoundSymbolAfterDotError = "(NotFoundSymbolAfterDotError) \".\"の后にシンボルがありません";

		public static readonly string NotFoundDicOrArrayRBRACKETError = "(NotFoundDicOrArrayRBRACKETError) 配列もしくは辞书要素を指す变数の终端に\"]\"がありません";

		public static readonly string NotFoundRPARENTHESISError = "(NotFoundRPARENTHESISError) \")\"が要求される文脉で\")\"がありません";

		public static readonly string NotFoundSemicolonAfterThrowError = "(NotFoundSemicolonAfterThrowError) throwの后の\";\"がありません";

		public static readonly string NotFoundRPARENTHESISAfterCatchError = "(NotFoundRPARENTHESISAfterCatchError) catchの后の\")\"がありません";

		public static readonly string NotFoundCaseOrDefaultError = "(NotFoundCaseOrDefaultError) caseかdefaultが要求される文脉でcaseかdefaultがありません";

		public static readonly string NotFoundWithLPARENTHESISError = "(NotFoundWithLPARENTHESISError) withの后に\"(\"がありません";

		public static readonly string NotFoundWithRPARENTHESISError = "(NotFoundWithRPARENTHESISError) withの后に\")\"がありません";

		public static readonly string NotFoundSwitchLPARENTHESISError = "(NotFoundSwitchLPARENTHESISError) switchの后に\"(\"がありません";

		public static readonly string NotFoundSwitchRPARENTHESISError = "(NotFoundSwitchRPARENTHESISError) switchの后に\")\"がありません";

		public static readonly string NotFoundSemicolonAfterReturnError = "(NotFoundSemicolonAfterReturnError) returnの后の\";\"がありません";

		public static readonly string NotFoundPropGetRPARENTHESISError = "(NotFoundPropGetRPARENTHESISError) property getterの后に\")\"がありません";

		public static readonly string NotFoundPropSetLPARENTHESISError = "(NotFoundPropSetLPARENTHESISError) property setterの后に\"(\"がありません";

		public static readonly string NotFoundPropSetRPARENTHESISError = "(NotFoundPropSetRPARENTHESISError) property setterの后に\")\"がありません";

		public static readonly string NotFoundPropError = "(NotFoundPropError) propertyの后に\"getter\"もしくは\"setter\"がありません";

		public static readonly string NotFoundSymbolAfterPropError = "(NotFoundSymbolAfterPropError) propertyの后にシンボルがありません";

		public static readonly string NotFoundLBRACEAfterPropError = "(NotFoundLBRACEAfterPropError) propertyの后に\"{\"がありません";

		public static readonly string NotFoundRBRACEAfterPropError = "(NotFoundRBRACEAfterPropError) propertyの后に\"}\"がありません";

		public static readonly string NotFoundFuncDeclRPARENTHESISError = "(NotFoundFuncDeclRPARENTHESISError) 关数定义の后に\")\"がありません";

		public static readonly string NotFoundFuncDeclSymbolError = "(NotFoundFuncDeclSymbolError) 关数定义にシンボル名がありません";

		public static readonly string NotFoundSymbolAfterVarError = "(NotFoundSymbolAfterVarError) 变数宣言にシンボルがありません";

		public static readonly string NotFoundForLPARENTHESISError = "(NotFoundForLPARENTHESISError) forの后に\"(\"がありません";

		public static readonly string NotFoundForRPARENTHESISError = "(NotFoundForRPARENTHESISError) forの后に\")\"がありません";

		public static readonly string NotFoundForSemicolonError = "(NotFoundForSemicolonError) forの各节の区切りに\";\"がありません";

		public static readonly string NotFoundIfLPARENTHESISError = "(NotFoundIfLPARENTHESISError) ifの后に\"(\"がありません";

		public static readonly string NotFoundIfRPARENTHESISError = "(NotFoundIfRPARENTHESISError) ifの后に\")\"がありません";

		public static readonly string NotFoundDoWhileLPARENTHESISError = "(NotFoundDoWhileLPARENTHESISError) do-whileの后に\"(\"がありません";

		public static readonly string NotFoundDoWhileRPARENTHESISError = "(NotFoundDoWhileRPARENTHESISError) do-whileの后に\")\"がありません";

		public static readonly string NotFoundDoWhileError = "(NotFoundDoWhileError) do-while文でwhileがありません";

		public static readonly string NotFoundDoWhileSemicolonError = "(NotFoundDoWhileSemicolonError) do-while文でwhileの后に\";\"がありません";

		public static readonly string NotFoundWhileLPARENTHESISError = "(NotFoundWhileLPARENTHESISError) whileの后に\"(\"がありません";

		public static readonly string NotFoundWhileRPARENTHESISError = "(NotFoundWhileRPARENTHESISError) whileの后に\")\"がありません";

		public static readonly string NotFoundLBRACEAfterBlockError = "(NotFoundLBRACEAfterBlockError) ブロックが要求される文脉で\"{\"がありません";

		public static readonly string NotFoundRBRACEAfterBlockError = "(NotFoundRBRACEAfterBlockError) ブロックが要求される文脉で\"}\"がありません";

		public static readonly string NotFoundSemicolonError = "(NotFoundSemicolonError) 文の终わりに\";\"がありません";

		public static readonly string NotFoundSemicolonOrTokenTypeError = "(NotFoundSemicolonOrTokenTypeError) 文の终わりに\";\"がないか、予约语のタイプミスです";

		public static readonly string NotFoundBlockRBRACEError = "(NotFoundBlockRBRACEError) ブロックの终わりに\"}\"がありません";

		public static readonly string NotFoundCatchError = "(NotFoundCatchError) tryの后にcatchがありません";

		public static readonly string NotFoundFuncCallLPARENTHESISError = "(NotFoundFuncCallLPARENTHESISError) 关数呼び出しの后に\"(\"がありません";

		public static readonly string NotFoundFuncCallRPARENTHESISError = "(NotFoundFuncCallRPARENTHESISError) 关数呼び出しの后に\")\"がありません";

		public static readonly string NotFoundVarSemicolonError = "(NotFoundVarSemicolonError) 变数宣言の后に\";\"がありません";

		public static readonly string NotFound3ColonError = "(NotFound3ColonError) 条件演算子の\":\"がありません";

		public static readonly string NotFoundCaseColonError = "(NotFoundCaseColonError) caseの后に\":\"がありません";

		public static readonly string NotFoundDefaultColonError = "(NotFoundDefaultColonError) defaultの后に\":\"がありません";

		public static readonly string NotFoundSymbolAfterClassError = "(NotFoundSymbolAfterClassError) classの后にシンボルがありません";

		public static readonly string NotFoundPropSetSymbolError = "(NotFoundPropSetSymbolError) property setterの引数がありません";

		public static readonly string NotFoundBreakSemicolonError = "(NotFoundBreakSemicolonError) breakの后に\";\"がありません";

		public static readonly string NotFoundContinueSemicolonError = "(NotFoundContinueSemicolonError) continueの后に\";\"がありません";

		public static readonly string NotFoundBebuggerSemicolonError = "(NotFoundBebuggerSemicolonError) debuggerの后に\";\"がありません";

		public static readonly string NotFoundAsteriskAfterError = "(NotFoundAsteriskAfterError) 关数呼び出し、关数定义の配列展开(*)が不正か、乘算が不正です";

		public static readonly string EndOfBlockError = "(EndOfBlockError) ブロックの对应が取れていません。\"}\"が多いです";

		public static readonly string NotFoundPreprocessorRPARENTHESISError = "(NotFoundPreprocessorRPARENTHESISError) プリプロセッサに\")\"がありません";

		public static readonly string PreprocessorZeroDiv = "(PreprocessorZeroDiv) プリプロセッサのゼロ除算エラー";

		public static readonly string ByteCodeBroken = "(ByteCodeBroken) バイトコードファイル读み迂みエラー。ファイルが坏れているかバイトコードとは异なるファイルです";

		public const int S_OK = 0;

		public const int S_TRUE = 1;

		public const int S_FALSE = 2;

		public const int E_FAIL = -1;

		public const int E_MEMBERNOTFOUND = -1001;

		public const int E_NOTIMPL = -1002;

		public const int E_INVALIDPARAM = -1003;

		public const int E_BADPARAMCOUNT = -1004;

		public const int E_INVALIDTYPE = -1005;

		public const int E_INVALIDOBJECT = -1006;

		public const int E_ACCESSDENIED = -1007;

		public const int E_NATIVECLASSCRASH = -1008;

		private static readonly string EXCEPTION_NAME = "Exception";

		/// <summary>TJSGetExceptionObject : retrieves TJS 'Exception' object</summary>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="VariantException">VariantException</exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public static void GetExceptionObject(Tjs tjs, Variant res, Variant msg, Variant 
			trace)
		{
			if (res == null)
			{
				return;
			}
			// not prcess
			// retrieve class "Exception" from global
			Dispatch2 global = tjs.GetGlobal();
			Variant val = new Variant();
			int hr = global.PropGet(0, EXCEPTION_NAME, val, global);
			if (hr < 0)
			{
				throw new TjsException(ExceptionNotFound);
			}
			// create an Exception object
			Holder<Dispatch2> excpobj = new Holder<Dispatch2>(null);
			VariantClosure clo = val.AsObjectClosure();
			Variant[] pmsg = new Variant[1];
			pmsg[0] = msg;
			hr = clo.CreateNew(0, null, excpobj, pmsg, clo.mObjThis);
			if (hr < 0)
			{
				throw new TjsException(ExceptionNotFound);
			}
			Dispatch2 disp = excpobj.mValue;
			if (trace != null)
			{
				string trace_name = "trace";
				disp.PropSet(Interface.MEMBERENSURE, trace_name, trace, disp);
			}
			res.Set(disp, disp);
			excpobj = null;
		}

		public static void ReportExceptionSource(string msg, InterCodeObject context, int
			 codepos)
		{
			if (Tjs.EnableDebugMode)
			{
				Tjs.OutputExceptionToConsole(msg + " at " + context.GetPositionDescriptionString(
					codepos));
			}
		}

		/// <exception cref="TjsException"></exception>
		public static void ThrowFrom_tjs_error(int hr, string name)
		{
			switch (hr)
			{
				case E_MEMBERNOTFOUND:
				{
					// raise an exception descripted as tjs_error
					// name = variable name ( otherwide it can be NULL )
					if (name != null)
					{
						string str = MemberNotFound.Replace("%1", name);
						throw new TjsException(str);
					}
                    throw new TjsException(MemberNotFoundNoNameGiven);
				}

				case E_NOTIMPL:
				{
					throw new TjsException(NotImplemented);
				}

				case E_INVALIDPARAM:
				{
					throw new TjsException(InvalidParam);
				}

				case E_BADPARAMCOUNT:
				{
					throw new TjsException(BadParamCount);
				}

				case E_INVALIDTYPE:
				{
					throw new TjsException(InvalidType);
				}

				case E_ACCESSDENIED:
				{
					throw new TjsException(AccessDenyed);
				}

				case E_INVALIDOBJECT:
				{
					throw new TjsException(InvalidObject);
				}

				case E_NATIVECLASSCRASH:
				{
					throw new TjsException(NativeClassCrash);
				}

				default:
				{
					if (hr < 0)
					{
						string buf = string.Format("Unknown failure : %08X", hr);
						throw new TjsException(buf);
					}
					break;
				}
			}
		}
	}
}

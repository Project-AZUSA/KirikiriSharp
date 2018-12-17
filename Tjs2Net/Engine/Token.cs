/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	internal class Token
	{
		public const int T_COMMA = 258;

		public const int T_EQUAL = 259;

		public const int T_AMPERSANDEQUAL = 260;

		public const int T_VERTLINEEQUAL = 261;

		public const int T_CHEVRONEQUAL = 262;

		public const int T_MINUSEQUAL = 263;

		public const int T_PLUSEQUAL = 264;

		public const int T_PERCENTEQUAL = 265;

		public const int T_SLASHEQUAL = 266;

		public const int T_BACKSLASHEQUAL = 267;

		public const int T_ASTERISKEQUAL = 268;

		public const int T_LOGICALOREQUAL = 269;

		public const int T_LOGICALANDEQUAL = 270;

		public const int T_RARITHSHIFTEQUAL = 271;

		public const int T_LARITHSHIFTEQUAL = 272;

		public const int T_RBITSHIFTEQUAL = 273;

		public const int T_QUESTION = 274;

		public const int T_LOGICALOR = 275;

		public const int T_LOGICALAND = 276;

		public const int T_VERTLINE = 277;

		public const int T_CHEVRON = 278;

		public const int T_AMPERSAND = 279;

		public const int T_NOTEQUAL = 280;

		public const int T_EQUALEQUAL = 281;

		public const int T_DISCNOTEQUAL = 282;

		public const int T_DISCEQUAL = 283;

		public const int T_SWAP = 284;

		public const int T_LT = 285;

		public const int T_GT = 286;

		public const int T_LTOREQUAL = 287;

		public const int T_GTOREQUAL = 288;

		public const int T_RARITHSHIFT = 289;

		public const int T_LARITHSHIFT = 290;

		public const int T_RBITSHIFT = 291;

		public const int T_PERCENT = 292;

		public const int T_SLASH = 293;

		public const int T_BACKSLASH = 294;

		public const int T_ASTERISK = 295;

		public const int T_EXCRAMATION = 296;

		public const int T_TILDE = 297;

		public const int T_DECREMENT = 298;

		public const int T_INCREMENT = 299;

		public const int T_NEW = 300;

		public const int T_DELETE = 301;

		public const int T_TYPEOF = 302;

		public const int T_PLUS = 303;

		public const int T_MINUS = 304;

		public const int T_SHARP = 305;

		public const int T_DOLLAR = 306;

		public const int T_ISVALID = 307;

		public const int T_INVALIDATE = 308;

		public const int T_INSTANCEOF = 309;

		public const int T_LPARENTHESIS = 310;

		public const int T_DOT = 311;

		public const int T_LBRACKET = 312;

		public const int T_THIS = 313;

		public const int T_SUPER = 314;

		public const int T_GLOBAL = 315;

		public const int T_RBRACKET = 316;

		public const int T_CLASS = 317;

		public const int T_RPARENTHESIS = 318;

		public const int T_COLON = 319;

		public const int T_SEMICOLON = 320;

		public const int T_LBRACE = 321;

		public const int T_RBRACE = 322;

		public const int T_CONTINUE = 323;

		public const int T_FUNCTION = 324;

		public const int T_DEBUGGER = 325;

		public const int T_DEFAULT = 326;

		public const int T_CASE = 327;

		public const int T_EXTENDS = 328;

		public const int T_FINALLY = 329;

		public const int T_PROPERTY = 330;

		public const int T_PRIVATE = 331;

		public const int T_PUBLIC = 332;

		public const int T_PROTECTED = 333;

		public const int T_STATIC = 334;

		public const int T_RETURN = 335;

		public const int T_BREAK = 336;

		public const int T_EXPORT = 337;

		public const int T_IMPORT = 338;

		public const int T_SWITCH = 339;

		public const int T_IN = 340;

		public const int T_INCONTEXTOF = 341;

		public const int T_FOR = 342;

		public const int T_WHILE = 343;

		public const int T_DO = 344;

		public const int T_IF = 345;

		public const int T_VAR = 346;

		public const int T_CONST = 347;

		public const int T_ENUM = 348;

		public const int T_GOTO = 349;

		public const int T_THROW = 350;

		public const int T_TRY = 351;

		public const int T_SETTER = 352;

		public const int T_GETTER = 353;

		public const int T_ELSE = 354;

		public const int T_CATCH = 355;

		public const int T_OMIT = 356;

		public const int T_SYNCHRONIZED = 357;

		public const int T_WITH = 358;

		public const int T_INT = 359;

		public const int T_REAL = 360;

		public const int T_STRING = 361;

		public const int T_OCTET = 362;

		public const int T_FALSE = 363;

		public const int T_NULL = 364;

		public const int T_TRUE = 365;

		public const int T_VOID = 366;

		public const int T_NAN = 367;

		public const int T_INFINITY = 368;

		public const int T_UPLUS = 369;

		public const int T_UMINUS = 370;

		public const int T_EVAL = 371;

		public const int T_POSTDECREMENT = 372;

		public const int T_POSTINCREMENT = 373;

		public const int T_IGNOREPROP = 374;

		public const int T_PROPACCESS = 375;

		public const int T_ARG = 376;

		public const int T_EXPANDARG = 377;

		public const int T_INLINEARRAY = 378;

		public const int T_ARRAYARG = 379;

		public const int T_INLINEDIC = 380;

		public const int T_DICELM = 381;

		public const int T_WITHDOT = 382;

		public const int T_THIS_PROXY = 383;

		public const int T_WITHDOT_PROXY = 384;

		public const int T_CONSTVAL = 385;

		public const int T_SYMBOL = 386;

		public const int T_REGEXP = 387;

		public const int T_CAST_INT = 512;

		public const int T_CAST_REAL = 513;

		public const int T_CAST_STRING = 514;

		public const int T_CAST_CONST = 515;

		public const int T_CAST_EXPR = 516;

		public const int T_ASTERISK_RPARENTHESIS = 517;

		public const int T_ASTERISK_COMMA = 518;

		public const int T_END_OF_VALUE = 10000;

		public const int PT_LPARENTHESIS = 258;

		public const int PT_RPARENTHESIS = 259;

		public const int PT_ERROR = 260;

		public const int PT_COMMA = 261;

		public const int PT_EQUAL = 262;

		public const int PT_NOTEQUAL = 263;

		public const int PT_EQUALEQUAL = 264;

		public const int PT_LOGICALOR = 265;

		public const int PT_LOGICALAND = 266;

		public const int PT_VERTLINE = 267;

		public const int PT_CHEVRON = 268;

		public const int PT_AMPERSAND = 269;

		public const int PT_LT = 270;

		public const int PT_GT = 271;

		public const int PT_LTOREQUAL = 272;

		public const int PT_GTOREQUAL = 273;

		public const int PT_PLUS = 274;

		public const int PT_MINUS = 275;

		public const int PT_ASTERISK = 276;

		public const int PT_SLASH = 277;

		public const int PT_PERCENT = 278;

		public const int PT_EXCLAMATION = 279;

		public const int PT_UN = 280;

		public const int PT_SYMBOL = 281;

		public const int PT_NUM = 282;

		// TJS2 token
		// 特殊
		// ( int )
		// ( real )
		// ( string )
		// ( const ) キャストじゃないけど、わかりやすく
		// ( expr ) キャストじゃないけど、わかりやすく
		// *) 关数呼び出しの引数配列展开に对应
		// *, 关数呼び出しの引数配列展开に对应
		// 末尾(ダミー)
		// for pre-processor token
		public static string GetTokenString(int token)
		{
			switch (token)
			{
				case T_COMMA:
				{
					return ",";
				}

				case T_EQUAL:
				{
					return "=";
				}

				case T_AMPERSANDEQUAL:
				{
					return "&=";
				}

				case T_VERTLINEEQUAL:
				{
					return "|=";
				}

				case T_CHEVRONEQUAL:
				{
					return "^=";
				}

				case T_MINUSEQUAL:
				{
					return "-=";
				}

				case T_PLUSEQUAL:
				{
					return "+=";
				}

				case T_PERCENTEQUAL:
				{
					return "%=";
				}

				case T_SLASHEQUAL:
				{
					return "/=";
				}

				case T_BACKSLASHEQUAL:
				{
					return "\\=";
				}

				case T_ASTERISKEQUAL:
				{
					return "*=";
				}

				case T_LOGICALOREQUAL:
				{
					return "||=";
				}

				case T_LOGICALANDEQUAL:
				{
					return "&&=";
				}

				case T_RARITHSHIFTEQUAL:
				{
					return ">>=";
				}

				case T_LARITHSHIFTEQUAL:
				{
					return "<<=";
				}

				case T_RBITSHIFTEQUAL:
				{
					return ">>>=";
				}

				case T_QUESTION:
				{
					return "?";
				}

				case T_LOGICALOR:
				{
					return "||";
				}

				case T_LOGICALAND:
				{
					return "&&";
				}

				case T_VERTLINE:
				{
					return "|";
				}

				case T_CHEVRON:
				{
					return "^";
				}

				case T_AMPERSAND:
				{
					return "&";
				}

				case T_NOTEQUAL:
				{
					return "!=";
				}

				case T_EQUALEQUAL:
				{
					return "==";
				}

				case T_DISCNOTEQUAL:
				{
					return "!==";
				}

				case T_DISCEQUAL:
				{
					return "==";
				}

				case T_SWAP:
				{
					return "<->";
				}

				case T_LT:
				{
					return "<";
				}

				case T_GT:
				{
					return ">";
				}

				case T_LTOREQUAL:
				{
					return "<=";
				}

				case T_GTOREQUAL:
				{
					return ">=";
				}

				case T_RARITHSHIFT:
				{
					return ">>";
				}

				case T_LARITHSHIFT:
				{
					return "<<";
				}

				case T_RBITSHIFT:
				{
					return ">>>";
				}

				case T_PERCENT:
				{
					return "%";
				}

				case T_SLASH:
				{
					return "/";
				}

				case T_BACKSLASH:
				{
					return "\\";
				}

				case T_ASTERISK:
				{
					return "*";
				}

				case T_EXCRAMATION:
				{
					return "!";
				}

				case T_TILDE:
				{
					return "~";
				}

				case T_DECREMENT:
				{
					return "--";
				}

				case T_INCREMENT:
				{
					return "++";
				}

				case T_NEW:
				{
					return "new";
				}

				case T_DELETE:
				{
					return "delete";
				}

				case T_TYPEOF:
				{
					return "typeof";
				}

				case T_PLUS:
				{
					return "+";
				}

				case T_MINUS:
				{
					return "-";
				}

				case T_SHARP:
				{
					return "#";
				}

				case T_DOLLAR:
				{
					return "$";
				}

				case T_ISVALID:
				{
					return "isvalid";
				}

				case T_INVALIDATE:
				{
					return "invalidate";
				}

				case T_INSTANCEOF:
				{
					return "instanceof";
				}

				case T_LPARENTHESIS:
				{
					return "(";
				}

				case T_DOT:
				{
					return ".";
				}

				case T_LBRACKET:
				{
					return "[";
				}

				case T_THIS:
				{
					return "this";
				}

				case T_SUPER:
				{
					return "super";
				}

				case T_GLOBAL:
				{
					return "global";
				}

				case T_RBRACKET:
				{
					return "]";
				}

				case T_CLASS:
				{
					return "class";
				}

				case T_RPARENTHESIS:
				{
					return ")";
				}

				case T_COLON:
				{
					return ":";
				}

				case T_SEMICOLON:
				{
					return ";";
				}

				case T_LBRACE:
				{
					return "{";
				}

				case T_RBRACE:
				{
					return "}";
				}

				case T_CONTINUE:
				{
					return "continue";
				}

				case T_FUNCTION:
				{
					return "function";
				}

				case T_DEBUGGER:
				{
					return "debugger";
				}

				case T_DEFAULT:
				{
					return "default";
				}

				case T_CASE:
				{
					return "case";
				}

				case T_EXTENDS:
				{
					return "extends";
				}

				case T_FINALLY:
				{
					return "finally";
				}

				case T_PROPERTY:
				{
					return "property";
				}

				case T_PRIVATE:
				{
					return "private";
				}

				case T_PUBLIC:
				{
					return "public";
				}

				case T_PROTECTED:
				{
					return "protected";
				}

				case T_STATIC:
				{
					return "static";
				}

				case T_RETURN:
				{
					return "return";
				}

				case T_BREAK:
				{
					return "break";
				}

				case T_EXPORT:
				{
					return "export";
				}

				case T_IMPORT:
				{
					return "import";
				}

				case T_SWITCH:
				{
					return "switch";
				}

				case T_IN:
				{
					return "in";
				}

				case T_INCONTEXTOF:
				{
					return "incontextof";
				}

				case T_FOR:
				{
					return "for";
				}

				case T_WHILE:
				{
					return "while";
				}

				case T_DO:
				{
					return "do";
				}

				case T_IF:
				{
					return "if";
				}

				case T_VAR:
				{
					return "var";
				}

				case T_CONST:
				{
					return "const";
				}

				case T_ENUM:
				{
					return "enum";
				}

				case T_GOTO:
				{
					return "goto";
				}

				case T_THROW:
				{
					return "throw";
				}

				case T_TRY:
				{
					return "try";
				}

				case T_SETTER:
				{
					return "setter";
				}

				case T_GETTER:
				{
					return "getter";
				}

				case T_ELSE:
				{
					return "else";
				}

				case T_CATCH:
				{
					return "catch";
				}

				case T_OMIT:
				{
					return "...";
				}

				case T_SYNCHRONIZED:
				{
					return "synchronized";
				}

				case T_WITH:
				{
					return "with";
				}

				case T_INT:
				{
					return "int";
				}

				case T_REAL:
				{
					return "real";
				}

				case T_STRING:
				{
					return "string";
				}

				case T_OCTET:
				{
					return "octet";
				}

				case T_FALSE:
				{
					return "false";
				}

				case T_NULL:
				{
					return "null";
				}

				case T_TRUE:
				{
					return "true";
				}

				case T_VOID:
				{
					return "void";
				}

				case T_NAN:
				{
					return "NaN";
				}

				case T_INFINITY:
				{
					return "Infinity";
				}

				case T_UPLUS:
				{
					return "uplus";
				}

				case T_UMINUS:
				{
					return "uminus";
				}

				case T_EVAL:
				{
					return "eval";
				}

				case T_POSTDECREMENT:
				{
					return "--(post)";
				}

				case T_POSTINCREMENT:
				{
					return "++(post)";
				}

				case T_IGNOREPROP:
				{
					return "ignoreprop";
				}

				case T_PROPACCESS:
				{
					return "propaccess";
				}

				case T_ARG:
				{
					return "arg";
				}

				case T_EXPANDARG:
				{
					return "expandarg";
				}

				case T_INLINEARRAY:
				{
					return "inlinearray";
				}

				case T_ARRAYARG:
				{
					return "arrayarg";
				}

				case T_INLINEDIC:
				{
					return "inlinedic";
				}

				case T_DICELM:
				{
					return "dicelm";
				}

				case T_WITHDOT:
				{
					return "withdot";
				}

				case T_THIS_PROXY:
				{
					return "this_proxy";
				}

				case T_WITHDOT_PROXY:
				{
					return "withdot_proxy";
				}

				case T_CONSTVAL:
				{
					return "constval";
				}

				case T_SYMBOL:
				{
					return "symbol";
				}

				case T_REGEXP:
				{
					return "regexp";
				}

				case T_CAST_INT:
				{
					// 特殊
					return "(int)";
				}

				case T_CAST_REAL:
				{
					// ( int )
					return "(real)";
				}

				case T_CAST_STRING:
				{
					// ( real )
					return "(string)";
				}

				case T_CAST_CONST:
				{
					// ( string )
					return "(const)";
				}

				case T_CAST_EXPR:
				{
					// ( const ) キャストじゃないけど、わかりやすく
					return "(expr)";
				}

				case T_ASTERISK_RPARENTHESIS:
				{
					// ( expr ) キャストじゃないけど、わかりやすく
					return "*)";
				}

				case T_ASTERISK_COMMA:
				{
					// *) 关数呼び出しの引数配列展开に对应
					return "*,";
				}

				case T_END_OF_VALUE:
				{
					// *, 关数呼び出しの引数配列展开に对应
					return "END_OF_VALUE";
				}

				case 0:
				{
					// 末尾(ダミー)
					return "EOF";
				}
			}
			return "unknown";
		}
	}
}

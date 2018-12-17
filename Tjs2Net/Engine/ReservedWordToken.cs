/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class ReservedWordToken
	{
		private const int T_NEW = 300;

		private const int T_DELETE = 301;

		private const int T_TYPEOF = 302;

		private const int T_ISVALID = 307;

		private const int T_INVALIDATE = 308;

		private const int T_INSTANCEOF = 309;

		private const int T_THIS = 313;

		private const int T_SUPER = 314;

		private const int T_GLOBAL = 315;

		private const int T_CLASS = 317;

		private const int T_CONTINUE = 323;

		private const int T_FUNCTION = 324;

		private const int T_DEBUGGER = 325;

		private const int T_DEFAULT = 326;

		private const int T_CASE = 327;

		private const int T_EXTENDS = 328;

		private const int T_FINALLY = 329;

		private const int T_PROPERTY = 330;

		private const int T_PRIVATE = 331;

		private const int T_PUBLIC = 332;

		private const int T_PROTECTED = 333;

		private const int T_STATIC = 334;

		private const int T_RETURN = 335;

		private const int T_BREAK = 336;

		private const int T_EXPORT = 337;

		private const int T_IMPORT = 338;

		private const int T_SWITCH = 339;

		private const int T_IN = 340;

		private const int T_INCONTEXTOF = 341;

		private const int T_FOR = 342;

		private const int T_WHILE = 343;

		private const int T_DO = 344;

		private const int T_IF = 345;

		private const int T_VAR = 346;

		private const int T_CONST = 347;

		private const int T_ENUM = 348;

		private const int T_GOTO = 349;

		private const int T_THROW = 350;

		private const int T_TRY = 351;

		private const int T_SETTER = 352;

		private const int T_GETTER = 353;

		private const int T_ELSE = 354;

		private const int T_CATCH = 355;

		private const int T_SYNCHRONIZED = 357;

		private const int T_WITH = 358;

		private const int T_INT = 359;

		private const int T_REAL = 360;

		private const int T_STRING = 361;

		private const int T_OCTET = 362;

		private const int T_FALSE = 363;

		private const int T_NULL = 364;

		private const int T_TRUE = 365;

		private const int T_VOID = 366;

		private const int T_NAN = 367;

		private const int T_INFINITY = 368;

		// 最适化のためにコピー
		public static int GetToken(string str)
		{
			int len = str.Length;
			int id = -1;
			char ch;
			switch (len)
			{
				case 2:
				{
					ch = str[1];
					if (ch == 'o' && str[0] == 'd')
					{
						id = T_DO;
					}
					else
					{
						// "do";
						if (ch == 'n' && str[0] == 'i')
						{
							id = T_IN;
						}
						else
						{
							// "in";
							if (ch == 'f' && str[0] == 'i')
							{
								id = T_IF;
							}
						}
					}
					// "if";
					goto L0_break;
				}

				case 3:
				{
					switch (str[0])
					{
						case 'f':
						{
							if (str[2] == 'r' && str[1] == 'o')
							{
								id = T_FOR;
							}
							// for
							goto L0_break;
						}

						case 'i':
						{
							if (str[2] == 't' && str[1] == 'n')
							{
								id = T_INT;
							}
							// int
							goto L0_break;
						}

						case 'n':
						{
							if (str[2] == 'w' && str[1] == 'e')
							{
								id = T_NEW;
							}
							// new
							goto L0_break;
						}

						case 't':
						{
							if (str[2] == 'y' && str[1] == 'r')
							{
								id = T_TRY;
							}
							// try
							goto L0_break;
						}

						case 'v':
						{
							if (str[2] == 'r' && str[1] == 'a')
							{
								id = T_VAR;
							}
							// var
							goto L0_break;
						}

						case 'N':
						{
							if (str[2] == 'N' && str[1] == 'a')
							{
								id = T_NAN;
							}
							// NaN
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 4:
				{
					switch (str[0])
					{
						case 'c':
						{
							if ("case".Equals(str))
							{
								id = T_CASE;
							}
							goto L0_break;
						}

						case 'e':
						{
							ch = str[1];
							if (ch == 'n' && str[3] == 'm' && str[2] == 'u')
							{
								id = T_ENUM;
							}
							else
							{
								// enum
								if (ch == 'l' && str[3] == 'e' && str[2] == 's')
								{
									id = T_ELSE;
								}
							}
							// else
							goto L0_break;
						}

						case 'g':
						{
							if ("goto".Equals(str))
							{
								id = T_GOTO;
							}
							goto L0_break;
						}

						case 'n':
						{
							if ("null".Equals(str))
							{
								id = T_NULL;
							}
							goto L0_break;
						}

						case 'r':
						{
							if ("real".Equals(str))
							{
								id = T_REAL;
							}
							goto L0_break;
						}

						case 't':
						{
							ch = str[1];
							if (ch == 'h' && str[3] == 's' && str[2] == 'i')
							{
								id = T_THIS;
							}
							else
							{
								// this
								if (ch == 'r' && str[3] == 'e' && str[2] == 'u')
								{
									id = T_TRUE;
								}
							}
							// true
							goto L0_break;
						}

						case 'v':
						{
							if ("void".Equals(str))
							{
								id = T_VOID;
							}
							goto L0_break;
						}

						case 'w':
						{
							if ("with".Equals(str))
							{
								id = T_WITH;
							}
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 5:
				{
					switch (str[0])
					{
						case 'b':
						{
							if ("break".Equals(str))
							{
								id = T_BREAK;
							}
							goto L0_break;
						}

						case 'c':
						{
							ch = str[1];
							if (ch == 'o' && "const".Equals(str))
							{
								id = T_CONST;
							}
							else
							{
								if (ch == 'a' && "catch".Equals(str))
								{
									id = T_CATCH;
								}
								else
								{
									if (ch == 'l' && "class".Equals(str))
									{
										id = T_CLASS;
									}
								}
							}
							goto L0_break;
						}

						case 'f':
						{
							if ("false".Equals(str))
							{
								id = T_FALSE;
							}
							goto L0_break;
						}

						case 'o':
						{
							if ("octet".Equals(str))
							{
								id = T_OCTET;
							}
							goto L0_break;
						}

						case 's':
						{
							if ("super".Equals(str))
							{
								id = T_SUPER;
							}
							goto L0_break;
						}

						case 't':
						{
							if ("throw".Equals(str))
							{
								id = T_THROW;
							}
							goto L0_break;
						}

						case 'w':
						{
							if ("while".Equals(str))
							{
								id = T_WHILE;
							}
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 6:
				{
					switch (str[0])
					{
						case 'd':
						{
							if ("delete".Equals(str))
							{
								id = T_DELETE;
							}
							goto L0_break;
						}

						case 'e':
						{
							if ("export".Equals(str))
							{
								id = T_EXPORT;
							}
							goto L0_break;
						}

						case 'g':
						{
							ch = str[1];
							if (ch == 'l' && "global".Equals(str))
							{
								id = T_GLOBAL;
							}
							else
							{
								if (ch == 'e' && "getter".Equals(str))
								{
									id = T_GETTER;
								}
							}
							goto L0_break;
						}

						case 'i':
						{
							if ("import".Equals(str))
							{
								id = T_IMPORT;
							}
							goto L0_break;
						}

						case 'p':
						{
							if ("public".Equals(str))
							{
								id = T_PUBLIC;
							}
							goto L0_break;
						}

						case 'r':
						{
							if ("return".Equals(str))
							{
								id = T_RETURN;
							}
							goto L0_break;
						}

						case 's':
						{
							switch (str[2])
							{
								case 't':
								{
									if ("setter".Equals(str))
									{
										id = T_SETTER;
									}
									goto L0_break;
								}

								case 'a':
								{
									if ("static".Equals(str))
									{
										id = T_STATIC;
									}
									goto L0_break;
								}

								case 'r':
								{
									if ("string".Equals(str))
									{
										id = T_STRING;
									}
									goto L0_break;
								}

								case 'i':
								{
									if ("switch".Equals(str))
									{
										id = T_SWITCH;
									}
									goto L0_break;
								}
							}
							goto case 't';
						}

						case 't':
						{
							if ("typeof".Equals(str))
							{
								id = T_TYPEOF;
							}
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 7:
				{
					switch (str[0])
					{
						case 'd':
						{
							if ("default".Equals(str))
							{
								id = T_DEFAULT;
							}
							goto L0_break;
						}

						case 'e':
						{
							if ("extends".Equals(str))
							{
								id = T_EXTENDS;
							}
							goto L0_break;
						}

						case 'f':
						{
							if ("finally".Equals(str))
							{
								id = T_FINALLY;
							}
							goto L0_break;
						}

						case 'i':
						{
							if ("isvalid".Equals(str))
							{
								id = T_ISVALID;
							}
							goto L0_break;
						}

						case 'p':
						{
							if ("private".Equals(str))
							{
								id = T_PRIVATE;
							}
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 8:
				{
					switch (str[0])
					{
						case 'c':
						{
							if ("continue".Equals(str))
							{
								id = T_CONTINUE;
							}
							goto L0_break;
						}

						case 'd':
						{
							if ("debugger".Equals(str))
							{
								id = T_DEBUGGER;
							}
							goto L0_break;
						}

						case 'f':
						{
							if ("function".Equals(str))
							{
								id = T_FUNCTION;
							}
							goto L0_break;
						}

						case 'p':
						{
							if ("property".Equals(str))
							{
								id = T_PROPERTY;
							}
							goto L0_break;
						}

						case 'I':
						{
							if ("Infinity".Equals(str))
							{
								id = T_INFINITY;
							}
							goto L0_break;
						}
					}
					goto L0_break;
				}

				case 9:
				{
					if ("protected".Equals(str))
					{
						id = T_PROTECTED;
					}
					goto L0_break;
				}

				case 10:
				{
					ch = str[9];
					if (ch == 'e' && "invalidate".Equals(str))
					{
						id = T_INVALIDATE;
					}
					else
					{
						if (ch == 'f' && "instanceof".Equals(str))
						{
							id = T_INSTANCEOF;
						}
					}
					goto L0_break;
				}

				case 11:
				{
					if ("incontextof".Equals(str))
					{
						id = T_INCONTEXTOF;
					}
					goto L0_break;
				}

				case 12:
				{
					if ("synchronized".Equals(str))
					{
						id = T_SYNCHRONIZED;
					}
					goto L0_break;
				}
			}
L0_break: ;
			return id;
		}
	}
}

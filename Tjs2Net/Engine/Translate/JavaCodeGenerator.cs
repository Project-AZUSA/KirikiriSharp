/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.Sharpen;

namespace Tjs2.Engine.Translate
{
	/// <summary>
	/// ディスJavaコンパイラ
	/// TJS2 のバイトコードをコンパイル可能なJavaソースに变换する
	/// </summary>
	public class JavaCodeGenerator
	{
		private const byte TYPE_VOID = 0;

		private const byte TYPE_OBJECT = 1;

		private const byte TYPE_INTER_OBJECT = 2;

		private const byte TYPE_STRING = 3;

		private const byte TYPE_OCTET = 4;

		private const byte TYPE_REAL = 5;

		private const byte TYPE_INTEGER = 8;

		private const byte TYPE_INTER_GENERATOR = 10;

		private const byte TYPE_NULL_CLOSURE = 11;

		private const byte TYPE_UNKNOWN = unchecked((byte)(-1));

		internal class ExceptionData
		{
			public int mCatchIp;

			public int mExobjReg;

			public ExceptionData(int catchip, int reg)
			{
				// temporary
				mCatchIp = catchip;
				mExobjReg = reg;
			}
		}

		private AList<string> mSourceCodes;

		private AList<JavaCodeGenerator.ExceptionData> mExceptionDataStack;

		private short[] mCode;

		private Variant[] mData;

		private SourceCodeAccessor mAccessor;

		private int mFuncDeclUnnamedArgArrayBase;

		public JavaCodeGenerator(short[] ca, Variant[] da, SourceCodeAccessor a)
		{
			mCode = ca;
			mData = da;
			mAccessor = a;
			mExceptionDataStack = new AList<JavaCodeGenerator.ExceptionData>();
			mSourceCodes = new AList<string>();
		}

		public virtual void Set(short[] ca, Variant[] da, SourceCodeAccessor a)
		{
			mCode = ca;
			mData = da;
			mAccessor = a;
			mExceptionDataStack = new AList<JavaCodeGenerator.ExceptionData>();
			mSourceCodes = new AList<string>();
		}

		private int FindJumpTarget(int[] array, int pos)
		{
			int count = array.Length;
			for (int i = 0; i < count; i++)
			{
				if (array[i] == pos)
				{
					return i + 1;
				}
			}
			return -1;
		}

		private void OutputFuncSrc(string msg, string name, int line)
		{
			string buf;
			if (line >= 0)
			{
				buf = string.Format("// #%s(%d) %s", name, line + 1, msg);
			}
			else
			{
				buf = string.Format("// #%s %s", name, msg);
			}
			mSourceCodes.AddItem(buf);
		}

		private byte GetType(Variant v)
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
						return TYPE_INTEGER;
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
									if (dsp == null)
									{
										return TYPE_NULL_CLOSURE;
									}
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
											return TYPE_INTEGER;
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

		private string GetRegisterName(int v)
		{
			if (v < 0)
			{
				if (v == -1)
				{
					return "varthis";
				}
				else
				{
					if (v == -2)
					{
						return "this_proxy";
					}
					else
					{
						v += 3;
						v = -v;
						return "args[" + v + "]";
					}
				}
			}
			else
			{
				return "r" + v;
			}
		}

		/// <exception cref="CompileException"></exception>
		private string GetDataToStrOrNum(Variant v)
		{
			object o = v.ToJavaObject();
			if (o == null)
			{
				return "null";
			}
			else
			{
				if (o is string)
				{
					return "\"" + (string)o + "\"";
				}
				else
				{
					if (o is int)
					{
						return ((int)o).ToString();
					}
					else
					{
						if (o is double)
						{
							return ((double)o).ToString();
						}
						else
						{
							if (o is VariantClosure)
							{
								throw new CompileException("非サポートの定数形式");
							}
							else
							{
								if (o is InterCodeGenerator)
								{
									throw new CompileException("非サポートの定数形式");
								}
								else
								{
									if (o is ByteBuffer)
									{
										throw new CompileException("非サポートの定数形式");
									}
									else
									{
										if (o is long)
										{
											return ((long)o).ToString();
										}
									}
								}
							}
						}
					}
				}
			}
			throw new CompileException("非サポートの定数形式");
		}

		public virtual void GenFunCall(int variable, int frame, int declArgCount, int declCollapseBase
			)
		{
			mSourceCodes.AddItem("protected int process(Variant result, Variant[] param, Dispatch2 objthis) throws VariantException, TJSException {"
				);
			int num_alloc = variable;
			if (num_alloc == 0)
			{
				mSourceCodes.AddItem("Variant[] args = TJS.NULL_ARG;");
			}
			else
			{
				mSourceCodes.AddItem("Variant[] args = new Variant[" + num_alloc + "];");
				for (int i = 0; i < num_alloc; i++)
				{
					mSourceCodes.AddItem("args[" + i + "] = new Variant();");
				}
			}
			mSourceCodes.AddItem("Variant this_proxy;");
			mSourceCodes.AddItem("if( objthis != null ) {");
			mSourceCodes.AddItem("ObjectProxy proxy = new ObjectProxy();");
			mSourceCodes.AddItem("proxy.setObjects( objthis, ScriptsClass.getGlobal() );");
			mSourceCodes.AddItem("this_proxy = new Variant( proxy );");
			mSourceCodes.AddItem("} else {");
			mSourceCodes.AddItem("Dispatch2 global = ScriptsClass.getGlobal();");
			mSourceCodes.AddItem("this_proxy = new Variant(global,global);");
			mSourceCodes.AddItem("}");
			mSourceCodes.AddItem("Variant varthis = new Variant(objthis,objthis);");
			if (num_alloc > 0)
			{
				mSourceCodes.AddItem("final int numargs = param != null ? param.length : 0;");
				mSourceCodes.AddItem("if( numargs >= " + declArgCount + " ) {");
				if (declArgCount != 0)
				{
					int r = 0;
					int n = declArgCount;
					int argOffset = 0;
					while (true)
					{
						mSourceCodes.AddItem("args[" + r + "].set( param[" + argOffset + "] );");
						argOffset++;
						n--;
						if (n == 0)
						{
							break;
						}
						r++;
					}
				}
				mSourceCodes.AddItem("} else {");
				mSourceCodes.AddItem("int i;");
				mSourceCodes.AddItem("for( i = 0; i < numargs; i++ ) {");
				mSourceCodes.AddItem("args[i].set( param[i] );");
				mSourceCodes.AddItem("}");
				mSourceCodes.AddItem("for( ; i < " + declArgCount + "; i++ ) {");
				mSourceCodes.AddItem("args[i].clear();");
				mSourceCodes.AddItem("}");
				mSourceCodes.AddItem("}");
				if (declCollapseBase >= 0)
				{
					int r = declCollapseBase;
					mSourceCodes.AddItem("{");
					mSourceCodes.AddItem("Dispatch2 dsp = TJS.createArrayObject();");
					mSourceCodes.AddItem("args[" + r + "].set(dsp, dsp);");
					mSourceCodes.AddItem("if( numargs > " + declCollapseBase + " ) {");
					mSourceCodes.AddItem("for( int c = 0, i = " + declCollapseBase + "; i < numargs; i++, c++) {"
						);
					mSourceCodes.AddItem("dsp.propSetByNum(0, c, param[i], dsp);");
					mSourceCodes.AddItem("}");
					mSourceCodes.AddItem("}");
				}
			}
			mSourceCodes.AddItem("executeCode( varthis, this_proxy, args, result, objthis );"
				);
			mSourceCodes.AddItem("return Error.S_OK;");
			mSourceCodes.AddItem("}");
		}

		private void GenReg(int count)
		{
			mSourceCodes.AddItem("private void executeCode( Variant varthis, Variant this_proxy, Variant[] args, Variant result, Dispatch2 objthis) throws VariantException, TJSException {"
				);
			count++;
			for (int i = 0; i < count; i++)
			{
				mSourceCodes.AddItem("Variant " + GetRegisterName(i) + " = new Variant();");
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		public virtual void Generate(int start, int end, int funcbase, int framecount)
		{
			GenReg(framecount);
			// dis-assemble the intermediate code.
			// "output_func" points a line output function.
			//String  s;
			mFuncDeclUnnamedArgArrayBase = funcbase;
			string msg;
			int prevline = -1;
			int curline = -1;
			short[] ca = mCode;
			if (end <= 0)
			{
				end = ca.Length;
			}
			if (end > ca.Length)
			{
				end = ca.Length;
			}
			IntVector target = CheckJumpCode(start, end);
			int[] jumpaddr = null;
			int jumpcount = 0;
			if (target.Size() > 0)
			{
				//target.add( Integer.MAX_VALUE );
				jumpaddr = target.ToArray();
				jumpcount = jumpaddr.Length;
				// ジャンプする可能性がある
				mSourceCodes.AddItem("boolean flag = false;");
				mSourceCodes.AddItem("boolean loop = true;");
				mSourceCodes.AddItem("int goto_target = 0;");
				mSourceCodes.AddItem("do {");
				mSourceCodes.AddItem("switch(goto_target) {");
				mSourceCodes.AddItem("case 0:");
				Arrays.Sort(jumpaddr);
			}
			bool outputmask = false;
			bool forcejump = false;
			bool alreadyreturn = false;
			int[] interret = new int[1];
			int type;
			int v;
			int v1;
			Variant[] da = mData;
			for (int code = start; code < end; )
			{
				msg = null;
				int srcpos = mAccessor.CodePosToSrcPos(code);
				int line = mAccessor.SrcPosToLine(srcpos);
				// output source lines as comment
				if (curline == -1 || curline <= line)
				{
					if (curline == -1)
					{
						curline = line;
					}
					int nl = line - curline;
					while (curline <= line)
					{
						if (nl < 3 || nl >= 3 && line - curline <= 2)
						{
							//int len;
							string src = mAccessor.GetLine(curline);
							OutputFuncSrc(src, string.Empty, curline);
							curline++;
						}
						else
						{
							curline = line - 2;
						}
					}
				}
				else
				{
					if (prevline != line)
					{
						string src = mAccessor.GetLine(line);
						OutputFuncSrc(src, string.Empty, line);
					}
				}
				prevline = line;
				// decode each instructions
				for (int i = 0; i < jumpcount; i++)
				{
					if (jumpaddr[i] == code)
					{
						// ジャンプターゲットの场合、case を插入する
						int addr = i + 1;
						mSourceCodes.AddItem("\ncase " + addr + ":");
						alreadyreturn = false;
						forcejump = false;
						break;
					}
				}
				if (alreadyreturn == true || forcejump == true)
				{
					outputmask = true;
				}
				else
				{
					outputmask = false;
				}
				int op = ca[code];
				switch (op)
				{
					case VM_NOP:
					{
						code++;
						break;
					}

					case VM_CONST:
					{
						type = GetType(da[ca[code + 2]]);
						v = ca[code + 1];
						switch (type)
						{
							case TYPE_INTEGER:
							{
								msg = GetRegisterName(v) + ".set( " + da[ca[code + 2]].AsInteger() + " );";
								break;
							}

							case TYPE_REAL:
							{
								msg = GetRegisterName(v) + ".set( " + da[ca[code + 2]].AsDouble() + " );";
								break;
							}

							case TYPE_STRING:
							{
								msg = GetRegisterName(v) + ".set( \"" + da[ca[code + 2]].AsString() + "\" );";
								break;
							}

							case TYPE_VOID:
							{
								msg = GetRegisterName(v) + ".set( null );";
								break;
							}

							case TYPE_INTER_OBJECT:
							{
								object o = da[ca[code + 2]].ToJavaObject();
								VariantClosure clo = (VariantClosure)o;
								Dispatch2 dsp = clo.mObject;
								InterCodeObject obj = (InterCodeObject)dsp;
								msg = GetRegisterName(v) + ".set( (Dispatch2)new " + obj.GetName() + "Class() );";
								break;
							}

							case TYPE_INTER_GENERATOR:
							{
								InterCodeGenerator obj = (InterCodeGenerator)da[ca[code + 2]].ToJavaObject();
								msg = GetRegisterName(v) + ".set( (Dispatch2)new " + obj.GetName() + "Class() );";
								goto case TYPE_NULL_CLOSURE;
							}

							case TYPE_NULL_CLOSURE:
							{
								msg = GetRegisterName(v) + ".set( null, null );";
								break;
							}

							default:
							{
								throw new CompileException("非サポートの定数形式");
							}
						}
						code += 3;
						break;
					}

					case VM_CP:
					{
						msg = GetRegisterName(ca[code + 1]) + ".set( " + GetRegisterName(ca[code + 2]) + 
							" );";
						code += 3;
						break;
					}

					case VM_CL:
					{
						msg = GetRegisterName(ca[code + 1]) + ".clear();";
						code += 2;
						break;
					}

					case VM_CCL:
					{
						// 展开してしまう
						v = ca[code + 1];
						v1 = v + ca[code + 2];
						msg = string.Empty;
						while (v < v1)
						{
							msg += GetRegisterName(v) + ".clear();\n";
							v++;
						}
						code += 3;
						break;
					}

					case VM_TT:
					{
						msg = "flag = " + GetRegisterName(ca[code + 1]) + ".asBoolean();";
						code += 2;
						break;
					}

					case VM_TF:
					{
						msg = "flag = !" + GetRegisterName(ca[code + 1]) + ".asBoolean();";
						code += 2;
						break;
					}

					case VM_CEQ:
					{
						msg = "flag = " + GetRegisterName(ca[code + 1]) + ".normalCompare( " + GetRegisterName
							(ca[code + 2]) + " );";
						code += 3;
						break;
					}

					case VM_CDEQ:
					{
						if (ca[code + 2] == 0)
						{
							msg = "flag = " + GetRegisterName(ca[code + 1]) + ".isVoid();";
						}
						else
						{
							msg = "flag = " + GetRegisterName(ca[code + 1]) + ".discernCompare( " + GetRegisterName
								(ca[code + 2]) + " ).asBoolean();";
						}
						code += 3;
						break;
					}

					case VM_CLT:
					{
						msg = "flag = " + GetRegisterName(ca[code + 1]) + ".greaterThan( " + GetRegisterName
							(ca[code + 2]) + " );";
						code += 3;
						break;
					}

					case VM_CGT:
					{
						msg = "flag = " + GetRegisterName(ca[code + 1]) + ".littlerThan( " + GetRegisterName
							(ca[code + 2]) + " );";
						code += 3;
						break;
					}

					case VM_SETF:
					{
						msg = GetRegisterName(ca[code + 1]) + ".set(flag?1:0);";
						code += 2;
						break;
					}

					case VM_SETNF:
					{
						msg = GetRegisterName(ca[code + 1]) + ".set(flag?0:1);";
						code += 2;
						break;
					}

					case VM_LNOT:
					{
						msg = GetRegisterName(ca[code + 1]) + ".logicalnot();";
						code += 2;
						break;
					}

					case VM_NF:
					{
						msg = "flag = !flag;";
						code++;
						break;
					}

					case VM_JF:
					{
						v = FindJumpTarget(jumpaddr, (code + ca[code + 1]));
						msg = "if( flag ) {\ngoto_target = " + v + ";\nbreak;\n}";
						code += 2;
						break;
					}

					case VM_JNF:
					{
						v = FindJumpTarget(jumpaddr, (code + ca[code + 1]));
						msg = "if( !flag ) {\ngoto_target = " + v + ";\nbreak;\n}";
						code += 2;
						break;
					}

					case VM_JMP:
					{
						v = FindJumpTarget(jumpaddr, (code + ca[code + 1]));
						msg = "goto_target = " + v + ";\nbreak;";
						forcejump = true;
						code += 2;
						break;
					}

					case VM_INC:
					{
						msg = GetRegisterName(ca[code + 1]) + ".increment();";
						code += 2;
						break;
					}

					case VM_INCPD:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "int hr = clo.operation( OP_INC, \"" + da[ca[code + 3]].GetString() + "\", "
								 + GetRegisterName(ca[code + 1]) + ", null, objthis );\n";
						}
						else
						{
							msg += "int hr = clo.operation( OP_INC, \"" + da[ca[code + 3]].GetString() + "\", null, null, objthis );\n";
						}
						msg += "if( hr < 0 ) throwFrom_tjs_error( hr, \"" + da[ca[code + 3]].GetString() 
							+ "\" );\n";
						msg += "}";
						//operatePropertyDirect0(ra, ra_offset, code, OP_INC);
						code += 4;
						break;
					}

					case VM_INCPI:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						msg += "Variant name = " + GetRegisterName(ca[code + 3]) + ";\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "operatePropertyIndirect0( clo, name, " + GetRegisterName(ca[code + 1]) + 
								", objthis, OP_INC );\n";
						}
						else
						{
							msg += "operatePropertyIndirect0( clo, name, null, objthis, OP_INC );\n";
						}
						msg += "}";
						//operatePropertyIndirect0(ra, ra_offset, code, OP_INC);
						code += 4;
						break;
					}

					case VM_INCP:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "int hr = clo.operation( OP_INC, null, " + GetRegisterName(ca[code + 1]) +
								 ", null, clo.mObjThis != null ?clo.mObjThis:objthis );\n";
						}
						else
						{
							msg += "int hr = clo.operation( OP_INC, null, null, null, clo.mObjThis != null ?clo.mObjThis:objthis );\n";
						}
						msg += "if( hr < 0 ) throwFrom_tjs_error( hr, null );\n";
						msg += "}";
						//operateProperty0(ra, ra_offset, code, OP_INC);
						code += 3;
						break;
					}

					case VM_DEC:
					{
						msg = GetRegisterName(ca[code + 1]) + ".decrement();";
						code += 2;
						break;
					}

					case VM_DECPD:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "int hr = clo.operation( OP_DEC, \"" + da[ca[code + 3]].GetString() + "\", "
								 + GetRegisterName(ca[code + 1]) + ", null, objthis );\n";
						}
						else
						{
							msg += "int hr = clo.operation( OP_DEC, \"" + da[ca[code + 3]].GetString() + "\", null, null, objthis );\n";
						}
						msg += "if( hr < 0 ) throwFrom_tjs_error( hr, \"" + da[ca[code + 3]].GetString() 
							+ "\" );\n";
						msg += "}";
						//operatePropertyDirect0(ra, ra_offset, code, OP_DEC);
						code += 4;
						break;
					}

					case VM_DECPI:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						msg += "Variant name = " + GetRegisterName(ca[code + 3]) + ";\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "operatePropertyIndirect0( clo, name, " + GetRegisterName(ca[code + 1]) + 
								", objthis, OP_DEC );\n";
						}
						else
						{
							msg += "operatePropertyIndirect0( clo, name, null, objthis, OP_DEC );\n";
						}
						msg += "}";
						//operatePropertyIndirect0(ra, ra_offset, code, OP_DEC);
						code += 4;
						break;
					}

					case VM_DECP:
					{
						msg = "{\n";
						msg += "VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						if (ca[code + 1] != 0)
						{
							// result
							msg += "int hr = clo.operation( OP_DEC, null, " + GetRegisterName(ca[code + 1]) +
								 ", null, clo.mObjThis != null ?clo.mObjThis:objthis );\n";
						}
						else
						{
							msg += "int hr = clo.operation( OP_DEC, null, null, null, clo.mObjThis != null ?clo.mObjThis:objthis );\n";
						}
						msg += "if( hr < 0 ) throwFrom_tjs_error( hr, null );\n";
						msg += "}";
						//operateProperty0(ra, ra_offset, code, OP_DEC);
						code += 3;
						break;
					}

					case VM_LOR:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".logicalorequal(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_LORPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_LOR );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_LOR );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_LOR);
						code += 5;
						break;
					}

					case VM_LORPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_LOR );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_LOR );";
						}
						// operatePropertyIndirect(ra, ra_offset, code, OP_LOR );
						code += 5;
						break;
					}

					case VM_LORP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_LOR );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_LOR );";
						}
						// operateProperty(ra, ra_offset, code, OP_LOR );
						code += 4;
						break;
					}

					case VM_LAND:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".logicalandequal(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_LANDPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_LAND );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_LAND );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_LAND );
						code += 5;
						break;
					}

					case VM_LANDPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_LAND );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_LAND );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_LAND );
						code += 5;
						break;
					}

					case VM_LANDP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_LAND );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_LAND );";
						}
						//operateProperty(ra, ra_offset, code, OP_LAND );
						code += 4;
						break;
					}

					case VM_BOR:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".orEqual(" + GetRegisterName(ca[code + 2])
							 + ");";
						code += 3;
						break;
					}

					case VM_BORPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_BOR );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_BOR );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_BOR );
						code += 5;
						break;
					}

					case VM_BORPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_BOR );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_BOR );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_BOR );
						code += 5;
						break;
					}

					case VM_BORP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BOR );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BOR );";
						}
						//operateProperty(ra, ra_offset, code, OP_BOR );
						code += 4;
						break;
					}

					case VM_BXOR:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".bitXorEqual(" + GetRegisterName(ca[code +
							 2]) + ");";
						code += 3;
						break;
					}

					case VM_BXORPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_BXOR );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_BXOR );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_BXOR );
						code += 5;
						break;
					}

					case VM_BXORPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_BXOR );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_BXOR );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_BXOR );
						code += 5;
						break;
					}

					case VM_BXORP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BXOR );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BXOR );";
						}
						//operateProperty(ra, ra_offset, code, OP_BXOR );
						code += 4;
						break;
					}

					case VM_BAND:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".andEqual(" + GetRegisterName(ca[code + 2]
							) + ");";
						code += 3;
						break;
					}

					case VM_BANDPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_BAND );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_BAND );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_BAND );
						code += 5;
						break;
					}

					case VM_BANDPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_BAND );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_BAND );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_BAND );
						code += 5;
						break;
					}

					case VM_BANDP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BAND );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_BAND );";
						}
						//operateProperty(ra, ra_offset, code, OP_BAND );
						code += 4;
						break;
					}

					case VM_SAR:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".rightShiftEqual(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_SARPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_SAR );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_SAR );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_SAR );
						code += 5;
						break;
					}

					case VM_SARPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_SAR );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_SAR );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_SAR );
						code += 5;
						break;
					}

					case VM_SARP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SAR );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SAR );";
						}
						//operateProperty(ra, ra_offset, code, OP_SAR );
						code += 4;
						break;
					}

					case VM_SAL:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".leftShiftEqual(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_SALPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_SAL );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_SAL );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_SAL );
						code += 5;
						break;
					}

					case VM_SALPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_SAL );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_SAL );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_SAL );
						code += 5;
						break;
					}

					case VM_SALP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SAL );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SAL );";
						}
						//operateProperty(ra, ra_offset, code, OP_SAL );
						code += 4;
						break;
					}

					case VM_SR:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".rbitshiftequal(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_SRPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_SR );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_SR );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_SR );
						code += 5;
						break;
					}

					case VM_SRPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_SR );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_SR );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_SR );
						code += 5;
						break;
					}

					case VM_SRP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SR );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SR );";
						}
						//operateProperty(ra, ra_offset, code, OP_SR );
						code += 4;
						break;
					}

					case VM_ADD:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".addEqual(" + GetRegisterName(ca[code + 2]
							) + ");";
						code += 3;
						break;
					}

					case VM_ADDPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_ADD );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_ADD );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_ADD );
						code += 5;
						break;
					}

					case VM_ADDPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_ADD );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_ADD );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_ADD );
						code += 5;
						break;
					}

					case VM_ADDP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_ADD );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_ADD );";
						}
						//operateProperty(ra, ra_offset, code, OP_ADD );
						code += 4;
						break;
					}

					case VM_SUB:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".subtractEqual(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_SUBPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_SUB );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_SUB );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_SUB );
						code += 5;
						break;
					}

					case VM_SUBPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_SUB );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_SUB );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_SUB );
						code += 5;
						break;
					}

					case VM_SUBP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SUB );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_SUB );";
						}
						//operateProperty(ra, ra_offset, code, OP_SUB );
						code += 4;
						break;
					}

					case VM_MOD:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".residueEqual(" + GetRegisterName(ca[code 
							+ 2]) + ");";
						code += 3;
						break;
					}

					case VM_MODPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_MOD );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_MOD );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_MOD );
						code += 5;
						break;
					}

					case VM_MODPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_MOD );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_MOD );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_MOD );
						code += 5;
						break;
					}

					case VM_MODP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_MOD );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_MOD );";
						}
						//operateProperty(ra, ra_offset, code, OP_MOD );
						code += 4;
						break;
					}

					case VM_DIV:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".divideEqual(" + GetRegisterName(ca[code +
							 2]) + ");";
						code += 3;
						break;
					}

					case VM_DIVPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_DIV );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_DIV );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_DIV );
						code += 5;
						break;
					}

					case VM_DIVPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_DIV );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_DIV );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_DIV );
						code += 5;
						break;
					}

					case VM_DIVP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_DIV );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_DIV );";
						}
						//operateProperty(ra, ra_offset, code, OP_DIV );
						code += 4;
						break;
					}

					case VM_IDIV:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".idivequal(" + GetRegisterName(ca[code + 2
							]) + ");";
						code += 3;
						break;
					}

					case VM_IDIVPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_IDIV );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_IDIV );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_IDIV );
						code += 5;
						break;
					}

					case VM_IDIVPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_IDIV );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_IDIV );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_IDIV );
						code += 5;
						break;
					}

					case VM_IDIVP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_IDIV );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_IDIV );";
						}
						//operateProperty(ra, ra_offset, code, OP_IDIV );
						code += 4;
						break;
					}

					case VM_MUL:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".multiplyEqual(" + GetRegisterName(ca[code
							 + 2]) + ");";
						code += 3;
						break;
					}

					case VM_MULPD:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", " + GetRegisterName(ca[code + 1]) + ", " 
								+ GetRegisterName(ca[code + 4]) + ", objthis, OP_MUL );";
						}
						else
						{
							msg = "operatePropertyDirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), \""
								 + da[ca[code + 3]].GetString() + "\", null, " + GetRegisterName(ca[code + 4]) +
								 ", objthis, OP_MUL );";
						}
						//operatePropertyDirect(ra, ra_offset, code, OP_MUL );
						code += 5;
						break;
					}

					case VM_MULPI:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", " + GetRegisterName(ca[code + 1]) + ", " +
								 GetRegisterName(ca[code + 4]) + ", objthis, OP_MUL );";
						}
						else
						{
							msg = "operatePropertyIndirect( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 3]) + ", null, " + GetRegisterName(ca[code + 4]) + 
								", objthis, OP_MUL );";
						}
						//operatePropertyIndirect(ra, ra_offset, code, OP_MUL );
						code += 5;
						break;
					}

					case VM_MULP:
					{
						if (ca[code + 1] != 0)
						{
							// result
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, OP_MUL );";
						}
						else
						{
							msg = "operateProperty( " + GetRegisterName(ca[code + 2]) + ".asObjectClosure(), "
								 + "null, " + GetRegisterName(ca[code + 3]) + ", objthis, OP_MUL );";
						}
						//operateProperty(ra, ra_offset, code, OP_MUL );
						code += 4;
						break;
					}

					case VM_BNOT:
					{
						// TJS_DEF_VM_P
						msg = GetRegisterName(ca[code + 1]) + ".bitnot();";
						code += 2;
						break;
					}

					case VM_ASC:
					{
						msg = "characterCodeOf( " + GetRegisterName(ca[code + 1]) + " );";
						code += 2;
						break;
					}

					case VM_CHR:
					{
						msg = "characterCodeFrom( " + GetRegisterName(ca[code + 1]) + " );";
						code += 2;
						break;
					}

					case VM_NUM:
					{
						msg = GetRegisterName(ca[code + 1]) + ".tonumber();";
						code += 2;
						break;
					}

					case VM_CHS:
					{
						msg = GetRegisterName(ca[code + 1]) + ".changesign();";
						code += 2;
						break;
					}

					case VM_INV:
					{
						int offset = ca[code + 1];
						msg = "boolean tmp = " + GetRegisterName(offset) + ".isObject() == false ? false : "
							 + GetRegisterName(offset) + ".asObjectClosure().invalidate(0, null, objthis) == Error.S_TRUE;\n";
						msg += GetRegisterName(offset) + ".set(tmp?1:0);";
						code += 2;
						break;
					}

					case VM_CHKINV:
					{
						int offset = ca[code + 1];
						msg = "{\nboolean tmp;\n";
						msg += "if( " + GetRegisterName(offset) + ".isObject() == false ) {\n";
						msg += "tmp = true;\n";
						msg += "} else {\n";
						msg += "int ret = " + GetRegisterName(offset) + ".asObjectClosure().isValid(0, null, objthis );\n";
						msg += "tmp = ret == Error.S_TRUE || ret == Error.E_NOTIMPL;\n";
						msg += "}\n";
						msg += GetRegisterName(offset) + ".set(tmp?1:0);\n";
						msg += "}";
						code += 2;
						break;
					}

					case VM_INT:
					{
						msg = GetRegisterName(ca[code + 1]) + ".toInteger();";
						code += 2;
						break;
					}

					case VM_REAL:
					{
						msg = GetRegisterName(ca[code + 1]) + ".toReal();";
						code += 2;
						break;
					}

					case VM_STR:
					{
						msg = GetRegisterName(ca[code + 1]) + ".selfToString();";
						code += 2;
						break;
					}

					case VM_OCTET:
					{
						msg = GetRegisterName(ca[code + 1]) + ".toOctet();";
						code += 2;
						break;
					}

					case VM_TYPEOF:
					{
						msg = "typeOf( " + GetRegisterName(ca[code + 1]) + " );";
						code += 2;
						break;
					}

					case VM_TYPEOFD:
					{
						msg = "typeOfMemberDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetDataToStrOrNum(da[ca[code + 3]]) + ", objthis, Interface.MEMBERMUSTEXIST );";
						code += 4;
						break;
					}

					case VM_TYPEOFI:
					{
						msg = "typeOfMemberIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERMUSTEXIST );\n";
						code += 4;
						break;
					}

					case VM_EVAL:
					{
						msg = "eval( " + GetRegisterName(ca[code + 1]) + ", TJS.mEvalOperatorIsOnGlobal ? null : objthis, true);";
						code += 2;
						break;
					}

					case VM_EEXP:
					{
						msg = "eval( " + GetRegisterName(ca[code + 1]) + ", TJS.mEvalOperatorIsOnGlobal ? null : objthis, false);";
						code += 2;
						break;
					}

					case VM_CHKINS:
					{
						msg = "instanceOf( " + GetRegisterName(ca[code + 2]) + ", " + GetRegisterName(ca[
							code + 1]) + " );";
						code += 3;
						break;
					}

					case VM_CALL:
					case VM_NEW:
					{
						msg = CallFunction(ca, interret, code, 3, FUNC_NORMAL);
						code += interret[0];
						break;
					}

					case VM_CALLD:
					{
						msg = CallFunction(ca, interret, code, 4, FUNC_DIRECT);
						code += interret[0];
						//code += callFunctionDirect(ra, ra_offset, code, args );
						break;
					}

					case VM_CALLI:
					{
						msg = CallFunction(ca, interret, code, 4, FUNC_INDIRECT);
						code += interret[0];
						//code += callFunctionIndirect(ra, ra_offset, code, args );
						break;
					}

					case VM_GPD:
					{
						msg = "getPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetDataToStrOrNum(da[ca[code + 3]]) + ", objthis, 0 );";
						//getPropertyDirect(ra, ra_offset, code, 0);
						code += 4;
						break;
					}

					case VM_GPDS:
					{
						msg = "getPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetDataToStrOrNum(da[ca[code + 3]]) + ", objthis, Interface.IGNOREPROP );";
						//getPropertyDirect(ra, ra_offset, code, Interface.IGNOREPROP);
						code += 4;
						break;
					}

					case VM_SPD:
					{
						msg = "setPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetDataToStrOrNum
							(da[ca[code + 2]]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, 0 );";
						//setPropertyDirect(ra, ra_offset, code, 0);
						code += 4;
						break;
					}

					case VM_SPDE:
					{
						msg = "setPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetDataToStrOrNum
							(da[ca[code + 2]]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERENSURE );";
						//setPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE);
						code += 4;
						break;
					}

					case VM_SPDEH:
					{
						msg = "setPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetDataToStrOrNum
							(da[ca[code + 2]]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERENSURE|Interface.HIDDENMEMBER );";
						//setPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE|Interface.HIDDENMEMBER);
						code += 4;
						break;
					}

					case VM_SPDS:
					{
						msg = "setPropertyDirect( " + GetRegisterName(ca[code + 1]) + ", " + GetDataToStrOrNum
							(da[ca[code + 2]]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERENSURE|Interface.IGNOREPROP );";
						//setPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE|Interface.IGNOREPROP);
						code += 4;
						break;
					}

					case VM_GPI:
					{
						msg = "getPropertyIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, 0 );";
						//getPropertyIndirect(ra, ra_offset, code, 0);
						code += 4;
						break;
					}

					case VM_GPIS:
					{
						msg = "getPropertyIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.IGNOREPROP );";
						//getPropertyIndirect(ra, ra_offset, code, Interface.IGNOREPROP);
						code += 4;
						break;
					}

					case VM_SPI:
					{
						msg = "setPropertyIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, 0 );";
						//setPropertyIndirect(ra, ra_offset, code, 0);
						code += 4;
						break;
					}

					case VM_SPIE:
					{
						msg = "setPropertyIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERENSURE );";
						//setPropertyIndirect(ra, ra_offset, code, Interface.MEMBERENSURE);
						code += 4;
						break;
					}

					case VM_SPIS:
					{
						msg = "setPropertyIndirect( " + GetRegisterName(ca[code + 1]) + ", " + GetRegisterName
							(ca[code + 2]) + ", " + GetRegisterName(ca[code + 3]) + ", objthis, Interface.MEMBERENSURE|Interface.IGNOREPROP );";
						//setPropertyIndirect(ra, ra_offset, code, Interface.MEMBERENSURE|Interface.IGNOREPROP);
						code += 4;
						break;
					}

					case VM_GETP:
					{
						msg = "{\nVariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						msg += "int hr = clo.propGet( 0, null, " + GetRegisterName(ca[code + 1]) + ", clo.mObjThis != null ? clo.mObjThis : objthis );\n";
						msg += "if( hr < 0 ) throwFrom_tjs_error(hr, null);\n}";
						//getProperty(ra, ra_offset, code);
						code += 3;
						break;
					}

					case VM_SETP:
					{
						msg = "{\nVariantClosure clo = " + GetRegisterName(ca[code + 1]) + ".asObjectClosure();\n";
						msg += "int hr = clo.propSet(0, null, " + GetRegisterName(ca[code + 2]) + ", clo.mObjThis != null ? clo.mObjThis : objthis );\n";
						msg += "if( hr < 0 ) throwFrom_tjs_error( hr, null );\n}";
						//setProperty(ra, ra_offset, code);
						code += 3;
						break;
					}

					case VM_DELD:
					{
						msg = "{\nVariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						string name = da[ca[code + 3]].GetString();
						msg += "int hr = clo.deleteMember(0, \"" + name + "\", objthis );\n";
						if (ca[code + 1] != 0)
						{
							msg += "if( hr < 0 ) " + GetRegisterName(ca[code + 1]) + ".set(0);\n";
							msg += "else " + GetRegisterName(ca[code + 1]) + ".set(1);\n";
						}
						msg += "}";
						//deleteMemberDirect(ra, ra_offset, code);
						code += 4;
						break;
					}

					case VM_DELI:
					{
						msg = "{\nVariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n";
						msg += "final String name = " + GetRegisterName(ca[code + 3]) + ".asString();\n";
						msg += "int hr = clo.deleteMember( 0, name, clo.mObjThis != null ? clo.mObjThis : objthis );\n";
						if (ca[code + 1] != 0)
						{
							msg += "if( hr < 0 ) " + GetRegisterName(ca[code + 1]) + ".set(0);\n";
							msg += "else " + GetRegisterName(ca[code + 1]) + ".set(1);\n";
						}
						msg += "}";
						//deleteMemberIndirect(ra, ra_offset, code);
						code += 4;
						break;
					}

					case VM_SRV:
					{
						msg = "if( result != null ) result.copyRef( " + GetRegisterName(ca[code + 1]) + " );";
						code += 2;
						break;
					}

					case VM_RET:
					{
						if (alreadyreturn == false)
						{
							msg = "return;";
							// + (code + 1) + ";";
							alreadyreturn = true;
						}
						code += 1;
						break;
					}

					case VM_ENTRY:
					{
						mExceptionDataStack.AddItem(new JavaCodeGenerator.ExceptionData(ca[code + 1] + code
							, ca[code + 2]));
						msg = "try {";
						//code = executeCodeInTryBlock( ra, ra_offset, code+3, args, result, ca[code+1]+code, ca[code+2] );
						code += 3;
						break;
					}

					case VM_EXTRY:
					{
						if (mExceptionDataStack.Count > 0)
						{
							int last = mExceptionDataStack.Count - 1;
							JavaCodeGenerator.ExceptionData ex = mExceptionDataStack[last];
							mExceptionDataStack.Remove(last);
							int exobjreg = ex.mExobjReg;
							int catchip = FindJumpTarget(jumpaddr, ex.mCatchIp);
							msg = "} catch( TJSScriptException e ) {\n";
							if (exobjreg != 0)
							{
								msg += GetRegisterName(exobjreg) + ".set( e.getValue() );\n";
							}
							msg += "goto_target = " + catchip + ";\n";
							msg += "break;\n";
							msg += "} catch( TJSScriptError e ) {\n";
							if (exobjreg != 0)
							{
								msg += "Variant msg = new Variant(e.getMessage());\n";
								msg += "Variant trace = new Variant(e.getTrace());\n";
								msg += "Variant ret = new Variant();\n";
								msg += "Error.getExceptionObject( mBlock.getTJS(), ret, msg, trace );\n";
								msg += GetRegisterName(exobjreg) + ".set( ret );\n";
							}
							msg += "goto_target = " + catchip + ";\n";
							msg += "break;\n";
							msg += "} catch( TJSException e ) {\n";
							if (exobjreg != 0)
							{
								msg += "Variant msg = new Variant( e.getMessage() );\n";
								msg += "Variant ret = new Variant();\n";
								msg += "Error.getExceptionObject( mBlock.getTJS(), ret, msg, null );\n";
								msg += GetRegisterName(exobjreg) + ".set( ret );\n";
							}
							msg += "goto_target = " + catchip + ";\n";
							msg += "break;\n";
							msg += "} catch( Exception e ) {\n";
							if (exobjreg != 0)
							{
								msg += "Variant msg = new Variant( e.getMessage() );\n";
								msg += "Variant ret = new Variant();\n";
								msg += "Error.getExceptionObject( mBlock.getTJS(), ret, msg, null );\n";
								msg += GetRegisterName(exobjreg) + ".set( ret );\n";
							}
							msg += "goto_target = " + catchip + ";\n";
							msg += "break;\n";
							msg += "}\n";
						}
						code += 1;
						goto case VM_THROW;
					}

					case VM_THROW:
					{
						msg = "throwScriptException( " + GetRegisterName(ca[code + 1]) + ", mBlock, " + mAccessor
							.CodePosToSrcPos(code) + " );\n";
						code += 2;
						// actually here not proceed...
						break;
					}

					case VM_CHGTHIS:
					{
						msg = GetRegisterName(ca[code + 1]) + ".changeClosureObjThis( " + GetRegisterName
							(ca[code + 2]) + ".asObject() );";
						//ra[ra_offset+ca[code+1]].changeClosureObjThis(ra[ra_offset+ca[code+2]].asObject());
						code += 3;
						break;
					}

					case VM_GLOBAL:
					{
						msg = GetRegisterName(ca[code + 1]) + ".set( ScriptsClass.getGlobal() );";
						code += 2;
						break;
					}

					case VM_ADDCI:
					{
						msg = "addClassInstanceInfo( " + GetRegisterName(ca[code + 1]) + ".asObject(), " 
							+ GetRegisterName(ca[code + 2]) + ".asString() );";
						//addClassInstanceInfo(ra, ra_offset,code);
						code += 3;
						break;
					}

					case VM_REGMEMBER:
					{
						msg = "copyAllMembers( (CustomObject)objthis );";
						//copyAllMembers( (CustomObject)ra[ra_offset-1].asObject() );
						code++;
						break;
					}

					case VM_DEBUGGER:
					{
						code++;
						break;
					}

					default:
					{
						throw new CompileException("未定义のVMオペコードです。");
					}
				}
				if (outputmask == false)
				{
					mSourceCodes.AddItem(msg);
				}
			}
			if (target.Size() > 0)
			{
				mSourceCodes.AddItem("default: loop = false;");
				mSourceCodes.AddItem("}");
				mSourceCodes.AddItem("} while(loop);");
				mSourceCodes.AddItem("return;");
			}
			mSourceCodes.AddItem("}");
		}

		/// <exception cref="VariantException"></exception>
		public virtual IntVector CheckJumpCode(int start, int end)
		{
			IntVector ret = new IntVector();
			short[] ca = mCode;
			if (end <= 0)
			{
				end = ca.Length;
			}
			if (end > ca.Length)
			{
				end = ca.Length;
			}
			int size = 0;
			for (int i = start; i < end; )
			{
				switch (ca[i])
				{
					case VM_NOP:
					{
						size = 1;
						break;
					}

					case VM_NF:
					{
						size = 1;
						break;
					}

					case VM_CONST:
					{
						size = 3;
						break;
					}

					case VM_CP:
					{
						size = 3;
						break;
					}

					case VM_CEQ:
					{
						size = 3;
						break;
					}

					case VM_CDEQ:
					{
						size = 3;
						break;
					}

					case VM_CLT:
					{
						size = 3;
						break;
					}

					case VM_CGT:
					{
						size = 3;
						break;
					}

					case VM_CHKINS:
					{
						size = 3;
						break;
					}

					case VM_LOR:
					{
						size = 3;
						break;
					}

					case VM_LOR + 1:
					{
						size = 5;
						break;
					}

					case VM_LOR + 2:
					{
						size = 5;
						break;
					}

					case VM_LOR + 3:
					{
						size = 4;
						break;
					}

					case VM_LAND:
					{
						size = 3;
						break;
					}

					case VM_LAND + 1:
					{
						size = 5;
						break;
					}

					case VM_LAND + 2:
					{
						size = 5;
						break;
					}

					case VM_LAND + 3:
					{
						size = 4;
						break;
					}

					case VM_BOR:
					{
						size = 3;
						break;
					}

					case VM_BOR + 1:
					{
						size = 5;
						break;
					}

					case VM_BOR + 2:
					{
						size = 5;
						break;
					}

					case VM_BOR + 3:
					{
						size = 4;
						break;
					}

					case VM_BXOR:
					{
						size = 3;
						break;
					}

					case VM_BXOR + 1:
					{
						size = 5;
						break;
					}

					case VM_BXOR + 2:
					{
						size = 5;
						break;
					}

					case VM_BXOR + 3:
					{
						size = 4;
						break;
					}

					case VM_BAND:
					{
						size = 3;
						break;
					}

					case VM_BAND + 1:
					{
						size = 5;
						break;
					}

					case VM_BAND + 2:
					{
						size = 5;
						break;
					}

					case VM_BAND + 3:
					{
						size = 4;
						break;
					}

					case VM_SAR:
					{
						size = 3;
						break;
					}

					case VM_SAR + 1:
					{
						size = 5;
						break;
					}

					case VM_SAR + 2:
					{
						size = 5;
						break;
					}

					case VM_SAR + 3:
					{
						size = 4;
						break;
					}

					case VM_SAL:
					{
						size = 3;
						break;
					}

					case VM_SAL + 1:
					{
						size = 5;
						break;
					}

					case VM_SAL + 2:
					{
						size = 5;
						break;
					}

					case VM_SAL + 3:
					{
						size = 4;
						break;
					}

					case VM_SR:
					{
						size = 3;
						break;
					}

					case VM_SR + 1:
					{
						size = 5;
						break;
					}

					case VM_SR + 2:
					{
						size = 5;
						break;
					}

					case VM_SR + 3:
					{
						size = 4;
						break;
					}

					case VM_ADD:
					{
						size = 3;
						break;
					}

					case VM_ADD + 1:
					{
						size = 5;
						break;
					}

					case VM_ADD + 2:
					{
						size = 5;
						break;
					}

					case VM_ADD + 3:
					{
						size = 4;
						break;
					}

					case VM_SUB:
					{
						size = 3;
						break;
					}

					case VM_SUB + 1:
					{
						size = 5;
						break;
					}

					case VM_SUB + 2:
					{
						size = 5;
						break;
					}

					case VM_SUB + 3:
					{
						size = 4;
						break;
					}

					case VM_MOD:
					{
						size = 3;
						break;
					}

					case VM_MOD + 1:
					{
						size = 5;
						break;
					}

					case VM_MOD + 2:
					{
						size = 5;
						break;
					}

					case VM_MOD + 3:
					{
						size = 4;
						break;
					}

					case VM_DIV:
					{
						size = 3;
						break;
					}

					case VM_DIV + 1:
					{
						size = 5;
						break;
					}

					case VM_DIV + 2:
					{
						size = 5;
						break;
					}

					case VM_DIV + 3:
					{
						size = 4;
						break;
					}

					case VM_IDIV:
					{
						size = 3;
						break;
					}

					case VM_IDIV + 1:
					{
						size = 5;
						break;
					}

					case VM_IDIV + 2:
					{
						size = 5;
						break;
					}

					case VM_IDIV + 3:
					{
						size = 4;
						break;
					}

					case VM_MUL:
					{
						size = 3;
						break;
					}

					case VM_MUL + 1:
					{
						size = 5;
						break;
					}

					case VM_MUL + 2:
					{
						size = 5;
						break;
					}

					case VM_MUL + 3:
					{
						size = 4;
						break;
					}

					case VM_TT:
					{
						size = 2;
						break;
					}

					case VM_TF:
					{
						size = 2;
						break;
					}

					case VM_SETF:
					{
						size = 2;
						break;
					}

					case VM_SETNF:
					{
						size = 2;
						break;
					}

					case VM_LNOT:
					{
						size = 2;
						break;
					}

					case VM_BNOT:
					{
						size = 2;
						break;
					}

					case VM_ASC:
					{
						size = 2;
						break;
					}

					case VM_CHR:
					{
						size = 2;
						break;
					}

					case VM_NUM:
					{
						size = 2;
						break;
					}

					case VM_CHS:
					{
						size = 2;
						break;
					}

					case VM_CL:
					{
						size = 2;
						break;
					}

					case VM_INV:
					{
						size = 2;
						break;
					}

					case VM_CHKINV:
					{
						size = 2;
						break;
					}

					case VM_TYPEOF:
					{
						size = 2;
						break;
					}

					case VM_EVAL:
					{
						size = 2;
						break;
					}

					case VM_EEXP:
					{
						size = 2;
						break;
					}

					case VM_INT:
					{
						size = 2;
						break;
					}

					case VM_REAL:
					{
						size = 2;
						break;
					}

					case VM_STR:
					{
						size = 2;
						break;
					}

					case VM_OCTET:
					{
						size = 2;
						break;
					}

					case VM_CCL:
					{
						size = 3;
						break;
					}

					case VM_INC:
					{
						size = 2;
						break;
					}

					case VM_INC + 1:
					{
						size = 4;
						break;
					}

					case VM_INC + 2:
					{
						size = 4;
						break;
					}

					case VM_INC + 3:
					{
						size = 3;
						break;
					}

					case VM_DEC:
					{
						size = 2;
						break;
					}

					case VM_DEC + 1:
					{
						size = 4;
						break;
					}

					case VM_DEC + 2:
					{
						size = 4;
						break;
					}

					case VM_DEC + 3:
					{
						size = 3;
						break;
					}

					case VM_JF:
					{
						ret.Add(ca[i + 1] + i);
						size = 2;
						break;
					}

					case VM_JNF:
					{
						ret.Add(ca[i + 1] + i);
						size = 2;
						break;
					}

					case VM_JMP:
					{
						ret.Add(ca[i + 1] + i);
						size = 2;
						break;
					}

					case VM_CALL:
					case VM_CALLD:
					case VM_CALLI:
					case VM_NEW:
					{
						int st;
						// start of arguments
						if (ca[i] == VM_CALLD || ca[i] == VM_CALLI)
						{
							st = 5;
						}
						else
						{
							st = 4;
						}
						int num = ca[i + st - 1];
						// st-1 = argument count
						if (num == -1)
						{
							// omit arg
							size = st;
						}
						else
						{
							if (num == -2)
							{
								// expand arg
								st++;
								num = ca[i + st - 1];
								size = st + num * 2;
							}
							else
							{
								// normal operation
								size = st + num;
							}
						}
						break;
					}

					case VM_GPD:
					case VM_GPDS:
					{
						size = 4;
						break;
					}

					case VM_SPD:
					case VM_SPDE:
					case VM_SPDEH:
					case VM_SPDS:
					{
						size = 4;
						break;
					}

					case VM_GPI:
					case VM_GPIS:
					{
						size = 4;
						break;
					}

					case VM_SPI:
					case VM_SPIE:
					case VM_SPIS:
					{
						size = 4;
						break;
					}

					case VM_SETP:
					{
						size = 3;
						break;
					}

					case VM_GETP:
					{
						size = 3;
						break;
					}

					case VM_DELD:
					case VM_TYPEOFD:
					{
						size = 4;
						break;
					}

					case VM_DELI:
					case VM_TYPEOFI:
					{
						size = 4;
						break;
					}

					case VM_SRV:
					{
						size = 2;
						break;
					}

					case VM_RET:
					{
						size = 1;
						break;
					}

					case VM_ENTRY:
					{
						ret.Add(ca[i + 1] + i);
						// catch アドレス
						size = 3;
						break;
					}

					case VM_EXTRY:
					{
						size = 1;
						break;
					}

					case VM_THROW:
					{
						size = 2;
						break;
					}

					case VM_CHGTHIS:
					{
						size = 3;
						break;
					}

					case VM_GLOBAL:
					{
						size = 2;
						break;
					}

					case VM_ADDCI:
					{
						size = 3;
						break;
					}

					case VM_REGMEMBER:
					{
						size = 1;
						break;
					}

					case VM_DEBUGGER:
					{
						size = 1;
						break;
					}

					default:
					{
						size = 1;
						break;
					}
				}
				i += size;
			}
			return ret;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="CompileException"></exception>
		private string CallFunction(short[] ca, int[] ret, int code, int offset, int style
			)
		{
			// function calling / create new object
			int code_offset = code + offset;
			int pass_args_count = ca[code_offset];
			if (pass_args_count == -1)
			{
				// ... の时、arg をそのまま渡す
				ret[0] = 1 + offset;
				switch (style)
				{
					case FUNC_NORMAL:
					{
						return CallFunctionInternalString(ca, code, "args");
					}

					case FUNC_INDIRECT:
					{
						return CallFunctionIndirectInternalString(ca, code, "args");
					}

					case FUNC_DIRECT:
					{
						return CallFunctionDirectInternalString(ca, code, "args");
					}
				}
				throw new CompileException(Error.InternalError);
			}
			else
			{
				if (pass_args_count == -2)
				{
					// 全引数の数をカウント
					int arg_written_count = ca[code_offset + 1];
					ret[0] = arg_written_count * 2 + 2 + offset;
					StringBuilder builder = new StringBuilder();
					builder.Append("{\n");
					builder.Append("int args_v_count = 0;\n");
					builder.Append("int pass_args_count = 0;\n");
					for (int i = 0; i < arg_written_count; i++)
					{
						switch (ca[code_offset + i * 2 + 2])
						{
							case fatNormal:
							{
								builder.Append("pass_args_count++;\n");
								break;
							}

							case fatExpand:
							{
								builder.Append("args_v_count += ");
								builder.Append("ArrayClass.getArrayElementCount( " + GetRegisterName(ca[code_offset
									 + i * 2 + 1 + 2]) + ".asObject();\n");
								break;
							}

							case fatUnnamedExpand:
							{
								builder.Append("pass_args_count += ");
								builder.Append("(args.length > " + mFuncDeclUnnamedArgArrayBase + ") ? (args.length - "
									 + mFuncDeclUnnamedArgArrayBase + ") : 0;\n");
								break;
							}
						}
					}
					builder.Append("pass_args_count += args_v_count;\n");
					// Array 用のテンポラリ配列を确保する
					builder.Append("Variant[] pass_args_v = new Variant[args_v_count];\n");
					// 实际の引数配列を确保する
					builder.Append("pass_args = new Variant[pass_args_count];\n");
					// 实际の引数配列に值(参照)を入れていく
					builder.Append("args_v_count = 0;\n");
					builder.Append("pass_args_count = 0;\n");
					for (int i_1 = 0; i_1 < arg_written_count; i_1++)
					{
						switch (ca[code_offset + i_1 * 2 + 2])
						{
							case fatNormal:
							{
								builder.Append("pass_args[pass_args_count++] = " + GetRegisterName(ca[code_offset
									 + i_1 * 2 + 1 + 2]) + ";");
								break;
							}

							case fatExpand:
							{
								builder.Append("int count = ArrayClass.copyArrayElementTo( " + GetRegisterName(ca
									[code_offset + i_1 * 2 + 1 + 2]) + ".asObject(), pass_args_v, args_v_count, 0, -1);\n"
									);
								builder.Append("for( int j = 0; j < count; j++ ) {\n");
								builder.Append("pass_args[pass_args_count++] = pass_args_v[j + args_v_count];\n");
								builder.Append("}\n");
								builder.Append("args_v_count += count;\n");
								break;
							}

							case fatUnnamedExpand:
							{
								builder.Append("int count = (args.length > " + mFuncDeclUnnamedArgArrayBase + ") ? (args.length - "
									 + mFuncDeclUnnamedArgArrayBase + ") : 0;\n");
								builder.Append("for( int j = 0; j < count; j++ ) {\n");
								builder.Append("pass_args[pass_args_count++] = args[" + mFuncDeclUnnamedArgArrayBase
									 + " + j];\n");
								builder.Append("}\n");
								break;
							}
						}
					}
					switch (style)
					{
						case FUNC_NORMAL:
						{
							builder.Append(CallFunctionInternalString(ca, code, "pass_args"));
							break;
						}

						case FUNC_INDIRECT:
						{
							builder.Append(CallFunctionIndirectInternalString(ca, code, "pass_args"));
							break;
						}

						case FUNC_DIRECT:
						{
							builder.Append(CallFunctionDirectInternalString(ca, code, "pass_args"));
							break;
						}
					}
					builder.Append("\npass_args_v = null;\n");
					builder.Append("}");
					return builder.ToString();
				}
				else
				{
					// 通常の引数を持つ关数呼び出し
					ret[0] = pass_args_count + 1 + offset;
					StringBuilder builder = new StringBuilder();
					builder.Append("{\n");
					string arg_name = "TJS.NULL_ARG";
					if (pass_args_count > 0)
					{
						builder.Append("Variant[] pass_args = new Variant[" + pass_args_count + "];\n");
						arg_name = "pass_args";
					}
					for (int i = 0; i < pass_args_count; i++)
					{
						builder.Append("pass_args[" + i + "] = " + GetRegisterName(ca[code_offset + 1 + i
							]) + ";\n");
					}
					switch (style)
					{
						case FUNC_NORMAL:
						{
							builder.Append(CallFunctionInternalString(ca, code, arg_name));
							break;
						}

						case FUNC_INDIRECT:
						{
							builder.Append(CallFunctionIndirectInternalString(ca, code, arg_name));
							break;
						}

						case FUNC_DIRECT:
						{
							builder.Append(CallFunctionDirectInternalString(ca, code, arg_name));
							break;
						}
					}
					builder.Append("\n}");
					return builder.ToString();
				}
			}
		}

		private string CallFunctionInternalString(short[] ca, int code, string pass_args)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{\n");
			builder.Append("VariantClosure clo = " + GetRegisterName(ca[code + 2]) + ".asObjectClosure();\n"
				);
			int offset = ca[code + 1];
			int op = ca[code];
			if (op == VM_CALL)
			{
				if (offset != 0)
				{
					builder.Append("int hr = clo.funcCall(0, null, " + GetRegisterName(offset) + ", "
						 + pass_args + "," + "clo.mObjThis != null ?clo.mObjThis:objthis);\n");
				}
				else
				{
					builder.Append("int hr = clo.funcCall(0, null, null, " + pass_args + "," + "clo.mObjThis != null ?clo.mObjThis:objthis);\n"
						);
				}
			}
			else
			{
				builder.Append("Holder<Dispatch2> dsp = new Holder<Dispatch2>(null);\n");
				builder.Append("int hr = clo.createNew(0, null, dsp, " + pass_args + ", clo.mObjThis != null ?clo.mObjThis:objthis);\n"
					);
				if (offset != 0)
				{
					builder.Append("if( hr >= 0 ) {\n");
					builder.Append("if( dsp.mValue != null  ) {\n");
					builder.Append(GetRegisterName(offset) + ".set(dsp.mValue, dsp.mValue);\n");
					builder.Append("}\n");
					builder.Append("}\n");
				}
			}
			builder.Append("if( hr < 0 ) throwFrom_tjs_error(hr, \"\" );\n");
			builder.Append("}");
			return builder.ToString();
		}

		/// <exception cref="VariantException"></exception>
		private string CallFunctionDirectInternalString(short[] ca, int code, string pass_args
			)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{\n");
			builder.Append("int hr;\n");
			string name = mData[ca[code + 3]].GetString();
			int offset = ca[code + 1];
			string ra_code2 = GetRegisterName(ca[code + 2]);
			builder.Append("if( " + ra_code2 + ".isObject() ) {\n");
			builder.Append("VariantClosure clo = " + ra_code2 + ".asObjectClosure();\n");
			if (offset != 0)
			{
				builder.Append("hr = clo.funcCall(0, \"" + name + "\", " + GetRegisterName(offset
					) + ", ");
				builder.Append(pass_args + ", clo.mObjThis != null ?clo.mObjThis:objthis);\n");
			}
			else
			{
				builder.Append("hr = clo.funcCall(0, \"" + name + "\", null, ");
				builder.Append(pass_args + ", clo.mObjThis != null ?clo.mObjThis:objthis);\n");
			}
			if (IsStringFunctionName(name))
			{
				// 呼び出し名が文字列用关数で无い时は出力をスキップ
				builder.Append("} else if( " + ra_code2 + ".isString() ) {\n");
				builder.Append("processStringFunction( \"" + name + "\", " + ra_code2 + ".asString(),"
					 + pass_args + ", ");
				if (offset != 0)
				{
					builder.Append(GetRegisterName(offset));
				}
				else
				{
					builder.Append("null");
				}
				builder.Append(");\n");
				builder.Append("hr = Error.S_OK;\n");
			}
			builder.Append("} else {\n");
			builder.Append("String mes = Error.VariantConvertErrorToObject.replace( \"%1\", Utils.VariantToReadableString("
				 + ra_code2 + ") );\n");
			builder.Append("throw new VariantException( mes );\n");
			builder.Append("}\n");
			builder.Append("if( hr < 0 ) throwFrom_tjs_error(hr, \"" + name + "\" );\n");
			builder.Append("}");
			return builder.ToString();
		}

		private string CallFunctionIndirectInternalString(short[] ca, int code, string pass_args
			)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{\n");
			builder.Append("int hr;\n");
			string ra_code2 = GetRegisterName(ca[code + 2]);
			string name = GetRegisterName(ca[code + 3]) + ".asString()";
			int offset = ca[code + 1];
			builder.Append("if( " + ra_code2 + ".isObject() ) {\n");
			builder.Append("VariantClosure clo = " + ra_code2 + ".asObjectClosure();\n");
			builder.Append("hr = clo.funcCall(0, " + name + ", ");
			if (offset != 0)
			{
				builder.Append(GetRegisterName(offset));
			}
			else
			{
				builder.Append("null");
			}
			builder.Append(pass_args);
			builder.Append(", clo.mObjThis != null ? clo.mObjThis:objthis);\n");
			if (IsStringFunctionName(name))
			{
				// 呼び出し名が文字列用关数で无い时は出力をスキップ
				builder.Append("} else if( " + ra_code2 + ".isString() ) {\n");
				builder.Append("processStringFunction( " + name + ", " + ra_code2 + ".asString(),"
					);
				builder.Append(pass_args);
				builder.Append(", ");
				if (offset != 0)
				{
					builder.Append(GetRegisterName(offset));
				}
				else
				{
					builder.Append("null");
				}
				builder.Append(");\n");
				builder.Append("hr = Error.S_OK;\n");
			}
			builder.Append("} else {\n");
			builder.Append("String mes = Error.VariantConvertErrorToObject.replace( \"%1\", Utils.VariantToReadableString("
				 + ra_code2 + ") );\n");
			builder.Append("throw new VariantException( mes );\n");
			builder.Append("}\n");
			builder.Append("if( hr < 0 ) throwFrom_tjs_error(hr, \"\" );\n");
			builder.Append("}");
			return builder.ToString();
		}

		public virtual AList<string> GetSourceCode()
		{
			return mSourceCodes;
		}

		private static bool IsStringFunctionName(string name)
		{
			if (name == null)
			{
				return false;
			}
			if (name.Length == 0)
			{
				return false;
			}
			int count = STR_FUNC.Length;
			for (int i = 0; i < count; i++)
			{
				if (name.Equals(STR_FUNC[i]))
				{
					return true;
				}
			}
			return false;
		}

		private const int VM_NOP = 0;

		private const int VM_CONST = 1;

		private const int VM_CP = 2;

		private const int VM_CL = 3;

		private const int VM_CCL = 4;

		private const int VM_TT = 5;

		private const int VM_TF = 6;

		private const int VM_CEQ = 7;

		private const int VM_CDEQ = 8;

		private const int VM_CLT = 9;

		private const int VM_CGT = 10;

		private const int VM_SETF = 11;

		private const int VM_SETNF = 12;

		private const int VM_LNOT = 13;

		private const int VM_NF = 14;

		private const int VM_JF = 15;

		private const int VM_JNF = 16;

		private const int VM_JMP = 17;

		private const int VM_INC = 18;

		private const int VM_INCPD = 19;

		private const int VM_INCPI = 20;

		private const int VM_INCP = 21;

		private const int VM_DEC = 22;

		private const int VM_DECPD = 23;

		private const int VM_DECPI = 24;

		private const int VM_DECP = 25;

		private const int VM_LOR = 26;

		private const int VM_LORPD = 27;

		private const int VM_LORPI = 28;

		private const int VM_LORP = 29;

		private const int VM_LAND = 30;

		private const int VM_LANDPD = 31;

		private const int VM_LANDPI = 32;

		private const int VM_LANDP = 33;

		private const int VM_BOR = 34;

		private const int VM_BORPD = 35;

		private const int VM_BORPI = 36;

		private const int VM_BORP = 37;

		private const int VM_BXOR = 38;

		private const int VM_BXORPD = 39;

		private const int VM_BXORPI = 40;

		private const int VM_BXORP = 41;

		private const int VM_BAND = 42;

		private const int VM_BANDPD = 43;

		private const int VM_BANDPI = 44;

		private const int VM_BANDP = 45;

		private const int VM_SAR = 46;

		private const int VM_SARPD = 47;

		private const int VM_SARPI = 48;

		private const int VM_SARP = 49;

		private const int VM_SAL = 50;

		private const int VM_SALPD = 51;

		private const int VM_SALPI = 52;

		private const int VM_SALP = 53;

		private const int VM_SR = 54;

		private const int VM_SRPD = 55;

		private const int VM_SRPI = 56;

		private const int VM_SRP = 57;

		private const int VM_ADD = 58;

		private const int VM_ADDPD = 59;

		private const int VM_ADDPI = 60;

		private const int VM_ADDP = 61;

		private const int VM_SUB = 62;

		private const int VM_SUBPD = 63;

		private const int VM_SUBPI = 64;

		private const int VM_SUBP = 65;

		private const int VM_MOD = 66;

		private const int VM_MODPD = 67;

		private const int VM_MODPI = 68;

		private const int VM_MODP = 69;

		private const int VM_DIV = 70;

		private const int VM_DIVPD = 71;

		private const int VM_DIVPI = 72;

		private const int VM_DIVP = 73;

		private const int VM_IDIV = 74;

		private const int VM_IDIVPD = 75;

		private const int VM_IDIVPI = 76;

		private const int VM_IDIVP = 77;

		private const int VM_MUL = 78;

		private const int VM_MULPD = 79;

		private const int VM_MULPI = 80;

		private const int VM_MULP = 81;

		private const int VM_BNOT = 82;

		private const int VM_TYPEOF = 83;

		private const int VM_TYPEOFD = 84;

		private const int VM_TYPEOFI = 85;

		private const int VM_EVAL = 86;

		private const int VM_EEXP = 87;

		private const int VM_CHKINS = 88;

		private const int VM_ASC = 89;

		private const int VM_CHR = 90;

		private const int VM_NUM = 91;

		private const int VM_CHS = 92;

		private const int VM_INV = 93;

		private const int VM_CHKINV = 94;

		private const int VM_INT = 95;

		private const int VM_REAL = 96;

		private const int VM_STR = 97;

		private const int VM_OCTET = 98;

		private const int VM_CALL = 99;

		private const int VM_CALLD = 100;

		private const int VM_CALLI = 101;

		private const int VM_NEW = 102;

		private const int VM_GPD = 103;

		private const int VM_SPD = 104;

		private const int VM_SPDE = 105;

		private const int VM_SPDEH = 106;

		private const int VM_GPI = 107;

		private const int VM_SPI = 108;

		private const int VM_SPIE = 109;

		private const int VM_GPDS = 110;

		private const int VM_SPDS = 111;

		private const int VM_GPIS = 112;

		private const int VM_SPIS = 113;

		private const int VM_SETP = 114;

		private const int VM_GETP = 115;

		private const int VM_DELD = 116;

		private const int VM_DELI = 117;

		private const int VM_SRV = 118;

		private const int VM_RET = 119;

		private const int VM_ENTRY = 120;

		private const int VM_EXTRY = 121;

		private const int VM_THROW = 122;

		private const int VM_CHGTHIS = 123;

		private const int VM_GLOBAL = 124;

		private const int VM_ADDCI = 125;

		private const int VM_REGMEMBER = 126;

		private const int VM_DEBUGGER = 127;

		private const int fatNormal = 0;

		private const int fatExpand = 1;

		private const int fatUnnamedExpand = 2;

		private const int FUNC_NORMAL = 0;

		private const int FUNC_INDIRECT = 1;

		private const int FUNC_DIRECT = 2;

		private static readonly string[] STR_FUNC = new string[] { "charAt", "indexOf", "toUpperCase"
			, "toLowerCase", "substring", "substr", "sprintf", "replace", "escape", "split", 
			"trim", "reverse", "repeat" };
		// VMCodes
		//__VM_LAST	= 128;
		// FuncArgType
		// 关数呼び出しスタイル
	}
}

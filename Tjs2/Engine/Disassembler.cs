/*
 * TJS2 CSharp
 * 
 * A really workable Disassembler rather than that stupid Sharpen one.
 * My hands broke after refactored this. ---Ulysses
 */

namespace Tjs2.Engine
{
	/// <summary>
	/// ディスアセンブラ
	/// TJS2 のバイトコードを可读可能なアセンブリソースに变换する
	/// </summary>
	public class Disassembler
	{
		private short[] mCode;

		private Variant[] mData;

		private SourceCodeAccessor mAccessor;

		public Disassembler(short[] ca, Variant[] da, SourceCodeAccessor a)
		{
			// バイナリの固まりのバイトコードからも取得できるように、クラスでラップした方がいいかも
			mCode = ca;
			mData = da;
			mAccessor = a;
		}

		public virtual void Set(short[] ca, Variant[] da, SourceCodeAccessor a)
		{
			mCode = ca;
			mData = da;
			mAccessor = a;
		}

		public virtual void Clear()
		{
			mCode = null;
			mData = null;
			mAccessor = null;
		}

		private void OutputFuncSrc(string msg, string name, int line, ScriptBlock data)
		{
            string buf;
            if (line >= 0)
            {
                //buf = string.Format("#%s(%d) %s", name, line + 1, msg);
                buf = $"#{name}({line + 1}) {msg}";
            }
            else
            {
                //buf = string.Format("#%s %s", name, msg);
                buf = $"#{name} {msg}";
            }
            Tjs.OutputToConsole(buf);
        }

		private void OutputFunc(string msg, string comment, int addr, int codestart, int 
			size, ScriptBlock data)
		{
            //string buf = string.Format("%08d %s", addr, msg);
            string buf = $"{addr:D8} {msg}";
            if (comment != null)
            {
                buf += "\t// " + comment;
            }
            Tjs.OutputToConsole(buf);
        }

		/// <exception cref="VariantException"></exception>
        public virtual void Disassemble(ScriptBlock data, int start, int end)
        {
            // dis-assemble the intermediate code.
            // "output_func" points a line output function.
            //String  s;
            string msg;
            string com;
            int prevline = -1;
            int curline = -1;
            if (end <= 0)
            {
                end = mCode.Length;
            }
            if (end > mCode.Length)
            {
                end = mCode.Length;
            }
            for (int i = start; i < end;)
            {
                msg = null;
                com = null;
                int size;
                int srcpos = mAccessor.CodePosToSrcPos(i);
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
                            OutputFuncSrc(src, string.Empty, curline, data);
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
                        OutputFuncSrc(src, string.Empty, line, data);
                    }
                }
                prevline = line;
                switch (mCode[i])
                {
                    case VM_NOP:
                        {
                            // decode each instructions
                            msg = "nop";
                            size = 1;
                            break;
                        }

                    case VM_NF:
                        {
                            msg = "nf";
                            size = 1;
                            break;
                        }

                    case VM_CONST:
                        {
                            //msg = string.Format("const %%%d, *%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"const %{mCode[i + 1]}, *{mCode[i + 2]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 2], Utils.VariantToReadableString(mData
                                //    [mCode[i + 2]]));
                                com = $"*{mCode[i + 2]} = {Utils.VariantToReadableString(mData[mCode[i + 2]])}";
                            }
                            size = 3;
                            break;
                        }

                    case VM_CP:
                        {
                            // instructions that
                            // 1. have two operands that represent registers.
                            // 2. do not have property access variants.
                            //msg = string.Format("cp %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"cp %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_CEQ:
                        {
                            //msg = string.Format("ceq %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"ceq %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_CDEQ:
                        {
                            //msg = string.Format("cdeq %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"cdeq %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_CLT:
                        {
                            //msg = string.Format("clt %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"clt %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_CGT:
                        {
                            //msg = string.Format("cgt %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"cgt %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_CHKINS:
                        {
                            //msg = string.Format("chkins %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"chkins %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_LOR:
                        {
                            // instructions that
                            // 1. have two operands that represent registers.
                            // 2. have property access variants
                            //msg = string.Format("lor %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"lor %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_LORPD:
                        {
                            //msg = string.Format("lorpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"lorpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_LORPI:
                        {
                            //msg = string.Format("lorpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"lorpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]} %{mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_LORP:
                        {
                            //msg = string.Format("lorp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"lorp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_LAND:
                        {
                            // OP2_DISASM
                            //msg = string.Format("land %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"land %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_LANDPD:
                        {
                            //msg = string.Format("landpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"landpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_LANDPI:
                        {
                            //msg = string.Format("landpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"landpi %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_LANDP:
                        {
                            //msg = string.Format("landp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //     + 3]);
                            msg = $"landp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_BOR:
                        {
                            // OP2_DISASM
                            //msg = string.Format("bor %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"bor %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_BORPD:
                        {
                            //msg = string.Format("borpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"borpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_BORPI:
                        {
                            //msg = string.Format("borpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"borpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_BORP:
                        {
                            //msg = string.Format("borp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"borp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_BXOR:
                        {
                            // OP2_DISASM
                            //msg = string.Format("bxor %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"bxor %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_BXORPD:
                        {
                            //msg = string.Format("bxorpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = msg = $"bxorpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_BXORPI:
                        {
                            //msg = string.Format("bxorpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"bxorpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_BXORP:
                        {
                            //msg = string.Format("bxorp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //     + 3]);
                            msg = $"bxorp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_BAND:
                        {
                            // OP2_DISASM
                            //msg = string.Format("band %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"band %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_BANDPD:
                        {
                            //msg = string.Format("bandpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"bandpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_BANDPI:
                        {
                            //msg = string.Format("bandpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"bandpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_BANDP:
                        {
                            //msg = string.Format("bandp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //     + 3]);
                            msg = $"bandp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SAR:
                        {
                            // OP2_DISASM
                            //msg = string.Format("sar %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"sar %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_SARPD:
                        {
                            //msg = string.Format("sarpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"sarpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_SARPI:
                        {
                            //msg = string.Format("sarpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"sarpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_SARP:
                        {
                            //msg = string.Format("sarp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"sarp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SAL:
                        {
                            // OP2_DISASM
                            //msg = string.Format("sal %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"sal %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_SALPD:
                        {
                            //msg = string.Format("salpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"salpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_SALPI:
                        {
                            //msg = string.Format("salpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"salpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_SALP:
                        {
                            //msg = string.Format("salp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"salp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SR:
                        {
                            // OP2_DISASM
                            //msg = string.Format("sr %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"sr %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_SRPD:
                        {
                            //msg = string.Format("srpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"srpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_SRPI:
                        {
                            //msg = string.Format("srpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"srpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_SRP:
                        {
                            //msg = string.Format("srp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i +
                            //     3]);
                            msg = $"srp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_ADD:
                        {
                            // OP2_DISASM
                            //msg = string.Format("add %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"add %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_ADDPD:
                        {
                            //msg = string.Format("addpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"addpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_ADDPI:
                        {
                            //msg = string.Format("addpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"addpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_ADDP:
                        {
                            //msg = string.Format("addp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"addp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SUB:
                        {
                            // OP2_DISASM
                            //msg = string.Format("sub %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"sub %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_SUBPD:
                        {
                            //msg = string.Format("subpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"subpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_SUBPI:
                        {
                            //msg = string.Format("subpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"subpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_SUBP:
                        {
                            //msg = string.Format("subp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"subp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_MOD:
                        {
                            // OP2_DISASM
                            //msg = string.Format("mod %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"mod %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_MODPD:
                        {
                            //msg = string.Format("modpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"modpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_MODPI:
                        {
                            //msg = string.Format("modpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"modpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_MODP:
                        {
                            //msg = string.Format("modp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"modp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_DIV:
                        {
                            // OP2_DISASM
                            //msg = string.Format("div %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"div %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_DIVPD:
                        {
                            //msg = string.Format("divpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"divpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_DIVPI:
                        {
                            //msg = string.Format("divpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"divpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_DIVP:
                        {
                            //msg = string.Format("divp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"divp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_IDIV:
                        {
                            // OP2_DISASM
                            //msg = string.Format("idiv %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"idiv %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_IDIVPD:
                        {
                            //msg = string.Format("idivpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"idivpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_IDIVPI:
                        {
                            //msg = string.Format("idivpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"idivpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_IDIVP:
                        {
                            //msg = string.Format("idivp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //     + 3]);
                            msg = $"idivp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_MUL:
                        {
                            // OP2_DISASM
                            //msg = string.Format("mul %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"mul %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_MULPD:
                        {
                            //msg = string.Format("mulpd %%%d, %%%d.*%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"mulpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}, %{mCode[i + 4]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 5;
                            break;
                        }

                    case VM_MULPI:
                        {
                            //msg = string.Format("mulpi %%%d, %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode
                            //    [i + 3], mCode[i + 4]);
                            msg = $"mulpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}, {mCode[i + 4]}";
                            size = 5;
                            break;
                        }

                    case VM_MULP:
                        {
                            //msg = string.Format("mulp %%%d, %%%d, %%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"mulp %{mCode[i + 1]}, %{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_TT:
                        {
                            // OP2_DISASM
                            // instructions that have one operand which represent a register,
                            // except for inc, dec
                            //msg = string.Format("tt %%%d", mCode[i + 1]);
                            msg = $"tt %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_TF:
                        {
                            //msg = string.Format("tf %%%d", mCode[i + 1]);
                            msg = $"tf %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_SETF:
                        {
                            //msg = string.Format("setf %%%d", mCode[i + 1]);
                            msg = $"setf %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_SETNF:
                        {
                            //msg = string.Format("setnf %%%d", mCode[i + 1]);
                            msg = $"setnf %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_LNOT:
                        {
                            //msg = string.Format("lnot %%%d", mCode[i + 1]);
                            msg = $"lnot %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_BNOT:
                        {
                            //msg = string.Format("bnot %%%d", mCode[i + 1]);
                            msg = $"bnot %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_ASC:
                        {
                            //msg = string.Format("asc %%%d", mCode[i + 1]);
                            msg = $"asc %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CHR:
                        {
                            //msg = string.Format("chr %%%d", mCode[i + 1]);
                            msg = $"chr %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_NUM:
                        {
                            //msg = string.Format("num %%%d", mCode[i + 1]);
                            msg = $"num %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CHS:
                        {
                            //msg = string.Format("chs %%%d", mCode[i + 1]);
                            msg = $"chs %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CL:
                        {
                            //msg = string.Format("cl %%%d", mCode[i + 1]);
                            msg = $"cl %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_INV:
                        {
                            //msg = string.Format("inv %%%d", mCode[i + 1]);
                            msg = $"inv %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CHKINV:
                        {
                            //msg = string.Format("chkinv %%%d", mCode[i + 1]);
                            msg = $"chkinv %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_TYPEOF:
                        {
                            //msg = string.Format("typeof %%%d", mCode[i + 1]);
                            msg = $"typeof %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_EVAL:
                        {
                            //msg = string.Format("eval %%%d", mCode[i + 1]);
                            msg = $"eval %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_EEXP:
                        {
                            //msg = string.Format("eexp %%%d", mCode[i + 1]);
                            msg = $"eexp %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_INT:
                        {
                            //msg = string.Format("int %%%d", mCode[i + 1]);
                            msg = $"int %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_REAL:
                        {
                            //msg = string.Format("real %%%d", mCode[i + 1]);
                            msg = $"real %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_STR:
                        {
                            //msg = string.Format("str %%%d", mCode[i + 1]);
                            msg = $"str %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_OCTET:
                        {
                            //msg = string.Format("octet %%%d", mCode[i + 1]);
                            msg = $"octet %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CCL:
                        {
                            //msg = string.Format("ccl %%%d-%%%d", mCode[i + 1], mCode[i + 1] + mCode[i + 2] -
                            //    1);
                            msg = $"ccl %{mCode[i + 1]}-%{mCode[i + 1] + mCode[i + 2] - 1}"; //range
                            size = 3;
                            break;
                        }

                    case VM_INC:
                        {
                            // inc and dec
                            //msg = string.Format("inc %%%d", mCode[i + 1]);
                            msg = $"inc %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_INCPD:
                        {
                            //msg = string.Format("incpd %%%d, %%%d.*%d", mCode[i + 1], mCode[i + 2], mCode[i +
                            //     3]);
                            msg = $"incpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 4;
                            break;
                        }

                    case VM_INCPI:
                        {
                            //msg = string.Format("incpi %%%d, %%%d.%%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"incpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_INCP:
                        {
                            //msg = string.Format("incp %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"incp %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_DEC:
                        {
                            // inc and dec
                            //msg = string.Format("dec %%%d", mCode[i + 1]);
                            msg = $"dec %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_DECPD:
                        {
                            //msg = string.Format("decpd %%%d, %%%d.*%d", mCode[i + 1], mCode[i + 2], mCode[i +
                            //     3]);
                            msg = $"decpd %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 4;
                            break;
                        }

                    case VM_DECPI:
                        {
                            //msg = string.Format("decpi %%%d, %%%d.%%%d", mCode[i + 1], mCode[i + 2], mCode[i
                            //    + 3]);
                            msg = $"decpi %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_DECP:
                        {
                            //msg = string.Format("decp %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"decp %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_JF:
                        {
                            // instructions that have one operand which represents code area
                            //msg = string.Format("jf %09d", mCode[i + 1] + i);
                            msg = $"jf {mCode[i + 1] + i:D9}";
                            size = 2;
                            break;
                        }

                    case VM_JNF:
                        {
                            //msg = string.Format("jnf %09d", mCode[i + 1] + i);
                            msg = $"jnf {mCode[i + 1] + i:D9}";
                            size = 2;
                            break;
                        }

                    case VM_JMP:
                        {
                            //msg = string.Format("jmp %09d", mCode[i + 1] + i);
                            msg = $"jmp {mCode[i + 1] + i:D9}";
                            size = 2;
                            break;
                        }

                    case VM_CALL:
                    case VM_CALLD:
                    case VM_CALLI:
                    case VM_NEW:
                        {
                            // function call variants
                            //msg = string.Format(mCode[i] == VM_CALL ? "call %%%d, %%%d(" : mCode[i] == VM_CALLD
                            //     ? "calld %%%d, %%%d.*%d(" : mCode[i] == VM_CALLI ? "calli %%%d, %%%d.%%%d(" : "new %%%d, %%%d("
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            if (mCode[i] == VM_CALL)
                            {
                                msg = $"call %{mCode[i + 1]}, %{mCode[i + 2]}(";
                            }
                            else if (mCode[i] == VM_CALLD)
                            {
                                msg = $"calld %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}(";
                            }
                            else if (mCode[i] == VM_CALLI)
                            {
                                msg = $"calli %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}(";
                            }
                            else
                            {
                                msg = $"new %{mCode[i + 1]}, %{mCode[i + 2]}(";
                            }
                            int st;
                            // start of arguments
                            if (mCode[i] == VM_CALLD || mCode[i] == VM_CALLI)
                            {
                                st = 5;
                            }
                            else
                            {
                                st = 4;
                            }
                            int num = mCode[i + st - 1];
                            // st-1 = argument count
                            bool first = true;
                            string buf = null;
                            int c = 0;
                            if (num == -1)
                            {
                                // omit arg
                                size = st;
                                msg += "...";
                            }
                            else
                            {
                                if (num == -2)
                                {
                                    // expand arg
                                    st++;
                                    num = mCode[i + st - 1];
                                    size = st + num * 2;
                                    for (int j = 0; j < num; j++)
                                    {
                                        if (!first)
                                        {
                                            msg += ", ";
                                        }
                                        first = false;
                                        switch (mCode[i + st + j * 2])
                                        {
                                            case fatNormal:
                                                {
                                                    //buf = string.Format("%%%d", mCode[i + st + j * 2 + 1]);
                                                    buf = $"%{mCode[i + st + j * 2 + 1]}";
                                                    break;
                                                }

                                            case fatExpand:
                                                {
                                                    //buf = string.Format("%%%d*", mCode[i + st + j * 2 + 1]);
                                                    buf = $"%{mCode[i + st + j * 2 + 1]}*";
                                                    break;
                                                }

                                            case fatUnnamedExpand:
                                                {
                                                    buf = "*";
                                                    break;
                                                }
                                        }
                                        msg += buf;
                                    }
                                }
                                else
                                {
                                    // normal operation
                                    size = st + num;
                                    while (num > 0)
                                    {
                                        if (!first)
                                        {
                                            msg += ", ";
                                        }
                                        first = false;
                                        //buf = string.Format("%%%d", mCode[i + c + st]);
                                        buf = $"%{mCode[i + c + st]}";
                                        c++;
                                        msg += buf;
                                        num--;
                                    }
                                }
                            }
                            msg += ")";
                            if (mData != null && mCode[i] == VM_CALLD)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            break;
                        }

                    case VM_GPD:
                    case VM_GPDS:
                        {
                            // property get direct
                            //msg = string.Format(mCode[i] == VM_GPD ? "gpd %%%d, %%%d.*%d" : "gpds %%%d, %%%d.*%d"
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            msg = mCode[i] == VM_GPD ? "gpd" : "gpds";
                            msg += $" %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 4;
                            break;
                        }

                    case VM_SPD:
                    case VM_SPDE:
                    case VM_SPDEH:
                    case VM_SPDS:
                        {
                            // property set direct
                            //msg = string.Format(mCode[i] == VM_SPD ? "spd %%%d.*%d, %%%d" : mCode[i] == VM_SPDE
                            //     ? "spde %%%d.*%d, %%%d" : mCode[i] == VM_SPDEH ? "spdeh %%%d.*%d, %%%d" : "spds %%%d.*%d, %%%d"
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            msg = mCode[i] == VM_SPD ? "spd" : mCode[i] == VM_SPDE ? "spde" : mCode[i] == VM_SPDEH ? "spdeh" : "spds";
                            msg += $" %{mCode[i + 1]}.*{mCode[i + 2]}, %{mCode[i + 3]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 2], Utils.VariantToReadableString(mData
                                //    [mCode[i + 2]]));
                                com = $"*{mCode[i + 2]} = {Utils.VariantToReadableString(mData[mCode[i + 2]])}"; //2
                            }
                            size = 4;
                            break;
                        }

                    case VM_GPI:
                    case VM_GPIS:
                        {
                            // property get indirect
                            //msg = string.Format(mCode[i] == VM_GPI ? "gpi %%%d, %%%d.%%%d" : "gpis %%%d, %%%d.%%%d"
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            msg = mCode[i] == VM_GPI ? "gpi" : "gpis";
                            msg += $" %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SPI:
                    case VM_SPIE:
                    case VM_SPIS:
                        {
                            // property set indirect
                            //msg = string.Format(mCode[i] == VM_SPI ? "spi %%%d.%%%d, %%%d" : mCode[i] == VM_SPIE
                            //     ? "spie %%%d.%%%d, %%%d" : "spis %%%d.%%%d, %%%d", mCode[i + 1], mCode[i + 2],
                            //    mCode[i + 3]);
                            msg = mCode[i] == VM_SPI ? "spi" : mCode[i] == VM_SPIE ? "spie" : "spis";
                            msg += $" %{mCode[i + 1]}.%{mCode[i + 2]}, %{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SETP:
                        {
                            // property set
                            //msg = string.Format("setp %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"setp %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_GETP:
                        {
                            // property get
                            //msg = string.Format("getp %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"getp %{mCode[i + 1]}, %{mCode[i + 2]}"; 
                            size = 3;
                            break;
                        }

                    case VM_DELD:
                    case VM_TYPEOFD:
                        {
                            // member delete direct / typeof direct
                            //msg = string.Format(mCode[i] == VM_DELD ? "deld %%%d, %%%d.*%d" : "typeofd %%%d, %%%d.*%d"
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            msg = mCode[i] == VM_DELD ? "deld" : "typeofd";
                            msg += $" %{mCode[i + 1]}, %{mCode[i + 2]}.*{mCode[i + 3]}";
                            if (mData != null)
                            {
                                //com = string.Format("*%d = %s", mCode[i + 3], Utils.VariantToReadableString(mData
                                //    [mCode[i + 3]]));
                                com = $"*{mCode[i + 3]} = {Utils.VariantToReadableString(mData[mCode[i + 3]])}";
                            }
                            size = 4;
                            break;
                        }

                    case VM_DELI:
                    case VM_TYPEOFI:
                        {
                            // member delete indirect / typeof indirect
                            //msg = string.Format(mCode[i] == VM_DELI ? "deli %%%d, %%%d.%%%d" : "typeofi %%%d, %%%d.%%%d"
                            //    , mCode[i + 1], mCode[i + 2], mCode[i + 3]);
                            msg = mCode[i] == VM_DELI ? "deli" : "typeofi";
                            msg += $" %{mCode[i + 1]}, %{mCode[i + 2]}.%{mCode[i + 3]}";
                            size = 4;
                            break;
                        }

                    case VM_SRV:
                        {
                            // set return value
                            //msg = string.Format("srv %%%d", mCode[i + 1]);
                            msg = $"srv %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_RET:
                        {
                            // return
                            msg = "ret";
                            size = 1;
                            break;
                        }

                    case VM_ENTRY:
                        {
                            // enter try-protected block
                            //msg = string.Format("entry %09d, %%%d", mCode[i + 1] + i, mCode[i + 2]);
                            msg = $"entry {mCode[i + 1] + i: D9}, {mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_EXTRY:
                        {
                            // exit from try-protected block
                            msg = "extry";
                            size = 1;
                            break;
                        }

                    case VM_THROW:
                        {
                            //msg = string.Format("throw %%%d", mCode[i + 1]);
                            msg = $"throw %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_CHGTHIS:
                        {
                            //msg = string.Format("chgthis %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"chgthis %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_GLOBAL:
                        {
                            //msg = string.Format("global %%%d", mCode[i + 1]);
                            msg = $"global %{mCode[i + 1]}";
                            size = 2;
                            break;
                        }

                    case VM_ADDCI:
                        {
                            //msg = string.Format("addci %%%d, %%%d", mCode[i + 1], mCode[i + 2]);
                            msg = $"addci %{mCode[i + 1]}, %{mCode[i + 2]}";
                            size = 3;
                            break;
                        }

                    case VM_REGMEMBER:
                        {
                            msg = "regmember";
                            size = 1;
                            break;
                        }

                    case VM_DEBUGGER:
                        {
                            msg = "debugger";
                            size = 1;
                            break;
                        }

                    default:
                        {
                            //msg = string.Format("unknown instruction %d", mCode[i]);
                            msg = $"unknown instruction {mCode[i]}";
                            size = 1;
                            break;
                        }
                }
                OutputFunc(msg, com, i, i, size, data);
                // call the callback
                i += size;
            }
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

		private const int __VM_LAST = 128;

		private const int fatNormal = 0;

		private const int fatExpand = 1;

		private const int fatUnnamedExpand = 2;
		// VMCodes
		// FuncArgType
	}
}

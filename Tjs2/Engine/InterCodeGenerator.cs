/*
 * TJS2 CSharp
 */

using System.Collections.Generic;
using System.Text;
using Tjs2.Engine.Translate;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
    /// <summary>TJS2 のバイトコードを生成する</summary>
    public class InterCodeGenerator : SourceCodeAccessor
    {
        private const int INC_ARRAY_COUNT = 256;

        private const int SEARCH_CONST_VAL_SIZE = 20;

        private VectorWrap<ExprNode> mNodeToDeleteVector;

        private VectorWrap<ExprNode> mCurrentNodeVector;

        private Compiler mBlock;

        private InterCodeGenerator mParent;

        private string mName;

        private int mContextType;

        private short[] mCodeArea;

        private int mCodeAreaPos;

        private short[] mCode;

        private int mPrevSourcePos;

        private bool mSourcePosArraySorted;

        private long[] mSourcePosArray;

        private int mSrcPosArrayPos;

        private AList<Variant> mDataArea;

        private AList<Variant> mInterCodeDataArea;

        private Variant[] mDataArray;

        private LocalNamespace mNamespace;

        private int mVariableReserveCount;

        private int mFrameBase;

        private int mMaxFrameCount;

        private IntVector mJumpList;

        private bool mAsGlobalContextMode;

        private ExprNode mSuperClassExpr;

        private VectorWrap<InterCodeGenerator.NestData> mNestVector;

        private Stack<InterCodeGenerator.ArrayArg> mArrayArgStack;

        private Stack<InterCodeGenerator.FuncArg> mFuncArgStack;

        private InterCodeGenerator mPropSetter;

        private InterCodeGenerator mPropGetter;

        private InterCodeGenerator mSuperClassGetter;

        private int mMaxVariableCount;

        private AList<InterCodeGenerator.FixData> mFixList;

        private VectorWrap<InterCodeGenerator.NonLocalFunctionDecl> mNonLocalFunctionDeclVector;

        private int mFunctionRegisterCodePoint;

        private int mPrevIfExitPatch;

        private IntVector mSuperClassGetterPointer;

        private int mFuncDeclArgCount;

        private int mFuncDeclUnnamedArgArrayBase;

        private int mFuncDeclCollapseBase;

        public class SubParam
        {
            public int mSubType;

            public int mSubFlag;

            public int mSubAddress;

            public SubParam()
            {
            }

            public SubParam(InterCodeGenerator.SubParam param)
            {
                //private static final int INC_SIZE_BYTE = 1024;
                //private static final int INC_SIZE_BYTE = 512;
                //private static final int INC_SIZE_LONG_BYTE = 2048;
                //private LongBuffer mSourcePosArray; // 上位をcodePos, 下位をsourcePos とする
                // 上位をcodePos, 下位をsourcePos とする
                // Dataの中に function として、InterCodeGenerator が入っている可能性がある、后で差し替えること。
                // Compiler に持たせた方がいいかな？
                //mSubType = 0;
                //mSubFlag = 0;
                //mSubAddress = 0;
                mSubType = param.mSubType;
                mSubFlag = param.mSubFlag;
                mSubAddress = param.mSubAddress;
            }
        }

        public static int ntBlock = 0;

        public static int ntWhile = 1;

        public static int ntDoWhile = 2;

        public static int ntFor = 3;

        public static int ntSwitch = 4;

        public static int ntIf = 5;

        public static int ntElse = 6;

        public static int ntTry = 7;

        public static int ntCatch = 8;

        public static int ntWith = 9;

        internal class NestData
        {
            internal int Type;

            internal int VariableCount;

            internal bool VariableCreated;

            internal int RefRegister;

            internal int StartIP;

            internal int LoopStartIP;

            internal IntVector ContinuePatchVector;

            internal IntVector ExitPatchVector;

            internal int Patch1;

            internal int Patch2;

            internal ExprNode PostLoopExpr;

            public NestData()
            {
                // tNestType
                // union {
                // boolean IsFirstCase; 上と同じとみなす
                //};
                ContinuePatchVector = new IntVector();
                ExitPatchVector = new IntVector();
            }
        }

        internal class FixData
        {
            internal int StartIP;

            internal int Size;

            internal int NewSize;

            internal bool BeforeInsertion;

            internal ShortBuffer Code;

            public FixData(int startip, int size, int newsize, ShortBuffer code, bool beforeinsertion
                )
            {
                StartIP = startip;
                Size = size;
                NewSize = newsize;
                Code = code;
                BeforeInsertion = beforeinsertion;
            }

            public FixData(InterCodeGenerator.FixData fixdata)
            {
                //Code = null;
                Copy(fixdata);
            }

            public virtual void Copy(InterCodeGenerator.FixData fixdata)
            {
                Code = null;
                StartIP = fixdata.StartIP;
                Size = fixdata.Size;
                NewSize = fixdata.NewSize;
                BeforeInsertion = fixdata.BeforeInsertion;
                short[] newbuff = new short[NewSize];
                //ByteBuffer buff = ByteBuffer.allocate(NewSize*2);
                //buff.order( ByteOrder.nativeOrder() );
                //ShortBuffer ibuff = buff.asShortBuffer();
                ShortBuffer ibuff = ShortBuffer.Wrap(newbuff);
                ibuff.Clear();
                ByteBuffer tmp = fixdata.Code.Duplicate();
                tmp.Flip();
                ibuff.Put(tmp);
                Code = ibuff;
            }
        }

        internal class NonLocalFunctionDecl
        {
            internal int DataPos;

            internal int NameDataPos;

            internal bool ChangeThis;

            public NonLocalFunctionDecl(int datapos, int namedatapos, bool changethis)
            {
                DataPos = datapos;
                NameDataPos = namedatapos;
                ChangeThis = changethis;
            }
        }

        internal class FuncArgItem
        {
            public int Register;

            public int Type;

            public FuncArgItem(int reg) : this(reg, fatNormal)
            {
            }

            public FuncArgItem(int reg, int type)
            {
                //private static final int fatNormal = 0, fatExpand = 1, fatUnnamedExpand = 2;
                // tTJSFuncArgType Type;
                Register = reg;
                Type = type;
            }
        }

        internal class FuncArg
        {
            public bool IsOmit;

            public bool HasExpand;

            public AList<InterCodeGenerator.FuncArgItem> ArgVector;

            public FuncArg()
            {
                //IsOmit = HasExpand = false;
                ArgVector = new AList<InterCodeGenerator.FuncArgItem>();
            }
        }

        internal class ArrayArg
        {
            internal int Object;

            internal int Counter;
        }

        /// <exception cref="VariantException"></exception>
        public InterCodeGenerator(InterCodeGenerator parent, string name, Compiler block,
            int type)
        {
            //super(getContextHashSize(type));
            //super.mCallFinalize = false;
            mNodeToDeleteVector = new VectorWrap<ExprNode>();
            mCurrentNodeVector = new VectorWrap<ExprNode>();
            mJumpList = new IntVector();
            mNestVector = new VectorWrap<InterCodeGenerator.NestData>();
            mArrayArgStack = new Stack<InterCodeGenerator.ArrayArg>();
            mFuncArgStack = new Stack<InterCodeGenerator.FuncArg>();
            mNamespace = new LocalNamespace();
            mFixList = new AList<InterCodeGenerator.FixData>();
            mNonLocalFunctionDeclVector = new VectorWrap<InterCodeGenerator.NonLocalFunctionDecl>();
            mSuperClassGetterPointer = new IntVector();
            mParent = parent;
            mPropGetter = mPropSetter = mSuperClassGetter = null;
            mCodeArea = new short[INC_ARRAY_COUNT];
            mDataArea = new AList<Variant>();
            mInterCodeDataArea = new AList<Variant>();
            mFrameBase = 1;
            //mSuperClassExpr = null;
            //mMaxFrameCount = 0;
            //mMaxVariableCount = 0;
            //mFuncDeclArgCount = 0;
            //mFuncDeclUnnamedArgArrayBase = 0;
            mFuncDeclCollapseBase = -1;
            //mFunctionRegisterCodePoint = 0;
            mPrevSourcePos = -1;
            //mSourcePosArraySorted = false;
            //mSourcePosArray = null;
            if (!string.IsNullOrEmpty(name))
            {
                mName = name;
            }
            //else {
            //	mName = null;
            //}
            //mAsGlobalContextMode = false;
            mContextType = type;
            switch (mContextType)
            {
                case ContextType.TOP_LEVEL:
                    {
                        // decide variable reservation count with context type
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.FUNCTION:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.EXPR_FUNCTION:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.PROPERTY:
                    {
                        mVariableReserveCount = 0;
                        break;
                    }

                case ContextType.PROPERTY_SETTER:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.PROPERTY_GETTER:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.CLASS:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }

                case ContextType.SUPER_CLASS_GETTER:
                    {
                        mVariableReserveCount = 2;
                        break;
                    }
            }
            mBlock = block;
            mBlock.Add(this);
            if (mContextType == ContextType.CLASS)
            {
                // add class information to the class instance information
                if (mMaxFrameCount < 1)
                {
                    mMaxFrameCount = 1;
                }
                int dp = PutData(new Variant(mName));
                int lexPos = GetLexPos();
                // const %1, name
                // addci %-1, %1
                // cl %1
                if ((mCodeAreaPos + 7) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexPos);
                }
                mCodeArea[mCodeAreaPos] = (short)VM_CONST;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)1;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)dp;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)VM_ADDCI;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)-1;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)1;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)VM_CL;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)1;
                mCodeAreaPos++;
                // update FunctionRegisterCodePoint
                mFunctionRegisterCodePoint = mCodeAreaPos;
            }
        }

        // update FunctionRegisterCodePoint
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        protected internal virtual void FinalizeObject()
        {
            if (mPropSetter != null)
            {
                mPropSetter = null;
            }
            if (mPropGetter != null)
            {
                mPropGetter = null;
            }
            if (mSuperClassGetter != null)
            {
                mSuperClassGetter = null;
            }
            if (mCodeArea != null)
            {
                mCodeArea = null;
            }
            if (mDataArea != null)
            {
                mDataArea.Clear();
                mDataArea = null;
            }
            mBlock.Remove(this);
            if (mContextType != ContextType.TOP_LEVEL && mBlock != null)
            {
                mBlock = null;
            }
            mNamespace.Clear();
            ClearNodesToDelete();
        }

        //super.finalizeObject();
        public string GetName()
        {
            return mName;
        }

        /// <exception cref="CompileException"></exception>
        private void ErrorMsg(string msg)
        {
            mBlock.Error(msg);
        }

        private int GetLexPos()
        {
            return mBlock.GetLexicalAnalyzer().GetCurrentPosition();
        }

        public virtual int GetNodeToDeleteVectorCount()
        {
            return mNodeToDeleteVector.Count;
        }

        public virtual void PushCurrentNode(ExprNode node)
        {
            mCurrentNodeVector.AddItem(node);
        }

        public virtual ExprNode GetCurrentNode()
        {
            if (mCurrentNodeVector.Count == 0)
            {
                return null;
            }
            return mCurrentNodeVector[mCurrentNodeVector.Count - 1];
        }

        public virtual void PopCurrentNode()
        {
            int count = mCurrentNodeVector.Count;
            if (count > 0)
            {
                mCurrentNodeVector.Remove(count - 1);
            }
        }

        //private int putCode( int num ) { return putCode( num, -1 ); }
        private void ExpandCodeArea()
        {
            int capacity = mCodeArea.Length;
            int newCount = capacity + INC_ARRAY_COUNT;
            short[] nArray = new short[newCount];
            System.Array.Copy(mCodeArea, 0, nArray, 0, capacity);
            mCodeArea = null;
            mCodeArea = nArray;
        }

        private void PutSrcPos(int pos)
        {
            if (pos == -1)
            {
                return;
            }
            if (mPrevSourcePos != pos)
            {
                mPrevSourcePos = pos;
                mSourcePosArraySorted = false;
                if (mSourcePosArray == null)
                {
                    mSourcePosArray = new long[INC_ARRAY_COUNT];
                }
                else
                {
                    if (mSrcPosArrayPos >= mSourcePosArray.Length)
                    {
                        int newcount = mSourcePosArray.Length + INC_ARRAY_COUNT;
                        long[] narray = new long[newcount];
                        System.Array.Copy(mSourcePosArray, 0, narray, 0, mSrcPosArrayPos);
                        mSourcePosArray = narray;
                    }
                }
                mSourcePosArray[mSrcPosArrayPos] = (((long)mCodeAreaPos << 32) | (long)pos);
                mSrcPosArrayPos++;
            }
        }

        /// <exception cref="VariantException"></exception>
        private int PutData(Variant val)
        {
            // 直近の20个の中で同じものがあるか调べる TODO 别コンパイルにするのなら、全データでチェックするようにした方がいいか
            int size = mDataArea.Count;
            int count = size > SEARCH_CONST_VAL_SIZE ? SEARCH_CONST_VAL_SIZE : size;
            int offset = size - 1;
            for (int i = 0; i < count; i++)
            {
                int pos = offset - i;
                if (mDataArea[pos].DiscernCompareStrictReal(val))
                {
                    return pos;
                }
            }
            Variant v;
            if (val.IsString())
            {
                v = new Variant(Tjs.MapGlobalStringMap(val.AsString()));
            }
            else
            {
                v = new Variant(val);
                object o = v.ToJavaObject();
                if (o is InterCodeGenerator)
                {
                    mInterCodeDataArea.AddItem(v);
                }
            }
            mDataArea.AddItem(v);
            return mDataArea.Count - 1;
        }

        /// <summary>DaraArray の中の InterCodeGenerator を InterCodeObject に差し替える</summary>
        /// <param name="compiler"></param>
        public virtual void DateReplace(Compiler compiler)
        {
            int count = mInterCodeDataArea.Count;
            for (int i = 0; i < count; i++)
            {
                Variant d = mInterCodeDataArea[i];
                object o = d.ToJavaObject();
                if (o is InterCodeGenerator)
                {
                    int idx = compiler.GetCodeIndex((InterCodeGenerator)o);
                    if (idx < 0)
                    {
                        Tjs.OutputToConsole("not found");
                    }
                    d.Set(compiler.GetCodeObject(idx));
                }
            }
        }

        /// <exception cref="VariantException"></exception>
        public virtual void AddLocalVariable(string name)
        {
            AddLocalVariable(name, 0);
        }

        /// <exception cref="VariantException"></exception>
        public virtual void AddLocalVariable(string name, int init)
        {
            int @base = mContextType == ContextType.CLASS ? 2 : 1;
            int lexPos = GetLexPos();
            if (mNamespace.GetLevel() >= @base)
            {
                mNamespace.Add(name);
                if (init != 0)
                {
                    int n = mNamespace.Find(name);
                    //putCode( VM_CP, lexPos );
                    //putCode( -n-mVariableReserveCount-1, lexPos );
                    //putCode( init, lexPos );
                    if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexPos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)VM_CP;
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)init;
                    mCodeAreaPos++;
                }
                else
                {
                    int n = mNamespace.Find(name);
                    //putCode( VM_CL, lexPos );
                    //putCode( -n-mVariableReserveCount-1, lexPos );
                    if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexPos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)VM_CL;
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                    mCodeAreaPos++;
                }
            }
            else
            {
                int dp = PutData(new Variant(name));
                //putCode( VM_SPDS, lexPos );
                //putCode( -1, lexPos );
                //putCode( dp, lexPos );
                //putCode( init, lexPos );
                if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexPos);
                }
                mCodeArea[mCodeAreaPos] = (short)VM_SPDS;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(-1);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)dp;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)init;
                mCodeAreaPos++;
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void InitLocalVariable(string name, ExprNode node)
        {
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            AddLocalVariable(name, resaddr);
            ClearFrame(fr);
        }

        /// <exception cref="VariantException"></exception>
        public virtual void InitLocalFunction(string name, int data)
        {
            // create a local function variable pointer by data ( in DataArea ),
            // named "name".
            int fr = mFrameBase;
            int pos = GetLexPos();
            //putCode(VM_CONST, pos );
            //putCode( fr, pos );
            //putCode( data);
            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(pos);
            }
            mCodeArea[mCodeAreaPos] = (short)VM_CONST;
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)fr;
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)data;
            mCodeAreaPos++;
            fr++;
            AddLocalVariable(name, fr - 1);
            ClearFrame(fr);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CreateExprCode(ExprNode node)
        {
            // create code of node
            IntWrapper fr = new IntWrapper(mFrameBase);
            GenNodeCode(fr, node, 0, 0, new InterCodeGenerator.SubParam());
            ClearFrame(fr);
        }

        public virtual void EnterWhileCode(bool doWhile)
        {
            // enter to "while"
            // ( do_while = true indicates do-while syntax )
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = doWhile ? ntDoWhile : ntWhile;
            mNestVector.LastElement().LoopStartIP = mCodeAreaPos;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CreateWhileExprCode(ExprNode node, bool doWhile)
        {
            // process the condition expression "node"
            if (doWhile)
            {
                DoContinuePatch(mNestVector.LastElement());
            }
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                ());
            int nodepos = node?.GetPosition() ?? -1;
            bool inv = false;
            if (!(resaddr == GNC_CFLAG || resaddr == GNC_CFLAG_I))
            {
                //putCode( VM_TT, nodepos );
                //putCode( resaddr, nodepos );
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)VM_TT;
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)resaddr;
                mCodeAreaPos++;
            }
            else
            {
                if (resaddr == GNC_CFLAG_I)
                {
                    inv = true;
                }
            }
            ClearFrame(fr);
            if (!doWhile)
            {
                mNestVector.LastElement().ExitPatchVector.Add(mCodeAreaPos);
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode(inv?VM_JF:VM_JNF, nodepos);
                //putCode(0, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JF : VM_JNF);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)0;
                mCodeAreaPos++;
            }
            else
            {
                int jmp_ip = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode(inv?VM_JNF:VM_JF, nodepos);
                //putCode(mNestVector.lastElement().LoopStartIP - jmp_ip, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JNF : VM_JF);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(mNestVector.LastElement().LoopStartIP - jmp_ip);
                mCodeAreaPos++;
            }
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitWhileCode(bool doWhile)
        {
            // exit from "while"
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (doWhile)
            {
                if (mNestVector.LastElement().Type != ntDoWhile)
                {
                    ErrorMsg(Error.SyntaxError);
                    return;
                }
            }
            else
            {
                if (mNestVector.LastElement().Type != ntWhile)
                {
                    ErrorMsg(Error.SyntaxError);
                    return;
                }
            }
            if (!doWhile)
            {
                int jmp_ip = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                int pos = GetLexPos();
                //putCode(VM_JMP, pos );
                //putCode( mNestVector.lastElement().LoopStartIP - jmp_ip, pos );
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(pos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(mNestVector.LastElement().LoopStartIP - jmp_ip);
                mCodeAreaPos++;
            }
            DoNestTopExitPatch();
            mNestVector.Remove(mNestVector.Count - 1);
        }

        public virtual void EnterIfCode()
        {
            // enter to "if"
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntIf;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CrateIfExprCode(ExprNode node)
        {
            // process condition expression "node"
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                ());
            int nodepos = (node != null ? node.GetPosition() : -1);
            bool inv = false;
            if (!(resaddr == GNC_CFLAG || resaddr == GNC_CFLAG_I))
            {
                //putCode(VM_TT, nodepos );
                //putCode(resaddr, nodepos );
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
            }
            else
            {
                if (resaddr == GNC_CFLAG_I)
                {
                    inv = true;
                }
            }
            ClearFrame(fr);
            mNestVector.LastElement().Patch1 = mCodeAreaPos;
            //addJumpList();
            mJumpList.Add(mCodeAreaPos);
            //putCode(inv?VM_JF:VM_JNF, nodepos );
            //putCode(0, nodepos );
            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(nodepos);
            }
            mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JF : VM_JNF);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitIfCode()
        {
            // exit from if
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntIf)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            mCodeArea[mNestVector.LastElement().Patch1 + 1] = (short)(mCodeAreaPos - mNestVector
                .LastElement().Patch1);
            mPrevIfExitPatch = mNestVector.LastElement().Patch1;
            mNestVector.Remove(mNestVector.Count - 1);
        }

        public virtual void EnterElseCode()
        {
            // enter to "else".
            // before is "if", is clear from syntax definition.
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntElse;
            mNestVector.LastElement().Patch2 = mCodeAreaPos;
            //addJumpList();
            mJumpList.Add(mCodeAreaPos);
            int pos = GetLexPos();
            //putCode(VM_JMP, pos);
            //putCode(0, pos);
            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(pos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
            mCodeArea[mPrevIfExitPatch + 1] = (short)(mCodeAreaPos - mPrevIfExitPatch);
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitElseCode()
        {
            // exit from else
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntElse)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            mCodeArea[mNestVector.LastElement().Patch2 + 1] = (short)(mCodeAreaPos - mNestVector
                .LastElement().Patch2);
            mNestVector.Remove(mNestVector.Count - 1);
        }

        public virtual void EnterForCode(bool varcreate)
        {
            // enter to "for".
            // ( varcreate = true, indicates that the variable is to be created in the
            //	first clause )
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntFor;
            if (varcreate)
            {
                EnterBlock();
            }
            // create a scope
            mNestVector.LastElement().VariableCreated = varcreate;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CreateForExprCode(ExprNode node)
        {
            // process the "for"'s second clause; a condition expression
            mNestVector.LastElement().LoopStartIP = mCodeAreaPos;
            if (node != null)
            {
                int nodepos = node.GetPosition();
                IntWrapper fr = new IntWrapper(mFrameBase);
                int resaddr = GenNodeCode(fr, node, RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                    ());
                bool inv = false;
                if (!(resaddr == GNC_CFLAG || resaddr == GNC_CFLAG_I))
                {
                    //putCode(VM_TT, nodepos );
                    //putCode(resaddr, nodepos );
                    if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(nodepos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                    mCodeAreaPos++;
                }
                else
                {
                    if (resaddr == GNC_CFLAG_I)
                    {
                        inv = true;
                    }
                }
                ClearFrame(fr);
                mNestVector.LastElement().ExitPatchVector.Add(mCodeAreaPos);
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode(inv?VM_JF:VM_JNF, nodepos);
                //putCode( 0, nodepos );
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JF : VM_JNF);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
            }
        }

        public virtual void SetForThirdExprCode(ExprNode node)
        {
            // process the "for"'s third clause; a post-loop expression
            mNestVector.LastElement().PostLoopExpr = node;
        }

        /// <exception cref="CompileException"></exception>
        /// <exception cref="VariantException"></exception>
        public virtual void ExitForCode()
        {
            // exit from "for"
            int nestsize = mNestVector.Count;
            if (nestsize == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntFor && mNestVector.LastElement().Type !=
                ntBlock)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type == ntFor)
            {
                DoContinuePatch(mNestVector.LastElement());
            }
            if (nestsize >= 2 && mNestVector[nestsize - 2].Type == ntFor)
            {
                DoContinuePatch(mNestVector[nestsize - 2]);
            }
            if (mNestVector.LastElement().PostLoopExpr != null)
            {
                IntWrapper fr = new IntWrapper(mFrameBase);
                GenNodeCode(fr, mNestVector.LastElement().PostLoopExpr, 0, 0, new InterCodeGenerator.SubParam
                    ());
                ClearFrame(fr);
            }
            int jmp_ip = mCodeAreaPos;
            //addJumpList();
            mJumpList.Add(mCodeAreaPos);
            int lexpos = GetLexPos();
            //putCode(VM_JMP, lexpos );
            //putCode( mNestVector.lastElement().LoopStartIP - jmp_ip, lexpos );
            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(lexpos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(mNestVector.LastElement().LoopStartIP - jmp_ip);
            mCodeAreaPos++;
            DoNestTopExitPatch();
            if (mNestVector.LastElement().VariableCreated)
            {
                ExitBlock();
            }
            DoNestTopExitPatch();
            mNestVector.Remove(mNestVector.Count - 1);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void EnterSwitchCode(ExprNode node)
        {
            // enter to "switch"
            // "node" indicates a reference expression
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntSwitch;
            mNestVector.LastElement().Patch1 = -1;
            mNestVector.LastElement().Patch2 = -1;
            //mNestVector.lastElement().IsFirstCase = true; // IsFirstCase と VariableCreated は同一に扱う
            mNestVector.LastElement().VariableCreated = true;
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            if (mFrameBase != resaddr)
            {
                int nodepos = (node != null ? node.GetPosition() : -1);
                //putCode(VM_CP, nodepos);
                //putCode(mFrameBase, nodepos ); // FrameBase points the reference value
                //putCode(resaddr, nodepos );
                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(mFrameBase);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
            }
            mNestVector.LastElement().RefRegister = mFrameBase;
            if (fr.value - 1 > mMaxFrameCount)
            {
                mMaxFrameCount = fr.value - 1;
            }
            mFrameBase++;
            // increment FrameBase
            if (mFrameBase - 1 > mMaxFrameCount)
            {
                mMaxFrameCount = mFrameBase - 1;
            }
            ClearFrame(fr);
            EnterBlock();
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitSwitchCode()
        {
            // exit from switch
            ExitBlock();
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntSwitch)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            int lexpos = GetLexPos();
            int patch3 = 0;
            //if( !mNestVector.lastElement().IsFirstCase ) // IsFirstCase と VariableCreated は同一に扱う
            if (!mNestVector.LastElement().VariableCreated)
            {
                patch3 = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode( VM_JMP, lexpos );
                //putCode( 0, lexpos );
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexpos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
            }
            if (mNestVector.LastElement().Patch1 != -1)
            {
                mCodeArea[mNestVector.LastElement().Patch1 + 1] = (short)(mCodeAreaPos - mNestVector
                    .LastElement().Patch1);
            }
            if (mNestVector.LastElement().Patch2 != -1)
            {
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                int jmp_start = mCodeAreaPos;
                //putCode( VM_JMP, lexpos);
                //putCode( mNestVector.lastElement().Patch2 - jmp_start, lexpos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexpos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(mNestVector.LastElement().Patch2 - jmp_start);
                mCodeAreaPos++;
            }
            //if( !mNestVector.lastElement().IsFirstCase ) {
            if (!mNestVector.LastElement().VariableCreated)
            {
                mCodeArea[patch3 + 1] = (short)(mCodeAreaPos - patch3);
            }
            DoNestTopExitPatch();
            mFrameBase--;
            mNestVector.Remove(mNestVector.Count - 1);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void ProcessCaseCode(ExprNode node)
        {
            // process "case expression :".
            // process "default :" if node == NULL.
            int nestsize = mNestVector.Count;
            if (nestsize < 3)
            {
                ErrorMsg(Error.MisplacedCase);
                return;
            }
            if (mNestVector[nestsize - 1].Type != ntBlock || mNestVector[nestsize - 2].Type !=
                 ntBlock || mNestVector[nestsize - 3].Type != ntSwitch)
            {
                // the stack layout must be ( from top )
                // ntBlock, ntBlock, ntSwitch
                ErrorMsg(Error.MisplacedCase);
                return;
            }
            InterCodeGenerator.NestData data = mNestVector[mNestVector.Count - 3];
            int patch3 = 0;
            //if( !data.IsFirstCase ) { // IsFirstCase と VariableCreated は同一に扱う
            if (!data.VariableCreated)
            {
                patch3 = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                int nodepos = (node != null ? node.GetPosition() : -1);
                //putCode(VM_JMP, nodepos);
                //putCode(0, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
            }
            ExitBlock();
            if (data.Patch1 != -1)
            {
                mCodeArea[data.Patch1 + 1] = (short)(mCodeAreaPos - data.Patch1);
            }
            if (node != null)
            {
                IntWrapper fr = new IntWrapper(mFrameBase);
                int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                    ());
                int nodepos = (node != null ? node.GetPosition() : -1);
                //putCode( VM_CEQ, nodepos);
                //putCode( data.RefRegister, nodepos);
                // compare to reference value with normal comparison
                //putCode( resaddr, nodepos);
                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_CEQ);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(data.RefRegister);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
                ClearFrame(fr);
                data.Patch1 = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode(VM_JNF, nodepos);
                //putCode(0, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JNF);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
            }
            else
            {
                data.Patch1 = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                int nodepos = (node != null ? node.GetPosition() : -1);
                //putCode(VM_JMP, nodepos);
                //putCode(0, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
                data.Patch2 = mCodeAreaPos;
            }
            // Patch2 = "default:"'s position
            //if( !data.IsFirstCase ) {
            if (!data.VariableCreated)
            {
                mCodeArea[patch3 + 1] = (short)(mCodeAreaPos - patch3);
            }
            //data.IsFirstCase = false;
            data.VariableCreated = false;
            EnterBlock();
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void EnterWithCode(ExprNode node)
        {
            // enter to "with"
            // "node" indicates a reference expression
            // this method and ExitWithCode are very similar to switch's code.
            // (those are more simple than that...)
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            int nodepos = (node != null ? node.GetPosition() : -1);
            if (mFrameBase != resaddr)
            {
                // bring the reference variable to frame base top
                //putCode(VM_CP, nodepos);
                //putCode( mFrameBase, nodepos); // FrameBase points the reference value
                //putCode( resaddr, nodepos);
                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(mFrameBase);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
            }
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntWith;
            mNestVector.LastElement().RefRegister = mFrameBase;
            if (fr.value - 1 > mMaxFrameCount)
            {
                mMaxFrameCount = fr.value - 1;
            }
            mFrameBase++;
            // increment FrameBase
            if (mFrameBase - 1 > mMaxFrameCount)
            {
                mMaxFrameCount = mFrameBase - 1;
            }
            ClearFrame(fr);
            EnterBlock();
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitWidthCode()
        {
            // exit from switch
            ExitBlock();
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntWith)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            mFrameBase--;
            mNestVector.Remove(mNestVector.Count - 1);
        }

        /// <exception cref="CompileException"></exception>
        public virtual void DoBreak()
        {
            // process "break".
            // search in NestVector backwards
            //int vc = mNamespace.getCount();
            //int pvc = vc;
            int lexpos = GetLexPos();
            int i = mNestVector.Count - 1;
            for (; i >= 0; i--)
            {
                InterCodeGenerator.NestData data = mNestVector[i];
                if (data.Type == ntSwitch || data.Type == ntWhile || data.Type == ntDoWhile || data
                    .Type == ntFor)
                {
                    // "break" can apply on this syntax
                    data.ExitPatchVector.Add(mCodeAreaPos);
                    //addJumpList();
                    mJumpList.Add(mCodeAreaPos);
                    //putCode(VM_JMP, lexpos);
                    //putCode(0, lexpos); // later patches here
                    if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexpos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)(0);
                    mCodeAreaPos++;
                    return;
                }
                else
                {
                    if (data.Type == ntBlock)
                    {
                    }
                    else
                    {
                        //pvc = data.VariableCount;
                        if (data.Type == ntTry)
                        {
                            //putCode(VM_EXTRY, -1 );
                            if ((mCodeAreaPos) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_EXTRY);
                            mCodeAreaPos++;
                        }
                        else
                        {
                            if (data.Type == ntSwitch || data.Type == ntWith)
                            {
                            }
                        }
                    }
                }
            }
            // clear reference register of "switch" or "with" syntax
            ErrorMsg(Error.MisplacedBreakContinue);
        }

        /// <exception cref="CompileException"></exception>
        public virtual void DoContinue()
        {
            // process "continue".
            // generate code that jumps before '}' ( the end of the loop ).
            // for "while" loop, the jump code immediately jumps to the condition check code.
            // search in NestVector backwards
            //int vc = mNamespace.getCount();
            //int pvc = vc;
            int i = mNestVector.Count - 1;
            int lexpos = GetLexPos();
            for (; i >= 0; i--)
            {
                InterCodeGenerator.NestData data = mNestVector[i];
                if (data.Type == ntWhile)
                {
                    // for "while" loop
                    int jmpstart = mCodeAreaPos;
                    //addJumpList();
                    mJumpList.Add(mCodeAreaPos);
                    //putCode(VM_JMP, lexpos);
                    //putCode(data.LoopStartIP - jmpstart, lexpos);
                    if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexpos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                    mCodeAreaPos++;
                    mCodeArea[mCodeAreaPos] = (short)(data.LoopStartIP - jmpstart);
                    mCodeAreaPos++;
                    return;
                }
                else
                {
                    if (data.Type == ntDoWhile || data.Type == ntFor)
                    {
                        // "do-while" or "for" loop needs forward jump
                        data.ContinuePatchVector.Add(mCodeAreaPos);
                        //addJumpList();
                        mJumpList.Add(mCodeAreaPos);
                        //putCode(VM_JMP, lexpos);
                        //putCode(0, lexpos); // later patch this
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(lexpos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        return;
                    }
                    else
                    {
                        if (data.Type == ntBlock)
                        {
                        }
                        else
                        {
                            // does not count variables which created at for loop's
                            // first clause
                            //if( i < 1 || mNestVector.get( i-1 ).Type != ntFor || !mNestVector.get(i).VariableCreated )
                            //pvc = data.VariableCount;
                            if (data.Type == ntTry)
                            {
                                //putCode(VM_EXTRY, lexpos);
                                if ((mCodeAreaPos) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(lexpos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_EXTRY);
                                mCodeAreaPos++;
                            }
                            else
                            {
                                if (data.Type == ntSwitch || data.Type == ntWith)
                                {
                                }
                            }
                        }
                    }
                }
            }
            // clear reference register of "switch" or "with" syntax
            ErrorMsg(Error.MisplacedBreakContinue);
        }

        public virtual void DoDebugger()
        {
            // process "debugger" statement.
            //putCode(VM_DEBUGGER, getLexPos() );
            if ((mCodeAreaPos) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(GetLexPos());
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_DEBUGGER);
            mCodeAreaPos++;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void ReturnFromFunc(ExprNode node)
        {
            // precess "return"
            // note: the "return" positioned in global immediately returns without
            // execution of the remainder code.
            int nodepos = (node != null ? node.GetPosition() : -1);
            if (node == null)
            {
                // no return value
                //putCode( VM_SRV, nodepos );
                //putCode( 0, nodepos );  // returns register #0 = void
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_SRV);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
            }
            else
            {
                // generates return expression
                IntWrapper fr = new IntWrapper(mFrameBase);
                int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                    ());
                //putCode(VM_SRV, nodepos);
                //putCode( resaddr, nodepos);
                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_SRV);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
                ClearFrame(fr);
            }
            // clear the frame
            int org_framebase = mFrameBase;
            ClearFrame(mFrameBase, 1);
            mFrameBase = org_framebase;
            int lexpos = GetLexPos();
            // check try block
            int i = mNestVector.Count - 1;
            for (; i >= 0; i--)
            {
                InterCodeGenerator.NestData data = mNestVector[i];
                if (data.Type == ntTry)
                {
                    //putCode(VM_EXTRY, lexpos); // exit from try-protected block
                    if ((mCodeAreaPos) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexpos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)(VM_EXTRY);
                    mCodeAreaPos++;
                }
            }
            //putCode(VM_RET, lexpos);
            if ((mCodeAreaPos) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(lexpos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_RET);
            mCodeAreaPos++;
        }

        public virtual void EnterTryCode()
        {
            // enter to "try"
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntTry;
            mNestVector.LastElement().VariableCreated = false;
            int lexpos = GetLexPos();
            mNestVector.LastElement().Patch1 = mCodeAreaPos;
            //addJumpList();
            mJumpList.Add(mCodeAreaPos);
            //putCode(VM_ENTRY, lexpos);
            //putCode(0, lexpos);
            //putCode( mFrameBase, lexpos); // an exception object will be held here
            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(lexpos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_ENTRY);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(mFrameBase);
            mCodeAreaPos++;
            if (mFrameBase > mMaxFrameCount)
            {
                mMaxFrameCount = mFrameBase;
            }
        }

        /// <exception cref="VariantException"></exception>
        public virtual void EnterCatchCode(string name)
        {
            // enter to "catch"
            int lexpos = GetLexPos();
            //putCode(VM_EXTRY, lexpos);
            if ((mCodeAreaPos + 3) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(lexpos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_EXTRY);
            mCodeAreaPos++;
            mNestVector.LastElement().Patch2 = mCodeAreaPos;
            //addJumpList();
            mJumpList.Add(mCodeAreaPos);
            //putCode(VM_JMP, lexpos);
            //putCode(0, lexpos);
            mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
            mCodeArea[mNestVector.LastElement().Patch1 + 1] = (short)(mCodeAreaPos - mNestVector
                .LastElement().Patch1);
            // clear frame
            int fr = mMaxFrameCount + 1;
            int @base = name != null ? mFrameBase + 1 : mFrameBase;
            ClearFrame(fr, @base);
            // change nest type to ntCatch
            mNestVector.LastElement().Type = ntCatch;
            // create variable if the catch clause has a receiver variable name
            if (name != null)
            {
                mNestVector.LastElement().VariableCreated = true;
                EnterBlock();
                AddLocalVariable(name, mFrameBase);
            }
        }

        // cleate a variable that receives the exception object
        /// <exception cref="CompileException"></exception>
        public virtual void ExitTryCode()
        {
            // exit from "try"
            if (mNestVector.Count >= 2)
            {
                if (mNestVector[mNestVector.Count - 2].Type == ntCatch)
                {
                    if (mNestVector[mNestVector.Count - 2].VariableCreated)
                    {
                        ExitBlock();
                    }
                }
            }
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntCatch)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            int p2addr = mNestVector.LastElement().Patch2;
            mCodeArea[p2addr + 1] = (short)(mCodeAreaPos - p2addr);
            mNestVector.Remove(mNestVector.Count - 1);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void ProcessThrowCode(ExprNode node)
        {
            // process "throw".
            // node = expressoin to throw
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            int nodepos = (node != null ? node.GetPosition() : -1);
            //putCode(VM_THROW, nodepos);
            //putCode(resaddr, nodepos);
            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(nodepos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_THROW);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(resaddr);
            mCodeAreaPos++;
            if (fr.value - 1 > mMaxFrameCount)
            {
                mMaxFrameCount = fr.value - 1;
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CreateExtendsExprCode(ExprNode node, bool hold)
        {
            // process class extender
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            int nodepos = (node != null ? node.GetPosition() : -1);
            if ((mCodeAreaPos + 6) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(nodepos);
            }
            //putCode(VM_CHGTHIS, nodepos);
            //putCode(resaddr, nodepos);
            //putCode(-1, nodepos);
            mCodeArea[mCodeAreaPos] = (short)(VM_CHGTHIS);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(resaddr);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(-1);
            mCodeAreaPos++;
            //putCode(VM_CALL, nodepos);
            //putCode(0, nodepos);
            //putCode(resaddr, nodepos);
            //putCode(0, nodepos);
            mCodeArea[mCodeAreaPos] = (short)(VM_CALL);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(resaddr);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(0);
            mCodeAreaPos++;
            if (hold)
            {
                mSuperClassExpr = node;
            }
            mFunctionRegisterCodePoint = mCodeAreaPos;
            // update FunctionRegisterCodePoint
            // create a Super Class Proxy context
            if (mSuperClassGetter == null)
            {
                mSuperClassGetter = new InterCodeGenerator(this, mName, mBlock, ContextType.SUPER_CLASS_GETTER
                    );
            }
            mSuperClassGetter.CreateExtendsExprProxyCode(node);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void CreateExtendsExprProxyCode(ExprNode node)
        {
            // create super class proxy to retrieve super class
            mSuperClassGetterPointer.Add(mCodeAreaPos);
            IntWrapper fr = new IntWrapper(mFrameBase);
            int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                ());
            //putCode(VM_SRV);
            //putCode(resaddr);
            if ((mCodeAreaPos + 3) >= mCodeArea.Length)
            {
                ExpandCodeArea();
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_SRV);
            mCodeAreaPos++;
            mCodeArea[mCodeAreaPos] = (short)(resaddr);
            mCodeAreaPos++;
            //put1OperandCode( VM_SRV, resaddr, -1 );
            ClearFrame(fr);
            //putCode(VM_RET, -1 );
            mCodeArea[mCodeAreaPos] = (short)(VM_RET);
            mCodeAreaPos++;
            int nodepos = (node != null ? node.GetPosition() : -1);
            //putCode(VM_NOP, nodepos);
            if (CompileState.mEnableDebugCode)
            {
                PutSrcPos(nodepos);
            }
            mCodeArea[mCodeAreaPos] = (short)(VM_NOP);
            mCodeAreaPos++;
        }

        public virtual void EnterBlock()
        {
            // enter to block
            mNamespace.Push();
            int varcount = mNamespace.GetCount();
            mNestVector.AddItem(new InterCodeGenerator.NestData());
            mNestVector.LastElement().Type = ntBlock;
            mNestVector.LastElement().VariableCount = varcount;
        }

        /// <exception cref="CompileException"></exception>
        public virtual void ExitBlock()
        {
            // exit from block
            if (mNestVector.Count == 0)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            if (mNestVector.LastElement().Type != ntBlock)
            {
                ErrorMsg(Error.SyntaxError);
                return;
            }
            mNestVector.Remove(mNestVector.Count - 1);
            //int prevcount = mNamespace.getCount();
            mNamespace.Pop();
        }

        //int curcount = mNamespace.getCount();
        public virtual void GenerateFuncCallArgCode()
        {
            int lexpos = GetLexPos();
            if (mFuncArgStack.Peek().IsOmit)
            {
                //putCode(-1, lexpos); // omit (...) is specified
                if ((mCodeAreaPos) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexpos);
                }
                mCodeArea[mCodeAreaPos] = (short)(-1);
                mCodeAreaPos++;
            }
            else
            {
                if (mFuncArgStack.Peek().HasExpand)
                {
                    //putCode(-2, lexpos); // arguments have argument expanding node
                    AList<InterCodeGenerator.FuncArgItem> vec = mFuncArgStack.Peek().ArgVector;
                    if ((mCodeAreaPos + (vec.Count * 2) + 1) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexpos);
                    }
                    mCodeArea[mCodeAreaPos] = (short)(-2);
                    mCodeAreaPos++;
                    //putCode(vec.size(), lexpos); // count of the arguments
                    mCodeArea[mCodeAreaPos] = (short)(vec.Count);
                    mCodeAreaPos++;
                    for (int i = 0; i < vec.Count; i++)
                    {
                        //putCode(vec.get(i).Type, lexpos);
                        //putCode(vec.get(i).Register, lexpos);
                        InterCodeGenerator.FuncArgItem arg = vec[i];
                        mCodeArea[mCodeAreaPos] = (short)(arg.Type);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(arg.Register);
                        mCodeAreaPos++;
                    }
                }
                else
                {
                    AList<InterCodeGenerator.FuncArgItem> vec = mFuncArgStack.Peek().ArgVector;
                    if ((mCodeAreaPos + vec.Count) >= mCodeArea.Length)
                    {
                        ExpandCodeArea();
                    }
                    if (CompileState.mEnableDebugCode)
                    {
                        PutSrcPos(lexpos);
                    }
                    //putCode(vec.size(), lexpos); // count of arguments
                    mCodeArea[mCodeAreaPos] = (short)(vec.Count);
                    mCodeAreaPos++;
                    for (int i = 0; i < vec.Count; i++)
                    {
                        //putCode( vec.get(i).Register, lexpos);
                        mCodeArea[mCodeAreaPos] = (short)(vec[i].Register);
                        mCodeAreaPos++;
                    }
                }
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        public virtual void AddFunctionDeclArg(string varname, ExprNode node)
        {
            // process the function argument of declaration
            // varname = argument name
            // init = initial expression
            mNamespace.Add(varname);
            if (node != null)
            {
                int nodepos = (node != null ? node.GetPosition() : -1);
                //putCode(VM_CDEQ, nodepos);
                //putCode(-3 - mFuncDeclArgCount, nodepos);
                //putCode(0, nodepos);
                if ((mCodeAreaPos + 4) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_CDEQ);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(-3 - mFuncDeclArgCount);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
                int jmp_ip = mCodeAreaPos;
                //addJumpList();
                mJumpList.Add(mCodeAreaPos);
                //putCode(VM_JNF, nodepos);
                //putCode(0, nodepos);
                mCodeArea[mCodeAreaPos] = (short)(VM_JNF);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
                IntWrapper fr = new IntWrapper(mFrameBase);
                int resaddr = GenNodeCode(fr, node, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                    ());
                //putCode(VM_CP, nodepos);
                //putCode(-3 - mFuncDeclArgCount, nodepos);
                //putCode(resaddr, nodepos);
                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(nodepos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(-3 - mFuncDeclArgCount);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                mCodeAreaPos++;
                ClearFrame(fr);
                mCodeArea[jmp_ip + 1] = (short)(mCodeAreaPos - jmp_ip);
            }
            mFuncDeclArgCount++;
        }

        public virtual void AddFunctionDeclArgCollapse(string varname)
        {
            // process the function "collapse" argument of declaration.
            // collapse argument is available to receive arguments in array object form.
            if (varname == null)
            {
                // receive arguments in unnamed array
                mFuncDeclUnnamedArgArrayBase = mFuncDeclArgCount;
            }
            else
            {
                // receive arguments in named array
                mFuncDeclCollapseBase = mFuncDeclArgCount;
                mNamespace.Add(varname);
            }
        }

        public virtual void SetPropertyDeclArg(string varname)
        {
            // process the setter argument
            mNamespace.Add(varname);
            mFuncDeclArgCount = 1;
        }

        private void DoNestTopExitPatch()
        {
            // process the ExitPatchList which must be in the top of NextVector
            IntVector vector = mNestVector.LastElement().ExitPatchVector;
            int count = vector.Size();
            int codeSize = mCodeAreaPos;
            for (int i = 0; i < count; i++)
            {
                int val = vector.Get(i);
                mCodeArea[val + 1] = (short)(codeSize - val);
            }
        }

        private void DoContinuePatch(InterCodeGenerator.NestData nestdata)
        {
            // process the ContinuePatchList which must be in the top of NextVector
            IntVector vector = nestdata.ContinuePatchVector;
            int count = vector.Size();
            int codeSize = mCodeAreaPos;
            for (int i = 0; i < count; i++)
            {
                int val = vector.Get(i);
                mCodeArea[val + 1] = (short)(codeSize - val);
            }
        }

        public virtual ExprNode MakeConstValNode(Variant val)
        {
            ExprNode node = new ExprNode();
            mNodeToDeleteVector.AddItem(node);
            node.SetOpecode(Token.T_CONSTVAL);
            node.SetValue(val);
            node.SetPosition(GetLexPos());
            return node;
        }

        /// <exception cref="VariantException"></exception>
        public static void CharacterCodeFrom(Variant val)
        {
            char[] ch = new char[1];
            ch[0] = (char)val.AsInteger();
            val.Set(new string(ch));
        }

        /// <exception cref="VariantException"></exception>
        public static void CharacterCodeOf(Variant val)
        {
            string str = val.AsString();
            if (str != null)
            {
                int v = str[0];
                val.Set(v);
            }
            else
            {
                val.Set(0);
            }
        }

        public virtual ExprNode MakeNP0(int opecode)
        {
            ExprNode node = new ExprNode();
            mNodeToDeleteVector.AddItem(node);
            node.SetOpecode(opecode);
            node.SetPosition(GetLexPos());
            return node;
        }

        /// <exception cref="VariantException"></exception>
        public virtual ExprNode MakeNP1(int opecode, ExprNode node1)
        {
            // 定数の最适化
            if (node1 != null && node1.GetOpecode() == Token.T_CONSTVAL)
            {
                ExprNode ret = null;
                switch (opecode)
                {
                    case Token.T_EXCRAMATION:
                        {
                            ret = MakeConstValNode(node1.GetValue().GetNotValue());
                            break;
                        }

                    case Token.T_TILDE:
                        {
                            ret = MakeConstValNode(node1.GetValue().GetBitNotValue());
                            break;
                        }

                    case Token.T_SHARP:
                        {
                            Variant val = new Variant(node1.GetValue());
                            CharacterCodeOf(val);
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_DOLLAR:
                        {
                            Variant val = new Variant(node1.GetValue());
                            CharacterCodeFrom(val);
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_UPLUS:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ToNumber();
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_UMINUS:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ChangeSign();
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_INT:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ToInteger();
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_REAL:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ToReal();
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_STRING:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ToString();
                            ret = MakeConstValNode(val);
                            break;
                        }

                    case Token.T_OCTET:
                        {
                            Variant val = new Variant(node1.GetValue());
                            val.ToOctet();
                            ret = MakeConstValNode(val);
                            break;
                        }
                }
                // swtich
                if (ret != null)
                {
                    node1.Clear();
                    return ret;
                }
            }
            ExprNode node = new ExprNode();
            mNodeToDeleteVector.AddItem(node);
            node.SetOpecode(opecode);
            node.SetPosition(GetLexPos());
            node.Add(node1);
            return node;
        }

        /// <exception cref="VariantException"></exception>
        public virtual ExprNode MakeNP2(int opecode, ExprNode node1, ExprNode node2)
        {
            // 定数の最适化
            if (node1 != null && node1.GetOpecode() == Token.T_CONSTVAL && node2 != null && node2
                .GetOpecode() == Token.T_CONSTVAL)
            {
                switch (opecode)
                {
                    case Token.T_COMMA:
                        {
                            return MakeConstValNode(node2.GetValue());
                        }

                    case Token.T_LOGICALOR:
                        {
                            return MakeConstValNode(node1.GetValue().LogicOr(node2.GetValue()));
                        }

                    case Token.T_LOGICALAND:
                        {
                            return MakeConstValNode(node1.GetValue().LogicAnd(node2.GetValue()));
                        }

                    case Token.T_VERTLINE:
                        {
                            return MakeConstValNode(node1.GetValue().BitOr(node2.GetValue()));
                        }

                    case Token.T_CHEVRON:
                        {
                            return MakeConstValNode(node1.GetValue().BitXor(node2.GetValue()));
                        }

                    case Token.T_AMPERSAND:
                        {
                            return MakeConstValNode(node1.GetValue().BitAnd(node2.GetValue()));
                        }

                    case Token.T_NOTEQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().NotEqual(node2.GetValue()));
                        }

                    case Token.T_EQUALEQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().EqualEqual(node2.GetValue()));
                        }

                    case Token.T_DISCNOTEQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().DiscNotEqual(node2.GetValue()));
                        }

                    case Token.T_DISCEQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().DiscernCompare(node2.GetValue()));
                        }

                    case Token.T_LT:
                        {
                            return MakeConstValNode(node1.GetValue().Lt(node2.GetValue()));
                        }

                    case Token.T_GT:
                        {
                            return MakeConstValNode(node1.GetValue().Gt(node2.GetValue()));
                        }

                    case Token.T_LTOREQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().LtOrEqual(node2.GetValue()));
                        }

                    case Token.T_GTOREQUAL:
                        {
                            return MakeConstValNode(node1.GetValue().GtOrEqual(node2.GetValue()));
                        }

                    case Token.T_RARITHSHIFT:
                        {
                            return MakeConstValNode(node1.GetValue().RightShift(node2.GetValue()));
                        }

                    case Token.T_LARITHSHIFT:
                        {
                            return MakeConstValNode(node1.GetValue().LeftShift(node2.GetValue()));
                        }

                    case Token.T_RBITSHIFT:
                        {
                            return MakeConstValNode(node1.GetValue().RightBitShift(node2.GetValue()));
                        }

                    case Token.T_PLUS:
                        {
                            return MakeConstValNode(node1.GetValue().Add(node2.GetValue()));
                        }

                    case Token.T_MINUS:
                        {
                            return MakeConstValNode(node1.GetValue().Subtract(node2.GetValue()));
                        }

                    case Token.T_PERCENT:
                        {
                            return MakeConstValNode(node1.GetValue().Residue(node2.GetValue()));
                        }

                    case Token.T_SLASH:
                        {
                            return MakeConstValNode(node1.GetValue().Divide(node2.GetValue()));
                        }

                    case Token.T_BACKSLASH:
                        {
                            return MakeConstValNode(node1.GetValue().Idiv(node2.GetValue()));
                        }

                    case Token.T_ASTERISK:
                        {
                            return MakeConstValNode(node1.GetValue().Multiply(node2.GetValue()));
                        }
                }
            }
            ExprNode node = new ExprNode();
            mNodeToDeleteVector.AddItem(node);
            node.SetOpecode(opecode);
            node.SetPosition(GetLexPos());
            node.Add(node1);
            node.Add(node2);
            return node;
        }

        public virtual ExprNode MakeNP3(int opecode, ExprNode node1, ExprNode node2, ExprNode
             node3)
        {
            // 三项演算子の最适化とかはしていない？
            ExprNode node = new ExprNode();
            mNodeToDeleteVector.AddItem(node);
            node.SetOpecode(opecode);
            node.SetPosition(GetLexPos());
            node.Add(node1);
            node.Add(node2);
            node.Add(node3);
            return node;
        }

        private int ClearFrame(IntWrapper frame)
        {
            return ClearFrame(frame, -1);
        }

        private int ClearFrame(IntWrapper frame, int @base)
        {
            if (@base == -1)
            {
                @base = mFrameBase;
            }
            if ((frame.value - 1) > mMaxFrameCount)
            {
                mMaxFrameCount = frame.value - 1;
            }
            if ((frame.value - @base) >= 3)
            {
                frame.value = @base;
            }
            else
            {
                if (frame.value > @base)
                {
                    frame.value = @base;
                }
            }
            return frame.value;
        }

        private int ClearFrame(int frame)
        {
            return ClearFrame(frame, -1);
        }

        private int ClearFrame(int frame, int @base)
        {
            if (@base == -1)
            {
                @base = mFrameBase;
            }
            if ((frame - 1) > mMaxFrameCount)
            {
                mMaxFrameCount = frame - 1;
            }
            if ((frame - @base) >= 3)
            {
                frame = @base;
            }
            else
            {
                if (frame > @base)
                {
                    frame = @base;
                }
            }
            return frame;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="CompileException"></exception>
        private int GenNodeCode(IntWrapper frame, ExprNode node, int restype, int reqresaddr
            , InterCodeGenerator.SubParam param)
        {
            if (node == null)
            {
                return 0;
            }
            int resaddr;
            int node_pos = (node != null ? node.GetPosition() : -1);
            switch (node.GetOpecode())
            {
                case Token.T_CONSTVAL:
                    {
                        // constant value
                        if (param.mSubType != stNone)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        if ((restype & RT_NEEDED) == 0)
                        {
                            return 0;
                        }
                        int dp = PutData(node.GetValue());
                        //putCode( VM_CONST, node_pos );
                        //putCode( frame.value, node_pos );
                        //putCode( dp, node_pos );
                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_CONST);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(dp);
                        mCodeAreaPos++;
                        int ret = frame.value;
                        frame.value++;
                        return ret;
                    }

                case Token.T_IF:
                    {
                        // 'if'
                        if ((restype & RT_NEEDED) != 0)
                        {
                            ErrorMsg(Error.CannotGetResult);
                        }
                        int resaddr1 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                            ());
                        bool inv = false;
                        if (!(resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I))
                        {
                            //putCode( VM_TT, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                        }
                        else
                        {
                            if (resaddr1 == GNC_CFLAG_I)
                            {
                                inv = true;
                            }
                        }
                        int addr = mCodeAreaPos;
                        //addJumpList();
                        mJumpList.Add(mCodeAreaPos);
                        //putCode( inv ? VM_JF : VM_JNF, node_pos );
                        //putCode( 0, node_pos ); // *
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JF : VM_JNF);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        GenNodeCode(frame, node.GetNode(0), 0, 0, param);
                        mCodeArea[addr + 1] = (short)(mCodeAreaPos - addr);
                        // patch "*"
                        return 0;
                    }

                case Token.T_INCONTEXTOF:
                    {
                        // 'incontextof'
                        if ((restype & RT_NEEDED) == 0)
                        {
                            return 0;
                        }
                        int resaddr1;
                        int resaddr2;
                        resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, param);
                        resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if (resaddr1 <= 0)
                        {
                            //putCode( VM_CP, node_pos );
                            //putCode( frame.value, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                            resaddr1 = frame.value;
                            frame.value++;
                        }
                        //putCode( VM_CHGTHIS, node_pos );
                        //putCode( resaddr1, node_pos );
                        //putCode( resaddr2, node_pos );
                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_CHGTHIS);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                        mCodeAreaPos++;
                        return resaddr1;
                    }

                case Token.T_COMMA:
                    {
                        // ','
                        GenNodeCode(frame, node.GetNode(0), 0, 0, new InterCodeGenerator.SubParam());
                        return GenNodeCode(frame, node.GetNode(1), restype, reqresaddr, param);
                    }

                case Token.T_SWAP:
                    {
                        // '<->'
                        if ((restype & RT_NEEDED) != 0)
                        {
                            ErrorMsg(Error.CannotGetResult);
                        }
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        int resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if (resaddr1 <= 0)
                        {
                            //putCode( VM_CP, node_pos );
                            //putCode( frame.value, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                            resaddr1 = frame.value;
                            frame.value++;
                        }
                        int resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                        param2.mSubType = stEqual;
                        param2.mSubAddress = resaddr2;
                        GenNodeCode(frame, node.GetNode(0), 0, 0, param2);
                        param2.mSubType = stEqual;
                        param2.mSubAddress = resaddr1;
                        GenNodeCode(frame, node.GetNode(1), 0, 0, param2);
                        return 0;
                    }

                case Token.T_EQUAL:
                    {
                        // '='
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        if ((restype & RT_CFLAG) != 0)
                        {
                            OutputWarning(Error.SubstitutionInBooleanContext, node_pos);
                        }
                        resaddr = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, param);
                        InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                        param2.mSubType = stEqual;
                        param2.mSubAddress = resaddr;
                        GenNodeCode(frame, node.GetNode(0), 0, 0, param2);
                        return resaddr;
                    }

                case Token.T_AMPERSANDEQUAL:
                case Token.T_VERTLINEEQUAL:
                case Token.T_CHEVRONEQUAL:
                case Token.T_MINUSEQUAL:
                case Token.T_PLUSEQUAL:
                case Token.T_PERCENTEQUAL:
                case Token.T_SLASHEQUAL:
                case Token.T_BACKSLASHEQUAL:
                case Token.T_ASTERISKEQUAL:
                case Token.T_LOGICALOREQUAL:
                case Token.T_LOGICALANDEQUAL:
                case Token.T_RARITHSHIFTEQUAL:
                case Token.T_LARITHSHIFTEQUAL:
                case Token.T_RBITSHIFTEQUAL:
                    {
                        // '&=' operator
                        // '|=' operator
                        // '^=' operator
                        // ^-=' operator
                        // '+=' operator
                        // '%=' operator
                        // '/=' operator
                        // '\=' operator
                        // '*=' operator
                        // '||=' operator
                        // '&&=' operator
                        // '>>=' operator
                        // '<<=' operator
                        // '>>>=' operator
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        resaddr = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                        switch (node.GetOpecode())
                        {
                            case Token.T_AMPERSANDEQUAL:
                                {
                                    // this may be sucking...
                                    param2.mSubType = stBitAND;
                                    break;
                                }

                            case Token.T_VERTLINEEQUAL:
                                {
                                    param2.mSubType = stBitOR;
                                    break;
                                }

                            case Token.T_CHEVRONEQUAL:
                                {
                                    param2.mSubType = stBitXOR;
                                    break;
                                }

                            case Token.T_MINUSEQUAL:
                                {
                                    param2.mSubType = stSub;
                                    break;
                                }

                            case Token.T_PLUSEQUAL:
                                {
                                    param2.mSubType = stAdd;
                                    break;
                                }

                            case Token.T_PERCENTEQUAL:
                                {
                                    param2.mSubType = stMod;
                                    break;
                                }

                            case Token.T_SLASHEQUAL:
                                {
                                    param2.mSubType = stDiv;
                                    break;
                                }

                            case Token.T_BACKSLASHEQUAL:
                                {
                                    param2.mSubType = stIDiv;
                                    break;
                                }

                            case Token.T_ASTERISKEQUAL:
                                {
                                    param2.mSubType = stMul;
                                    break;
                                }

                            case Token.T_LOGICALOREQUAL:
                                {
                                    param2.mSubType = stLogOR;
                                    break;
                                }

                            case Token.T_LOGICALANDEQUAL:
                                {
                                    param2.mSubType = stLogAND;
                                    break;
                                }

                            case Token.T_RARITHSHIFTEQUAL:
                                {
                                    param2.mSubType = stSAR;
                                    break;
                                }

                            case Token.T_LARITHSHIFTEQUAL:
                                {
                                    param2.mSubType = stSAL;
                                    break;
                                }

                            case Token.T_RBITSHIFTEQUAL:
                                {
                                    param2.mSubType = stSR;
                                    break;
                                }
                        }
                        param2.mSubAddress = resaddr;
                        return GenNodeCode(frame, node.GetNode(0), restype, reqresaddr, param2);
                    }

                case Token.T_QUESTION:
                    {
                        // '?' ':' operator
                        // three-term operator ( ? : )
                        int resaddr1;
                        int resaddr2;
                        int frame1;
                        int frame2;
                        resaddr = GenNodeCode(frame, node.GetNode(0), RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                            ());
                        bool inv = false;
                        if (!(resaddr == GNC_CFLAG || resaddr == GNC_CFLAG_I))
                        {
                            //putCode( VM_TT, node_pos );
                            //putCode( resaddr, node_pos );
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr);
                            mCodeAreaPos++;
                        }
                        else
                        {
                            if (resaddr == GNC_CFLAG_I)
                            {
                                inv = true;
                            }
                        }
                        int cur_frame = frame.value;
                        int addr1 = mCodeAreaPos;
                        //addJumpList();
                        mJumpList.Add(mCodeAreaPos);
                        //putCode( inv ? VM_JF : VM_JNF, node_pos );
                        //putCode( 0, node_pos ); // patch
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(inv ? VM_JF : VM_JNF);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        resaddr1 = GenNodeCode(frame, node.GetNode(1), restype, reqresaddr, param);
                        if ((restype & RT_CFLAG) != 0)
                        {
                            if (!(resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I))
                            {
                                //putCode( VM_TT, node_pos );
                                //putCode( resaddr1 );
                                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                                mCodeAreaPos++;
                            }
                            else
                            {
                                if (resaddr1 == GNC_CFLAG_I)
                                {
                                    //putCode( VM_NF, node_pos ); // invert flag
                                    if ((mCodeAreaPos) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(VM_NF);
                                    mCodeAreaPos++;
                                }
                            }
                        }
                        else
                        {
                            if ((restype & RT_NEEDED) != 0 && !(resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I
                                ) && resaddr1 <= 0)
                            {
                                //putCode( VM_CP, node_pos );
                                //putCode( frame.value, node_pos );
                                //putCode( resaddr1, node_pos );
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                                mCodeAreaPos++;
                                resaddr1 = frame.value;
                                frame.value++;
                            }
                        }
                        frame1 = frame.value;
                        int addr2 = mCodeAreaPos;
                        //addJumpList();
                        mJumpList.Add(mCodeAreaPos);
                        //putCode( VM_JMP, node_pos );
                        //putCode( 0, node_pos ); // patch
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_JMP);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        mCodeArea[addr1 + 1] = (short)(mCodeAreaPos - addr1);
                        // patch
                        frame.value = cur_frame;
                        resaddr2 = GenNodeCode(frame, node.GetNode(2), restype, reqresaddr, param);
                        if ((restype & RT_CFLAG) != 0)
                        {
                            // condition flag required
                            if (!(resaddr2 == GNC_CFLAG || resaddr2 == GNC_CFLAG_I))
                            {
                                //putCode( VM_TT, node_pos );
                                //putCode( resaddr2 );
                                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                                mCodeAreaPos++;
                            }
                            else
                            {
                                if (resaddr2 == GNC_CFLAG_I)
                                {
                                    //putCode( VM_NF, node_pos ); // invert flag
                                    if ((mCodeAreaPos) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(VM_NF);
                                    mCodeAreaPos++;
                                }
                            }
                        }
                        else
                        {
                            if ((restype & RT_NEEDED) != 0 && !(resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I
                                ) && resaddr1 != resaddr2)
                            {
                                //putCode( VM_CP, node_pos );
                                //putCode( resaddr1, node_pos );
                                //putCode( resaddr2, node_pos );
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                                mCodeAreaPos++;
                                frame.value++;
                            }
                        }
                        frame2 = frame.value;
                        mCodeArea[addr2 + 1] = (short)(mCodeAreaPos - addr2);
                        // patch
                        frame.value = frame2 < frame1 ? frame1 : frame2;
                        return (restype & RT_CFLAG) != 0 ? GNC_CFLAG : resaddr1;
                    }

                case Token.T_LOGICALOR:
                case Token.T_LOGICALAND:
                    {
                        // '||'
                        // '&&'
                        // "logical or" and "locical and"
                        // these process with th "shortcut" :
                        // OR  : does not evaluate right when left results true
                        // AND : does not evaluate right when left results false
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        int resaddr1;
                        int resaddr2;
                        resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                            ());
                        bool inv = false;
                        if (!(resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I))
                        {
                            //putCode( VM_TT, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_TT);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                        }
                        if (resaddr1 == GNC_CFLAG_I)
                        {
                            inv = true;
                        }
                        int addr1 = mCodeAreaPos;
                        //addJumpList();
                        mJumpList.Add(mCodeAreaPos);
                        //putCode( node.getOpecode() == Token.T_LOGICALOR ? (inv?VM_JNF:VM_JF) : (inv?VM_JF:VM_JNF), node_pos );
                        //putCode( 0, node_pos ); // *A
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(node.GetOpecode() == Token.T_LOGICALOR ? (inv ?
                            VM_JNF : VM_JF) : (inv ? VM_JF : VM_JNF));
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED | RT_CFLAG, 0, new InterCodeGenerator.SubParam
                            ());
                        if (!(resaddr2 == GNC_CFLAG || resaddr2 == GNC_CFLAG_I))
                        {
                            //putCode( inv ? VM_TF : VM_TT, node_pos );
                            //putCode( resaddr2, node_pos );
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(inv ? VM_TF : VM_TT);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                            mCodeAreaPos++;
                        }
                        else
                        {
                            if ((inv != false) != (resaddr2 == GNC_CFLAG_I))
                            {
                                //putCode( VM_NF, node_pos ); // invert flag
                                if ((mCodeAreaPos) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_NF);
                                mCodeAreaPos++;
                            }
                        }
                        mCodeArea[addr1 + 1] = (short)(mCodeAreaPos - addr1);
                        // patch
                        if ((restype & RT_CFLAG) == 0)
                        {
                            // requested result type is not condition flag
                            if ((resaddr1 == GNC_CFLAG || resaddr1 == GNC_CFLAG_I) || resaddr1 <= 0)
                            {
                                //putCode( inv ? VM_SETNF : VM_SETF, node_pos );
                                //putCode( frame.value, node_pos );
                                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(inv ? VM_SETNF : VM_SETF);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                mCodeAreaPos++;
                                resaddr1 = frame.value;
                                frame.value++;
                            }
                            else
                            {
                                //putCode( inv ? VM_SETNF : VM_SETF, node_pos );
                                //putCode( resaddr1, node_pos );
                                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(inv ? VM_SETNF : VM_SETF);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                                mCodeAreaPos++;
                            }
                        }
                        return (restype & RT_CFLAG) != 0 ? (inv ? GNC_CFLAG_I : GNC_CFLAG) : resaddr1;
                    }

                case Token.T_INSTANCEOF:
                    {
                        // 'instanceof' operator
                        // instanceof operator
                        int resaddr1;
                        int resaddr2;
                        resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if (resaddr1 <= 0)
                        {
                            //putCode( VM_CP, node_pos );
                            //putCode( frame.value, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                            resaddr1 = frame.value;
                            frame.value++;
                        }
                        resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        //putCode( VM_CHKINS, node_pos );
                        //putCode( resaddr1, node_pos );
                        //putCode( resaddr2, node_pos );
                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_CHKINS);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                        mCodeAreaPos++;
                        return resaddr1;
                    }

                case Token.T_VERTLINE:
                case Token.T_CHEVRON:
                case Token.T_AMPERSAND:
                case Token.T_RARITHSHIFT:
                case Token.T_LARITHSHIFT:
                case Token.T_RBITSHIFT:
                case Token.T_PLUS:
                case Token.T_MINUS:
                case Token.T_PERCENT:
                case Token.T_SLASH:
                case Token.T_BACKSLASH:
                case Token.T_ASTERISK:
                    {
                        // '|' operator
                        // '^' operator
                        // binary '&' operator
                        // '>>' operator
                        // '<<' operator
                        // '>>>' operator
                        // binary '+' operator
                        // '-' operator
                        // '%' operator
                        // '/' operator
                        // '\' operator
                        // binary '*' operator
                        // general two-term operator
                        int resaddr1;
                        int resaddr2;
                        if (param.mSubType != stNone)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if (resaddr1 <= 0)
                        {
                            //putCode( VM_CP, node_pos );
                            //putCode( frame.value, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                            resaddr1 = frame.value;
                            frame.value++;
                        }
                        resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        int code = 0;
                        switch (node.GetOpecode())
                        {
                            case Token.T_VERTLINE:
                                {
                                    // sucking...
                                    code = VM_BOR;
                                    break;
                                }

                            case Token.T_CHEVRON:
                                {
                                    code = VM_BXOR;
                                    break;
                                }

                            case Token.T_AMPERSAND:
                                {
                                    code = VM_BAND;
                                    break;
                                }

                            case Token.T_RARITHSHIFT:
                                {
                                    code = VM_SAR;
                                    break;
                                }

                            case Token.T_LARITHSHIFT:
                                {
                                    code = VM_SAL;
                                    break;
                                }

                            case Token.T_RBITSHIFT:
                                {
                                    code = VM_SR;
                                    break;
                                }

                            case Token.T_PLUS:
                                {
                                    code = VM_ADD;
                                    break;
                                }

                            case Token.T_MINUS:
                                {
                                    code = VM_SUB;
                                    break;
                                }

                            case Token.T_PERCENT:
                                {
                                    code = VM_MOD;
                                    break;
                                }

                            case Token.T_SLASH:
                                {
                                    code = VM_DIV;
                                    break;
                                }

                            case Token.T_BACKSLASH:
                                {
                                    code = VM_IDIV;
                                    break;
                                }

                            case Token.T_ASTERISK:
                                {
                                    code = VM_MUL;
                                    break;
                                }
                        }
                        //putCode( code, node_pos );
                        //putCode( resaddr1, node_pos );
                        //putCode( resaddr2, node_pos );
                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(code);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                        mCodeAreaPos++;
                        return resaddr1;
                    }

                case Token.T_NOTEQUAL:
                case Token.T_EQUALEQUAL:
                case Token.T_DISCNOTEQUAL:
                case Token.T_DISCEQUAL:
                case Token.T_LT:
                case Token.T_GT:
                case Token.T_LTOREQUAL:
                case Token.T_GTOREQUAL:
                    {
                        // '!=' operator
                        // '==' operator
                        // '!==' operator
                        // '===' operator
                        // '<' operator
                        // '>' operator
                        // '<=' operator
                        // '>=' operator
                        // comparison operators
                        int resaddr1;
                        int resaddr2;
                        if (param.mSubType != stNone)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        resaddr1 = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if ((restype & RT_CFLAG) == 0)
                        {
                            if (resaddr1 <= 0)
                            {
                                //putCode( VM_CP, node_pos );
                                //putCode( frame.value, node_pos );
                                //putCode( resaddr1, node_pos );
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                                mCodeAreaPos++;
                                resaddr1 = frame.value;
                                frame.value++;
                            }
                        }
                        resaddr2 = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        int code1 = 0;
                        int code2 = 0;
                        switch (node.GetOpecode())
                        {
                            case Token.T_NOTEQUAL:
                                {
                                    code1 = VM_CEQ;
                                    code2 = VM_SETNF;
                                    break;
                                }

                            case Token.T_EQUALEQUAL:
                                {
                                    code1 = VM_CEQ;
                                    code2 = VM_SETF;
                                    break;
                                }

                            case Token.T_DISCNOTEQUAL:
                                {
                                    code1 = VM_CDEQ;
                                    code2 = VM_SETNF;
                                    break;
                                }

                            case Token.T_DISCEQUAL:
                                {
                                    code1 = VM_CDEQ;
                                    code2 = VM_SETF;
                                    break;
                                }

                            case Token.T_LT:
                                {
                                    code1 = VM_CLT;
                                    code2 = VM_SETF;
                                    break;
                                }

                            case Token.T_GT:
                                {
                                    code1 = VM_CGT;
                                    code2 = VM_SETF;
                                    break;
                                }

                            case Token.T_LTOREQUAL:
                                {
                                    code1 = VM_CGT;
                                    code2 = VM_SETNF;
                                    break;
                                }

                            case Token.T_GTOREQUAL:
                                {
                                    code1 = VM_CLT;
                                    code2 = VM_SETNF;
                                    break;
                                }
                        }
                        //putCode( code1, node_pos );
                        //putCode( resaddr1, node_pos );
                        //putCode( resaddr2, node_pos );
                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(code1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr2);
                        mCodeAreaPos++;
                        if ((restype & RT_CFLAG) == 0)
                        {
                            //putCode( code2, node_pos );
                            //putCode( resaddr1, node_pos );
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            mCodeArea[mCodeAreaPos] = (short)(code2);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr1);
                            mCodeAreaPos++;
                        }
                        return (restype & RT_CFLAG) != 0 ? (code2 == VM_SETNF ? GNC_CFLAG_I : GNC_CFLAG) :
                            resaddr1;
                    }

                case Token.T_EXCRAMATION:
                    {
                        // ここから一气に
                        // pre-positioned '!' operator
                        // logical not
                        if ((param.mSubType != stNone))
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        resaddr = GenNodeCode(frame, node.GetNode(0), restype, reqresaddr, new InterCodeGenerator.SubParam
                            ());
                        if ((restype & RT_CFLAG) == 0)
                        {
                            // value as return value required
                            if (!(resaddr > 0))
                            {
                                //putCode(VM_CP, node_pos);
                                //putCode( frame.value, node_pos);
                                //putCode( resaddr, node_pos);
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                mCodeAreaPos++;
                                resaddr = frame.value;
                                frame.value++;
                            }
                            //putCode(VM_LNOT, node_pos);
                            //putCode( resaddr, node_pos);
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_LNOT);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr);
                            mCodeAreaPos++;
                            return resaddr;
                        }
                        else
                        {
                            // condifion flag required
                            if (!(resaddr == GNC_CFLAG || resaddr == GNC_CFLAG_I))
                            {
                                //putCode(VM_TF, node_pos);
                                //putCode( resaddr);
                                if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_TF);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                mCodeAreaPos++;
                                return GNC_CFLAG;
                            }
                            return resaddr == GNC_CFLAG_I ? GNC_CFLAG : GNC_CFLAG_I;
                        }
                    }

                case Token.T_TILDE:
                case Token.T_SHARP:
                case Token.T_DOLLAR:
                case Token.T_UPLUS:
                case Token.T_UMINUS:
                case Token.T_INVALIDATE:
                case Token.T_ISVALID:
                case Token.T_EVAL:
                case Token.T_INT:
                case Token.T_REAL:
                case Token.T_STRING:
                case Token.T_OCTET:
                    {
                        // invert flag
                        // '~' operator
                        // '#' operator
                        // '$' operator
                        // unary '+' operator
                        // unary '-' operator
                        // 'invalidate' operator
                        // 'isvalid' operator
                        // post-positioned '!' operator
                        // 'int' operator
                        // 'real' operator
                        // 'string' operator
                        // 'octet' operator
                        // general unary operators
                        if ((param.mSubType != stNone))
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        resaddr = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        if (!(resaddr > 0))
                        {
                            //putCode(VM_CP, node_pos);
                            //putCode( frame.value, node_pos);
                            //putCode( resaddr, node_pos);
                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr);
                            mCodeAreaPos++;
                            resaddr = frame.value;
                            frame.value++;
                        }
                        int code = 0;
                        switch (node.GetOpecode())
                        {
                            case Token.T_TILDE:
                                {
                                    code = VM_BNOT;
                                    break;
                                }

                            case Token.T_SHARP:
                                {
                                    code = VM_ASC;
                                    break;
                                }

                            case Token.T_DOLLAR:
                                {
                                    code = VM_CHR;
                                    break;
                                }

                            case Token.T_UPLUS:
                                {
                                    code = VM_NUM;
                                    break;
                                }

                            case Token.T_UMINUS:
                                {
                                    code = VM_CHS;
                                    break;
                                }

                            case Token.T_INVALIDATE:
                                {
                                    code = VM_INV;
                                    break;
                                }

                            case Token.T_ISVALID:
                                {
                                    code = VM_CHKINV;
                                    break;
                                }

                            case Token.T_TYPEOF:
                                {
                                    code = VM_TYPEOF;
                                    break;
                                }

                            case Token.T_EVAL:
                                {
                                    code = (restype & RT_NEEDED) != 0 ? VM_EVAL : VM_EEXP;
                                    // warn if T_EVAL is used in non-global position
                                    if (Tjs.mWarnOnNonGlobalEvalOperator && mContextType != ContextType.TOP_LEVEL)
                                    {
                                        OutputWarning(Error.WarnEvalOperator);
                                    }
                                    break;
                                }

                            case Token.T_INT:
                                {
                                    code = VM_INT;
                                    break;
                                }

                            case Token.T_REAL:
                                {
                                    code = VM_REAL;
                                    break;
                                }

                            case Token.T_STRING:
                                {
                                    code = VM_STR;
                                    break;
                                }

                            case Token.T_OCTET:
                                {
                                    code = VM_OCTET;
                                    break;
                                }
                        }
                        //putCode(code, node_pos);
                        //putCode( resaddr, node_pos);
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(code);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                        mCodeAreaPos++;
                        return resaddr;
                    }

                case Token.T_TYPEOF:
                    {
                        // 'typeof' operator
                        // typeof
                        if ((param.mSubType != stNone))
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        bool haspropnode;
                        ExprNode cnode = node.GetNode(0);
                        if (cnode.GetOpecode() == Token.T_DOT || cnode.GetOpecode() == Token.T_LBRACKET ||
                             cnode.GetOpecode() == Token.T_WITHDOT)
                        {
                            haspropnode = true;
                        }
                        else
                        {
                            haspropnode = false;
                        }
                        if (haspropnode)
                        {
                            // has property access node
                            InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                            param2.mSubType = stTypeOf;
                            return GenNodeCode(frame, cnode, RT_NEEDED, 0, param2);
                        }
                        else
                        {
                            // normal operation
                            resaddr = GenNodeCode(frame, cnode, RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                ());
                            if (!(resaddr > 0))
                            {
                                //putCode(VM_CP, node_pos);
                                //putCode( frame.value, node_pos);
                                //putCode( resaddr, node_pos);
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                mCodeAreaPos++;
                                resaddr = frame.value;
                                frame.value++;
                            }
                            //putCode(VM_TYPEOF, node_pos);
                            //putCode( resaddr, node_pos);
                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                            {
                                ExpandCodeArea();
                            }
                            if (CompileState.mEnableDebugCode)
                            {
                                PutSrcPos(node_pos);
                            }
                            mCodeArea[mCodeAreaPos] = (short)(VM_TYPEOF);
                            mCodeAreaPos++;
                            mCodeArea[mCodeAreaPos] = (short)(resaddr);
                            mCodeAreaPos++;
                            return resaddr;
                        }
                    }

                case Token.T_DELETE:
                case Token.T_INCREMENT:
                case Token.T_DECREMENT:
                case Token.T_POSTINCREMENT:
                case Token.T_POSTDECREMENT:
                    {
                        // 'delete' operator
                        // pre-positioned '++' operator
                        // pre-positioned '--' operator
                        // post-positioned '++' operator
                        // post-positioned '--' operator
                        // delete, typeof, increment and decrement
                        if ((param.mSubType != stNone))
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                        switch (node.GetOpecode())
                        {
                            case Token.T_TYPEOF:
                                {
                                    param2.mSubType = stTypeOf;
                                    break;
                                }

                            case Token.T_DELETE:
                                {
                                    param2.mSubType = stDelete;
                                    break;
                                }

                            case Token.T_INCREMENT:
                                {
                                    param2.mSubType = stPreInc;
                                    break;
                                }

                            case Token.T_DECREMENT:
                                {
                                    param2.mSubType = stPreDec;
                                    break;
                                }

                            case Token.T_POSTINCREMENT:
                                {
                                    param2.mSubType = stPostInc;
                                    break;
                                }

                            case Token.T_POSTDECREMENT:
                                {
                                    param2.mSubType = stPostDec;
                                    break;
                                }
                        }
                        return GenNodeCode(frame, node.GetNode(0), restype, reqresaddr, param2);
                    }

                case Token.T_LPARENTHESIS:
                case Token.T_NEW:
                    {
                        // '( )' operator
                        // 'new' operator
                        // function call or create-new object
                        // does (*node)[0] have a node that acceesses any properties ?
                        bool haspropnode;
                        bool hasnonlocalsymbol;
                        ExprNode cnode = node.GetNode(0);
                        if (node.GetOpecode() == Token.T_LPARENTHESIS && (cnode.GetOpecode() == Token.T_DOT
                             || cnode.GetOpecode() == Token.T_LBRACKET))
                        {
                            haspropnode = true;
                        }
                        else
                        {
                            haspropnode = false;
                        }
                        // does (*node)[0] have a node that accesses non-local functions ?
                        if (node.GetOpecode() == Token.T_LPARENTHESIS && cnode.GetOpecode() == Token.T_SYMBOL)
                        {
                            if (mAsGlobalContextMode)
                            {
                                hasnonlocalsymbol = true;
                            }
                            else
                            {
                                string str = cnode.GetValue().AsString();
                                if (mNamespace.Find(str) == -1)
                                {
                                    hasnonlocalsymbol = true;
                                }
                                else
                                {
                                    hasnonlocalsymbol = false;
                                }
                            }
                        }
                        else
                        {
                            hasnonlocalsymbol = false;
                        }
                        // flag which indicates whether to do direct or indirect call access
                        bool do_direct_access = haspropnode || hasnonlocalsymbol;
                        // reserve frame
                        if (!do_direct_access && (restype & RT_NEEDED) != 0)
                        {
                            frame.value++;
                        }
                        // reserve the frame for a result value
                        // generate function call codes
                        StartFuncArg();
                        int framestart = frame.value;
                        int res;
                        try
                        {
                            // arguments is
                            if (node.GetNode(1).GetSize() == 1 && node.GetNode(1).GetNode(0) == null)
                            {
                            }
                            else
                            {
                                // empty
                                // exist
                                GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                    ());
                            }
                            // compilation of expression that represents the function
                            InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                            if (do_direct_access)
                            {
                                param2.mSubType = stFuncCall;
                                // creates code with stFuncCall
                                res = GenNodeCode(frame, node.GetNode(0), restype, reqresaddr, param2);
                            }
                            else
                            {
                                param2.mSubType = stNone;
                                resaddr = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, param2);
                                // code generatio of function calling
                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                {
                                    ExpandCodeArea();
                                }
                                if (CompileState.mEnableDebugCode)
                                {
                                    PutSrcPos(node_pos);
                                }
                                mCodeArea[mCodeAreaPos] = (short)(node.GetOpecode() == Token.T_NEW ? VM_NEW : VM_CALL
                                    );
                                mCodeAreaPos++;
                                res = ((restype & RT_NEEDED) != 0 ? (framestart - 1) : 0);
                                mCodeArea[mCodeAreaPos] = (short)(res);
                                mCodeAreaPos++;
                                mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                mCodeAreaPos++;
                                // generate argument code
                                GenerateFuncCallArgCode();
                                // clears the frame
                                ClearFrame(frame, framestart);
                            }
                        }
                        finally
                        {
                            EndFuncArg();
                        }
                        return res;
                    }

                case Token.T_ARG:
                    {
                        // a function argument
                        if (node.GetSize() >= 2)
                        {
                            if (node.GetNode(1) != null)
                            {
                                GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                    ());
                            }
                        }
                        if (node.GetNode(0) != null)
                        {
                            ExprNode n = node.GetNode(0);
                            if (n.GetOpecode() == Token.T_EXPANDARG)
                            {
                                // expanding argument
                                if (n.GetNode(0) != null)
                                {
                                    AddFuncArg(GenNodeCode(frame, n.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                        ()), fatExpand);
                                }
                                else
                                {
                                    AddFuncArg(0, fatUnnamedExpand);
                                }
                            }
                            else
                            {
                                AddFuncArg(GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                    ()), fatNormal);
                            }
                        }
                        else
                        {
                            AddFuncArg(0, fatNormal);
                        }
                        return 0;
                    }

                case Token.T_OMIT:
                    {
                        // omitting of the function arguments
                        AddOmitArg();
                        return 0;
                    }

                case Token.T_DOT:
                case Token.T_LBRACKET:
                    {
                        // '.' operator
                        // '[ ]' operator
                        // member access ( direct or indirect )
                        bool direct = node.GetOpecode() == Token.T_DOT;
                        int dp;
                        InterCodeGenerator.SubParam param2 = new InterCodeGenerator.SubParam();
                        param2.mSubType = stNone;
                        resaddr = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, param2);
                        if (direct)
                        {
                            dp = PutData(node.GetNode(1).GetValue());
                        }
                        else
                        {
                            dp = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                ());
                        }
                        switch (param.mSubType)
                        {
                            case stNone:
                            case stIgnorePropGet:
                                {
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    if (param.mSubType == stNone)
                                    {
                                        //putCode(direct ? VM_GPD : VM_GPI, node_pos);
                                        mCodeArea[mCodeAreaPos] = (short)(direct ? VM_GPD : VM_GPI);
                                        mCodeAreaPos++;
                                    }
                                    else
                                    {
                                        //putCode(direct ? VM_GPDS : VM_GPIS, node_pos);
                                        mCodeArea[mCodeAreaPos] = (short)(direct ? VM_GPDS : VM_GPIS);
                                        mCodeAreaPos++;
                                    }
                                    //putCode( frame.value, node_pos);
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    frame.value++;
                                    return frame.value - 1;
                                }

                            case stEqual:
                            case stIgnorePropSet:
                                {
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    if (param.mSubType == stEqual)
                                    {
                                        if (node.GetNode(0).GetOpecode() == Token.T_THIS_PROXY)
                                        {
                                            //putCode(direct ? VM_SPD : VM_SPI, node_pos);
                                            mCodeArea[mCodeAreaPos] = (short)(direct ? VM_SPD : VM_SPI);
                                            mCodeAreaPos++;
                                        }
                                        else
                                        {
                                            //putCode(direct ? VM_SPDE : VM_SPIE, node_pos);
                                            mCodeArea[mCodeAreaPos] = (short)(direct ? VM_SPDE : VM_SPIE);
                                            mCodeAreaPos++;
                                        }
                                    }
                                    else
                                    {
                                        //putCode(direct ? VM_SPDS : VM_SPIS, node_pos);
                                        mCodeArea[mCodeAreaPos] = (short)(direct ? VM_SPDS : VM_SPIS);
                                        mCodeAreaPos++;
                                    }
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    //putCode( param.mSubAddress, node_pos);
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                    mCodeAreaPos++;
                                    return param.mSubAddress;
                                }

                            case stBitAND:
                            case stBitOR:
                            case stBitXOR:
                            case stSub:
                            case stAdd:
                            case stMod:
                            case stDiv:
                            case stIDiv:
                            case stMul:
                            case stLogOR:
                            case stLogAND:
                            case stSAR:
                            case stSAL:
                            case stSR:
                                {
                                    //putCode( param.mSubType + (direct?1:2), node_pos);
                                    // here adds 1 or 2 to the ope-code
                                    // ( see the ope-code's positioning order )
                                    //putCode(( (restype & RT_NEEDED) != 0 ? frame.value : 0), node_pos);
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    //putCode( param.mSubAddress, node_pos);
                                    if ((mCodeAreaPos + 4) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(param.mSubType + (direct ? 1 : 2));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                    mCodeAreaPos++;
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        frame.value++;
                                    }
                                    return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                }

                            case stPreInc:
                            case stPreDec:
                                {
                                    //putCode((param.mSubType == stPreInc ? VM_INC : VM_DEC) + (direct? 1:2), node_pos);
                                    //putCode(((restype & RT_NEEDED) != 0 ? frame.value : 0), node_pos);
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)((param.mSubType == stPreInc ? VM_INC : VM_DEC)
                                        + (direct ? 1 : 2));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        frame.value++;
                                    }
                                    return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                }

                            case stPostInc:
                            case stPostDec:
                                {
                                    int retresaddr = 0;
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        // need result ...
                                        //putCode(direct ? VM_GPD : VM_GPI, node_pos);
                                        //putCode( frame.value, node_pos);
                                        //putCode( resaddr, node_pos);
                                        //putCode( dp, node_pos);
                                        if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)(direct ? VM_GPD : VM_GPI);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(dp);
                                        mCodeAreaPos++;
                                        retresaddr = frame.value;
                                        frame.value++;
                                    }
                                    //putCode( (param.mSubType == stPostInc ? VM_INC : VM_DEC) + (direct? 1:2), node_pos);
                                    //putCode( 0, node_pos );
                                    //putCode( resaddr, node_pos );
                                    //putCode( dp, node_pos );
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)((param.mSubType == stPostInc ? VM_INC : VM_DEC)
                                         + (direct ? 1 : 2));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(0);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    return retresaddr;
                                }

                            case stTypeOf:
                                {
                                    // typeof
                                    //putCode(direct? VM_TYPEOFD:VM_TYPEOFI, node_pos);
                                    //putCode(( (restype & RT_NEEDED) != 0 ? frame.value:0), node_pos);
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(direct ? VM_TYPEOFD : VM_TYPEOFI);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        frame.value++;
                                    }
                                    return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                }

                            case stDelete:
                                {
                                    // deletion
                                    //putCode(direct? VM_DELD:VM_DELI, node_pos);
                                    //putCode(( (restype & RT_NEEDED) != 0 ? frame.value:0), node_pos);
                                    //putCode( resaddr, node_pos);
                                    //putCode( dp, node_pos);
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(direct ? VM_DELD : VM_DELI);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        frame.value++;
                                    }
                                    return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                }

                            case stFuncCall:
                                {
                                    // function call
                                    //putCode(direct ? VM_CALLD:VM_CALLI, node_pos);
                                    //putCode(( (restype & RT_NEEDED) != 0 ? frame.value:0), node_pos); // result target
                                    //putCode( resaddr, node_pos); // the object
                                    //putCode( dp, node_pos); // function name
                                    if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                    {
                                        ExpandCodeArea();
                                    }
                                    if (CompileState.mEnableDebugCode)
                                    {
                                        PutSrcPos(node_pos);
                                    }
                                    mCodeArea[mCodeAreaPos] = (short)(direct ? VM_CALLD : VM_CALLI);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                    mCodeAreaPos++;
                                    mCodeArea[mCodeAreaPos] = (short)(dp);
                                    mCodeAreaPos++;
                                    // generate argument code
                                    GenerateFuncCallArgCode();
                                    // extend frame and return
                                    if ((restype & RT_NEEDED) != 0)
                                    {
                                        frame.value++;
                                    }
                                    return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                }

                            default:
                                {
                                    ErrorMsg(Error.CannotModifyLHS);
                                    return 0;
                                }
                        }
                    }

                case Token.T_SYMBOL:
                    {
                        // symbol
                        // accessing to a variable
                        int n;
                        if (mAsGlobalContextMode)
                        {
                            n = -1;
                        }
                        else
                        {
                            // global mode cannot access local variables
                            string str = node.GetValue().AsString();
                            n = mNamespace.Find(str);
                        }
                        if (n != -1)
                        {
                            bool isstnone = param.mSubType == stNone;
                            if (!isstnone)
                            {
                                switch (param.mSubType)
                                {
                                    case stEqual:
                                        {
                                            // substitution, or like it
                                            //putCode(VM_CP, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            //putCode( param.mSubAddress, node_pos);
                                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                            mCodeAreaPos++;
                                            break;
                                        }

                                    case stBitAND:
                                    case stBitOR:
                                    case stBitXOR:
                                    case stSub:
                                    case stAdd:
                                    case stMod:
                                    case stDiv:
                                    case stIDiv:
                                    case stMul:
                                    case stLogOR:
                                    case stLogAND:
                                    case stSAR:
                                    case stSAL:
                                    case stSR:
                                        {
                                            //putCode(param.mSubType, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            //putCode( param.mSubAddress, node_pos);
                                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(param.mSubType);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                            mCodeAreaPos++;
                                            return (restype & RT_NEEDED) != 0 ? -n - mVariableReserveCount - 1 : 0;
                                        }

                                    case stPreInc:
                                        {
                                            // pre-positioning
                                            //putCode(VM_INC, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_INC);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            return (restype & RT_NEEDED) != 0 ? -n - mVariableReserveCount - 1 : 0;
                                        }

                                    case stPreDec:
                                        {
                                            // pre-
                                            //putCode(VM_DEC, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_DEC);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            return (restype & RT_NEEDED) != 0 ? -n - mVariableReserveCount - 1 : 0;
                                        }

                                    case stPostInc:
                                        {
                                            // post-
                                            if ((restype & RT_NEEDED) != 0)
                                            {
                                                //putCode(VM_CP, node_pos);
                                                //putCode( frame.value, node_pos);
                                                //putCode((-n-mVariableReserveCount-1), node_pos);
                                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                                {
                                                    ExpandCodeArea();
                                                }
                                                if (CompileState.mEnableDebugCode)
                                                {
                                                    PutSrcPos(node_pos);
                                                }
                                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                                mCodeAreaPos++;
                                                frame.value++;
                                            }
                                            //putCode(VM_INC, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_INC);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                        }

                                    case stPostDec:
                                        {
                                            // post-
                                            if ((restype & RT_NEEDED) != 0)
                                            {
                                                //putCode(VM_CP, node_pos);
                                                //putCode( frame.value, node_pos);
                                                //putCode((-n-mVariableReserveCount-1), node_pos);
                                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                                {
                                                    ExpandCodeArea();
                                                }
                                                if (CompileState.mEnableDebugCode)
                                                {
                                                    PutSrcPos(node_pos);
                                                }
                                                mCodeArea[mCodeAreaPos] = (short)(VM_CP);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                                mCodeAreaPos++;
                                                frame.value++;
                                            }
                                            //putCode(VM_DEC, node_pos);
                                            //putCode((-n-mVariableReserveCount-1), node_pos);
                                            if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_DEC);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(-n - mVariableReserveCount - 1);
                                            mCodeAreaPos++;
                                            return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                        }

                                    case stDelete:
                                        {
                                            // deletion
                                            string str = node.GetValue().AsString();
                                            mNamespace.Remove(str);
                                            if ((restype & RT_NEEDED) != 0)
                                            {
                                                int dp = PutData(new Variant(1));
                                                // true
                                                //putCode(VM_CONST, node_pos);
                                                //putCode( frame.value, node_pos);
                                                //putCode( dp, node_pos);
                                                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                                {
                                                    ExpandCodeArea();
                                                }
                                                if (CompileState.mEnableDebugCode)
                                                {
                                                    PutSrcPos(node_pos);
                                                }
                                                mCodeArea[mCodeAreaPos] = (short)(VM_CONST);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                                mCodeAreaPos++;
                                                mCodeArea[mCodeAreaPos] = (short)(dp);
                                                mCodeAreaPos++;
                                                return frame.value - 1;
                                            }
                                            return 0;
                                        }

                                    default:
                                        {
                                            ErrorMsg(Error.CannotModifyLHS);
                                            break;
                                        }
                                }
                                return 0;
                            }
                            else
                            {
                                // read
                                string str = node.GetValue().AsString();
                                int n1 = mNamespace.Find(str);
                                return -n1 - mVariableReserveCount - 1;
                            }
                        }
                        else
                        {
                            // n==-1 ( indicates the variable is not found in the local  )
                            // assume the variable is in "this".
                            // make nodes that refer "this" and process it
                            ExprNode nodep = new ExprNode();
                            nodep.SetOpecode(Token.T_DOT);
                            nodep.SetPosition(node_pos);
                            ExprNode node1 = new ExprNode();
                            mNodeToDeleteVector.AddItem(node1);
                            nodep.Add(node1);
                            node1.SetOpecode(mAsGlobalContextMode ? Token.T_GLOBAL : Token.T_THIS_PROXY);
                            node1.SetPosition(node_pos);
                            ExprNode node2 = new ExprNode();
                            mNodeToDeleteVector.AddItem(node2);
                            nodep.Add(node2);
                            node2.SetOpecode(Token.T_SYMBOL);
                            node2.SetPosition(node_pos);
                            node2.SetValue(node.GetValue());
                            return GenNodeCode(frame, nodep, restype, reqresaddr, param);
                        }
                    }

                case Token.T_IGNOREPROP:
                case Token.T_PROPACCESS:
                    {
                        // unary '&' operator
                        // unary '*' operator
                        if (node.GetOpecode() == (Tjs.mUnaryAsteriskIgnoresPropAccess ? Token.T_PROPACCESS
                             : Token.T_IGNOREPROP))
                        {
                            // unary '&' operator
                            // substance accessing (ignores property operation)
                            InterCodeGenerator.SubParam sp = new InterCodeGenerator.SubParam(param);
                            if (sp.mSubType == stNone)
                            {
                                sp.mSubType = stIgnorePropGet;
                            }
                            else
                            {
                                if (sp.mSubType == stEqual)
                                {
                                    sp.mSubType = stIgnorePropSet;
                                }
                                else
                                {
                                    ErrorMsg(Error.CannotModifyLHS);
                                }
                            }
                            return GenNodeCode(frame, node.GetNode(0), restype, reqresaddr, sp);
                        }
                        else
                        {
                            // unary '*' operator
                            // force property access
                            resaddr = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                ());
                            switch (param.mSubType)
                            {
                                case stNone:
                                    {
                                        // read from property object
                                        //putCode(VM_GETP, node_pos);
                                        //putCode( frame.value, node_pos);
                                        //putCode( resaddr, node_pos);
                                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)(VM_GETP);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        frame.value++;
                                        return frame.value - 1;
                                    }

                                case stEqual:
                                    {
                                        // write to property object
                                        //putCode(VM_SETP, node_pos);
                                        //putCode( resaddr, node_pos);
                                        //putCode( param.mSubAddress, node_pos);
                                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)(VM_SETP);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                        mCodeAreaPos++;
                                        return param.mSubAddress;
                                    }

                                case stBitAND:
                                case stBitOR:
                                case stBitXOR:
                                case stSub:
                                case stAdd:
                                case stMod:
                                case stDiv:
                                case stIDiv:
                                case stMul:
                                case stLogOR:
                                case stLogAND:
                                case stSAR:
                                case stSAL:
                                case stSR:
                                    {
                                        //putCode(param.mSubType + 3, node_pos);
                                        // +3 : property access
                                        // ( see the ope-code's positioning order )
                                        //putCode(((restype & RT_NEEDED) != 0 ? frame.value: 0), node_pos);
                                        //putCode( resaddr, node_pos);
                                        //putCode( param.mSubAddress, node_pos);
                                        if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)(param.mSubType + 3);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(param.mSubAddress);
                                        mCodeAreaPos++;
                                        if ((restype & RT_NEEDED) != 0)
                                        {
                                            frame.value++;
                                        }
                                        return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                    }

                                case stPreInc:
                                case stPreDec:
                                    {
                                        //putCode((param.mSubType == stPreInc ? VM_INC : VM_DEC) + 3, node_pos);
                                        //putCode(((restype & RT_NEEDED) != 0 ? frame.value : 0), node_pos);
                                        //putCode( resaddr, node_pos);
                                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)((param.mSubType == stPreInc ? VM_INC : VM_DEC)
                                            + 3);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(((restype & RT_NEEDED) != 0 ? frame.value : 0));
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        if ((restype & RT_NEEDED) != 0)
                                        {
                                            frame.value++;
                                        }
                                        return (restype & RT_NEEDED) != 0 ? frame.value - 1 : 0;
                                    }

                                case stPostInc:
                                case stPostDec:
                                    {
                                        int retresaddr = 0;
                                        if ((restype & RT_NEEDED) != 0)
                                        {
                                            // need result ...
                                            //putCode(VM_GETP, node_pos);
                                            //putCode( frame.value, node_pos);
                                            //putCode( resaddr, node_pos);
                                            if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                            {
                                                ExpandCodeArea();
                                            }
                                            if (CompileState.mEnableDebugCode)
                                            {
                                                PutSrcPos(node_pos);
                                            }
                                            mCodeArea[mCodeAreaPos] = (short)(VM_GETP);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(frame.value);
                                            mCodeAreaPos++;
                                            mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                            mCodeAreaPos++;
                                            retresaddr = frame.value;
                                            frame.value++;
                                        }
                                        //putCode((param.mSubType == stPostInc ? VM_INC : VM_DEC) + 3, node_pos);
                                        //putCode( 0, node_pos);
                                        //putCode( resaddr, node_pos);
                                        if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                                        {
                                            ExpandCodeArea();
                                        }
                                        if (CompileState.mEnableDebugCode)
                                        {
                                            PutSrcPos(node_pos);
                                        }
                                        mCodeArea[mCodeAreaPos] = (short)((param.mSubType == stPostInc ? VM_INC : VM_DEC)
                                             + 3);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(0);
                                        mCodeAreaPos++;
                                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                                        mCodeAreaPos++;
                                        return retresaddr;
                                    }

                                default:
                                    {
                                        ErrorMsg(Error.CannotModifyLHS);
                                        return 0;
                                    }
                            }
                        }
                    }

                case Token.T_SUPER:
                    {
                        // 'super'
                        // refer super class
                        //int dp;
                        ExprNode node1;
                        if (mParent != null && mParent.mContextType == ContextType.PROPERTY)
                        {
                            if ((node1 = mParent.mParent.mSuperClassExpr) == null)
                            {
                                ErrorMsg(Error.CannotGetSuper);
                                return 0;
                            }
                        }
                        else
                        {
                            if (mParent == null || (node1 = mParent.mSuperClassExpr) == null)
                            {
                                ErrorMsg(Error.CannotGetSuper);
                                return 0;
                            }
                        }
                        mAsGlobalContextMode = true;
                        // the code must be generated in global context
                        try
                        {
                            resaddr = GenNodeCode(frame, node1, restype, reqresaddr, param);
                        }
                        finally
                        {
                            mAsGlobalContextMode = false;
                        }
                        return resaddr;
                    }

                case Token.T_THIS:
                    {
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        return -1;
                    }

                case Token.T_THIS_PROXY:
                    {
                        // this-proxy is a special register that points
                        // both "objthis" and "global"
                        // if refering member is not in "objthis", this-proxy
                        // refers "global".
                        return -mVariableReserveCount;
                    }

                case Token.T_WITHDOT:
                    {
                        // unary '.' operator
                        // dot operator omitting object name
                        ExprNode nodep = new ExprNode();
                        nodep.SetOpecode(Token.T_DOT);
                        nodep.SetPosition(node_pos);
                        ExprNode node1 = new ExprNode();
                        mNodeToDeleteVector.AddItem(node1);
                        nodep.Add(node1);
                        node1.SetOpecode(Token.T_WITHDOT_PROXY);
                        node1.SetPosition(node_pos);
                        nodep.Add(node.GetNode(0));
                        return GenNodeCode(frame, nodep, restype, reqresaddr, param);
                    }

                case Token.T_WITHDOT_PROXY:
                    {
                        // virtual left side of "." operator which omits object
                        // search in NestVector
                        int i = mNestVector.Count - 1;
                        for (; i >= 0; i--)
                        {
                            InterCodeGenerator.NestData data = mNestVector[i];
                            if (data.Type == ntWith)
                            {
                                // found
                                return data.RefRegister;
                            }
                        }
                        goto case Token.T_GLOBAL;
                    }

                case Token.T_GLOBAL:
                    {
                        // not found in NestVector ...
                        // NO "break" HERE!!!!!! (pass thru to global)
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        if ((restype & RT_NEEDED) == 0)
                        {
                            return 0;
                        }
                        //putCode(VM_GLOBAL, node_pos);
                        //putCode( frame.value, node_pos);
                        if ((mCodeAreaPos + 1) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_GLOBAL);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        frame.value++;
                        return frame.value - 1;
                    }

                case Token.T_INLINEARRAY:
                    {
                        // inline array
                        int arraydp = PutData(new Variant("Array"));
                        //	global %frame0
                        //	gpd %frame1, %frame0 . #arraydp // #arraydp = Array
                        int frame0 = frame.value;
                        if ((mCodeAreaPos + 12) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        //putCode(VM_GLOBAL, node_pos);
                        //putCode((frame.value+0), node_pos);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GLOBAL);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        //putCode(VM_GPD, node_pos);
                        //putCode((frame.value+1), node_pos);
                        //putCode((frame.value+0), node_pos);
                        //putCode( arraydp, node_pos);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GPD);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(arraydp);
                        mCodeAreaPos++;
                        //	new %frame0, %frame1()
                        //putCode(VM_NEW, node_pos);
                        //putCode((frame.value+0), node_pos);
                        //putCode((frame.value+1), node_pos);
                        //putCode(0);  // argument count for "new Array"
                        mCodeArea[mCodeAreaPos] = (short)(VM_NEW);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        //	const %frame1, #zerodp
                        int zerodp = PutData(new Variant(0));
                        //putCode(VM_CONST, node_pos);
                        //putCode((frame.value+1), node_pos);
                        //putCode( zerodp, node_pos);
                        mCodeArea[mCodeAreaPos] = (short)(VM_CONST);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(zerodp);
                        mCodeAreaPos++;
                        frame.value += 2;
                        mArrayArgStack.Push(new InterCodeGenerator.ArrayArg());
                        mArrayArgStack.Peek().Object = frame0;
                        mArrayArgStack.Peek().Counter = frame0 + 1;
                        int nodesize = node.GetSize();
                        if (node.GetSize() == 1 && node.GetNode(0).GetNode(0) == null)
                        {
                        }
                        else
                        {
                            // the element is empty
                            for (int i = 0; i < nodesize; i++)
                            {
                                GenNodeCode(frame, node.GetNode(i), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                    ());
                            }
                        }
                        // elements
                        mArrayArgStack.Pop();
                        return (restype & RT_NEEDED) != 0 ? (frame0) : 0;
                    }

                case Token.T_ARRAYARG:
                    {
                        // an element of inline array
                        int framestart = frame.value;
                        resaddr = node.GetNode(0) != null ? GenNodeCode(frame, node.GetNode(0), RT_NEEDED
                            , 0, new InterCodeGenerator.SubParam()) : 0;
                        // spis %object.%count, %resaddr
                        //putCode(VM_SPIS, node_pos);
                        //putCode((mArrayArgStack.peek().Object));
                        //putCode((mArrayArgStack.peek().Counter));
                        //putCode( resaddr);
                        if ((mCodeAreaPos + 5) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_SPIS);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(mArrayArgStack.Peek().Object);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(mArrayArgStack.Peek().Counter);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(resaddr);
                        mCodeAreaPos++;
                        // inc %count
                        //putCode(VM_INC);
                        //putCode((mArrayArgStack.peek().Counter));
                        mCodeArea[mCodeAreaPos] = (short)(VM_INC);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(mArrayArgStack.Peek().Counter);
                        mCodeAreaPos++;
                        ClearFrame(frame, framestart);
                        return 0;
                    }

                case Token.T_INLINEDIC:
                    {
                        // inline dictionary
                        int dicdp = PutData(new Variant("Dictionary"));
                        //	global %frame0
                        //	gpd %frame1, %frame0 . #dicdp // #dicdp = Dictionary
                        int frame0 = frame.value;
                        if ((mCodeAreaPos + 9) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        //putCode(VM_GLOBAL, node_pos);
                        //putCode((frame.value+0), node_pos);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GLOBAL);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        //putCode(VM_GPD, node_pos);
                        //putCode((frame.value+1), node_pos);
                        //putCode((frame.value+0), node_pos);
                        //putCode( dicdp, node_pos);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GPD);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(dicdp);
                        mCodeAreaPos++;
                        //	new %frame0, %frame1()
                        //putCode(VM_NEW, node_pos);
                        //putCode((frame.value+0), node_pos);
                        //putCode((frame.value+1), node_pos);
                        //putCode(0);  // argument count for "Dictionary" class
                        mCodeArea[mCodeAreaPos] = (short)(VM_NEW);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        frame.value += 2;
                        ClearFrame(frame, frame0 + 1);
                        // clear register at frame+1
                        mArrayArgStack.Push(new InterCodeGenerator.ArrayArg());
                        mArrayArgStack.Peek().Object = frame0;
                        int nodesize = node.GetSize();
                        for (int i = 0; i < nodesize; i++)
                        {
                            GenNodeCode(frame, node.GetNode(i), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                                ());
                        }
                        // element
                        mArrayArgStack.Pop();
                        return (restype & RT_NEEDED) != 0 ? (frame0) : 0;
                    }

                case Token.T_DICELM:
                    {
                        // an element of inline dictionary
                        int framestart = frame.value;
                        int name;
                        int value;
                        name = GenNodeCode(frame, node.GetNode(0), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        value = GenNodeCode(frame, node.GetNode(1), RT_NEEDED, 0, new InterCodeGenerator.SubParam
                            ());
                        // spis %object.%name, %value
                        //putCode(VM_SPIS, node_pos);
                        //putCode((mArrayArgStack.peek().Object));
                        //putCode( name);
                        //putCode( value);
                        if ((mCodeAreaPos + 3) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        mCodeArea[mCodeAreaPos] = (short)(VM_SPIS);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(mArrayArgStack.Peek().Object);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(name);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(value);
                        mCodeAreaPos++;
                        ClearFrame(frame, framestart);
                        return 0;
                    }

                case Token.T_REGEXP:
                    {
                        // constant regular expression
                        if ((restype & RT_NEEDED) == 0)
                        {
                            return 0;
                        }
                        int regexpdp = PutData(new Variant("RegExp"));
                        int patdp = PutData(node.GetValue());
                        int compiledp = PutData(new Variant("_compile"));
                        // global %frame0
                        //	gpd %frame1, %frame0 . #regexpdp // #regexpdp = RegExp
                        int frame0 = frame.value;
                        if ((mCodeAreaPos + 18) >= mCodeArea.Length)
                        {
                            ExpandCodeArea();
                        }
                        if (CompileState.mEnableDebugCode)
                        {
                            PutSrcPos(node_pos);
                        }
                        //putCode(VM_GLOBAL, node_pos);
                        //putCode( frame.value);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GLOBAL);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        //putCode(VM_GPD);
                        //putCode((frame.value + 1));
                        //putCode( frame.value);
                        //putCode( regexpdp);
                        mCodeArea[mCodeAreaPos] = (short)(VM_GPD);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(regexpdp);
                        mCodeAreaPos++;
                        // const frame2, patdp;
                        //putCode(VM_CONST);
                        //putCode((frame.value + 2));
                        //putCode( patdp);
                        mCodeArea[mCodeAreaPos] = (short)(VM_CONST);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 2);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(patdp);
                        mCodeAreaPos++;
                        // new frame0 , frame1();
                        //putCode(VM_NEW);
                        //putCode( frame.value);
                        //putCode((frame.value+1));
                        //putCode(0);
                        mCodeArea[mCodeAreaPos] = (short)(VM_NEW);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        // calld 0, frame0 . #compiledp(frame2)
                        //putCode(VM_CALLD);
                        //putCode( 0);
                        //putCode( frame0);
                        //putCode( compiledp);
                        //putCode(1);
                        //putCode((frame.value+2));
                        mCodeArea[mCodeAreaPos] = (short)(VM_CALLD);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(0);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame0);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(compiledp);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(1);
                        mCodeAreaPos++;
                        mCodeArea[mCodeAreaPos] = (short)(frame.value + 2);
                        mCodeAreaPos++;
                        frame.value += 3;
                        ClearFrame(frame, frame0 + 1);
                        return frame0;
                    }

                case Token.T_VOID:
                    {
                        if (param.mSubType != 0)
                        {
                            ErrorMsg(Error.CannotModifyLHS);
                        }
                        if ((restype & RT_NEEDED) == 0)
                        {
                            return 0;
                        }
                        return 0;
                    }
            }
            // 0 is always void
            return 0;
        }

        /// <exception cref="CompileException"></exception>
        private void AddOmitArg()
        {
            // omit of the function arguments
            if (mContextType != ContextType.FUNCTION && mContextType != ContextType.EXPR_FUNCTION)
            {
                ErrorMsg(Error.CannotOmit);
            }
            mFuncArgStack.Peek().IsOmit = true;
        }

        private void EndFuncArg()
        {
            // notify the end of function arguments
            mFuncArgStack.Pop();
        }

        private void StartFuncArg()
        {
            // notify the start of function arguments
            // create a stack for function arguments
            InterCodeGenerator.FuncArg arg = new InterCodeGenerator.FuncArg();
            mFuncArgStack.Push(arg);
        }

        private void OutputWarning(string mes)
        {
            OutputWarning(mes, -1);
        }

        private void OutputWarning(string mes, int pos)
        {
            int errpos = pos == -1 ? mBlock.GetLexicalAnalyzer().GetCurrentPosition() : pos;
            StringBuilder strBuilder = new StringBuilder(512);
            strBuilder.Append(Error.Warning);
            strBuilder.Append(mes);
            strBuilder.Append(" at ");
            strBuilder.Append(mBlock.GetName());
            strBuilder.Append(" line ");
            strBuilder.Append((1 + mBlock.SrcPosToLine(errpos)).ToString());
            //mBlock.getTJS().outputToConsole( strBuilder.toString() );
            Tjs.OutputToConsole(strBuilder.ToString());
            strBuilder = null;
        }

        private void AddFuncArg(int addr, int type)
        {
            // add a function argument
            // addr = register address to add
            mFuncArgStack.Peek().ArgVector.AddItem(new InterCodeGenerator.FuncArgItem(addr, type
                ));
            if (type == fatExpand || type == fatUnnamedExpand)
            {
                mFuncArgStack.Peek().HasExpand = true;
            }
        }

        // has expanding node
        //void addJumpList() { mJumpList.add( mCodeAreaPos ); }
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public virtual void Commit()
        {
            // some context-related processing at final, and commits it
            if (mContextType == ContextType.CLASS)
            {
                // clean up super class proxy
                if (mSuperClassGetter != null)
                {
                    mSuperClassGetter.Commit();
                }
            }
            if (mContextType != ContextType.PROPERTY && mContextType != ContextType.SUPER_CLASS_GETTER)
            {
                int lexpos = GetLexPos();
                //putCode( VM_SRV, lexpos );
                //putCode( 0 );
                if ((mCodeAreaPos + 2) >= mCodeArea.Length)
                {
                    ExpandCodeArea();
                }
                if (CompileState.mEnableDebugCode)
                {
                    PutSrcPos(lexpos);
                }
                mCodeArea[mCodeAreaPos] = (short)(VM_SRV);
                mCodeAreaPos++;
                mCodeArea[mCodeAreaPos] = (short)(0);
                mCodeAreaPos++;
                //putCode( VM_RET, -1 );
                mCodeArea[mCodeAreaPos] = (short)(VM_RET);
                mCodeAreaPos++;
            }
            RegisterFunction();
            if (mContextType != ContextType.PROPERTY && mContextType != ContextType.SUPER_CLASS_GETTER)
            {
                FixCode();
            }
            mDataArray = new Variant[mDataArea.Count];
            mDataArray = Sharpen.Collections.ToArray(mDataArea, mDataArray);
            mDataArea.Clear();
            mDataArea = null;
            if (mContextType == ContextType.SUPER_CLASS_GETTER)
            {
                mMaxVariableCount = 2;
            }
            else
            {
                // always 2
                mMaxVariableCount = mNamespace.GetMaxCount();
            }
            mSuperClassExpr = null;
            ClearNodesToDelete();
            mCode = new short[mCodeAreaPos];
            System.Array.Copy(mCodeArea, 0, mCode, 0, mCodeAreaPos);
            // set object type info for debugging
            // we do thus nasty thing because the std::vector does not free its storage
            // even we call 'clear' method...
            //mNodeToDeleteVector = null; mNodeToDeleteVector = new VectorWrap<ExprNode>(); 直前でクリアされているはず
            mCurrentNodeVector.Clear();
            // mCurrentNodeVector = null; mCurrentNodeVector = new VectorWrap<ExprNode>();
            mFuncArgStack = null;
            mFuncArgStack = new Stack<InterCodeGenerator.FuncArg>();
            mArrayArgStack = null;
            mArrayArgStack = new Stack<InterCodeGenerator.ArrayArg>();
            mNestVector = null;
            mNestVector = new VectorWrap<InterCodeGenerator.NestData>();
            mJumpList = null;
            mJumpList = new IntVector();
            mFixList = null;
            mFixList = new AList<InterCodeGenerator.FixData>();
            mNonLocalFunctionDeclVector = null;
            mNonLocalFunctionDeclVector = new VectorWrap<InterCodeGenerator.NonLocalFunctionDecl
                >();
        }

        private void ClearNodesToDelete()
        {
            if (mNodeToDeleteVector.Count > 0)
            {
                int count = mNodeToDeleteVector.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    mNodeToDeleteVector[i].Clear();
                }
            }
            mNodeToDeleteVector.Clear();
        }

        private void FixCode()
        {
            // code re-positioning and patch processing
            // OriginalTODO: InterCodeContext::fixCode fasten the algorithm
            // create 'regmember' instruction to register class members to
            // newly created object
            if (mContextType == ContextType.CLASS)
            {
                // generate a code
                //ByteBuffer buff = ByteBuffer.allocate(2);
                //buff.order( ByteOrder.nativeOrder() );
                //ShortBuffer code = buff.asShortBuffer();
                //code.clear();
                //code.put( (short) VM_REGMEMBER );
                short[] newbuff = new short[1];
                ShortBuffer code = ShortBuffer.Wrap(newbuff);
                code.Clear();
                code.Put((short)VM_REGMEMBER);
                // make a patch information
                // use FunctionRegisterCodePoint for insertion point
                mFixList.AddItem(new InterCodeGenerator.FixData(mFunctionRegisterCodePoint, 0, 1,
                    code, true));
            }
            // process funtion reservation to enable backward reference of
            // global/method functions
            if (mNonLocalFunctionDeclVector.Count >= 1)
            {
                if (mMaxFrameCount < 1)
                {
                    mMaxFrameCount = 1;
                }
                //std::vector<tNonLocalFunctionDecl>::iterator func;
                Iterator<InterCodeGenerator.NonLocalFunctionDecl> func;
                // make function registration code to objthis
                // compute codesize
                int codesize = 2;
                func = mNonLocalFunctionDeclVector.Iterator();
                while (func.HasNext())
                {
                    InterCodeGenerator.NonLocalFunctionDecl dec = (InterCodeGenerator.NonLocalFunctionDecl
                        )func.Next();
                    if (dec.ChangeThis)
                    {
                        codesize += 10;
                    }
                    else
                    {
                        codesize += 7;
                    }
                }
                short[] newbuff = new short[codesize];
                //ByteBuffer buff = ByteBuffer.allocate(codesize*2);
                //buff.order( ByteOrder.nativeOrder() );
                //ShortBuffer code = buff.asShortBuffer();
                ShortBuffer code = ShortBuffer.Wrap(newbuff);
                code.Clear();
                // generate code
                func = mNonLocalFunctionDeclVector.Iterator();
                while (func.HasNext())
                {
                    InterCodeGenerator.NonLocalFunctionDecl dec = func.Next();
                    // const %1, #funcdata
                    code.Put(VM_CONST);
                    code.Put((short)1);
                    code.Put((short)dec.DataPos);
                    // chgthis %1, %-1
                    if (dec.ChangeThis)
                    {
                        code.Put(VM_CHGTHIS);
                        code.Put((short)1);
                        code.Put((short)-1);
                    }
                    // spds %-1.#funcname, %1
                    code.Put(VM_SPDS);
                    code.Put((short)-1);
                    // -1 =  objthis
                    code.Put((short)dec.NameDataPos);
                    code.Put((short)1);
                }
                // cl %1
                code.Put(VM_CL);
                code.Put((short)1);
                code.Flip();
                // make a patch information
                mFixList.AddItem(new InterCodeGenerator.FixData(mFunctionRegisterCodePoint, 0, codesize
                    , code, true));
                mNonLocalFunctionDeclVector.Clear();
            }
            // sort SourcePosVector
            SortSourcePos();
            // re-position patch
            int count = mFixList.Count;
            for (int i = 0; i < count; i++)
            {
                InterCodeGenerator.FixData fix = mFixList[i];
                int jcount = mJumpList.Size();
                for (int j = 0; j < jcount; j++)
                {
                    int jmp = mJumpList.Get(j);
                    int jmptarget = mCodeArea[jmp + 1] + jmp;
                    if (jmp >= fix.StartIP && jmp < fix.Size + fix.StartIP)
                    {
                        // jmp is in the re-positioning target -> delete
                        mJumpList.Remove(j);
                        if ((j + 1) < jcount)
                        {
                            j++;
                            jmp = mJumpList.Get(j);
                        }
                        else
                        {
                            jmp = 0;
                        }
                    }
                    else
                    {
                        if (fix.BeforeInsertion ? (jmptarget < fix.StartIP) : (jmptarget <= fix.StartIP)
                            && jmp > fix.StartIP + fix.Size || jmp < fix.StartIP && jmptarget >= fix.StartIP
                             + fix.Size)
                        {
                            // jmp and its jumping-target is in the re-positioning target
                            int v = mCodeArea[jmp + 1];
                            v += fix.NewSize - fix.Size;
                            mCodeArea[jmp + 1] = (short)v;
                        }
                    }
                    if (jmp >= fix.StartIP + fix.Size)
                    {
                        // fix up jmp
                        jmp += fix.NewSize - fix.Size;
                        mJumpList.Set(j, Sharpen.Extensions.ValueOf(jmp));
                    }
                }
                // move the code
                if (fix.NewSize > fix.Size)
                {
                    // when code inflates on fixing
                    //final int newBufferSize = 2 * (mCodeArea.position() + fix.NewSize - fix.Size);
                    //ByteBuffer buff = ByteBuffer.allocate( newBufferSize );
                    //buff.order( ByteOrder.nativeOrder() );
                    //ShortBuffer ibuff = buff.asShortBuffer();
                    int newBufferSize = (mCodeAreaPos + fix.NewSize - fix.Size);
                    short[] newbuff = new short[newBufferSize];
                    //ShortBuffer ibuff = ShortBuffer.wrap(newbuff);
                    System.Array.Copy(mCodeArea, 0, newbuff, 0, mCodeAreaPos);
                    mCodeArea = null;
                    mCodeArea = newbuff;
                }
                //ibuff.clear();
                //mCodeArea.flip();
                //ibuff.put( mCodeArea );
                //mCodeArea = null;
                //mCodeArea = ibuff;
                if (mCodeAreaPos - (fix.StartIP + fix.Size) > 0)
                {
                    // move the existing code
                    int dst = fix.StartIP + fix.NewSize;
                    int src = fix.StartIP + fix.Size;
                    int size = mCodeAreaPos - (fix.StartIP + fix.Size);
                    //ByteBuffer buff = ByteBuffer.allocate(size*2);
                    //buff.order( ByteOrder.nativeOrder() );
                    //ShortBuffer ibuff = buff.asShortBuffer();
                    short[] newbuff = new short[size];
                    //ShortBuffer ibuff = ShortBuffer.wrap(newbuff);
                    //ibuff.clear();
                    //for( int j = 0; j < size; j++ ) { // テンポラリへコピー
                    //ibuff.put( j, mCodeArea[src+j] );
                    //}
                    //for( int j = 0; j < size; j++ ) {
                    ///	mCodeArea[dst+j] = ibuff.get(j);
                    //}
                    //ibuff = null;
                    System.Array.Copy(mCodeArea, src, newbuff, 0, size);
                    System.Array.Copy(newbuff, 0, mCodeArea, dst, size);
                    // move sourcepos
                    if (CompileState.mEnableDebugCode)
                    {
                        int srcSize = mSrcPosArrayPos;
                        long[] srcPos = mSourcePosArray;
                        for (int j_1 = 0; j_1 < srcSize; j_1++)
                        {
                            long val = srcPos[j_1];
                            val = (long)(((ulong)val) >> 32);
                            if (val >= fix.StartIP + fix.Size)
                            {
                                val += fix.NewSize - fix.Size;
                                val = (val << 32) | (srcPos[j_1] & unchecked((long)(0xFFFFFFFFL)));
                                srcPos[j_1] = val;
                            }
                        }
                    }
                }
                if (fix.NewSize > 0 && fix.Code != null)
                {
                    // copy the new code
                    int size = fix.NewSize;
                    int dst = fix.StartIP;
                    for (int j_1 = 0; j_1 < size; j_1++)
                    {
                        mCodeArea[dst + j_1] = fix.Code.Get(j_1);
                    }
                }
                mCodeAreaPos = mCodeAreaPos + fix.NewSize - fix.Size;
            }
            // eliminate redundant jump codes
            int jcount_1 = mJumpList.Size();
            for (int i_1 = 0; i_1 < jcount_1; i_1++)
            {
                int jmp = mJumpList.Get(i_1);
                int jumptarget = mCodeArea[jmp + 1] + jmp;
                int jumpcode = mCodeArea[jmp];
                int addr = jmp;
                addr += mCodeArea[addr + 1];
                for (; ; )
                {
                    if (mCodeArea[addr] == VM_JMP || (mCodeArea[addr] == jumpcode && (jumpcode == VM_JF
                         || jumpcode == VM_JNF)))
                    {
                        // simple jump code or
                        // JF after JF or JNF after JNF
                        jumptarget = mCodeArea[addr + 1] + addr;
                        // skip jump after jump
                        if (mCodeArea[addr + 1] != 0)
                        {
                            addr += mCodeArea[addr + 1];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        // must be an error
                        if (mCodeArea[addr] == VM_JF && jumpcode == VM_JNF || mCodeArea[addr] == VM_JNF &&
                             jumpcode == VM_JF)
                        {
                            // JF after JNF or JNF after JF
                            jumptarget = addr + 2;
                            // jump code after jump will not jump
                            addr += 2;
                        }
                        else
                        {
                            // other codes
                            break;
                        }
                    }
                }
                mCodeArea[jmp + 1] = (short)(jumptarget - jmp);
            }
            mJumpList.Clear();
            mFixList.Clear();
        }

        //private static void bubbleSort( LongBuffer a, int n ) {
        //	long[] array = a.array();
        //	Arrays.sort(array,0,n);
        //}
        private void SortSourcePos()
        {
            // 上位をcodePos, 下位をsourcePos とする, codePos でのソートなので下位は气にせずソートししまう
            if (!mSourcePosArraySorted && mSourcePosArray != null)
            {
                //quickSort( mSourcePosArray, 0, mSourcePosArray.position()-1 );
                //bubbleSort( mSourcePosArray, mSourcePosArray.position() );
                if (CompileState.mEnableDebugCode)
                {
                    Arrays.Sort(mSourcePosArray, 0, mSrcPosArrayPos);
                }
                mSourcePosArraySorted = true;
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        private void RegisterFunction()
        {
            // registration of function to the parent's context
            if (mParent == null)
            {
                return;
            }
            if (mContextType == ContextType.PROPERTY_SETTER)
            {
                mParent.mPropSetter = this;
                return;
            }
            if (mContextType == ContextType.PROPERTY_GETTER)
            {
                mParent.mPropGetter = this;
                return;
            }
            if (mContextType == ContextType.SUPER_CLASS_GETTER)
            {
                return;
            }
            // these are already registered to parent context
            if (mContextType != ContextType.FUNCTION && mContextType != ContextType.PROPERTY
                && mContextType != ContextType.CLASS)
            {
                // ctExprFunction is not concerned here
                return;
            }
            int data = -1;
            if (mParent.mContextType == ContextType.TOP_LEVEL)
            {
                Variant val;
                val = new Variant(this);
                data = mParent.PutData(val);
                val = new Variant(mName);
                int name = mParent.PutData(val);
                bool changethis = mContextType == ContextType.FUNCTION || mContextType == ContextType
                    .PROPERTY;
                mParent.mNonLocalFunctionDeclVector.AddItem(new InterCodeGenerator.NonLocalFunctionDecl
                    (data, name, changethis));
            }
            if (mContextType == ContextType.FUNCTION && mParent.mContextType == ContextType.FUNCTION)
            {
                // local functions
                // adds the function as a parent's local variable
                if (data == -1)
                {
                    Variant val;
                    val = new Variant(this);
                    data = mParent.PutData(val);
                }
                mParent.InitLocalFunction(mName, data);
            }
            if (mParent.mContextType == ContextType.FUNCTION || mParent.mContextType == ContextType
                .CLASS)
            {
                // register members to the parent object
                //Variant val = new Variant( this );
                //mParent.propSet( MEMBERENSURE|IGNOREPROP, mName, val, mParent );
                if (mProperties == null)
                {
                    mProperties = new AList<InterCodeGenerator.Property>();
                }
                AddProperty(mName, this);
            }
        }

        public class Property
        {
            public string Name;

            public InterCodeGenerator Value;

            public Property(string name, InterCodeGenerator val)
            {
                Name = name;
                Value = val;
            }
        }

        private AList<InterCodeGenerator.Property> mProperties;

        private void AddProperty(string name, InterCodeGenerator val)
        {
            mProperties.AddItem(new InterCodeGenerator.Property(name, val));
        }

        internal virtual AList<InterCodeGenerator.Property> GetProp()
        {
            return mProperties;
        }

        public virtual bool IsClass()
        {
            return (mContextType == ContextType.CLASS || mContextType == ContextType.TOP_LEVEL
                );
        }

        public virtual void DumpClassStructure(int nest)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < nest; i++)
            {
                builder.Append("  ");
            }
            string space = builder.ToString();
            switch (mContextType)
            {
                case ContextType.TOP_LEVEL:
                    {
                        Tjs.OutputToConsole(space + "Top level Name: " + mName);
                        break;
                    }

                case ContextType.FUNCTION:
                    {
                        Tjs.OutputToConsole(space + "Function Name: " + mName);
                        break;
                    }

                case ContextType.EXPR_FUNCTION:
                    {
                        Tjs.OutputToConsole(space + "Expr function Name: " + mName);
                        break;
                    }

                case ContextType.PROPERTY:
                    {
                        Tjs.OutputToConsole(space + "Property Name: " + mName);
                        break;
                    }

                case ContextType.PROPERTY_SETTER:
                    {
                        Tjs.OutputToConsole(space + "Property setter Name: " + mName);
                        break;
                    }

                case ContextType.PROPERTY_GETTER:
                    {
                        Tjs.OutputToConsole(space + "Property getter Name: " + mName);
                        break;
                    }

                case ContextType.CLASS:
                    {
                        Tjs.OutputToConsole(space + "Class Name: " + mName);
                        break;
                    }

                case ContextType.SUPER_CLASS_GETTER:
                    {
                        Tjs.OutputToConsole(space + "Super class getter Name: " + mName);
                        break;
                    }
            }
            Tjs.OutputToConsole(space + "  Max variable count: " + mMaxVariableCount);
            Tjs.OutputToConsole(space + "  Variable reserve count: " + mVariableReserveCount);
            Tjs.OutputToConsole(space + "  Max frame count: " + mMaxFrameCount);
            Tjs.OutputToConsole(space + "  Func decl arg count: " + mFuncDeclArgCount);
            Tjs.OutputToConsole(space + "  Func decl unnamed arg array base: " + mFuncDeclUnnamedArgArrayBase
                );
            Tjs.OutputToConsole(space + "  Func decl collapse base: " + mFuncDeclCollapseBase
                );
            if (mPropSetter != null)
            {
                Tjs.OutputToConsole(space + "  Prop setter:");
                mPropSetter.DumpClassStructure(nest + 1);
            }
            else
            {
                Tjs.OutputToConsole(space + "  Prop setter: not found");
            }
            if (mPropGetter != null)
            {
                Tjs.OutputToConsole(space + "  Prop getter: true");
                mPropGetter.DumpClassStructure(nest + 1);
            }
            else
            {
                Tjs.OutputToConsole(space + "  Prop getter: not found");
            }
            if (mSuperClassGetter != null)
            {
                Tjs.OutputToConsole(space + "  Super class getter:");
                mSuperClassGetter.DumpClassStructure(nest + 1);
            }
            else
            {
                Tjs.OutputToConsole(space + "  Super class getter: not found");
            }
            if (mProperties != null)
            {
                int count = mProperties.Count;
                if (count > 0)
                {
                    Tjs.OutputToConsole(space + "  Members:");
                    for (int i_1 = 0; i_1 < count; i_1++)
                    {
                        mProperties[i_1].Value.DumpClassStructure(nest + 1);
                    }
                }
                else
                {
                    Tjs.OutputToConsole(space + "  Members: not found");
                }
            }
            else
            {
                Tjs.OutputToConsole(space + "  Members: not found");
            }
            {
                Tjs.OutputToConsole(space + "  Data array members:");
                int count = mDataArray.Length;
                Variant[] da = mDataArray;
                for (int i_1 = 0; i_1 < count; i_1++)
                {
                    Variant d = da[i_1];
                    object o = d.ToJavaObject();
                    if (o is InterCodeGenerator)
                    {
                        ((InterCodeGenerator)o).DumpClassStructure(nest + 1);
                    }
                }
            }
        }

        private const short VM_NOP = 0;

        private const short VM_CONST = 1;

        private const short VM_CP = 2;

        private const short VM_CL = 3;

        private const short VM_CCL = 4;

        private const short VM_TT = 5;

        private const short VM_TF = 6;

        private const short VM_CEQ = 7;

        private const short VM_CDEQ = 8;

        private const short VM_CLT = 9;

        private const short VM_CGT = 10;

        private const short VM_SETF = 11;

        private const short VM_SETNF = 12;

        private const short VM_LNOT = 13;

        private const short VM_NF = 14;

        private const short VM_JF = 15;

        private const short VM_JNF = 16;

        private const short VM_JMP = 17;

        private const short VM_INC = 18;

        private const short VM_INCPD = 19;

        private const short VM_INCPI = 20;

        private const short VM_INCP = 21;

        private const short VM_DEC = 22;

        private const short VM_DECPD = 23;

        private const short VM_DECPI = 24;

        private const short VM_DECP = 25;

        private const short VM_LOR = 26;

        private const short VM_LORPD = 27;

        private const short VM_LORPI = 28;

        private const short VM_LORP = 29;

        private const short VM_LAND = 30;

        private const short VM_LANDPD = 31;

        private const short VM_LANDPI = 32;

        private const short VM_LANDP = 33;

        private const short VM_BOR = 34;

        private const short VM_BORPD = 35;

        private const short VM_BORPI = 36;

        private const short VM_BORP = 37;

        private const short VM_BXOR = 38;

        private const short VM_BXORPD = 39;

        private const short VM_BXORPI = 40;

        private const short VM_BXORP = 41;

        private const short VM_BAND = 42;

        private const short VM_BANDPD = 43;

        private const short VM_BANDPI = 44;

        private const short VM_BANDP = 45;

        private const short VM_SAR = 46;

        private const short VM_SARPD = 47;

        private const short VM_SARPI = 48;

        private const short VM_SARP = 49;

        private const short VM_SAL = 50;

        private const short VM_SALPD = 51;

        private const short VM_SALPI = 52;

        private const short VM_SALP = 53;

        private const short VM_SR = 54;

        private const short VM_SRPD = 55;

        private const short VM_SRPI = 56;

        private const short VM_SRP = 57;

        private const short VM_ADD = 58;

        private const short VM_ADDPD = 59;

        private const short VM_ADDPI = 60;

        private const short VM_ADDP = 61;

        private const short VM_SUB = 62;

        private const short VM_SUBPD = 63;

        private const short VM_SUBPI = 64;

        private const short VM_SUBP = 65;

        private const short VM_MOD = 66;

        private const short VM_MODPD = 67;

        private const short VM_MODPI = 68;

        private const short VM_MODP = 69;

        private const short VM_DIV = 70;

        private const short VM_DIVPD = 71;

        private const short VM_DIVPI = 72;

        private const short VM_DIVP = 73;

        private const short VM_IDIV = 74;

        private const short VM_IDIVPD = 75;

        private const short VM_IDIVPI = 76;

        private const short VM_IDIVP = 77;

        private const short VM_MUL = 78;

        private const short VM_MULPD = 79;

        private const short VM_MULPI = 80;

        private const short VM_MULP = 81;

        private const short VM_BNOT = 82;

        private const short VM_TYPEOF = 83;

        private const short VM_TYPEOFD = 84;

        private const short VM_TYPEOFI = 85;

        private const short VM_EVAL = 86;

        private const short VM_EEXP = 87;

        private const short VM_CHKINS = 88;

        private const short VM_ASC = 89;

        private const short VM_CHR = 90;

        private const short VM_NUM = 91;

        private const short VM_CHS = 92;

        private const short VM_INV = 93;

        private const short VM_CHKINV = 94;

        private const short VM_INT = 95;

        private const short VM_REAL = 96;

        private const short VM_STR = 97;

        private const short VM_OCTET = 98;

        private const short VM_CALL = 99;

        private const short VM_CALLD = 100;

        private const short VM_CALLI = 101;

        private const short VM_NEW = 102;

        private const short VM_GPD = 103;

        private const short VM_SPD = 104;

        private const short VM_SPDE = 105;

        private const short VM_SPDEH = 106;

        private const short VM_GPI = 107;

        private const short VM_SPI = 108;

        private const short VM_SPIE = 109;

        private const short VM_GPDS = 110;

        private const short VM_SPDS = 111;

        private const short VM_GPIS = 112;

        private const short VM_SPIS = 113;

        private const short VM_SETP = 114;

        private const short VM_GETP = 115;

        private const short VM_DELD = 116;

        private const short VM_DELI = 117;

        private const short VM_SRV = 118;

        private const short VM_RET = 119;

        private const short VM_ENTRY = 120;

        private const short VM_EXTRY = 121;

        private const short VM_THROW = 122;

        private const short VM_CHGTHIS = 123;

        private const short VM_GLOBAL = 124;

        private const short VM_ADDCI = 125;

        private const short VM_REGMEMBER = 126;

        private const short VM_DEBUGGER = 127;

        private const short __VM_LAST = 128;

        private const short stNone = VM_NOP;

        private const short stEqual = VM_CP;

        private const short stBitAND = VM_BAND;

        private const short stBitOR = VM_BOR;

        private const short stBitXOR = VM_BXOR;

        private const short stSub = VM_SUB;

        private const short stAdd = VM_ADD;

        private const short stMod = VM_MOD;

        private const short stDiv = VM_DIV;

        private const short stIDiv = VM_IDIV;

        private const short stMul = VM_MUL;

        private const short stLogOR = VM_LOR;

        private const short stLogAND = VM_LAND;

        private const short stSAR = VM_SAR;

        private const short stSAL = VM_SAL;

        private const short stSR = VM_SR;

        private const short stPreInc = __VM_LAST;

        private const short stPreDec = 129;

        private const short stPostInc = 130;

        private const short stPostDec = 131;

        private const short stDelete = 132;

        private const short stFuncCall = 133;

        private const short stIgnorePropGet = 134;

        private const short stIgnorePropSet = 135;

        private const short stTypeOf = 136;

        private const byte fatNormal = 0;

        private const byte fatExpand = 1;

        private const byte fatUnnamedExpand = 2;

        private const int RT_NEEDED = unchecked((int)(0x0001));

        private const int RT_CFLAG = unchecked((int)(0x0002));

        private const int GNC_CFLAG = (1 << (4 * 8 - 1));

        private const int GNC_CFLAG_I = (GNC_CFLAG + 1);

        // VMCodes
        // SubType
        // FuncArgType
        // result needed
        // condition flag needed
        // true logic
        // inverted logic
        /// <summary>バイトコードを出力する</summary>
        /// <returns></returns>
        public virtual ByteBuffer ExportByteCode(Compiler block, ConstArrayData constarray
            )
        {
            int parent = -1;
            if (mParent != null)
            {
                parent = block.GetCodeIndex(mParent);
            }
            int propSetter = -1;
            if (mPropSetter != null)
            {
                propSetter = block.GetCodeIndex(mPropSetter);
            }
            int propGetter = -1;
            if (mPropGetter != null)
            {
                propGetter = block.GetCodeIndex(mPropGetter);
            }
            int superClassGetter = -1;
            if (mSuperClassGetter != null)
            {
                superClassGetter = block.GetCodeIndex(mSuperClassGetter);
            }
            int name = -1;
            if (mName != null)
            {
                name = constarray.PutString(mName);
            }
            // 13 * 4 データ部分のサイズ
            //int srcpossize = mSourcePosArray != null ? mSourcePosArray.position() * 8 : 0; // mSourcePosArray は配列の方がいいかな, codepos, sorcepos の顺で、int型で登录した方がいいかも
            int srcpossize = 0;
            // 主にデバッグ用の情报なので出力抑止
            int codesize = (mCode.Length % 2) == 1 ? mCode.Length * 2 + 2 : mCode.Length * 2;
            int datasize = mDataArray.Length * 4;
            int scgpsize = mSuperClassGetterPointer != null ? mSuperClassGetterPointer.Size()
                 * 4 : 0;
            int propsize = (mProperties != null ? mProperties.Count * 8 : 0) + 4;
            int size = 12 * 4 + srcpossize + codesize + datasize + scgpsize + propsize + 4 *
                4;
            ByteBuffer result = ByteBuffer.Allocate(size);
            result.Order(ByteOrder.LITTLE_ENDIAN);
            result.Clear();
            IntBuffer buf = result.AsIntBuffer();
            buf.Clear();
            buf.Put(parent);
            buf.Put(name);
            buf.Put(mContextType);
            buf.Put(mMaxVariableCount);
            buf.Put(mVariableReserveCount);
            buf.Put(mMaxFrameCount);
            buf.Put(mFuncDeclArgCount);
            buf.Put(mFuncDeclUnnamedArgArrayBase);
            buf.Put(mFuncDeclCollapseBase);
            buf.Put(propSetter);
            buf.Put(propGetter);
            buf.Put(superClassGetter);
            //int count = mSourcePosArray != null ? mSourcePosArray.position() : 0;
            int count = 0;
            buf.Put(count);
            // 主にデバッグ用の情报なので出力抑止
            count = mCode.Length;
            buf.Put(count);
            ShortBuffer sbuf = result.AsShortBuffer();
            sbuf.Clear();
            sbuf.Position(buf.Position() * 2);
            sbuf.Put(mCode);
            if ((count % 2) == 1)
            {
                // アライメント
                sbuf.Put((short)0);
            }
            buf.Position(sbuf.Position() / 2);
            count = mDataArray.Length;
            buf.Put(count);
            sbuf.Position(buf.Position() * 2);
            for (int i = 0; i < count; i++)
            {
                Variant val = mDataArray[i];
                short type = constarray.GetType(val);
                short v = (short)constarray.PutVariant(val, block);
                sbuf.Put(type);
                sbuf.Put(v);
            }
            buf.Position(sbuf.Position() / 2);
            count = mSuperClassGetterPointer != null ? mSuperClassGetterPointer.Size() : 0;
            buf.Put(count);
            for (int i_1 = 0; i_1 < count; i_1++)
            {
                int v = mSuperClassGetterPointer.Get(i_1);
                buf.Put(v);
            }
            count = 0;
            if (mProperties != null)
            {
                count = mProperties.Count;
                buf.Put(count);
                if (count > 0)
                {
                    for (int i_2 = 0; i_2 < count; i_2++)
                    {
                        InterCodeGenerator.Property prop = mProperties[i_2];
                        // .Value.dumpClassStructure(nest+1);
                        int propname = constarray.PutString(prop.Name);
                        int propobj = -1;
                        if (prop.Value != null)
                        {
                            propobj = block.GetCodeIndex(prop.Value);
                        }
                        buf.Put(propname);
                        buf.Put(propobj);
                    }
                }
            }
            else
            {
                buf.Put(count);
            }
            result.Limit(result.Capacity());
            result.Position(0);
            return result;
        }

        /// <exception cref="CompileException"></exception>
        /// <exception cref="VariantException"></exception>
        public virtual AList<string> ToJavaCode(int start, int end)
        {
            JavaCodeGenerator gen = new JavaCodeGenerator(mCode, mDataArray, this);
            gen.GenFunCall(mMaxVariableCount, mMaxFrameCount, mFuncDeclArgCount, mFuncDeclCollapseBase
                );
            gen.Generate(start, end, mFuncDeclUnnamedArgArrayBase, mMaxFrameCount);
            AList<string> ret = gen.GetSourceCode();
            gen = null;
            return ret;
        }

        /// <summary>生成一段阶目</summary>
        /// <returns></returns>
        public virtual InterCodeObject CreteCodeObject(ScriptBlock block)
        {
            SortSourcePos();
            // 常にソートして渡す
            LongBuffer srcPos = null;
            if (mSourcePosArray != null)
            {
                srcPos = LongBuffer.Wrap(mSourcePosArray);
                srcPos.Position(mSrcPosArrayPos);
            }
            return new InterCodeObject(block, mName, mContextType, mCode, mDataArray, mMaxVariableCount
                , mVariableReserveCount, mMaxFrameCount, mFuncDeclArgCount, mFuncDeclUnnamedArgArrayBase
                , mFuncDeclCollapseBase, mSourcePosArraySorted, srcPos, mSuperClassGetterPointer
                .ToArray());
        }

        public virtual void CreateSecond(InterCodeObject obj)
        {
            obj.SetCodeObject(mParent != null ? mBlock.GetCodeObject(mBlock.GetCodeIndex(mParent
                )) : null, mPropSetter != null ? mBlock.GetCodeObject(mBlock.GetCodeIndex(mPropSetter
                )) : null, mPropGetter != null ? mBlock.GetCodeObject(mBlock.GetCodeIndex(mPropGetter
                )) : null, mSuperClassGetter != null ? mBlock.GetCodeObject(mBlock.GetCodeIndex(
                mSuperClassGetter)) : null);
        }

        // SourceCodeAccessor
        public virtual int CodePosToSrcPos(int codepos)
        {
            // converts from
            // CodeArea oriented position to source oriented position
            if (mSourcePosArray == null)
            {
                return 0;
            }
            int s = 0;
            int e = mSrcPosArrayPos;
            if (e == 0)
            {
                return 0;
            }
            while (true)
            {
                if (e - s <= 1)
                {
                    return (int)(mSourcePosArray[s] & unchecked((long)(0xFFFFFFFFL)));
                }
                int m = s + (e - s) / 2;
                if (((long)(((ulong)mSourcePosArray[m]) >> 32)) > codepos)
                {
                    e = m;
                }
                else
                {
                    s = m;
                }
            }
        }

        public virtual int SrcPosToLine(int srcpos)
        {
            return mBlock.SrcPosToLine(srcpos);
        }

        public virtual string GetLine(int line)
        {
            return mBlock.GetLine(line);
        }

        public virtual string GetScript()
        {
            return mBlock.GetScript();
        }

        public virtual int GetLineOffset()
        {
            return mBlock.GetLineOffset();
        }

        public virtual int GetContextType()
        {
            return mContextType;
        }

        public virtual InterCodeGenerator GetParent()
        {
            return mParent;
        }
    }
}

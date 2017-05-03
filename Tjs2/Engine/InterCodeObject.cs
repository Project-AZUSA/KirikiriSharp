/*
 * TJS2 CSharp
 */

using System;
using System.Text;
using Tjs2.NativeApi;
using Tjs2.NativeApi.Internal;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
    /// <summary>TJS2 バイトコードを持ったオブジェクト</summary>
    public class InterCodeObject : CustomObject, SourceCodeAccessor
    {
        private static readonly string[] mStrFuncs = new string[] { "charAt", "indexOf", "toUpperCase"
            , "toLowerCase", "substring", "substr", "sprintf", "replace", "escape", "split",
            "trim", "reverse", "repeat" };

        private const int StrMethod_charAt = 0;

        private const int StrMethod_indexOf = 1;

        private const int StrMethod_toUpperCase = 2;

        private const int StrMethod_toLowerCase = 3;

        private const int StrMethod_substring = 4;

        private const int StrMethod_substr = 5;

        private const int StrMethod_sprintf = 6;

        private const int StrMethod_replace = 7;

        private const int StrMethod_escape = 8;

        private const int StrMethod_split = 9;

        private const int StrMethod_trim = 10;

        private const int StrMethod_reverse = 11;

        private const int StrMethod_repeat = 12;

        private ScriptBlock mBlock;

        internal InterCodeObject mParent;

        private string mName;

        private int mContextType;

        private short[] mCode;

        private Variant[] mDataArray;

        /// <summary>引数の数</summary>
        private int mMaxVariableCount;

        /// <summary>予约领域(this,proxy用)</summary>
        private int mVariableReserveCount;

        /// <summary>关数内レジスタ数(ローカル变数の数)</summary>
        private int mMaxFrameCount;

        private int mFuncDeclArgCount;

        private int mFuncDeclUnnamedArgArrayBase;

        private int mFuncDeclCollapseBase;

        private LongBuffer mSourcePosArray;

        private InterCodeObject mPropSetter;

        private InterCodeObject mPropGetter;

        private InterCodeObject mSuperClassGetter;

        private int[] mSuperClassGetterPointer;

        /// <summary>ディスアセンブラ</summary>
        private Disassembler mDisassembler;

        private const int NAMESPACE_DEFAULT_HASH_BITS = 3;

        //private boolean mSourcePosArraySorted;
        // 上位をcodePos, 下位をsourcePos とする
        //private IntVector mSuperClassGetterPointer; // int[] に书き换えた方がいいかも
        private static int GetContextHashSize(int type)
        {
            switch (type)
            {
                case ContextType.TOP_LEVEL:
                    {
                        return 0;
                    }

                case ContextType.FUNCTION:
                    {
                        return 1;
                    }

                case ContextType.EXPR_FUNCTION:
                    {
                        return 1;
                    }

                case ContextType.PROPERTY:
                    {
                        return 1;
                    }

                case ContextType.PROPERTY_SETTER:
                    {
                        return 0;
                    }

                case ContextType.PROPERTY_GETTER:
                    {
                        return 0;
                    }

                case ContextType.CLASS:
                    {
                        return NAMESPACE_DEFAULT_HASH_BITS;
                    }

                case ContextType.SUPER_CLASS_GETTER:
                    {
                        return 0;
                    }

                default:
                    {
                        return NAMESPACE_DEFAULT_HASH_BITS;
                    }
            }
        }

        public InterCodeObject(ScriptBlock block, string name, int type, short[] code, Variant
            [] da, int varcount, int verrescount, int maxframe, int argcount, int arraybase,
            int colbase, bool srcsorted, LongBuffer srcpos, int[] superpointer) : base(GetContextHashSize
            (type))
        {
            //super.mCallFinalize = false;
            mBlock = block;
            //mBlock.add( this );
            mName = name;
            mContextType = type;
            mCode = code;
            mDataArray = da;
            mMaxVariableCount = varcount;
            mVariableReserveCount = verrescount;
            mMaxFrameCount = maxframe;
            mFuncDeclArgCount = argcount;
            mFuncDeclUnnamedArgArrayBase = arraybase;
            mFuncDeclCollapseBase = colbase;
            //mSourcePosArraySorted = srcsorted;
            mSourcePosArray = srcpos;
            mSuperClassGetterPointer = superpointer;
        }

        public virtual void SetCodeObject(InterCodeObject parent, InterCodeObject
             setter, InterCodeObject getter, InterCodeObject superclass
            )
        {
            mParent = parent;
            mPropSetter = setter;
            mPropGetter = getter;
            mSuperClassGetter = superclass;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        protected internal override void FinalizeObject()
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
            mBlock.Remove(this);
            if (mContextType != ContextType.TOP_LEVEL && mBlock != null)
            {
                mBlock = null;
            }
            base.FinalizeObject();
        }

        public virtual void Compact()
        {
            if (Tjs.IsLowMemory)
            {
                mSourcePosArray = null;
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        private void ExecuteAsFunction(Dispatch2 objthis, Variant[] args, Variant result,
            int start_ip)
        {
            int num_alloc = mMaxVariableCount + mVariableReserveCount + 1 + mMaxFrameCount;
            // TJSVariantArrayStackAddRef();
            Variant[] regs = null;
            //int offset = -1;
            try
            {
                regs = new Variant[num_alloc];
                for (int i = 0; i < num_alloc; i++)
                {
                    //regs[i] = TJS.allocateVariant();
                    regs[i] = new Variant();
                }
                int arrayOffset = mMaxVariableCount + mVariableReserveCount;
                // register area
                // objthis-proxy
                if (objthis != null)
                {
                    ObjectProxy proxy = new ObjectProxy();
                    proxy.SetObjects(objthis, mBlock.GetTJS().GetGlobal());
                    // OriginalTODO: caching of objthis-proxy
                    //ra[-2] = proxy;
                    regs[arrayOffset - 2].Set(proxy);
                }
                else
                {
                    //proxy.setObjects( null, null );
                    Dispatch2 global = mBlock.GetTJS().GetGlobal();
                    //ra[-2].setObject( global, global );
                    regs[arrayOffset - 2].Set(global, global);
                }
                //			if( TJSStackTracerEnabled() ) TJSStackTracerPush( this, false );
                //			// check whether the objthis is deleting
                //			if( TJSWarnOnExecutionOnDeletingObject && TJSObjectFlagEnabled() && mBlock.getTJS().getConsoleOutput() )
                //				TJSWarnIfObjectIsDeleting( mBlock.getTJS().getConsoleOutput(), objthis);
                try
                {
                    //ra[-1].SetObject(objthis, objthis);
                    regs[arrayOffset - 1].Set(objthis, objthis);
                    //ra[0].Clear();
                    //regs[arrayOffset].clear();
                    // transfer arguments
                    int numargs = args != null ? args.Length : 0;
                    if (numargs >= mFuncDeclArgCount)
                    {
                        // given arguments are greater than or equal to desired arguments
                        if (mFuncDeclArgCount != 0)
                        {
                            //Variant *r = ra - 3;
                            int r = arrayOffset - 3;
                            //Variant **a = args;
                            int n = mFuncDeclArgCount;
                            int argOffset = 0;
                            while (true)
                            {
                                regs[r].Set(args[argOffset]);
                                argOffset++;
                                //*r = **(a++);
                                n--;
                                if (n == 0)
                                {
                                    break;
                                }
                                r--;
                            }
                        }
                    }
                    else
                    {
                        // given arguments are less than desired arguments
                        //Variant *r = ra - 3;
                        int r = arrayOffset - 3;
                        //Variant **a = args;
                        int argOffset = 0;
                        int i_1;
                        for (i_1 = 0; i_1 < numargs; i_1++)
                        {
                            regs[r].Set(args[argOffset]);
                            argOffset++;
                            //*(r--) = **(a++);
                            r--;
                        }
                        for (; i_1 < mFuncDeclArgCount; i_1++)
                        {
                            //(r--)->Clear();
                            regs[r].Clear();
                            r--;
                        }
                    }
                    // collapse into array when FuncDeclCollapseBase >= 0
                    if (mFuncDeclCollapseBase >= 0)
                    {
                        //Variant *r = ra - 3 - mFuncDeclCollapseBase; // target variant
                        int r = arrayOffset - 3 - mFuncDeclCollapseBase;
                        // target variant
                        Dispatch2 dsp = Tjs.CreateArrayObject();
                        //*r = new Variant(dsp, dsp);
                        regs[r].Set(dsp, dsp);
                        //dsp->Release();
                        if (numargs > mFuncDeclCollapseBase)
                        {
                            // there are arguments to store
                            for (int c = 0, i = mFuncDeclCollapseBase; i < numargs; i++, c++)
                            {
                                dsp.PropSetByNum(0, c, args[i], dsp);
                            }
                        }
                    }
                    // execute
                    ExecuteCode(regs, arrayOffset, start_ip, args, result);
                }
                finally
                {
                    //regs[arrayOffset-2].clear();
                    //TJS.releaseVariant(regs);
                    //TJS.releaseVA( offset, regs );
                    regs = null;
                }
            }
            finally
            {
                //ra[-2].Clear(); // at least we must clear the object placed at local stack
                //TJSVariantArrayStack->Deallocate(num_alloc, regs);
                //				if(TJSStackTracerEnabled()) TJSStackTracerPop();
                //ra[-2].Clear(); // at least we must clear the object placed at local stack
                // TJSVariantArrayStack->Deallocate(num_alloc, regs);
                //			if(TJSStackTracerEnabled()) TJSStackTracerPop();
                // TJSVariantArrayStackRelease();
                regs = null;
            }
        }

        /// <exception cref="TjsScriptError"></exception>
        /// <exception cref="VariantException"></exception>
        private int ExecuteCode(Variant[] ra_org, int ra_offset, int startip, Variant[] args
            , Variant result)
        {
            // execute VM codes
            int codesave = startip;
            try
            {
                int code = startip;
                //mCodeArea.get(startip);
                //if(TJSStackTracerEnabled()) TJSStackTracerSetCodePointer(CodeArea, &codesave);
                Variant[] ra = ra_org;
                Variant[] da = mDataArray;
                short[] ca = mCode;
                int ri;
                bool flag = false;
                //int op;
                while (true)
                {
                    codesave = code;
                    switch (ca[code])
                    {
                        case VM_NOP:
                            {
                                //op = ca[code];
                                code++;
                                break;
                            }

                        case VM_CONST:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).CopyRef(TJS_GET_VM_REG(da, code[2]));
                                ra[ra_offset + ca[code + 1]].Set(da[ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_CP:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).CopyRef(TJS_GET_VM_REG(ra, code[2]));
                                ra[ra_offset + ca[code + 1]].Set(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_CL:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).Clear();
                                ra[ra_offset + ca[code + 1]].Clear();
                                code += 2;
                                break;
                            }

                        case VM_CCL:
                            {
                                ContinuousClear(ra, ra_offset, code);
                                code += 3;
                                break;
                            }

                        case VM_TT:
                            {
                                //flag = TJS_GET_VM_REG(ra, code[1]).operator bool();
                                flag = ra[ra_offset + ca[code + 1]].AsBoolean();
                                code += 2;
                                break;
                            }

                        case VM_TF:
                            {
                                //flag = !(TJS_GET_VM_REG(ra, code[1]).operator bool());
                                flag = !ra[ra_offset + ca[code + 1]].AsBoolean();
                                code += 2;
                                break;
                            }

                        case VM_CEQ:
                            {
                                //flag = TJS_GET_VM_REG(ra, code[1]).NormalCompare( TJS_GET_VM_REG(ra, code[2]));
                                flag = ra[ra_offset + ca[code + 1]].NormalCompare(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_CDEQ:
                            {
                                //flag = TJS_GET_VM_REG(ra, code[1]).DiscernCompare( TJS_GET_VM_REG(ra, code[2]));
                                ri = ca[code + 2];
                                if (ri == 0)
                                {
                                    flag = ra[ra_offset + ca[code + 1]].IsVoid();
                                }
                                else
                                {
                                    flag = ra[ra_offset + ca[code + 1]].DiscernCompare(ra[ra_offset + ca[code + 2]]).
                                        AsBoolean();
                                }
                                code += 3;
                                break;
                            }

                        case VM_CLT:
                            {
                                //flag = TJS_GET_VM_REG(ra, code[1]).GreaterThan( TJS_GET_VM_REG(ra, code[2]));
                                flag = ra[ra_offset + ca[code + 1]].GreaterThan(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_CGT:
                            {
                                //flag = TJS_GET_VM_REG(ra, code[1]).LittlerThan( TJS_GET_VM_REG(ra, code[2]));
                                flag = ra[ra_offset + ca[code + 1]].LittlerThan(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_SETF:
                            {
                                //TJS_GET_VM_REG(ra, code[1]) = flag;
                                ra[ra_offset + ca[code + 1]].Set(flag ? 1 : 0);
                                code += 2;
                                break;
                            }

                        case VM_SETNF:
                            {
                                //TJS_GET_VM_REG(ra, code[1]) = !flag;
                                ra[ra_offset + ca[code + 1]].Set(flag ? 0 : 1);
                                code += 2;
                                break;
                            }

                        case VM_LNOT:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).logicalnot();
                                ra[ra_offset + ca[code + 1]].Logicalnot();
                                code += 2;
                                break;
                            }

                        case VM_NF:
                            {
                                flag = !flag;
                                code++;
                                break;
                            }

                        case VM_JF:
                            {
                                // TJS_ADD_VM_CODE_ADDR(dest, x)  ((*(char **)&(dest)) += (x))
                                if (flag)
                                {
                                    //TJS_ADD_VM_CODE_ADDR(code, code[1]);
                                    //code += ra[ra_offset+ca[code+1]).asInteger();
                                    code += ca[code + 1];
                                }
                                else
                                {
                                    code += 2;
                                }
                                break;
                            }

                        case VM_JNF:
                            {
                                if (!flag)
                                {
                                    //TJS_ADD_VM_CODE_ADDR(code, code[1]);
                                    //code += ra[ra_offset+ca[code+1]).asInteger();
                                    code += ca[code + 1];
                                }
                                else
                                {
                                    code += 2;
                                }
                                break;
                            }

                        case VM_JMP:
                            {
                                //TJS_ADD_VM_CODE_ADDR(code, code[1]);
                                //code += ra[ra_offset+ca[code+1]).asInteger();
                                code += ca[code + 1];
                                break;
                            }

                        case VM_INC:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).increment();
                                ra[ra_offset + ca[code + 1]].Increment();
                                code += 2;
                                break;
                            }

                        case VM_INCPD:
                            {
                                OperatePropertyDirect0(ra, ra_offset, code, OP_INC);
                                code += 4;
                                break;
                            }

                        case VM_INCPI:
                            {
                                OperatePropertyIndirect0(ra, ra_offset, code, OP_INC);
                                code += 4;
                                break;
                            }

                        case VM_INCP:
                            {
                                OperateProperty0(ra, ra_offset, code, OP_INC);
                                code += 3;
                                break;
                            }

                        case VM_DEC:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).decrement();
                                ra[ra_offset + ca[code + 1]].Decrement();
                                code += 2;
                                break;
                            }

                        case VM_DECPD:
                            {
                                OperatePropertyDirect0(ra, ra_offset, code, OP_DEC);
                                code += 4;
                                break;
                            }

                        case VM_DECPI:
                            {
                                OperatePropertyIndirect0(ra, ra_offset, code, OP_DEC);
                                code += 4;
                                break;
                            }

                        case VM_DECP:
                            {
                                OperateProperty0(ra, ra_offset, code, OP_DEC);
                                code += 3;
                                break;
                            }

                        case VM_LOR:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].Logicalorequal(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_LORPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_LOR);
                                code += 5;
                                break;
                            }

                        case VM_LORPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_LOR);
                                code += 5;
                                break;
                            }

                        case VM_LORP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_LOR);
                                code += 4;
                                break;
                            }

                        case VM_LAND:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].Logicalandequal(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_LANDPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_LAND);
                                code += 5;
                                break;
                            }

                        case VM_LANDPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_LAND);
                                code += 5;
                                break;
                            }

                        case VM_LANDP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_LAND);
                                code += 4;
                                break;
                            }

                        case VM_BOR:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].OrEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_BORPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_BOR);
                                code += 5;
                                break;
                            }

                        case VM_BORPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_BOR);
                                code += 5;
                                break;
                            }

                        case VM_BORP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_BOR);
                                code += 4;
                                break;
                            }

                        case VM_BXOR:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].BitXorEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_BXORPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_BXOR);
                                code += 5;
                                break;
                            }

                        case VM_BXORPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_BXOR);
                                code += 5;
                                break;
                            }

                        case VM_BXORP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_BXOR);
                                code += 4;
                                break;
                            }

                        case VM_BAND:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].AndEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_BANDPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_BAND);
                                code += 5;
                                break;
                            }

                        case VM_BANDPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_BAND);
                                code += 5;
                                break;
                            }

                        case VM_BANDP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_BAND);
                                code += 4;
                                break;
                            }

                        case VM_SAR:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].RightShiftEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_SARPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_SAR);
                                code += 5;
                                break;
                            }

                        case VM_SARPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_SAR);
                                code += 5;
                                break;
                            }

                        case VM_SARP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_SAR);
                                code += 4;
                                break;
                            }

                        case VM_SAL:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].LeftShiftEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_SALPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_SAL);
                                code += 5;
                                break;
                            }

                        case VM_SALPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_SAL);
                                code += 5;
                                break;
                            }

                        case VM_SALP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_SAL);
                                code += 4;
                                break;
                            }

                        case VM_SR:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].Rbitshiftequal(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_SRPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_SR);
                                code += 5;
                                break;
                            }

                        case VM_SRPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_SR);
                                code += 5;
                                break;
                            }

                        case VM_SRP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_SR);
                                code += 4;
                                break;
                            }

                        case VM_ADD:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].AddEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_ADDPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_ADD);
                                code += 5;
                                break;
                            }

                        case VM_ADDPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_ADD);
                                code += 5;
                                break;
                            }

                        case VM_ADDP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_ADD);
                                code += 4;
                                break;
                            }

                        case VM_SUB:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].SubtractEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_SUBPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_SUB);
                                code += 5;
                                break;
                            }

                        case VM_SUBPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_SUB);
                                code += 5;
                                break;
                            }

                        case VM_SUBP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_SUB);
                                code += 4;
                                break;
                            }

                        case VM_MOD:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].ResidueEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_MODPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_MOD);
                                code += 5;
                                break;
                            }

                        case VM_MODPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_MOD);
                                code += 5;
                                break;
                            }

                        case VM_MODP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_MOD);
                                code += 4;
                                break;
                            }

                        case VM_DIV:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].DivideEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_DIVPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_DIV);
                                code += 5;
                                break;
                            }

                        case VM_DIVPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_DIV);
                                code += 5;
                                break;
                            }

                        case VM_DIVP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_DIV);
                                code += 4;
                                break;
                            }

                        case VM_IDIV:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].Idivequal(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_IDIVPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_IDIV);
                                code += 5;
                                break;
                            }

                        case VM_IDIVPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_IDIV);
                                code += 5;
                                break;
                            }

                        case VM_IDIVP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_IDIV);
                                code += 4;
                                break;
                            }

                        case VM_MUL:
                            {
                                // TJS_DEF_VM_P
                                ra[ra_offset + ca[code + 1]].MultiplyEqual(ra[ra_offset + ca[code + 2]]);
                                code += 3;
                                break;
                            }

                        case VM_MULPD:
                            {
                                OperatePropertyDirect(ra, ra_offset, code, OP_MUL);
                                code += 5;
                                break;
                            }

                        case VM_MULPI:
                            {
                                OperatePropertyIndirect(ra, ra_offset, code, OP_MUL);
                                code += 5;
                                break;
                            }

                        case VM_MULP:
                            {
                                OperateProperty(ra, ra_offset, code, OP_MUL);
                                code += 4;
                                break;
                            }

                        case VM_BNOT:
                            {
                                // TJS_DEF_VM_P
                                //TJS_GET_VM_REG(ra, code[1]).bitnot();
                                ra[ra_offset + ca[code + 1]].Bitnot();
                                code += 2;
                                break;
                            }

                        case VM_ASC:
                            {
                                //CharacterCodeOf(TJS_GET_VM_REG(ra, code[1]));
                                CharacterCodeOf(ra[ra_offset + ca[code + 1]]);
                                code += 2;
                                break;
                            }

                        case VM_CHR:
                            {
                                //CharacterCodeFrom(TJS_GET_VM_REG(ra, code[1]));
                                CharacterCodeFrom(ra[ra_offset + ca[code + 1]]);
                                code += 2;
                                break;
                            }

                        case VM_NUM:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).tonumber();
                                ra[ra_offset + ca[code + 1]].Tonumber();
                                code += 2;
                                break;
                            }

                        case VM_CHS:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).changesign();
                                ra[ra_offset + ca[code + 1]].Changesign();
                                code += 2;
                                break;
                            }

                        case VM_INV:
                            {
                                int offset = ra_offset + ca[code + 1];
                                bool tmp = ra[offset].IsObject() == false ? false : ra[offset].AsObjectClosure().
                                    Invalidate(0, null, ra[ra_offset - 1].AsObject()) == Error.S_TRUE;
                                ra[offset].Set(tmp ? 1 : 0);
                                code += 2;
                                break;
                            }

                        case VM_CHKINV:
                            {
                                int offset = ra_offset + ca[code + 1];
                                bool tmp;
                                if (ra[offset].IsObject() == false)
                                {
                                    tmp = true;
                                }
                                else
                                {
                                    int ret = ra[offset].AsObjectClosure().IsValid(0, null, ra[ra_offset - 1].AsObject
                                        ());
                                    tmp = ret == Error.S_TRUE || ret == Error.E_NOTIMPL;
                                }
                                ra[offset].Set(tmp ? 1 : 0);
                                code += 2;
                                break;
                            }

                        case VM_INT:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).ToInteger();
                                ra[ra_offset + ca[code + 1]].ToInteger();
                                code += 2;
                                break;
                            }

                        case VM_REAL:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).ToReal();
                                ra[ra_offset + ca[code + 1]].ToReal();
                                code += 2;
                                break;
                            }

                        case VM_STR:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).ToString();
                                ra[ra_offset + ca[code + 1]].SelfToString();
                                code += 2;
                                break;
                            }

                        case VM_OCTET:
                            {
                                //TJS_GET_VM_REG(ra, code[1]).ToOctet();
                                ra[ra_offset + ca[code + 1]].ToOctet();
                                code += 2;
                                break;
                            }

                        case VM_TYPEOF:
                            {
                                //TypeOf(TJS_GET_VM_REG(ra, code[1]));
                                TypeOf(ra[ra_offset + ca[code + 1]]);
                                code += 2;
                                break;
                            }

                        case VM_TYPEOFD:
                            {
                                TypeOfMemberDirect(ra, ra_offset, code, Interface.MEMBERMUSTEXIST);
                                code += 4;
                                break;
                            }

                        case VM_TYPEOFI:
                            {
                                TypeOfMemberIndirect(ra, ra_offset, code, Interface.MEMBERMUSTEXIST);
                                code += 4;
                                break;
                            }

                        case VM_EVAL:
                            {
                                Eval(ra[ra_offset + ca[code + 1]], Tjs.mEvalOperatorIsOnGlobal ? null : ra[ra_offset
                                     - 1].AsObject(), true);
                                code += 2;
                                break;
                            }

                        case VM_EEXP:
                            {
                                Eval(ra[ra_offset + ca[code + 1]], Tjs.mEvalOperatorIsOnGlobal ? null : ra[ra_offset
                                     - 1].AsObject(), false);
                                code += 2;
                                break;
                            }

                        case VM_CHKINS:
                            {
                                InstanceOf(ra[ra_offset + ca[code + 2]], ra[ra_offset + ca[code + 1]]);
                                code += 3;
                                break;
                            }

                        case VM_CALL:
                        case VM_NEW:
                            {
                                code += CallFunction(ra, ra_offset, code, args);
                                break;
                            }

                        case VM_CALLD:
                            {
                                code += CallFunctionDirect(ra, ra_offset, code, args);
                                break;
                            }

                        case VM_CALLI:
                            {
                                code += CallFunctionIndirect(ra, ra_offset, code, args);
                                break;
                            }

                        case VM_GPD:
                            {
                                GetPropertyDirect(ra, ra_offset, code, 0);
                                code += 4;
                                break;
                            }

                        case VM_GPDS:
                            {
                                GetPropertyDirect(ra, ra_offset, code, Interface.IGNOREPROP);
                                code += 4;
                                break;
                            }

                        case VM_SPD:
                            {
                                SetPropertyDirect(ra, ra_offset, code, 0);
                                code += 4;
                                break;
                            }

                        case VM_SPDE:
                            {
                                SetPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE);
                                code += 4;
                                break;
                            }

                        case VM_SPDEH:
                            {
                                SetPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE | Interface.HIDDENMEMBER
                                    );
                                code += 4;
                                break;
                            }

                        case VM_SPDS:
                            {
                                SetPropertyDirect(ra, ra_offset, code, Interface.MEMBERENSURE | Interface.IGNOREPROP
                                    );
                                code += 4;
                                break;
                            }

                        case VM_GPI:
                            {
                                GetPropertyIndirect(ra, ra_offset, code, 0);
                                code += 4;
                                break;
                            }

                        case VM_GPIS:
                            {
                                GetPropertyIndirect(ra, ra_offset, code, Interface.IGNOREPROP);
                                code += 4;
                                break;
                            }

                        case VM_SPI:
                            {
                                SetPropertyIndirect(ra, ra_offset, code, 0);
                                code += 4;
                                break;
                            }

                        case VM_SPIE:
                            {
                                SetPropertyIndirect(ra, ra_offset, code, Interface.MEMBERENSURE);
                                code += 4;
                                break;
                            }

                        case VM_SPIS:
                            {
                                SetPropertyIndirect(ra, ra_offset, code, Interface.MEMBERENSURE | Interface.IGNOREPROP
                                    );
                                code += 4;
                                break;
                            }

                        case VM_GETP:
                            {
                                GetProperty(ra, ra_offset, code);
                                code += 3;
                                break;
                            }

                        case VM_SETP:
                            {
                                SetProperty(ra, ra_offset, code);
                                code += 3;
                                break;
                            }

                        case VM_DELD:
                            {
                                DeleteMemberDirect(ra, ra_offset, code);
                                code += 4;
                                break;
                            }

                        case VM_DELI:
                            {
                                DeleteMemberIndirect(ra, ra_offset, code);
                                code += 4;
                                break;
                            }

                        case VM_SRV:
                            {
                                if (result != null)
                                {
                                    result.CopyRef(ra[ra_offset + ca[code + 1]]);
                                }
                                code += 2;
                                break;
                            }

                        case VM_RET:
                            {
                                return code + 1;
                            }

                        case VM_ENTRY:
                            {
                                // TJS_FROM_VM_REG_ADDR(x) ((tjs_int)(x) / (tjs_int)sizeof(tTJSVariant))
                                // TJS_FROM_VM_CODE_ADDR(x)  ((tjs_int)(x) / (tjs_int)sizeof(tjs_uint32))
                                code = ExecuteCodeInTryBlock(ra, ra_offset, code + 3, args, result, ca[code + 1]
                                    + code, ca[code + 2]);
                                break;
                            }

                        case VM_EXTRY:
                            {
                                return code + 1;
                            }

                        case VM_THROW:
                            {
                                // same as ret
                                ThrowScriptException(ra[ra_offset + ca[code + 1]], mBlock, CodePosToSrcPos(code));
                                code += 2;
                                // actually here not proceed...
                                break;
                            }

                        case VM_CHGTHIS:
                            {
                                ra[ra_offset + ca[code + 1]].ChangeClosureObjThis(ra[ra_offset + ca[code + 2]].AsObject
                                    ());
                                code += 3;
                                break;
                            }

                        case VM_GLOBAL:
                            {
                                ra[ra_offset + ca[code + 1]].Set(mBlock.GetTJS().GetGlobal());
                                code += 2;
                                break;
                            }

                        case VM_ADDCI:
                            {
                                AddClassInstanceInfo(ra, ra_offset, code);
                                code += 3;
                                break;
                            }

                        case VM_REGMEMBER:
                            {
                                //registerObjectMember( ra[ra_offset-1].asObject() );
                                CopyAllMembers((CustomObject)ra[ra_offset - 1].AsObject());
                                code++;
                                break;
                            }

                        case VM_DEBUGGER:
                            {
                                //TJSNativeDebuggerBreak();
                                code++;
                                break;
                            }

                        default:
                            {
                                ThrowInvalidVMCode();
                                break;
                            }
                    }
                }
            }
            catch (TjsScriptException e)
            {
                e.AddTrace(this, codesave);
                //e.printStackTrace();
                throw;
            }
            catch (TjsScriptError e)
            {
                e.AddTrace(this, codesave);
                //e.printStackTrace();
                throw;
            }
            catch (TjsException e)
            {
                DisplayExceptionGeneratedCode(codesave, ra_org, ra_offset);
                //TJS_eTJSScriptError( e.getMessage(), this, codesave );
                //e.printStackTrace();
                Error.ReportExceptionSource(e.Message, this, CodePosToSrcPos(codesave));
                throw new TjsScriptError(e.Message, mBlock, CodePosToSrcPos(codesave));
            }
            catch (Exception e)
            {
                Sharpen.Runtime.PrintStackTrace(e);
                DisplayExceptionGeneratedCode(codesave, ra_org, ra_offset);
                //TJS_eTJSScriptError( e.getMessage(), this, codesave );
                Error.ReportExceptionSource(e.Message, this, CodePosToSrcPos(codesave));
                throw new TjsScriptError(e.Message, mBlock, CodePosToSrcPos(codesave));
            }
        }

        //return codesave;
        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperateProperty(Variant[] ra, int ra_offset, int code, int ope)
        {
            short[] ca = mCode;
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            VariantClosure clo = ra_code2.AsObjectClosure();
            int offset = ca[code + 1];
            Variant result = offset != 0 ? ra[ra_offset + offset] : null;
            Dispatch2 objThis = clo.mObjThis ?? ra[ra_offset - 1].AsObject();
            int hr = clo.Operation(ope, null, result, ra_code3, objThis);
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, null);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperatePropertyIndirect(Variant[] ra, int ra_offset, int code, int ope
            )
        {
            short[] ca = mCode;
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            Variant ra_code4 = ra[ra_offset + ca[code + 4]];
            VariantClosure clo = ra_code2.AsObjectClosure();
            int offset = ca[code + 1];
            Variant result = offset != 0 ? ra[ra_offset + offset] : null;
            Dispatch2 objThis = clo.mObjThis ?? ra[ra_offset - 1].AsObject();
            if (ra_code3.IsInteger() != true)
            {
                string str = ra_code3.AsString();
                int hr = clo.Operation(ope, str, result, ra_code4, objThis);
                if (hr < 0)
                {
                    ThrowFrom_tjs_error(hr, str);
                }
            }
            else
            {
                int num = ra_code3.AsInteger();
                int hr = clo.OperationByNum(ope, num, result, ra_code4, objThis);
                if (hr < 0)
                {
                    ThrowFrom_tjs_error_num(hr, num);
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperatePropertyDirect(Variant[] ra, int ra_offset, int code, int ope
            )
        {
            short[] ca = mCode;
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant da_code3 = mDataArray[ca[code + 3]];
            Variant ra_code4 = ra[ra_offset + ca[code + 4]];
            VariantClosure clo = ra_code2.AsObjectClosure();
            string nameStr = da_code3.GetString();
            int offset = ca[code + 1];
            Variant result = offset != 0 ? ra[ra_offset + offset] : null;
            Dispatch2 objThis = clo.mObjThis ?? ra[ra_offset - 1].AsObject();
            int hr = clo.Operation(ope, nameStr, result, ra_code4, objThis);
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, nameStr);
            }
        }

        /// <exception cref="VariantException"></exception>
        private void DisplayExceptionGeneratedCode(int codepos, Variant[] ra, int ra_offset
            )
        {
            StringBuilder builder = new StringBuilder(128);
            builder.Append("==== An exception occured at ");
            builder.Append(GetPositionDescriptionString(codepos));
            builder.Append(", VM ip = ");
            builder.Append(codepos);
            builder.Append(" ====");
            int info_len = builder.Length;
            Tjs.OutputToConsole(builder.ToString());
            builder.Delete(0, builder.Length);
            Tjs.OutputToConsole("-- Disassembled VM code --");
            DisassenbleSrcLine(codepos);
            Tjs.OutputToConsole("-- Register dump --");
            int ra_start = ra_offset - (mMaxVariableCount + mVariableReserveCount);
            int ra_count = mMaxVariableCount + mVariableReserveCount + 1 + mMaxFrameCount;
            StringBuilder line = new StringBuilder(128);
            // if( (ra_count + ra_start) > ra.size() ) ra_count = ra.size() - ra_start;
            for (int i = 0; i < ra_count; i++)
            {
                builder.Append("%");
                builder.Append((i - (mMaxVariableCount + mVariableReserveCount)));
                builder.Append('=');
                builder.Append(Utils.VariantToReadableString(ra[ra_start + i]));
                if (line.Length + builder.Length + 2 > info_len)
                {
                    Tjs.OutputToConsole(line.ToString());
                    line.Delete(0, line.Length);
                    line.Append(builder);
                }
                else
                {
                    if (line.Length > 0)
                    {
                        line.Append("  ");
                    }
                    line.Append(builder);
                }
                builder.Delete(0, builder.Length);
            }
            if (line.Length > 0)
            {
                Tjs.OutputToConsole(line.ToString());
            }
            Tjs.OutputToConsoleSeparator("-", info_len);
        }

        /// <exception cref="VariantException"></exception>
        private void DisassenbleSrcLine(int codepos)
        {
            int start = FindSrcLineStartCodePos(codepos);
            Disassemble(start, codepos + 1);
        }

        /// <exception cref="VariantException"></exception>
        private void Disassemble(int start, int end)
        {
            if (mDisassembler == null)
            {
                mDisassembler = new Disassembler(mCode, mDataArray, this);
            }
            else
            {
                mDisassembler.Set(mCode, mDataArray, this);
            }
            mDisassembler.Disassemble(mBlock, start, end);
        }

        /// <exception cref="VariantException"></exception>
        public virtual void Disassemble(ScriptBlock data, int start, int end)
        {
            if (mDisassembler == null)
            {
                mDisassembler = new Disassembler(mCode, mDataArray, this);
            }
            else
            {
                mDisassembler.Set(mCode, mDataArray, this);
            }
            mDisassembler.Disassemble(data, start, end);
        }

        /// <summary>同一行の最初のコード位置を得る</summary>
        /// <param name="codepos">检索するコード位置</param>
        /// <returns>同一行の最初のコード位置</returns>
        public virtual int FindSrcLineStartCodePos(int codepos)
        {
            // find code address which is the first instruction of the source line
            if (mSourcePosArray == null)
            {
                return 0;
            }
            int srcpos = CodePosToSrcPos(codepos);
            int line = mBlock.SrcPosToLine(srcpos);
            srcpos = mBlock.LineToSrcPos(line);
            int codeposmin = -1;
            int count = mSourcePosArray.Position();
            for (int i = 0; i < count; i++)
            {
                // 上位をcodePos, 下位をsourcePos とする
                long sourcePosArray = mSourcePosArray.Get(i);
                int sourcePos = (int)(sourcePosArray & unchecked((long)(0xFFFFFFFFL)));
                if (sourcePos >= srcpos)
                {
                    int codePos = (int)((sourcePosArray >> 32) & unchecked((long)(0xFFFFFFFFL)));
                    if (codeposmin == -1 || codePos < codeposmin)
                    {
                        codeposmin = codePos;
                    }
                }
            }
            if (codeposmin < 0)
            {
                codeposmin = 0;
            }
            return codeposmin;
        }

        /// <exception cref="TjsException"></exception>
        private static void ThrowInvalidVMCode()
        {
            throw new TjsException(Error.InvalidOpecode);
        }

        /// <exception cref="VariantException"></exception>
        private void AddClassInstanceInfo(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            Dispatch2 dsp;
            dsp = ra[ra_offset + ca[code + 1]].AsObject();
            if (dsp != null)
            {
                //dsp.classInstanceInfo( Interface.CII_ADD, 0, ra[ra_offset+mCode[code+2]] );
                dsp.AddClassInstanceInfo(ra[ra_offset + ca[code + 2]].AsString());
            }
        }

        // ?? must be an error
        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void ThrowScriptException(Variant val, ScriptBlock block, int srcpos)
        {
            string msg = null;
            if (val.IsObject())
            {
                VariantClosure clo = val.AsObjectClosure();
                if (clo.mObject != null)
                {
                    Variant v2 = new Variant();
                    string message_name = "message";
                    int hr = clo.PropGet(0, message_name, v2, null);
                    if (hr >= 0)
                    {
                        msg = "script exception : " + v2.AsString();
                    }
                }
            }
            if (msg == null || msg.Length == 0)
            {
                msg = "script exception";
            }
            //TJSReportExceptionSource(msg, block, srcpos);
            throw new TjsScriptException(msg, block, srcpos, val);
        }

        public virtual int CodePosToSrcPos(int codepos)
        {
            // converts from
            // CodeArea oriented position to source oriented position
            if (mSourcePosArray == null)
            {
                return 0;
            }
            int s = 0;
            int e = mSourcePosArray.Position();
            if (e == 0)
            {
                return 0;
            }
            while (true)
            {
                if (e - s <= 1)
                {
                    return (int)(mSourcePosArray.Get(s) & unchecked((long)(0xFFFFFFFFL)));
                }
                int m = s + (e - s) / 2;
                if (((long)(((ulong)mSourcePosArray.Get(m)) >> 32)) > codepos)
                {
                    e = m;
                }
                else
                {
                    s = m;
                }
            }
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        private int ExecuteCodeInTryBlock(Variant[] ra, int ra_offset, int startip, Variant
            [] args, Variant result, int catchip, int exobjreg)
        {
            // execute codes in a try-protected block
            try
            {
                //if( TJS.stackTracerEnabled() ) TJS.stackTracerPush(this, true);
                int ret;
                try
                {
                    ret = ExecuteCode(ra, ra_offset, startip, args, result);
                }
                finally
                {
                }
                //if(TJSStackTracerEnabled()) TJSStackTracerPop();
                return ret;
            }
            catch (TjsScriptException e)
            {
                if (exobjreg != 0)
                {
                    ra[ra_offset + exobjreg].Set(e.GetValue());
                }
                return catchip;
            }
            catch (TjsScriptError e)
            {
                if (exobjreg != 0)
                {
                    Variant msg = new Variant(e.Message);
                    Variant trace = new Variant(e.GetTrace());
                    Variant ret = new Variant();
                    Error.GetExceptionObject(mBlock.GetTJS(), ret, msg, trace);
                    ra[ra_offset + exobjreg].Set(ret);
                }
                return catchip;
            }
            catch (TjsException e)
            {
                if (exobjreg != 0)
                {
                    Variant msg = new Variant(e.Message);
                    Variant ret = new Variant();
                    Error.GetExceptionObject(mBlock.GetTJS(), ret, msg, null);
                    ra[ra_offset + exobjreg].Set(ret);
                }
                return catchip;
            }
            catch (Exception e)
            {
                if (exobjreg != 0)
                {
                    Variant msg = new Variant(e.Message);
                    Variant ret = new Variant();
                    Error.GetExceptionObject(mBlock.GetTJS(), ret, msg, null);
                    ra[ra_offset + exobjreg].Set(ret);
                }
                return catchip;
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void DeleteMemberIndirect(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            string str = ra[ra_offset + ca[code + 3]].AsString();
            int hr;
            try
            {
                hr = clo.DeleteMember(0, str, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset
                    - 1].AsObject());
                int offset = ca[code + 1];
                if (offset != 0)
                {
                    if (hr < 0)
                    {
                        ra[ra_offset + offset].Set(0);
                    }
                    else
                    {
                        ra[ra_offset + offset].Set(1);
                    }
                }
            }
            finally
            {
                str = null;
                clo = null;
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void DeleteMemberDirect(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            int hr;
            try
            {
                Variant name = mDataArray[ca[code + 3]];
                string nameStr = name.GetString();
                hr = clo.DeleteMember(0, nameStr, ra[ra_offset - 1].AsObject());
            }
            finally
            {
                clo = null;
            }
            int offset = ca[code + 1];
            if (offset != 0)
            {
                if (hr < 0)
                {
                    ra[ra_offset + offset].Set(0);
                }
                else
                {
                    ra[ra_offset + offset].Set(1);
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void SetProperty(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 1]].AsObjectClosure();
            int hr;
            hr = clo.PropSet(0, null, ra[ra_offset + ca[code + 2]], clo.mObjThis != null ? clo
                .mObjThis : ra[ra_offset - 1].AsObject());
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, null);
            }
        }

        /// <exception cref="TjsException"></exception>
        private static void ThrowFrom_tjs_error_num(int hr, int num)
        {
            Error.ThrowFrom_tjs_error(hr, num.ToString());
        }

        /// <exception cref="TjsException"></exception>
        private static void ThrowFrom_tjs_error(int hr, string name)
        {
            Error.ThrowFrom_tjs_error(hr, name);
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void GetProperty(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            int hr = clo.PropGet(0, null, ra[ra_offset + ca[code + 1]], clo.mObjThis != null ?
                clo.mObjThis : ra[ra_offset - 1].AsObject());
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, null);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void SetPropertyIndirect(Variant[] ra, int ra_offset, int code, int flags
            )
        {
            short[] ca = mCode;
            Variant ra_code1 = ra[ra_offset + ca[code + 1]];
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            if (ra_code1.IsObject())
            {
                VariantClosure clo = ra_code1.AsObjectClosure();
                if (ra_code2.IsInteger() != true)
                {
                    string str;
                    str = ra_code2.AsString();
                    int hr;
                    hr = clo.PropSet(flags, str, ra_code3, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset
                         - 1].AsObject());
                    if (hr < 0)
                    {
                        ThrowFrom_tjs_error(hr, str);
                    }
                }
                else
                {
                    int hr;
                    hr = clo.PropSetByNum(flags, ra_code2.AsInteger(), ra_code3, clo.mObjThis != null
                         ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                    if (hr < 0)
                    {
                        ThrowFrom_tjs_error_num(hr, ra_code2.AsInteger());
                    }
                }
            }
            else
            {
                if (ra_code1.IsString())
                {
                    SetStringProperty(ra_code3, ra_code1, ra_code2);
                }
                else
                {
                    if (ra_code1.IsOctet())
                    {
                        SetOctetProperty(ra_code3, ra_code1, ra_code2);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra_code1));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void GetOctetProperty(Variant result, Variant octet, Variant member)
        {
            // processes properties toward octets.
            if (member.IsNumber() != true)
            {
                string name = member.GetString();
                if (name == null)
                {
                    ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, string.Empty);
                }
                if (name.Equals("length"))
                {
                    // get string length
                    ByteBuffer o = octet.AsOctet();
                    if (o != null)
                    {
                        result.Set(o.Capacity());
                    }
                    else
                    {
                        result.Set(0);
                    }
                    return;
                }
                else
                {
                    if (name[0] >= '0' && name[0] <= '9')
                    {
                        ByteBuffer o = octet.AsOctet();
                        int n = int.Parse(name);
                        int len = o != null ? o.Capacity() : 0;
                        if (n < 0 || n >= len)
                        {
                            throw new TjsException(Error.RangeError);
                        }
                        result.Set(o.Get(n));
                        return;
                    }
                }
                ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, name);
            }
            else
            {
                // member.Type() == tvtInteger || member.Type() == tvtReal
                ByteBuffer o = octet.AsOctet();
                int n = member.AsInteger();
                int len = o != null ? o.Capacity() : 0;
                if (n < 0 || n >= len)
                {
                    throw new TjsException(Error.RangeError);
                }
                result.Set(o.Get(n));
                return;
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void SetOctetProperty(Variant param, Variant octet, Variant member)
        {
            // processes properties toward octets.
            if (member.IsNumber() != true)
            {
                string name = member.GetString();
                if (name == null)
                {
                    ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, string.Empty);
                }
                if (name.Equals("length"))
                {
                    ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
                }
                else
                {
                    if (name[0] >= '0' && name[0] <= '9')
                    {
                        ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
                    }
                }
                ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, name);
            }
            else
            {
                // member.Type() == tvtInteger || member.Type() == tvtReal
                ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void GetStringProperty(Variant result, Variant str, Variant member)
        {
            // processes properties toward strings.
            if (member.IsNumber() != true)
            {
                string name = member.GetString();
                if (name == null)
                {
                    ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, string.Empty);
                }
                if (name.Equals("length"))
                {
                    // get string length
                    string s = str.AsString();
                    if (s == null)
                    {
                        result.Set(0);
                    }
                    else
                    {
                        // tTJSVariantString::GetLength can return zero if 'this' is NULL
                        result.Set(s.Length);
                    }
                    return;
                }
                else
                {
                    if (name[0] >= '0' && name[0] <= '9')
                    {
                        string s = str.AsString();
                        int n = int.Parse(name);
                        int len = s.Length;
                        if (n == len)
                        {
                            result.Set(string.Empty);
                            return;
                        }
                        if (n < 0 || n > len)
                        {
                            throw new TjsException(Error.RangeError);
                        }
                        result.Set(Sharpen.Runtime.Substring(s, n, n + 1));
                        return;
                    }
                }
                ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, name);
            }
            else
            {
                // member.Type() == tvtInteger || member.Type() == tvtReal
                string s = str.AsString();
                int n = member.AsInteger();
                int len = s.Length;
                if (n == len)
                {
                    result.Set(string.Empty);
                    return;
                }
                if (n < 0 || n > len)
                {
                    throw new TjsException(Error.RangeError);
                }
                result.Set(Sharpen.Runtime.Substring(s, n, n + 1));
                return;
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void SetStringProperty(Variant param, Variant str, Variant member)
        {
            // processes properties toward strings.
            if (member.IsNumber() != true)
            {
                string name = member.GetString();
                if (name == null)
                {
                    ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, string.Empty);
                }
                if (name.Equals("length"))
                {
                    ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
                }
                else
                {
                    if (name[0] >= '0' && name[0] <= '9')
                    {
                        ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
                    }
                }
                ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, name);
            }
            else
            {
                // member.Type() == tvtInteger || member.Type() == tvtReal
                ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void GetPropertyIndirect(Variant[] ra, int ra_offset, int code, int flags
            )
        {
            short[] ca = mCode;
            Variant ra_code1 = ra[ra_offset + ca[code + 1]];
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            if (ra_code2.IsObject())
            {
                int hr;
                VariantClosure clo = ra_code2.AsObjectClosure();
                if (ra_code3.IsInteger() != true)
                {
                    string str = ra_code3.AsString();
                    hr = clo.PropGet(flags, str, ra_code1, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset
                         - 1].AsObject());
                    if (hr < 0)
                    {
                        ThrowFrom_tjs_error(hr, str);
                    }
                }
                else
                {
                    hr = clo.PropGetByNum(flags, ra_code3.AsInteger(), ra_code1, clo.mObjThis != null
                         ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                    if (hr < 0)
                    {
                        ThrowFrom_tjs_error_num(hr, ra_code3.AsInteger());
                    }
                }
            }
            else
            {
                if (ra_code2.IsString())
                {
                    GetStringProperty(ra_code1, ra_code2, ra_code3);
                }
                else
                {
                    if (ra_code2.IsOctet())
                    {
                        GetOctetProperty(ra_code1, ra_code2, ra_code3);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra_code2));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void SetPropertyDirect(Variant[] ra, int ra_offset, int code, int flags)
        {
            short[] ca = mCode;
            Variant ra_code1 = ra[ra_offset + ca[code + 1]];
            Variant da_code2 = mDataArray[ca[code + 2]];
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            if (ra_code1.IsObject())
            {
                VariantClosure clo = ra_code1.AsObjectClosure();
                string name = da_code2.GetString();
                Dispatch2 objThis = clo.mObjThis;
                if (objThis == null)
                {
                    objThis = ra[ra_offset - 1].AsObject();
                }
                int hr = clo.PropSet(flags, name, ra_code3, objThis);
                if (hr < 0)
                {
                    ThrowFrom_tjs_error(hr, name);
                }
            }
            else
            {
                if (ra_code1.IsString())
                {
                    SetStringProperty(ra_code3, ra_code1, da_code2);
                }
                else
                {
                    if (ra_code1.IsOctet())
                    {
                        SetOctetProperty(ra_code3, ra_code1, da_code2);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra_code1));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void GetPropertyDirect(Variant[] ra, int ra_offset, int code, int flags)
        {
            short[] ca = mCode;
            Variant ra_code1 = ra[ra_offset + ca[code + 1]];
            Variant ra_code2 = ra[ra_offset + ca[code + 2]];
            Variant da_code3 = mDataArray[ca[code + 3]];
            if (ra_code2.IsObject())
            {
                VariantClosure clo = ra_code2.AsObjectClosure();
                string nameStr = da_code3.GetString();
                int hr = clo.PropGet(flags, nameStr, ra_code1, clo.mObjThis != null ? clo.mObjThis
                     : ra[ra_offset - 1].AsObject());
                if (hr < 0)
                {
                    ThrowFrom_tjs_error(hr, nameStr);
                }
            }
            else
            {
                if (ra_code2.IsString())
                {
                    GetStringProperty(ra_code1, ra_code2, da_code3);
                }
                else
                {
                    if (ra_code2.IsOctet())
                    {
                        GetOctetProperty(ra_code1, ra_code2, da_code3);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra_code2));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        private void ProcessOctetFunction(string member, string target, Variant[] args, Variant
             result)
        {
            // OrigianlTODO: unpack/pack implementation
            ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, member);
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        public static void ProcessStringFunction(string member, string target, Variant[]
            args, Variant result)
        {
            if (member == null)
            {
                ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, string.Empty);
            }
            int hash = member.GetHashCode();
            string s = new string(target.ToCharArray());
            // target string
            int s_len = target.Length;
            if ((hash == mStrFuncs[StrMethod_charAt].GetHashCode() && mStrFuncs[StrMethod_charAt
                ].Equals(member)))
            {
                if (args.Length != 1)
                {
                    ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                }
                if (s_len == 0)
                {
                    if (result != null)
                    {
                        result.Set(string.Empty);
                    }
                    return;
                }
                int i = args[0].AsInteger();
                if (i < 0 || i >= s_len)
                {
                    if (result != null)
                    {
                        result.Set(string.Empty);
                    }
                    return;
                }
                if (result != null)
                {
                    result.Set(Sharpen.Runtime.Substring(s, i, i + 1));
                }
                return;
            }
            else
            {
                if ((hash == mStrFuncs[StrMethod_indexOf].GetHashCode() && mStrFuncs[StrMethod_indexOf
                    ].Equals(member)))
                {
                    if (args.Length != 1 && args.Length != 2)
                    {
                        ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                    }
                    string pstr = args[0].AsString();
                    // sub string
                    if (s == null || pstr == null)
                    {
                        if (result != null)
                        {
                            result.Set(-1);
                        }
                        return;
                    }
                    int start;
                    if (args.Length == 1)
                    {
                        start = 0;
                    }
                    else
                    {
                        // integer convertion may raise an exception
                        start = args[1].AsInteger();
                    }
                    if (start >= s_len)
                    {
                        if (result != null)
                        {
                            result.Set(-1);
                        }
                        return;
                    }
                    int found = s.IndexOf(pstr, start);
                    if (result != null)
                    {
                        result.Set(found);
                    }
                    return;
                }
                else
                {
                    if ((hash == mStrFuncs[StrMethod_toUpperCase].GetHashCode() && mStrFuncs[StrMethod_toUpperCase
                        ].Equals(member)))
                    {
                        if (args.Length != 0)
                        {
                            ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                        }
                        if (result != null)
                        {
                            result.Set(s.ToUpper());
                        }
                        return;
                    }
                    else
                    {
                        if ((hash == mStrFuncs[StrMethod_toLowerCase].GetHashCode() && mStrFuncs[StrMethod_toLowerCase
                            ].Equals(member)))
                        {
                            if (args.Length != 0)
                            {
                                ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                            }
                            if (result != null)
                            {
                                result.Set(s.ToLower());
                            }
                            return;
                        }
                        else
                        {
                            if ((hash == mStrFuncs[StrMethod_substring].GetHashCode() && mStrFuncs[StrMethod_substring
                                ].Equals(member)) || (hash == mStrFuncs[StrMethod_substr].GetHashCode() && mStrFuncs
                                [StrMethod_substr].Equals(member)))
                            {
                                if (args.Length != 1 && args.Length != 2)
                                {
                                    ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                }
                                int start = args[0].AsInteger();
                                if (start < 0 || start >= s_len)
                                {
                                    if (result != null)
                                    {
                                        result.Set(string.Empty);
                                    }
                                    return;
                                }
                                int count;
                                if (args.Length == 2)
                                {
                                    count = args[1].AsInteger();
                                    if (count < 0)
                                    {
                                        if (result != null)
                                        {
                                            result.Set(string.Empty);
                                        }
                                        return;
                                    }
                                    if (start + count > s_len)
                                    {
                                        count = s_len - start;
                                    }
                                    if (result != null)
                                    {
                                        result.Set(Sharpen.Runtime.Substring(s, start, start + count));
                                    }
                                    return;
                                }
                                else
                                {
                                    if (result != null)
                                    {
                                        result.Set(Sharpen.Runtime.Substring(s, start));
                                    }
                                }
                                return;
                            }
                            else
                            {
                                if ((hash == mStrFuncs[StrMethod_sprintf].GetHashCode() && mStrFuncs[StrMethod_sprintf
                                    ].Equals(member)))
                                {
                                    if (result != null)
                                    {
                                        string res = Utils.FormatString(s, args);
                                        result.Set(res);
                                    }
                                    return;
                                }
                                else
                                {
                                    if ((hash == mStrFuncs[StrMethod_replace].GetHashCode() && mStrFuncs[StrMethod_replace
                                        ].Equals(member)))
                                    {
                                        // string.replace(pattern, replacement-string)  -->
                                        // pattern.replace(string, replacement-string)
                                        if (args.Length < 2)
                                        {
                                            ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                        }
                                        VariantClosure clo = args[0].AsObjectClosure();
                                        Variant str = new Variant(target);
                                        Variant[] @params = new Variant[2];
                                        @params[0] = str;
                                        @params[1] = args[1];
                                        string replace_name = "replace";
                                        clo.FuncCall(0, replace_name, result, @params, null);
                                        return;
                                    }
                                    else
                                    {
                                        if ((hash == mStrFuncs[StrMethod_escape].GetHashCode() && mStrFuncs[StrMethod_escape
                                            ].Equals(member)))
                                        {
                                            if (result != null)
                                            {
                                                result.Set(LexBase.EscapeC(target));
                                            }
                                            return;
                                        }
                                        else
                                        {
                                            if ((hash == mStrFuncs[StrMethod_split].GetHashCode() && mStrFuncs[StrMethod_split
                                                ].Equals(member)))
                                            {
                                                // string.split(pattern, reserved, purgeempty) -->
                                                // Array.split(pattern, string, reserved, purgeempty)
                                                if (args.Length < 1)
                                                {
                                                    ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                                }
                                                Dispatch2 array = Tjs.CreateArrayObject();
                                                try
                                                {
                                                    Variant str = new Variant(target);
                                                    Variant[] @params;
                                                    if (args.Length >= 3)
                                                    {
                                                        @params = new Variant[4];
                                                        @params[0] = args[0];
                                                        @params[1] = str;
                                                        @params[2] = args[1];
                                                        @params[3] = args[2];
                                                    }
                                                    else
                                                    {
                                                        if (args.Length >= 2)
                                                        {
                                                            @params = new Variant[3];
                                                            @params[0] = args[0];
                                                            @params[1] = str;
                                                            @params[2] = args[1];
                                                        }
                                                        else
                                                        {
                                                            @params = new Variant[2];
                                                            @params[0] = args[0];
                                                            @params[1] = str;
                                                        }
                                                    }
                                                    string split_name = "split";
                                                    array.FuncCall(0, split_name, null, @params, array);
                                                    if (result != null)
                                                    {
                                                        result.Set(new Variant(array, array));
                                                    }
                                                }
                                                finally
                                                {
                                                    array = null;
                                                }
                                                return;
                                            }
                                            else
                                            {
                                                if ((hash == mStrFuncs[StrMethod_trim].GetHashCode() && mStrFuncs[StrMethod_trim]
                                                    .Equals(member)))
                                                {
                                                    if (args.Length != 0)
                                                    {
                                                        ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                                    }
                                                    if (result == null)
                                                    {
                                                        return;
                                                    }
                                                    result.Set(s.Trim());
                                                    return;
                                                }
                                                else
                                                {
                                                    if ((hash == mStrFuncs[StrMethod_reverse].GetHashCode() && mStrFuncs[StrMethod_reverse
                                                        ].Equals(member)))
                                                    {
                                                        if (args.Length != 0)
                                                        {
                                                            ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                                        }
                                                        if (result == null)
                                                        {
                                                            return;
                                                        }
                                                        StringBuilder builder = new StringBuilder(s_len);
                                                        for (int i = 0; i < s_len; i++)
                                                        {
                                                            builder.Append(s[s_len - i - 1]);
                                                        }
                                                        result.Set(builder.ToString());
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        if ((hash == mStrFuncs[StrMethod_repeat].GetHashCode() && mStrFuncs[StrMethod_repeat
                                                            ].Equals(member)))
                                                        {
                                                            if (args.Length != 1)
                                                            {
                                                                ThrowFrom_tjs_error(Error.E_BADPARAMCOUNT, null);
                                                            }
                                                            if (result == null)
                                                            {
                                                                return;
                                                            }
                                                            int count = args[0].AsInteger();
                                                            if (count <= 0 || s_len <= 0)
                                                            {
                                                                result.Set(string.Empty);
                                                                return;
                                                            }
                                                            int destLength = s_len * count;
                                                            StringBuilder builder = new StringBuilder(destLength);
                                                            while (count > 0)
                                                            {
                                                                builder.Append(s);
                                                                count--;
                                                            }
                                                            result.Set(builder.ToString());
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, member);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        private int CallFunctionIndirect(Variant[] ra, int ra_offset, int code, Variant[]
             args)
        {
            int hr;
            short[] ca = mCode;
            string name = ra[ra_offset + ca[code + 3]].AsString();
            //TJS_BEGIN_FUNC_CALL_ARGS(code + 4)
            Variant[] pass_args;
            int code_size;
            try
            {
                int code_offset = code + 4;
                int pass_args_count = ca[code_offset];
                if (pass_args_count == -1)
                {
                    pass_args = args;
                    pass_args_count = args.Length;
                    code_size = 1;
                }
                else
                {
                    if (pass_args_count == -2)
                    {
                        int args_v_count = 0;
                        pass_args_count = 0;
                        int arg_written_count = ca[code_offset + 1];
                        code_size = (arg_written_count << 1) + 2;
                        for (int i = 0; i < arg_written_count; i++)
                        {
                            int pos = code_offset + (i << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args_count++;
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        args_v_count += ArrayClass.GetArrayElementCount(ra[ra_offset + ca[pos + 1]].AsObject
                                            ());
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        pass_args_count += (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length -
                                            mFuncDeclUnnamedArgArrayBase) : 0;
                                        break;
                                    }
                            }
                        }
                        pass_args_count += args_v_count;
                        Variant[] pass_args_v = new Variant[args_v_count];
                        pass_args = new Variant[pass_args_count];
                        args_v_count = 0;
                        pass_args_count = 0;
                        for (int i_1 = 0; i_1 < arg_written_count; i_1++)
                        {
                            int pos = code_offset + (i_1 << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args[pass_args_count++] = ra[ra_offset + ca[pos + 1]];
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        int count = ArrayClass.CopyArrayElementTo(ra[ra_offset + ca[pos + 1]].AsObject(),
                                            pass_args_v, args_v_count, 0, -1);
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = pass_args_v[j + args_v_count];
                                        }
                                        args_v_count += count;
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        int count = (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length - mFuncDeclUnnamedArgArrayBase
                                            ) : 0;
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = args[mFuncDeclUnnamedArgArrayBase + j];
                                        }
                                        break;
                                    }
                            }
                        }
                        pass_args_v = null;
                    }
                    else
                    {
                        code_size = pass_args_count + 1;
                        pass_args = new Variant[pass_args_count];
                        for (int i = 0; i < pass_args_count; i++)
                        {
                            pass_args[i] = ra[ra_offset + ca[code_offset + 1 + i]];
                        }
                    }
                }
                //TJS_BEGIN_FUNC_CALL_ARGS(code + 4)
                Variant ra_code2 = ra[ra_offset + ca[code + 2]];
                int offset = ca[code + 1];
                if (ra_code2.IsObject())
                {
                    VariantClosure clo = ra_code2.AsObjectClosure();
                    hr = clo.FuncCall(0, name, offset != 0 ? ra[ra_offset + offset] : null, pass_args
                        , clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                }
                else
                {
                    if (ra_code2.IsString())
                    {
                        ProcessStringFunction(name, ra_code2.AsString(), pass_args, offset != 0 ? ra[ra_offset
                             + offset] : null);
                        hr = Error.S_OK;
                    }
                    else
                    {
                        if (ra_code2.IsOctet())
                        {
                            ProcessOctetFunction(name, ra_code2.AsString(), pass_args, offset != 0 ? ra[ra_offset
                                 + offset] : null);
                            hr = Error.S_OK;
                        }
                        else
                        {
                            string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                                (ra_code2));
                            throw new VariantException(mes);
                        }
                    }
                }
            }
            finally
            {
                pass_args = null;
            }
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, name);
            }
            return code_size + 4;
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private int CallFunctionDirect(Variant[] ra, int ra_offset, int code, Variant[] args
            )
        {
            int hr;
            short[] ca = mCode;
            //TJS_BEGIN_FUNC_CALL_ARGS(code + 4)
            Variant[] pass_args;
            //Variant[] pass_args_p = new Variant[PASS_ARGS_PREPARED_ARRAY_COUNT];
            //Variant[] pass_args_v = null;
            int code_size;
            try
            {
                int code_offset = code + 4;
                int pass_args_count = ca[code_offset];
                if (pass_args_count == -1)
                {
                    pass_args = args;
                    pass_args_count = args.Length;
                    code_size = 1;
                }
                else
                {
                    if (pass_args_count == -2)
                    {
                        int args_v_count = 0;
                        pass_args_count = 0;
                        int arg_written_count = ca[code_offset + 1];
                        code_size = (arg_written_count << 1) + 2;
                        for (int i = 0; i < arg_written_count; i++)
                        {
                            int pos = code_offset + (i << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args_count++;
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        args_v_count += ArrayClass.GetArrayElementCount(ra[ra_offset + ca[pos + 1]].AsObject
                                            ());
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        pass_args_count += (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length -
                                            mFuncDeclUnnamedArgArrayBase) : 0;
                                        break;
                                    }
                            }
                        }
                        pass_args_count += args_v_count;
                        Variant[] pass_args_v = new Variant[args_v_count];
                        pass_args = new Variant[pass_args_count];
                        args_v_count = 0;
                        pass_args_count = 0;
                        for (int i_1 = 0; i_1 < arg_written_count; i_1++)
                        {
                            int pos = code_offset + (i_1 << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args[pass_args_count++] = ra[ra_offset + ca[pos + 1]];
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        int count = ArrayClass.CopyArrayElementTo(ra[ra_offset + ca[pos + 1]].AsObject(),
                                            pass_args_v, args_v_count, 0, -1);
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = pass_args_v[j + args_v_count];
                                        }
                                        args_v_count += count;
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        int count = (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length - mFuncDeclUnnamedArgArrayBase
                                            ) : 0;
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = args[mFuncDeclUnnamedArgArrayBase + j];
                                        }
                                        break;
                                    }
                            }
                        }
                        pass_args_v = null;
                    }
                    else
                    {
                        code_size = pass_args_count + 1;
                        pass_args = new Variant[pass_args_count];
                        for (int i = 0; i < pass_args_count; i++)
                        {
                            pass_args[i] = ra[ra_offset + ca[code_offset + 1 + i]];
                        }
                    }
                }
                //TJS_BEGIN_FUNC_CALL_ARGS(code + 4)
                Variant ra_code2 = ra[ra_offset + ca[code + 2]];
                Variant name = mDataArray[ca[code + 3]];
                int offset = ca[code + 1];
                if (ra_code2.IsObject())
                {
                    VariantClosure clo = ra_code2.AsObjectClosure();
                    string nameStr = name.GetString();
                    hr = clo.FuncCall(0, nameStr, offset != 0 ? ra[ra_offset + offset] : null, pass_args
                        , clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                }
                else
                {
                    if (ra_code2.IsString())
                    {
                        ProcessStringFunction(name.GetString(), ra_code2.AsString(), pass_args, offset !=
                             0 ? ra[ra_offset + offset] : null);
                        hr = Error.S_OK;
                    }
                    else
                    {
                        if (ra_code2.IsOctet())
                        {
                            ProcessOctetFunction(name.GetString(), ra_code2.AsString(), pass_args, offset !=
                                0 ? ra[ra_offset + offset] : null);
                            hr = Error.S_OK;
                        }
                        else
                        {
                            string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                                (ra_code2));
                            throw new VariantException(mes);
                        }
                    }
                }
            }
            finally
            {
                pass_args = null;
            }
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, mDataArray[ca[code + 3]].AsString());
            }
            return code_size + 4;
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private int CallFunction(Variant[] ra, int ra_offset, int code, Variant[] args)
        {
            // function calling / create new object
            int hr;
            short[] ca = mCode;
            //TJS_BEGIN_FUNC_CALL_ARGS(code + 3)
            Variant[] pass_args;
            int code_size;
            try
            {
                int code_offset = code + 3;
                int pass_args_count = ca[code_offset];
                if (pass_args_count == -1)
                {
                    pass_args = args;
                    pass_args_count = args.Length;
                    code_size = 1;
                }
                else
                {
                    if (pass_args_count == -2)
                    {
                        int args_v_count = 0;
                        pass_args_count = 0;
                        int arg_written_count = ca[code_offset + 1];
                        code_size = (arg_written_count << 1) + 2;
                        for (int i = 0; i < arg_written_count; i++)
                        {
                            int pos = code_offset + (i << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args_count++;
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        args_v_count += ArrayClass.GetArrayElementCount(ra[ra_offset + ca[pos + 1]].AsObject
                                            ());
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        pass_args_count += (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length -
                                            mFuncDeclUnnamedArgArrayBase) : 0;
                                        break;
                                    }
                            }
                        }
                        pass_args_count += args_v_count;
                        Variant[] pass_args_v = new Variant[args_v_count];
                        pass_args = new Variant[pass_args_count];
                        args_v_count = 0;
                        pass_args_count = 0;
                        for (int i_1 = 0; i_1 < arg_written_count; i_1++)
                        {
                            int pos = code_offset + (i_1 << 1) + 2;
                            switch (ca[pos])
                            {
                                case fatNormal:
                                    {
                                        pass_args[pass_args_count++] = ra[ra_offset + ca[pos + 1]];
                                        break;
                                    }

                                case fatExpand:
                                    {
                                        int count = ArrayClass.CopyArrayElementTo(ra[ra_offset + ca[pos + 1]].AsObject(),
                                            pass_args_v, args_v_count, 0, -1);
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = pass_args_v[j + args_v_count];
                                        }
                                        args_v_count += count;
                                        break;
                                    }

                                case fatUnnamedExpand:
                                    {
                                        int count = (args.Length > mFuncDeclUnnamedArgArrayBase) ? (args.Length - mFuncDeclUnnamedArgArrayBase
                                            ) : 0;
                                        for (int j = 0; j < count; j++)
                                        {
                                            pass_args[pass_args_count++] = args[mFuncDeclUnnamedArgArrayBase + j];
                                        }
                                        break;
                                    }
                            }
                        }
                        pass_args_v = null;
                    }
                    else
                    {
                        code_size = pass_args_count + 1;
                        pass_args = new Variant[pass_args_count];
                        for (int i = 0; i < pass_args_count; i++)
                        {
                            pass_args[i] = ra[ra_offset + ca[code_offset + 1 + i]];
                        }
                    }
                }
                //TJS_BEGIN_FUNC_CALL_ARGS(code + 3)
                VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
                int offset = ca[code + 1];
                int op = ca[code];
                if (op == VM_CALL)
                {
                    hr = clo.FuncCall(0, null, offset != 0 ? ra[ra_offset + offset] : null, pass_args
                        , clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                }
                else
                {
                    Holder<Dispatch2> dsp = new Holder<Dispatch2>(null);
                    hr = clo.CreateNew(0, null, dsp, pass_args, clo.mObjThis != null ? clo.mObjThis :
                        ra[ra_offset - 1].AsObject());
                    if (hr >= 0)
                    {
                        if (dsp.mValue != null)
                        {
                            if (offset != 0)
                            {
                                ra[ra_offset + offset].Set(dsp.mValue, dsp.mValue);
                            }
                        }
                    }
                }
            }
            catch (TjsScriptException e)
            {
                // OriginalTODO: Null Check
                Sharpen.Runtime.PrintStackTrace(e);
                throw;
            }
            catch (TjsScriptError e)
            {
                Sharpen.Runtime.PrintStackTrace(e);
                throw;
            }
            catch (TjsException e)
            {
                Sharpen.Runtime.PrintStackTrace(e);
                throw;
            }
            catch (Exception e)
            {
                Sharpen.Runtime.PrintStackTrace(e);
                throw new TjsException(e.Message);
            }
            finally
            {
                pass_args = null;
            }
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, string.Empty);
            }
            return code_size + 3;
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        private void InstanceOf(Variant name, Variant targ)
        {
            // checks instance inheritance.
            string str = name.AsString();
            if (str != null)
            {
                int hr = CustomObject.DefaultIsInstanceOf(0, targ, str, null);
                if (hr < 0)
                {
                    ThrowFrom_tjs_error(hr, null);
                }
                targ.Set((hr == Error.S_TRUE) ? 1 : 0);
                return;
            }
            targ.Set(0);
        }

        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        /// <exception cref="CompileException"></exception>
        private void Eval(Variant val, Dispatch2 objthis, bool resneed)
        {
            Variant res = new Variant();
            string str = val.AsString();
            if (str.Length > 0)
            {
                if (resneed)
                {
                    mBlock.GetTJS().EvalExpression(str, res, objthis, null, 0);
                }
                else
                {
                    mBlock.GetTJS().EvalExpression(str, null, objthis, null, 0);
                }
            }
            if (resneed)
            {
                val.Set(res);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void TypeOfMemberIndirect(Variant[] ra, int ra_offset, int code, int flags
            )
        {
            short[] ca = mCode;
            Variant ra1 = ra[ra_offset + ca[code + 1]];
            Variant ra2 = ra[ra_offset + ca[code + 2]];
            Variant ra3 = ra[ra_offset + ca[code + 3]];
            if (ra2.IsObject())
            {
                VariantClosure clo = ra2.AsObjectClosure();
                if (ra3.IsInteger() != true)
                {
                    string str = ra3.AsString();
                    int hr = clo.PropGet(flags, str, ra1, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset
                         - 1].AsObject());
                    if (hr == Error.S_OK)
                    {
                        TypeOf(ra1);
                    }
                    else
                    {
                        if (hr == Error.E_MEMBERNOTFOUND)
                        {
                            // ra[ra_offset+mCode[code+1]] = new Variant("undefined");
                            ra1.Set("undefined");
                        }
                        else
                        {
                            if (hr < 0)
                            {
                                ThrowFrom_tjs_error(hr, str);
                            }
                        }
                    }
                }
                else
                {
                    int hr = clo.PropGetByNum(flags, ra3.AsInteger(), ra1, clo.mObjThis != null ? clo
                        .mObjThis : ra[ra_offset - 1].AsObject());
                    if (hr == Error.S_OK)
                    {
                        TypeOf(ra1);
                    }
                    else
                    {
                        if (hr == Error.E_MEMBERNOTFOUND)
                        {
                            ra1.Set("undefined");
                        }
                        else
                        {
                            if (hr < 0)
                            {
                                ThrowFrom_tjs_error_num(hr, ra3.AsInteger());
                            }
                        }
                    }
                }
            }
            else
            {
                if (ra2.IsString())
                {
                    GetStringProperty(ra1, ra2, ra3);
                    TypeOf(ra1);
                }
                else
                {
                    if (ra2.IsOctet())
                    {
                        GetOctetProperty(ra1, ra2, ra3);
                        TypeOf(ra1);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra2));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void TypeOfMemberDirect(Variant[] ra, int ra_offset, int code, int flags)
        {
            short[] ca = mCode;
            Variant ra1 = ra[ra_offset + ca[code + 1]];
            Variant ra2 = ra[ra_offset + ca[code + 2]];
            Variant da3 = mDataArray[ca[code + 3]];
            if (ra2.IsObject())
            {
                int hr;
                VariantClosure clo = ra2.AsObjectClosure();
                string name = da3.GetString();
                hr = clo.PropGet(flags, name, ra1, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset
                     - 1].AsObject());
                if (hr == Error.S_OK)
                {
                    TypeOf(ra1);
                }
                else
                {
                    if (hr == Error.E_MEMBERNOTFOUND)
                    {
                        ra1.Set("undefined");
                    }
                    else
                    {
                        if (hr < 0)
                        {
                            ThrowFrom_tjs_error(hr, name);
                        }
                    }
                }
            }
            else
            {
                if (ra2.IsString())
                {
                    GetStringProperty(ra1, ra2, da3);
                    TypeOf(ra1);
                }
                else
                {
                    if (ra2.IsOctet())
                    {
                        GetOctetProperty(ra1, ra2, da3);
                        TypeOf(ra1);
                    }
                    else
                    {
                        string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
                            (ra2));
                        throw new VariantException(mes);
                    }
                }
            }
        }

        private void TypeOf(Variant val)
        {
            // processes TJS2's typeof operator.
            string name = val.GetTypeName();
            if (name != null)
            {
                val.Set(name);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperateProperty0(Variant[] ra, int ra_offset, int code, int ope)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            int offset = ca[code + 1];
            int hr = clo.Operation(ope, null, offset != 0 ? ra[ra_offset + offset] : null, null
                , clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1].AsObject());
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, null);
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperatePropertyIndirect0(Variant[] ra, int ra_offset, int code, int
            ope)
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            Variant ra_code3 = ra[ra_offset + ca[code + 3]];
            int offset = ca[code + 1];
            if (ra_code3.IsInteger() != true)
            {
                string str = ra_code3.AsString();
                int hr = clo.Operation(ope, str, offset != 0 ? ra[ra_offset + offset] : null, null
                    , clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1].AsObject());
                if (hr < 0)
                {
                    ThrowFrom_tjs_error(hr, str);
                }
            }
            else
            {
                int hr = clo.OperationByNum(ope, ra_code3.AsInteger(), offset != 0 ? ra[ra_offset
                     + offset] : null, null, clo.mObjThis != null ? clo.mObjThis : ra[ra_offset - 1]
                    .AsObject());
                if (hr < 0)
                {
                    ThrowFrom_tjs_error_num(hr, ra_code3.AsInteger());
                }
            }
        }

        /// <exception cref="TjsException"></exception>
        /// <exception cref="VariantException"></exception>
        private void OperatePropertyDirect0(Variant[] ra, int ra_offset, int code, int ope
            )
        {
            short[] ca = mCode;
            VariantClosure clo = ra[ra_offset + ca[code + 2]].AsObjectClosure();
            Variant name = mDataArray[ca[code + 3]];
            string nameStr = name.GetString();
            int offset = ca[code + 1];
            int hr = clo.Operation(ope, nameStr, offset != 0 ? ra[ra_offset + offset] : null,
                null, ra[ra_offset - 1].AsObject());
            if (hr < 0)
            {
                ThrowFrom_tjs_error(hr, nameStr);
            }
        }

        private void ContinuousClear(Variant[] ra, int ra_offset, int code)
        {
            short[] ca = mCode;
            int start = ra_offset + ca[code + 1];
            int end = start + ca[code + 2];
            while (start < end)
            {
                ra[start].Clear();
                start++;
            }
        }

        public virtual string GetPositionDescriptionString(int codepos)
        {
            return mBlock.GetLineDescriptionString(CodePosToSrcPos(codepos)) + "[" + GetShortDescription
                () + "]";
        }

        private string GetShortDescription()
        {
            string ret = "(" + GetContextTypeName() + ")";
            string name;
            if (mContextType == ContextType.PROPERTY_SETTER || mContextType == ContextType.PROPERTY_GETTER)
            {
                if (mParent != null)
                {
                    name = mParent.mName;
                }
                else
                {
                    name = null;
                }
            }
            else
            {
                name = mName;
            }
            if (name != null)
            {
                ret += " " + name;
            }
            return ret;
        }

        public virtual string GetContextTypeName()
        {
            switch (mContextType)
            {
                case ContextType.TOP_LEVEL:
                    {
                        return "top level script";
                    }

                case ContextType.FUNCTION:
                    {
                        return "function";
                    }

                case ContextType.EXPR_FUNCTION:
                    {
                        return "function expression";
                    }

                case ContextType.PROPERTY:
                    {
                        return "property";
                    }

                case ContextType.PROPERTY_SETTER:
                    {
                        return "property setter";
                    }

                case ContextType.PROPERTY_GETTER:
                    {
                        return "property getter";
                    }

                case ContextType.CLASS:
                    {
                        return "class";
                    }

                case ContextType.SUPER_CLASS_GETTER:
                    {
                        return "super class getter proxy";
                    }

                default:
                    {
                        return "unknown";
                    }
            }
        }

        // Dispatch2 implementation
        // function invocation
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int FuncCall(int flag, string membername, Variant result, Variant
            [] param, Dispatch2 objthis)
        {
            if (!GetValidity())
            {
                return Error.E_INVALIDOBJECT;
            }
            if (membername == null)
            {
                switch (mContextType)
                {
                    case ContextType.TOP_LEVEL:
                        {
                            ExecuteAsFunction(objthis != null ? objthis : mBlock.GetTJS().GetGlobal(), null,
                                result, 0);
                            break;
                        }

                    case ContextType.FUNCTION:
                    case ContextType.EXPR_FUNCTION:
                    case ContextType.PROPERTY_GETTER:
                    case ContextType.PROPERTY_SETTER:
                        {
                            ExecuteAsFunction(objthis, param, result, 0);
                            break;
                        }

                    case ContextType.CLASS:
                        {
                            // on super class' initialization
                            ExecuteAsFunction(objthis, param, result, 0);
                            break;
                        }

                    case ContextType.PROPERTY:
                        {
                            return Error.E_INVALIDTYPE;
                        }
                }
                return Error.S_OK;
            }
            int hr = base.FuncCall(flag, membername, result, param, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count > 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        int v = pointer[i];
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, v);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.FuncCall(flag, membername, result, param, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // property get
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int PropGet(int flag, string membername, Variant result, Dispatch2
             objthis)
        {
            if (!GetValidity())
            {
                return Error.E_INVALIDOBJECT;
            }
            if (membername == null)
            {
                if (mContextType == ContextType.PROPERTY)
                {
                    // executed as a property getter
                    if (mPropGetter != null)
                    {
                        return mPropGetter.FuncCall(0, null, result, null, objthis);
                    }
                    else
                    {
                        return Error.E_ACCESSDENIED;
                    }
                }
            }
            int hr = base.PropGet(flag, membername, result, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.PropGet(flag, membername, result, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // property set
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int PropSet(int flag, string membername, Variant param, Dispatch2
             objthis)
        {
            if (!GetValidity())
            {
                return Error.E_INVALIDOBJECT;
            }
            if (membername == null)
            {
                if (mContextType == ContextType.PROPERTY)
                {
                    // executed as a property setter
                    if (mPropSetter != null)
                    {
                        Variant[] @params = new Variant[1];
                        @params[0] = param;
                        return mPropSetter.FuncCall(0, null, null, @params, objthis);
                    }
                    else
                    {
                        return Error.E_ACCESSDENIED;
                    }
                }
            }
            // WARNING!! const tTJSVariant ** -> tTJSVariant** force casting
            int hr;
            if (membername != null && mContextType == ContextType.CLASS && mSuperClassGetter
                != null)
            {
                int pseudo_flag = (flag & Interface.IGNOREPROP) != 0 ? flag : (flag & ~Interface.
                    MEMBERENSURE);
                // member ensuring is temporarily disabled unless Interface.IGNOREPROP
                hr = base.PropSet(pseudo_flag, membername, param, objthis);
                if (hr == Error.E_MEMBERNOTFOUND)
                {
                    int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                    int count = pointer.Length;
                    if (count != 0)
                    {
                        Variant res = new Variant();
                        for (int i = count - 1; i >= 0; i--)
                        {
                            mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                            VariantClosure clo = res.AsObjectClosure();
                            hr = clo.PropSet(pseudo_flag, membername, param, objthis);
                            if (hr != Error.E_MEMBERNOTFOUND)
                            {
                                break;
                            }
                        }
                    }
                }
                if (hr == Error.E_MEMBERNOTFOUND && (flag & Interface.MEMBERENSURE) != 0)
                {
                    // re-ensure the member for "this" object
                    hr = base.PropSet(flag, membername, param, objthis);
                }
            }
            else
            {
                hr = base.PropSet(flag, membername, param, objthis);
            }
            return hr;
        }

        // create new object
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int CreateNew(int flag, string membername, Holder<Dispatch2> result
            , Variant[] param, Dispatch2 objthis)
        {
            if (!GetValidity())
            {
                return Error.E_INVALIDOBJECT;
            }
            if (membername == null)
            {
                if (mContextType != ContextType.CLASS)
                {
                    return Error.E_INVALIDTYPE;
                }
                Dispatch2 dsp = new CustomObject();
                ExecuteAsFunction(dsp, null, null, 0);
                FuncCall(0, mName, null, param, dsp);
                result.Set(dsp);
                return Error.S_OK;
            }
            int hr = base.CreateNew(flag, membername, result, param, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.CreateNew(flag, membername, result, param, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // class instance matching returns false or true
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int IsInstanceOf(int flag, string membername, string classname, Dispatch2
             objthis)
        {
            if (!GetValidity())
            {
                return Error.E_INVALIDOBJECT;
            }
            if (membername == null)
            {
                switch (mContextType)
                {
                    case ContextType.TOP_LEVEL:
                    case ContextType.PROPERTY_SETTER:
                    case ContextType.PROPERTY_GETTER:
                    case ContextType.SUPER_CLASS_GETTER:
                        {
                            break;
                        }

                    case ContextType.FUNCTION:
                    case ContextType.EXPR_FUNCTION:
                        {
                            if ("Function".Equals(classname))
                            {
                                return Error.S_TRUE;
                            }
                            break;
                        }

                    case ContextType.PROPERTY:
                        {
                            if ("Property".Equals(classname))
                            {
                                return Error.S_TRUE;
                            }
                            break;
                        }

                    case ContextType.CLASS:
                        {
                            if ("Class".Equals(classname))
                            {
                                return Error.S_TRUE;
                            }
                            break;
                        }
                }
            }
            int hr = base.IsInstanceOf(flag, membername, classname, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.IsInstanceOf(flag, membername, classname, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // get member count
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int GetCount(IntWrapper result, string membername, Dispatch2 objthis
            )
        {
            int hr = base.GetCount(result, membername, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.GetCount(result, membername, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // delete member
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int DeleteMember(int flag, string membername, Dispatch2 objthis)
        {
            int hr = base.DeleteMember(flag, membername, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.DeleteMember(flag, membername, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // invalidation
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int Invalidate(int flag, string membername, Dispatch2 objthis)
        {
            int hr = base.Invalidate(flag, membername, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.Invalidate(flag, membername, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // get validation, returns true or false
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int IsValid(int flag, string membername, Dispatch2 objthis)
        {
            int hr = base.IsValid(flag, membername, objthis);
            if (membername != null && hr == Error.E_MEMBERNOTFOUND && mContextType == ContextType
                .CLASS && mSuperClassGetter != null)
            {
                // look up super class
                int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                int count = pointer.Length;
                if (count != 0)
                {
                    Variant res = new Variant();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                        VariantClosure clo = res.AsObjectClosure();
                        hr = clo.IsValid(flag, membername, objthis);
                        if (hr != Error.E_MEMBERNOTFOUND)
                        {
                            break;
                        }
                    }
                }
            }
            return hr;
        }

        // operation with member
        /// <exception cref="VariantException"></exception>
        /// <exception cref="TjsException"></exception>
        public override int Operation(int flag, string membername, Variant result, Variant
             param, Dispatch2 objthis)
        {
            if (membername == null)
            {
                if (mContextType == ContextType.PROPERTY)
                {
                    // operation for property object
                    return base.DispatchOperation(flag, membername, result, param, objthis);
                }
                else
                {
                    return base.Operation(flag, membername, result, param, objthis);
                }
            }
            int hr;
            if (membername != null && mContextType == ContextType.CLASS && mSuperClassGetter
                != null)
            {
                int pseudo_flag = (flag & Interface.IGNOREPROP) != 0 ? flag : (flag & ~Interface.
                    MEMBERENSURE);
                hr = base.Operation(pseudo_flag, membername, result, param, objthis);
                if (hr == Error.E_MEMBERNOTFOUND)
                {
                    // look up super class
                    int[] pointer = mSuperClassGetter.mSuperClassGetterPointer;
                    int count = pointer.Length;
                    if (count != 0)
                    {
                        Variant res = new Variant();
                        for (int i = count - 1; i >= 0; i--)
                        {
                            mSuperClassGetter.ExecuteAsFunction(null, null, res, pointer[i]);
                            VariantClosure clo = res.AsObjectClosure();
                            hr = clo.Operation(pseudo_flag, membername, result, param, objthis);
                            if (hr != Error.E_MEMBERNOTFOUND)
                            {
                                break;
                            }
                        }
                    }
                }
                if (hr == Error.E_MEMBERNOTFOUND)
                {
                    hr = base.Operation(flag, membername, result, param, objthis);
                }
                return hr;
            }
            else
            {
                return base.Operation(flag, membername, result, param, objthis);
            }
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
                char v = str[0];
                val.Set(v);
            }
            else
            {
                val.Set(0);
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

        private const int fatNormal = 0;

        private const int fatExpand = 1;

        private const int fatUnnamedExpand = 2;

        private const int OP_BAND = unchecked((int)(0x0001));

        private const int OP_BOR = unchecked((int)(0x0002));

        private const int OP_BXOR = unchecked((int)(0x0003));

        private const int OP_SUB = unchecked((int)(0x0004));

        private const int OP_ADD = unchecked((int)(0x0005));

        private const int OP_MOD = unchecked((int)(0x0006));

        private const int OP_DIV = unchecked((int)(0x0007));

        private const int OP_IDIV = unchecked((int)(0x0008));

        private const int OP_MUL = unchecked((int)(0x0009));

        private const int OP_LOR = unchecked((int)(0x000a));

        private const int OP_LAND = unchecked((int)(0x000b));

        private const int OP_SAR = unchecked((int)(0x000c));

        private const int OP_SAL = unchecked((int)(0x000d));

        private const int OP_SR = unchecked((int)(0x000e));

        private const int OP_INC = unchecked((int)(0x000f));

        private const int OP_DEC = unchecked((int)(0x0010));

        // VMCodes
        //		__VM_LAST	= 128;
        // FuncArgType
        //		OP_MASK	= 0x001f,
        //		OP_MIN	= OP_BAND,
        //		OP_MAX	= OP_DEC;
        public virtual int GetCodeSize()
        {
            return mCode.Length;
        }

        public virtual int GetDataSize()
        {
            return mDataArray.Length;
        }

        public virtual int SrcPosToLine(int srcpos)
        {
            return mBlock.SrcPosToLine(srcpos);
        }

        public virtual string GetLine(int line)
        {
            return mBlock.GetLine(line);
        }

        public string GetName()
        {
            return mName;
        }

        /// <summary>DaraArray の中の InterCodeGenerator を InterCodeObject に差し替える</summary>
        /// <param name="compiler"></param>
        public virtual void DateReplace(Compiler compiler)
        {
            int count = mDataArray.Length;
            Variant[] da = mDataArray;
            for (int i = 0; i < count; i++)
            {
                Variant d = da[i];
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

        public virtual string GetScript()
        {
            return mBlock.GetScript();
        }

        public virtual int GetLineOffset()
        {
            return mBlock.GetLineOffset();
        }

        public override string ToString()
        {
            var s = base.ToString();
            if (string.IsNullOrWhiteSpace(s) || s.Equals("empty"))
            {
                s = mName;
            }
            return s;
        }
    }
}

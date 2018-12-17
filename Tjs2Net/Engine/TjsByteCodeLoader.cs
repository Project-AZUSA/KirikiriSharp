using System;
using System.IO;
using Tjs2.Engine;
using Tjs2.Sharpen;

namespace Tjs2
{
    /// <summary>
    /// A better implemented ByteCodeLoader rather than the silly Sharpen one
    /// </summary>
    //Moved from Tjs2.Sharper
    public class TjsByteCodeLoader
    {
        private const bool LOAD_SRC_POS = false;

        public const string FILE_TAG_LE = "TJS2";
        public const string VER_TAG_LE = "100";
        private const string OBJ_TAG_LE = "OBJS";
        private const string DATA_TAG_LE = "DATA";

        private const int TYPE_VOID = 0;

        private const int TYPE_OBJECT = 1;

        private const int TYPE_INTER_OBJECT = 2;

        private const int TYPE_STRING = 3;

        private const int TYPE_OCTET = 4;

        private const int TYPE_REAL = 5;

        private const int TYPE_BYTE = 6;

        private const int TYPE_SHORT = 7;

        private const int TYPE_INTEGER = 8;

        private const int TYPE_LONG = 9;

        private const int TYPE_INTER_GENERATOR = 10;

        private const int TYPE_UNKNOWN = -1;

        private static byte[] mByteArray;

        private static short[] mShortArray;

        private static int[] mIntArray;

        private static long[] mLongArray;

        private static double[] mDoubleArray;

        private static long[] mDoubleTmpArray;

        private static string[] mStringArray;

        private static ByteBuffer[] mByteBufferArray;

        private static short[] mVariantTypeData;

        private const int MIN_BYTE_COUNT = 64;

        private const int MIN_SHORT_COUNT = 64;

        private const int MIN_INT_COUNT = 64;

        private const int MIN_DOUBLE_COUNT = 8;

        private const int MIN_LONG_COUNT = 8;

        private const int MIN_STRING_COUNT = 1024;

        private static bool mDeleteBuffer;

        private static byte[] mReadBuffer;

        private const int MIN_READ_BUFFER_SIZE = 160 * 1024;

        internal class ObjectsCache
        {
            public InterCodeObject[] mObjs;

            public AList<VariantRepalace> mWork;

            public int[] mParent;

            public int[] mPropSetter;

            public int[] mPropGetter;

            public int[] mSuperClassGetter;

            public int[][] mProperties;

            private const int MIN_COUNT = 500;

            // temporary
            //static private final int MIN_VARIANT_DATA_COUNT = 400*2;
            public virtual void Create(int count)
            {
                if (count < MIN_COUNT)
                {
                    count = MIN_COUNT;
                }
                if (mWork == null)
                {
                    mWork = new AList<VariantRepalace>();
                }
                mWork.Clear();
                if (mObjs == null || mObjs.Length < count)
                {
                    mObjs = new InterCodeObject[count];
                    mParent = new int[count];
                    mPropSetter = new int[count];
                    mPropGetter = new int[count];
                    mSuperClassGetter = new int[count];
                    mProperties = new int[count][];
                }
            }

            public virtual void Release()
            {
                mWork = null;
                mObjs = null;
                mParent = null;
                mPropSetter = null;
                mPropGetter = null;
                mSuperClassGetter = null;
                mProperties = null;
            }
        }

        private static ObjectsCache mObjectsCache;

        public static void Initialize()
        {
            mDeleteBuffer = false;
            mReadBuffer = null;
            mByteArray = null;
            mShortArray = null;
            mIntArray = null;
            mLongArray = null;
            mDoubleArray = null;
            mDoubleTmpArray = null;
            mStringArray = null;
            mByteBufferArray = null;
            mObjectsCache = new ObjectsCache();
            mVariantTypeData = null;
        }

        public static void FinalizeApplication()
        {
            mDeleteBuffer = true;
            mReadBuffer = null;
            mByteArray = null;
            mShortArray = null;
            mIntArray = null;
            mLongArray = null;
            mDoubleArray = null;
            mDoubleTmpArray = null;
            mStringArray = null;
            mByteBufferArray = null;
            mObjectsCache = null;
            mVariantTypeData = null;
        }

        public static void AllwaysFreeReadBuffer()
        {
            mDeleteBuffer = true;
            mReadBuffer = null;
            mByteArray = null;
            mShortArray = null;
            mIntArray = null;
            mLongArray = null;
            mDoubleArray = null;
            mDoubleTmpArray = null;
            mStringArray = null;
            mByteBufferArray = null;
            mObjectsCache.Release();
            mVariantTypeData = null;
        }

        public TjsByteCodeLoader()
        {
            Initialize();
        }

        /// <exception cref="Kirikiri.Tjs2.TJSException"></exception>
        public virtual ScriptBlock ReadByteCode(Tjs owner, string name, BinaryStream input)
        {
            try
            {
                var br = new BinaryReader(input.GetInputStream());
                int size = (int) input.GetSize();

                // TJS2
                var tag = br.ReadChars(4).ToRealString();
                if (tag != FILE_TAG_LE)
                {
                    return null;
                }
                // 100'\0'
                if (br.ReadChars(3).ToRealString() != VER_TAG_LE)
                {
                    return null;
                }
                br.ReadChar();

                int filesize = br.ReadInt32();
                if (filesize != size)
                {
                    return null;
                }
                //// DATA
                if (br.ReadChars(4).ToRealString() != DATA_TAG_LE)
                {
                    return null;
                }
                size = br.ReadInt32();
                ReadDataArea(br, size);
                // これがデータエリア后の位置
                // OBJS
                if (br.ReadChars(4).ToRealString() != OBJ_TAG_LE)
                {
                    return null;
                }
                //int objsize = ibuff.get();
                ScriptBlock block = new ScriptBlock(owner, name, 0, null, null);
                ReadObjects(block, br);
                return block;
            }
            finally
            {
                if (mDeleteBuffer)
                {
                    mReadBuffer = null;
                    mByteArray = null;
                    mShortArray = null;
                    mIntArray = null;
                    mLongArray = null;
                    mDoubleArray = null;
                    mDoubleTmpArray = null;
                    mStringArray = null;
                    mByteBufferArray = null;
                    mObjectsCache.Release();
                    mVariantTypeData = null;
                }
            }
        }

        /// <summary>InterCodeObject へ置换するために一时的に觉えておくクラス</summary>
        internal class VariantRepalace
        {
            public Variant Work;

            public int Index;

            public VariantRepalace(Variant w, int i)
            {
                Work = w;
                Index = i;
            }
        }

        /// <exception cref="Kirikiri.Tjs2.TJSException"></exception>
        private void ReadObjects(ScriptBlock block, BinaryReader br)
        {
            int totalSize = br.ReadInt32();

            string[] strarray = mStringArray;
            ByteBuffer[] bbarray = mByteBufferArray;
            double[] dblarray = mDoubleArray;
            byte[] barray = mByteArray;
            short[] sarray = mShortArray;
            int[] iarray = mIntArray;
            long[] larray = mLongArray;
            int toplevel = br.ReadInt32();
            int objcount = br.ReadInt32();

            mObjectsCache.Create(objcount);
            InterCodeObject[] objs = mObjectsCache.mObjs;
            AList<VariantRepalace> work = mObjectsCache.mWork;
            int[] parent = mObjectsCache.mParent;
            int[] propSetter = mObjectsCache.mPropSetter;
            int[] propGetter = mObjectsCache.mPropGetter;
            int[] superClassGetter = mObjectsCache.mSuperClassGetter;
            int[][] properties = mObjectsCache.mProperties;
            for (int o = 0; o < objcount; o++)
            {
                if (br.ReadChars(4).ToRealString() != FILE_TAG_LE)
                {
                    throw new TjsException("ByteCode Broken");
                }
                int objsize = br.ReadInt32();
                parent[o] = br.ReadInt32();
                int name = br.ReadInt32();
                int contextType = br.ReadInt32();
                int maxVariableCount = br.ReadInt32();
                int variableReserveCount = br.ReadInt32();
                int maxFrameCount = br.ReadInt32();
                int funcDeclArgCount = br.ReadInt32();
                int funcDeclUnnamedArgArrayBase = br.ReadInt32();
                int funcDeclCollapseBase = br.ReadInt32();
                propSetter[o] = br.ReadInt32();
                propGetter[o] = br.ReadInt32(); 
                superClassGetter[o] = br.ReadInt32();
                int count = br.ReadInt32();
                LongBuffer srcpos = null;
                // codePos/srcPos は今のところ使ってない、ソート济みなので、longにする必要はないが……
                // codePos/srcPos currently not used. it's for debug. Please refer to newer krkrz code and fix here later.
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        br.ReadInt64();
                    }
                }
                else
                {
                    //br.BaseStream.Seek(count << 3, SeekOrigin.Current);
                    //br.ReadInt32();
                }

                count = br.ReadInt32();
                short[] code = new short[count];
                for (int i = 0; i < count; i++)
                {
                    code[i] = br.ReadInt16();
                }
                //TranslateCodeAddress( block, code, codeSize );
                var padding = 4 - (count * 2) % 4;
                if (padding > 0 && padding < 4)
                {
                    br.ReadBytes(padding);
                }

                count = br.ReadInt32();
                int vcount = count * 2;
                if (mVariantTypeData == null || mVariantTypeData.Length < vcount)
                {
                    mVariantTypeData = new short[vcount];
                }
                for (int i = 0; i < vcount; i++)
                {
                    mVariantTypeData[i] = br.ReadInt16();
                }
                Variant[] vdata = new Variant[count];
                int datacount = count;
                Variant tmp;
                for (int i = 0; i < datacount; i++)
                {
                    int pos = i << 1;
                    int type = mVariantTypeData[pos];
                    int index = mVariantTypeData[pos + 1];
                    switch (type)
                    {
                        case TYPE_VOID:
                        {
                            vdata[i] = new Variant();
                            // null
                            break;
                        }

                        case TYPE_OBJECT:
                        {
                            vdata[i] = new Variant(null, null);
                            // null Array Dictionary はまだサポートしていない TODO
                            break;
                        }

                        case TYPE_INTER_OBJECT:
                        {
                            tmp = new Variant();
                            work.AddItem(new VariantRepalace(tmp, index));
                            vdata[i] = tmp;
                            break;
                        }

                        case TYPE_INTER_GENERATOR:
                        {
                            tmp = new Variant();
                            work.AddItem(new VariantRepalace(tmp, index));
                            vdata[i] = tmp;
                            break;
                        }

                        case TYPE_STRING:
                        {
                            vdata[i] = new Variant(strarray[index]);
                            break;
                        }

                        case TYPE_OCTET:
                        {
                            vdata[i] = new Variant(bbarray[index]);
                            break;
                        }

                        case TYPE_REAL:
                        {
                            vdata[i] = new Variant(dblarray[index]);
                            break;
                        }

                        case TYPE_BYTE:
                        {
                            vdata[i] = new Variant(barray[index]);
                            break;
                        }

                        case TYPE_SHORT:
                        {
                            vdata[i] = new Variant(sarray[index]);
                            break;
                        }

                        case TYPE_INTEGER:
                        {
                            vdata[i] = new Variant(iarray[index]);
                            break;
                        }

                        case TYPE_LONG:
                        {
                            vdata[i] = new Variant(larray[index]);
                            break;
                        }

                        case TYPE_UNKNOWN:
                        default:
                        {
                            vdata[i] = new Variant();
                            // null;
                            break;
                            break;
                        }
                    }
                }
                count = br.ReadInt32();
                int[] scgetterps = new int[count];
                for (int i = 0; i < count; i++)
                {
                    scgetterps[i] = br.ReadInt32();
                }
                // properties
                count = br.ReadInt32();
                if (count > 0)
                {
                    int pcount = count << 1;
                    int[] props = new int[pcount];
                    for (int i = 0; i < pcount; i++)
                    {
                        props[i] = br.ReadInt32();
                    }
                    properties[o] = props;
                }
                //IntVector superpointer = IntVector.wrap( scgetterps );
                InterCodeObject obj = new InterCodeObject(block, mStringArray[name], contextType,
                    code, vdata, maxVariableCount, variableReserveCount, maxFrameCount, funcDeclArgCount
                    , funcDeclUnnamedArgArrayBase, funcDeclCollapseBase, true, srcpos, scgetterps);
                //objs.add(obj);
                objs[o] = obj;
            }
            Variant val = new Variant();
            for (int o = 0; o < objcount; o++)
            {
                InterCodeObject parentObj = null;
                InterCodeObject propSetterObj = null;
                InterCodeObject propGetterObj = null;
                InterCodeObject superClassGetterObj = null;
                if (parent[o] >= 0)
                {
                    parentObj = objs[parent[o]];
                }
                if (propSetter[o] >= 0)
                {
                    propSetterObj = objs[propSetter[o]];
                }
                if (propGetter[o] >= 0)
                {
                    propGetterObj = objs[propGetter[o]];
                }
                if (superClassGetter[o] >= 0)
                {
                    superClassGetterObj = objs[superClassGetter[o]];
                }
                objs[o]
                    .SetCodeObject(parentObj, propSetterObj, propGetterObj, superClassGetterObj
                    );
                if (properties[o] != null)
                {
                    InterCodeObject obj = parentObj;
                    // objs.get(o).mParent;
                    int[] prop = properties[o];
                    int length = (int) (((uint) prop.Length) >> 1);
                    for (int i = 0; i < length; i++)
                    {
                        int pos = i << 1;
                        int pname = prop[pos];
                        int pobj = prop[pos + 1];
                        val.Set(objs[pobj]);
                        obj.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP, mStringArray[pname], val
                            , obj);
                    }
                    properties[o] = null;
                }
            }
            for (int i = 0; i < work.Count; i++)
            {
                VariantRepalace w = work[i];
                w.Work.Set(objs[w.Index]);
            }
            work.Clear();
            InterCodeObject top = null;
            if (toplevel >= 0)
            {
                top = objs[toplevel];
            }
            block.SetObjects(top, objs, objcount);
        }

        private void ReadDataArea(BinaryReader br, int size)
        {
            //Load bytes
            int count = br.ReadInt32();
            if (mByteArray == null || mByteArray.Length < count)
            {
                int c = count < MIN_BYTE_COUNT ? MIN_BYTE_COUNT : count;
                mByteArray = new byte[c];
            }
            if (count > 0)
            {
                Array.Copy(br.ReadBytes(count), 0, mByteArray, 0, count);
                var padding = 4 - count % 4;
                if (padding > 0 && padding < 4)
                {
                    br.ReadBytes(padding);
                }
            }

            count = br.ReadInt32();
            // load short
            if (mShortArray == null || mShortArray.Length < count)
            {
                int c = count < MIN_SHORT_COUNT ? MIN_SHORT_COUNT : count;
                mShortArray = new short[c];
            }
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    mShortArray[i] = br.ReadInt16();
                }
                var padding = 4 - (count * 2) % 4;
                if (padding > 0 && padding < 4)
                {
                    br.ReadBytes(padding);
                }
            }

            count = br.ReadInt32();
            //Load int
            if (mIntArray == null || mIntArray.Length < count)
            {
                int c = count < MIN_INT_COUNT ? MIN_INT_COUNT : count;
                mIntArray = new int[c];
            }
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    mIntArray[i] = br.ReadInt32();
                }
            }

            count = br.ReadInt32();
            // load long
            if (mLongArray == null || mLongArray.Length < count)
            {
                int c = count < MIN_LONG_COUNT ? MIN_LONG_COUNT : count;
                mLongArray = new long[c];
            }
            if (count > 0)
            {

                for (int i = 0; i < count; i++)
                {
                    mLongArray[i] = br.ReadInt64();
                }
            }

            count = br.ReadInt32();
            // load double
            if (mDoubleArray == null || mDoubleArray.Length < count)
            {
                int c = count < MIN_DOUBLE_COUNT ? MIN_DOUBLE_COUNT : count;
                mDoubleArray = new double[c];
            }
            if (count > 0)
            {
                //TODO: Remove it
                if (mDoubleTmpArray == null || mDoubleTmpArray.Length < count)
                {
                    int c = count < MIN_DOUBLE_COUNT ? MIN_DOUBLE_COUNT : count;
                    mDoubleTmpArray = new long[c];
                }
                for (int i = 0; i < count; i++)
                {
                    mDoubleArray[i] = br.ReadDouble();
                }
            }

            count = br.ReadInt32();
            //load string
            if (mStringArray == null || mStringArray.Length < count)
            {
                int c = count < MIN_STRING_COUNT ? MIN_STRING_COUNT : count;
                mStringArray = new string[c];
            }
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int len = br.ReadInt32();
                    char[] ch = new char[len];
                    for (int j = 0; j < len; j++)
                    {
                        ch[j] = (char)br.ReadInt16();
                    }
                    mStringArray[i] = Tjs.MapGlobalStringMap(new string(ch));
                    var padding = 4 - (len * 2) % 4;
                    if (padding > 0 && padding < 4)
                    {
                        br.ReadBytes(padding);
                    }
                }
            }

            count = br.ReadInt32();
            //load bytebuffer
            if (mByteBufferArray == null || mByteBufferArray.Length < count)
            {
                mByteBufferArray = new ByteBuffer[count];
            }
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int len = br.ReadInt32();
                    byte[] tmp = new byte[len];
                    System.Array.Copy(br.ReadBytes(len), 0, tmp, 0, len);
                    mByteBufferArray[i] = ByteBuffer.Wrap(tmp);
                    mByteBufferArray[i].Position(len);
                    var padding = 4 - len % 4;
                    if (padding > 0 && padding < 4)
                    {
                        br.ReadBytes(padding);
                    }
                }
            }
        }
    }

    /// <summary>InterCodeObject へ置换するために一时的に觉えておくクラス</summary>
    internal class VariantRepalace
    {
        public Variant Work;

        public int Index;

        public VariantRepalace(Variant w, int i)
        {
            Work = w;
            Index = i;
        }
    }

    internal class ObjectsCache
    {
        public InterCodeObject[] mObjs;

        public AList<VariantRepalace> mWork;

        public int[] mParent;

        public int[] mPropSetter;

        public int[] mPropGetter;

        public int[] mSuperClassGetter;

        public int[][] mProperties;

        private const int MIN_COUNT = 500;

        // temporary
        //static private final int MIN_VARIANT_DATA_COUNT = 400*2;
        public virtual void Create(int count)
        {
            if (count < MIN_COUNT)
            {
                count = MIN_COUNT;
            }
            if (mWork == null)
            {
                mWork = new AList<VariantRepalace>();
            }
            mWork.Clear();
            if (mObjs == null || mObjs.Length < count)
            {
                mObjs = new InterCodeObject[count];
                mParent = new int[count];
                mPropSetter = new int[count];
                mPropGetter = new int[count];
                mSuperClassGetter = new int[count];
                mProperties = new int[count][];
            }
        }

        public virtual void Release()
        {
            mWork = null;
            mObjs = null;
            mParent = null;
            mPropSetter = null;
            mPropGetter = null;
            mSuperClassGetter = null;
            mProperties = null;
        }
    }
}
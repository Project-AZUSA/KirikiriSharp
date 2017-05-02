/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi
{
	public class CustomObject : Dispatch
	{
		private const bool AUTO_REBUILD_HASH = false;

		private const int AUTO_REBUILD_HASH_THRESHOLD = 2;

		private const int MAX_NATIVE_CLASS = 4;

		private const int OBJECT_HASH__BITS_LIMITS = 32;

		private const int NAMESPACE_DEFAULT_HASH_BITS = 3;

		private const int SYMBOL_USING = unchecked((int)(0x1));

		private const int SYMBOL_INIT = unchecked((int)(0x2));

		private const int SYMBOL_HIDDEN = unchecked((int)(0x8));

		private const int SYMBOL_STATIC = unchecked((int)(0x10));

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

		private const int OP_MASK = unchecked((int)(0x001f));

		private const int OP_MIN = OP_BAND;

		private const int OP_MAX = OP_DEC;

		private static string mFinalizeName;

		private static string mMissingName;

		private static int mGlobalRebuildHashMagic = 0;

		//private static final boolean LOGD = false;
		public static void DoRehash()
		{
			mGlobalRebuildHashMagic++;
		}

		public int mCount;

		public int mHashMask;

		public int mHashSize;

		public SymbolData[] mSymbols;

		public int mRebuildHashMagic;

		public bool mIsInvalidated;

		public bool mIsInvalidating;

		public NativeInstance[] mClassInstances;

		public int[] mClassIDs;

		private NativeInstance mPrimaryClassInstances;

		private int mPrimaryClassID;

		protected internal bool mCallFinalize;

		protected internal string mfinalize_name;

		protected internal bool mCallMissing;

		protected internal bool mProsessingMissing;

		protected internal string mmissing_name;

		protected internal AList<string> mClassNames;

		//[MAX_NATIVE_CLASS];
		// [MAX_NATIVE_CLASS];
		// set false if this object does not need to call "finalize"
		// name of the 'finalize' method
		// set true if this object should call 'missing' method
		// true if 'missing' method is being called
		// name of the 'missing' method
		public static void Initialize()
		{
			mFinalizeName = null;
			mMissingName = null;
			mGlobalRebuildHashMagic = 0;
		}

		public static void FinalizeApplication()
		{
			mFinalizeName = null;
			mMissingName = null;
		}

		protected internal bool GetValidity()
		{
			return !mIsInvalidated;
		}

		~CustomObject()
		{
            //try
            //{
            //    base.Finalize();
            //}
            //catch
            //{
            //}
			for (int i = MAX_NATIVE_CLASS - 1; i >= 0; i--)
			{
				if (mClassIDs[i] != -1)
				{
					if (mClassInstances[i] != null)
					{
						mClassInstances[i].Destruct();
					}
				}
			}
			//mSymbols = null;
		}

		//if(TJSObjectHashMapEnabled()) TJSRemoveObjectHashRecord(this);
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal virtual void FinalizeObject()
		{
			// call this object's "finalize"
			if (mCallFinalize && Tjs.IsTarminating == false)
			{
				//funcCall( 0, mfinalize_name, mfinalize_name.hashCode(), null, 0, null, this );
				FuncCall(0, mfinalize_name, null, Tjs.NULL_ARG, this);
			}
			for (int i = MAX_NATIVE_CLASS - 1; i >= 0; i--)
			{
				if (mClassIDs[i] != -1)
				{
					if (mClassInstances[i] != null)
					{
						mClassInstances[i].Invalidate();
					}
				}
			}
			mPrimaryClassInstances = null;
			DeleteAllMembers();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void FinalizeInternal()
		{
			if (mIsInvalidating)
			{
				return;
			}
			// to avoid re-entrance
			mIsInvalidating = true;
			try
			{
				if (!mIsInvalidated)
				{
					FinalizeObject();
					mIsInvalidated = true;
				}
			}
			finally
			{
				mIsInvalidating = false;
			}
		}

		public CustomObject() : this(NAMESPACE_DEFAULT_HASH_BITS)
		{
		}

		public CustomObject(int hashbits) : base()
		{
			// デバッグ系はなし
			// if(TJSObjectHashMapEnabled()) TJSAddObjectHashRecord(this);
			mRebuildHashMagic = mGlobalRebuildHashMagic;
			if (hashbits > OBJECT_HASH__BITS_LIMITS)
			{
				hashbits = OBJECT_HASH__BITS_LIMITS;
			}
			mHashSize = (1 << hashbits);
			mHashMask = mHashSize - 1;
			mSymbols = new SymbolData[mHashSize];
			for (int i = 0; i < mHashSize; i++)
			{
				mSymbols[i] = new SymbolData();
			}
			//mIsInvalidated = false;
			//mIsInvalidating = false;
			mCallFinalize = true;
			//mCallMissing = false;
			//mProsessingMissing = false;
			if (mFinalizeName == null)
			{
				// first time; initialize 'finalize' name and 'missing' name
				mFinalizeName = Tjs.MapGlobalStringMap("finalize");
				mMissingName = Tjs.MapGlobalStringMap("missing");
			}
			mfinalize_name = mFinalizeName;
			mmissing_name = mMissingName;
			mClassInstances = new NativeInstance[MAX_NATIVE_CLASS];
			mClassIDs = new int[MAX_NATIVE_CLASS];
			for (int i_1 = 0; i_1 < MAX_NATIVE_CLASS; i_1++)
			{
				mClassIDs[i_1] = -1;
			}
			mClassNames = new AList<string>();
			//mPrimaryClassInstances;
			mPrimaryClassID = -1;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal override void BeforeDestruction()
		{
			// デバッグ系はなし
			// if(TJSObjectHashMapEnabled()) TJSSetObjectHashFlag(this, TJS_OHMF_DELETING, TJS_OHMF_SET);
			FinalizeInternal();
		}

		//super.beforeDestruction();
		//private void checkObjectClosureAdd( final Variant val ) {
		// adjust the reference counter when the object closure's "objthis" is
		// referring to the object itself.
		//}
		//private void checkObjectClosureRemove( final Variant val ) {
		//}
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private bool CallGetMissing(string name, Variant result)
		{
			// call 'missing' method for PopGet
			if (mProsessingMissing)
			{
				return false;
			}
			mProsessingMissing = true;
			bool res = false;
			try
			{
				Variant val = new Variant();
				SimpleGetSetProperty prop = new SimpleGetSetProperty(val);
				try
				{
					Variant[] args = new Variant[3];
					args[0] = new Variant(0);
					// false: get
					args[1] = new Variant(name);
					// member name
					args[2] = new Variant(prop);
					//tTJSVariant *pargs[3] = {args +0, args +1, args +2};
					Variant funcresult = new Variant();
					int er = FuncCall(0, mmissing_name, funcresult, args, this);
					if (er < 0)
					{
						res = false;
					}
					else
					{
						res = funcresult.AsInteger() != 0;
						result.Set(val);
					}
				}
				finally
				{
				}
			}
			finally
			{
				// prop.Release();
				mProsessingMissing = false;
			}
			return res;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private bool CallSetMissing(string name, Variant value)
		{
			// call 'missing' method for PopSet
			if (mProsessingMissing)
			{
				return false;
			}
			mProsessingMissing = true;
			bool res = false;
			try
			{
				Variant val = new Variant(value);
				SimpleGetSetProperty prop = new SimpleGetSetProperty(val);
				try
				{
					Variant[] args = new Variant[3];
					args[0] = new Variant(1);
					// true: set
					args[1] = new Variant(name);
					// member name
					args[2] = new Variant(prop);
					//tTJSVariant *pargs[3] = {args +0, args +1, args +2};
					Variant funcresult = new Variant();
					int er = FuncCall(0, mmissing_name, funcresult, args, this);
					if (er < 0)
					{
						res = false;
					}
					else
					{
						res = funcresult.AsInteger() != 0;
					}
				}
				finally
				{
				}
			}
			finally
			{
				//prop.Release();
				mProsessingMissing = false;
			}
			return res;
		}

		// Adds the symbol, returns the newly created data;
		// if already exists, returns the data.
		/// <exception cref="TjsException"></exception>
		private SymbolData Add(string name)
		{
			// add a data element named "name".
			// return existing element if the element named "name" is already alive.
			if (name == null)
			{
				return null;
			}
			SymbolData data;
			data = Find(name);
			if (data != null)
			{
				// the element is already alive
				return data;
			}
			int hash = name.GetHashCode();
			int pos = (hash & mHashMask);
			//if( LOGD ) Logger.log("Symbol Pos:"+pos);
			SymbolData lv1 = mSymbols[pos];
			if ((lv1.mSymFlags & SYMBOL_USING) != 0)
			{
				// lv1 is using
				// make a chain and insert it after lv1
				data = new SymbolData();
				data.SelfClear();
				data.mNext = lv1.mNext;
				lv1.mNext = data;
				data.SetName(name, hash);
				data.mSymFlags |= SYMBOL_USING;
			}
			else
			{
				// lv1 is unused
				if ((lv1.mSymFlags & SYMBOL_INIT) == 0)
				{
					lv1.SelfClear();
				}
				lv1.SetName(name, hash);
				lv1.mSymFlags |= SYMBOL_USING;
				data = lv1;
			}
			mCount++;
			return data;
		}

		/// <exception cref="TjsException"></exception>
		private SymbolData AddTo(string name, SymbolData[] newdata, int newhashmask)
		{
			// similar to Add, except for adding member to new hash space.
			if (name == null)
			{
				return null;
			}
			// at this point, the member must not exist in destination hash space
			int hash;
			hash = name.GetHashCode();
			SymbolData lv1 = newdata[hash & newhashmask];
			SymbolData data;
			if ((lv1.mSymFlags & SYMBOL_USING) != 0)
			{
				// lv1 is using
				// make a chain and insert it after lv1
				data = new SymbolData();
				data.SelfClear();
				data.mNext = lv1.mNext;
				lv1.mNext = data;
				data.SetName(name, hash);
				data.mSymFlags |= SYMBOL_USING;
			}
			else
			{
				// lv1 is unused
				if ((lv1.mSymFlags & SYMBOL_INIT) == 0)
				{
					lv1.SelfClear();
				}
				lv1.SetName(name, hash);
				lv1.mSymFlags |= SYMBOL_USING;
				data = lv1;
			}
			// count is not incremented
			return data;
		}

		/// <exception cref="TjsException"></exception>
		private void RebuildHash()
		{
			// rebuild hash table
			mRebuildHashMagic = mGlobalRebuildHashMagic;
			// decide new hash table size
			int r;
			int v = mCount;
			if ((v & unchecked((int)(0xffff0000))) != 0)
			{
				r = 16;
				v >>= 16;
			}
			else
			{
				r = 0;
			}
			if ((v & unchecked((int)(0xff00))) != 0)
			{
				r += 8;
				v >>= 8;
			}
			if ((v & unchecked((int)(0xf0))) != 0)
			{
				r += 4;
				v >>= 4;
			}
			v <<= 1;
			int newhashbits = r + ((unchecked((int)(0xffffaa50)) >> v) & unchecked((int)(0x03
				))) + 2;
			if (newhashbits > OBJECT_HASH__BITS_LIMITS)
			{
				newhashbits = OBJECT_HASH__BITS_LIMITS;
			}
			int newhashsize = (1 << newhashbits);
			if (newhashsize == mHashSize)
			{
				return;
			}
			int newhashmask = newhashsize - 1;
			int orgcount = mCount;
			// allocate new hash space
			SymbolData[] newsymbols = new SymbolData[newhashsize];
			for (int i = 0; i < newhashsize; i++)
			{
				newsymbols[i] = new SymbolData();
			}
			// enumerate current symbol and push to new hash space
			try
			{
				//memset(newsymbols, 0, sizeof(tTJSSymbolData) * newhashsize);
				//int i;
				//SymbolData lv1 = mSymbols[0];
				//SymbolData lv1lim = mSymbols[mHashSize]; // 末尾か、iterator のように处理してるんだな
				for (int i_1 = 0; i_1 < mHashSize; i_1++)
				{
					//for( ; lv1 < lv1lim; lv1++ ) {
					SymbolData lv1 = mSymbols[i_1];
					SymbolData d = lv1.mNext;
					while (d != null)
					{
						SymbolData nextd = d.mNext;
						if ((d.mSymFlags & SYMBOL_USING) != 0)
						{
							//						d->ReShare();
							SymbolData data = AddTo(d.mName, newsymbols, newhashmask);
							if (data != null)
							{
								//data.mValue = d.mValue;
								data.mValue.CopyRef(d.mValue);
								//GetValue(data).CopyRef(*(tTJSVariant*)(&(d->Value)));
								data.mSymFlags &= ~(SYMBOL_HIDDEN | SYMBOL_STATIC);
								data.mSymFlags |= d.mSymFlags & (SYMBOL_HIDDEN | SYMBOL_STATIC);
							}
						}
						//checkObjectClosureAdd( (Variant)data.mValue );
						d = nextd;
					}
					if ((lv1.mSymFlags & SYMBOL_USING) != 0)
					{
						//					lv1->ReShare();
						SymbolData data = AddTo(lv1.mName, newsymbols, newhashmask);
						if (data != null)
						{
							//data.mValue = lv1.mValue;
							data.mValue.CopyRef(lv1.mValue);
							//GetValue(data).CopyRef(*(tTJSVariant*)(&(lv1->Value)));
							data.mSymFlags &= ~(SYMBOL_HIDDEN | SYMBOL_STATIC);
							data.mSymFlags |= lv1.mSymFlags & (SYMBOL_HIDDEN | SYMBOL_STATIC);
						}
					}
				}
			}
			catch (TjsException)
			{
				//checkObjectClosureAdd( (Variant)data.mValue );
				// recover
				int _HashMask = mHashMask;
				int _HashSize = mHashSize;
				SymbolData[] _Symbols = mSymbols;
				mSymbols = newsymbols;
				mHashSize = newhashsize;
				mHashMask = newhashmask;
				DeleteAllMembers();
				mSymbols = null;
				mHashMask = _HashMask;
				mHashSize = _HashSize;
				mSymbols = _Symbols;
				mCount = orgcount;
				throw;
			}
			// delete all current members
			DeleteAllMembers();
			mSymbols = null;
			// assign new members
			mSymbols = newsymbols;
			mHashSize = newhashsize;
			mHashMask = newhashmask;
			mCount = orgcount;
		}

		private bool DeleteByName(string name)
		{
			// find an element named "name" and deletes it
			int hash = name.GetHashCode();
			SymbolData lv1 = mSymbols[hash & mHashMask];
			if ((lv1.mSymFlags & SYMBOL_USING) == 0 && lv1.mNext == null)
			{
				return false;
			}
			// not found
			if ((lv1.mSymFlags & SYMBOL_USING) != 0 && lv1.NameMatch(name))
			{
				// mark the element place as "unused"
				//checkObjectClosureRemove( (Variant)lv1.mValue );
				lv1.PostClear();
				mCount--;
				return true;
			}
			// chain processing
			SymbolData d = lv1.mNext;
			SymbolData prevd = lv1;
			while (d != null)
			{
				if ((d.mSymFlags & SYMBOL_USING) != 0 && d.mHash == hash)
				{
					if (d.NameMatch(name))
					{
						// sever from the chain
						prevd.mNext = d.mNext;
						//checkObjectClosureRemove( (Variant)d.mValue );
						d.Destory();
						d = null;
						mCount--;
						return true;
					}
				}
				prevd = d;
				d = d.mNext;
			}
			return false;
		}

		private void DeleteAllMembers()
		{
            if (mSymbols == null)
            {
                return;
            }
			// delete all members
			//Vector<Dispatch2> vector = new Vector<Dispatch2>();
			//try {
			SymbolData lv1;
			//, lv1lim;
			// list all members up that hold object
			for (int i = 0; i < mHashSize; i++)
			{
				lv1 = mSymbols[i];
				SymbolData d = lv1.mNext;
				while (d != null)
				{
					SymbolData nextd = d.mNext;
					if ((d.mSymFlags & SYMBOL_USING) != 0)
					{
						Variant val = (Variant)d.mValue;
						if (val.IsObject())
						{
							//checkObjectClosureRemove( val );
							//VariantClosure clo = val.asObjectClosure();
							//if( clo.mObject != null ) vector.add( clo.mObject );
							//if( clo.mObjThis != null ) vector.add( clo.mObjThis );
							val.Clear();
						}
					}
					d = nextd;
				}
				if ((lv1.mSymFlags & SYMBOL_USING) != 0)
				{
					Variant val = (Variant)lv1.mValue;
					if (val.IsObject())
					{
						//checkObjectClosureRemove( val );
						//VariantClosure clo = val.asObjectClosure();
						//if( clo.mObject != null ) vector.add( clo.mObject );
						//if( clo.mObjThis != null ) vector.add( clo.mObjThis );
						val.Clear();
					}
				}
			}
			// delete all members
			for (int i_1 = 0; i_1 < mHashSize; i_1++)
			{
				lv1 = mSymbols[i_1];
				SymbolData d = lv1.mNext;
				while (d != null)
				{
					SymbolData nextd = d.mNext;
					if ((d.mSymFlags & SYMBOL_USING) != 0)
					{
						d.Destory();
					}
					d = null;
					d = nextd;
				}
				if ((lv1.mSymFlags & SYMBOL_USING) != 0)
				{
					lv1.PostClear();
				}
				lv1.mNext = null;
			}
			mCount = 0;

		    mSymbols = null;
		}

		//} finally {}
		// release all objects
		private SymbolData Find(string name)
		{
			// searche an element named "name" and return its "SymbolData".
			// return NULL if the element is not found.
			if (name == null)
			{
				return null;
			}
			int hash = name.GetHashCode();
			int pos = (hash & mHashMask);
			// if( LOGD ) Logger.log("Symbol Pos:"+pos);
			SymbolData lv1 = mSymbols[pos];
			if ((lv1.mSymFlags & SYMBOL_USING) == 0 && lv1.mNext == null)
			{
				return null;
			}
			// lv1 is unused and does not have any chains
			// search over the chain
			int cnt = 0;
			SymbolData prevd = lv1;
			SymbolData d = lv1.mNext;
			for (; d != null; prevd = d, d = d.mNext, cnt++)
			{
				if (d.mHash == hash && (d.mSymFlags & SYMBOL_USING) != 0)
				{
					if (d.NameMatch(name))
					{
						if (cnt > 2)
						{
							// move to first
							prevd.mNext = d.mNext;
							d.mNext = lv1.mNext;
							lv1.mNext = d;
						}
						return d;
					}
				}
			}
			if (lv1.mHash == hash && (lv1.mSymFlags & SYMBOL_USING) != 0)
			{
				if (lv1.NameMatch(name))
				{
					return lv1;
				}
			}
			return null;
		}

		/// <exception cref="TjsException"></exception>
		private static bool EnumCallback(int flags, EnumMembersCallback callback, Variant
			 value, Dispatch2 objthis, SymbolData data)
		{
			int newflags = 0;
			if ((data.mSymFlags & SYMBOL_HIDDEN) != 0)
			{
				newflags |= Interface.HIDDENMEMBER;
			}
			if ((data.mSymFlags & SYMBOL_STATIC) != 0)
			{
				newflags |= Interface.STATICMEMBER;
			}
			value.Clear();
			if ((flags & Interface.ENUM_NO_VALUE) == 0)
			{
				bool getvalues = false;
				if ((flags & Interface.IGNOREPROP) == 0)
				{
					Variant targ = data.mValue;
					if (targ.IsObject())
					{
						VariantClosure tvclosure = targ.AsObjectClosure();
						int hr = Error.E_NOTIMPL;
						if (tvclosure.mObject != null)
						{
							Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
							hr = tvclosure.mObject.PropGet(0, null, value, disp);
						}
						if (hr >= 0)
						{
							getvalues = true;
						}
						else
						{
							if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
							{
								return false;
							}
						}
					}
				}
				if (getvalues == false)
				{
					value.CopyRef(data.mValue);
				}
			}
			return callback.Callback(data.mName, newflags, value);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int EnumMembers(int flags, EnumMembersCallback callback, Dispatch2
			 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (callback == null)
			{
				return Error.S_OK;
			}
			Variant value = new Variant();
			for (int i = 0; i < mHashSize; i++)
			{
				SymbolData lv1 = mSymbols[i];
				SymbolData d = lv1.mNext;
				while (d != null)
				{
					SymbolData nextd = d.mNext;
					if ((d.mSymFlags & SYMBOL_USING) != 0)
					{
						if (EnumCallback(flags, callback, value, objthis, d) == false)
						{
							return Error.S_OK;
						}
					}
					d = nextd;
				}
				if ((lv1.mSymFlags & SYMBOL_USING) != 0)
				{
					if (EnumCallback(flags, callback, value, objthis, lv1) == false)
					{
						return Error.S_OK;
					}
				}
			}
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultPropGet(int flag, Variant targ, Variant result
			, Dispatch2 objthis)
		{
			if ((flag & Interface.IGNOREPROP) == 0)
			{
				// if IGNOREPROP is not specified
				// if member's type is tvtObject, call the object's PropGet with "member=NULL"
				//  ( default member invocation ). if it is succeeded, return its return value.
				// if the PropGet's return value is TJS_E_ACCESSDENYED,
				// return as an error, otherwise return the member itself.
				if (targ.IsObject())
				{
					VariantClosure tvclosure = targ.AsObjectClosure();
					int hr = Error.E_NOTIMPL;
					if (tvclosure.mObject != null)
					{
						Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
						hr = tvclosure.mObject.PropGet(0, null, result, disp);
					}
					if (hr >= 0)
					{
						return hr;
					}
					if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
					{
						return hr;
					}
				}
			}
			// return the member itself
			if (result == null)
			{
				return Error.E_INVALIDPARAM;
			}
			result.CopyRef(targ);
			return Error.S_OK;
		}

		/// <summary>new する时のメンバコピー</summary>
		/// <param name="dest">コピー先</param>
		/// <returns>エラーコード</returns>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="TjsException"></exception>
		protected internal virtual int CopyAllMembers(CustomObject dest)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (!dest.GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			Variant result = new Variant();
			for (int i = 0; i < mHashSize; i++)
			{
				SymbolData lv1 = mSymbols[i];
				SymbolData d = lv1.mNext;
				while (d != null)
				{
					SymbolData nextd = d.mNext;
					if (((d.mSymFlags & SYMBOL_USING) != 0) && ((d.mSymFlags & SYMBOL_STATIC) == 0))
					{
						if (d.mValue.IsObject())
						{
							result.Set(d.mValue);
							if (result.AsObjectThis() == null)
							{
								result.ChangeClosureObjThis(dest);
							}
							SymbolData data = dest.Add(d.mName);
							if ((d.mSymFlags & SYMBOL_HIDDEN) != 0)
							{
								data.mSymFlags |= SYMBOL_HIDDEN;
							}
							else
							{
								data.mSymFlags &= ~SYMBOL_HIDDEN;
							}
							data.mValue.CopyRef(result);
						}
					}
					d = nextd;
				}
				if (((lv1.mSymFlags & SYMBOL_USING) != 0) && ((lv1.mSymFlags & SYMBOL_STATIC) == 
					0))
				{
					if (lv1.mValue.IsObject())
					{
						result.Set(lv1.mValue);
						if (result.AsObjectThis() == null)
						{
							result.ChangeClosureObjThis(dest);
						}
						SymbolData data = dest.Add(lv1.mName);
						if ((lv1.mSymFlags & SYMBOL_HIDDEN) != 0)
						{
							data.mSymFlags |= SYMBOL_HIDDEN;
						}
						else
						{
							data.mSymFlags &= ~SYMBOL_HIDDEN;
						}
						data.mValue.CopyRef(result);
					}
				}
			}
			return Error.S_OK;
		}

		public virtual void Clear()
		{
			DeleteAllMembers();
		}

		// service function for lexical analyzer
		/// <exception cref="VariantException"></exception>
		public virtual int GetValueInteger(string name)
		{
			SymbolData data = Find(name);
			if (data == null)
			{
				return -1;
			}
			Variant val = (Variant)data.mValue;
			return val.AsInteger();
		}

		// オリジナルでは、强制的に int で返す(アドレスになるかもしれない)もののようだが……
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private static int TryFuncCallViaPropGet(VariantClosure tvclosure, int flag, Variant
			 result, Variant[] param, Dispatch2 objthis)
		{
			// retry using PropGet
			Variant tmp = new Variant();
			Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
			int er = tvclosure.mObject.PropGet(0, null, tmp, disp);
			if (er >= 0)
			{
				tvclosure = tmp.AsObjectClosure();
				disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
				er = tvclosure.mObject.FuncCall(flag, null, result, param, disp);
			}
			return er;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultFuncCall(int flag, Variant targ, Variant result
			, Variant[] param, Dispatch2 objthis)
		{
			if (targ.IsObject())
			{
				int er = Error.E_INVALIDTYPE;
				VariantClosure tvclosure = targ.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					// bypass
					Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					er = tvclosure.mObject.FuncCall(flag, null, result, param, disp);
					if (er == Error.E_INVALIDTYPE)
					{
						// retry using PropGet
						er = TryFuncCallViaPropGet(tvclosure, flag, result, param, objthis);
					}
				}
				return er;
			}
			return Error.E_INVALIDTYPE;
		}

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
				// this function is called as to call a default method,
				// but this object is not a function.
				return Error.E_INVALIDTYPE;
			}
			// so returns TJS_E_INVALIDTYPE
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value_func = new Variant();
					if (CallGetMissing(membername, value_func))
					{
						return DefaultFuncCall(flag, value_func, result, param, objthis);
					}
				}
				return Error.E_MEMBERNOTFOUND;
			}
			// member not found
			return DefaultFuncCall(flag, data.mValue, result, param, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropGet(int flag, string membername, Variant result, Dispatch2
			 objthis)
		{
			if (mRebuildHashMagic != mGlobalRebuildHashMagic)
			{
				RebuildHash();
			}
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (membername == null)
			{
				// this object itself has no information on PropGet with membername == NULL
				return Error.E_INVALIDTYPE;
			}
			SymbolData data;
			data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value = new Variant();
					if (CallGetMissing(membername, value))
					{
						return DefaultPropGet(flag, value, result, objthis);
					}
				}
			}
			if (data == null && (flag & Interface.MEMBERENSURE) != 0)
			{
				// create a member when TJS_MEMBERENSURE is specified
				data = Add(membername);
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			//mLastPropGetSymbol = data;
			//mLastPropGetName = membername;
			return DefaultPropGet(flag, data.mValue, result, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultPropSet(int flag, Variant targ, Variant param
			, Dispatch2 objthis)
		{
			if ((flag & Interface.IGNOREPROP) == 0)
			{
				if (targ.IsObject())
				{
					// roughly the same as TJSDefaultPropGet
					VariantClosure tvclosure = targ.AsObjectClosure();
					int hr = Error.E_NOTIMPL;
					if (tvclosure.mObject != null)
					{
						Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
						hr = tvclosure.mObject.PropSet(0, null, param, disp);
					}
					if (hr >= 0)
					{
						return hr;
					}
					if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
					{
						return hr;
					}
				}
			}
			// normal substitution
			if (param == null)
			{
				return Error.E_INVALIDPARAM;
			}
			targ.CopyRef(param);
			return Error.S_OK;
		}

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
				// no action is defined with the default member
				return Error.E_INVALIDTYPE;
			}
			SymbolData data = null;
			if (mCallMissing)
			{
				data = Find(membername);
				if (data != null)
				{
					// call 'missing' method
					if (CallSetMissing(membername, param))
					{
						return Error.S_OK;
					}
				}
			}
			if ((flag & Interface.MEMBERENSURE) != 0)
			{
				data = Add(membername);
			}
			else
			{
				// create a member when MEMBERENSURE is specified
				data = Find(membername);
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			if ((flag & Interface.HIDDENMEMBER) != 0)
			{
				data.mSymFlags |= SYMBOL_HIDDEN;
			}
			else
			{
				data.mSymFlags &= ~SYMBOL_HIDDEN;
			}
			if ((flag & Interface.STATICMEMBER) != 0)
			{
				data.mSymFlags |= SYMBOL_STATIC;
			}
			else
			{
				data.mSymFlags &= ~SYMBOL_STATIC;
			}
			//-- below is mainly the same as defaultPropSet
			if ((flag & Interface.IGNOREPROP) == 0)
			{
				if (data.mValue.IsObject())
				{
					VariantClosure tvclosure = data.mValue.AsObjectClosure();
					if (tvclosure.mObject != null)
					{
						Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
						int hr = tvclosure.mObject.PropSet(0, null, param, disp);
						if (hr >= 0)
						{
							return hr;
						}
						if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
						{
							return hr;
						}
					}
					data = Find(membername);
				}
			}
			if (param == null)
			{
				return Error.E_INVALIDPARAM;
			}
			//mLastPropSetName = membername;
			//mLastPropSetSymbol = data;
			data.mValue.CopyRef(param);
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int GetCount(IntWrapper result, string membername, Dispatch2 objthis
			)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (result == null)
			{
				return Error.E_INVALIDPARAM;
			}
			result.value = mCount;
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int DeleteMember(int flag, string membername, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (membername == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			if (!DeleteByName(membername))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultInvalidate(int flag, Variant targ, Dispatch2
			 objthis)
		{
			if (targ.IsObject())
			{
				VariantClosure tvclosure = targ.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					// bypass
					Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					return tvclosure.mObject.Invalidate(flag, null, disp);
				}
			}
			return Error.S_FALSE;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int Invalidate(int flag, string membername, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (membername == null)
			{
				if (mIsInvalidated)
				{
					return Error.S_FALSE;
				}
				FinalizeInternal();
				return Error.S_TRUE;
			}
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value = new Variant();
					if (CallGetMissing(membername, value))
					{
						return DefaultInvalidate(flag, value, objthis);
					}
				}
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			return DefaultInvalidate(flag, data.mValue, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultIsValid(int flag, Variant targ, Dispatch2 objthis
			)
		{
			if (targ.IsObject())
			{
				VariantClosure tvclosure = targ.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					// bypass
					Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					return tvclosure.mObject.IsValid(flag, null, disp);
				}
			}
			// the target type is not tvtObject
			return Error.S_TRUE;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsValid(int flag, string membername, Dispatch2 objthis)
		{
			if (membername == null)
			{
				if (mIsInvalidated)
				{
					return Error.S_FALSE;
				}
				return Error.S_TRUE;
			}
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value = new Variant();
					if (CallGetMissing(membername, value))
					{
						return DefaultIsValid(flag, value, objthis);
					}
				}
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			return DefaultIsValid(flag, data.mValue, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultCreateNew(int flag, Variant targ, Holder<Dispatch2
			> result, Variant[] param, Dispatch2 objthis)
		{
			if (targ.IsObject())
			{
				VariantClosure tvclosure = targ.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					// bypass
					Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					return tvclosure.mObject.CreateNew(flag, null, result, param, disp);
				}
			}
			return Error.E_INVALIDTYPE;
		}

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
				// as an action of the default member, this object cannot create an object
				// because this object is not a class
				return Error.E_INVALIDTYPE;
			}
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value = new Variant();
					if (CallGetMissing(membername, value))
					{
						return DefaultCreateNew(flag, value, result, param, objthis);
					}
				}
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			return DefaultCreateNew(flag, data.mValue, result, param, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public static int DefaultIsInstanceOf(int flag, Variant targ, string name, Dispatch2
			 objthis)
		{
			if (targ.IsVoid())
			{
				return Error.S_FALSE;
			}
			if ("Object".Equals(name))
			{
				return Error.S_TRUE;
			}
			if (targ.IsNumber())
			{
				if ("Number".Equals(name))
				{
					return Error.S_TRUE;
				}
				else
				{
					return Error.S_FALSE;
				}
			}
			else
			{
				if (targ.IsString())
				{
					if ("String".Equals(name))
					{
						return Error.S_TRUE;
					}
					else
					{
						return Error.S_FALSE;
					}
				}
				else
				{
					if (targ.IsOctet())
					{
						if ("Octet".Equals(name))
						{
							return Error.S_TRUE;
						}
						else
						{
							return Error.S_FALSE;
						}
					}
					else
					{
						if (targ.IsObject())
						{
							VariantClosure tvclosure = targ.AsObjectClosure();
							if (tvclosure.mObject != null)
							{
								// bypass
								Dispatch2 disp = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
								return tvclosure.mObject.IsInstanceOf(flag, null, name, disp);
							}
							return Error.S_FALSE;
						}
					}
				}
			}
			return Error.S_FALSE;
		}

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
				// always returns true if "Object" is specified
				if ("Object".Equals(classname))
				{
					return Error.S_TRUE;
				}
				// look for the class instance information
				int count = mClassNames.Count;
				for (int i = 0; i < count; i++)
				{
					if (mClassNames[i].Equals(classname))
					{
						return Error.S_TRUE;
					}
				}
				return Error.S_FALSE;
			}
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call 'missing' method
					Variant value = new Variant();
					if (CallGetMissing(membername, value))
					{
						return DefaultIsInstanceOf(flag, value, classname, objthis);
					}
				}
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			return DefaultIsInstanceOf(flag, data.mValue, classname, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static int DefaultOperation(int flag, Variant targ, Variant result
			, Variant param, Dispatch2 objthis)
		{
			int op = flag & OP_MASK;
			if (op != OP_INC && op != OP_DEC && param == null)
			{
				return Error.E_INVALIDPARAM;
			}
			if (op < OP_MIN || op > OP_MAX)
			{
				return Error.E_INVALIDPARAM;
			}
			if (targ.IsObject())
			{
				// the member may be a property handler if the member's type is "tvtObject"
				// so here try to access the object.
				int hr;
				VariantClosure tvclosure = targ.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					Dispatch2 ot = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					Variant tmp = new Variant();
					hr = tvclosure.mObject.PropGet(0, null, tmp, ot);
					if (hr >= 0)
					{
						DoVariantOperation(op, tmp, param);
						hr = tvclosure.mObject.PropSet(0, null, tmp, ot);
						if (hr < 0)
						{
							return hr;
						}
						if (result != null)
						{
							result.CopyRef(tmp);
						}
						return Error.S_OK;
					}
					else
					{
						if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
						{
							return hr;
						}
					}
				}
			}
			// normal operation is proceeded if "PropGet" is failed.
			DoVariantOperation(op, targ, param);
			if (result != null)
			{
				result.CopyRef(targ);
			}
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int Operation(int flag, string membername, Variant result, Variant
			 param, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			// operation about the member
			// processing line is the same as above function
			if (membername == null)
			{
				return Error.E_INVALIDTYPE;
			}
			int op = flag & OP_MASK;
			if (op != OP_INC && op != OP_DEC && param == null)
			{
				return Error.E_INVALIDPARAM;
			}
			if (op < OP_MIN || op > OP_MAX)
			{
				return Error.E_INVALIDPARAM;
			}
			SymbolData data = Find(membername);
			if (data == null)
			{
				if (mCallMissing)
				{
					// call default operation
					return base.Operation(flag, membername, result, param, objthis);
				}
			}
			if (data == null)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			// not found
			if (data.mValue.IsObject())
			{
				int hr;
				VariantClosure tvclosure;
				tvclosure = data.mValue.AsObjectClosure();
				if (tvclosure.mObject != null)
				{
					Dispatch2 ot = tvclosure.mObjThis != null ? tvclosure.mObjThis : objthis;
					Variant tmp = new Variant();
					hr = tvclosure.mObject.PropGet(0, null, tmp, ot);
					if (hr >= 0)
					{
						DoVariantOperation(op, tmp, param);
						hr = tvclosure.mObject.PropSet(0, null, tmp, ot);
						if (hr < 0)
						{
							return hr;
						}
						if (result != null)
						{
							result.CopyRef(tmp);
						}
						return Error.S_OK;
					}
					else
					{
						if (hr != Error.E_NOTIMPL && hr != Error.E_INVALIDTYPE && hr != Error.E_INVALIDOBJECT)
						{
							return hr;
						}
					}
				}
			}
			//checkObjectClosureRemove( data.mValue );
			Variant tmp_1 = data.mValue;
			try
			{
				DoVariantOperation(op, tmp_1, param);
			}
			finally
			{
			}
			//checkObjectClosureAdd( data.mValue );
			if (result != null)
			{
				result.CopyRef(tmp_1);
			}
			return Error.S_OK;
		}

		// Dispatch クラスのメソッドを呼び出すため
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int DispatchOperation(int flag, string membername, Variant result, 
			Variant param, Dispatch2 objthis)
		{
			return base.Operation(flag, membername, result, param, objthis);
		}

		public override int NativeInstanceSupport(int flag, int classid, Holder<NativeInstance
			> pointer)
		{
			if (flag == Interface.NIS_GETINSTANCE)
			{
				// search "classid"
				for (int i = 0; i < MAX_NATIVE_CLASS; i++)
				{
					if (mClassIDs[i] == classid)
					{
						pointer.mValue = mClassInstances[i];
						return Error.S_OK;
					}
				}
				return Error.E_FAIL;
			}
			else
			{
				if (flag == Interface.NIS_REGISTER)
				{
					// search for the empty place
					if (mPrimaryClassID == -1)
					{
						mPrimaryClassID = classid;
						mPrimaryClassInstances = pointer.mValue;
					}
					for (int i = 0; i < MAX_NATIVE_CLASS; i++)
					{
						if (mClassIDs[i] == -1)
						{
							// found... writes there
							mClassIDs[i] = classid;
							mClassInstances[i] = pointer.mValue;
							return Error.S_OK;
						}
					}
					return Error.E_FAIL;
				}
			}
			return Error.E_NOTIMPL;
		}

		public override int AddClassInstanveInfo(string name)
		{
			mClassNames.AddItem(name);
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		public override int ClassInstanceInfo(int flag, int num, Variant value)
		{
			switch (flag)
			{
				case Interface.CII_ADD:
				{
					// add value
					string name = value.AsString();
					// デバッグ系はなし
					//if( objectHashMapEnabled() && mClassNames.size() == 0)
					//	objectHashSetType( this, "instance of class " + name );
					// First class name is used for the object classname
					// because the order of the class name
					// registration is from descendant to ancestor.
					mClassNames.AddItem(name);
					return Error.S_OK;
				}

				case Interface.CII_GET:
				{
					// get value
					if (num >= mClassNames.Count)
					{
						return Error.E_FAIL;
					}
					value.Set(mClassNames[num]);
					return Error.S_OK;
				}

				case Interface.CII_SET_FINALIZE:
				{
					// set 'finalize' method name
					mfinalize_name = value.AsString();
					mCallFinalize = mfinalize_name.Length > 0;
					return Error.S_OK;
				}

				case Interface.CII_SET_MISSING:
				{
					// set 'missing' method name
					mmissing_name = value.AsString();
					mCallMissing = mmissing_name.Length > 0;
					return Error.S_OK;
				}
			}
			return Error.E_NOTIMPL;
		}

		// special funcsion
		public override NativeInstance GetNativeInstance(int classid)
		{
			if (mPrimaryClassID == classid)
			{
				return mPrimaryClassInstances;
			}
			else
			{
				for (int i = 0; i < MAX_NATIVE_CLASS; i++)
				{
					if (mClassIDs[i] == classid)
					{
						return mClassInstances[i];
					}
				}
				return null;
			}
		}

		public override int SetNativeInstance(int classid, NativeInstance ni)
		{
			// search for the empty place
			if (mPrimaryClassID == -1)
			{
				mPrimaryClassID = classid;
				mPrimaryClassInstances = ni;
			}
			for (int i = 0; i < MAX_NATIVE_CLASS; i++)
			{
				if (mClassIDs[i] == -1)
				{
					// found... writes there
					mClassIDs[i] = classid;
					mClassInstances[i] = ni;
					return Error.S_OK;
				}
			}
			return Error.E_FAIL;
		}

		public string GetClassNames()
		{
			if (mClassNames != null && mClassNames.Count > 0)
			{
				StringBuilder builder = new StringBuilder(512);
				int count = mClassNames.Count;
				for (int i = 0; i < count; i++)
				{
					if (i != 0)
					{
						builder.Append(',');
					}
					builder.Append(mClassNames[i]);
				}
				return builder.ToString();
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(1024);
			for (int i = 0; i < mHashSize; i++)
			{
				SymbolData lv1 = mSymbols[i];
				SymbolData d = lv1.mNext;
				while (d != null)
				{
					SymbolData nextd = d.mNext;
					if ((d.mSymFlags & SYMBOL_USING) != 0)
					{
						builder.Append(d.mName);
						builder.Append(" : ");
						builder.Append(d.mValue.ToString());
						builder.Append(", ");
					}
					d = nextd;
				}
				if ((lv1.mSymFlags & SYMBOL_USING) != 0)
				{
					builder.Append(lv1.mName);
					builder.Append(" : ");
					builder.Append(lv1.mValue.ToString());
					builder.Append(", ");
				}
			}
			if (builder.Length == 0)
			{
				return "empty";
			}
			return builder.ToString();
		}

		/// <summary>最初に一气に定数值を登录する</summary>
		/// <param name="membername"></param>
		/// <param name="param"></param>
		/// <param name="objthis"></param>
		/// <returns></returns>
		/// <exception cref="VariantException">VariantException</exception>
		/// <exception cref="TjsException">TJSException</exception>
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropSetConstArray(string[] membername, int[] param, Dispatch2 
			objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			if (membername.Length != param.Length)
			{
				throw new TjsException(Error.InternalError);
			}
			int count = membername.Length;
			for (int i = 0; i < count; i++)
			{
				if (membername[i] == null)
				{
					throw new TjsException(Error.InternalError);
				}
				string name = Tjs.MapGlobalStringMap(membername[i]);
				SymbolData data = Add(name);
				data.mSymFlags &= ~SYMBOL_HIDDEN;
				data.mSymFlags &= ~SYMBOL_STATIC;
				if (data.mValue.IsObject())
				{
					throw new TjsException(Error.InternalError);
				}
				data.mValue.Set(param[i]);
			}
			return Error.S_OK;
		}
	}
}

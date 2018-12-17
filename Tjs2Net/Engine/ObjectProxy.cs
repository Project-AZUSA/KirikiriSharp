/*
 * TJS2 CSharp
 */

using Tjs2.NativeApi;

namespace Tjs2.Engine
{
	public class ObjectProxy : Dispatch2
	{
		public ObjectProxy()
		{
		}

		private Dispatch2 mDispatch1;

		private Dispatch2 mDispatch2;

		public virtual void SetObjects(Dispatch2 dsp1, Dispatch2 dsp2)
		{
			mDispatch1 = dsp1;
			mDispatch2 = dsp2;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int FuncCall(int flag, string membername, Variant result, Variant[]
			 param, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.FuncCall(flag, membername, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.FuncCall(flag, membername, result, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int FuncCallByNum(int flag, int num, Variant result, Variant[] param
			, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.FuncCallByNum(flag, num, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.FuncCallByNum(flag, num, result, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropGet(int flag, string membername, Variant result, Dispatch2
			 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.PropGet(flag, membername, result, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.PropGet(flag, membername, result, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropGetByNum(int flag, int num, Variant result, Dispatch2 objthis
			)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.PropGetByNum(flag, num, result, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.PropGetByNum(flag, num, result, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropSet(int flag, string membername, Variant param, Dispatch2 
			objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.PropSet(flag, membername, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.PropSet(flag, membername, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropSetByNum(int flag, int num, Variant param, Dispatch2 objthis
			)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.PropSetByNum(flag, num, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.PropSetByNum(flag, num, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int GetCount(IntWrapper result, string membername, Dispatch2 objthis
			)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.GetCount(result, membername, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.GetCount(result, membername, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int GetCountByNum(IntWrapper result, int num, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.GetCountByNum(result, num, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.GetCountByNum(result, num, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int EnumMembers(int flags, EnumMembersCallback callback, Dispatch2
			 objthis)
		{
			return Error.E_NOTIMPL;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int DeleteMember(int flag, string membername, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.DeleteMember(flag, membername, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.DeleteMember(flag, membername, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int DeleteMemberByNum(int flag, int num, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.DeleteMemberByNum(flag, num, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.DeleteMemberByNum(flag, num, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int Invalidate(int flag, string membername, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.Invalidate(flag, membername, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.Invalidate(flag, membername, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int InvalidateByNum(int flag, int num, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.InvalidateByNum(flag, num, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.InvalidateByNum(flag, num, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsValid(int flag, string membername, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.IsValid(flag, membername, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.IsValid(flag, membername, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsValidByNum(int flag, int num, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.IsValidByNum(flag, num, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.IsValidByNum(flag, num, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int CreateNew(int flag, string membername, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.CreateNew(flag, membername, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.CreateNew(flag, membername, result, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int CreateNewByNum(int flag, int num, Holder<Dispatch2> result, Variant
			[] param, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.CreateNewByNum(flag, num, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.CreateNewByNum(flag, num, result, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsInstanceOf(int flag, string membername, string classname, Dispatch2
			 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.IsInstanceOf(flag, membername, classname, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.IsInstanceOf(flag, membername, classname, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsInstanceOfByNum(int flag, int num, string classname, Dispatch2
			 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.IsInstanceOfByNum(flag, num, classname, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.IsInstanceOfByNum(flag, num, classname, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int Operation(int flag, string membername, Variant result, Variant
			 param, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.Operation(flag, membername, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.Operation(flag, membername, result, param, OBJ2);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int OperationByNum(int flag, int num, Variant result, Variant param
			, Dispatch2 objthis)
		{
			Dispatch2 OBJ1 = ((objthis != null) ? (objthis) : (mDispatch1));
			int hr = mDispatch1.OperationByNum(flag, num, result, param, OBJ1);
			if (hr == Error.E_MEMBERNOTFOUND && mDispatch1 != mDispatch2)
			{
				Dispatch2 OBJ2 = ((objthis != null) ? (objthis) : (mDispatch2));
				return mDispatch2.OperationByNum(flag, num, result, param, OBJ2);
			}
			return hr;
		}

		public virtual int NativeInstanceSupport(int flag, int classid, Holder<NativeInstance
			> pointer)
		{
			return Error.E_NOTIMPL;
		}

		/// <exception cref="VariantException"></exception>
		public virtual int ClassInstanceInfo(int flag, int num, Variant value)
		{
			return Error.E_NOTIMPL;
		}

		public virtual int AddClassInstanceInfo(string name)
		{
			return Error.E_NOTIMPL;
		}

		public virtual NativeInstance GetNativeInstance(int classid)
		{
			return null;
		}

		public virtual int SetNativeInstance(int classid, NativeInstance ni)
		{
			return Error.E_NOTIMPL;
		}
	}
}

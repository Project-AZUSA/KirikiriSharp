/*
 * TJS2 CSharp
 */

using Tjs2.NativeApi;

namespace Tjs2.Engine
{
	public class Dispatch : Dispatch2
	{
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

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal virtual void BeforeDestruction()
		{
		}

		private bool mBeforeDestructionCalled;

		public Dispatch()
		{
		}

		~Dispatch()
		{
			//mBeforeDestructionCalled = false;
			// object destruction
			if (!mBeforeDestructionCalled)
			{
				mBeforeDestructionCalled = true;
				try
				{
					BeforeDestruction();
				}
				catch (VariantException)
				{
				}
				catch (TjsException)
				{
				}
			}
            //try
            //{
            //    base.Finalize();
            //}
            //catch
            //{
            //}
		}

		// function invocation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int FuncCall(int flag, string memberName, Variant result, Variant[]
			 param, Dispatch2 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// function invocation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int FuncCallByNum(int flag, int num, Variant result, Variant[] param
			, Dispatch2 objThis)
		{
			return FuncCall(flag, num.ToString(), result, param, objThis);
		}

		// property get
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropGet(int flag, string memberName, Variant result, Dispatch2
			 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// property get by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropGetByNum(int flag, int num, Variant result, Dispatch2 objThis
			)
		{
			return PropGet(flag, num.ToString(), result, objThis);
		}

		// property set
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropSet(int flag, string memberName, Variant param, Dispatch2 
			objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// property set by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int PropSetByNum(int flag, int num, Variant param, Dispatch2 objThis
			)
		{
			return PropSet(flag, num.ToString(), param, objThis);
		}

		// get member count
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int GetCount(IntWrapper result, string memberName, Dispatch2 objThis
			)
		{
			return Error.E_NOTIMPL;
		}

		// get member count by index number ( result is Integer )
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int GetCountByNum(IntWrapper result, int num, Dispatch2 objThis)
		{
			return GetCount(result, num.ToString(), objThis);
		}

		// enumerate members
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int EnumMembers(int flags, EnumMembersCallback callback, Dispatch2
			 objthis)
		{
			return Error.E_NOTIMPL;
		}

		// delete member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int DeleteMember(int flag, string memberName, Dispatch2 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// delete member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int DeleteMemberByNum(int flag, int num, Dispatch2 objThis)
		{
			return DeleteMember(flag, num.ToString(), objThis);
		}

		// invalidation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int Invalidate(int flag, string memberName, Dispatch2 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// invalidation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int InvalidateByNum(int flag, int num, Dispatch2 objThis)
		{
			return Invalidate(flag, num.ToString(), objThis);
		}

		// get validation, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsValid(int flag, string memberName, Dispatch2 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// get validation by index number, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsValidByNum(int flag, int num, Dispatch2 objThis)
		{
			return IsValid(flag, num.ToString(), objThis);
		}

		// create new object
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int CreateNew(int flag, string memberName, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// create new object by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int CreateNewByNum(int flag, int num, Holder<Dispatch2> result, Variant
			[] param, Dispatch2 objThis)
		{
			return CreateNew(flag, num.ToString(), result, param, objThis);
		}

		// reserved1 not use
		// class instance matching returns false or true
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsInstanceOf(int flag, string memberName, string className, Dispatch2
			 objThis)
		{
			return memberName != null ? Error.E_MEMBERNOTFOUND : Error.E_NOTIMPL;
		}

		// class instance matching by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int IsInstanceOfByNum(int flag, int num, string className, Dispatch2
			 objThis)
		{
			return IsInstanceOf(flag, num.ToString(), className, objThis);
		}

		// operation with member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int Operation(int flag, string memberName, Variant result, Variant
			 param, Dispatch2 objThis)
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
			Variant tmp = new Variant();
			int hr = PropGet(0, memberName, tmp, objThis);
			if (hr < 0)
			{
				return hr;
			}
			// #define TJS_FAILED(x) ((x)<0)
			DoVariantOperation(op, tmp, param);
			hr = PropSet(0, memberName, tmp, objThis);
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

		// operation with member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int OperationByNum(int flag, int num, Variant result, Variant param
			, Dispatch2 objThis)
		{
			return Operation(flag, num.ToString(), result, param, objThis);
		}

		// support for native instance
		public virtual int NativeInstanceSupport(int flag, int classid, Holder<NativeInstance
			> pointer)
		{
			return Error.E_NOTIMPL;
		}

		// support for class instance infomation
		/// <exception cref="VariantException"></exception>
		public virtual int ClassInstanceInfo(int flag, int num, Variant value)
		{
			return Error.E_NOTIMPL;
		}

		public virtual int AddClassInstanceInfo(string name)
		{
			return Error.E_NOTIMPL;
		}

		// special funcsion
		public virtual NativeInstance GetNativeInstance(int classid)
		{
			Holder<NativeInstance> holder = new Holder<NativeInstance>(null);
			int hr = NativeInstanceSupport(Interface.NIS_GETINSTANCE, classid, holder);
			if (hr >= 0)
			{
				return holder.mValue;
			}
			else
			{
				return null;
			}
		}

		public virtual int SetNativeInstance(int classid, NativeInstance ni)
		{
			Holder<NativeInstance> holder = new Holder<NativeInstance>(ni);
			return NativeInstanceSupport(Interface.NIS_REGISTER, classid, holder);
		}

		/// <exception cref="VariantException"></exception>
		public static void DoVariantOperation(int op, Variant target, Variant param)
		{
			switch (op)
			{
				case OP_BAND:
				{
					target.AndEqual(param);
					return;
				}

				case OP_BOR:
				{
					target.OrEqual(param);
					return;
				}

				case OP_BXOR:
				{
					target.BitXorEqual(param);
					return;
				}

				case OP_SUB:
				{
					target.SubtractEqual(param);
					return;
				}

				case OP_ADD:
				{
					target.AddEqual(param);
					return;
				}

				case OP_MOD:
				{
					target.ResidueEqual(param);
					return;
				}

				case OP_DIV:
				{
					target.DivideEqual(param);
					return;
				}

				case OP_IDIV:
				{
					target.Idivequal(param);
					return;
				}

				case OP_MUL:
				{
					target.MultiplyEqual(param);
					return;
				}

				case OP_LOR:
				{
					target.Logicalorequal(param);
					return;
				}

				case OP_LAND:
				{
					target.Logicalandequal(param);
					return;
				}

				case OP_SAR:
				{
					target.RightShiftEqual(param);
					return;
				}

				case OP_SAL:
				{
					target.LeftShiftEqual(param);
					return;
				}

				case OP_SR:
				{
					target.Rbitshiftequal(param);
					return;
				}

				case OP_INC:
				{
					target.Increment();
					return;
				}

				case OP_DEC:
				{
					target.Decrement();
					return;
				}
			}
		}
		// reserved2
		// reserved3
	}
}

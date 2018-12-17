/*
 * TJS2 CSharp
 */

using Tjs2.NativeApi;

namespace Tjs2.Engine
{
	public interface Dispatch2
	{
		// function invocation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int FuncCall(int flag, string memberName, Variant result, Variant[] param, Dispatch2
			 objThis);

		// function invocation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int FuncCallByNum(int flag, int num, Variant result, Variant[] param, Dispatch2 objThis
			);

		// property get
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int PropGet(int flag, string memberName, Variant result, Dispatch2 objThis);

		// property get by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int PropGetByNum(int flag, int num, Variant result, Dispatch2 objThis);

		// property set
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int PropSet(int flag, string memberName, Variant param, Dispatch2 objThis);

		// property set by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int PropSetByNum(int flag, int num, Variant param, Dispatch2 objThis);

		// get member count
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int GetCount(IntWrapper result, string memberName, Dispatch2 objThis);

		// get member count by index number ( result is Integer )
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int GetCountByNum(IntWrapper result, int num, Dispatch2 objThis);

		// enumerate members
		//public int enumMembers( int flag, VariantClosure callback, Dispatch2 objThis ) throws VariantException, TJSException;
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TJSException"></exception>
		int EnumMembers(int flags, EnumMembersCallback callback, Dispatch2 objthis);

		// delete member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int DeleteMember(int flag, string memberName, Dispatch2 objThis);

		// delete member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int DeleteMemberByNum(int flag, int num, Dispatch2 objThis);

		// invalidation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int Invalidate(int flag, string memberName, Dispatch2 objThis);

		// invalidation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int InvalidateByNum(int flag, int num, Dispatch2 objThis);

		// get validation, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int IsValid(int flag, string memberName, Dispatch2 objThis);

		// get validation by index number, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int IsValidByNum(int flag, int num, Dispatch2 objThis);

		// create new object
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int CreateNew(int flag, string memberName, Holder<Dispatch2> result, Variant[] param
			, Dispatch2 objThis);

		// create new object by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int CreateNewByNum(int flag, int num, Holder<Dispatch2> result, Variant[] param, 
			Dispatch2 objThis);

		// reserved1 not use
		// class instance matching returns false or true
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int IsInstanceOf(int flag, string memberName, string className, Dispatch2 objThis
			);

		// class instance matching by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int IsInstanceOfByNum(int flag, int num, string className, Dispatch2 objThis);

		// operation with member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int Operation(int flag, string memberName, Variant result, Variant param, Dispatch2
			 objThis);

		// operation with member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		int OperationByNum(int flag, int num, Variant result, Variant param, Dispatch2 objThis
			);

		// support for native instance
		int NativeInstanceSupport(int flag, int classid, Holder<NativeInstance> pointer);

		// support for class instance infomation
		/// <exception cref="VariantException"></exception>
		int ClassInstanceInfo(int flag, int num, Variant value);

		// special funcsion
		int AddClassInstanceInfo(string name);

		// special funcsion
		NativeInstance GetNativeInstance(int classid);

		// special funcsion
		int SetNativeInstance(int classid, NativeInstance ni);
		// reserved2
		// reserved3
	}
}

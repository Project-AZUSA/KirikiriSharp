/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.NativeApi;

namespace Tjs2.Engine
{
	public class VariantClosure
	{
		public Dispatch2 mObject;

		public Dispatch2 mObjThis;

		public VariantClosure(Dispatch2 obj)
		{
			mObject = obj;
		}

		public VariantClosure(Dispatch2 obj, Dispatch2 objthis)
		{
			//mObjThis = null;
			mObject = obj;
			mObjThis = objthis;
		}

		public virtual void Set(Dispatch2 obj)
		{
			Set(obj, null);
		}

		public virtual void Set(Dispatch2 obj, Dispatch2 objthis)
		{
			mObject = obj;
			mObjThis = objthis;
		}

		public virtual void Set(VariantClosure clo)
		{
			mObject = clo.mObject;
			mObjThis = clo.mObjThis;
		}

		public virtual Dispatch2 SelectObject()
		{
			if (mObjThis != null)
			{
				return mObjThis;
			}
			else
			{
				return mObject;
			}
		}

		public override bool Equals(object o)
		{
			if (o is VariantClosure)
			{
				VariantClosure vc = (VariantClosure)o;
				return mObject == vc.mObject && mObjThis == vc.mObjThis;
			}
			else
			{
				return false;
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int FuncCall(int flag, string memberName, Variant result, Variant[]
			 param, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.FuncCall(flag, memberName, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int FuncCallByNum(int flag, int num, Variant result, Variant[] param
			, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.FuncCallByNum(flag, num, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int PropGet(int flag, string mumberName, Variant result, Dispatch2
			 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.PropGet(flag, mumberName, result, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int PropGetByNum(int flag, int num, Variant result, Dispatch2 objThis
			)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.PropGetByNum(flag, num, result, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int PropSet(int flag, string mumberName, Variant param, Dispatch2 
			objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.PropSet(flag, mumberName, param, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int PropSetByNum(int flag, int num, Variant param, Dispatch2 objThis
			)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.PropSetByNum(flag, num, param, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int GetCount(IntWrapper result, string memberName, Dispatch2 objThis
			)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.GetCount(result, memberName, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int GetCountByNum(IntWrapper result, int num, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.GetCountByNum(result, num, mObjThis != null ? mObjThis : (objThis 
				!= null ? objThis : mObject));
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int EnumMembers(int flags, EnumMembersCallback callback, Dispatch2
			 objthis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.EnumMembers(flags, callback, mObjThis != null ? mObjThis : (objthis
				 != null ? objthis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int DeleteMember(int flag, string memberName, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.DeleteMember(flag, memberName, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int DeleteMemberByNum(int flag, int num, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.DeleteMemberByNum(flag, num, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int Invalidate(int flag, string memberName, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.Invalidate(flag, memberName, mObjThis != null ? mObjThis : (objThis
				 != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int InvalidateByNum(int flag, int num, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.InvalidateByNum(flag, num, mObjThis != null ? mObjThis : (objThis 
				!= null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int IsValid(int flag, string memberName, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.IsValid(flag, memberName, mObjThis != null ? mObjThis : (objThis !=
				 null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int IsValidByNum(int flag, int num, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.IsValidByNum(flag, num, mObjThis != null ? mObjThis : (objThis != 
				null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int CreateNew(int flag, string memberName, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.CreateNew(flag, memberName, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int CreateNewByNum(int flag, int num, Holder<Dispatch2> result, Variant
			[] param, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.CreateNewByNum(flag, num, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int IsInstanceOf(int flag, string memberName, string className, Dispatch2
			 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.IsInstanceOf(flag, memberName, className, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		// オリジナルはバグ？ 关数名が一致していない
		//tjs_error IsInstanceOf(tjs_uint32 flag, tjs_int num, tjs_char *classname, iTJSDispatch2 *objthis) const {
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int IsInstanceOfByNum(int flag, int num, string className, Dispatch2
			 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.IsInstanceOfByNum(flag, num, className, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int Operation(int flag, string memberName, Variant result, Variant
			 param, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.Operation(flag, memberName, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual int OperationByNum(int flag, int num, Variant result, Variant param
			, Dispatch2 objThis)
		{
			if (mObject == null)
			{
				throw new TjsException(Error.NullAccess);
			}
			return mObject.OperationByNum(flag, num, result, param, mObjThis != null ? mObjThis
				 : (objThis != null ? objThis : mObject));
		}

		public sealed override string ToString()
		{
			StringBuilder str = new StringBuilder(128);
			str.Append("(object)");
			str.Append('(');
			if (mObject != null)
			{
				str.Append('[');
				if (mObject is NativeApi.NativeClass)
				{
					str.Append(((NativeApi.NativeClass)mObject).GetClassName());
				}
				else
				{
					if (mObject is InterCodeObject)
					{
						str.Append(((InterCodeObject)mObject).GetName());
					}
					else
					{
						if (mObject is CustomObject)
						{
							string name = ((CustomObject)mObject).GetClassNames();
							if (name != null)
							{
								str.Append(name);
							}
							else
							{
								str.Append(mObject.GetType().FullName);
							}
						}
						else
						{
							str.Append(mObject.GetType().FullName);
						}
					}
				}
				str.Append(']');
			}
			else
			{
				str.Append("0x00000000");
			}
			if (mObjThis != null)
			{
				str.Append('[');
				if (mObjThis is NativeApi.NativeClass)
				{
					str.Append(((NativeApi.NativeClass)mObjThis).GetClassName());
				}
				else
				{
					if (mObjThis is InterCodeObject)
					{
						str.Append(((InterCodeObject)mObjThis).GetName());
					}
					else
					{
						if (mObjThis is CustomObject)
						{
							string name = ((CustomObject)mObjThis).GetClassNames();
							if (name != null)
							{
								str.Append(name);
							}
							else
							{
								str.Append(mObjThis.GetType().FullName);
							}
						}
						else
						{
							str.Append(mObjThis.GetType().FullName);
						}
					}
				}
				str.Append(']');
			}
			else
			{
				str.Append(":0x00000000");
			}
			str.Append(')');
			return str.ToString();
		}
	}
}

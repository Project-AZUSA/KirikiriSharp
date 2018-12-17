/*
 * TJS2 CSharp
 */

using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	internal class ArrayObject : CustomObject
	{
		private static Variant VoidValue;

		private const int WORK_CHAR_LEN = 256;

		private static char[] WorkChar;

		private static int[] Result;

		public static void Initialize()
		{
			WorkChar = new char[WORK_CHAR_LEN];
			Result = new int[1];
			VoidValue = new Variant();
		}

		public static void FinalizeApplication()
		{
			WorkChar = null;
			Result = null;
			VoidValue = null;
		}

		public ArrayObject() : base()
		{
			mCallFinalize = false;
		}

		/// <exception cref="TjsException"></exception>
		protected internal override void FinalizeObject()
		{
			ArrayNI ni = (ArrayNI)GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				throw new TjsException(Error.NativeClassCrash);
			}
			Clear(ni);
			base.FinalizeObject();
		}

		public virtual void Clear(ArrayNI ni)
		{
			// clear members
			int count = ni.mItems.Count;
			for (int i = 0; i < count; i++)
			{
				Variant v = ni.mItems[i];
				v.Clear();
			}
			ni.mItems.Clear();
		}

		/// <exception cref="TjsException"></exception>
		public virtual void Erase(ArrayNI ni, int num)
		{
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num < 0)
			{
				throw new TjsException(Error.RangeError);
			}
			if (num >= ni.mItems.Count)
			{
				throw new TjsException(Error.RangeError);
			}
			ni.mItems.Remove(num);
		}

		public virtual int Remove(ArrayNI ni, Variant @ref, bool removeall)
		{
			int count = 0;
			IntVector todelete = new IntVector();
			int arrayCount = ni.mItems.Count;
			for (int i = 0; i < arrayCount; i++)
			{
				Variant v = ni.mItems[i];
				if (@ref.DiscernCompareInternal(v))
				{
					count++;
					todelete.Add(i);
					if (!removeall)
					{
						break;
					}
				}
			}
			// list objects up
			int delCount = todelete.Size();
			for (int i_1 = 0; i_1 < delCount; i_1++)
			{
				int pos = todelete.Get(i_1);
				Variant v = ni.mItems[pos];
				v.Clear();
			}
			// remove items found
			for (int i_2 = delCount - 1; i_2 >= 0; i_2--)
			{
				ni.mItems.Remove(todelete.Get(i_2));
			}
			todelete = null;
			return count;
		}

		/// <exception cref="TjsException"></exception>
		public virtual void Insert(ArrayNI ni, Variant val, int num)
		{
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num < 0)
			{
				throw new TjsException(Error.RangeError);
			}
			int count = ni.mItems.Count;
			if (num > count)
			{
				throw new TjsException(Error.RangeError);
			}
			ni.mItems.Add(num, new Variant(val));
		}

		public virtual void Add(ArrayNI ni, Variant val)
		{
			ni.mItems.AddItem(new Variant(val));
		}

		/// <exception cref="TjsException"></exception>
		public virtual void Insert(ArrayNI ni, Variant[] val, int num)
		{
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num < 0)
			{
				throw new TjsException(Error.RangeError);
			}
			int count = ni.mItems.Count;
			if (num > count)
			{
				throw new TjsException(Error.RangeError);
			}
			int end = val.Length;
			ni.mItems.EnsureCapacity(count + end);
			for (int i = 0; i < end; i++)
			{
				ni.mItems.Add(num + i, new Variant(val[i]));
			}
		}

		private bool IsNumber(string str, int[] result)
		{
			if (str == null)
			{
				return false;
			}
			int len = str.Length;
			// 1文字目をチェックして、数值以外は早々に除外する
			if (len > 0)
			{
				char ch = str[0];
				if ((ch >= '0' && ch <= '9') || ch == '-' || ch == '+' || (ch >= unchecked((int)(
					0x09)) && ch <= unchecked((int)(0x0D))) || ch == ' ')
				{
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			char[] work;
			char ch_1;
			if (len < WORK_CHAR_LEN)
			{
				Sharpen.Runtime.GetCharsForString(str, 0, len, WorkChar, 0);
				work = WorkChar;
			}
			else
			{
				work = str.ToCharArray();
			}
			int i = 0;
			// skip space
			while (i < len)
			{
				ch_1 = work[i];
				if ((ch_1 >= unchecked((int)(0x09)) && ch_1 <= unchecked((int)(0x0D))) || ch_1 ==
					 ' ')
				{
					i++;
				}
				else
				{
					break;
				}
			}
			if (i >= len)
			{
				return false;
			}
			bool sign = false;
			ch_1 = work[i];
			if (ch_1 == '-')
			{
				sign = true;
				i++;
			}
			else
			{
				if (ch_1 == '+')
				{
					i++;
				}
			}
			if (i >= len)
			{
				return false;
			}
			// skip space
			while (i < len)
			{
				ch_1 = work[i];
				if ((ch_1 >= unchecked((int)(0x09)) && ch_1 <= unchecked((int)(0x0D))) || ch_1 ==
					 ' ')
				{
					i++;
				}
				else
				{
					break;
				}
			}
			if (i >= len)
			{
				return false;
			}
			int number = 0;
			for (; i < len; i++)
			{
				ch_1 = work[i];
				if (ch_1 >= '0' && ch_1 <= '9')
				{
					int num = ch_1 - '0';
					number = number * 10 + num;
				}
				else
				{
					if (ch_1 == '.' || (ch_1 >= unchecked((int)(0x09)) && ch_1 <= unchecked((int)(0x0D
						))) || ch_1 == ' ')
					{
						break;
					}
					else
					{
						return false;
					}
				}
			}
			if (ch_1 == '.')
			{
				for (; i < len; i++)
				{
					ch_1 = work[i];
					if ((ch_1 >= '0' && ch_1 <= '9') || ch_1 == '.')
					{
					}
					else
					{
						break;
					}
				}
			}
			// skip space
			while (i < len)
			{
				ch_1 = work[i];
				if ((ch_1 >= unchecked((int)(0x09)) && ch_1 <= unchecked((int)(0x0D))) || ch_1 ==
					 ' ')
				{
					i++;
				}
				else
				{
					break;
				}
			}
			if (i == len)
			{
				if (sign)
				{
					result[0] = -number;
				}
				else
				{
					result[0] = number;
				}
				return true;
			}
			return false;
		}

		// function invocation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int FuncCall(int flag, string memberName, Variant result, Variant
			[] param, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return FuncCallByNum(flag, Result[0], result, param, objThis);
			}
			return base.FuncCall(flag, memberName, result, param, objThis);
		}

		// function invocation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int FuncCallByNum(int flag, int num, Variant result, Variant[] param
			, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultFuncCall(flag, val, result, param, objthis);
		}

		// property get
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropGet(int flag, string memberName, Variant result, Dispatch2
			 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return PropGetByNum(flag, Result[0], result, objThis);
			}
			return base.PropGet(flag, memberName, result, objThis);
		}

		// property get by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropGetByNum(int flag, int num, Variant result, Dispatch2 objthis
			)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultPropGet(flag, val, result, objthis);
		}

		// property set
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropSet(int flag, string memberName, Variant param, Dispatch2
			 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return PropSetByNum(flag, Result[0], param, objThis);
			}
			return base.PropSet(flag, memberName, param, objThis);
		}

		// property set by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropSetByNum(int flag, int num, Variant param, Dispatch2 objthis
			)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num >= ni.mItems.Count)
			{
				if ((flag & Interface.MEMBERMUSTEXIST) != 0)
				{
					return Error.E_MEMBERNOTFOUND;
				}
				//ni.mItems.resize(num+1);
				for (int i = ni.mItems.Count; i <= num; i++)
				{
					ni.mItems.AddItem(new Variant());
				}
			}
			if (num < 0)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = ni.mItems[num];
			return DefaultPropSet(flag, val, param, objthis);
		}

		// enumerate members
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual int EnumMembers(int flag, VariantClosure callback, Dispatch2 objThis
			)
		{
			return Error.E_NOTIMPL;
		}

		// currently not implemented
		// delete member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int DeleteMember(int flag, string memberName, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return DeleteMemberByNum(flag, Result[0], objThis);
			}
			return base.DeleteMember(flag, memberName, objThis);
		}

		// delete member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int DeleteMemberByNum(int flag, int num, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num < 0 || num >= ni.mItems.Count)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			ni.mItems.Remove(num);
			return Error.S_OK;
		}

		// invalidation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int Invalidate(int flag, string memberName, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return InvalidateByNum(flag, Result[0], objThis);
			}
			return base.Invalidate(flag, memberName, objThis);
		}

		// invalidation by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int InvalidateByNum(int flag, int num, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultInvalidate(flag, val, objthis);
		}

		// get validation, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsValid(int flag, string memberName, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return IsValidByNum(flag, Result[0], objThis);
			}
			return base.IsValid(flag, memberName, objThis);
		}

		// get validation by index number, returns true or false
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsValidByNum(int flag, int num, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultIsValid(flag, val, objthis);
		}

		// create new object
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int CreateNew(int flag, string memberName, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return CreateNewByNum(flag, Result[0], result, param, objThis);
			}
			return base.CreateNew(flag, memberName, result, param, objThis);
		}

		// create new object by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int CreateNewByNum(int flag, int num, Holder<Dispatch2> result, Variant
			[] param, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultCreateNew(flag, val, result, param, objthis);
		}

		// reserved1 not use
		// class instance matching returns false or true
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOf(int flag, string memberName, string className, Dispatch2
			 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return IsInstanceOfByNum(flag, Result[0], className, objThis);
			}
			return base.IsInstanceOf(flag, memberName, className, objThis);
		}

		// class instance matching by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOfByNum(int flag, int num, string className, Dispatch2
			 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			int membercount = ni.mItems.Count;
			if (num < 0)
			{
				num = membercount + num;
			}
			if ((flag & Interface.MEMBERMUSTEXIST) != 0 && (num < 0 || membercount <= num))
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = new Variant((membercount <= num || num < 0) ? VoidValue : ni.mItems
				[num]);
			return DefaultIsInstanceOf(flag, val, className, objthis);
		}

		// operation with member
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int Operation(int flag, string memberName, Variant result, Variant
			 param, Dispatch2 objThis)
		{
			if (IsNumber(memberName, Result))
			{
				return OperationByNum(flag, Result[0], result, param, objThis);
			}
			return base.Operation(flag, memberName, result, param, objThis);
		}

		// operation with member by index number
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int OperationByNum(int flag, int num, Variant result, Variant param
			, Dispatch2 objthis)
		{
			if (!GetValidity())
			{
				return Error.E_INVALIDOBJECT;
			}
			ArrayNI ni = (ArrayNI)objthis.GetNativeInstance(ArrayClass.ClassID);
			if (ni == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			if (num < 0)
			{
				num += ni.mItems.Count;
			}
			if (num >= ni.mItems.Count)
			{
				if ((flag & Interface.MEMBERMUSTEXIST) != 0)
				{
					return Error.E_MEMBERNOTFOUND;
				}
				//ni.mItems.resize(num+1);
				for (int i = ni.mItems.Count; i <= num; i++)
				{
					ni.mItems.AddItem(new Variant());
				}
			}
			if (num < 0)
			{
				return Error.E_MEMBERNOTFOUND;
			}
			Variant val = ni.mItems[num];
			return DefaultOperation(flag, val, result, param, objthis);
		}
	}
}

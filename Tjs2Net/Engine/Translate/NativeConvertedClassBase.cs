/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.NativeApi;
using Tjs2.Sharpen;

namespace Tjs2.Engine.Translate
{
	public class NativeConvertedClassBase : Dispatch
	{
		protected internal const int OP_BAND = unchecked((int)(0x0001));

		protected internal const int OP_BOR = unchecked((int)(0x0002));

		protected internal const int OP_BXOR = unchecked((int)(0x0003));

		protected internal const int OP_SUB = unchecked((int)(0x0004));

		protected internal const int OP_ADD = unchecked((int)(0x0005));

		protected internal const int OP_MOD = unchecked((int)(0x0006));

		protected internal const int OP_DIV = unchecked((int)(0x0007));

		protected internal const int OP_IDIV = unchecked((int)(0x0008));

		protected internal const int OP_MUL = unchecked((int)(0x0009));

		protected internal const int OP_LOR = unchecked((int)(0x000a));

		protected internal const int OP_LAND = unchecked((int)(0x000b));

		protected internal const int OP_SAR = unchecked((int)(0x000c));

		protected internal const int OP_SAL = unchecked((int)(0x000d));

		protected internal const int OP_SR = unchecked((int)(0x000e));

		protected internal const int OP_INC = unchecked((int)(0x000f));

		protected internal const int OP_DEC = unchecked((int)(0x0010));

		internal WeakReference<Tjs> mOwner;

		public virtual Tjs GetOwner()
		{
			return mOwner.Get();
		}

		public NativeConvertedClassBase(Tjs owner)
		{
			mOwner = new WeakReference<Tjs>(owner);
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void OperateProperty(VariantClosure clo, Variant result
			, Variant param, Dispatch2 objthis, int ope)
		{
			Dispatch2 objThis = clo.mObjThis != null ? clo.mObjThis : objthis;
			int hr = clo.Operation(ope, null, result, param, objThis);
			if (hr < 0)
			{
				ThrowFrom_tjs_error(hr, null);
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void OperatePropertyIndirect(VariantClosure clo, Variant
			 name, Variant result, Variant param, Dispatch2 objthis, int ope)
		{
			Dispatch2 objThis = clo.mObjThis != null ? clo.mObjThis : objthis;
			if (name.IsInteger() != true)
			{
				string str = name.AsString();
				int hr = clo.Operation(ope, str, result, param, objThis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, str);
				}
			}
			else
			{
				int num = name.AsInteger();
				int hr = clo.OperationByNum(ope, num, result, param, objThis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error_num(hr, num);
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void OperatePropertyDirect(VariantClosure clo, string name
			, Variant result, Variant param, Dispatch2 objthis, int ope)
		{
			Dispatch2 objThis = clo.mObjThis != null ? clo.mObjThis : objthis;
			int hr = clo.Operation(ope, name, result, param, objThis);
			if (hr < 0)
			{
				ThrowFrom_tjs_error(hr, name);
			}
		}

		/// <exception cref="VariantException"></exception>
		protected internal static void DisplayExceptionGeneratedCode(int codepos, Variant
			[] ra, int ra_offset)
		{
			StringBuilder builder = new StringBuilder(128);
			builder.Append("==== An exception occured");
			builder.Append(", VM ip = ");
			builder.Append(codepos);
			builder.Append(" ====");
			Tjs.OutputToConsole(builder.ToString());
		}

		// ディスアセンブルコードは出力できない
		// レジスタダンプもほとんど意味がないので出力しない
		/// <exception cref="TjsException"></exception>
		protected internal static void ThrowInvalidVMCode()
		{
			throw new TjsException(Error.InvalidOpecode);
		}

		/// <exception cref="VariantException"></exception>
		protected internal static void AddClassInstanceInfo(Dispatch2 dsp, string className
			)
		{
			if (dsp != null)
			{
				dsp.AddClassInstanceInfo(className);
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void ThrowScriptException(Variant val, ScriptBlock block
			, int srcpos)
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
			throw new TjsScriptException(msg, block, srcpos, val);
		}

		/// <exception cref="TjsException"></exception>
		public static void ThrowFrom_tjs_error_num(int hr, int num)
		{
			Error.ThrowFrom_tjs_error(hr, num.ToString());
		}

		/// <exception cref="TjsException"></exception>
		public static void ThrowFrom_tjs_error(int hr, string name)
		{
			Error.ThrowFrom_tjs_error(hr, name);
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void SetPropertyIndirect(Variant target, Variant member
			, Variant param, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				if (member.IsInteger() != true)
				{
					string str = member.AsString();
					int hr = clo.PropSet(flags, str, param, clo.mObjThis != null ? clo.mObjThis : objthis
						);
					if (hr < 0)
					{
						ThrowFrom_tjs_error(hr, str);
					}
				}
				else
				{
					int hr = clo.PropSetByNum(flags, member.AsInteger(), param, clo.mObjThis != null ? 
						clo.mObjThis : objthis);
					if (hr < 0)
					{
						ThrowFrom_tjs_error_num(hr, member.AsInteger());
					}
				}
			}
			else
			{
				if (target.IsString())
				{
					SetStringProperty(param, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						SetOctetProperty(param, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void GetOctetProperty(Variant result, Variant octet, Variant member
			)
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
		private static void GetOctetProperty(Variant result, Variant octet, string name)
		{
			// processes properties toward octets.
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

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void GetOctetProperty(Variant result, Variant octet, int n)
		{
			// processes properties toward octets.
			ByteBuffer o = octet.AsOctet();
			int len = o != null ? o.Capacity() : 0;
			if (n < 0 || n >= len)
			{
				throw new TjsException(Error.RangeError);
			}
			result.Set(o.Get(n));
			return;
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void SetOctetProperty(Variant param, Variant octet, Variant member
			)
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
		private static void SetOctetProperty(Variant param, Variant octet, string name)
		{
			// processes properties toward octets.
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

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void SetOctetProperty(Variant param, Variant octet, int member)
		{
			ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void GetStringProperty(Variant result, Variant str, Variant member
			)
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
		private static void GetStringProperty(Variant result, Variant str, string name)
		{
			// processes properties toward strings.
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

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void GetStringProperty(Variant result, Variant str, int n)
		{
			// processes properties toward strings.
			string s = str.AsString();
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

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void SetStringProperty(Variant param, Variant str, Variant member)
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
		private static void SetStringProperty(Variant param, Variant str, string name)
		{
			// processes properties toward strings.
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

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		private static void SetStringProperty(Variant param, Variant str, int member)
		{
			// processes properties toward strings.
			ThrowFrom_tjs_error(Error.E_ACCESSDENIED, string.Empty);
		}

		// getPropertyIndirect( ra[ra_offset+ca[code+1]], ra[ra_offset+ca[code+2]], ra[ra_offset+ca[code+3]], objthis, flags );
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void GetPropertyIndirect(Variant result, Variant target
			, Variant member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				if (member.IsInteger() != true)
				{
					string str = member.AsString();
					int hr = clo.PropGet(flags, str, result, clo.mObjThis != null ? clo.mObjThis : objthis
						);
					if (hr < 0)
					{
						ThrowFrom_tjs_error(hr, str);
					}
				}
				else
				{
					int hr = clo.PropGetByNum(flags, member.AsInteger(), result, clo.mObjThis != null
						 ? clo.mObjThis : objthis);
					if (hr < 0)
					{
						ThrowFrom_tjs_error_num(hr, member.AsInteger());
					}
				}
			}
			else
			{
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		// setPropertyDirect( ra[ra_offset+ca[code+1]], da[ca[code+2]], ra[ra_offset+ca[code+3]], objthis, flags );
		// member は、固定值なので、事前に分岐判定出来るから、展开するようにした方がいいな
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void SetPropertyDirect(Variant target, string member, Variant
			 param, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				int hr = clo.PropSet(flags, member, param, clo.mObjThis != null ? clo.mObjThis : 
					objthis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, member);
				}
			}
			else
			{
				if (target.IsString())
				{
					SetStringProperty(param, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						SetOctetProperty(param, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void SetPropertyDirect(Variant target, int member, Variant
			 param, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				string name = Sharpen.Extensions.ToString(member);
				int hr = clo.PropSet(flags, name, param, clo.mObjThis != null ? clo.mObjThis : objthis
					);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, name);
				}
			}
			else
			{
				if (target.IsString())
				{
					SetStringProperty(param, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						SetOctetProperty(param, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		//getPropertyDirect( ra[ra_offset+ca[code+1]], ra[ra_offset+ca[code+2]], da[ca[code+3]], objthis, flags );
		// member は、固定值なので、事前に条件分岐できる、文字か数值で割り分け
		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void GetPropertyDirect(Variant result, Variant target, 
			string member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				int hr = clo.PropGet(flags, member, result, clo.mObjThis != null ? clo.mObjThis : 
					objthis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, member);
				}
			}
			else
			{
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void GetPropertyDirect(Variant result, Variant target, 
			int member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				string name = Sharpen.Extensions.ToString(member);
				int hr = clo.PropGet(flags, name, result, clo.mObjThis != null ? clo.mObjThis : objthis
					);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, name);
				}
			}
			else
			{
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		protected internal static void ProcessOctetFunction(string member, string target, 
			Variant[] args, Variant result)
		{
			ThrowFrom_tjs_error(Error.E_MEMBERNOTFOUND, member);
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void ProcessStringFunction(string member, string target
			, Variant[] args, Variant result)
		{
			InterCodeObject.ProcessStringFunction(member, target, args, result);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal static void InstanceOf(Variant name, Variant targ)
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
		protected internal virtual void Eval(Variant val, Dispatch2 objthis, bool resneed
			)
		{
			Variant res = new Variant();
			string str = val.AsString();
			if (str.Length > 0)
			{
				if (resneed)
				{
					GetOwner().EvalExpression(str, res, objthis, null, 0);
				}
				else
				{
					GetOwner().EvalExpression(str, null, objthis, null, 0);
				}
			}
			if (resneed)
			{
				val.Set(res);
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void TypeOfMemberIndirect(Variant result, Variant target
			, Variant member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				VariantClosure clo = target.AsObjectClosure();
				if (member.IsInteger() != true)
				{
					string str = member.AsString();
					int hr = clo.PropGet(flags, str, result, clo.mObjThis != null ? clo.mObjThis : objthis
						);
					if (hr == Error.S_OK)
					{
						TypeOf(result);
					}
					else
					{
						if (hr == Error.E_MEMBERNOTFOUND)
						{
							result.Set("undefined");
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
					int hr = clo.PropGetByNum(flags, member.AsInteger(), result, clo.mObjThis != null
						 ? clo.mObjThis : objthis);
					if (hr == Error.S_OK)
					{
						TypeOf(result);
					}
					else
					{
						if (hr == Error.E_MEMBERNOTFOUND)
						{
							result.Set("undefined");
						}
						else
						{
							if (hr < 0)
							{
								ThrowFrom_tjs_error_num(hr, member.AsInteger());
							}
						}
					}
				}
			}
			else
			{
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
					TypeOf(result);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
						TypeOf(result);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void TypeOfMemberDirect(Variant result, Variant target, 
			string member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				int hr;
				VariantClosure clo = target.AsObjectClosure();
				hr = clo.PropGet(flags, member, result, clo.mObjThis != null ? clo.mObjThis : objthis
					);
				if (hr == Error.S_OK)
				{
					TypeOf(result);
				}
				else
				{
					if (hr == Error.E_MEMBERNOTFOUND)
					{
						result.Set("undefined");
					}
					else
					{
						if (hr < 0)
						{
							ThrowFrom_tjs_error(hr, member);
						}
					}
				}
			}
			else
			{
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
					TypeOf(result);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
						TypeOf(result);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		protected internal static void TypeOfMemberDirect(Variant result, Variant target, 
			int member, Dispatch2 objthis, int flags)
		{
			if (target.IsObject())
			{
				int hr;
				VariantClosure clo = target.AsObjectClosure();
				string name = Sharpen.Extensions.ToString(member);
				hr = clo.PropGet(flags, name, result, clo.mObjThis != null ? clo.mObjThis : objthis
					);
				if (hr == Error.S_OK)
				{
					TypeOf(result);
				}
				else
				{
					if (hr == Error.E_MEMBERNOTFOUND)
					{
						result.Set("undefined");
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
				if (target.IsString())
				{
					GetStringProperty(result, target, member);
					TypeOf(result);
				}
				else
				{
					if (target.IsOctet())
					{
						GetOctetProperty(result, target, member);
						TypeOf(result);
					}
					else
					{
						string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
							(target));
						throw new VariantException(mes);
					}
				}
			}
		}

		protected internal static void TypeOf(Variant val)
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
		protected internal static void OperatePropertyIndirect0(VariantClosure clo, Variant
			 name, Variant result, Dispatch2 objthis, int ope)
		{
			if (name.IsInteger() != true)
			{
				string str = name.AsString();
				int hr = clo.Operation(ope, str, result, null, clo.mObjThis != null ? clo.mObjThis
					 : objthis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error(hr, str);
				}
			}
			else
			{
				int hr = clo.OperationByNum(ope, name.AsInteger(), result, null, clo.mObjThis != 
					null ? clo.mObjThis : objthis);
				if (hr < 0)
				{
					ThrowFrom_tjs_error_num(hr, name.AsInteger());
				}
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
				int v = str[0];
				val.Set(v);
			}
			else
			{
				val.Set(0);
			}
		}
	}
}

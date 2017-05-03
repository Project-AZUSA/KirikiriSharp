/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi.Internal
{
	public class ExceptionClass : NativeClass
	{
		public static int ClassID = -1;

		private static readonly string CLASS_NAME = "Exception";

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public ExceptionClass() : base(CLASS_NAME)
		{
            ClassID = Tjs.FindNativeClassId(CLASS_NAME);
            // constructor
            RegisterNCM(CLASS_NAME, new _NativeClassConstructor_15(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("finalize", new _NativeClassMethod_38(), CLASS_NAME, Interface.nitMethod
				, 0);
		}

		private sealed class _NativeClassConstructor_15 : NativeClassConstructor
		{
			public _NativeClassConstructor_15()
			{
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				Variant val = new Variant(string.Empty);
				if ((param.Length > 0) ? param[0].IsVoid() != true : false)
				{
					val.CopyRef(param[0]);
				}
				string message_name = "message";
				objthis.PropSet(Interface.MEMBERENSURE, message_name, val, objthis);
				if ((param.Length > 1) ? param[1].IsVoid() != true : false)
				{
					val.CopyRef(param[1]);
				}
				else
				{
					val.Set(string.Empty);
				}
				string trace_name = "trace";
				objthis.PropSet(Interface.MEMBERENSURE, trace_name, val, objthis);
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_38 : NativeClassMethod
		{
			public _NativeClassMethod_38()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}
	}
}

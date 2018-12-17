/*
 * TJS2 CSharp
 */

using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	public class DictionaryClass : NativeClass
	{
		public static int ClassID = -1;

		private static readonly string CLASS_NAME = "Dictionary";

		protected internal override NativeInstance CreateNativeInstance()
		{
			return new DictionaryNI();
		}

		protected internal override Dispatch2 CreateBaseTJSObject()
		{
			return new DictionaryObject();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public DictionaryClass() : base(CLASS_NAME)
		{
		    ClassID = Tjs.FindNativeClassId(CLASS_NAME);
            // constructor
            RegisterNCM(CLASS_NAME, new _NativeClassConstructor_24(), CLASS_NAME, Interface.nitMethod
				, Interface.STATICMEMBER);
			RegisterNCM("load", new _NativeClassMethod_36(), CLASS_NAME, Interface.nitMethod, 
				Interface.STATICMEMBER);
			// OribinalTODO: implement Dictionary.load()
			RegisterNCM("save", new _NativeClassMethod_47(), CLASS_NAME, Interface.nitMethod, 
				Interface.STATICMEMBER);
			// OribinalTODO: implement Dictionary.save()
			RegisterNCM("saveStruct", new _NativeClassMethod_58(), CLASS_NAME, Interface.nitMethod
				, Interface.STATICMEMBER);
			// Structured output for flie;
			// the content can be interpret as an expression to re-construct the object.
			RegisterNCM("assign", new _NativeClassMethod_85(), CLASS_NAME, Interface.nitMethod
				, Interface.STATICMEMBER);
			RegisterNCM("assignStruct", new _NativeClassMethod_106(), CLASS_NAME, Interface.nitMethod
				, Interface.STATICMEMBER);
			RegisterNCM("clear", new _NativeClassMethod_125(), CLASS_NAME, Interface.nitMethod
				, Interface.STATICMEMBER);
		}

		private sealed class _NativeClassConstructor_24 : NativeClassConstructor
		{
			public _NativeClassConstructor_24()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI _this = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				int hr = _this.Construct(param, objthis);
				if (hr < 0)
				{
					return hr;
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_36 : NativeClassMethod
		{
			public _NativeClassMethod_36()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_47 : NativeClassMethod
		{
			public _NativeClassMethod_47()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_58 : NativeClassMethod
		{
			public _NativeClassMethod_58()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string name = param[0].AsString();
				string mode = null;
				if (param.Length >= 2 && param[1].IsVoid() != true)
				{
					mode = param[1].AsString();
				}
				TextWriteStreamInterface stream = Tjs.mStorage.CreateTextWriteStream(name, mode);
				try
				{
					AList<Dispatch2> stack = new AList<Dispatch2>();
					stack.AddItem(objthis);
					ni.SaveStructuredData(stack, stream, string.Empty);
				}
				finally
				{
					stream.Destruct();
				}
				if (result != null)
				{
					result.Set(new Variant(objthis, objthis));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_85 : NativeClassMethod
		{
			public _NativeClassMethod_85()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				bool clear = true;
				if (param.Length >= 2 && param[1].IsVoid() != true)
				{
					clear = param[1].AsBoolean();
				}
				VariantClosure clo = param[0].AsObjectClosure();
				if (clo.mObjThis != null)
				{
					ni.Assign(clo.mObjThis, clear);
				}
				else
				{
					if (clo.mObject != null)
					{
						ni.Assign(clo.mObject, clear);
					}
					else
					{
						throw new TjsException(Error.NullAccess);
					}
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_106 : NativeClassMethod
		{
			public _NativeClassMethod_106()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				AList<Dispatch2> stack = new AList<Dispatch2>();
				VariantClosure clo = param[0].AsObjectClosure();
				if (clo.mObjThis != null)
				{
					ni.AssignStructure(clo.mObjThis, stack);
				}
				else
				{
					if (clo.mObject != null)
					{
						ni.AssignStructure(clo.mObject, stack);
					}
					else
					{
						throw new TjsException(Error.NullAccess);
					}
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_125 : NativeClassMethod
		{
			public _NativeClassMethod_125()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DictionaryNI ni = (DictionaryNI)objthis.GetNativeInstance(DictionaryClass
					.ClassID);
				if (ni == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (!ni.IsValid())
				{
					return Error.E_INVALIDOBJECT;
				}
				ni.Clear();
				return Error.S_OK;
			}
		}
	}
}

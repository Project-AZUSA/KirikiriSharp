/*
 * TJS2 CSharp
 */

using System;
using Tjs2.Engine;

namespace Tjs2.NativeApi
{
	public class NativeClass : CustomObject
	{
		public int ClassId { get; }

		public string ClassName { get; }

		private Variant mWorkParam;

		internal class Callback : Dispatch
		{
			public Dispatch2 mDest;

			//private Callback mCallback;
			//private VariantClosure mCallbackClosure;
			// a class to receive member callback from class
			// destination object
			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			public override int FuncCall(int flag, string membername, Variant result, Variant
				[] param, Dispatch2 objthis)
			{
				// *param[0] = name   *param[1] = flags   *param[2] = value
				int flags = param[1].AsInteger();
				if ((flags & Interface.STATICMEMBER) == 0)
				{
					Variant val = new Variant(param[2]);
					if (val.IsObject())
					{
						// change object's objthis if the object's objthis is null
						if (val.AsObjectThis() == null)
						{
							val.ChangeClosureObjThis(mDest);
						}
					}
					mDest.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP | flags, param[0].AsString
						(), val, mDest);
				}
				if (result != null)
				{
					result.Set(1);
				}
				// returns true
				return Error.S_OK;
			}
		}

		public NativeClass(string name) : base()
		{
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "Must provide a class name.");
			mCallFinalize = false;
			ClassName = Tjs.MapGlobalStringMap(name);
            ClassId = Tjs.RegisterNativeClass(name);
            mWorkParam = new Variant();
		}

		//mCallback = new Callback();
		//mCallbackClosure = new VariantClosure(mCallback,null);
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void RegisterNCM(string name, Dispatch2 dsp, string className, int
			 type)
		{
			RegisterNCM(name, dsp, className, type, 0);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void RegisterNCM(string name, Dispatch2 dsp, string className, int
			 type, int flags)
		{
			string tname = Tjs.MapGlobalStringMap(name);
			// add to this
			//Variant val = new Variant(dsp);
			//propSet( (Interface.MEMBERENSURE | Interface.IGNOREPROP) | flags, tname, val, this);
			mWorkParam.Set(dsp);
			try
			{
				PropSet((Interface.MEMBERENSURE | Interface.IGNOREPROP) | flags, tname, mWorkParam
					, this);
			}
			finally
			{
				mWorkParam.Clear();
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		protected internal override void FinalizeObject()
		{
			base.FinalizeObject();
		}

		protected internal virtual Dispatch2 CreateBaseTJSObject()
		{
			return new CustomObject();
		}

		/// <exception cref="TjsException"></exception>
		protected internal virtual NativeInstance CreateNativeInstance()
		{
			return null;
		}

		public string GetClassName()
		{
			return ClassName;
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
			if (membername != null)
			{
				return base.FuncCall(flag, membername, result, param, objthis);
			}
			// 生成を高速化するためにメンバコピーを特别处理する形で实装
			objthis.AddClassInstanceInfo(ClassName);
			NativeInstance nativeptr = CreateNativeInstance();
			objthis.SetNativeInstance(ClassId, nativeptr);
			int hr = CopyAllMembers((CustomObject)objthis);
			if (hr < 0)
			{
				return hr;
			}
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int CreateNew(int flag, string membername, Holder<Dispatch2> result
			, Variant[] param, Dispatch2 objthis)
		{
			// CreateNew
			Dispatch2 dsp = CreateBaseTJSObject();
			// instance initialization
			//int hr = funcCall( 0, null, null, null, dsp); // add member to dsp
			// 生成を高速化するためにメンバコピーを特别处理する形で实装
			dsp.AddClassInstanceInfo(ClassName);
			NativeInstance nativeptr = CreateNativeInstance();
			dsp.SetNativeInstance(ClassId, nativeptr);
			int hr = CopyAllMembers((CustomObject)dsp);
			if (hr < 0)
			{
				return hr;
			}
			hr = base.FuncCall(0, ClassName, null, param, dsp);
			// call constructor
			// call the constructor
			if (hr == Error.E_MEMBERNOTFOUND)
			{
				hr = Error.S_OK;
			}
			// missing constructor is OK ( is this ugly ? )
			if (hr >= 0)
			{
				result.Set(dsp);
			}
			return hr;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOf(int flag, string membername, string classname, Dispatch2
			 objthis)
		{
			if (membername == null)
			{
				if ("Class".Equals(classname))
				{
					return Error.S_TRUE;
				}
				if (ClassName != null && ClassName.Equals(classname))
				{
					return Error.S_TRUE;
				}
			}
			return base.IsInstanceOf(flag, membername, classname, objthis);
		}
	}
}

/*
 * TJS2 CSharp
 */

using System;
using System.Reflection;
using Tjs2.Engine;

namespace Tjs2.NativeApi.Java
{
	public class NativeJavaClassMethod : Dispatch
	{
		private MethodInfo mProcess;

		private int mClassID;

		private Type[] mParamTypes;

		private Type mReturnType;

		private bool mIsStatic;

		/// <exception cref="TjsException"></exception>
		public NativeJavaClassMethod(MethodInfo m, int classID)
		{
			mProcess = m;
			mClassID = classID;
			if (m == null)
			{
				throw new TjsException(Error.InternalError);
			}
			mParamTypes = Sharpen.Runtime.GetParameterTypes(m);
			mReturnType = m.ReturnType;
            if (m.IsStatic)
			{
				mIsStatic = true;
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOf(int flag, string membername, string classname, Dispatch2
			 objthis)
		{
			if (membername == null)
			{
				if ("Function".Equals(classname))
				{
					return Error.S_TRUE;
				}
			}
			int ret = base.IsInstanceOf(flag, membername, classname, objthis);
			return ret;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int FuncCall(int flag, string membername, Variant result, Variant
			[] param, Dispatch2 objthis)
		{
			if (membername != null)
			{
				return base.FuncCall(flag, membername, result, param, objthis);
			}
			if (param.Length < mParamTypes.Length)
			{
				return Error.E_INVALIDPARAM;
			}
			// パラメータが少ない
			if (result != null)
			{
				result.Clear();
			}
			object self;
			if (mIsStatic)
			{
				self = null;
			}
			else
			{
				// static 时は null
				if (objthis == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				NativeJavaInstance ni = (NativeJavaInstance)objthis.GetNativeInstance(mClassID);
				if (ni == null)
				{
					return Error.E_FAIL;
				}
				self = ni.GetNativeObject();
				if (self == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
			}
			int er = Error.S_OK;
			object[] args = NativeJavaClass.VariantArrayToJavaObjectArray(param, mParamTypes);
			try
			{
				object ret = mProcess.Invoke(self, args);
				if (result != null)
				{
					NativeJavaClass.JavaObjectToVariant(result, mReturnType, ret);
				}
			}
			catch (ArgumentException)
			{
				er = Error.E_INVALIDPARAM;
			}
			catch (MemberAccessException)
			{
				er = Error.E_ACCESSDENIED;
			}
			catch (TargetInvocationException e)
			{
				Exception t = e.InnerException;
				if (t is VariantException)
				{
					throw (VariantException)t;
				}
				else
				{
					if (t is TjsException)
					{
						throw (TjsException)t;
					}
					else
					{
						throw new TjsException(t.ToString());
					}
				}
			}
			return er;
		}
	}
}

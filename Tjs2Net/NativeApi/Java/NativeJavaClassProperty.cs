/*
 * TJS2 CSharp
 */

using System;
using System.Reflection;
using Tjs2.Engine;

namespace Tjs2.NativeApi.Java
{
	public class NativeJavaClassProperty : Dispatch
	{
		private MethodInfo mGet;

		private MethodInfo mSet;

		private Type mParamType;

		private Type mReturnType;

		private bool mIsStaticGet;

		private bool mIsStaticSet;

		private int mClassID;

		/// <exception cref="TjsException"></exception>
		public NativeJavaClassProperty(MethodInfo get, MethodInfo set, int classID)
		{
			mClassID = classID;
			mGet = get;
			if (get != null)
			{
				mReturnType = get.ReturnType;
				if (mReturnType.Equals(typeof(void)))
				{
					throw new TjsException(Error.InternalError);
				}
				if (get.IsStatic)
				{
					mIsStaticGet = true;
				}
			}
			mSet = set;
			if (set != null)
			{
				Type[] @params = Sharpen.Runtime.GetParameterTypes(set);
				if (@params.Length == 1)
				{
					mParamType = @params[0];
				}
				else
				{
					throw new TjsException(Error.InternalError);
				}
				if (set.IsStatic)
				{
					mIsStaticSet = true;
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int IsInstanceOf(int flag, string membername, string classname, Dispatch2
			 objthis)
		{
			if (membername == null)
			{
				if ("Property".Equals(classname))
				{
					return Error.S_TRUE;
				}
			}
			return base.IsInstanceOf(flag, membername, classname, objthis);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropGet(int flag, string membername, Variant result, Dispatch2
			 objthis)
		{
			if (membername != null)
			{
				return base.PropGet(flag, membername, result, objthis);
			}
			if (result == null)
			{
				return Error.E_FAIL;
			}
			if (mGet == null)
			{
				return Error.E_ACCESSDENIED;
			}
			object self;
			if (mIsStaticGet)
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
			try
			{
				object ret = mGet.Invoke(self, new object[]{});
				NativeJavaClass.JavaObjectToVariant(result, mReturnType, ret);
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

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override int PropSet(int flag, string membername, Variant param, Dispatch2
			 objthis)
		{
			if (membername != null)
			{
				return base.PropSet(flag, membername, param, objthis);
			}
			if (objthis == null)
			{
				return Error.E_NATIVECLASSCRASH;
			}
			if (param == null)
			{
				return Error.E_FAIL;
			}
			if (mSet == null)
			{
				return Error.E_ACCESSDENIED;
			}
			object self;
			if (mIsStaticSet)
			{
				self = null;
			}
			else
			{
				// static 时は null
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
			object arg = NativeJavaClass.VariantToJavaObject(param, mParamType);
			if (arg == null)
			{
				return Error.E_INVALIDPARAM;
			}
			object[] args = new object[1];
			args[0] = arg;
			try
			{
				mSet.Invoke(self, args);
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

/*
 * TJS2 CSharp
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Java
{
	public class NativeJavaClass : NativeClass
	{
		private Type mJavaClass;

		private int mClassID;

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public NativeJavaClass(string name, Type c) : base(name)
		{
			mJavaClass = c;
			string classname = name;
			mClassID = Tjs.RegisterNativeClass(classname);
			try
			{
				HashSet<string> registProp = new HashSet<string>();
				// set/getで重复しないようにチェック
				MethodInfo[] methods = c.GetMethods();
				foreach (MethodInfo m in methods)
				{
					string methodName = m.Name;
					int flag = 0;
					if (m.IsStatic)
					{
						flag |= Interface.STATICMEMBER;
					}
					if ("constructor".Equals(methodName))
					{
						// コンストラクタ
						RegisterNCM(classname, new NativeJavaClassConstructor(m, mClassID), classname, Interface
							.nitMethod, flag);
					}
					else
					{
						if (methodName.StartsWith("prop_"))
						{
							// プロパティ prop_ で始まるものはプロパティとみなす
							Type[] @params = Runtime.GetParameterTypes(m);
							MethodInfo setMethod = null;
							MethodInfo getMethod = null;
							string propName = null;
							if (methodName.StartsWith("prop_set_"))
							{
								if (@params.Length == 1)
								{
									setMethod = m;
									propName = Runtime.Substring(methodName, "prop_set_".Length);
									if (registProp.Contains(propName) == false)
									{
										string getMethodName = "prop_get_" + propName;
										foreach (MethodInfo getm in methods)
										{
											if (getm.Name.Equals(getMethodName))
											{
												Type[] p = Runtime.GetParameterTypes(getm);
												if (p.Length == 0 && getm.ReturnType == typeof(void) != true)
												{
													getMethod = getm;
													break;
												}
											}
										}
									}
								}
							}
							else
							{
								if (methodName.StartsWith("prop_get_"))
								{
									if (@params.Length == 0 && m.ReturnType == typeof(void) != true)
									{
										getMethod = m;
										propName = Runtime.Substring(methodName, "prop_get_".Length);
										if (registProp.Contains(propName) == false)
										{
											string setMethodName = "prop_set_" + propName;
											foreach (MethodInfo setm in methods)
											{
												if (setm.Name.Equals(setMethodName))
												{
													Type[] p = Runtime.GetParameterTypes(setm);
													if (p.Length == 1)
													{
														setMethod = setm;
														break;
													}
												}
											}
										}
									}
								}
							}
							if (propName != null && registProp.Contains(propName) == false)
							{
								if (setMethod != null || getMethod != null)
								{
									RegisterNCM(propName, new NativeJavaClassProperty(getMethod, setMethod, mClassID)
										, classname, Interface.nitProperty, flag);
									registProp.AddItem(propName);
								}
							}
						}
						else
						{
							// 通常メソッド
							RegisterNCM(methodName, new NativeJavaClassMethod(m, mClassID), classname, Interface
								.nitMethod, flag);
						}
					}
				}
				registProp = null;
			}
			catch (SecurityException e)
			{
				throw new TjsException(Error.InternalError + e);
			}
		}

		/// <summary>
		/// 引数があるコンストラクタには未对应
		/// TODO エラー时エラー表示するようにした方がいいかも
		/// </summary>
		protected internal override NativeInstance CreateNativeInstance()
		{
			object obj;
			try
			{
				obj = Activator.CreateInstance(mJavaClass);
			}
			catch (InstantiationException e)
			{
				Tjs.OutputExceptionToConsole(e.ToString());
				return null;
			}
			catch (MemberAccessException e)
			{
				Tjs.OutputExceptionToConsole(e.ToString());
				return null;
			}
			if (obj != null)
			{
				return new NativeJavaInstance(obj);
			}
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public static object VariantToJavaObject(Variant param, Type type)
		{
			if (type.GetTypeInfo().IsPrimitive)
			{
				// プリミティブタイプの场合
			    if (type == typeof(int))
			        return Extensions.ValueOf(param.AsInteger());
			    if (type == typeof(double))
			        return param.AsDouble();
			    if (type == typeof(bool))
			        return Extensions.ValueOf(param.AsInteger() != 0);
			    if (type == typeof(float))
			        return (float) param.AsDouble();
			    if (type == typeof(long))
			        return Extensions.ValueOf(param.AsInteger());
			    if (type == typeof(char))
			        return (char) param.AsInteger();
			    if (type == typeof(byte))
			        return unchecked((byte) param.AsInteger());
			    if (type == typeof(short))
			        return (short) param.AsInteger();
			    // may be Void.TYPE
			    return null;
			}
		    if (type == typeof(string))
		        return param.AsString();
		    if (type == typeof(ByteBuffer))
		        return param.AsOctet();
		    if (type == typeof(Variant))
		        return param;
		    if (type == typeof(VariantClosure))
		        return param.AsObjectClosure();
		    if (type == typeof(Dispatch2))
		        return param.AsObject();
		    if (type == param.ToJavaObject().GetType())
		        return param.ToJavaObject();
		    // その他 のクラス
		    return null;
		}

		public static void JavaObjectToVariant(Variant result, Type type, object src)
		{
			if (result == null)
			{
				return;
			}
			if (type.GetTypeInfo().IsPrimitive)
			{
				// プリミティブタイプの场合
			    if (type == typeof(int))
			        result.Set(((int) src));
			    else if (type == typeof(double))
			        result.Set(((double) src));
			    else if (type == typeof(bool))
			        result.Set(((bool) src) ? 1 : 0);
			    else if (type == typeof(float))
			        result.Set(((float) src));
			    else if (type == typeof(long))
			        result.Set(((long) src));
			    else if (type == typeof(char))
			        result.Set((char) src);
			    else if (type == typeof(byte))
			        result.Set(((byte) src));
			    else if (type == typeof(short))
			        result.Set(((short) src));
			    else
			    // may be Void.TYPE
			        result.Clear();
			}
			else
			{
			    if (type == typeof(string))
			        result.Set((string) src);
			    else if (type == typeof(ByteBuffer))
			        result.Set((ByteBuffer) src);
			    else if (type == typeof(Variant))
			        result.Set((Variant) src);
			    else if (type == typeof(VariantClosure))
			        result.Set(((VariantClosure) src).mObject, ((VariantClosure) src).mObjThis);
			    else if (type == typeof(Dispatch2))
			        result.Set((Dispatch2) src);
			    else
			    // その他 のクラス, 直接入れてしまう
			        result.SetJavaObject(src);
			}
		}

		/// <exception cref="VariantException"></exception>
		public static object[] VariantArrayToJavaObjectArray(Variant[] @params, Type[] types)
		{
			if (types.Length == 0)
			{
				return null;
			}
			// 元々引数不要
			if (@params.Length < types.Length)
			{
				return null;
			}
			// パラメータが少ない
			int count = types.Length;
			object[] ret = new object[count];
			for (int i = 0; i < count; i++)
			{
				Type type = types[i];
				Variant param = @params[i];
			    if (type.GetTypeInfo().IsPrimitive)
			    {
			        // プリミティブタイプの场合
			        if (type == typeof(int))
			            ret[i] = Extensions.ValueOf(param.AsInteger());
			        else if (type == typeof(double))
			            ret[i] = (param.AsDouble());
			        else if (type == typeof(bool))
			            ret[i] = Extensions.ValueOf(param.AsInteger() != 0);
			        else if (type == typeof(float))
			            ret[i] = ((float) param.AsDouble());
			        else if (type == typeof(long))
			            ret[i] = Extensions.ValueOf(param.AsInteger());
			        else if (type == typeof(char))
			            ret[i] = ((char) param.AsInteger());
			        else if (type == typeof(byte))
			            ret[i] = (unchecked((byte) param.AsInteger()));
			        else if (type == typeof(short))
			            ret[i] = ((short) param.AsInteger());
			        else
			        // may be Void.TYPE
			            ret[i] = null;
			    }
			    else if (type == typeof(string))
			        ret[i] = param.AsString();
			    else if (type == typeof(ByteBuffer))
			        ret[i] = param.AsOctet();
			    else if (type == typeof(Variant))
			        ret[i] = param;
			    else if (type == typeof(VariantClosure))
			        ret[i] = param.AsObjectClosure();
			    else if (type == typeof(Dispatch2))
			        ret[i] = param.AsObject();
			    else if (type == param.ToJavaObject().GetType())
			        ret[i] = param.ToJavaObject();
			    else
			    // その他 のクラス
			        ret[i] = null;
			}
			return ret;
		}
	}
}

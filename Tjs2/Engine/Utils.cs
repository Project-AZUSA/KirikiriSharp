/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.NativeApi;

namespace Tjs2.Engine
{
	public static class Utils
	{
        public static string ToRealString(this char[] chars)
        {
            return new string(chars);
        }

        /// <exception cref="VariantException"></exception>
        public static string VariantToReadableString(Variant val)
		{
			return VariantToReadableString(val, 512);
		}

		/// <exception cref="VariantException"></exception>
		public static string VariantToReadableString(Variant val, int maxlen)
		{
			string ret = null;
			if (val == null || val.IsVoid())
			{
				ret = "(void)";
			}
			else
			{
				if (val.IsInteger())
				{
					ret = "(int)" + val.AsString();
				}
				else
				{
					if (val.IsReal())
					{
						ret = "(real)" + val.AsString();
					}
					else
					{
						if (val.IsString())
						{
							ret = "(string)\"" + LexBase.EscapeC(val.AsString()) + "\"";
						}
						else
						{
							if (val.IsOctet())
							{
								ret = "(octet)<% " + Variant.OctetToListString(val.AsOctet()) + " %>";
							}
							else
							{
								if (val.IsObject())
								{
									VariantClosure c = (VariantClosure)val.AsObjectClosure();
									StringBuilder str = new StringBuilder(128);
									str.Append("(object)");
									str.Append('(');
									if (c.mObject != null)
									{
										str.Append('[');
										if (c.mObject is NativeClass)
										{
											str.Append(((NativeClass)c.mObject).GetClassName());
										}
										else
										{
											if (c.mObject is InterCodeObject)
											{
												str.Append(((InterCodeObject)c.mObject).GetName());
											}
											else
											{
												if (c.mObject is CustomObject)
												{
													string name = ((CustomObject)c.mObject).GetClassNames();
													if (name != null)
													{
														str.Append(name);
													}
													else
													{
														str.Append(c.mObject.GetType().FullName);
													}
												}
												else
												{
													str.Append(c.mObject.GetType().FullName);
												}
											}
										}
										str.Append(']');
									}
									else
									{
										str.Append("0x00000000");
									}
									if (c.mObjThis != null)
									{
										str.Append('[');
										if (c.mObjThis is NativeClass)
										{
											str.Append(((NativeClass)c.mObjThis).GetClassName());
										}
										else
										{
											if (c.mObjThis is InterCodeObject)
											{
												str.Append(((InterCodeObject)c.mObjThis).GetName());
											}
											else
											{
												if (c.mObjThis is CustomObject)
												{
													string name = ((CustomObject)c.mObjThis).GetClassNames();
													if (name != null)
													{
														str.Append(name);
													}
													else
													{
														str.Append(c.mObjThis.GetType().FullName);
													}
												}
												else
												{
													str.Append(c.mObjThis.GetType().FullName);
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
									ret = str.ToString();
								}
								else
								{
									// native object ?
									ret = "(octet) [" + val.GetType().FullName + "]";
								}
							}
						}
					}
				}
			}
			if (ret != null)
			{
				if (ret.Length > maxlen)
				{
					return Sharpen.Runtime.Substring(ret, 0, maxlen);
				}
				else
				{
					return ret;
				}
			}
			return string.Empty;
		}

		/// <exception cref="VariantException"></exception>
		public static string VariantToExpressionString(Variant val)
		{
			// convert given variant to string which can be interpret as an expression.
			// this function does not convert objects ( returns empty string )
			if (val.IsVoid())
			{
				return "void";
			}
			else
			{
				if (val.IsInteger())
				{
					return val.AsString();
				}
				else
				{
					if (val.IsReal())
					{
						string s = Variant.RealToHexString(val.AsDouble());
						return s + " /* " + val.AsString() + " */";
					}
					else
					{
						if (val.IsString())
						{
							string s = LexBase.EscapeC(val.AsString());
							return "\"" + s + "\"";
						}
						else
						{
							if (val.IsOctet())
							{
								string s = Variant.OctetToListString(val.AsOctet());
								return "<%" + s + "%>";
							}
							else
							{
								return string.Empty;
							}
						}
					}
				}
			}
		}

		public static string FormatString(string format, Variant[] @params)
		{
			int count = @params.Length;
			object[] args = new object[count];
			for (int i = 0; i < count; i++)
			{
				args[i] = @params[i].ToJavaObject();
				if (args[i] is string)
				{
					int length = ((string)args[i]).Length;
					if (length == 1)
					{
						args[i] = ((string)args[i])[0];
					}
				}
			}
			return string.Format(format, args);
		}
	}
}

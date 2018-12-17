/*
 * TJS2 CSharp
 */

using System.Reflection;
using System.Text;
using Tjs2.Sharpen;
using Double = Tjs2.Sharpen.Double;

namespace Tjs2.Engine
{
	public class Variant 
	{
		public static void Initialize()
		{
			NullVariantClosure = new VariantClosure(null, null);
		}

		public static void FinalizeApplication()
		{
			NullVariantClosure = null;
		}

		private static VariantClosure NullVariantClosure;

		public const int VOID = 0;

		public const int OBJECT = 1;

		public const int STRING = 2;

		public const int OCTET = 3;

		public const int INTEGER = 4;

		public const int REAL = 5;

		public static readonly string TYPE_VOID = "void";

		public static readonly string TYPE_INTEGER = "int";

		public static readonly string TYPE_REAL = "real";

		public static readonly string TYPE_STRING = "string";

		public static readonly string TYPE_OCTET = "octet";

		public static readonly string TYPE_OBJECT = "object";

		private object mObject;

		public Variant()
		{
		}

		public Variant(int value)
		{
			// empty
			// octet binary data
			//mObject = null; // 初期值なので不要
			mObject = Sharpen.Extensions.ValueOf(value);
		}

		public Variant(double value)
		{
			mObject = (value);
		}

		public Variant(ByteBuffer value)
		{
			mObject = value;
		}

		public Variant(string value)
		{
			mObject = value;
		}

		public Variant(Variant value)
		{
			mObject = value.CloneSeparate();
		}

		public Variant(object value)
		{
			mObject = value;
		}

		public Variant(Dispatch2 dsp, Dispatch2 dsp2)
		{
			mObject = new VariantClosure(dsp, dsp2);
		}

		public Variant(Dispatch2 dsp)
		{
			mObject = new VariantClosure(dsp, null);
		}

		public void Set(int value)
		{
			mObject = Sharpen.Extensions.ValueOf(value);
		}

		public void Set(double value)
		{
			mObject = (value);
		}

		public void Set(ByteBuffer value)
		{
			mObject = value;
		}

		public void Set(string value)
		{
			mObject = value;
		}

		public void SetJavaObject(object value)
		{
			mObject = value;
		}

		public void Set(Variant value)
		{
			//mObject = value.mObject;
			CopyRef(value);
		}

		public void Set(Dispatch2 dsp, Dispatch2 dsp2)
		{
			mObject = new VariantClosure(dsp, dsp2);
		}

		public void Set(Dispatch2 dsp)
		{
			mObject = new VariantClosure(dsp);
		}

		public void CopyRef(Variant value)
		{
			if (value.mObject is VariantClosure)
			{
				VariantClosure clo = (VariantClosure)value.mObject;
				mObject = new VariantClosure(clo.mObject, clo.mObjThis);
			}
			else
			{
				if (mObject != value.mObject)
				{
					{
						mObject = value.mObject;
					}
				}
			}
		}

		// 一部オブジェクトは参照コピー
		public object CloneSeparate()
		{
			if (mObject is VariantClosure)
			{
				VariantClosure clo = (VariantClosure)mObject;
				return new VariantClosure(clo.mObject, clo.mObjThis);
			}
			else
			{
				return mObject;
			}
		}

		public static string OctetToListString(ByteBuffer oct)
		{
			int size = oct.Capacity();
			string hex = "0123456789ABCDEF";
			StringBuilder str = new StringBuilder(size * 3);
			for (int i = 0; i < size; i++)
			{
				byte b = oct.Get(i);
				str.Append(hex[(b >> 4) & unchecked((int)(0x0f))]);
				str.Append(hex[b & unchecked((int)(0x0f))]);
				if (i != (size - 1))
				{
					str.Append(' ');
				}
			}
			return str.ToString();
		}

		// ~ ビット单位NOT
		/// <exception cref="VariantException"></exception>
		public Variant GetBitNotValue()
		{
			int val = AsInteger();
			return new Variant(~val);
		}

		// ! 论理否定
		public Variant GetNotValue()
		{
			bool val = !AsBoolean();
			return new Variant(val ? 1 : 0);
		}

		// ||
		public Variant LogicOr(Variant val)
		{
			bool v = AsBoolean() || val.AsBoolean();
			return new Variant(v ? 1 : 0);
		}

		// &&
		public Variant LogicAnd(Variant val)
		{
			bool v = AsBoolean() && val.AsBoolean();
			return new Variant(v ? 1 : 0);
		}

		// |
		/// <exception cref="VariantException"></exception>
		public Variant BitOr(Variant val)
		{
			int v = AsInteger() | val.AsInteger();
			return new Variant(v);
		}

		// ^
		/// <exception cref="VariantException"></exception>
		public Variant BitXor(Variant val)
		{
			int v = AsInteger() ^ val.AsInteger();
			return new Variant(v);
		}

		// &
		/// <exception cref="VariantException"></exception>
		public Variant BitAnd(Variant val)
		{
			int v = AsInteger() & val.AsInteger();
			return new Variant(v);
		}

		// !=
		/// <exception cref="VariantException"></exception>
		public Variant NotEqual(Variant val)
		{
			bool v = NormalCompare(val);
			return new Variant((!v) ? 1 : 0);
		}

		// ==
		/// <exception cref="VariantException"></exception>
		public Variant EqualEqual(Variant val)
		{
			bool v = NormalCompare(val);
			return new Variant(v ? 1 : 0);
		}

		// !==
		public Variant DiscNotEqual(Variant val)
		{
			bool v = DiscernCompareInternal(val);
			return new Variant((!v) ? 1 : 0);
		}

		// ===
		public Variant DiscernCompare(Variant val)
		{
			bool v = DiscernCompareInternal(val);
			return new Variant(v ? 1 : 0);
		}

		// <
		/// <exception cref="VariantException"></exception>
		public Variant Lt(Variant val)
		{
			bool v = GreaterThan(val);
			// なんか逆转してない？ 元の实装がそうだけど？
			return new Variant(v ? 1 : 0);
		}

		// >
		/// <exception cref="VariantException"></exception>
		public Variant Gt(Variant val)
		{
			bool v = LittlerThan(val);
			// なんか逆转してない？ 元の实装がそうだけど？
			return new Variant(v ? 1 : 0);
		}

		// <=
		/// <exception cref="VariantException"></exception>
		public Variant LtOrEqual(Variant val)
		{
			bool v = LittlerThan(val);
			return new Variant((!v) ? 1 : 0);
		}

		// >=
		/// <exception cref="VariantException"></exception>
		public Variant GtOrEqual(Variant val)
		{
			bool v = GreaterThan(val);
			return new Variant((!v) ? 1 : 0);
		}

		// >>
		/// <exception cref="VariantException"></exception>
		public Variant RightShift(Variant val)
		{
			int v = AsInteger() >> val.AsInteger();
			return new Variant(v);
		}

		// <<
		/// <exception cref="VariantException"></exception>
		public Variant LeftShift(Variant val)
		{
			int v = AsInteger() << val.AsInteger();
			return new Variant(v);
		}

		// >>>
		/// <exception cref="VariantException"></exception>
		public Variant RightBitShift(Variant val)
		{
			int v = AsInteger();
			v = (int)(((uint)v) >> val.AsInteger());
			return new Variant((int)v);
		}

		// +
		/// <exception cref="VariantException"></exception>
		public Variant Add(Variant val)
		{
			if (mObject is string || val.mObject is string)
			{
				string s1 = AsString();
				string s2 = val.AsString();
				if (s1 != null && s2 != null)
				{
					StringBuilder builder = new StringBuilder(256);
					builder.Append(s1);
					builder.Append(s2);
					return new Variant(builder.ToString());
				}
				else
				{
					if (s1 != null)
					{
						return new Variant(s1);
					}
					else
					{
						return new Variant(s2);
					}
				}
			}
			if (mObject != null && val.mObject != null)
			{
				if (mObject.GetType().IsInstanceOfType(val.mObject))
				{
					// 同じクラス
					if (mObject is ByteBuffer)
					{
						ByteBuffer b1 = (ByteBuffer)mObject;
						ByteBuffer b2 = (ByteBuffer)val.mObject;
						ByteBuffer result = ByteBuffer.Allocate(b1.Capacity() + b2.Capacity());
						b1.Position(0);
						b2.Position(0);
						result.Put(b1);
						result.Put(b2);
						result.Position(0);
						return new Variant(result);
					}
					if (mObject is int)
					{
						int result = ((int)mObject) + ((int)val.mObject);
						return new Variant(result);
					}
				}
			}
			if (mObject == null)
			{
				if (val.mObject != null)
				{
					if (val.mObject is int)
					{
						return new Variant(((int)val.mObject));
					}
					else
					{
						if (val.mObject is double)
						{
							return new Variant(((double)val.mObject));
						}
					}
				}
			}
			if (val.mObject == null)
			{
				if (mObject != null)
				{
					if (mObject is int)
					{
						return new Variant(((int)mObject));
					}
					else
					{
						if (mObject is double)
						{
							return new Variant(((double)mObject));
						}
					}
				}
			}
			return new Variant(AsDouble() + val.AsDouble());
		}

		// -
		/// <exception cref="VariantException"></exception>
		public Variant Subtract(Variant val)
		{
			if (mObject is int && val.mObject is int)
			{
				int result = ((int)mObject) - ((int)val.mObject);
				return new Variant(result);
			}
			Number n1 = AsNumber();
			Number n2 = val.AsNumber();
			if (n1.IsInt() && n2.IsInt())
			{
				int result = n1 - n2;
				return new Variant(result);
			}
			else
			{
				double result = n1 - n2;
				return new Variant(result);
			}
		}

		// %
		/// <exception cref="VariantException"></exception>
		public Variant Residue(Variant val)
		{
			int r = val.AsInteger();
			if (r == 0)
			{
				ThrowDividedByZero();
			}
			int l = AsInteger();
			return new Variant(l % r);
		}

		// /
		/// <exception cref="VariantException"></exception>
		public Variant Divide(Variant val)
		{
			double l = AsDouble();
			double r = val.AsDouble();
			return new Variant(l / r);
		}

		// \
		/// <exception cref="VariantException"></exception>
		public Variant Idiv(Variant val)
		{
			int r = val.AsInteger();
			if (r == 0)
			{
				ThrowDividedByZero();
			}
			int l = AsInteger();
			return new Variant(l / r);
		}

		// *
		/// <exception cref="VariantException"></exception>
		public Variant Multiply(Variant val)
		{
			if (mObject == null || val.mObject == null)
			{
				return new Variant(0);
			}
			if ((mObject is int) && (val.mObject is int))
			{
				int result = ((int)mObject) * ((int)val.mObject);
				return new Variant(result);
			}
			Number n1 = AsNumber();
			Number n2 = val.AsNumber();
			if (n1.IsInt() && n2.IsInt())
			{
				int result = n1 * n2;
				return new Variant(result);
			}
			else
			{
				double result = n1 * n2;
				return new Variant(result);
			}
		}

		/// <exception cref="VariantException"></exception>
		public bool NormalCompare(Variant val2)
		{
			if (mObject != null && val2.mObject != null)
			{
				if (mObject.GetType().IsInstanceOfType(val2.mObject))
				{
					// 同じクラス
					if (mObject is int)
					{
						return ((int)mObject) == ((int)val2.mObject);
					}
					if (mObject is string)
					{
						return ((string)mObject).Equals(((string)val2.mObject));
					}
					if (mObject is ByteBuffer)
					{
						//return ((ByteBuffer)mObject).compareTo( ((ByteBuffer)val2.mObject) ) == 0;
						ByteBuffer v1 = (ByteBuffer)mObject;
						ByteBuffer v2 = (ByteBuffer)val2.mObject;
						int c1 = v1.Limit();
						int c2 = v2.Limit();
						if (c1 == c2)
						{
							for (int i = 0; i < c1; i++)
							{
								byte b1 = v1.Get(i);
								byte b2 = v2.Get(i);
								if (b1 != b2)
								{
									return false;
								}
							}
						}
						else
						{
							return false;
						}
						return true;
					}
					if (mObject is double)
					{
						return ((double)mObject) == ((double)val2.mObject);
					}
					return mObject.Equals(val2.mObject);
				}
				else
				{
					if (mObject is string || val2.mObject is string)
					{
						string v1 = AsString();
						string v2 = val2.AsString();
						return v1.Equals(v2);
					}
					else
					{
						if (mObject is Number && val2.mObject is Number)
						{
							double r1 = ((Number)mObject);
							double r2 = ((Number)val2.mObject);
							if (Double.IsNaN(r1) || Double.IsNaN(r2))
							{
								return false;
							}
							if (Double.IsInfinite(r1) || Double.IsInfinite(r2))
							{
								return Double.Compare(r1, r2) == 0;
							}
							return r1 == r2;
						}
						else
						{
							return false;
						}
					}
				}
			}
			else
			{
				// 片方はnull
				if (mObject == null && val2.mObject == null)
				{
					return true;
				}
				if (mObject == null)
				{
					if (val2.mObject is int)
					{
						return ((int)val2.mObject) == 0;
					}
					if (val2.mObject is double)
					{
						return ((double)val2.mObject) == 0.0;
					}
					if (val2.mObject is string)
					{
						return ((string)val2.mObject).Length == 0;
					}
					return false;
				}
				else
				{
					if (mObject is int)
					{
						return ((int)mObject) == 0;
					}
					if (mObject is double)
					{
						return ((double)mObject) == 0.0;
					}
					if (mObject is string)
					{
						return ((string)mObject).Length == 0;
					}
					return false;
				}
			}
		}

		public bool DiscernCompareInternal(Variant val)
		{
			if (mObject != null && val.mObject != null)
			{
				if ((mObject is int) && (val.mObject is int))
				{
					return ((int)mObject) == ((int)val.mObject);
				}
				else
				{
					if ((mObject is string) && (val.mObject is string))
					{
						return ((string)mObject).Equals(((string)val.mObject));
					}
					else
					{
						if ((mObject is double) && (val.mObject is double))
						{
							double r1 = ((Number)mObject);
							double r2 = ((Number)val.mObject);
							if (Double.IsNaN(r1) || Double.IsNaN(r2))
							{
								return false;
							}
							if (Double.IsInfinite(r1) || Double.IsInfinite(r2))
							{
								return Double.Compare(r1, r2) == 0;
							}
							return r1 == r2;
						}
						else
						{
							if ((mObject is VariantClosure) && (val.mObject is VariantClosure))
							{
								VariantClosure v1 = (VariantClosure)mObject;
								VariantClosure v2 = (VariantClosure)val.mObject;
								return (v1.mObject == v2.mObject && v1.mObjThis == v2.mObjThis);
							}
							else
							{
								if ((mObject is ByteBuffer) && (val.mObject is ByteBuffer))
								{
									//return ((ByteBuffer)mObject).compareTo( ((ByteBuffer)val.mObject) ) == 0;
									ByteBuffer v1 = (ByteBuffer)mObject;
									ByteBuffer v2 = (ByteBuffer)val.mObject;
									int c1 = v1.Limit();
									int c2 = v2.Limit();
									if (c1 == c2)
									{
										for (int i = 0; i < c1; i++)
										{
											byte b1 = v1.Get(i);
											byte b2 = v2.Get(i);
											if (b1 != b2)
											{
												return false;
											}
										}
									}
									else
									{
										return false;
									}
									return true;
								}
								else
								{
									if (mObject.GetType().IsInstanceOfType(val.mObject))
									{
										// 同じクラス
										return mObject.Equals(val.mObject);
									}
									else
									{
										return false;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (mObject == null && val.mObject == null)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public bool DiscernCompareStrictReal(Variant val)
		{
			if (mObject != null && val.mObject != null)
			{
				if ((mObject is double) && (val.mObject is double))
				{
					return ((double)mObject) == ((double)val.mObject);
				}
				else
				{
					if ((mObject is int) && (val.mObject is int))
					{
						return ((int)mObject) == ((int)val.mObject);
					}
					else
					{
						if ((mObject is string) && (val.mObject is string))
						{
							return ((string)mObject).Equals(((string)val.mObject));
						}
						else
						{
							if ((mObject is VariantClosure) && (val.mObject is VariantClosure))
							{
								VariantClosure v1 = (VariantClosure)mObject;
								VariantClosure v2 = (VariantClosure)val.mObject;
								return (v1.mObject == v2.mObject && v1.mObjThis == v2.mObjThis);
							}
							else
							{
								if ((mObject is ByteBuffer) && (val.mObject is ByteBuffer))
								{
									ByteBuffer v1 = (ByteBuffer)mObject;
									ByteBuffer v2 = (ByteBuffer)val.mObject;
									int c1 = v1.Limit();
									int c2 = v2.Limit();
									if (c1 == c2)
									{
										for (int i = 0; i < c1; i++)
										{
											byte b1 = v1.Get(i);
											byte b2 = v2.Get(i);
											if (b1 != b2)
											{
												return false;
											}
										}
									}
									else
									{
										return false;
									}
									return true;
								}
								else
								{
									if (mObject.GetType().IsInstanceOfType(val.mObject))
									{
										// 同じクラス
										return mObject.Equals(val.mObject);
									}
									else
									{
										return false;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				//return discernCompareInternal(val);
				if (mObject == null && val.mObject == null)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		//return normalCompare(val);
		/// <exception cref="VariantException"></exception>
		public bool GreaterThan(Variant val)
		{
			if ((mObject is string) == false || (val.mObject is string) == false)
			{
				if ((mObject is int) && (val.mObject is int))
				{
					return ((int)mObject) < ((int)val.mObject);
				}
				return AsDouble() < val.AsDouble();
			}
			string s1 = AsString();
			string s2 = val.AsString();
			return Sharpen.Runtime.CompareOrdinal(s1, s2) < 0;
		}

		//0：等しい。1：より大きい。-1：より小さい
		/// <exception cref="VariantException"></exception>
		public int GreaterThanForSort(Variant val)
		{
			if ((mObject is string) == false || (val.mObject is string) == false)
			{
				if ((mObject is int) && (val.mObject is int))
				{
					return ((int)mObject) - ((int)val.mObject);
				}
				double ret = (AsDouble() - val.AsDouble());
				if (ret == 0.0)
				{
					return 0;
				}
				else
				{
					if (ret < 0.0)
					{
						return -1;
					}
					else
					{
						return 1;
					}
				}
			}
			string s1 = AsString();
			string s2 = val.AsString();
			return Sharpen.Runtime.CompareOrdinal(s1, s2);
		}

		/// <exception cref="VariantException"></exception>
		public bool LittlerThan(Variant val)
		{
			if ((mObject is string) == false || (val.mObject is string) == false)
			{
				if ((mObject is int) && (val.mObject is int))
				{
					return ((int)mObject) > ((int)val.mObject);
				}
				return AsDouble() > val.AsDouble();
			}
			string s1 = AsString();
			string s2 = val.AsString();
			return Sharpen.Runtime.CompareOrdinal(s1, s2) > 0;
		}

		/// <exception cref="VariantException"></exception>
		public int LittlerThanForSort(Variant val)
		{
			if ((mObject is string) == false || (val.mObject is string) == false)
			{
				if ((mObject is int) && (val.mObject is int))
				{
					return ((int)val.mObject) - ((int)mObject);
				}
				double ret = val.AsDouble() - AsDouble();
				if (ret == 0.0)
				{
					return 0;
				}
				else
				{
					if (ret < 0.0)
					{
						return -1;
					}
					else
					{
						return 1;
					}
				}
			}
			string s1 = AsString();
			string s2 = val.AsString();
			return Sharpen.Runtime.CompareOrdinal(s2, s1);
		}

		/// <exception cref="VariantException"></exception>
		public void AsNumber(Variant targ)
		{
			if (mObject == null)
			{
				targ.Set(0);
			}
			else
			{
				if (mObject is Number)
				{
					// Integer or Double
					if (mObject is int)
					{
						targ.Set(((int)mObject));
					}
					else
					{
						targ.Set(((Number)mObject));
					}
				}
				else
				{
					if (mObject is string)
					{
						LexBase lex = new LexBase((string)mObject);
						Number num = lex.ParseNumber();
						if (num != null)
						{
							if (num.IsInt())
							{
								targ.Set(((int)num));
							}
							else
							{
								targ.Set(((Number)num));
							}
						}
						else
						{
							targ.Set(0);
						}
					}
					else
					{
						// convert error
						ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public Number AsNumber()
		{
			if (mObject is int)
			{
                return new Number((int)mObject);
			}
			else
			{
				if (mObject is double)
				{
					return new Number((double)mObject);
				}
				else
				{
					if (mObject is string)
					{
						LexBase lex = new LexBase((string)mObject);
						Number num = lex.ParseNumber();
						if (num != null)
						{
							return num;
						}
						else
						{
                            return new Number(0);
						}
					}
					else
					{
						if (mObject == null)
						{
                            return new Number(0);
						}
					}
				}
			}
			// convert error
			ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public void ChangeSign()
		{
			if (mObject is int)
			{
				mObject = Sharpen.Extensions.ValueOf(-((int)mObject));
				return;
			}
			Number val = AsNumber();
			if (val.IsInt())
			{
				mObject = Sharpen.Extensions.ValueOf(-val);
			}
			else
			{
				mObject = (-val);
			}
		}

		/// <exception cref="VariantException"></exception>
		public void ToNumber()
		{
			if (mObject == null)
			{
				mObject = Sharpen.Extensions.ValueOf(0);
			}
			else
			{
				if (mObject is Number)
				{
					return;
				}
				else
				{
					if (mObject is string)
					{
						Number num = StringToNumber((string)mObject);
						if (num.IsInt())
						{
							mObject = Sharpen.Extensions.ValueOf(num);
						}
						else
						{
							mObject = (num);
						}
					}
					else
					{
						ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public void ToInteger()
		{
			mObject = Sharpen.Extensions.ValueOf(AsInteger());
		}

		/// <exception cref="VariantException"></exception>
		public void ToReal()
		{
			mObject = (AsDouble());
		}

		/// <exception cref="VariantException"></exception>
		public void SelfToString()
		{
			if (mObject == null || mObject is string)
			{
				return;
			}
			else
			{
				if (mObject is int)
				{
					mObject = ((int)mObject).ToString();
				}
				else
				{
					if (mObject is double)
					{
						mObject = ((double)mObject).ToString();
					}
					else
					{
						if (mObject is ByteBuffer)
						{
							ThrowVariantConvertError(this, TYPE_STRING);
						}
						else
						{
							mObject = Utils.VariantToReadableString(this);
						}
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public void ToOctet()
		{
			if (mObject == null || mObject is ByteBuffer)
			{
				return;
			}
			ThrowVariantConvertError(this, TYPE_OCTET);
		}

		public bool AsBoolean()
		{
			if (mObject is int)
			{
				return ((int)mObject) == 0 ? false : true;
			}
			else
			{
				if (mObject is double)
				{
					return ((double)mObject) == 0.0 ? false : true;
				}
				else
				{
					if (mObject is string)
					{
						LexBase lex = new LexBase((string)mObject);
						Number num = lex.ParseNumber();
						if (num != null)
						{
							return num == 0 ? false : true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						if (mObject is VariantClosure)
						{
							VariantClosure v = (VariantClosure)mObject;
							return (v.mObject != null);
						}
						else
						{
							if (mObject is ByteBuffer)
							{
								return true;
							}
							else
							{
								if (mObject == null)
								{
									return false;
								}
								else
								{
									return false;
								}
							}
						}
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public int AsInteger()
		{
			if (mObject is int)
			{
				return ((int)mObject);
			}
			else
			{
				if (mObject is double)
				{
					return (int)((double)mObject);
				}
				else
				{
					if (mObject is string)
					{
						LexBase lex = new LexBase((string)mObject);
						Number num = lex.ParseNumber();
						if (num != null)
						{
							return num;
						}
						else
						{
							return 0;
						}
					}
					else
					{
						if (mObject == null)
						{
							return 0;
						}
						else
						{
							// bytebuffer or object
							ThrowVariantConvertError(this, TYPE_INTEGER);
						}
					}
				}
			}
			return 0;
		}

		/// <exception cref="VariantException"></exception>
		public double AsDouble()
		{
			if (mObject is double)
			{
				return ((double)mObject);
			}
			else
			{
				if (mObject is int)
				{
					return ((int)mObject);
				}
				else
				{
					if (mObject is string)
					{
						LexBase lex = new LexBase((string)mObject);
						Number num = lex.ParseNumber();
						if (num != null)
						{
							return num;
						}
						else
						{
							return 0.0;
						}
					}
					else
					{
						if (mObject == null)
						{
							return 0.0;
						}
						else
						{
							// bytebuffer or object
							ThrowVariantConvertError(this, TYPE_REAL);
						}
					}
				}
			}
			return 0;
		}

		/// <exception cref="VariantException"></exception>
		public string AsString()
		{
			if (mObject is string)
			{
				//return new String( (String)mObject );
				return (string)mObject;
			}
			else
			{
				if (mObject is int)
				{
					return ((int)mObject).ToString();
				}
				else
				{
					if (mObject is double)
					{
						return ((double)mObject).ToString();
					}
					else
					{
						if (mObject == null)
						{
							return null;
						}
						else
						{
							if (mObject is ByteBuffer)
							{
								ThrowVariantConvertError(this, TYPE_STRING);
							}
							else
							{
								return Utils.VariantToReadableString(this);
							}
						}
					}
				}
			}
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public VariantClosure AsObjectClosure()
		{
			if (mObject is VariantClosure)
			{
				return (VariantClosure)mObject;
			}
			ThrowVariantConvertError(this, TYPE_OBJECT);
			return NullVariantClosure;
		}

		/// <exception cref="VariantException"></exception>
		public ByteBuffer AsOctet()
		{
			if (mObject == null)
			{
				return null;
			}
			else
			{
				if (mObject is ByteBuffer)
				{
					return (ByteBuffer)mObject;
				}
				else
				{
					ThrowVariantConvertError(this, TYPE_OCTET);
				}
			}
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public Dispatch2 AsObject()
		{
			if (mObject is VariantClosure)
			{
				return ((VariantClosure)mObject).mObject;
			}
			ThrowVariantConvertError(this, TYPE_OBJECT);
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public Dispatch2 AsObjectThis()
		{
			if (mObject is VariantClosure)
			{
				return ((VariantClosure)mObject).mObjThis;
			}
			ThrowVariantConvertError(this, TYPE_OBJECT);
			return null;
		}

		/// <exception cref="VariantException"></exception>
		public void ChangeClosureObjThis(Dispatch2 objthis)
		{
			if (mObject is VariantClosure)
			{
				VariantClosure vc = (VariantClosure)mObject;
				if (vc.mObjThis != null)
				{
					vc.mObjThis = null;
				}
				vc.mObjThis = objthis;
			}
			else
			{
				ThrowVariantConvertError(this, TYPE_OBJECT);
			}
		}

		/// <exception cref="VariantException"></exception>
		public static void ThrowVariantConvertError(Variant from, string to
			)
		{
			if (to.Equals(TYPE_OBJECT))
			{
				string mes = Error.VariantConvertErrorToObject.Replace("%1", Utils.VariantToReadableString
					(from));
				throw new VariantException(mes);
			}
			else
			{
				string mes = Error.VariantConvertError.Replace("%1", Utils.VariantToReadableString
					(from));
				string mes2 = mes.Replace("%2", to);
				throw new VariantException(mes2);
			}
		}

		/// <exception cref="VariantException"></exception>
		public static void ThrowVariantConvertError(Variant from, string to1
			, string to2)
		{
			string mes = Error.VariantConvertError.Replace("%1", Utils.VariantToReadableString
				(from));
			string mes2 = mes.Replace("%2", to1 + "/" + to2);
			throw new VariantException(mes2);
		}

		/// <exception cref="VariantException"></exception>
		public static void ThrowDividedByZero()
		{
			throw new VariantException(Error.DivideByZero);
		}

		public static Number StringToNumber(string str)
		{
			LexBase lex = new LexBase(str);
			Number num = lex.ParseNumber();
			if (num != null)
			{
				return num;
			}
			else
			{
				return new Number(0);
			}
		}

		public bool IsString()
		{
			return mObject is string;
		}

		public bool IsObject()
		{
			return mObject is VariantClosure;
		}

		public bool IsInteger()
		{
			return mObject is int;
		}

		public bool IsReal()
		{
			return mObject is double;
		}

		public bool IsOctet()
		{
			return mObject is ByteBuffer;
		}

		public bool IsVoid()
		{
			return mObject == null;
		}

		public bool IsNumber()
		{
			return mObject is Number;
		}

		public void Clear()
		{
			mObject = null;
		}

		private const long IEEE_D_SIGN_MASK = unchecked((long)(0x8000000000000000L));

		public static string SpecialRealToString(double r)
		{
			if (Double.IsNaN(r))
			{
				return "NaN";
			}
			if (Double.IsInfinite(r))
			{
				if (Double.NegativeInfinity == r)
				{
					return "-Infinity";
				}
				else
				{
					return "+Infinity";
				}
			}
			if (r == 0.0)
			{
				long ui64 = Double.DoubleToLongBits(r);
				if ((ui64 & IEEE_D_SIGN_MASK) != 0)
				{
					return "-0.0";
				}
				else
				{
					return "+0.0";
				}
			}
			return null;
		}

		private const long D_EXP_MASK = unchecked((long)(0x7ff0000000000000L));

		private const int D_SIGNIFICAND_BITS = 52;

		private const long D_EXP_BIAS = 1023;

		public static string RealToHexString(double r)
		{
			string v = SpecialRealToString(r);
			if (v != null)
			{
				return v;
			}
			long ui64 = Double.DoubleToLongBits(r);
			StringBuilder builder = new StringBuilder(64);
			if ((ui64 & IEEE_D_SIGN_MASK) != 0)
			{
				builder.Append("-0x1.");
			}
			else
			{
				builder.Append("0x1.");
			}
			string hexdigits = "0123456789ABCDEF";
			int exp = (int)(((ui64 & D_EXP_MASK) >> D_SIGNIFICAND_BITS) - D_EXP_BIAS);
			int bits = D_SIGNIFICAND_BITS;
			while (true)
			{
				bits -= 4;
				if (bits < 0)
				{
					break;
				}
				builder.Append(hexdigits[(int)(ui64 >> bits) & unchecked((int)(0x0f))]);
			}
			builder.Append('p');
			builder.Append(exp.ToString());
			return builder.ToString();
		}

        //public static string OctetToListString(ByteBuffer oct)
        //{
        //    if (oct == null)
        //    {
        //        return null;
        //    }
        //    if (oct.Capacity() == 0)
        //    {
        //        return null;
        //    }
        //    int stringlen = oct.Capacity() * 3 - 1;
        //    StringBuilder str = new StringBuilder(stringlen);
        //    string hex = "0123456789ABCDEF";
        //    int count = oct.Capacity();
        //    for (int i = 0; i < count; i++)
        //    {
        //        byte data = oct.Get(i);
        //        str.Append(hex[(data >> 4) & unchecked((int)(0x0f))]);
        //        str.Append(hex[data & unchecked((int)(0x0f))]);
        //        if (i != (count - 1))
        //        {
        //            str.Append(' ');
        //        }
        //    }
        //    return str.ToString();
        //}

		// &=
		/// <exception cref="VariantException"></exception>
		public void AndEqual(Variant rhs)
		{
			int l = AsInteger();
			mObject = null;
			mObject = Sharpen.Extensions.ValueOf(l & rhs.AsInteger());
		}

		// |=
		/// <exception cref="VariantException"></exception>
		public void OrEqual(Variant rhs)
		{
			int l = AsInteger();
			mObject = null;
			mObject = Sharpen.Extensions.ValueOf(l | rhs.AsInteger());
		}

		// ^=
		/// <exception cref="VariantException"></exception>
		public void BitXorEqual(Variant rhs)
		{
			int l = AsInteger();
			mObject = null;
			mObject = Sharpen.Extensions.ValueOf(l ^ rhs.AsInteger());
		}

		// -=
		/// <exception cref="VariantException"></exception>
		public void SubtractEqual(Variant rhs)
		{
			if (mObject is int && rhs.mObject is int)
			{
				mObject = Sharpen.Extensions.ValueOf(((int)mObject) - ((int)rhs.mObject));
				return;
			}
			Number l = AsNumber();
			Number r = rhs.AsNumber();
			if (l.IsInt() && r.IsInt())
			{
				mObject = Sharpen.Extensions.ValueOf(((int)l) - ((int)r));
			}
			else
			{
				mObject = (l - r);
			}
		}

		// +=
		/// <exception cref="VariantException"></exception>
		public void AddEqual(Variant rhs)
		{
			if (mObject is string || rhs.mObject is string)
			{
				if (mObject is string && rhs.mObject is string)
				{
					// both are string
					mObject = (string)mObject + (string)rhs.mObject;
					return;
				}
				string s1 = AsString();
				string s2 = rhs.AsString();
				if (s1 != null && s2 != null)
				{
					StringBuilder builder = new StringBuilder(256);
					builder.Append(s1);
					builder.Append(s2);
					mObject = builder.ToString();
				}
				else
				{
					if (s1 != null)
					{
						mObject = s1;
					}
					else
					{
						mObject = s2;
					}
				}
				//mObject = s1 + s2;
				return;
			}
			if (mObject != null && rhs.mObject != null)
			{
				if (mObject.GetType().IsInstanceOfType(rhs.mObject))
				{
					// 同じクラス
					if (mObject is ByteBuffer)
					{
						ByteBuffer b1 = (ByteBuffer)mObject;
						ByteBuffer b2 = (ByteBuffer)rhs.mObject;
						ByteBuffer result = ByteBuffer.Allocate(b1.Capacity() + b2.Capacity());
						b1.Position(0);
						b2.Position(0);
						result.Put(b1);
						result.Put(b2);
						result.Position(0);
						mObject = result;
						return;
					}
					if (mObject is int)
					{
						int result = ((int)mObject) + ((int)rhs.mObject);
						mObject = Sharpen.Extensions.ValueOf(result);
						return;
					}
				}
			}
			if (mObject == null)
			{
				if (rhs.mObject != null)
				{
					if (rhs.mObject is int)
					{
						mObject = Sharpen.Extensions.ValueOf(((int)rhs.mObject));
						return;
					}
					else
					{
						if (rhs.mObject is double)
						{
							mObject = (((double)rhs.mObject));
							return;
						}
					}
				}
			}
			if (rhs.mObject == null)
			{
				if (mObject != null)
				{
					if (mObject is int)
					{
						return;
					}
					else
					{
						if (mObject is double)
						{
							return;
						}
					}
				}
			}
			mObject = (AsDouble() + rhs.AsDouble());
		}

		// %=
		/// <exception cref="VariantException"></exception>
		public void ResidueEqual(Variant rhs)
		{
			int r = rhs.AsInteger();
			if (r == 0)
			{
				ThrowDividedByZero();
			}
			int l = AsInteger();
			mObject = Sharpen.Extensions.ValueOf(l % r);
		}

		// /=
		/// <exception cref="VariantException"></exception>
		public void DivideEqual(Variant rhs)
		{
			double l = AsDouble();
			double r = rhs.AsDouble();
			mObject = (l / r);
		}

		/// <exception cref="VariantException"></exception>
		public void Idivequal(Variant rhs)
		{
			int r = rhs.AsInteger();
			if (r == 0)
			{
				ThrowDividedByZero();
			}
			int l = AsInteger();
			mObject = Sharpen.Extensions.ValueOf(l / r);
		}

		public void Logicalorequal(Variant rhs)
		{
			bool l = AsBoolean();
			bool r = rhs.AsBoolean();
			mObject = Sharpen.Extensions.ValueOf((l || r) ? 1 : 0);
		}

		// *=
		/// <exception cref="VariantException"></exception>
		public void MultiplyEqual(Variant rhs)
		{
			if (mObject is int && rhs.mObject is int)
			{
				mObject = Sharpen.Extensions.ValueOf(((int)mObject) * ((int)rhs.mObject));
				return;
			}
			Number l = AsNumber();
			Number r = rhs.AsNumber();
			if (l.IsInt() && r.IsInt())
			{
				mObject = Sharpen.Extensions.ValueOf(l * r);
				return;
			}
			mObject = (l * r);
		}

		public void Logicalandequal(Variant rhs)
		{
			bool l = AsBoolean();
			bool r = rhs.AsBoolean();
			mObject = Sharpen.Extensions.ValueOf((l && r) ? 1 : 0);
		}

		// >>=
		/// <exception cref="VariantException"></exception>
		public void RightShiftEqual(Variant rhs)
		{
			int l = AsInteger();
			int r = rhs.AsInteger();
			mObject = Sharpen.Extensions.ValueOf(l >> r);
		}

		// <<=
		/// <exception cref="VariantException"></exception>
		public void LeftShiftEqual(Variant rhs)
		{
			int l = AsInteger();
			int r = rhs.AsInteger();
			mObject = Sharpen.Extensions.ValueOf(l << r);
		}

		/// <exception cref="VariantException"></exception>
		public void Rbitshiftequal(Variant rhs)
		{
			int l = AsInteger();
			int r = rhs.AsInteger();
			mObject = Sharpen.Extensions.ValueOf((int)((int)(((uint)l) >> r)));
		}

		/// <exception cref="VariantException"></exception>
		public void Increment()
		{
			if (mObject is string)
			{
				ToNumber();
			}
			if (mObject is double)
			{
				mObject = (((double)mObject) + 1.0);
			}
			else
			{
				if (mObject is int)
				{
					mObject = Sharpen.Extensions.ValueOf(((int)mObject) + 1);
				}
				else
				{
					if (mObject == null)
					{
						mObject = Sharpen.Extensions.ValueOf(1);
					}
					else
					{
						ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public void Decrement()
		{
			if (mObject is string)
			{
				ToNumber();
			}
			if (mObject is double)
			{
				mObject = (((double)mObject) - 1.0);
			}
			else
			{
				if (mObject is int)
				{
					mObject = Sharpen.Extensions.ValueOf(((int)mObject) - 1);
				}
				else
				{
					if (mObject == null)
					{
						mObject = Sharpen.Extensions.ValueOf(-1);
					}
					else
					{
						ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
					}
				}
			}
		}

		/// <exception cref="VariantException"></exception>
		public string GetString()
		{
			// returns String
			if (mObject != null && mObject is string)
			{
				return (string)mObject;
			}
			else
			{
				ThrowVariantConvertError(this, TYPE_STRING);
				return null;
			}
		}

		public string GetTypeName()
		{
			if (mObject == null)
			{
				return "void";
			}
			else
			{
				if (mObject is VariantClosure)
				{
					return "Object";
				}
				else
				{
					if (mObject is string)
					{
						return "String";
					}
					else
					{
						if (mObject is int)
						{
							return "Integer";
						}
						else
						{
							if (mObject is double)
							{
								return "Real";
							}
							else
							{
								if (mObject is ByteBuffer)
								{
									return "Octet";
								}
								else
								{
									return null;
								}
							}
						}
					}
				}
			}
		}

		public void Logicalnot()
		{
			bool res = !AsBoolean();
			mObject = Sharpen.Extensions.ValueOf(res ? 1 : 0);
		}

		/// <exception cref="VariantException"></exception>
		public void Bitnot()
		{
			int res = ~AsInteger();
			mObject = Sharpen.Extensions.ValueOf(res);
		}

		/// <exception cref="VariantException"></exception>
		public void Tonumber()
		{
			if (mObject is Number)
			{
				return;
			}
			// nothing to do
			if (mObject is string)
			{
				Number num = StringToNumber((string)mObject);
				if (num.IsInt())
				{
					mObject = Sharpen.Extensions.ValueOf(num);
				}
				else
				{
					mObject = (num);
				}
				return;
			}
			if (mObject == null)
			{
				mObject = Sharpen.Extensions.ValueOf(0);
				return;
			}
			ThrowVariantConvertError(this, TYPE_INTEGER, TYPE_REAL);
		}

		/// <exception cref="VariantException"></exception>
		public void Changesign()
		{
			if (mObject is int)
			{
				mObject = Sharpen.Extensions.ValueOf(-((int)mObject));
				return;
			}
			Number val = AsNumber();
			if (val.IsInt())
			{
				mObject = Sharpen.Extensions.ValueOf(-val);
			}
			else
			{
				mObject = (-val);
			}
		}

		public object ToJavaObject()
		{
			return mObject;
		}

		public void SetObject(Dispatch2 @ref)
		{
			mObject = new VariantClosure(@ref, null);
		}

		public void SetObject(Dispatch2 @object, Dispatch2 objthis)
		{
			mObject = new VariantClosure(@object, objthis);
		}

		public sealed override string ToString()
		{
			try
			{
				if (IsVoid())
				{
					return "(void)";
				}
				else
				{
					if (IsInteger())
					{
						return "(int)" + AsString();
					}
					else
					{
						if (IsReal())
						{
							return "(real)" + AsString();
						}
						else
						{
							if (IsString())
							{
								return "(string)\"" + LexBase.EscapeC(AsString()) + "\"";
							}
							else
							{
								if (IsOctet())
								{
									return "(octet)<% " + Variant.OctetToListString(AsOctet(
										)) + " %>";
								}
								else
								{
									if (IsObject())
									{
										VariantClosure c = (VariantClosure)AsObjectClosure();
										return c.ToString();
									}
									else
									{
										// native object ?
										return "(octet) [" + GetType().FullName + "]";
									}
								}
							}
						}
					}
				}
			}
			catch (VariantException)
			{
				return string.Empty;
			}
		}

		public string ToJavaString()
		{
			try
			{
				if (IsVoid())
				{
					return null;
				}
				else
				{
					if (IsInteger())
					{
						return AsString();
					}
					else
					{
						if (IsReal())
						{
							return AsString();
						}
						else
						{
							if (IsString())
							{
								return "\"" + LexBase.EscapeC(AsString()) + "\"";
							}
							else
							{
								if (IsOctet())
								{
									ByteBuffer buf = AsOctet();
									if (buf != null && buf.Capacity() != 0)
									{
										StringBuilder builder = new StringBuilder();
										int count = buf.Capacity();
										builder.Append('{');
										for (int i = 0; i < count; i++)
										{
											byte b = buf.Get(i);
											builder.Append("0x");
											builder.Append(Sharpen.Extensions.ToHexString(b));
											builder.Append(", ");
										}
										builder.Append('}');
									}
									return "(octet)<% " + Variant.OctetToListString(AsOctet(
										)) + " %>";
								}
								else
								{
									if (IsObject())
									{
										return " new " + mObject.GetType().FullName;
									}
									else
									{
										// native object ?
										return " new " + mObject.GetType().FullName;
									}
								}
							}
						}
					}
				}
			}
			catch (VariantException)
			{
				return " error ";
			}
		}
	}
}

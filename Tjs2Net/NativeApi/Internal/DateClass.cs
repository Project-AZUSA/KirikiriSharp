/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;
using Tjs2.NativeApi;
using Tjs2.NativeApi.Internal;
using Tjs2;

namespace Tjs2.Engine.NativeClass.Internal
{
	public class DateClass : NativeClass
	{
		public static int mClassID;

		private static readonly string CLASS_NAME = "Date";

		protected internal override NativeInstance CreateNativeInstance()
		{
			return new DateNI();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TJSException"></exception>
		public DateClass() : base(CLASS_NAME)
		{
			int NCM_CLASSID = Tjs.RegisterNativeClass(CLASS_NAME);
			SetClassID(NCM_CLASSID);
			mClassID = NCM_CLASSID;
			//Class<? extends DateClass> c = getClass();
			//registerMethods( c, CLASS_NAME );
			// constructor
			RegisterNCM(CLASS_NAME, new _NativeClassConstructor_35(), CLASS_NAME, Interface.nitMethod
				, 0);
			// formatted string -> date/time
			// if( _this.mDateTime == -1) throw new TJSException(Error.InvalidValueForTimestamp);
			RegisterNCM("finalize", new _NativeClassMethod_68(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("setYear", new _NativeClassMethod_75(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setMonth", new _NativeClassMethod_86(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setDate", new _NativeClassMethod_97(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setHours", new _NativeClassMethod_108(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setMinutes", new _NativeClassMethod_119(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setSeconds", new _NativeClassMethod_130(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("setTime", new _NativeClassMethod_141(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			//int num = param[0].asInteger();
			//long y = num < 0 ? (long)num + 0x100000000L : num;
			//_this.mDateTime.setTimeInMillis( y );
			RegisterNCM("getDate", new _NativeClassMethod_156(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getDay", new _NativeClassMethod_168(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getHours", new _NativeClassMethod_180(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getMinutes", new _NativeClassMethod_192(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getMonth", new _NativeClassMethod_204(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getSeconds", new _NativeClassMethod_216(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("getTime", new _NativeClassMethod_228(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			//int y = (int) (num > Integer.MAX_VALUE ? num - 0x100000000L : num);
			//result.set( y );
			RegisterNCM("getTimezoneOffset", new _NativeClassMethod_243(), CLASS_NAME, Interface
				.nitMethod, 0);
			RegisterNCM("getYear", new _NativeClassMethod_253(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("parse", new _NativeClassMethod_265(), CLASS_NAME, Interface.nitMethod
				, 0);
		}

		private sealed class _NativeClassConstructor_35 : NativeClassConstructor
		{
			public _NativeClassConstructor_35()
			{
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TJSException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				int hr = _this.Construct(param, objthis);
				if (hr < 0)
				{
					return hr;
				}
				if (param.Length == 0)
				{
					_this.mDateTime = Calendar.GetInstance();
				}
				else
				{
					if (param.Length >= 1)
					{
						if (param[0].IsString())
						{
							_this.mDateTime = DateClass.ParseDateString(param[0].GetString());
						}
						else
						{
							int y;
							int mon = 0;
							int day = 1;
							int h = 0;
							int m = 0;
							int s = 0;
							y = param[0].AsInteger();
							if (param.Length > 1 && param[1].IsVoid() != true)
							{
								mon = param[1].AsInteger();
							}
							if (param.Length > 2 && param[2].IsVoid() != true)
							{
								day = param[2].AsInteger();
							}
							if (param.Length > 3 && param[3].IsVoid() != true)
							{
								h = param[3].AsInteger();
							}
							if (param.Length > 4 && param[4].IsVoid() != true)
							{
								m = param[4].AsInteger();
							}
							if (param.Length > 5 && param[5].IsVoid() != true)
							{
								s = param[5].AsInteger();
							}
							Calendar cal = Calendar.GetInstance();
							cal.Set(y, mon, day, h, m, s);
							_this.mDateTime = cal;
						}
					}
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_68 : NativeClassMethod
		{
			public _NativeClassMethod_68()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_75 : NativeClassMethod
		{
			public _NativeClassMethod_75()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.YEAR, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_86 : NativeClassMethod
		{
			public _NativeClassMethod_86()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.MONTH, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_97 : NativeClassMethod
		{
			public _NativeClassMethod_97()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.DAY_OF_MONTH, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_108 : NativeClassMethod
		{
			public _NativeClassMethod_108()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.HOUR_OF_DAY, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_119 : NativeClassMethod
		{
			public _NativeClassMethod_119()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.MINUTE, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_130 : NativeClassMethod
		{
			public _NativeClassMethod_130()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				_this.mDateTime.Set(Calendar.SECOND, param[0].AsInteger());
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_141 : NativeClassMethod
		{
			public _NativeClassMethod_141()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				double num = param[0].AsDouble();
				_this.mDateTime.SetTimeInMillis((long)num);
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_156 : NativeClassMethod
		{
			public _NativeClassMethod_156()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.DAY_OF_MONTH));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_168 : NativeClassMethod
		{
			public _NativeClassMethod_168()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.DAY_OF_WEEK));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_180 : NativeClassMethod
		{
			public _NativeClassMethod_180()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.HOUR_OF_DAY));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_192 : NativeClassMethod
		{
			public _NativeClassMethod_192()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.MINUTE));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_204 : NativeClassMethod
		{
			public _NativeClassMethod_204()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.MONTH));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_216 : NativeClassMethod
		{
			public _NativeClassMethod_216()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.mDateTime.Get(Calendar.SECOND));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_228 : NativeClassMethod
		{
			public _NativeClassMethod_228()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					long num = _this.mDateTime.GetTimeInMillis();
					result.Set(num);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_243 : NativeClassMethod
		{
			public _NativeClassMethod_243()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (result != null)
				{
					result.Set(System.TimeZoneInfo.Local.GetRawOffset() / (60 * 1000));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_253 : NativeClassMethod
		{
			public _NativeClassMethod_253()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set((int)_this.mDateTime.Get(Calendar.YEAR));
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_265 : NativeClassMethod
		{
			public _NativeClassMethod_265()
			{
			}

			/// <exception cref="TJSException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				DateNI _this = (DateNI)objthis.GetNativeInstance(DateClass.mClassID
					);
				// インスタンス所得
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				_this.mDateTime = DateClass.ParseDateString(param[0].GetString());
				return Error.S_OK;
			}
		}

		/// <exception cref="TJSException"></exception>
		public static Calendar ParseDateString(string str)
		{
			DateFormat format = DateFormat.GetInstance();
			try
			{
				Calendar cal = Calendar.GetInstance();
				cal.SetTime(format.Parse(str));
				return cal;
			}
			catch (ParseException)
			{
				// TODO "年/月/日 时:分:秒" には对应していないかも、以下でいいか确认する
				Sharpen.Pattern regex = Sharpen.Pattern.Compile("([0-9]+)\\/([0-9]+)\\/([0-9]+)[ \t]+([0-9]+):([0-9]+):([0-9]+)"
					);
				Matcher m = regex.Matcher(str);
				if (m.GroupCount() > 6)
				{
					int y = Sharpen.Extensions.ValueOf(m.Group(1));
					int mon = Sharpen.Extensions.ValueOf(m.Group(2));
					int day = Sharpen.Extensions.ValueOf(m.Group(3));
					int h = Sharpen.Extensions.ValueOf(m.Group(4));
					int min = Sharpen.Extensions.ValueOf(m.Group(5));
					int s = Sharpen.Extensions.ValueOf(m.Group(6));
					Calendar cal = Calendar.GetInstance();
					cal.Set(y, mon, day, h, min, s);
					return cal;
				}
				else
				{
					throw new TJSException(Error.CannotParseDate);
				}
			}
		}
	}
}

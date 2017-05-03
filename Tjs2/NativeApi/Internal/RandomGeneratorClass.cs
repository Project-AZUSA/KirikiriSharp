/*
 * TJS2 CSharp
 */

using Tjs2.Engine;

namespace Tjs2.NativeApi.Internal
{
	public class RandomGeneratorClass : NativeClass
	{
		public static int ClassID;

		private static readonly string CLASS_NAME = "RandomGenerator";

		protected internal override NativeInstance CreateNativeInstance()
		{
			return new RandomGeneratorNI();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public RandomGeneratorClass() : base(CLASS_NAME)
		{
            ClassID = Tjs.FindNativeClassId(CLASS_NAME);
            // constructor
            RegisterNCM(CLASS_NAME, new _NativeClassConstructor_24(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("finalize", new _NativeClassMethod_37(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("randomize", new _NativeClassMethod_44(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			RegisterNCM("random", new _NativeClassMethod_54(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			// returns 53-bit precision random value x, x is in 0 <= x < 1
			RegisterNCM("random32", new _NativeClassMethod_69(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			// returns 32-bit precision integer value x, x is in 0 <= x <= 4294967295
			RegisterNCM("random63", new _NativeClassMethod_84(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			// returns 63-bit precision integer value x, x is in 0 <= x <= 9223372036854775807
			RegisterNCM("random64", new _NativeClassMethod_99(), CLASS_NAME, Interface.nitMethod
				, 0);
			// インスタンス所得
			// returns 64-bit precision integer value x, x is in
			// -9223372036854775808 <= x <= 9223372036854775807
			// Java 实装では、int は32 bitまで
			RegisterNCM("serialize", new _NativeClassMethod_116(), CLASS_NAME, Interface.nitMethod
				, 0);
		}

		private sealed class _NativeClassConstructor_24 : NativeClassConstructor
		{
			public _NativeClassConstructor_24()
			{
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
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
				_this.Randomize(param);
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_37 : NativeClassMethod
		{
			public _NativeClassMethod_37()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_44 : NativeClassMethod
		{
			public _NativeClassMethod_44()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				_this.Randomize(param);
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_54 : NativeClassMethod
		{
			public _NativeClassMethod_54()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.Random());
				}
				else
				{
					_this.Random();
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_69 : NativeClassMethod
		{
			public _NativeClassMethod_69()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.Random32());
				}
				else
				{
					_this.Random32();
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_84 : NativeClassMethod
		{
			public _NativeClassMethod_84()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.Random63());
				}
				else
				{
					_this.Random63();
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_99 : NativeClassMethod
		{
			public _NativeClassMethod_99()
			{
			}

			/// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (result != null)
				{
					result.Set(_this.Random64());
				}
				else
				{
					_this.Random64();
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_116 : NativeClassMethod
		{
			public _NativeClassMethod_116()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RandomGeneratorNI _this = (RandomGeneratorNI)objthis.GetNativeInstance(RandomGeneratorClass
					.ClassID);
				// インスタンス所得
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				// returns 64-bit precision integer value x, x is in
				// -9223372036854775808 <= x <= 9223372036854775807
				if (result != null)
				{
					Dispatch2 dsp = _this.Serialize();
					result.Set(dsp, dsp);
				}
				return Error.S_OK;
			}
		}
	}
}

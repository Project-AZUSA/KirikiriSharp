/*
 * TJS2 CSharp
 */

using System;
using Tjs2.Engine;
using Double = Tjs2.Sharpen.Double;

namespace Tjs2.NativeApi.Internal
{
	public sealed class MathClass : NativeClass
    {

        private const string CLASS_NAME = "Math";

		private static Random mRandomGenerator;

	    public static void Initialize()
		{
			mRandomGenerator = null;
		}

		public static void FinalizeApplication()
		{
			mRandomGenerator = null;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public MathClass() : base(CLASS_NAME)
		{
			if (mRandomGenerator == null)
			{
				mRandomGenerator = new Random((int)DateTime.Now.Ticks);
			}
			// constructor
			RegisterNCM(CLASS_NAME, new MathConstructorMethod(), CLASS_NAME, Interface.nitMethod, 0);
			RegisterNCM("abs", new AbsMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("acos", new AcosMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("asin", new AsinMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("atan", new AtanMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("atan2", new Atan2Method(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("ceil", new CeilMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("exp", new ExpMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("floor", new FloorMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("log", new LogMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("pow", new PowMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("max", new MaxMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("min", new MinMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("random", new RandomMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("round", new RoundMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("sin", new SinMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("cos", new CosMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("sqrt", new SqrtMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("tan", new TanMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
			RegisterNCM("E", new EProperty(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("LOG2E", new Log2EProperty(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("LOG10E", new Log10EProperty(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("LN10", new Ln10Property(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("LN2", new Ln2Property(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("PI", new PiProperty(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("SQRT1_2", new Sqrt1_2Property(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
			RegisterNCM("SQRT2", new Sqrt2Property(), CLASS_NAME, Interface.nitProperty, Interface.STATICMEMBER);
		}

		private sealed class MathConstructorMethod : NativeClassConstructor
		{
		    protected internal override int Process(Variant result, Variant[] param, Dispatch2 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class AbsMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (param.Length < 1)
					return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Abs(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class AcosMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (param.Length < 1)
					return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Acos(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class AsinMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (param.Length < 1)
					return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Asin(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class AtanMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (param.Length < 1)
					return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Atan(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class Atan2Method : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if (param.Length < 2)
					return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Atan2(param[0].AsDouble(), param[1].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class CeilMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Ceiling(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class ExpMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Exp(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class FloorMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Floor(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class LogMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Log(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class PowMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 2)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Pow(param[0].AsDouble(), param[1].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class MaxMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (result == null) return Error.S_OK;

			    double r = double.NegativeInfinity;
			    int count = param.Length;
			    for (int i = 0; i < count; i++)
			    {
			        double v = param[i].AsDouble();
			        if (double.IsNaN(v))
			        {
			            result.Set(double.NaN);
			            return Error.S_OK;
			        }
			        if (Double.Compare(v, r) > 0)
			        {
			            r = v;
			        }
			    }
			    result.Set(r);
			    return Error.S_OK;
			}
		}

		private sealed class MinMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (result == null) return Error.S_OK;

			    double r = double.PositiveInfinity;
			    int count = param.Length;
			    for (int i = 0; i < count; i++)
			    {
			        double v = param[i].AsDouble();
			        if (double.IsNaN(v))
			        {
			            result.Set(double.NaN);
			            return Error.S_OK;
			        }
			        if (Double.Compare(v, r) < 0)
			        {
			            r = v;
			        }
			    }
			    result.Set(r);
			    return Error.S_OK;
			}
		}

		private sealed class RandomMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    result?.Set(mRandomGenerator.NextDouble());
			    return Error.S_OK;
			}
		}

		private sealed class RoundMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Round(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class SinMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Sin(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class CosMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Cos(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class SqrtMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Sqrt(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class TanMethod : NativeClassMethod
		{
		    /// <exception cref="VariantException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
			    if (param.Length < 1)
			        return Error.E_BADPARAMCOUNT;
			    result?.Set(Math.Tan(param[0].AsDouble()));
			    return Error.S_OK;
			}
		}

		private sealed class EProperty : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(2.7182818284590452354);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Log2EProperty : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(1.4426950408889634074);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Log10EProperty : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(0.4342944819032518276);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Ln10Property : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(2.30258509299404568402);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Ln2Property : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(0.69314718055994530942);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class PiProperty : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(3.14159265358979323846);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Sqrt1_2Property : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(0.70710678118654752440);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class Sqrt2Property : NativeClassProperty
		{
		    public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(1.41421356237309504880);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}
	}
}

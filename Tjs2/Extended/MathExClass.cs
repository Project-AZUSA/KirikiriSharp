using System;
using System.Collections.Generic;
using System.Text;
using Tjs2.Engine;
using Tjs2.NativeApi;

namespace Tjs2.Extended
{
    public sealed class MathExClass : NativeClass
    {
        private const string CLASS_NAME = "MathEx";
        
        public MathExClass() : base(CLASS_NAME)
        {
            RegisterNCM(CLASS_NAME, new MathExConstructorMethod(), CLASS_NAME, Interface.nitMethod, 0);
            RegisterNCM("cbrt", new CbrtMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("cosh", new CoshMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("sinh", new SinhMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("tanh", new TanhMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("log10", new Log10Method(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("sign", new SignMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
            RegisterNCM("trunc", new TruncMethod(), CLASS_NAME, Interface.nitMethod, Interface.STATICMEMBER);
        }

        public sealed class MathExConstructorMethod : NativeClassConstructor
        {
            protected internal override int Process(Variant result, Variant[] param, Dispatch2 objthis)
            {
                return Error.S_OK;
            }
        }

        private sealed class CbrtMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Pow(param[0].AsDouble(), 1.0 / 3.0));
                return Error.S_OK;
            }
        }

        private sealed class CoshMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Cosh(param[0].AsDouble()));
                return Error.S_OK;
            }
        }

        private sealed class SinhMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Sinh(param[0].AsDouble()));
                return Error.S_OK;
            }
        }

        private sealed class TanhMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Tanh(param[0].AsDouble()));
                return Error.S_OK;
            }
        }

        private sealed class Log10Method : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Log10(param[0].AsDouble()));
                return Error.S_OK;
            }
        }

        private sealed class SignMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Sign(param[0].AsDouble()));
                return Error.S_OK;
            }
        }

        private sealed class TruncMethod : NativeClassMethod
        {
            /// <exception cref="VariantException"></exception>
            protected internal override int Process(Variant result, Variant[] param, Dispatch2
                 objthis)
            {
                if (param.Length != 1)
                    return Error.E_BADPARAMCOUNT;
                result?.Set(Math.Truncate(param[0].AsDouble()));
                return Error.S_OK;
            }
        }
    }

}

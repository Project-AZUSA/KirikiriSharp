/*
 * The TJS2 interpreter from kirikirij
 */

using System.Text;
using Tjs2.Engine;
using Tjs2.NativeApi;

namespace Tjs2.Extended
{
	public class DebugClass : NativeClass
	{
		public static int ClassID = -1;

		private static readonly string CLASS_NAME = "Debug";

		protected internal override NativeInstance CreateNativeInstance()
		{
			return null;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public DebugClass() : base(CLASS_NAME)
		{
            ClassID = Tjs.FindNativeClassId(CLASS_NAME);
            // constructor
            RegisterNCM(CLASS_NAME, new ReturnOKConstructor(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("finalize", new ReturnOKMethod(), CLASS_NAME, Interface.nitMethod, 
				0);
			
			RegisterNCM("message", new DebugMessageMethod(), CLASS_NAME, Interface.nitMethod, 
				0);
		}

		private sealed class ReturnOKConstructor : NativeClassConstructor
		{
			public ReturnOKConstructor()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class ReturnOKMethod : NativeClassMethod
		{
			public ReturnOKMethod()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class DebugMessageMethod : NativeClassMethod
		{
			public DebugMessageMethod()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				if( param.Length < 1 ) return Error.E_BADPARAMCOUNT;
				if( param.Length == 1 ) {
                    Tjs.OutputToConsole(param[0].AsString());
					//Console.WriteLine( param[0].AsString() );
				} else {
					// display the arguments separated with ", "
					StringBuilder builder = new StringBuilder(512);
					for( int i = 0; i < param.Length; i++ ) {
						if( i != 0 ) builder.Append(", ");
						builder.Append( param[i].AsString() );
                    }
                    //Console.WriteLine(builder.ToString());
                    Tjs.OutputToConsole(builder.ToString());
                }
				return Error.S_OK;
			}
		}
	}
}

/*
 * TJS2 CSharp
 */

using System;
using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	public class RegExpClass : NativeClass
	{
		public static int ClassID = -1;

		public static Variant mLastRegExp;

		private const int globalsearch = (1 << 31);

		private const int tjsflagsmask = unchecked((int)(0xff000000));

		private static readonly string CLASS_NAME = "RegExp";

		protected internal override NativeInstance CreateNativeInstance()
		{
			return new RegExpNI();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private static void Compile(Variant[] param, RegExpNI _this)
		{
			string expr = param[0].AsString();
			int flags;
			if (param.Length >= 2)
			{
				string fs = param[1].AsString();
				flags = RegExpNI.GetRegExpFlagsFromString(fs);
			}
			else
			{
				flags = RegExpNI.RegExpFlagToValue((char)0, 0);
			}
			if (expr.Length == 0)
			{
				expr = "(?:)";
			}
			// generate empty regular expression
			try
			{
				int pflag = (flags & ~tjsflagsmask);
				if (pflag != 0)
				{
					_this.RegEx = Sharpen.Pattern.Compile(expr, pflag);
				}
				else
				{
					_this.RegEx = Sharpen.Pattern.Compile(expr);
				}
			}
            //catch (PatternSyntaxException e)
            //{
            //    _this.RegEx = null;
            //    throw new TJSException(e.Message);
            //}
			catch (ArgumentException e)
			{
				_this.RegEx = null;
				throw new TjsException(e.Message);
			}
			_this.mFlags = flags;
		}

		private static bool Match(string target, RegExpNI _this)
		{
			if (_this.RegEx == null)
			{
				return false;
			}
			int targlen = target.Length;
			if (_this.mStart == targlen)
			{
				// Start already reached at end
				return _this.RegEx == null;
			}
			else
			{
				// returns true if empty
				if (_this.mStart > targlen)
				{
					// Start exceeds target's length
					return false;
				}
			}
			int searchstart = _this.mStart;
			_this.mMatch = _this.RegEx.Matcher(Sharpen.Runtime.Substring(target, searchstart)
				);
			return _this.mMatch.Matches();
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private static bool Exec(string target, RegExpNI _this)
		{
			bool matched = Match(target, _this);
			Dispatch2 array = GetResultArray(matched, _this, _this.mMatch);
			_this.mArray = new Variant(array, array);
			_this.mInput = target;
			if (!matched || _this.RegEx == null)
			{
				_this.mIndex = _this.mStart;
				_this.mLastIndex = _this.mStart;
				_this.mLastMatch = string.Empty;
				_this.mLastParen = string.Empty;
				_this.mLeftContext = Sharpen.Runtime.Substring(target, 0, _this.mStart);
			}
			else
			{
				_this.mIndex = _this.mStart + _this.mMatch.Start();
				_this.mLastIndex = _this.mStart + _this.mMatch.End();
				_this.mLastMatch = _this.mMatch.Group(0);
				_this.mLastParen = _this.mMatch.Group(_this.mMatch.GroupCount() - 1);
				_this.mLeftContext = Sharpen.Runtime.Substring(target, _this.mIndex);
				_this.mRightContext = Sharpen.Runtime.Substring(target, _this.mLastIndex);
				if ((_this.mFlags & globalsearch) != 0)
				{
					// global search flag changes the next search starting position.
					int match_end = _this.mLastIndex;
					_this.mStart = match_end;
				}
			}
			return matched;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		private static Dispatch2 GetResultArray(bool matched, RegExpNI _this, Matcher m)
		{
			Dispatch2 array = Tjs.CreateArrayObject();
			if (matched)
			{
				if (_this.RegEx == null)
				{
					Variant val = new Variant(string.Empty);
					array.PropSetByNum(Interface.MEMBERENSURE | Interface.IGNOREPROP, 0, val, array);
				}
				else
				{
					if (m != null)
					{
						bool isMatch = m.Matches();
						Variant val;
						if (isMatch)
						{
							val = new Variant(m.Group(0));
							array.PropSetByNum(Interface.MEMBERENSURE | Interface.IGNOREPROP, 0, val, array);
						}
						int size = m.GroupCount();
						for (int i = 0; i < size; i++)
						{
							val = new Variant(m.Group(i + 1));
							array.PropSetByNum(Interface.MEMBERENSURE | Interface.IGNOREPROP, i + 1, val, array
								);
						}
					}
				}
			}
			return array;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public RegExpClass() : base(CLASS_NAME)
		{
            ClassID = Tjs.FindNativeClassId(CLASS_NAME);
            // constructor
            RegisterNCM(CLASS_NAME, new _NativeClassConstructor_132(), CLASS_NAME, Interface.
				nitMethod, 0);
			RegisterNCM("finalize", new _NativeClassMethod_147(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("compile", new _NativeClassMethod_154(), CLASS_NAME, Interface.nitMethod
				, 0);
			// compiles given regular expression and flags.
			RegisterNCM("_compile", new _NativeClassMethod_166(), CLASS_NAME, Interface.nitMethod
				, 0);
			//_this->RegEx.assign(exprstart, (wregex::flag_type));
			RegisterNCM("test", new _NativeClassMethod_206(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("match", new _NativeClassMethod_230(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("exec", new _NativeClassMethod_254(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("replace", new _NativeClassMethod_279(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("split", new _NativeClassMethod_350(), CLASS_NAME, Interface.nitMethod
				, 0);
			RegisterNCM("matches", new _NativeClassProperty_385(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("start", new _NativeClassProperty_398(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("index", new _NativeClassProperty_414(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("lastIndex", new _NativeClassProperty_427(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("input", new _NativeClassProperty_440(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("lastMatch", new _NativeClassProperty_453(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("lastParen", new _NativeClassProperty_464(), CLASS_NAME, Interface.nitProperty
				, 0);
			RegisterNCM("leftContext", new _NativeClassProperty_475(), CLASS_NAME, Interface.
				nitProperty, 0);
			RegisterNCM("rightContext", new _NativeClassProperty_486(), CLASS_NAME, Interface
				.nitProperty, 0);
			RegisterNCM("last", new _NativeClassProperty_498(), CLASS_NAME, Interface.nitProperty
				, Interface.STATICMEMBER);
		}

		private sealed class _NativeClassConstructor_132 : NativeClassConstructor
		{
			public _NativeClassConstructor_132()
			{
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
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
				if (param.Length >= 1)
				{
					RegExpClass.Compile(param, _this);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_147 : NativeClassMethod
		{
			public _NativeClassMethod_147()
			{
			}

			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_154 : NativeClassMethod
		{
			public _NativeClassMethod_154()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				RegExpClass.Compile(param, _this);
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_166 : NativeClassMethod
		{
			public _NativeClassMethod_166()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length != 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string expr = param[0].GetString();
				if (expr == null || expr[0] == 0)
				{
					return Error.E_FAIL;
				}
				if (expr[0] != '/' || expr[1] != '/')
				{
					return Error.E_FAIL;
				}
				int exprstart = expr.IndexOf('/', 2);
				if (exprstart < 0)
				{
					return Error.E_FAIL;
				}
				int flags = RegExpNI.GetRegExpFlagsFromString(Sharpen.Runtime.Substring(expr, 2));
				int pflag = (flags & ~RegExpClass.tjsflagsmask);
				try
				{
					if (pflag != 0)
					{
						_this.RegEx = Sharpen.Pattern.Compile(Sharpen.Runtime.Substring(expr, exprstart +
							 1), pflag);
					}
					else
					{
						_this.RegEx = Sharpen.Pattern.Compile(Sharpen.Runtime.Substring(expr, exprstart +
							 1));
					}
				}
                //catch (PatternSyntaxException e)
                //{
                //    throw new TJSException(e.Message);
                //}
				catch (ArgumentException e)
				{
					throw new TjsException(e.Message);
				}
				_this.mFlags = flags;
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_206 : NativeClassMethod
		{
			public _NativeClassMethod_206()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string target = param[0].AsString();
				bool matched = RegExpClass.Exec(target, _this);
				RegExpClass.mLastRegExp = new Variant(objthis, objthis);
				if (result != null)
				{
					result.Set(matched ? 1 : 0);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_230 : NativeClassMethod
		{
			public _NativeClassMethod_230()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				if (result != null)
				{
					string target = param[0].AsString();
					bool matched = RegExpClass.Match(target, _this);
					Dispatch2 array = RegExpClass.GetResultArray(matched, _this, _this.
						mMatch);
					result.Set(array, array);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_254 : NativeClassMethod
		{
			public _NativeClassMethod_254()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string target = param[0].AsString();
				RegExpClass.Exec(target, _this);
				RegExpClass.mLastRegExp = new Variant(objthis, objthis);
				if (result != null)
				{
					result.Set(_this.mArray);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_279 : NativeClassMethod
		{
			public _NativeClassMethod_279()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 2)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string target = param[0].AsString();
				string to = null;
				bool func;
				VariantClosure funcval = null;
				if (param[1].IsObject() != true)
				{
					to = param[1].AsString();
					func = false;
				}
				else
				{
					funcval = param[1].AsObjectClosure();
					if (funcval.mObjThis == null)
					{
						funcval.mObjThis = objthis;
					}
					func = true;
				}
				string ret = null;
				Matcher m = _this.RegEx.Matcher(target);
				if (func == false)
				{
					ret = m.ReplaceAll(to);
				}
				else
				{
					int hr;
					VariantClosure clo = new VariantClosure(null, null);
					Variant funcret = new Variant();
					Variant arrayval = new Variant(clo);
					Variant[] args = new Variant[1];
					args[0] = arrayval;
					int size = target.Length;
					ret = string.Empty;
					for (int i = 0; i < size; )
					{
						if (m.Find(i))
						{
							ret += Sharpen.Runtime.Substring(target, i, m.Start());
							Dispatch2 array = RegExpClass.GetResultArray(true, _this, m);
							clo.Set(array, array);
							hr = funcval.FuncCall(0, null, funcret, args, null);
							if (hr >= 0)
							{
								ret += funcret.AsString();
							}
							i = m.End();
						}
						else
						{
							break;
						}
					}
				}
				if (result != null)
				{
					result.Set(ret);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassMethod_350 : NativeClassMethod
		{
			public _NativeClassMethod_350()
			{
			}

			/// <exception cref="TjsException"></exception>
			protected internal override int Process(Variant result, Variant[] param, Dispatch2
				 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				if (param.Length < 1)
				{
					return Error.E_BADPARAMCOUNT;
				}
				string target = param[0].AsString();
				bool purgeempty = false;
				if (param.Length >= 3)
				{
					purgeempty = param[2].AsBoolean();
				}
				Holder<Dispatch2> array = new Holder<Dispatch2>(null);
				_this.Split(array, target, purgeempty);
				if (result != null)
				{
					result.Set(array.mValue, array.mValue);
				}
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassProperty_385 : NativeClassProperty
		{
			public _NativeClassProperty_385()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mArray);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_398 : NativeClassProperty
		{
			public _NativeClassProperty_398()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mStart);
				return Error.S_OK;
			}

			/// <exception cref="VariantException"></exception>
			public override int Set(Variant param, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				_this.mStart = param.AsInteger();
				return Error.S_OK;
			}
		}

		private sealed class _NativeClassProperty_414 : NativeClassProperty
		{
			public _NativeClassProperty_414()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mIndex);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_427 : NativeClassProperty
		{
			public _NativeClassProperty_427()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mLastIndex);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_440 : NativeClassProperty
		{
			public _NativeClassProperty_440()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mInput);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_453 : NativeClassProperty
		{
			public _NativeClassProperty_453()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mLastMatch);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_464 : NativeClassProperty
		{
			public _NativeClassProperty_464()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mLastParen);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_475 : NativeClassProperty
		{
			public _NativeClassProperty_475()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mLeftContext);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_486 : NativeClassProperty
		{
			public _NativeClassProperty_486()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				RegExpNI _this = (RegExpNI)objthis.GetNativeInstance(RegExpClass.ClassID
					);
				if (_this == null)
				{
					return Error.E_NATIVECLASSCRASH;
				}
				result.Set(_this.mRightContext);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}

		private sealed class _NativeClassProperty_498 : NativeClassProperty
		{
			public _NativeClassProperty_498()
			{
			}

			public override int Get(Variant result, Dispatch2 objthis)
			{
				result.Set(RegExpClass.mLastRegExp);
				return Error.S_OK;
			}

			public override int Set(Variant param, Dispatch2 objthis)
			{
				return Error.E_ACCESSDENIED;
			}
		}
	}
}

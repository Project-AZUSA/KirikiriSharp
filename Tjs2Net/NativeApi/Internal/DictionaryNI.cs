/*
 * TJS2 CSharp
 */

using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	public class DictionaryNI : NativeInstanceObject
	{
		private WeakReference<CustomObject> mOwner;

		internal class AssignCallback : EnumMembersCallback
		{
			private CustomObject mOwner;

			public AssignCallback(CustomObject owner)
			{
				mOwner = owner;
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			public virtual bool Callback(string name, int flags, Variant value)
			{
				// hidden members are not copied
				if ((flags & Interface.HIDDENMEMBER) != 0)
				{
					return true;
				}
				mOwner.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP | flags, name, value
					, mOwner);
				return true;
			}
		}

		internal class SaveStructCallback : EnumMembersCallback
		{
			private AList<Dispatch2> mStack;

			private TextWriteStreamInterface mStream;

			private string mIndentStr;

			public bool mCalled;

			public SaveStructCallback(AList<Dispatch2> stack, TextWriteStreamInterface stream
				, string indent)
			{
				mStack = stack;
				mStream = stream;
				mIndentStr = indent;
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			public virtual bool Callback(string name, int flags, Variant value)
			{
				if ((flags & Interface.HIDDENMEMBER) != 0)
				{
					return true;
				}
				if (mCalled)
				{
					mStream.Write(",\n");
				}
				mCalled = true;
				mStream.Write(mIndentStr);
				mStream.Write("\"");
				mStream.Write(LexBase.EscapeC(name));
				mStream.Write("\" => ");
				if (value.IsObject())
				{
					// object
					VariantClosure clo = value.AsObjectClosure();
					ArrayNI.SaveStructuredDataForObject(clo.SelectObject(), mStack, mStream, mIndentStr
						);
				}
				else
				{
					mStream.Write(Utils.VariantToExpressionString(value));
				}
				return true;
			}
		}

		internal class AssignStructCallback : EnumMembersCallback
		{
			private AList<Dispatch2> mStack;

			private Dispatch2 mDest;

			public AssignStructCallback(AList<Dispatch2> stack, Dispatch2 dest)
			{
				mStack = stack;
				mDest = dest;
			}

			/// <exception cref="VariantException"></exception>
			/// <exception cref="TjsException"></exception>
			public virtual bool Callback(string name, int flags, Variant value)
			{
				if ((flags & Interface.HIDDENMEMBER) != 0)
				{
					return true;
				}
				if (value.IsObject())
				{
					// object
					Dispatch2 dsp = value.AsObject();
					// determin dsp's object type
					Variant val;
					if (dsp != null)
					{
						if (dsp.GetNativeInstance(DictionaryClass.ClassID) != null)
						{
							// dictionary
							bool objrec = false;
							int count = mStack.Count;
							for (int i = 0; i < count; i++)
							{
								Dispatch2 v = mStack[i];
								if (v == dsp)
								{
									// object recursion detected
									objrec = true;
									break;
								}
							}
							val = new Variant();
							if (objrec)
							{
								val.SetObject(null);
							}
							else
							{
								// becomes null
								Dispatch2 newobj = Tjs.CreateDictionaryObject();
								val.SetObject(newobj, newobj);
								DictionaryNI newni;
								if ((newni = (DictionaryNI)newobj.GetNativeInstance(DictionaryClass.ClassID)) != 
									null)
								{
									newni.AssignStructure(dsp, mStack);
								}
							}
						}
						else
						{
							if (dsp.GetNativeInstance(ArrayClass.ClassID) != null)
							{
								// array
								bool objrec = false;
								int count = mStack.Count;
								for (int i = 0; i < count; i++)
								{
									Dispatch2 v = mStack[i];
									if (v == dsp)
									{
										// object recursion detected
										objrec = true;
										break;
									}
								}
								val = new Variant();
								if (objrec)
								{
									val.SetObject(null);
								}
								else
								{
									// becomes null
									Dispatch2 newobj = Tjs.CreateArrayObject();
									val.SetObject(newobj, newobj);
									ArrayNI newni;
									if ((newni = (ArrayNI)newobj.GetNativeInstance(ArrayClass.ClassID)) != null)
									{
										newni.AssignStructure(dsp, mStack);
									}
								}
							}
							else
							{
								val = value;
							}
						}
					}
					else
					{
						// other object types
						val = value;
					}
					mDest.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP, name, val, mDest);
				}
				else
				{
					// other types
					mDest.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP, name, value, mDest);
				}
				return true;
			}
		}

		public DictionaryNI()
		{
		}

		// super(); // スーパークラスでは何もしていない
		//mOwner = null;
		public override int Construct(Variant[] param, Dispatch2 tjsobj)
		{
			// called from TJS constructor
			if (param != null && param.Length != 0)
			{
				return Error.E_BADPARAMCOUNT;
			}
			mOwner = new WeakReference<CustomObject>((CustomObject)(tjsobj));
			return Error.S_OK;
		}

		// Invalidate override
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public override void Invalidate()
		{
			// put here something on invalidation
			//mOwner = null;
            mOwner = new WeakReference<CustomObject>(null);
			base.Invalidate();
		}

		public virtual bool IsValid()
		{
			return mOwner.Get() != null;
		}

		// check validation
		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Assign(Dispatch2 dsp)
		{
			Assign(dsp, true);
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Assign(Dispatch2 dsp, bool clear)
		{
			// copy members from "dsp" to "Owner"
			// determin dsp's object type
			ArrayNI arrayni = null;
			CustomObject owner = mOwner.Get();
			if (dsp != null && (arrayni = (ArrayNI)dsp.GetNativeInstance(ArrayClass.ClassID))
				 != null)
			{
				// convert from array
				if (clear)
				{
					owner.Clear();
				}
				int count = arrayni.mItems.Count;
				for (int i = 0; i < count; i++)
				{
					Variant v = arrayni.mItems[i];
					string name = v.AsString();
					i++;
					if (i >= count)
					{
						break;
					}
					Variant v2 = arrayni.mItems[i];
					owner.PropSet(Interface.MEMBERENSURE | Interface.IGNOREPROP, name, v2, owner);
				}
			}
			else
			{
				// otherwise
				if (clear)
				{
					owner.Clear();
				}
				DictionaryNI.AssignCallback callback = new DictionaryNI.AssignCallback(owner);
				dsp.EnumMembers(Interface.IGNOREPROP, callback, dsp);
			}
		}

		public virtual void Clear()
		{
			CustomObject owner = mOwner.Get();
			if (owner != null)
			{
				owner.Clear();
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void SaveStructuredData(AList<Dispatch2> stack, TextWriteStreamInterface
			 stream, string indentstr)
		{
			stream.Write("(const) %[\n");
			string indentstr2 = indentstr + " ";
			DictionaryNI.SaveStructCallback callback = new DictionaryNI.SaveStructCallback(stack
				, stream, indentstr2);
			CustomObject owner = mOwner.Get();
			owner.EnumMembers(Interface.IGNOREPROP, callback, owner);
			if (callback.mCalled)
			{
				stream.Write("\n");
			}
			stream.Write(indentstr);
			stream.Write("]");
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void AssignStructure(Dispatch2 dsp, AList<Dispatch2> stack)
		{
			// assign structured data from dsp
			//ArrayNI dicni = null;
			if (dsp.GetNativeInstance(DictionaryClass.ClassID) != null)
			{
				// copy from dictionary
				stack.AddItem(dsp);
				try
				{
					CustomObject owner = mOwner.Get();
					owner.Clear();
					DictionaryNI.AssignStructCallback callback = new DictionaryNI.AssignStructCallback
						(stack, owner);
					dsp.EnumMembers(Interface.IGNOREPROP, callback, dsp);
				}
				finally
				{
					stack.Remove(stack.Count - 1);
				}
			}
			else
			{
				throw new TjsException(Error.SpecifyDicOrArray);
			}
		}

		public override string ToString()
		{
			CustomObject owner = mOwner.Get();
			if (owner != null)
			{
				return owner.ToString();
			}
			return base.ToString();
		}
	}
}

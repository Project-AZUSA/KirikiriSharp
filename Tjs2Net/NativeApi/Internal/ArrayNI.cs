/*
 * TJS2 CSharp
 */

using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	internal class ArrayNI : NativeInstanceObject
	{
		public AList<Variant> mItems;

		internal class DictionaryEnumCallback : EnumMembersCallback
		{
			public AList<Variant> mItems;

			public DictionaryEnumCallback(AList<Variant> items)
			{
				mItems = items;
			}

			public virtual bool Callback(string name, int flags, Variant value)
			{
				// hidden members are not processed
				if ((flags & Interface.HIDDENMEMBER) != 0)
				{
					return true;
				}
				// push items
				mItems.AddItem(new Variant(name));
				mItems.AddItem(new Variant(value));
				return true;
			}
		}

		public ArrayNI()
		{
			//super(); // スーパークラスでは何もしていない
			mItems = new AList<Variant>();
		}

		public override int Construct(Variant[] param, Dispatch2 tjsObj)
		{
			// called by TJS constructor
			if (param != null && param.Length != 0)
			{
				return Error.E_BADPARAMCOUNT;
			}
			return Error.S_OK;
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void Assign(Dispatch2 dsp)
		{
			// copy members from "dsp" to "Owner"
			// determin dsp's object type
			//Holder<ArrayNI> arrayni = new Holder<ArrayNI>(null);
			ArrayNI array = (ArrayNI)dsp.GetNativeInstance(ArrayClass.ClassID);
			if (array != null)
			{
				// copy from array
				mItems.Clear();
				int count = array.mItems.Count;
				for (int i = 0; i < count; i++)
				{
					mItems.AddItem(new Variant(array.mItems[i]));
				}
			}
			else
			{
				//mItems.addAll( array.mItems );
				// convert from dictionary or others
				mItems.Clear();
				ArrayNI.DictionaryEnumCallback callback = new ArrayNI.DictionaryEnumCallback(mItems
					);
				dsp.EnumMembers(Interface.IGNOREPROP, callback, dsp);
			}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual void SaveStructuredData(AList<Dispatch2> stack, TextWriteStreamInterface
			 stream, string indentstr)
		{
			stream.Write("(const) [\n");
			string indentstr2 = indentstr + " ";
			int count = mItems.Count;
			for (int i = 0; i < count; i++)
			{
				Variant v = mItems[i];
				stream.Write(indentstr2);
				if (v.IsObject())
				{
					// object
					VariantClosure clo = v.AsObjectClosure();
					SaveStructuredDataForObject(clo.SelectObject(), stack, stream, indentstr2);
				}
				else
				{
					stream.Write(Utils.VariantToExpressionString(v));
				}
				if (i != mItems.Count - 1)
				{
					// unless last
					stream.Write(",\n");
				}
				else
				{
					stream.Write("\n");
				}
			}
			stream.Write(indentstr);
			stream.Write("]");
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public static void SaveStructuredDataForObject(Dispatch2 dsp, AList<Dispatch2> stack
			, TextWriteStreamInterface stream, string indentstr)
		{
			// check object recursion
			int count = stack.Count;
			for (int i = 0; i < count; i++)
			{
				Dispatch2 d = stack[i];
				if (d == dsp)
				{
					// object recursion detected
					stream.Write("null /* object recursion detected */");
					return;
				}
			}
			// determin dsp's object type
			DictionaryNI dicni;
			ArrayNI arrayni;
			if (dsp != null)
			{
				dicni = (DictionaryNI)dsp.GetNativeInstance(DictionaryClass.ClassID);
				if (dicni != null)
				{
					// dictionary
					stack.AddItem(dsp);
					dicni.SaveStructuredData(stack, stream, indentstr);
					stack.Remove(stack.Count - 1);
					return;
				}
				else
				{
					arrayni = (ArrayNI)dsp.GetNativeInstance(ArrayClass.ClassID);
					if (arrayni != null)
					{
						// array
						stack.AddItem(dsp);
						arrayni.SaveStructuredData(stack, stream, indentstr);
						stack.Remove(stack.Count - 1);
						return;
					}
					else
					{
						// other objects
						stream.Write("null /* (object) \"");
						// stored as a null
						Variant val = new Variant(dsp, dsp);
						stream.Write(LexBase.EscapeC(val.AsString()));
						stream.Write("\" */");
						return;
					}
				}
			}
			stream.Write("null");
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual void AssignStructure(Dispatch2 dsp, AList<Dispatch2> stack)
		{
			// assign structured data from dsp
			ArrayNI arrayni = (ArrayNI)dsp.GetNativeInstance(ArrayClass.ClassID);
			if (arrayni != null)
			{
				// copy from array
				stack.AddItem(dsp);
				try
				{
					mItems.Clear();
					int count = arrayni.mItems.Count;
					for (int i = 0; i < count; i++)
					{
						Variant v = arrayni.mItems[i];
						if (v.IsObject())
						{
							// object
							Dispatch2 dsp1 = v.AsObject();
							// determin dsp's object type
							//DictionaryNI dicni = null;
							//ArrayNI arrayni1 = null;
							if (dsp1 != null && dsp1.GetNativeInstance(DictionaryClass.ClassID) != null)
							{
								//dicni = (DictionaryNI)ni.mValue;
								// dictionary
								bool objrec = false;
								int scount = stack.Count;
								for (int j = 0; j < scount; j++)
								{
									Dispatch2 d = stack[j];
									if (d == dsp1)
									{
										// object recursion detected
										objrec = true;
										break;
									}
								}
								if (objrec)
								{
									mItems.AddItem(new Variant());
								}
								else
								{
									// becomes null
									Dispatch2 newobj = Tjs.CreateDictionaryObject();
									mItems.AddItem(new Variant(newobj, newobj));
									DictionaryNI newni;
									if ((newni = (DictionaryNI)newobj.GetNativeInstance(DictionaryClass.ClassID)) != 
										null)
									{
										newni.AssignStructure(dsp1, stack);
									}
								}
							}
							else
							{
								if (dsp1 != null && dsp1.GetNativeInstance(ArrayClass.ClassID) != null)
								{
									// array
									bool objrec = false;
									int scount = stack.Count;
									for (int j = 0; j < scount; j++)
									{
										Dispatch2 d = stack[j];
										if (d == dsp1)
										{
											// object recursion detected
											objrec = true;
											break;
										}
									}
									if (objrec)
									{
										mItems.AddItem(new Variant());
									}
									else
									{
										// becomes null
										Dispatch2 newobj = Tjs.CreateArrayObject();
										mItems.AddItem(new Variant(newobj, newobj));
										ArrayNI newni;
										if ((newni = (ArrayNI)newobj.GetNativeInstance(ArrayClass.ClassID)) != null)
										{
											newni.AssignStructure(dsp1, stack);
										}
									}
								}
								else
								{
									// other object types
									mItems.AddItem(v);
								}
							}
						}
						else
						{
							// others
							mItems.AddItem(v);
						}
					}
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
	}
}

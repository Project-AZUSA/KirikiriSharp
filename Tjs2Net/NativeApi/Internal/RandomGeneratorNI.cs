/*
 * TJS2 CSharp
 */

using System.Text;
using Tjs2.Engine;
using Tjs2.Sharpen;
using Error = Tjs2.Engine.Error;

namespace Tjs2.NativeApi.Internal
{
	public class RandomGeneratorNI : NativeInstanceObject
	{
		private static RandomBits128 mRandomBits128 = null;

		public static void SetRandomBits128(RandomBits128 rbit128)
		{
			mRandomBits128 = rbit128;
		}

		private const int MT_N = 624;

		private MersenneTwister mGenerator;

		public RandomGeneratorNI() : base()
		{
		}

		~RandomGeneratorNI()
		{
			//mGenerator = null;
			mGenerator = null;
            //try
            //{
            //    base.Finalize();
            //}
            //catch
            //{
            //}
		}

		/// <exception cref="VariantException"></exception>
		/// <exception cref="TjsException"></exception>
		public virtual Dispatch2 Serialize()
		{
			// create dictionary object which has reconstructible information
			// which can be passed into constructor or randomize method.
			if (mGenerator == null)
			{
				return null;
			}
			Dispatch2 dic = null;
			Variant val = new Variant();
			// retrive tTJSMersenneTwisterData
			MersenneTwisterData data = mGenerator.GetData();
			// create 'state' string
			string state;
			StringBuilder p = new StringBuilder(MT_N * 8);
			for (int i = 0; i < MT_N; i++)
			{
				string hex = "0123456789abcdef";
				p.Append(hex[(int)((data.state.Get(i) >> 28) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 24) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 20) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 16) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 12) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 8) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 4) & unchecked((int)(0x000f)))]);
				p.Append(hex[(int)((data.state.Get(i) >> 0) & unchecked((int)(0x000f)))]);
			}
			state = p.ToString();
			// create dictionary and store information
			dic = Tjs.CreateDictionaryObject();
			val.Set(state);
			dic.PropSet(Interface.MEMBERENSURE, "state", val, dic);
			val.Set(data.left);
			dic.PropSet(Interface.MEMBERENSURE, "left", val, dic);
			val.Set(data.next);
			dic.PropSet(Interface.MEMBERENSURE, "next", val, dic);
			return dic;
		}

		/// <exception cref="TjsException"></exception>
		/// <exception cref="VariantException"></exception>
		public virtual void Randomize(Variant[] param)
		{
			if (param.Length == 0)
			{
				// parametor not given
				if (mRandomBits128 != null)
				{
					// another random generator is given
					//tjs_uint8 buf[32];
					//unsigned long tmp[32];
					ByteBuffer buf = ByteBuffer.AllocateDirect(32);
					mRandomBits128.GetRandomBits128(buf, 0);
					mRandomBits128.GetRandomBits128(buf, 16);
					int[] tmp = new int[32];
					for (int i = 0; i < 32; i++)
					{
						long num = (long)buf.Get(i) + ((long)buf.Get(i) << 8) + ((long)buf.Get(1) << 16) 
							+ ((long)buf.Get(i) << 24);
						tmp[i] = (int)(num > int.MaxValue ? num - unchecked((long)(0x100000000L)) : num);
					}
					if (mGenerator != null)
					{
						mGenerator = null;
					}
					mGenerator = new MersenneTwister(tmp);
				}
				else
				{
					if (mGenerator != null)
					{
						mGenerator = null;
					}
					mGenerator = new MersenneTwister(Runtime.CurrentTimeMillis());
				}
			}
			else
			{
				if (param.Length >= 1)
				{
					if (param[0].IsObject())
					{
						MersenneTwisterData data = null;
						try
						{
							// may be a reconstructible information
							VariantClosure clo = param[0].AsObjectClosure();
							if (clo.mObject == null)
							{
								throw new TjsException(Error.NullAccess);
							}
							string state;
							Variant val = new Variant();
							data = new MersenneTwisterData();
							// get state array
							//TJSThrowFrom_tjs_error
							int hr = clo.PropGet(Interface.MEMBERMUSTEXIST, "state", val, null);
							if (hr < 0)
							{
								Error.ThrowFrom_tjs_error(hr, null);
							}
							state = val.AsString();
							if (state.Length != MT_N * 8)
							{
								throw new TjsException(Error.NotReconstructiveRandomizeData);
							}
							int p = 0;
							for (int i = 0; i < MT_N; i++)
							{
								long n = 0;
								int tmp;
								for (int j = 0; j < 8; j++)
								{
									int c = state[p + j];
									tmp = -1;
									if (c >= '0' && c <= '9')
									{
										n = c - '0';
									}
									else
									{
										if (c >= 'a' && c <= 'f')
										{
											n = c - 'a' + 10;
										}
										else
										{
											if (c >= 'A' && c <= 'F')
											{
												n = c - 'A' + 10;
											}
										}
									}
									if (tmp == -1)
									{
										throw new TjsException(Error.NotReconstructiveRandomizeData);
									}
									else
									{
										n <<= 4;
										n += tmp;
									}
								}
								p += 8;
								data.state.Put(i, n & unchecked((long)(0xffffffffL)));
							}
							// get other members
							hr = clo.PropGet(Interface.MEMBERMUSTEXIST, "left", val, null);
							if (hr < 0)
							{
								Error.ThrowFrom_tjs_error(hr, null);
							}
							data.left = val.AsInteger();
							hr = clo.PropGet(Interface.MEMBERMUSTEXIST, "next", val, null);
							data.next = val.AsInteger();
							if (mGenerator != null)
							{
								mGenerator = null;
							}
							mGenerator = new MersenneTwister(data);
						}
						catch (VariantException)
						{
							data = null;
							throw new TjsException(Error.NotReconstructiveRandomizeData);
						}
						catch (TjsException)
						{
							data = null;
							throw new TjsException(Error.NotReconstructiveRandomizeData);
						}
						data = null;
					}
					else
					{
						// 64bitじゃなくて、32bit にしてしまっている。实用上问题あれば修正。
						int n = param[0].AsInteger();
						int[] tmp = new int[1];
						tmp[0] = n;
						if (mGenerator != null)
						{
							mGenerator = null;
						}
						mGenerator = new MersenneTwister(tmp);
					}
				}
			}
		}

		public virtual double Random()
		{
			// returns double precision random value x, x is in 0 <= x < 1
			if (mGenerator == null)
			{
				return 0;
			}
			return mGenerator.Rand_double();
		}

		public virtual int Random32()
		{
			// returns 63 bit integer random value
			if (mGenerator == null)
			{
				return 0;
			}
			return mGenerator.Int32();
		}

		public virtual long Random63()
		{
			// returns 63 bit integer random value
			if (mGenerator == null)
			{
				return 0;
			}
			long v;
			v = (long)mGenerator.Int32() << 32;
			v |= mGenerator.Int32();
			return v & unchecked((long)(0x7fffffffffffffffL));
		}

		public virtual long Random64()
		{
			// returns 64 bit integer random value
			if (mGenerator == null)
			{
				return 0;
			}
			long v;
			v = (long)mGenerator.Int32() << 32;
			v |= mGenerator.Int32();
			return v;
		}
	}
}

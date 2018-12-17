/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class MersenneTwister : MersenneTwisterData
	{
		private const int MT_N = 624;

		private const int N = 624;

		private const int M = 397;

		private const long MATRIX_A = unchecked((long)(0x9908b0dfL));

		private const long UMASK = unchecked((long)(0x80000000L));

		private const long LMASK = unchecked((long)(0x7fffffffL));

		public MersenneTwister() : this(5489L)
		{
		}

		public MersenneTwister(long s) : base()
		{
			//#define MIXBITS(u,v) ( ((u) & UMASK) | ((v) & LMASK) )
			//#define TWIST(u,v) ((MIXBITS(u,v) >> 1) ^ ((v)&1UL ? MATRIX_A : 0UL))
			left = 1;
			Init_genrand(s);
		}

		public MersenneTwister(int[] init_key) : base()
		{
			int i;
			int j;
			int k;
			Init_genrand(19650218L);
			i = 1;
			j = 0;
			k = (N > init_key.Length ? N : init_key.Length);
			for (; k > 0; k--)
			{
				state.Put(i, ((state.Get(i) ^ ((state.Get(i - 1) ^ (state.Get(i - 1) >> 30)) * 1664525
					)) + init_key[j] + j) & unchecked((long)(0xffffffffL)));
				i++;
				j++;
				if (i >= N)
				{
					state.Put(0, state.Get(N - 1));
					i = 1;
				}
				if (j >= init_key.Length)
				{
					j = 0;
				}
			}
			for (k = N - 1; k > 0; k--)
			{
				state.Put(i, ((state.Get(i) ^ ((state.Get(i - 1) ^ (state.Get(i - 1) >> 30)) * 1566083941
					)) - i) & unchecked((long)(0xffffffffL)));
				i++;
				if (i >= N)
				{
					state.Put(0, state.Get(N - 1));
					i = 1;
				}
			}
			state.Put(0, unchecked((long)(0x80000000L)));
			left = 1;
			next = 0;
		}

		public MersenneTwister(MersenneTwisterData data) : base()
		{
			SetData(data);
		}

		//protected void finalize() {}
		private void Init_genrand(long s)
		{
			state.Put(0, s & unchecked((long)(0xffffffffL)));
			for (int j = 1; j < N; j++)
			{
				state.Put(j, (1812433253L * (state.Get(j - 1) ^ (state.Get(j - 1) >> 30)) + j) & 
					unchecked((long)(0xffffffffL)));
			}
			left = 1;
			next = 0;
		}

		private void Next_state()
		{
			int p = 0;
			int j;
			left = N;
			next = 0;
			for (j = N - M + 1; (--j) > 0; p++)
			{
				long x = ((state.Get(p) & UMASK) | (state.Get(p + 1) & LMASK));
				long y = state.Get(p + M) ^ ((long)(((ulong)x) >> 1)) ^ ((x & 1) != 0 ? MATRIX_A : 
					0);
				state.Put(p, y);
			}
			for (j = M; (--j) > 0; p++)
			{
				long x = ((state.Get(p) & UMASK) | (state.Get(p + 1) & LMASK));
				long y = state.Get(p + M - N) ^ ((long)(((ulong)x) >> 1)) ^ ((x & 1) != 0 ? MATRIX_A
					 : 0);
				state.Put(p, y);
			}
			{
				long x = ((state.Get(p) & UMASK) | (state.Get(0) & LMASK));
				long y = state.Get(M - N) ^ ((long)(((ulong)x) >> 1)) ^ ((x & 1) != 0 ? MATRIX_A : 
					0);
				state.Put(p, y);
			}
		}

		public virtual int Int32()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long num = state.Get(next);
			int y = (int)(num > int.MaxValue ? num - unchecked((long)(0x100000000L)) : num);
			next++;
			y ^= ((int)(((uint)y) >> 11));
			y ^= (y << 7) & unchecked((int)(0x9d2c5680));
			y ^= (y << 15) & unchecked((int)(0xefc60000));
			y ^= ((int)(((uint)y) >> 18));
			return y;
		}

		public virtual int Int31()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long num = state.Get(left);
			int y = (int)(num > int.MaxValue ? num - unchecked((long)(0x100000000L)) : num);
			y ^= ((int)(((uint)y) >> 11));
			y ^= (y << 7) & unchecked((int)(0x9d2c5680));
			y ^= (y << 15) & unchecked((int)(0xefc60000));
			y ^= ((int)(((uint)y) >> 18));
			return (int)(((uint)y) >> 1);
		}

		internal virtual double Real1()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long y = state.Get(next);
			next++;
			y ^= ((long)(((ulong)y) >> 11));
			y ^= (y << 7) & unchecked((long)(0x9d2c5680L));
			y ^= (y << 15) & unchecked((long)(0xefc60000L));
			y ^= ((long)(((ulong)y) >> 18));
			return (double)y * (1.0 / 4294967295.0);
		}

		public virtual double Real2()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long y = state.Get(next);
			next++;
			y ^= ((long)(((ulong)y) >> 11));
			y ^= (y << 7) & unchecked((long)(0x9d2c5680L));
			y ^= (y << 15) & unchecked((long)(0xefc60000L));
			y ^= ((long)(((ulong)y) >> 18));
			return (double)y * (1.0 / 4294967296.0);
		}

		public virtual double Real3()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long y = state.Get(next);
			next++;
			y ^= ((long)(((ulong)y) >> 11));
			y ^= (y << 7) & unchecked((long)(0x9d2c5680L));
			y ^= (y << 15) & unchecked((long)(0xefc60000L));
			y ^= ((long)(((ulong)y) >> 18));
			return ((double)y + 0.5) * (1.0 / 4294967296.0);
		}

		public virtual double Res53()
		{
			if (--left == 0)
			{
				Next_state();
			}
			long a = state.Get(next);
			next++;
			a ^= ((long)(((ulong)a) >> 11));
			a ^= (a << 7) & unchecked((int)(0x9d2c5680));
			a ^= (a << 15) & unchecked((int)(0xefc60000));
			a ^= ((long)(((ulong)a) >> 18));
			a = (long)(((ulong)a) >> 5);
			if (--left == 0)
			{
				Next_state();
			}
			long b = state.Get(next);
			next++;
			b ^= ((long)(((ulong)b) >> 11));
			b ^= (b << 7) & unchecked((int)(0x9d2c5680));
			b ^= (b << 15) & unchecked((int)(0xefc60000));
			b ^= ((long)(((ulong)b) >> 18));
			b = (long)(((ulong)b) >> 6);
			return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
		}

		public virtual double Rand_double()
		{
			long y;
			{
				if (--left == 0)
				{
					Next_state();
				}
				y = state.Get(next);
				next++;
				y ^= ((long)(((ulong)y) >> 11));
				y ^= (y << 7) & unchecked((long)(0x9d2c5680L));
				y ^= (y << 15) & unchecked((long)(0xefc60000L));
				y ^= ((long)(((ulong)y) >> 18));
			}
			long v = (y & unchecked((long)(0xffffffffL))) << 32;
			{
				if (--left == 0)
				{
					Next_state();
				}
				y = state.Get(next);
				next++;
				y ^= ((long)(((ulong)y) >> 11));
				y ^= (y << 7) & unchecked((long)(0x9d2c5680L));
				y ^= (y << 15) & unchecked((long)(0xefc60000L));
				y ^= ((long)(((ulong)y) >> 18));
			}
			v |= (y & unchecked((long)(0xffffffffL)));
			v &= unchecked((long)(0x000fffffffffffffL));
			v = v | (1023L << 52);
			// at this point, v is : 1.0 <= v < 2.0
			return Double.LongBitsToDouble(v) - 1.0;
		}

		// returned value x is : 0.0 <= x < 1.0
		public MersenneTwisterData GetData()
		{
			return this;
		}

		public virtual void SetData(MersenneTwisterData rhs)
		{
			// copy
			ByteBuffer src = rhs.state.Duplicate();
			src.Position(0);
			src.Limit(MT_N);
			ByteBuffer buff = ByteBuffer.AllocateDirect(MT_N * 8);
            buff.Order(ByteBuffer.NativeOrder());
			state = buff.AsLongBuffer();
			state.Clear();
			state.Put(src);
			next = rhs.next;
			left = rhs.left;
		}
	}
}

/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	internal class VariantPool
	{
		private const int DEFAULT_SIZE = 64;

		private const int LIMIT_SIZE = 48;

		private AList<Variant> mPool;

		public VariantPool()
		{
			mPool = new AList<Variant>(DEFAULT_SIZE);
		}

		public Variant Allocate()
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant();
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				return ret;
			}
		}

		public virtual Variant Allocate(int value)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(value);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(value);
				return ret;
			}
		}

		public virtual Variant Allocate(double value)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(value);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(value);
				return ret;
			}
		}

		public virtual Variant Allocate(ByteBuffer value)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(value);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(value);
				return ret;
			}
		}

		public virtual Variant Allocate(string value)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(value);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(value);
				return ret;
			}
		}

		public virtual Variant Allocate(Variant value)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(value);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(value);
				return ret;
			}
		}

		public virtual Variant Allocate(Dispatch2 dsp, Dispatch2 dsp2)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(dsp, dsp2);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(dsp, dsp2);
				return ret;
			}
		}

		public virtual Variant Allocate(Dispatch2 dsp)
		{
			int len = mPool.Count;
			if (len == 0)
			{
				return new Variant(dsp, null);
			}
			else
			{
				Variant ret = mPool[len - 1];
				mPool.Remove(len - 1);
				ret.Set(dsp, null);
				return ret;
			}
		}

		public void Release(Variant va)
		{
			if (va == null)
			{
				return;
			}
			va.Clear();
			int poolSize = mPool.Count;
			if (poolSize < LIMIT_SIZE)
			{
				mPool.AddItem(va);
			}
		}

		public void Release(Variant[] va)
		{
			if (va == null)
			{
				return;
			}
			int poolSize = mPool.Count;
			if (poolSize < LIMIT_SIZE)
			{
				int count = va.Length;
				for (int i = 0; i < count; i++)
				{
					va[i].Clear();
					mPool.AddItem(va[i]);
				}
			}
		}
	}
}

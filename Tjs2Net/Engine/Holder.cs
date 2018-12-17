/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class Holder<T>
	{
		public T mValue;

		public Holder(T v)
		{
			this.mValue = v;
		}

		public virtual T Get()
		{
			return mValue;
		}

		public virtual void Set(T v)
		{
			mValue = v;
		}

		public sealed override string ToString()
		{
			if (mValue == null)
			{
				return "Hold : null";
			}
			return "Hold : " + mValue.ToString();
		}
	}
}

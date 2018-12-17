/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	public class LongQue
	{
		private const int DEFAULT_SIZE = 16;

		private long[] mItems;

		private int mFront;

		private int mTail;

		public LongQue()
		{
			mItems = new long[DEFAULT_SIZE];
		}

		public virtual bool IsEmpty()
		{
			return (mFront == mTail);
		}

		public virtual long Front()
		{
			return mItems[mFront];
		}

		public virtual long Pop_front()
		{
			long ret = mItems[mFront];
			if (mFront == mTail)
			{
				return ret;
			}
			else
			{
				// データが空っぽ
				int mask = mItems.Length - 1;
				// 16の倍数になるので-1でマスクが作れる
				mFront = (mFront + 1) & mask;
			}
			return ret;
		}

		public virtual void Push_back(long v)
		{
			int mask = mItems.Length - 1;
			// 16の倍数になるので-1でマスクが作れる
			int tail = (mTail + 1) & mask;
			if (tail == mFront)
			{
				// 溢れる
				int count = mItems.Length << 1;
				long[] newArray = new long[count];
				if (mTail < mFront)
				{
					int copySize = mItems.Length - mFront;
					System.Array.Copy(mItems, mFront, newArray, 0, copySize);
					// front ～ 末尾までコピー
					System.Array.Copy(mItems, 0, newArray, copySize, mTail);
					// 先端 ～ tailまでコピー
					mTail = copySize + mTail;
				}
				else
				{
					// mFront == 0
					System.Array.Copy(mItems, 0, newArray, 0, mTail);
					// 先端 ～ tailまでコピー
					//mTail = mTail;
				}
				mFront = 0;
				mItems = newArray;
				mItems[mTail] = v;
				mTail++;
			}
			else
			{
				mItems[mTail] = v;
				mTail = tail;
			}
		}
	}
}

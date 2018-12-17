/*
 * TJS2 CSharp
 */

using System;

namespace Tjs2.Engine
{
	public class IntVector
	{
		private const int DEFAULT_SIZE = 16;

		private int[] mItems;

		private int mIndex;

		public IntVector() : this(DEFAULT_SIZE)
		{
		}

		public IntVector(int initialCapacity)
		{
			mItems = new int[initialCapacity];
		}

		public IntVector(int[] datasrc, int index)
		{
			mItems = new int[index];
			mIndex = index;
			for (int i = 0; i < index; i++)
			{
				mItems[i] = datasrc[i];
			}
		}

		private IntVector(int[] src)
		{
			mItems = src;
			mIndex = src.Length;
		}

		public static IntVector Wrap(int[] src)
		{
			return new IntVector(src);
		}

		private void Resize()
		{
			int count = mItems.Length * 2;
			int[] newArray = new int[count];
			System.Array.Copy(mItems, 0, newArray, 0, mItems.Length);
			mItems = null;
			mItems = newArray;
		}

		public void Clear()
		{
			mIndex = 0;
		}

		public void Push_back(int val)
		{
			if (mIndex < mItems.Length)
			{
				mItems[mIndex] = val;
				mIndex++;
			}
			else
			{
				Resize();
				if (mIndex < mItems.Length)
				{
					mItems[mIndex] = val;
					mIndex++;
				}
				else
				{
					throw new OutOfMemoryException(Error.InternalError);
				}
			}
		}

		public void Add(int val)
		{
			if (mIndex < mItems.Length)
			{
				mItems[mIndex] = val;
				mIndex++;
			}
			else
			{
				Resize();
				if (mIndex < mItems.Length)
				{
					mItems[mIndex] = val;
					mIndex++;
				}
				else
				{
					throw new OutOfMemoryException(Error.InternalError);
				}
			}
		}

		public int Size()
		{
			return mIndex;
		}

		/// <exception cref="System.IndexOutOfRangeException"></exception>
		public int LastElement()
		{
			if (mIndex == 0)
			{
				throw new IndexOutOfRangeException();
			}
			return mItems[mIndex - 1];
		}

		/// <exception cref="System.IndexOutOfRangeException"></exception>
		public int Back()
		{
			if (mIndex == 0)
			{
				throw new IndexOutOfRangeException();
			}
			return mItems[mIndex - 1];
		}

		/// <exception cref="System.IndexOutOfRangeException"></exception>
		public int Top()
		{
			if (mIndex == 0)
			{
				throw new IndexOutOfRangeException();
			}
			return mItems[0];
		}

		public void Pop_back()
		{
			if (mIndex > 0)
			{
				mIndex--;
			}
		}

		public void Set(int index, int element)
		{
			if (index >= 0 && index < mIndex)
			{
				mItems[index] = element;
			}
		}

		/// <exception cref="System.IndexOutOfRangeException"></exception>
		public int Get(int index)
		{
			if (index < 0 && index >= mIndex)
			{
				throw new IndexOutOfRangeException();
			}
			return mItems[index];
		}

		/// <exception cref="System.IndexOutOfRangeException"></exception>
		public void Remove(int index)
		{
			if (index < 0 && index >= mIndex)
			{
				throw new IndexOutOfRangeException();
			}
			int count = mIndex - 1;
			for (int i = index; i < count; i++)
			{
				mItems[i] = mItems[i + 1];
			}
			mIndex--;
		}

		public virtual IntVector Clone()
		{
			return new IntVector(mItems, mIndex);
		}

		public virtual bool IsEmpty()
		{
			return mIndex == 0;
		}

		public virtual int[] ToArray()
		{
			int[] ret = new int[mIndex];
			System.Array.Copy(mItems, 0, ret, 0, mIndex);
			return ret;
		}

		public virtual int[] GetRowArray()
		{
			return mItems;
		}
	}
}

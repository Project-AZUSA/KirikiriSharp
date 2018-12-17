/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class LocalNamespace
	{
		private VectorWrap<LocalSymbolList> mLevels;

		private int mMaxCount;

		private int mCurrentCount;

		private MaxCountWriter mMaxCountWriter;

		public LocalNamespace()
		{
			//mMaxCount = 0;
			//mCurrentCount = 0;
			mLevels = new VectorWrap<LocalSymbolList>();
		}

		public virtual void SetMaxCountWriter(MaxCountWriter writer)
		{
			mMaxCountWriter = writer;
		}

		public virtual int GetCount()
		{
			int count = 0;
			int size = mLevels.Count;
			for (int i = 0; i < size; i++)
			{
				LocalSymbolList list = mLevels[i];
				count += list.GetCount();
			}
			return count;
		}

		public virtual void Push()
		{
			mCurrentCount = GetCount();
			mLevels.AddItem(new LocalSymbolList(mCurrentCount));
		}

		public virtual void Pop()
		{
			LocalSymbolList list = mLevels.LastElement();
			Commit();
			mCurrentCount = list.GetLocalCountStart();
			mLevels.Remove(mLevels.Count - 1);
			list = null;
		}

		public virtual int Find(string name)
		{
			int count = mLevels.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				LocalSymbolList list = mLevels[i];
				int lindex = list.Find(name);
				if (lindex != -1)
				{
					return lindex + list.GetLocalCountStart();
				}
			}
			return -1;
		}

		public virtual int GetLevel()
		{
			return mLevels.Count;
		}

		public virtual void Add(string name)
		{
			LocalSymbolList top = GetTopSymbolList();
			if (top == null)
			{
				return;
			}
			top.Add(name);
		}

		public virtual void Remove(string name)
		{
			int count = mLevels.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				LocalSymbolList list = mLevels[i];
				int lindex = list.Find(name);
				if (lindex != -1)
				{
					list.Remove(lindex);
					return;
				}
			}
		}

		public virtual void Commit()
		{
			int count = 0;
			for (int i = mLevels.Count - 1; i >= 0; i--)
			{
				LocalSymbolList list = mLevels[i];
				count += list.GetCount();
			}
			if (mMaxCount < count)
			{
				mMaxCount = count;
				if (mMaxCountWriter != null)
				{
					mMaxCountWriter.SetMaxCount(count);
				}
			}
		}

		public virtual LocalSymbolList GetTopSymbolList()
		{
			if (mLevels.Count == 0)
			{
				return null;
			}
			return mLevels.LastElement();
		}

		public virtual void Clear()
		{
			while (mLevels.Count > 0)
			{
				Pop();
			}
		}

		public virtual int GetMaxCount()
		{
			return mMaxCount;
		}
	}
}

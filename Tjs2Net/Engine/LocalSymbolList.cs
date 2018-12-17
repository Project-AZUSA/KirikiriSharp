/*
 * TJS2 CSharp
 */

using Tjs2.Sharpen;

namespace Tjs2.Engine
{
	public class LocalSymbolList
	{
		private AList<string> mList;

		private int mLocalCountStart;

		public LocalSymbolList(int localCount)
		{
			//private int mStartWrite;
			//private int mCountWrite;
			mLocalCountStart = localCount;
			//mStartWrite = mCountWrite = 0;
			mList = new AList<string>();
		}

		public virtual void Add(string name)
		{
			if (Find(name) == -1)
			{
				string str = name;
				int size = mList.Count;
				for (int i = 0; i < size; i++)
				{
					string s = mList[i];
					if (s == null)
					{
						mList.Set(i, str);
					}
				}
				mList.AddItem(str);
			}
		}

		public virtual int Find(string name)
		{
			int size = mList.Count;
			for (int i = 0; i < size; i++)
			{
				string str = mList[i];
				if (str != null && name.Equals(str))
				{
					return i;
				}
			}
			return -1;
		}

		public virtual void Remove(string name)
		{
			int index = Find(name);
			if (index != -1)
			{
				mList.Set(index, null);
			}
		}

		public virtual void Remove(int index)
		{
			if (index != -1)
			{
				mList.Set(index, null);
			}
		}

		public virtual int GetCount()
		{
			return mList.Count;
		}

		public virtual int GetLocalCountStart()
		{
			return mLocalCountStart;
		}
	}
}

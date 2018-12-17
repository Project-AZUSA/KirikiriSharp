/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	/// <summary>最大格纳数制限付きハッシュテーブル</summary>
	/// <?></?>
	/// <?></?>
	public class HashCache<Key, Value> : HashTable<Key, Value> where Key : class where Value:class
	{
		/// <summary>格纳可能な最大要素数</summary>
		private int mMaxCount;

		/// <summary>コンストラクタ</summary>
		/// <param name="maxcount">格纳可能な最大数</param>
		public HashCache(int maxcount)
		{
			mMaxCount = maxcount;
		}

		/// <summary>要素の追加、最大数を超えた时は古いものが削除される</summary>
		/// <param name="key">キー</param>
		/// <param name="value">格纳のする值</param>
		public override void Put(Key key, Value value)
		{
			base.Put(key, value);
			if (GetCount() > mMaxCount)
			{
				ChopLast(GetCount() - mMaxCount);
			}
		}

		/// <summary>格纳可能な数の设定</summary>
		/// <param name="maxcount">格纳可能な数</param>
		public virtual void SetMaxCount(int maxcount)
		{
			mMaxCount = maxcount;
			if (GetCount() > mMaxCount)
			{
				ChopLast(GetCount() - mMaxCount);
			}
		}

		/// <summary>格纳可能な数を得る</summary>
		/// <returns>格纳可能な数</returns>
		public virtual int GetMaxCount()
		{
			return mMaxCount;
		}
	}
}

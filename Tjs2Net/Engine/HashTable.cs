/*
 * TJS2 CSharp
 */

namespace Tjs2.Engine
{
	/// <summary>
	/// 追加顺を持ったハッシュテーブル、配列の扩大は行われない
	/// 古いものを削除できる
	/// </summary>
	/// <?></?>
	/// <?></?>
	public class HashTable<Key, Value> where Key : class where Value:class
	{
		/// <summary>デフォルト配列サイズ</summary>
		private const int DEFAULT_HASH_SIZE = 64;

		/// <summary>使用中フラグ</summary>
		private const int HASH_USING = unchecked((int)(0x1));

		/// <summary>配列に直に入っている要素フラグ</summary>
		private const int HASH_LV1 = unchecked((int)(0x2));

		/// <summary>各要素</summary>
		/// <?></?>
		/// <?></?>
		internal class Element<Key, Value> where Key : class where Value:class
		{
			/// <summary>ハッシュ值</summary>
			internal int mHash;

			/// <summary>内部で使用するフラグ</summary>
			internal int mFlags;

			/// <summary>キー</summary>
			internal Key mKey;

			/// <summary>格纳する值</summary>
			internal Value mValue;

			/// <summary>アイテムチェーンで前のアイテム</summary>
			internal HashTable<Key, Value>.Element<Key, Value> mPrev;

			/// <summary>アイテムチェーンで次のアイテム</summary>
			internal HashTable<Key, Value>.Element<Key, Value> mNext;

			/// <summary>追加顺で直前に追加されたアイテム</summary>
			internal HashTable<Key, Value>.Element<Key, Value> mNPrev;

			/// <summary>追加顺で直后に追加されたアイテム</summary>
			internal HashTable<Key, Value>.Element<Key, Value> mNNext;
		}

		/// <summary>要素配列</summary>
		private HashTable<Key, Value>.Element<Key, Value>[] mElms;

		/// <summary>实要素数</summary>
		private int mCount;

		/// <summary>追加顺で最初に追加されたアイテム</summary>
		private HashTable<Key, Value>.Element<Key, Value> mNFirst;

		/// <summary>追加顺で最后に追加されたアイテム</summary>
		private HashTable<Key, Value>.Element<Key, Value> mNLast;

		/// <summary>
		/// デフォルトコンストラクタ
		/// 要素数は DEFAULT_HASH_SIZE となる。
		/// </summary>
		public HashTable() : this(DEFAULT_HASH_SIZE)
		{
		}

		/// <summary>コンストラクタ</summary>
		/// <param name="initCapacity">初期サイズ</param>
		public HashTable(int initCapacity)
		{
			// サイズが必ず2の累乘值になるようにする
			int capacity = 1;
			while (capacity < initCapacity)
			{
				capacity <<= 1;
			}
			mElms = new HashTable<Key, Value>.Element<Key,Value>[capacity];
		}

		/// <summary>全要素を削除する</summary>
		public virtual void Clear()
		{
			InternalClear();
		}

		/// <summary>キーに对应した值を得る</summary>
		/// <param name="key">キー</param>
		/// <returns>キーに对应した值</returns>
		public virtual Value Get(Key key)
		{
			if (key == null)
			{
				return null;
			}
			HashTable<Key, Value>.Element<Key, Value> e = InternalFindWithHash(key, key.GetHashCode());
			if (e == null)
			{
				return null;
			}
			return e.mValue;
		}

		/// <summary>キーに对应した值を得つつ、并び顺で一番新しいものとする</summary>
		/// <param name="key">キー</param>
		/// <returns>キーに对应した值</returns>
		public virtual Value GetAndTouch(Key key)
		{
			if (key == null)
			{
				return null;
			}
			HashTable<Key, Value>.Element<Key, Value> e = InternalFindWithHash(key, key.GetHashCode());
			if (e == null)
			{
				return null;
			}
			CheckUpdateElementOrder(e);
			return e.mValue;
		}

		/// <summary>キーに对应した要素を探す</summary>
		/// <param name="key">キー</param>
		/// <param name="hash">ハッシュ值</param>
		/// <returns>キーに对应した要素</returns>
		private HashTable<Key, Value>.Element<Key, Value> InternalFindWithHash(Key key, int hash)
		{
			// find key ( hash )
			int mask = mElms.Length - 1;
			HashTable<Key, Value>.Element<Key, Value> lv1 = mElms[hash & mask];
			if (lv1 == null)
			{
				return null;
			}
			if (hash == lv1.mHash && (lv1.mFlags & HASH_USING) != 0)
			{
				if (key.Equals(lv1.mKey))
				{
					return lv1;
				}
			}
			HashTable<Key, Value>.Element<Key, Value> elm = lv1.mNext;
			while (elm != null)
			{
				if (hash == elm.mHash)
				{
					if (key.Equals(elm.mKey))
					{
						return elm;
					}
				}
				elm = elm.mNext;
			}
			return null;
		}

		// not found
		/// <summary>キーと值のペアを格纳する</summary>
		/// <param name="key">キー</param>
		/// <param name="value">格纳する值</param>
		public virtual void Put(Key key, Value value)
		{
			if (key == null)
			{
				return;
			}
			AddWithHash(key, key.GetHashCode(), value);
		}

		/// <summary>キーと值のペアを格纳する</summary>
		/// <param name="key">キー</param>
		/// <param name="hash">ハッシュ值</param>
		/// <param name="value">值</param>
		private void AddWithHash(Key key, int hash, Value value)
		{
			int mask = mElms.Length - 1;
			int index = hash & mask;
			HashTable<Key, Value>.Element<Key, Value> lv1 = mElms[index];
			if (lv1 == null)
			{
				lv1 = new HashTable<Key, Value>.Element<Key, Value>();
				mElms[index] = lv1;
				lv1.mFlags = HASH_LV1;
			}
			HashTable<Key, Value>.Element<Key, Value> elm = lv1.mNext;
			while (elm != null)
			{
				if (hash == elm.mHash)
				{
					if (key.Equals(elm.mKey))
					{
						CheckUpdateElementOrder(elm);
						elm.mValue = value;
						return;
					}
				}
				elm = elm.mNext;
			}
			if ((lv1.mFlags & HASH_USING) == 0)
			{
				lv1.mKey = key;
				lv1.mValue = value;
				lv1.mFlags |= HASH_USING;
				lv1.mHash = hash;
				lv1.mPrev = null;
				CheckAddingElementOrder(lv1);
				return;
			}
			if (hash == lv1.mHash)
			{
				if (key.Equals(lv1.mHash))
				{
					CheckUpdateElementOrder(lv1);
					lv1.mValue = value;
					return;
				}
			}
			// insert after lv1
			HashTable<Key, Value>.Element<Key, Value> newelm = new HashTable<Key, Value>.Element<Key, Value>();
			//newelm.mFlags = 0;
			newelm.mKey = key;
			newelm.mValue = value;
			newelm.mFlags |= HASH_USING;
			newelm.mHash = hash;
			if (lv1.mNext != null)
			{
				lv1.mNext.mPrev = newelm;
			}
			newelm.mNext = lv1.mNext;
			newelm.mPrev = lv1;
			lv1.mNext = newelm;
			CheckAddingElementOrder(newelm);
		}

		/// <summary>キーに对应した要素を削除する</summary>
		/// <param name="key">キー</param>
		/// <returns>实际に削除したかどうか</returns>
		internal virtual bool Remove(Key key)
		{
			if (key == null)
			{
				return false;
			}
			return DeleteWithHash(key, key.GetHashCode());
		}

		/// <summary>キーに对应した要素を削除する</summary>
		/// <param name="key">キー</param>
		/// <param name="hash">ハッシュ值</param>
		/// <returns>实际に削除したかどうか</returns>
		private bool DeleteWithHash(Key key, int hash)
		{
			// delete key ( hash ) and return true if succeeded
			int mask = mElms.Length - 1;
			HashTable<Key, Value>.Element<Key, Value> lv1 = mElms[hash & mask];
			if (lv1 == null)
			{
				return false;
			}
			if ((lv1.mFlags & HASH_USING) != 0 && hash == lv1.mHash)
			{
				if (key.Equals(lv1.mKey))
				{
					// delete lv1
					CheckDeletingElementOrder(lv1);
					lv1.mKey = null;
					lv1.mValue = null;
					lv1.mFlags &= ~HASH_USING;
					return true;
				}
			}
			HashTable<Key, Value>.Element<Key, Value> prev = lv1;
			HashTable<Key, Value>.Element<Key, Value> elm = lv1.mNext;
			while (elm != null)
			{
				if (hash == elm.mHash)
				{
					if (key.Equals(elm.mKey))
					{
						CheckDeletingElementOrder(elm);
						elm.mKey = null;
						elm.mValue = null;
						elm.mFlags &= ~HASH_USING;
						prev.mNext = elm.mNext;
						// sever from the chain
						if (elm.mNext != null)
						{
							elm.mNext.mPrev = prev;
						}
						elm.mNext = null;
						elm.mPrev = null;
						return true;
					}
				}
				prev = elm;
				elm = elm.mNext;
			}
			return false;
		}

		// not found
		public virtual int GetCount()
		{
			return mCount;
		}

		/// <summary>古いものから指定个数削除する</summary>
		/// <param name="count">削除する数</param>
		/// <returns>实际に削除した数</returns>
		public virtual int ChopLast(int count)
		{
			int ret = 0;
			while (count > 0)
			{
				count--;
				if (mNLast == null)
				{
					break;
				}
				DeleteBytElement(mNLast);
				ret++;
			}
			return ret;
		}

		/// <summary>指定要素を削除する</summary>
		/// <param name="elm">削除する要素</param>
		/// <returns>配列要素かどうか</returns>
		private bool DeleteBytElement(HashTable<Key, Value>.Element<Key, Value> elm)
		{
			CheckDeletingElementOrder(elm);
			elm.mKey = null;
			elm.mValue = null;
			elm.mFlags &= ~HASH_USING;
			if ((elm.mFlags & HASH_LV1) != 0)
			{
				// lv1 element
				// nothing to do
				return false;
			}
			else
			{
				// other elements
				if (elm.mPrev != null)
				{
					elm.mPrev.mNext = elm.mNext;
				}
				if (elm.mNext != null)
				{
					elm.mNext.mPrev = elm.mPrev;
				}
				return true;
			}
		}

		/// <summary>要素削除に伴い并び顺を更新する</summary>
		/// <param name="elm">削除对象要素</param>
		private void CheckDeletingElementOrder(HashTable<Key, Value>.Element<Key, Value> elm)
		{
			mCount--;
			if (mCount > 0)
			{
				if (elm == mNFirst)
				{
					// deletion of first item
					mNFirst = elm.mNNext;
					mNFirst.mNPrev = null;
				}
				else
				{
					if (elm == mNLast)
					{
						// deletion of last item
						mNLast = elm.mNPrev;
						mNLast.mNNext = null;
					}
					else
					{
						// deletion of intermediate item
						elm.mNPrev.mNNext = elm.mNNext;
						elm.mNNext.mNPrev = elm.mNPrev;
					}
				}
			}
			else
			{
				// when the count becomes zero...
				mNFirst = mNLast = null;
			}
		}

		/// <summary>指定要素の并び顺を更新する</summary>
		/// <param name="elm">先头に持ってくる要素</param>
		private void CheckUpdateElementOrder(HashTable<Key, Value>.Element<Key, Value> elm)
		{
			// move elm to the front of addtional order
			if (elm != mNFirst)
			{
				if (mNLast == elm)
				{
					mNLast = elm.mNPrev;
				}
				elm.mNPrev.mNNext = elm.mNNext;
				if (elm.mNNext != null)
				{
					elm.mNNext.mNPrev = elm.mNPrev;
				}
				elm.mNNext = mNFirst;
				elm.mNPrev = null;
				mNFirst.mNPrev = elm;
				mNFirst = elm;
			}
		}

		/// <summary>要素追加に伴い并び顺を更新する</summary>
		/// <param name="elm">追加する要素</param>
		private void CheckAddingElementOrder(HashTable<Key, Value>.Element<Key, Value> elm)
		{
			if (mCount == 0)
			{
				mNLast = elm;
				// first addition
				elm.mNNext = null;
			}
			else
			{
				mNFirst.mNPrev = elm;
				elm.mNNext = mNFirst;
			}
			mNFirst = elm;
			elm.mNPrev = null;
			mCount++;
		}

		/// <summary>初期化</summary>
		private void InternalInit()
		{
			mCount = 0;
			mNFirst = null;
			mNLast = null;
		}

		/// <summary>全要素クリア</summary>
		private void InternalClear()
		{
			int count = mElms.Length;
			for (int i = 0; i < count; i++)
			{
				if (mElms[i] != null)
				{
					HashTable<Key, Value>.Element<Key, Value> e = mElms[i].mNext;
					while (e != null)
					{
						e.mKey = null;
						e.mValue = null;
						e.mFlags &= ~HASH_USING;
						HashTable<Key, Value>.Element<Key, Value> next = e.mNext;
						e.mPrev = null;
						e.mNext = null;
						e.mNPrev = null;
						e.mNNext = null;
						e = next;
					}
					e = mElms[i];
					if ((e.mFlags & HASH_USING) != 0)
					{
						e.mKey = null;
						e.mValue = null;
						e.mFlags &= ~HASH_USING;
						e.mPrev = null;
						e.mNext = null;
						e.mNPrev = null;
						e.mNNext = null;
					}
				}
			}
			InternalInit();
		}

		public virtual Value GetLastValue()
		{
			if (mNLast != null)
			{
				return mNLast.mValue;
			}
			return null;
		}
	}
}

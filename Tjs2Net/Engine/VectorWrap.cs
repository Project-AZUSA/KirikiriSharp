/*
 * TJS2 CSharp
 */

using System;
using System.Collections.Generic;
using Tjs2.Sharpen;

namespace Tjs2.Engine
{
#if WIN32
    [Serializable]
#endif
    public class VectorWrap<E> : AList<E>
	{
		private const long serialVersionUID = -8162877809635079713L;

		public VectorWrap() : base()
		{
		}

		public VectorWrap(ICollection<E> c) : base(c)
		{
		}

		public VectorWrap(int initialCapacity) : base(initialCapacity)
		{
		}

		// vector を ArrayList で置き换えるためのクラス
		public virtual E LastElement()
		{
			return this[Count - 1];
		}

		public virtual E Back()
		{
			return this[Count - 1];
		}

		public virtual void Pop_back()
		{
			RemoveAt(Count - 1);
		}
	}
}

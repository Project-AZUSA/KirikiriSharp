using System.Collections;
using System.Collections.Generic;

namespace Tjs2.Sharpen
{
    public abstract class Iterable<T> : IEnumerable, IEnumerable<T>
	{
		protected Iterable ()
		{
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return this.Iterator ();
		}

		public abstract Iterator<T> Iterator ();
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.Iterator ();
		}
	}
}

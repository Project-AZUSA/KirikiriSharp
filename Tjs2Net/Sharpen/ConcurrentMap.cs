using System.Collections;
using System.Collections.Generic;

namespace Tjs2.Sharpen
{
    internal interface ConcurrentMap<T, U> : IEnumerable, IDictionary<T, U>, IEnumerable<KeyValuePair<T, U>>, ICollection<KeyValuePair<T, U>>
	{
		U PutIfAbsent (T key, U value);
		bool Remove (object key, object value);
		bool Replace (T key, U oldValue, U newValue);
	}
}

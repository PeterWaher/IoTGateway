using System;
using System.Collections.Generic;

namespace Waher.Utility.ExStat
{
	public class Histogram<T>
	{
		public SortedDictionary<T, Bucket> Buckets = new SortedDictionary<T, Bucket>();

		public void Inc(T Key, params string[] SubKeys)
		{
			if (!this.Buckets.TryGetValue(Key, out Bucket Bucket))
			{
				Bucket = new Bucket();
				this.Buckets[Key] = Bucket;
			}

			Bucket.Inc(SubKeys);
		}

		public void Join(T Key, int Count, bool First, params string[] SubKeys)
		{
			if (!this.Buckets.TryGetValue(Key, out Bucket Bucket))
			{
				if (!First)
					return;

				Bucket = new Bucket();
				this.Buckets[Key] = Bucket;
			}

			Bucket.Join(Count, First, SubKeys);
		}

		public void RemoveUntouched()
		{
			LinkedList<T> ToRemove = new LinkedList<T>();

			foreach (KeyValuePair<T, Bucket> P in Buckets)
			{
				if (P.Value.Touched)
				{
					P.Value.Touched = false;
					P.Value.RemoveUntouched();
				}
				else
					ToRemove.AddLast(P.Key);
			}

			foreach (T Key in ToRemove)
				Buckets.Remove(Key);
		}
	}
}

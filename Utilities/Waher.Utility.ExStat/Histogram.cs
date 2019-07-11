using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Utility.ExStat
{
    public class Histogram<T>
    {
        public SortedDictionary<T, Bucket> Buckets = new SortedDictionary<T, Bucket>();

        public void Inc(T Key, params string[] SubKeys)
        {
            if (!this.Buckets.TryGetValue(Key,out Bucket Bucket))
            {
                Bucket = new Bucket();
                this.Buckets[Key] = Bucket;
            }

			Bucket.Inc(SubKeys);
        }
    }
}

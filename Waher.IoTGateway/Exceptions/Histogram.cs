using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Exceptions
{
    internal class Histogram<T>
    {
        internal SortedDictionary<T, Bucket> Buckets = new SortedDictionary<T, Bucket>();

        internal void Inc(T Key, params string[] SubKeys)
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

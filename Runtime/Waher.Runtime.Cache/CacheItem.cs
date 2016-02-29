using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Cache
{
    internal class CacheItem<KeyType, ValueType>
    {
        private KeyType key;
        private ValueType value;
        private DateTime created;
        private DateTime lastUsed;

        internal CacheItem(KeyType Key, ValueType Value, DateTime Created)
        {
            this.key = Key;
            this.value = Value;
            this.created = Created;
            this.lastUsed = Created;
        }

        internal KeyType Key
        {
            get { return this.key; }
        }

        internal ValueType Value
        {
            get { return this.value; }
        }

        internal DateTime Created
        {
            get { return this.created; }
        }

        internal DateTime LastUsed
        {
            get { return this.lastUsed; }
            set { this.lastUsed = value; }
        }
    }
}

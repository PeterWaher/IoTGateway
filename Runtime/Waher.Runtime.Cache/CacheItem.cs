using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Cache
{
    internal class CacheItem<KeyType, ValueType>
    {
        private readonly KeyType key;
        private readonly ValueType value;
        private readonly DateTime created;
        private DateTime lastUsed;

        internal CacheItem(KeyType Key, ValueType Value, DateTime Created)
        {
            this.key = Key;
            this.value = Value;
            this.created = Created;
            this.lastUsed = Created;
        }

        internal KeyType Key => this.key;

        internal ValueType Value => this.value;

        internal DateTime Created => this.created;

        internal DateTime LastUsed
        {
            get => this.lastUsed;
            set => this.lastUsed = value;
        }
    }
}

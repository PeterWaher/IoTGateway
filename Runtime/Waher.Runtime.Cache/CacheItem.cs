using System;

namespace Waher.Runtime.Cache
{
    internal class CacheItem<KeyType, ValueType>
    {
        private readonly KeyType key;
        private readonly ValueType value;
        private readonly DateTime created;
        private readonly DateTime? expires;
        private DateTime lastUsed;

        internal CacheItem(KeyType Key, ValueType Value, DateTime Created, DateTime? Expires)
        {
            this.key = Key;
            this.value = Value;
            this.created = Created;
            this.expires = Expires;
            this.lastUsed = Created;
        }

        internal KeyType Key => this.key;
        internal ValueType Value => this.value;
        internal DateTime Created => this.created;
        internal DateTime? Expires => this.expires;

        internal DateTime LastUsed
        {
            get => this.lastUsed;
            set => this.lastUsed = value;
        }
    }
}

using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Cache
{
    /// <summary>
    /// Reason for removing the item.
    /// </summary>
    public enum RemovedReason
    {
        /// <summary>
        /// Item was replaced by a newer value.
        /// </summary>
        Replaced,

        /// <summary>
        /// Item has not been used.
        /// </summary>
        NotUsed,

        /// <summary>
        /// Item is too old.
        /// </summary>
        Old,

        /// <summary>
        /// Cache is full and space had to be made for new items.
        /// </summary>
        Space,

        /// <summary>
        /// Item was manually removed by the controlling application.
        /// </summary>
        Manual
    }

    /// <summary>
    /// Event arguments for cache item removal events.
    /// </summary>
    /// <typeparam name="KeyType">Cache key type.</typeparam>
    /// <typeparam name="ValueType">Cache value type.</typeparam>
    public class CacheItemEventArgs<KeyType, ValueType> : EventArgs
    {
        private readonly KeyType key;
        private readonly ValueType value;
        private readonly RemovedReason reason;

        internal CacheItemEventArgs(KeyType Key, ValueType Value, RemovedReason Reason)
        {
            this.key = Key;
            this.value = Value;
            this.reason = Reason;
        }

        /// <summary>
        /// Key of item that was removed.
        /// </summary>
        public KeyType Key => this.key;

        /// <summary>
        /// Value of item that was removed.
        /// </summary>
        public ValueType Value => this.value;

        /// <summary>
        /// Reason for removing the item.
        /// </summary>
        public RemovedReason Reason => this.reason;
    }
}

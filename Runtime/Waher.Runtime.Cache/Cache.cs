using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Waher.Events;

namespace Waher.Runtime.Cache
{
	/// <summary>
	/// Implements an in-memory cache.
	/// </summary>
	/// <typeparam name="KeyType">Cache key type.</typeparam>
	/// <typeparam name="ValueType">Cache value type.</typeparam>
	public class Cache<KeyType, ValueType> : ICache, IDictionary<KeyType, ValueType>
	{
		private readonly Guid id = Guid.NewGuid();
		private readonly Dictionary<KeyType, CacheItem<KeyType, ValueType>> valuesByKey = new Dictionary<KeyType, CacheItem<KeyType, ValueType>>();
		private readonly SortedDictionary<DateTime, KeyType> keysByLastUsage = new SortedDictionary<DateTime, KeyType>();
		private readonly SortedDictionary<DateTime, KeyType> keysByCreation = new SortedDictionary<DateTime, KeyType>();
		private readonly Random rnd = new Random();
		private readonly object synchObject = new object();
		private readonly int maxItems;
		private TimeSpan maxTimeUsed;
		private TimeSpan maxTimeUnused;
		private Timer timer;

		/// <summary>
		/// Implements an in-memory cache.
		/// </summary>
		/// <param name="MaxItems">Maximum number of items in cache.</param>
		/// <param name="MaxTimeUsed">Maximum time to keep items that are being used.</param>
		/// <param name="MaxTimeUnused">Maximum time to keep items that are not being used.</param>
		public Cache(int MaxItems, TimeSpan MaxTimeUsed, TimeSpan MaxTimeUnused)
		{
			this.maxItems = MaxItems;
			this.maxTimeUsed = MaxTimeUsed;
			this.maxTimeUnused = MaxTimeUnused;

			if (this.maxTimeUsed < TimeSpan.MaxValue || this.maxTimeUnused < TimeSpan.MaxValue)
				this.timer = new Timer(this.TimerCallback, null, 5000, 5000);
			else
				this.timer = null;

			Caches.Register(this.id, this);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			Caches.Unregister(this.id);

			this.timer?.Dispose();
			this.timer = null;

			this.Clear();
		}

		private void TimerCallback(object state)
		{
			LinkedList<CacheItem<KeyType, ValueType>> ToRemove1 = null;
			LinkedList<CacheItem<KeyType, ValueType>> ToRemove2 = null;
			DateTime Now = DateTime.Now;
			DateTime Limit;

			try
			{
				lock (this.synchObject)
				{
					if (this.maxTimeUnused < TimeSpan.MaxValue)
					{
						Limit = Now - this.maxTimeUnused;

						foreach (KeyValuePair<DateTime, KeyType> P in this.keysByLastUsage)
						{
							if (P.Key > Limit)
								break;

							if (ToRemove1 is null)
								ToRemove1 = new LinkedList<CacheItem<KeyType, ValueType>>();

							ToRemove1.AddLast(this.valuesByKey[P.Value]);
						}

						if (!(ToRemove1 is null))
						{
							foreach (CacheItem<KeyType, ValueType> Item in ToRemove1)
							{
								this.valuesByKey.Remove(Item.Key);
								this.keysByCreation.Remove(Item.Created);
								this.keysByLastUsage.Remove(Item.LastUsed);
							}
						}
					}

					if (this.maxTimeUsed < TimeSpan.MaxValue)
					{
						Limit = Now - this.maxTimeUsed;

						foreach (KeyValuePair<DateTime, KeyType> P in this.keysByCreation)
						{
							if (P.Key > Limit)
								break;

							if (ToRemove2 is null)
								ToRemove2 = new LinkedList<CacheItem<KeyType, ValueType>>();

							ToRemove2.AddLast(this.valuesByKey[P.Value]);
						}

						if (!(ToRemove2 is null))
						{
							foreach (CacheItem<KeyType, ValueType> Item in ToRemove2)
							{
								this.valuesByKey.Remove(Item.Key);
								this.keysByCreation.Remove(Item.Created);
								this.keysByLastUsage.Remove(Item.LastUsed);
							}
						}
					}
				}

				if (!(ToRemove1 is null))
				{
					foreach (CacheItem<KeyType, ValueType> Item in ToRemove1)
						this.OnRemoved(Item.Key, Item.Value, RemovedReason.NotUsed);
				}

				if (!(ToRemove2 is null))
				{
					foreach (CacheItem<KeyType, ValueType> Item in ToRemove2)
						this.OnRemoved(Item.Key, Item.Value, RemovedReason.Old);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Maximum number of items in cache.
		/// </summary>
		public int MaxItems
		{
			get { return this.maxItems; }
		}

		/// <summary>
		/// Maximum time to keep items that are being used.
		/// </summary>
		public TimeSpan MaxTimeUsed
		{
			get { return this.maxTimeUsed; }
			set { this.maxTimeUsed = value; }
		}

		/// <summary>
		/// Maximum time to keep items that are not being used.
		/// </summary>
		public TimeSpan MaxTimeUnused
		{
			get { return this.maxTimeUnused; }
			set { this.maxTimeUnused = value; }
		}

		/// <summary>
		/// Tries to get a value from the cache.
		/// </summary>
		/// <param name="Key">Key of value.</param>
		/// <param name="Value">Value, if found.</param>
		/// <returns>If the item was found or not.</returns>
		public bool TryGetValue(KeyType Key, out ValueType Value)
		{
			lock (this.synchObject)
			{
				if (this.valuesByKey.TryGetValue(Key, out CacheItem<KeyType, ValueType> Item))
				{
					Value = Item.Value;

					this.keysByLastUsage.Remove(Item.LastUsed);
					Item.LastUsed = this.GetLastUsageTimeLocked();
					this.keysByLastUsage[Item.LastUsed] = Key;

					return true;
				}
				else
				{
					Value = default;
					return false;
				}
			}
		}

		/// <summary>
		/// Number of items in cache
		/// </summary>
		public int Count
		{
			get
			{
				lock (this.synchObject)
				{
					return this.valuesByKey.Count;
				}
			}
		}

		/// <summary>
		/// Keys in cache.
		/// </summary>
		public ICollection<KeyType> Keys => this.GetKeys();

		/// <summary>
		/// Values in cache.
		/// </summary>
		public ICollection<ValueType> Values => this.GetValues();

		/// <summary>
		/// If the dictionary is read-only.
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Gets all available keys in the cache.
		/// </summary>
		/// <returns>Array of keys.</returns>
		public KeyType[] GetKeys()
		{
			KeyType[] Result;

			lock (this.synchObject)
			{
				Result = new KeyType[this.valuesByKey.Count];
				this.valuesByKey.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets all available values in the cache.
		/// </summary>
		/// <returns>Array of values.</returns>
		public ValueType[] GetValues()
		{
			ValueType[] Result;
			int i = 0;

			lock (this.synchObject)
			{
				Result = new ValueType[this.valuesByKey.Count];

				foreach (CacheItem<KeyType, ValueType> Rec in this.valuesByKey.Values)
					Result[i++] = Rec.Value;
			}

			return Result;
		}

		/// <summary>
		/// Checks if a key is available in the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <returns>If the key is available.</returns>
		public bool ContainsKey(KeyType Key)
		{
			return (this.TryGetValue(Key, out ValueType _));
		}

		private DateTime GetLastUsageTimeLocked()
		{
			DateTime TP = DateTime.Now;

			while (this.keysByLastUsage.ContainsKey(TP))
				TP = TP.AddTicks(rnd.Next(1, 10));

			return TP;
		}

		/// <summary>
		/// Access to values in the cache.
		/// </summary>
		/// <param name="Key">Value key</param>
		/// <returns>Value corresponding to key.</returns>
		/// <exception cref="ArgumentException">If key was not found.</exception>
		public ValueType this[KeyType Key]
		{
			get
			{
				if (this.TryGetValue(Key, out ValueType Result))
					return Result;
				else
					throw new ArgumentException("Value not found.", nameof(Key));
			}

			set
			{
				this.Add(Key, value);
			}
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="Key"></param>
		/// <param name="Value"></param>
		public void Add(KeyType Key, ValueType Value)
		{
			CacheItem<KeyType, ValueType> Prev;
			CacheItem<KeyType, ValueType> Item;
			RemovedReason Reason;

			lock (this.synchObject)
			{
				if (this.valuesByKey.TryGetValue(Key, out Prev))
				{
					this.valuesByKey.Remove(Key);
					this.keysByCreation.Remove(Prev.Created);
					this.keysByLastUsage.Remove(Prev.LastUsed);
					Reason = RemovedReason.Replaced;
				}
				else
				{
					Reason = RemovedReason.Space;

					if (this.valuesByKey.Count >= this.maxItems)
					{
						KeyType OldKey = default;
						bool Found = false;

						foreach (KeyType Key2 in this.keysByLastUsage.Values)
						{
							OldKey = Key2;
							Found = true;
							break;
						}

						if (Found)
						{
							Prev = this.valuesByKey[OldKey];

							this.valuesByKey.Remove(OldKey);
							this.keysByCreation.Remove(Prev.Created);
							this.keysByLastUsage.Remove(Prev.LastUsed);
						}
						else
							Prev = null;
					}
					else
						Prev = null;
				}

				DateTime TP = DateTime.Now;

				while (this.keysByCreation.ContainsKey(TP) || this.keysByLastUsage.ContainsKey(TP))
					TP = TP.AddTicks(rnd.Next(1, 10));

				Item = new CacheItem<KeyType, ValueType>(Key, Value, TP);

				this.valuesByKey[Key] = Item;
				this.keysByCreation[TP] = Key;
				this.keysByLastUsage[TP] = Key;
			}

			if (!(Prev is null))
				this.OnRemoved(Key, Prev.Value, Reason);
		}

		private void OnRemoved(KeyType Key, ValueType Value, RemovedReason Reason)
		{
			CacheItemEventHandler<KeyType, ValueType> h = this.Removed;

			if (!(h is null))
			{
				try
				{
					h(this, new CacheItemEventArgs<KeyType, ValueType>(Key, Value, Reason));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Removes an item from the cache.
		/// </summary>
		/// <param name="Key">Key of item to remove.</param>
		/// <returns>If an item with the given key was found and removed.</returns>
		public bool Remove(KeyType Key)
		{
			CacheItem<KeyType, ValueType> Item;

			lock (this.synchObject)
			{
				if (!this.valuesByKey.TryGetValue(Key, out Item))
					return false;

				this.valuesByKey.Remove(Item.Key);
				this.keysByCreation.Remove(Item.Created);
				this.keysByLastUsage.Remove(Item.LastUsed);
			}

			this.OnRemoved(Key, Item.Value, RemovedReason.Manual);

			return true;
		}

		/// <summary>
		/// Event raised when an item has been removed from the cache.
		/// </summary>
		public event CacheItemEventHandler<KeyType, ValueType> Removed = null;

		/// <summary>
		/// Clears the cache.
		/// </summary>
		public void Clear()
		{
			CacheItem<KeyType, ValueType>[] Values;

			lock (this.synchObject)
			{
				Values = new CacheItem<KeyType, ValueType>[this.valuesByKey.Count];
				this.valuesByKey.Values.CopyTo(Values, 0);
				this.valuesByKey.Clear();
				this.keysByLastUsage.Clear();
				this.keysByCreation.Clear();
			}

			foreach (CacheItem<KeyType, ValueType> Item in Values)
				this.OnRemoved(Item.Key, Item.Value, RemovedReason.Manual);
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="item">Key and value pair.</param>
		public void Add(KeyValuePair<KeyType, ValueType> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Checks if an item (key and value) exists in the cache.
		/// </summary>
		/// <param name="item">Key and value pair.</param>
		/// <returns>If the cache contains the item.</returns>
		public bool Contains(KeyValuePair<KeyType, ValueType> item)
		{
			return this.TryGetValue(item.Key, out ValueType Value) && Value.Equals(item.Value);
		}

		/// <summary>
		/// Copies all items in the cache to an array.
		/// </summary>
		/// <param name="array">Destination array.</param>
		/// <param name="arrayIndex">Index to start copying to.</param>
		public void CopyTo(KeyValuePair<KeyType, ValueType>[] array, int arrayIndex)
		{
			lock (this.synchObject)
			{
				foreach (CacheItem<KeyType, ValueType> Item in this.valuesByKey.Values)
					array[arrayIndex++] = new KeyValuePair<KeyType, ValueType>(Item.Key, Item.Value);
			}
		}

		/// <summary>
		/// Returns the contents of the cache as an array.
		/// </summary>
		/// <returns>Contents in an array.</returns>
		public KeyValuePair<KeyType, ValueType>[] ToArray()
		{
			KeyValuePair<KeyType, ValueType>[] Result;
			int i = 0;

			lock (this.synchObject)
			{
				Result = new KeyValuePair<KeyType, ValueType>[this.valuesByKey.Count];

				foreach (CacheItem<KeyType, ValueType> Item in this.valuesByKey.Values)
					Result[i++] = new KeyValuePair<KeyType, ValueType>(Item.Key, Item.Value);
			}

			return Result;
		}

		/// <summary>
		/// Removes an item from the cache.
		/// </summary>
		/// <param name="item">Key and value pair.</param>
		/// <returns>If the item was found and removed.</returns>
		public bool Remove(KeyValuePair<KeyType, ValueType> item)
		{
			if (this.TryGetValue(item.Key, out ValueType Value) && Value.Equals(item.Value))
				return this.Remove(item.Key);
			else
				return false;
		}

		/// <summary>
		/// Gets an enumerator of contents in the cache.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<KeyValuePair<KeyType, ValueType>> GetEnumerator()
		{
			IEnumerable<KeyValuePair<KeyType, ValueType>> Array = this.ToArray();
			return Array.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of contents in the cache.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.ToArray().GetEnumerator();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Collections;

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
		private readonly SortedDictionary<DateTime, KeyType> keysByExpiry = new SortedDictionary<DateTime, KeyType>();
		private readonly Random rnd = new Random();
		private readonly object synchObject = new object();
		private readonly int maxItems;
		private readonly bool standalone;
		private TimeSpan maxTimeUsed;
		private TimeSpan maxTimeUnused;
		private Timer timer;
		private int maxTimerIntervalMs = 5000;
		private int minTimerIntervalMs = 100;
		private bool hasExplicitExpiry = false;

		/// <summary>
		/// Implements an in-memory cache.
		/// </summary>
		/// <param name="MaxItems">Maximum number of items in cache.</param>
		/// <param name="MaxTimeUsed">Maximum time to keep items that are being used.</param>
		/// <param name="MaxTimeUnused">Maximum time to keep items that are not being used.</param>
		public Cache(int MaxItems, TimeSpan MaxTimeUsed, TimeSpan MaxTimeUnused)
			: this(MaxItems, MaxTimeUsed, MaxTimeUnused, false)
		{
		}

		/// <summary>
		/// Implements an in-memory cache.
		/// </summary>
		/// <param name="MaxItems">Maximum number of items in cache.</param>
		/// <param name="MaxTimeUsed">Maximum time to keep items that are being used.</param>
		/// <param name="MaxTimeUnused">Maximum time to keep items that are not being used.</param>
		/// <param name="Standalone">If cache is a standalone cache, or if it can be managed collectively
		/// with other caches.</param>
		public Cache(int MaxItems, TimeSpan MaxTimeUsed, TimeSpan MaxTimeUnused, bool Standalone)
		{
			this.maxItems = MaxItems;
			this.maxTimeUsed = MaxTimeUsed;
			this.maxTimeUnused = MaxTimeUnused;
			this.standalone = Standalone;

			Caches.Register(this.id, this);
		}

		private void CreateTimerLocked()
		{
			if (this.maxTimeUsed < TimeSpan.MaxValue ||
				this.maxTimeUnused < TimeSpan.MaxValue)
			{
				int Interval = Math.Min((int)((this.maxTimeUnused.TotalMilliseconds / 2) + 0.5), this.maxTimerIntervalMs);
				if (Interval < this.minTimerIntervalMs)
					Interval = this.minTimerIntervalMs;

				this.timer = new Timer(this.TimerCallback, null, Interval, Interval);
			}
			else
				this.timer = null;
		}

		/// <summary>
		/// Minimum expiry timer interval, in milliseconds.
		/// </summary>
		public int MinTimerIntervalMs
		{
			get => this.minTimerIntervalMs;
			set => this.minTimerIntervalMs = value;
		}

		/// <summary>
		/// Maximum expiry timer interval, in milliseconds.
		/// </summary>
		public int MaxTimerIntervalMs
		{
			get => this.maxTimerIntervalMs;
			set => this.maxTimerIntervalMs = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			Caches.Unregister(this.id);
			this.Clear();
		}

		/// <summary>
		/// If cache is a standalone cache, or if it can be managed collectively
		/// with other caches.
		/// </summary>
		public bool Standalone => this.standalone;

		private void TimerCallback(object state)
		{
			ChunkedList<CacheItem<KeyType, ValueType>> ToRemoveNotUsed = null;
			ChunkedList<CacheItem<KeyType, ValueType>> ToRemoveOld = null;
			ChunkedList<CacheItem<KeyType, ValueType>> ToRemoveExpired = null;
			DateTime UtcNow = DateTime.UtcNow;
			DateTime Limit;
			bool Removed = false;

			try
			{
				lock (this.synchObject)
				{
					if (this.maxTimeUnused < TimeSpan.MaxValue)
					{
						Limit = UtcNow - this.maxTimeUnused;

						foreach (KeyValuePair<DateTime, KeyType> P in this.keysByLastUsage)
						{
							if (P.Key > Limit)
								break;

							if (ToRemoveNotUsed is null)
								ToRemoveNotUsed = new ChunkedList<CacheItem<KeyType, ValueType>>();

							ToRemoveNotUsed.Add(this.valuesByKey[P.Value]);
						}

						if (!(ToRemoveNotUsed is null))
						{
							ChunkNode<CacheItem<KeyType, ValueType>> Loop = ToRemoveNotUsed.FirstChunk;
							CacheItem<KeyType, ValueType> Item;

							while (!(Loop is null))
							{
								for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
								{
									Item = Loop[i];

									this.valuesByKey.Remove(Item.Key);
									this.keysByCreation.Remove(Item.Created);
									this.keysByLastUsage.Remove(Item.LastUsed);

									if (Item.Expires.HasValue &&
										this.keysByExpiry.Remove(Item.Expires.Value))
									{
										this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
									}
								}

								Loop = Loop.Next;
							}

							Removed = true;
						}
					}

					if (this.maxTimeUsed < TimeSpan.MaxValue)
					{
						Limit = UtcNow - this.maxTimeUsed;

						foreach (KeyValuePair<DateTime, KeyType> P in this.keysByCreation)
						{
							if (P.Key > Limit)
								break;

							if (ToRemoveOld is null)
								ToRemoveOld = new ChunkedList<CacheItem<KeyType, ValueType>>();

							ToRemoveOld.Add(this.valuesByKey[P.Value]);
						}

						if (!(ToRemoveOld is null))
						{
							ChunkNode<CacheItem<KeyType, ValueType>> Loop = ToRemoveOld.FirstChunk;
							CacheItem<KeyType, ValueType> Item;

							while (!(Loop is null))
							{
								for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
								{
									Item = Loop[i];

									this.valuesByKey.Remove(Item.Key);
									this.keysByCreation.Remove(Item.Created);
									this.keysByLastUsage.Remove(Item.LastUsed);

									if (Item.Expires.HasValue &&
										this.keysByExpiry.Remove(Item.Expires.Value))
									{
										this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
									}
								}

								Loop = Loop.Next;
							}

							Removed = true;
						}
					}

					if (this.hasExplicitExpiry)
					{
						foreach (KeyValuePair<DateTime, KeyType> P in this.keysByExpiry)
						{
							if (P.Key > UtcNow)
								break;

							if (ToRemoveExpired is null)
								ToRemoveExpired = new ChunkedList<CacheItem<KeyType, ValueType>>();

							ToRemoveExpired.Add(this.valuesByKey[P.Value]);
						}

						if (!(ToRemoveExpired is null))
						{
							ChunkNode<CacheItem<KeyType, ValueType>> Loop = ToRemoveExpired.FirstChunk;
							CacheItem<KeyType, ValueType> Item;

							while (!(Loop is null))
							{
								for (int i = Loop.Start, c = Loop.Pos; i < c; i++)
								{
									Item = Loop[i];

									this.valuesByKey.Remove(Item.Key);
									this.keysByCreation.Remove(Item.Created);
									this.keysByLastUsage.Remove(Item.LastUsed);
									this.keysByExpiry.Remove(Item.Expires.Value);
								}

								Loop = Loop.Next;
							}

							this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
							Removed = true;
						}
					}

					if (Removed && this.valuesByKey.Count == 0)
					{
						this.timer?.Dispose();
						this.timer = null;
					}
				}

				if (!(ToRemoveNotUsed is null))
					this.OnRemoved(ToRemoveNotUsed, RemovedReason.NotUsed);

				if (!(ToRemoveOld is null))
					this.OnRemoved(ToRemoveOld, RemovedReason.Old);

				if (!(ToRemoveExpired is null))
					this.OnRemoved(ToRemoveExpired, RemovedReason.Old);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Maximum number of items in cache.
		/// </summary>
		public int MaxItems => this.maxItems;

		/// <summary>
		/// Maximum time to keep items that are being used.
		/// </summary>
		public TimeSpan MaxTimeUsed
		{
			get => this.maxTimeUsed;
			set => this.maxTimeUsed = value;
		}

		/// <summary>
		/// Maximum time to keep items that are not being used.
		/// </summary>
		public TimeSpan MaxTimeUnused
		{
			get => this.maxTimeUnused;
			set => this.maxTimeUnused = value;
		}

		/// <summary>
		/// Pings an entry in the cache, to keep it from being removed.
		/// </summary>
		/// <param name="Key">Key of value.</param>
		/// <returns>If the item was found in the cache.</returns>
		public bool Ping(KeyType Key)
		{
			return this.TryGetValue(Key, out _);
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
			return this.TryGetValue(Key, out ValueType _);
		}

		private DateTime GetLastUsageTimeLocked()
		{
			DateTime TP = DateTime.UtcNow;

			while (this.keysByLastUsage.ContainsKey(TP))
				TP = TP.AddTicks(this.rnd.Next(1, 10));

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
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		public void Add(KeyType Key, ValueType Value)
		{
			this.Add(Key, Value, null, null);
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Expires">Explicit expiry time, if any.</param>
		public void Add(KeyType Key, ValueType Value, DateTime? Expires)
		{
			this.Add(Key, Value, Expires, null);
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Expires">Relative expiry time, if any.</param>
		public void Add(KeyType Key, ValueType Value, TimeSpan? Expires)
		{
			this.Add(Key, Value, null, Expires);
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Expires">Explicit expiry time, if any.</param>
		/// <param name="Expires2">Relative expiry time, if any.</param>
		private void Add(KeyType Key, ValueType Value, DateTime? Expires, TimeSpan? Expires2)
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

					if (Prev.Expires.HasValue &&
						this.keysByExpiry.Remove(Prev.Expires.Value))
					{
						this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
					}

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

							if (Prev.Expires.HasValue &&
								this.keysByExpiry.Remove(Prev.Expires.Value))
							{
								this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
							}
						}
						else
							Prev = null;
					}
					else
						Prev = null;
				}

				DateTime TP = DateTime.UtcNow;

				while (this.keysByCreation.ContainsKey(TP) ||
					this.keysByLastUsage.ContainsKey(TP))
				{
					TP = TP.AddTicks(this.rnd.Next(1, 10));
				}

				if (Expires2.HasValue)
					Expires = TP.Add(Expires2.Value);

				if (Expires.HasValue)
				{
					if (Expires.Value.Kind != DateTimeKind.Utc)
						Expires = Expires.Value.ToUniversalTime();

					while (this.keysByExpiry.ContainsKey(Expires.Value))
						Expires = Expires.Value.AddTicks(this.rnd.Next(1, 10));
				}

				Item = new CacheItem<KeyType, ValueType>(Key, Value, TP, Expires);

				this.valuesByKey[Key] = Item;
				this.keysByCreation[TP] = Key;
				this.keysByLastUsage[TP] = Key;

				if (Expires.HasValue)
				{
					this.keysByExpiry[Expires.Value] = Key;
					this.hasExplicitExpiry = true;
				}

				if (this.timer is null)
					this.CreateTimerLocked();
			}

			if (!(Prev is null))
				_ = this.OnRemoved(Key, Prev.Value, Reason);
		}

		private async Task OnRemoved(KeyType Key, ValueType Value, RemovedReason Reason)
		{
			try
			{
				EventHandlerAsync<CacheItemEventArgs<KeyType, ValueType>> h = this.Removed;

				if (!(h is null))
					await h.Raise(this, new CacheItemEventArgs<KeyType, ValueType>(Key, Value, Reason));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async void OnRemoved(IEnumerable<CacheItem<KeyType, ValueType>> Items, RemovedReason Reason)
		{
			EventHandlerAsync<CacheItemEventArgs<KeyType, ValueType>> h = this.Removed;
			try
			{
				if (!(h is null))
				{
					foreach (CacheItem<KeyType, ValueType> Item in Items)
						await h.Raise(this, new CacheItemEventArgs<KeyType, ValueType>(Item.Key, Item.Value, Reason));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Removes an item from the cache.
		/// </summary>
		/// <param name="Key">Key of item to remove.</param>
		/// <returns>If an item with the given key was found and removed.</returns>
		public bool Remove(KeyType Key)
		{
			if (this.RemoveNoEvent(Key, out CacheItem<KeyType, ValueType> Item))
			{
				_ = this.OnRemoved(Key, Item.Value, RemovedReason.Manual);
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Removes an item from the cache. Waits for the removal event to complete
		/// before returning.
		/// </summary>
		/// <param name="Key">Key of item to remove.</param>
		/// <returns>If an item with the given key was found and removed.</returns>
		public async Task<bool> RemoveAsync(KeyType Key)
		{
			if (this.RemoveNoEvent(Key, out CacheItem<KeyType, ValueType> Item))
			{
				await this.OnRemoved(Key, Item.Value, RemovedReason.Manual);
				return true;
			}
			else
				return false;
		}

		private bool RemoveNoEvent(KeyType Key, out CacheItem<KeyType, ValueType> Item)
		{
			lock (this.synchObject)
			{
				if (!this.valuesByKey.TryGetValue(Key, out Item))
					return false;

				this.valuesByKey.Remove(Item.Key);
				this.keysByCreation.Remove(Item.Created);
				this.keysByLastUsage.Remove(Item.LastUsed);

				if (Item.Expires.HasValue &&
					this.keysByExpiry.Remove(Item.Expires.Value))
				{
					this.hasExplicitExpiry = this.keysByExpiry.Count > 0;
				}

				if (this.valuesByKey.Count == 0)
				{
					this.timer?.Dispose();
					this.timer = null;
				}
			}

			return true;
		}

		/// <summary>
		/// Event raised when an item has been removed from the cache.
		/// </summary>
		public event EventHandlerAsync<CacheItemEventArgs<KeyType, ValueType>> Removed = null;

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
				this.keysByExpiry.Clear();
				this.hasExplicitExpiry = false;

				this.timer?.Dispose();
				this.timer = null;
			}

			this.OnRemoved(Values, RemovedReason.Manual);
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
		/// <param name="Item">Key and value pair.</param>
		/// <returns>If the item was found and removed.</returns>
		public bool Remove(KeyValuePair<KeyType, ValueType> Item)
		{
			if (this.TryGetValue(Item.Key, out ValueType Value) && Value.Equals(Item.Value))
				return this.Remove(Item.Key);
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

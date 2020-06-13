using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Waher.Events;
using Waher.Persistence.Filters;
using Waher.Persistence.MongoDB.Serialization;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// This class manages a string dictionary in a persisted storage.
	/// </summary>
	public class StringDictionary : IPersistentDictionary
	{
		private readonly MongoDBProvider provider;
		private readonly string collectionName;

		/// <summary>
		/// This class manages a string dictionary in a persisted storage.
		/// </summary>
		/// <param name="CollectionName">Collection Name.</param>
		/// <param name="Provider">Files provider.</param>
		public StringDictionary(string CollectionName, MongoDBProvider Provider)
		{
			this.provider = Provider;
			this.collectionName = CollectionName;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public bool ContainsKey(string key)
		{
			return this.ContainsKeyAsync(key).Result;
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public async Task<bool> ContainsKeyAsync(string key)
		{
			DictionaryEntry Entry = await Database.FindFirstIgnoreRest<DictionaryEntry>(new FilterAnd(
				new FilterFieldEqualTo("Collection", this.collectionName),
				new FilterFieldEqualTo("Key", key)));

			return !(Entry is null);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public void Add(string key, object value)
		{
			this.AddAsync(key, value, false).Wait();
		}

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public Task AddAsync(string key, object value)
		{
			return this.AddAsync(key, value, false);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <param name="ReplaceIfExists">If replacement of any existing value is desired.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public async Task AddAsync(string key, object value, bool ReplaceIfExists)
		{
			if (key is null)
				throw new ArgumentNullException("key is null.", nameof(key));

			DictionaryEntry Entry = await Database.FindFirstIgnoreRest<DictionaryEntry>(new FilterAnd(
				new FilterFieldEqualTo("Collection", this.collectionName),
				new FilterFieldEqualTo("Key", key)));

			if (Entry is null)
			{
				Entry = new DictionaryEntry()
				{
					Collection = this.collectionName,
					Key = key,
					Value = value
				};

				await this.provider.Insert(Entry);
			}
			else if (ReplaceIfExists)
			{
				Entry.Value = value;

				await this.provider.Update(Entry);
			}
			else
				throw new ArgumentException("Key already exists.", nameof(key));
		}

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public bool Remove(string key)
		{
			return this.RemoveAsync(key).Result;
		}

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<bool> RemoveAsync(string key)
		{
			DictionaryEntry Entry = await Database.FindFirstDeleteRest<DictionaryEntry>(new FilterAnd(
				new FilterFieldEqualTo("Collection", this.collectionName),
				new FilterFieldEqualTo("Key", key)));

			if (Entry is null)
				return false;

			await this.provider.Delete(Entry);

			return true;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element with the specified key; 
		/// otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public bool TryGetValue(string key, out object value)
		{
			KeyValuePair<bool, object> Result = this.TryGetValueAsync(key).Result;
			value = Result.Value;
			return Result.Key;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <returns>Returns a pair of values:
		/// 
		/// First value is true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element 
		/// with the specified key; otherwise, false.
		/// When this method returns, the second value contains the value associated with the key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<KeyValuePair<bool, object>> TryGetValueAsync(string key)
		{
			if (key is null)
				throw new ArgumentNullException("key is null.", nameof(key));

			DictionaryEntry Entry = await Database.FindFirstIgnoreRest<DictionaryEntry>(new FilterAnd(
				new FilterFieldEqualTo("Collection", this.collectionName),
				new FilterFieldEqualTo("Key", key)));

			return new KeyValuePair<bool, object>(!(Entry is null), Entry?.Value);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <returns>Returns the value associated with the specified key, if the key is found.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		/// <exception cref="KeyNotFoundException">If <paramref name="key"/> was not found.</exception>
		public async Task<KeyValuePair<string, object>> GetValueAsync(string key)
		{
			if (key is null)
				throw new ArgumentNullException("key is null.", nameof(key));

			if (key is null)
				throw new ArgumentNullException("key is null.", nameof(key));

			DictionaryEntry Entry = await Database.FindFirstIgnoreRest<DictionaryEntry>(new FilterAnd(
				new FilterFieldEqualTo("Collection", this.collectionName),
				new FilterFieldEqualTo("Key", key)));

			return new KeyValuePair<string, object>(key, Entry?.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.Add(T)"/>
		/// </summary>
		public void Add(KeyValuePair<string, object> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.Clear()"/>
		/// </summary>
		public void Clear()
		{
			this.ClearAsync().Wait();
		}

		/// <summary>
		/// Clears the dictionary.
		/// </summary>
		public async Task ClearAsync()
		{
			while (true)
			{
				IEnumerable<DictionaryEntry> Entries = await this.provider.Find<DictionaryEntry>(0, 1000, new FilterFieldEqualTo("Collection", this.collectionName));
				bool Empty = true;

				foreach (DictionaryEntry Entry in Entries)
				{
					Empty = false;
					break;
				}

				if (Empty)
					break;

				await this.provider.Delete(Entries);
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.Contains(T)"/>
		/// </summary>
		public bool Contains(KeyValuePair<string, object> item)
		{
			if (!this.TryGetValue(item.Key, out object Value))
				return false;

			return Value.Equals(item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.CopyTo(T[], int)"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			IEnumerable<DictionaryEntry> Entries = this.provider.Find<DictionaryEntry>(0, int.MaxValue, new FilterFieldEqualTo("Collection", this.collectionName)).Result;

			foreach (DictionaryEntry Entry in Entries)
				array[arrayIndex++] = new KeyValuePair<string, object>(Entry.Key, Entry.Value);
		}

		/// <summary>
		/// Loads the entire table and returns it as an array.
		/// </summary>
		/// <returns>Array of key-value pairs.</returns>
		public KeyValuePair<string, object>[] ToArray()
		{
			return this.ToArrayAsync().Result;
		}

		/// <summary>
		/// Loads the entire table and returns it as an array.
		/// </summary>
		/// <returns>Array of key-value pairs.</returns>
		public async Task<KeyValuePair<string, object>[]> ToArrayAsync()
		{
			List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>();

			IEnumerable<DictionaryEntry> Entries = await this.provider.Find<DictionaryEntry>(0, int.MaxValue, new FilterFieldEqualTo("Collection", this.collectionName));

			foreach (DictionaryEntry Entry in Entries)
				Result.Add(new KeyValuePair<string, object>(Entry.Key, Entry.Value));

			return Result.ToArray();
		}

		/// <summary>
		/// <see cref="ICollection{T}.Remove(T)"/>
		/// </summary>
		public bool Remove(KeyValuePair<string, object> item)
		{
			if (!this.Contains(item))
				return false;
			else
				return this.Remove(item.Key);
		}

		/// <summary>
		/// <see cref="IEnumerable{T}.GetEnumerator"/>
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return this.GetEnumeratorAsync().Result;
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumeratorAsync().Result;
		}

		/// <summary>
		/// Gets an enumerator for all entries in the dictionary.
		/// </summary>
		/// <returns>Enumerator</returns>
		public async Task<IEnumerator<KeyValuePair<string, object>>> GetEnumeratorAsync()
		{
			IEnumerable<DictionaryEntry> Entries = await this.provider.Find<DictionaryEntry>(0, int.MaxValue, new FilterFieldEqualTo("Collection", this.collectionName));
			return new DictionaryEnumerator(Entries);
		}

		/// <summary>
		/// Name of corresponding collection name.
		/// </summary>
		public string CollectionName { get { return this.collectionName; } }

		/// <summary>
		/// <see cref="IDictionary{TKey, TValue}.Keys"/>
		/// </summary>
		public ICollection<string> Keys
		{
			get
			{
				if (this.keyCollection is null)
					this.keyCollection = new KeyCollection(this);

				return this.keyCollection;
			}
		}

		private KeyCollection keyCollection = null;

		/// <summary>
		/// <see cref="IDictionary{TKey, TValue}.Values"/>
		/// </summary>
		public ICollection<object> Values
		{
			get
			{
				if (this.valueCollection is null)
					this.valueCollection = new ValueCollection(this);

				return this.valueCollection;
			}
		}

		private ValueCollection valueCollection = null;

		/// <summary>
		/// <see cref="ICollection{T}.Count"/>
		/// </summary>
		public int Count
		{
			get
			{
				ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(typeof(DictionaryEntry));
				IMongoCollection<BsonDocument> Collection = this.provider.GetCollection(DictionaryEntry.CollectionName);
				FilterDefinition<BsonDocument> BsonFilter = this.provider.Convert(new FilterFieldEqualTo("Collection", this.collectionName), Serializer);

				long Result = Collection.CountDocuments(BsonFilter);

				if (Result > int.MaxValue)
					return int.MaxValue;
				else if (Result < int.MinValue)
					return int.MinValue;
				else
					return (int)Result;
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.IsReadOnly"/>
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key of the element to get or set.</param>
		/// <returns>The element with the specified key.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		/// <exception cref="KeyNotFoundException">The property is retrieved and key is not found.</exception>
		public object this[string key]
		{
			get
			{
				return this.GetValueAsync(key).Result;
			}

			set
			{
				this.AddAsync(key, value, true).Wait();
			}
		}

		/// <summary>
		/// Deletes the dictionary and disposes the object.
		/// </summary>
		public void DeleteAndDispose()
		{
			this.Clear();
			this.Dispose();
		}
	}
}

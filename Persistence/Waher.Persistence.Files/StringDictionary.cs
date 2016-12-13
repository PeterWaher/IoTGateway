using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a string dictionary in a persisted file.
	/// </summary>
	public class StringDictionary : IDisposable, IDictionary<string, object>
	{
		private Dictionary<string, object> inMemory;
		private ObjectBTreeFile dictionaryFile;
		private StringDictionaryRecords recordHandler;
		private KeyValueSerializer keyValueSerializer;
		private GenericObjectSerializer genericSerializer;
		private FilesProvider provider;
		private Encoding encoding;
		private string collectionName;
		private int timeoutMilliseconds;

		/// <summary>
		/// This class manages a string dictionary in a persisted file.
		/// </summary>
		/// <param name="Id">Internal identifier of the file.</param>
		/// <param name="FileName">File name of index file.</param>
		/// <param name="BlobFileName">Name of file in which BLOBs are stored.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="RetainInMemory">Retain the dictionary in memory.</param>
		public StringDictionary(int Id, string FileName, string BlobFileName, FilesProvider Provider, bool RetainInMemory)
		{
			this.provider = Provider;
			this.collectionName = this.provider.DefaultCollectionName;
			this.encoding = this.provider.Encoding;
			this.timeoutMilliseconds = this.provider.TimeoutMilliseconds;
			this.genericSerializer = new GenericObjectSerializer(this.provider);
			this.keyValueSerializer = new KeyValueSerializer(this.provider, this.genericSerializer);

			this.recordHandler = new StringDictionaryRecords(this.collectionName, this.encoding,
				(this.provider.BlockSize - ObjectBTreeFile.BlockHeaderSize) / 2 - 4, this.genericSerializer, this.provider);

			this.dictionaryFile = new ObjectBTreeFile(Id, FileName, this.collectionName, BlobFileName,
				this.provider.BlockSize, this.provider.BlobBlockSize, this.provider, this.encoding, this.timeoutMilliseconds,
				this.provider.Encrypted, this.recordHandler);

			if (RetainInMemory)
				this.inMemory = new Dictionary<string, object>();
			else
				this.inMemory = null;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.dictionaryFile != null)
			{
				this.dictionaryFile.Dispose();
				this.dictionaryFile = null;

				this.recordHandler = null;
			}
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public bool ContainsKey(string key)
		{
			Task<bool> Task = this.ContainsKeyAsync(key);
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public async Task<bool> ContainsKeyAsync(string key)
		{
			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					if (this.inMemory.ContainsKey(key))
						return true;
				}
			}

			await this.dictionaryFile.Lock();
			try
			{
				BlockInfo Info = await this.dictionaryFile.FindNodeLocked(key);
				return Info != null;
			}
			finally
			{
				await this.dictionaryFile.Release();
			}
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
			if (key == null)
				throw new ArgumentNullException("key is null.", "key");

			Type Type = value.GetType();
			IObjectSerializer Serializer = this.provider.GetObjectSerializer(Type);
			byte[] Bin = this.recordHandler.Serialize(key, value, Serializer);

			if (Bin.Length > this.dictionaryFile.InlineObjectSizeLimit)
				throw new ArgumentException("BLOBs not supported.", "value");

			await this.dictionaryFile.Lock();
			try
			{
				BlockInfo Info = await this.dictionaryFile.FindLeafNodeLocked(key);
				if (Info != null)
					await this.dictionaryFile.InsertObjectLocked(Info.BlockIndex, Info.Header, Info.Block, Bin, Info.InternalPosition, 0, 0, true, Info.LastObject);
				else
				{
					if (!ReplaceIfExists)
						throw new ArgumentException("A key with that value already exists.", "key");

					Info = await this.dictionaryFile.FindNodeLocked(key);

					await this.dictionaryFile.ReplaceObjectLocked(Bin, Info, true);
				}
			}
			finally
			{
				await this.dictionaryFile.Release();
			}

			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					this.inMemory[key] = value;
				}
			}
		}

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public bool Remove(string key)
		{
			Task<bool> Task = this.RemoveAsync(key);
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<bool> RemoveAsync(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key is null.", "key");

			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					this.inMemory.Remove(key);
				}
			}

			object DeletedObject;

			await this.dictionaryFile.Lock();
			try
			{
				DeletedObject = await this.dictionaryFile.DeleteObjectLocked(key, false, true, this.keyValueSerializer, null);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
			finally
			{
				await this.dictionaryFile.Release();
			}

			return DeletedObject != null;
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
			Task<KeyValuePair<bool, KeyValuePair<string, object>>> Task = this.TryGetValueAsync(key);
			Task.Wait();
			value = Task.Result.Value;
			return Task.Result.Key;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <returns>Returns a pair of values:
		/// 
		/// First value is true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element 
		/// with the specified key; otherwise, false.
		/// When this method returns, the second value associated with the specified key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter. This parameter is passed uninitialized.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<KeyValuePair<bool, KeyValuePair<string, object>>> TryGetValueAsync(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key is null.", "key");

			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					object value;

					if (this.inMemory.TryGetValue(key, out value))
						return new KeyValuePair<bool, KeyValuePair<string, object>>(true, new KeyValuePair<string, object>(key, value));
				}
			}

			await this.dictionaryFile.Lock();
			try
			{
				object Result = this.dictionaryFile.LoadObjectLocked(key, this.keyValueSerializer);
				return new KeyValuePair<bool, KeyValuePair<string, object>>(true, (KeyValuePair<string, object>)Result);
			}
			catch (KeyNotFoundException)
			{
				return new KeyValuePair<bool, KeyValuePair<string, object>>(false, new KeyValuePair<string, object>(key, null));
			}
			finally
			{
				await this.dictionaryFile.Release();
			}
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
			if (key == null)
				throw new ArgumentNullException("key is null.", "key");

			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					object value;

					if (this.inMemory.TryGetValue(key, out value))
						return new KeyValuePair<string, object>(key, value);
				}
			}

			await this.dictionaryFile.Lock();
			try
			{
				return (KeyValuePair<string, object>)await this.dictionaryFile.LoadObjectLocked(key, this.keyValueSerializer);
			}
			finally
			{
				await this.dictionaryFile.Release();
			}
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String,Object}}.Add(KeyValuePair{string, object})"/>
		/// </summary>
		public void Add(KeyValuePair<string, object> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String,Object}}.Clear()"/>
		/// </summary>
		public void Clear()
		{
			this.dictionaryFile.Clear();

			if (this.inMemory != null)
			{
				lock (this.inMemory)
				{
					this.inMemory.Clear();
				}
			}
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String,Object}}.Contains(KeyValuePair{string, object})"/>
		/// </summary>
		public bool Contains(KeyValuePair<string, object> item)
		{
			object Value;

			if (!this.TryGetValue(item.Key, out Value))
				return false;

			return Value.Equals(item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String,Object}}.CopyTo(KeyValuePair{string, object}[], int)"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(true);
			Task.Wait();

			using (ObjectBTreeFileEnumerator<KeyValuePair<string, object>> e = Task.Result)
			{
				while (e.MoveNext())
					array[arrayIndex++] = e.Current;
			}
		}

		/// <summary>
		/// Loads the entire table and returns it as an array.
		/// </summary>
		/// <returns>Array of key-value pairs.</returns>
		public KeyValuePair<string, object>[] ToArray()
		{
			List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>();
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(true);
			Task.Wait();

			using (ObjectBTreeFileEnumerator<KeyValuePair<string, object>> e = Task.Result)
			{
				while (e.MoveNext())
					Result.Add(e.Current);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String,Object}}.Remove(KeyValuePair{string, object})"/>
		/// </summary>
		public bool Remove(KeyValuePair<string, object> item)
		{
			if (!this.Contains(item))
				return false;
			else
				return this.Remove(item.Key);
		}

		/// <summary>
		/// <see cref="IEnumerable{KeyValuePair{String,Object}}.GetEnumerator"/>
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(false);
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(false);
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Gets an enumerator for all entries in the dictionary.
		/// </summary>
		/// <param name="Locked">If the file should be locked.</param>
		/// <returns>Enumerator</returns>
		public async Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> GetEnumerator(bool Locked)
		{
			await this.dictionaryFile.Lock();
			try
			{
				return new ObjectBTreeFileEnumerator<KeyValuePair<string, object>>(this.dictionaryFile, Locked, this.recordHandler, null,
					this.keyValueSerializer);
			}
			finally
			{
				if (!Locked)
					await this.dictionaryFile.Release();
			}
		}

		/// <summary>
		/// Index file.
		/// </summary>
		public ObjectBTreeFile DictionaryFile
		{
			get { return this.dictionaryFile; }
		}

		/// <summary>
		/// Name of corresponding collection name.
		/// </summary>
		public string CollectionName { get { return this.collectionName; } }

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding { get { return this.encoding; } }

		/// <summary>
		/// <see cref="IDictionary{string,object}.Keys"/>
		/// </summary>
		public ICollection<string> Keys
		{
			get
			{
				if (this.keyCollection == null)
					this.keyCollection = new KeyCollection(this);

				return this.keyCollection;
			}
		}

		private KeyCollection keyCollection = null;

		/// <summary>
		/// <see cref="IDictionary{string,object}.Values"/>
		/// </summary>
		public ICollection<object> Values
		{
			get
			{
				if (this.valueCollection == null)
					this.valueCollection = new ValueCollection(this);

				return this.valueCollection;
			}
		}

		private ValueCollection valueCollection = null;

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{string,object}}.Count"/>
		/// </summary>
		public int Count
		{
			get
			{
				return this.dictionaryFile.Count;
			}
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{string,object}}.IsReadOnly"/>
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
				Task<KeyValuePair<string, object>> Task = this.GetValueAsync(key);
				Task.Wait();
				return Task.Result.Value;
			}

			set
			{
				this.AddAsync(key, value, true).Wait();
			}
		}
	}
}

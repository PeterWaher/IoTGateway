#define ASSERT_LOCKS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a string dictionary in a persisted file.
	/// </summary>
	public class StringDictionary : IPersistentDictionary
	{
		private readonly Dictionary<string, object> inMemory;
		private ObjectBTreeFile dictionaryFile;
		private StringDictionaryRecords recordHandler;
		private readonly KeyValueSerializer keyValueSerializer;
		private readonly GenericObjectSerializer genericSerializer;
		private readonly FilesProvider provider;
		private readonly Encoding encoding;
		private readonly string collectionName;
		private readonly int timeoutMilliseconds;

		/// <summary>
		/// This class manages a string dictionary in a persisted file.
		/// </summary>
		/// <param name="CollectionName">Collection Name.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="RetainInMemory">Retain the dictionary in memory.</param>
		private StringDictionary(string CollectionName, FilesProvider Provider, bool RetainInMemory)
		{
			this.provider = Provider;
			this.collectionName = CollectionName;
			this.encoding = this.provider.Encoding;
			this.timeoutMilliseconds = this.provider.TimeoutMilliseconds;
			this.genericSerializer = new GenericObjectSerializer(this.provider);
			this.keyValueSerializer = new KeyValueSerializer(this.provider, this.genericSerializer);

			this.recordHandler = new StringDictionaryRecords(this.collectionName, this.encoding, this.genericSerializer, this.provider);

			if (RetainInMemory)
				this.inMemory = new Dictionary<string, object>();
			else
				this.inMemory = null;
		}

		/// <summary>
		/// This class manages a string dictionary in a persisted file.
		/// </summary>
		/// <param name="FileName">File name of index file.</param>
		/// <param name="BlobFileName">Name of file in which BLOBs are stored.</param>
		/// <param name="CollectionName">Collection Name.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="RetainInMemory">Retain the dictionary in memory.</param>
		public static async Task<StringDictionary> Create(string FileName, string BlobFileName, string CollectionName, FilesProvider Provider, bool RetainInMemory)
		{
			StringDictionary Result = new StringDictionary(CollectionName, Provider, RetainInMemory);

			Result.dictionaryFile = await ObjectBTreeFile.Create(FileName, Result.collectionName, BlobFileName,
				Result.provider.BlockSize, Result.provider.BlobBlockSize, Result.provider, Result.encoding, Result.timeoutMilliseconds,
				Result.provider.Encrypted, Result.recordHandler, null);

			Provider.Register(Result);

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.provider.Unregister(this);

			this.dictionaryFile?.Dispose();
			this.dictionaryFile = null;

			this.recordHandler = null;
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public bool ContainsKey(string key)
		{
			Task<bool> Task = this.ContainsKeyAsync(key);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public async Task<bool> ContainsKeyAsync(string key)
		{
			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.ContainsKey(key))
						return true;
				}
			}

			await this.dictionaryFile.BeginRead();
			try
			{
				BlockInfo Info = await this.dictionaryFile.FindNodeLocked(key);
				return !(Info is null);
			}
			finally
			{
				await this.dictionaryFile.EndRead();
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
			FilesProvider.Wait(this.AddAsync(key, value, false), this.timeoutMilliseconds);
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
				throw new ArgumentNullException("key is null.", "key");

			Type Type = value?.GetType() ?? typeof(object);
			IObjectSerializer Serializer = await this.provider.GetObjectSerializer(Type);

			await this.dictionaryFile.BeginWrite();
			try
			{
				byte[] Bin = await this.SerializeLocked(key, value, Serializer);
				BlockInfo Info = await this.dictionaryFile.FindLeafNodeLocked(key);
				if (Info is null)
				{
					if (!ReplaceIfExists)
						throw new ArgumentException("A key with that value already exists.", nameof(key));

					Info = await this.dictionaryFile.FindNodeLocked(key);

					await this.dictionaryFile.ReplaceObjectLocked(Bin, Info, true);
				}
				else
					await this.dictionaryFile.SaveNewObjectLocked(Bin, Info);
			}
			finally
			{
				await this.dictionaryFile.EndWrite();
			}

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					this.inMemory[key] = value;
				}
			}
		}

		/// <summary>
		/// Serializes a (Key,Value) pair.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Serializer">Serializer.</param>
		/// <returns>Serialized record.</returns>
		private async Task<byte[]> SerializeLocked(string Key, object Value, IObjectSerializer Serializer)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);

			Writer.WriteBit(true);
			Writer.Write(Key);

			await Serializer.Serialize(Writer, true, true, Value,
				NestedLocks.CreateIfNested(this.dictionaryFile, true, Serializer));

			return Writer.GetSerialization();
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
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
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
			if (key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					this.inMemory.Remove(key);
				}
			}

			object DeletedObject;

			await this.dictionaryFile.BeginWrite();
			try
			{
				DeletedObject = await this.dictionaryFile.DeleteObjectLocked(key, false, true, this.keyValueSerializer, null, 0);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
			finally
			{
				await this.dictionaryFile.EndWrite();
			}

			return !(DeletedObject is null);
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
			Task<KeyValuePair<bool, object>> Task = this.TryGetValueAsync(key);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
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
		public async Task<KeyValuePair<bool, object>> TryGetValueAsync(string key)
		{
			if (key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.TryGetValue(key, out object value))
						return new KeyValuePair<bool, object>(true, value);
				}
			}

			await this.dictionaryFile.BeginRead();
			try
			{
				object Result = await this.dictionaryFile.TryLoadObjectLocked(key, this.keyValueSerializer);
				if (Result is null)
					return new KeyValuePair<bool, object>(false, null);
				else
					return new KeyValuePair<bool, object>(true, Result);
			}
			finally
			{
				await this.dictionaryFile.EndRead();
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
			if (key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.TryGetValue(key, out object value))
						return new KeyValuePair<string, object>(key, value);
				}
			}

			await this.dictionaryFile.BeginRead();
			try
			{
				return (KeyValuePair<string, object>)await this.dictionaryFile.LoadObjectLocked(key, this.keyValueSerializer);
			}
			finally
			{
				await this.dictionaryFile.EndRead();
			}
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
			FilesProvider.Wait(this.ClearAsync(), this.timeoutMilliseconds);
		}

		/// <summary>
		/// Clears the dictionary.
		/// </summary>
		public async Task ClearAsync()
		{
			await this.dictionaryFile.ClearAsync();

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					this.inMemory.Clear();
				}
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.Contains(T)"/>
		/// </summary>
		public bool Contains(KeyValuePair<string, object> item)
		{
			if (!this.TryGetValue(item.Key, out object Value))
				return false;

			if (Value is null)
				return item.Value is null;
			else
				return Value.Equals(item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.CopyTo(T[], int)"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			Task Task = this.CopyToAsync(array, arrayIndex);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
		}

		/// <summary>
		/// Copies the contents of the dicitionary to an array.
		/// </summary>
		/// <param name="array">Array</param>
		/// <param name="arrayIndex">Start index</param>
		public async Task CopyToAsync(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			await this.dictionaryFile.BeginRead();
			try
			{
				ObjectBTreeFileCursor<KeyValuePair<string, object>> e = await this.GetEnumeratorLocked();

				while (await e.MoveNextAsyncLocked())
					array[arrayIndex++] = e.Current;
			}
			finally
			{
				await this.dictionaryFile.EndRead();
			}
		}

		/// <summary>
		/// Loads the entire table and returns it as an array.
		/// </summary>
		/// <returns>Array of key-value pairs.</returns>
		public KeyValuePair<string, object>[] ToArray()
		{
			Task<KeyValuePair<string, object>[]> Task = this.ToArrayAsync();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Loads the entire table and returns it as an array.
		/// </summary>
		/// <returns>Array of key-value pairs.</returns>
		public async Task<KeyValuePair<string, object>[]> ToArrayAsync()
		{
			await this.dictionaryFile.BeginRead();
			try
			{
				List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>();
				ObjectBTreeFileCursor<KeyValuePair<string, object>> e = await this.GetEnumeratorLocked();

				while (await e.MoveNextAsyncLocked())
					Result.Add(e.Current);

				return Result.ToArray();
			}
			finally
			{
				await this.dictionaryFile.EndRead();
			}
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
			ObjectBTreeFileCursor<KeyValuePair<string, object>> Cursor = this.GetCursor();
			CursorEnumerator<KeyValuePair<string, object>> e = new CursorEnumerator<KeyValuePair<string, object>>(Cursor, this.ResetCursor, this.timeoutMilliseconds);
			return e;
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			ObjectBTreeFileCursor<KeyValuePair<string, object>> Cursor = this.GetCursor();
			CursorEnumerator<KeyValuePair<string, object>> e = new CursorEnumerator<KeyValuePair<string, object>>(Cursor, this.ResetCursor, this.timeoutMilliseconds);
			return e;
		}

		private ObjectBTreeFileCursor<KeyValuePair<string, object>> GetCursor()
		{
			Task<ObjectBTreeFileCursor<KeyValuePair<string, object>>> Task = this.GetCursorAsync();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		private async Task<ObjectBTreeFileCursor<KeyValuePair<string, object>>> GetCursorAsync()
		{
			ObjectBTreeFileCursor<KeyValuePair<string, object>> Result;

			try
			{
				await this.dictionaryFile.BeginRead();
				Result = await this.GetEnumeratorLocked();
				Result.readLock = true;
			}
			catch (Exception ex)
			{
				await this.dictionaryFile.EndRead();
				ExceptionDispatchInfo.Capture(ex).Throw();
				Result = null;
			}

			return Result;
		}

		/// <summary>
		/// Gets an enumerator for all entries in the dictionary.
		/// </summary>
		/// <returns>Enumerator</returns>
		public Task<ObjectBTreeFileCursor<KeyValuePair<string, object>>> GetEnumeratorLocked()
		{
#if ASSERT_LOCKS
			this.dictionaryFile.fileAccess.AssertReadingOrWriting();
#endif
			return ObjectBTreeFileCursor<KeyValuePair<string, object>>.CreateLocked(this.dictionaryFile, this.recordHandler, this.keyValueSerializer);
		}

		private ICursor<KeyValuePair<string, object>> ResetCursor(ICursor<KeyValuePair<string, object>> Cursor)
		{
			if (Cursor is ObjectBTreeFileCursor<KeyValuePair<string, object>> e)
				e.GoToFirstLocked().Wait();

			return Cursor;
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
				return (int)this.dictionaryFile.CountAsync.Result;
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
				Task<KeyValuePair<string, object>> Task = this.GetValueAsync(key);
				FilesProvider.Wait(Task, this.timeoutMilliseconds);
				return Task.Result.Value;
			}

			set
			{
				FilesProvider.Wait(this.AddAsync(key, value, true), this.timeoutMilliseconds);
			}
		}

		/// <summary>
		/// Deletes the dictionary and disposes the object.
		/// </summary>
		public void DeleteAndDispose()
		{
			if (!(this.dictionaryFile is null))
			{
				string FileName = this.dictionaryFile.FileName;
				string BlobFileName = this.dictionaryFile.BlobFileName;

				this.Dispose();

				File.Delete(FileName);
				File.Delete(BlobFileName);
			}
		}
	}
}

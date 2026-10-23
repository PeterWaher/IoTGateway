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
using Waher.Runtime.Collections;
using Waher.Persistence.Files.Storage.PrimaryKeys;

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
		/// <param name="Key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public bool ContainsKey(string Key)
		{
			Task<bool> Task = this.ContainsKeyAsync(Key);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.IDictionary{string,object} contains an element with the specified key.
		/// </summary>
		/// <param name="Key">The key to locate in the System.Collections.Generic.IDictionary{string,object}.</param>
		/// <returns>true if the System.Collections.Generic.IDictionary{string,object} contains an element with the key; otherwise, false.</returns>
		public async Task<bool> ContainsKeyAsync(string Key)
		{
			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.ContainsKey(Key))
						return true;
				}
			}

			if (this.dictionaryFile is null)
				return false;

			await this.dictionaryFile.BeginRead();
			try
			{
				BlockInfo Info = await this.dictionaryFile.FindNodeLocked(
					new StringPrimaryKey(Key), false);

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
		/// <param name="Key">The object to use as the key of the element to add.</param>
		/// <param name="Value">The object to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public void Add(string Key, object Value)
		{
			FilesProvider.Wait(this.AddAsync(Key, Value, false), this.timeoutMilliseconds);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="Key">The object to use as the key of the element to add.</param>
		/// <param name="Value">The object to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public Task AddAsync(string Key, object Value)
		{
			return this.AddAsync(Key, Value, false);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary{string,object}.
		/// </summary>
		/// <param name="Key">The object to use as the key of the element to add.</param>
		/// <param name="Value">The object to use as the value of the element to add.</param>
		/// <param name="ReplaceIfExists">If replacement of any existing value is desired.</param>
		/// <exception cref="ArgumentNullException">key is null</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the System.Collections.Generic.IDictionary{string,object}.</exception>
		public async Task AddAsync(string Key, object Value, bool ReplaceIfExists)
		{
			if (Key is null)
				throw new ArgumentNullException("key is null.", "key");

			Type Type = Value?.GetType() ?? typeof(object);
			IObjectSerializer Serializer = await this.provider.GetObjectSerializer(Type);

			await this.dictionaryFile.BeginWrite();
			try
			{
				byte[] Bin = await this.SerializeLocked(Key, Value, Serializer);
				IPrimaryKey PrimaryKey = new StringPrimaryKey(Key);
				BlockInfo Info;

				if (ReplaceIfExists)
				{
					Info = await this.dictionaryFile.FindNodeLocked(PrimaryKey, true);

					if (Info.Match)
						await this.dictionaryFile.ReplaceObjectLocked(Bin, Info, true);
					else
						await this.dictionaryFile.SaveNewObjectLocked(Bin, Info);
				}
				else
				{
					Info = await this.dictionaryFile.FindLeafNodeLocked(PrimaryKey);
					if (Info is null)
						throw new ArgumentException("A key with that value already exists.", nameof(Key));

					await this.dictionaryFile.SaveNewObjectLocked(Bin, Info);
				}
			}
			finally
			{
				await this.dictionaryFile.EndWrite();
			}

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					this.inMemory[Key] = Value;
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
		/// <param name="Key">The key of the element to remove.</param>
		/// <returns>If key was found and removed.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public bool Remove(string Key)
		{
			Task<bool> Task = this.RemoveAsync(Key);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Removes the element with the specified key from the System.Collections.IDictionary object.
		/// </summary>
		/// <param name="Key">The key of the element to remove.</param>
		/// <returns>If key was found and removed.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<bool> RemoveAsync(string Key)
		{
			if (Key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					this.inMemory.Remove(Key);
				}
			}

			object DeletedObject;

			await this.dictionaryFile.BeginWrite();
			try
			{
				DeletedObject = await this.dictionaryFile.DeleteObjectLocked(
					new StringPrimaryKey(Key), false, true, this.keyValueSerializer, 
					null, 0);
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
		/// <param name="Key">The key whose value to get.</param>
		/// <param name="Value">When this method returns, the value associated with the specified key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element with the specified key; 
		/// otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public bool TryGetValue(string Key, out object Value)
		{
			Task<KeyValuePair<bool, object>> Task = this.TryGetValueAsync(Key);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			KeyValuePair<bool, object> P = Task.Result;
			Value = P.Value;
			return P.Key;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="Key">The key whose value to get.</param>
		/// <returns>Returns a pair of values:
		/// 
		/// First value is true if the object that implements System.Collections.Generic.IDictionary{string,object} contains an element 
		/// with the specified key; otherwise, false.
		/// When this method returns, the second value associated with the specified key, if the key is found; otherwise, 
		/// the default value for the type of the value parameter. This parameter is passed uninitialized.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		public async Task<KeyValuePair<bool, object>> TryGetValueAsync(string Key)
		{
			if (Key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.TryGetValue(Key, out object value))
						return new KeyValuePair<bool, object>(true, value);
				}
			}

			await this.dictionaryFile.BeginRead();
			try
			{
				object Result = await this.dictionaryFile.TryLoadObjectLocked(
					new StringPrimaryKey(Key), this.keyValueSerializer);

				if (Result is null)
					return new KeyValuePair<bool, object>(false, null);
				else if (Result is KeyValuePair<string, object> P)
					return new KeyValuePair<bool, object>(P.Key == Key, P.Value);
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
		/// <param name="Key">The key whose value to get.</param>
		/// <returns>Returns the value associated with the specified key, if the key is found.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		/// <exception cref="KeyNotFoundException">If <paramref name="Key"/> was not found.</exception>
		public async Task<KeyValuePair<string, object>> GetValueAsync(string Key)
		{
			if (Key is null)
				throw new ArgumentNullException("key is null.", "key");

			if (!(this.inMemory is null))
			{
				lock (this.inMemory)
				{
					if (this.inMemory.TryGetValue(Key, out object Value))
						return new KeyValuePair<string, object>(Key, Value);
				}
			}

			await this.dictionaryFile.BeginRead();
			try
			{
				object Obj = await this.dictionaryFile.LoadObjectLocked(
					new StringPrimaryKey(Key), this.keyValueSerializer);

				if (Obj is KeyValuePair<string, object> P)
					return P;
				else
					return new KeyValuePair<string, object>(Key, Obj);
			}
			finally
			{
				await this.dictionaryFile.EndRead();
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.Add(T)"/>
		/// </summary>
		public void Add(KeyValuePair<string, object> Item)
		{
			this.Add(Item.Key, Item.Value);
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
			if (!(this.dictionaryFile is null))
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
		public bool Contains(KeyValuePair<string, object> Item)
		{
			if (!this.TryGetValue(Item.Key, out object Value))
				return false;

			if (Value is null)
				return Item.Value is null;
			else
				return Value.Equals(Item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.CopyTo(T[], int)"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] Array, int ArrayIndex)
		{
			Task Task = this.CopyToAsync(Array, ArrayIndex);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
		}

		/// <summary>
		/// Copies the contents of the dicitionary to an array.
		/// </summary>
		/// <param name="Array">Array</param>
		/// <param name="ArrayIndex">Start index</param>
		public async Task CopyToAsync(KeyValuePair<string, object>[] Array, int ArrayIndex)
		{
			await this.dictionaryFile.BeginRead();
			try
			{
				ObjectBTreeFileCursor<KeyValuePair<string, object>> e = await this.GetEnumeratorLocked();

				while (await e.MoveNextAsyncLocked())
					Array[ArrayIndex++] = e.Current;
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
				ChunkedList<KeyValuePair<string, object>> Result = new ChunkedList<KeyValuePair<string, object>>();
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
		public bool Remove(KeyValuePair<string, object> Item)
		{
			if (!this.Contains(Item))
				return false;
			else
				return this.Remove(Item.Key);
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
		public ObjectBTreeFile DictionaryFile => this.dictionaryFile;

		/// <summary>
		/// Name of corresponding collection name.
		/// </summary>
		public string CollectionName => this.collectionName;

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding => this.encoding;

		/// <summary>
		/// <see cref="IDictionary{TKey, TValue}.Keys"/>
		/// </summary>
		public ICollection<string> Keys
		{
			get
			{
				this.keyCollection ??= new KeyCollection(this);
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
				this.valueCollection ??= new ValueCollection(this);
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
		public bool IsReadOnly => false;

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="Key">The key of the element to get or set.</param>
		/// <returns>The element with the specified key.</returns>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		/// <exception cref="KeyNotFoundException">The property is retrieved and key is not found.</exception>
		public object this[string Key]
		{
			get
			{
				Task<KeyValuePair<string, object>> Task = this.GetValueAsync(Key);
				FilesProvider.Wait(Task, this.timeoutMilliseconds);
				return Task.Result.Value;
			}

			set
			{
				FilesProvider.Wait(this.AddAsync(Key, value, true), this.timeoutMilliseconds);
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

		/// <summary>
		/// Gets a range of entries from one key to another.
		/// </summary>
		/// <param name="FromKey">Inclusive limit. If null, all keys to <paramref name="ToKey"/> are included.</param>
		/// <param name="ToKey">Exclusive limit. If null, all keys from <paramref name="FromKey"/> are included.</param>
		/// <returns>Found entries.</returns>
		public async Task<KeyValuePair<string, object>[]> GetEntriesAsync(string FromKey, string ToKey)
		{
			await this.dictionaryFile.BeginRead();
			try
			{
				ICursor<KeyValuePair<string, object>> Entries;

				if (FromKey is null)
					Entries = await this.GetEnumeratorLocked();
				else
				{
					BlockInfo Info = await this.dictionaryFile.FindNodeLocked(
						new StringPrimaryKey(FromKey), true);

					ObjectBTreeFileCursor<KeyValuePair<string, object>> e =
						await ObjectBTreeFileCursor<KeyValuePair<string, object>>.CreateLocked(this.dictionaryFile, this.recordHandler, this.keyValueSerializer);

					e.SetStartingPoint(Info);

					Entries = e;
				}

				ChunkedList<KeyValuePair<string, object>> Result = new ChunkedList<KeyValuePair<string, object>>();

				if (ToKey is null)
				{
					while (await Entries.MoveNextAsyncLocked())
						Result.Add(Entries.Current);
				}
				else
				{
					while (await Entries.MoveNextAsyncLocked())
					{
						if (string.Compare(Entries.Current.Key, ToKey, false) >= 0)
							break;

						Result.Add(Entries.Current);
					}
				}

				return Result.ToArray();
			}
			finally
			{
				await this.dictionaryFile.EndRead();
			}
		}

		/// <summary>
		/// Copies available keys to a string array.
		/// </summary>
		/// <param name="Keys">Array to receive keys.</param>
		/// <param name="Offset">Offset into array to start copying to.</param>
		public Task CopyKeysToAsync(string[] Keys, int Offset)
		{
			return ((KeyCollection)this.Keys).CopyToAsync(Keys, Offset);
		}

		/// <summary>
		/// Copies available values to an array.
		/// </summary>
		/// <param name="Values">Array to receive values.</param>
		/// <param name="Offset">Offset into array to start copying to.</param>
		public Task CopyValuesToAsync(object[] Values, int Offset)
		{
			return ((ValueCollection)this.Values).CopyToAsync(Values, Offset);
		}

		/// <summary>
		/// Gets all keys.
		/// </summary>
		/// <returns>Array of keys.</returns>
		public Task<string[]> GetKeysAsync()
		{
			return ((KeyCollection)this.Keys).GetKeysAsync();
		}

		/// <summary>
		/// Gets all values.
		/// </summary>
		/// <returns>Array of values.</returns>
		public Task<object[]> GetValuesAsync()
		{
			return ((ValueCollection)this.Values).GetValuesAsync();
		}

	}
}

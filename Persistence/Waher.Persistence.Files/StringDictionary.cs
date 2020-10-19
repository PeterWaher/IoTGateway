using System;
using System.Collections;
using System.Collections.Generic;
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
		/// <param name="FileName">File name of index file.</param>
		/// <param name="BlobFileName">Name of file in which BLOBs are stored.</param>
		/// <param name="CollectionName">Collection Name.</param>
		/// <param name="Provider">Files provider.</param>
		/// <param name="RetainInMemory">Retain the dictionary in memory.</param>
		public StringDictionary(string FileName, string BlobFileName, string CollectionName, FilesProvider Provider, bool RetainInMemory)
		{
			this.provider = Provider;
			this.collectionName = CollectionName;
			this.encoding = this.provider.Encoding;
			this.timeoutMilliseconds = this.provider.TimeoutMilliseconds;
			this.genericSerializer = new GenericObjectSerializer(this.provider);
			this.keyValueSerializer = new KeyValueSerializer(this.provider, this.genericSerializer);

			this.recordHandler = new StringDictionaryRecords(this.collectionName, this.encoding,
				this.genericSerializer, this.provider);

			this.dictionaryFile = new ObjectBTreeFile(FileName, this.collectionName, BlobFileName,
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

			await this.dictionaryFile.LockRead();
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
			IObjectSerializer Serializer = this.provider.GetObjectSerializer(Type);

			await this.dictionaryFile.LockWrite();
			try
			{
				byte[] Bin = this.SerializeLocked(key, value, Serializer);
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
		private byte[] SerializeLocked(string Key, object Value, IObjectSerializer Serializer)
		{
			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);

			Writer.WriteBit(true);
			Writer.Write(Key);

			Serializer.Serialize(Writer, true, true, Value);

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

			await this.dictionaryFile.LockWrite();
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

			await this.dictionaryFile.LockRead();
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

			await this.dictionaryFile.LockRead();
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
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(LockType.Read);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);

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
			List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>();
			using (ObjectBTreeFileEnumerator<KeyValuePair<string, object>> e = await this.GetEnumerator(LockType.Read))
			{
				while (await e.MoveNextAsync())
					Result.Add(e.Current);
			}

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
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(LockType.None);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.GetEnumerator(LockType.None);
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Gets an enumerator for all entries in the dictionary.
		/// </summary>
		/// <param name="LockType">
		/// If locked access to the file is requested, and of what type.
		/// 
		/// If unlocked access is desired, any change to the database will invalidate the enumerator, and further access to the
		/// enumerator will cause an <see cref="InvalidOperationException"/> to be thrown.
		/// 
		/// If read locked access is desired, the database cannot be updated, until the enumerator has been disposed.
		/// If write locked access is desired, the database cannot be accessed at all, until the enumerator has been disposed.
		/// 
		/// Make sure to call the <see cref="ObjectBTreeFileEnumerator{T}.Dispose"/> method when done with the enumerator, to release 
		/// the database lock after use.
		/// </param>
		/// <returns>Enumerator</returns>
		public async Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> GetEnumerator(LockType LockType)
		{
			ObjectBTreeFileEnumerator<KeyValuePair<string, object>> Result = null;

			try
			{
				Result = new ObjectBTreeFileEnumerator<KeyValuePair<string, object>>(this.dictionaryFile, this.recordHandler, this.keyValueSerializer);
				if (LockType != LockType.None)
					await Result.Lock(LockType);
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				Result = null;

				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Result;
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
				return this.dictionaryFile.Count;
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

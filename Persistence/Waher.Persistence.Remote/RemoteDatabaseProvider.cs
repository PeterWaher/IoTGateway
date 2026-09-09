using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Profiling;

namespace Waher.Persistence.Remote
{
	/// <summary>
	/// Database provider acting as a proxy to a remote database provider.
	/// </summary>
	public class RemoteDatabaseProvider : IDatabaseProvider, ISerializerContext, IDisposable
	{
		#region IDisposable

		/// <summary>
		/// Disposes the object.
		/// </summary>
		public void Dispose()
		{
			// TODO
		}

		#endregion

		#region ISerializerContext

		/// <summary>
		/// An ID of the serialization context. It's unique, and constant during the life-time of the application.
		/// </summary>
		public string Id
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Timeout, in milliseconds, for asynchronous operations.
		/// </summary>
		public int TimeoutMilliseconds
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// If normalized names are to be used or not. Normalized names reduces the number
		/// of bytes required to serialize objects, but do not work in a decentralized
		/// architecture.
		/// </summary>
		public bool NormalizedNames
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public Task<ulong> GetFieldCode(string Collection, string FieldName)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public Task<string> GetFieldName(string Collection, ulong FieldCode)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public Task<IObjectSerializer> GetObjectSerializer(Type Type)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type, if one exists.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer if exists, or null if not.</returns>
		public Task<IObjectSerializer> GetObjectSerializerNoCreate(Type Type)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Creates a new GUID.
		/// </summary>
		/// <returns>New GUID.</returns>
		public Guid CreateGuid()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Saves an unsaved object, and returns a new GUID identifying the saved object.
		/// </summary>
		/// <param name="Value">Object to save.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		/// <returns>GUID identifying the saved object.</returns>
		public Task<Guid> SaveNewObject(object Value, object State)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public Task<T> TryLoadObject<T>(Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <paramref name="T"/>.
		/// </summary>
		/// <param name="T">Base type.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public Task<object> TryLoadObject(Type T, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Encrypts field data.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Property">Name of property.</param>
		/// <param name="Collection">Collection in which the data is persisted.</param>
		/// <param name="ObjectId">Object ID of object with encrypted data.</param>
		/// <param name="MinLength">Minimum length of the property, in bytes, before 
		/// encryption. If the clear text property is shorter than this, random bytes 
		/// will be appended to pad the property to this length, before encryption.</param>
		/// <returns>Encrypted field data.</returns>
		public Task<byte[]> Encrypt(byte[] Data, string Property, string Collection, Guid ObjectId,
			int MinLength)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Decrypts field data.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Property">Name of property.</param>
		/// <param name="Collection">Collection in which the data is persisted.</param>
		/// <param name="ObjectId">Object ID of object with encrypted data.</param>
		/// <returns>Decrypted field data.</returns>
		public Task<byte[]> Decrypt(byte[] Data, string Property, string Collection, Guid ObjectId)
		{
			throw new NotImplementedException();    // TODO
		}

		#endregion

		#region IDatabaseProvider

		/// <summary>
		/// Inserts an object into the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(object Object)
		{
			throw new NotImplementedException();	// TODO
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(params object[] Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(IEnumerable<object> Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task InsertLazy(object Object, ObjectCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task InsertLazy(object[] Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task InsertLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<T>> Find<T>(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public Task<IPage<T>> FindFirst<T>(int PageSize, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public Task<IPage<T>> FindFirst<T>(int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public Task<IPage<object>> FindFirst(string Collection, int PageSize, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public Task<IPage<object>> FindFirst(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public Task<IPage<T>> FindFirst<T>(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the next page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<T>> FindNext<T>(IPage<T> Page)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds the next page of objects in a given collection.
		/// </summary>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<object>> FindNext(IPage<object> Page)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, int Offset, int MaxCount,
			Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Processes untyped objects.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Collection">Name of collection to process.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process(IProcessor<object> Processor, string Collection, int Offset, int MaxCount,
			params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Processes untyped objects.
		/// </summary>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Collection">Name of collection to process.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process(IProcessor<object> Processor, string Collection, int Offset, int MaxCount,
			Filter Filter, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Processes objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Processor">Processor to call for every object, unless the
		/// processor returns false, in which the process is cancelled.</param>
		/// <param name="Collection">Name of collection to process.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to process.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>If process was completed (true) or cancelled (false).</returns>
		public Task<bool> Process<T>(IProcessor<T> Processor, string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Update(object Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(params object[] Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(IEnumerable<object> Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates an object in the database, if unlocked. If locked, object will be updated at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task UpdateLazy(object Object, ObjectCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task UpdateLazy(object[] Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task UpdateLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Delete(object Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(params object[] Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes an object in the database, if unlocked. If locked, object will be deleted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(object Object, ObjectCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(object[] Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found and deleted.</returns>
		public Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found and deleted.</returns>
		public Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found and deleted.</returns>
		public Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found and deleted.</returns>
		public Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy<T>(int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy<T>(int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(string Collection, int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(string Collection, int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its class type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public Task<T> TryLoadObject<T>(object ObjectId)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its class type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public Task<T> TryLoadObject<T>(string CollectionName, object ObjectId)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object, or null if not found.</returns>
		public Task<object> TryLoadObject(string CollectionName, object ObjectId)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(IDatabaseExport Output, string[] CollectionNames)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(IDatabaseExport Output, string[] CollectionNames, ProfilerThread Thread)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>public Task object for synchronization purposes.</returns>
		public Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>public Task object for synchronization purposes.</returns>
		public Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames, ProfilerThread Thread)
			where T : class
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Clears a collection of all objects.
		/// </summary>
		/// <param name="CollectionName">Name of collection to clear.</param>
		/// <returns>public Task object for synchronization purposes.</returns>
		public Task Clear(string CollectionName)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <returns>Collections with errors found.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors found.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <returns>Collections with errors found and repaired.</returns>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors found and repaired.</returns>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <returns>Collections with errors found, and repaired if <paramref name="Repair"/>=true.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors found, and repaired if <paramref name="Repair"/>=true.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair, ProfilerThread Thread)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(params string[] CollectionNames)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(ProfilerThread Thread, params string[] CollectionNames)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Adds an index to a collection, if one does not already exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public Task AddIndex(string CollectionName, string[] FieldNames)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public Task RemoveIndex(string CollectionName, string[] FieldNames)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <returns>Sort order of each index. Each string represents a field name. 
		/// By default, sort order is ascending. If descending sort order is desired, 
		/// the field name is prefixed by a hyphen (minus) sign.</returns>
		public Task<string[][]> GetIndices(string CollectionName)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Starts bulk-proccessing of data. Must be followed by a call to <see cref="EndBulk"/>.
		/// </summary>
		public Task StartBulk()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Ends bulk-processing of data. Must be called once for every call to <see cref="StartBulk"/>.
		/// </summary>
		public Task EndBulk()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		public Task Start()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		public Task Stop()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		public Task Flush()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Number of bytes used by an Object ID.
		/// </summary>
		public int ObjectIdByteCount
		{
			get
			{
				throw new NotImplementedException();    // TODO
			}
		}

		/// <summary>
		/// Gets a persistent dictionary containing objects in a collection.
		/// </summary>
		/// <param name="Collection">Collection Name</param>
		/// <returns>Persistent dictionary</returns>
		public Task<IPersistentDictionary> GetDictionary(string Collection)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets an array of available dictionary collections.
		/// </summary>
		/// <returns>Array of dictionary collections.</returns>
		public Task<string[]> GetDictionaries()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets a persistent dictionary containing objects in a collection.
		/// </summary>
		/// <param name="QueueName">Queue Name</param>
		/// <param name="CanBeNull">Can return null if a queue is not found.</param>
		/// <returns>Persistent queue</returns>
		public Task<IPersistedQueue> GetQueue(string QueueName, bool CanBeNull)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets an array of available queue names.
		/// </summary>
		/// <returns>Array of queue names.</returns>
		public Task<string[]> GetQueues()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public Task<string[]> GetCollections()
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets the collection corresponding to a given type.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <returns>Collection name.</returns>
		public Task<string> GetCollection(Type Type)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets the collection corresponding to a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Collection name.</returns>
		public Task<string> GetCollection(Object Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Checks if a string is a label in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="Label">Label to check.</param>
		/// <returns>If <paramref name="Label"/> is a label in the collection
		/// defined by <paramref name="Collection"/>.</returns>
		public Task<bool> IsLabel(string Collection, string Label)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets an array of available labels for a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <returns>Array of labels.</returns>
		public Task<string[]> GetLabels(string Collection)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Tries to get the Object ID of an object, if it exists.
		/// </summary>
		/// <param name="Object">Object whose Object ID is of interest.</param>
		/// <returns>Object ID, if found, null otherwise.</returns>
		public Task<object> TryGetObjectId(object Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Drops a collection, if it exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		public Task DropCollection(string CollectionName)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Creates a generalized representation of an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Generalized representation.</returns>
		public Task<GenericObject> Generalize(object Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Creates a specialized representation of a generic object.
		/// </summary>
		/// <param name="Object">Generic object.</param>
		/// <returns>Specialized representation.</returns>
		public Task<object> Specialize(GenericObject Object)
		{
			throw new NotImplementedException();    // TODO
		}

		/// <summary>
		/// Gets an array of collections that should be excluded from backups.
		/// </summary>
		/// <returns>Array of excluded collections.</returns>
		public string[] GetExcludedCollections()
		{
			throw new NotImplementedException();    // TODO
		}

		#endregion
	}
}

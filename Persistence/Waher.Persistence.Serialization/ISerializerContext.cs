using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Delegate for embedded object value setter methods. Is used when loading embedded objects.
	/// </summary>
	/// <param name="EmbeddedObject">Embedded object.</param>
	public delegate void EmbeddedObjectSetter(object EmbeddedObject);

	/// <summary>
	/// Serialization context.
	/// </summary>
	public interface ISerializerContext
	{
		/// <summary>
		/// An ID of the serialization context. It's unique, and constant during the life-time of the application.
		/// </summary>
		string Id
		{
			get;
		}

		/// <summary>
		/// Default collection name.
		/// </summary>
		string DefaultCollectionName
		{
			get;
		}

		/// <summary>
		/// Timeout, in milliseconds, for asynchronous operations.
		/// </summary>
		int TimeoutMilliseconds
		{
			get;
		}

		/// <summary>
		/// If normalized names are to be used or not. Normalized names reduces the number
		/// of bytes required to serialize objects, but do not work in a decentralized
		/// architecture.
		/// </summary>
		bool NormalizedNames
		{
			get;
		}

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		Task<ulong> GetFieldCode(string Collection, string FieldName);

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		Task<string> GetFieldName(string Collection, ulong FieldCode);

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		Task<IObjectSerializer> GetObjectSerializer(Type Type);
		
		/// <summary>
		/// Gets the object serializer corresponding to a specific type, if one exists.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer if exists, or null if not.</returns>
		Task<IObjectSerializer> GetObjectSerializerNoCreate(Type Type);

		/// <summary>
		/// Creates a new GUID.
		/// </summary>
		/// <returns>New GUID.</returns>
		Guid CreateGuid();

		/// <summary>
		/// Saves an unsaved object, and returns a new GUID identifying the saved object.
		/// </summary>
		/// <param name="Value">Object to save.</param>
		/// <returns>GUID identifying the saved object.</returns>
		Task<Guid> SaveNewObject(object Value);

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		Task<T> TryLoadObject<T>(Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
			where T : class;

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <paramref name="T"/>.
		/// </summary>
		/// <param name="T">Base type.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		Task<object> TryLoadObject(Type T, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter);

	}
}

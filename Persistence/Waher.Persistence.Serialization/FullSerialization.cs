using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Provides a Serializer context for full serialization.
	/// </summary>
	public class FullSerialization : ISerializerContext
	{
		private readonly SerializerCollection serializers;
		private readonly string id = Guid.NewGuid().ToString();
		private readonly string defaultCollectionName;
		private readonly int timeoutMilliseconds;

		/// <summary>
		/// Provides a Serializer context for full serialization.
		/// </summary>
		public FullSerialization()
			: this(string.Empty)
		{
		}

		/// <summary>
		/// Provides a Serializer context for full serialization.
		/// </summary>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		public FullSerialization(string DefaultCollectionName)
			: this(DefaultCollectionName, 30000)
		{
		}

		/// <summary>
		/// Provides a Serializer context for full serialization.
		/// </summary>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="TimeoutMilliseconds">Timeout threshold, in milliseconds.</param>
		public FullSerialization(string DefaultCollectionName, int TimeoutMilliseconds)
		{
			this.defaultCollectionName = DefaultCollectionName;
			this.timeoutMilliseconds = TimeoutMilliseconds;
#if COMPILED
			this.serializers = new SerializerCollection(this, false);
#else
			this.serializers = new SerializerCollection(this);
#endif
		}

		/// <summary>
		/// An ID of the serialization context. It's unique, and constant during the life-time of the application.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName => this.defaultCollectionName;

		/// <summary>
		/// Timeout, in milliseconds, for asynchronous operations.
		/// </summary>
		public int TimeoutMilliseconds => this.timeoutMilliseconds;

		/// <summary>
		/// Serializers collection.
		/// </summary>
		public SerializerCollection Serializers => this.serializers;

		/// <summary>
		/// If normalized names are to be used or not. Normalized names reduces the number
		/// of bytes required to serialize objects, but do not work in a decentralized
		/// architecture.
		/// </summary>
		public bool NormalizedNames => false;

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public Task<ulong> GetFieldCode(string Collection, string FieldName) =>
			throw new InvalidOperationException();

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public Task<string> GetFieldName(string Collection, ulong FieldCode) =>
			throw new InvalidOperationException();

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public Task<IObjectSerializer> GetObjectSerializer(Type Type) =>
			this.serializers.GetObjectSerializer(Type);

		/// <summary>
		/// Gets the object serializer corresponding to a specific type, if one exists.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer if exists, or null if not.</returns>
		public Task<IObjectSerializer> GetObjectSerializerNoCreate(Type Type) =>
			this.serializers.GetObjectSerializerNoCreate(Type);

		/// <summary>
		/// Creates a new GUID.
		/// </summary>
		/// <returns>New GUID.</returns>
		public Guid CreateGuid() => Guid.NewGuid();

		/// <summary>
		/// Saves an unsaved object, and returns a new GUID identifying the saved object.
		/// </summary>
		/// <param name="Value">Object to save.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		/// <returns>GUID identifying the saved object.</returns>
		public Task<Guid> SaveNewObject(object Value, object State) =>
			throw new InvalidOperationException();

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public Task<T> TryLoadObject<T>(Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
			where T : class =>
			throw new InvalidOperationException();

		/// <summary>
		/// Tries to load an object given its Object ID <paramref name="ObjectId"/> and its base type <paramref name="T"/>.
		/// </summary>
		/// <param name="T">Base type.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public Task<object> TryLoadObject(Type T, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter) =>
			throw new InvalidOperationException();

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
			int MinLength) =>
			throw new InvalidOperationException();

		/// <summary>
		/// Decrypts field data.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Property">Name of property.</param>
		/// <param name="Collection">Collection in which the data is persisted.</param>
		/// <param name="ObjectId">Object ID of object with encrypted data.</param>
		/// <returns>Decrypted field data.</returns>
		public Task<byte[]> Decrypt(byte[] Data, string Property, string Collection, Guid ObjectId) =>
			throw new InvalidOperationException();
	}
}

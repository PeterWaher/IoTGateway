using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.NullableTypes
{
	/// <summary>
	/// Abstract base class for nullable value type serializers.
	/// </summary>
	public abstract class NullableValueTypeSerializer : IObjectSerializer
	{
		/// <summary>
		/// Abstract base class for nullable value type serializers.
		/// </summary>
		public NullableValueTypeSerializer()
		{
		}

		/// <summary>
		/// Initializes the serializer before first-time use.
		/// </summary>
		public Task Init()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public abstract Type ValueType
		{
			get;
		}

		/// <summary>
		/// If the value being serialized, can be null.
		/// </summary>
		public bool IsNullable
		{
			get { return true; }
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public abstract Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded);

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public abstract Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State);

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public virtual Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			return Task.FromResult<object>(null);
		}
	}
}

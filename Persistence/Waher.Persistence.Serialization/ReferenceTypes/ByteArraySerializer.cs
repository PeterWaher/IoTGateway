using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ReferenceTypes
{
	/// <summary>
	/// Serializes a byte array.
	/// </summary>
	public class ByteArraySerializer : IObjectSerializer
	{
		/// <summary>
		/// Serializes a byte array.
		/// </summary>
		public ByteArraySerializer()
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
		public Type ValueType
		{
			get
			{
				return typeof(byte[]);
			}
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
		public Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BYTEARRAY: return Task.FromResult<object>(Reader.ReadByteArray());
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a byte array.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_BYTEARRAY, 6);

				Writer.Write((byte[])Value);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			return Task.FromResult<object>(null);
		}

	}
}

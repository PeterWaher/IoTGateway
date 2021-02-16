using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="Guid"/> value.
	/// </summary>
	public class NullableGuidSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Guid"/> value.
		/// </summary>
		public NullableGuidSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(Guid?);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_GUID: return Task.FromResult<object>((Guid?)Reader.ReadGuid());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>((Guid?)Guid.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a nullable Guid value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			Guid? Value2 = (Guid?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return Task.CompletedTask;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_GUID, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);

			return Task.CompletedTask;
		}

	}
}

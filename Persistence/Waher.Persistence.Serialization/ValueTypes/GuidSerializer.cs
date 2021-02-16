using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="Guid"/> value.
	/// </summary>
	public class GuidSerializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="Guid"/> value.
		/// </summary>
		public GuidSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(Guid);
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
				case ObjectSerializer.TYPE_GUID: return Task.FromResult<object>(Reader.ReadGuid());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>(Guid.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a Guid value.");
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
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_GUID, 6);

			Writer.Write((Guid)Value);

			return Task.CompletedTask;
		}

	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="DateTimeOffset"/> value.
	/// </summary>
	public class DateTimeOffsetSerializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="DateTimeOffset"/> value.
		/// </summary>
		public DateTimeOffsetSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(DateTimeOffset);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override object Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_DATETIME: return (DateTimeOffset)Reader.ReadDateTime();
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return Reader.ReadDateTimeOffset();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return DateTimeOffset.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return DateTimeOffset.MinValue;
				case ObjectSerializer.TYPE_MAX: return DateTimeOffset.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a DateTimeOffset value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override void Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_DATETIMEOFFSET, 6);

			Writer.Write((DateTimeOffset)Value);
		}

	}
}

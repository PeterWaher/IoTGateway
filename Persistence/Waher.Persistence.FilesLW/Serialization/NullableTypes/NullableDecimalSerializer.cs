using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="Decimal"/> value.
	/// </summary>
	public class NullableDecimalSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Decimal"/> value.
		/// </summary>
		public NullableDecimalSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(decimal?);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (decimal?)1 : (decimal?)0;
				case ObjectSerializer.TYPE_BYTE: return (decimal?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (decimal?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (decimal?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (decimal?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (decimal?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (decimal?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (decimal?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (decimal?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (decimal?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (decimal?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (decimal?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (decimal?)decimal.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return decimal.MinValue;
				case ObjectSerializer.TYPE_MAX: return decimal.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Decimal value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			decimal? Value2 = (decimal?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_DECIMAL, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

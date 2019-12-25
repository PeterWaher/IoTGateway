using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="UInt16"/> value.
	/// </summary>
	public class UInt16Serializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="UInt16"/> value.
		/// </summary>
		public UInt16Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(ushort);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ushort)1 : (ushort)0;
				case ObjectSerializer.TYPE_BYTE: return (ushort)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ushort)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ushort)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ushort)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ushort)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (ushort)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (ushort)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ushort)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ushort)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ushort)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return ushort.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return ushort.MinValue;
				case ObjectSerializer.TYPE_MAX: return ushort.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an UInt16 value.");
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
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_UINT16, 6);

			Writer.Write((ushort)Value);
		}

	}
}

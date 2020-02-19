using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="UInt32"/> value.
	/// </summary>
	public class UInt32Serializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="UInt32"/> value.
		/// </summary>
		public UInt32Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(uint);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (uint)1 : (uint)0;
				case ObjectSerializer.TYPE_BYTE: return (uint)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (uint)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (uint)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (uint)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (uint)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (uint)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (uint)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (uint)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (uint)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (uint)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (uint)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return uint.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return uint.MinValue;
				case ObjectSerializer.TYPE_MAX: return uint.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an UInt32 value.");
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
				Writer.WriteBits(ObjectSerializer.TYPE_UINT32, 6);

			Writer.Write((uint)Value);
		}

	}
}

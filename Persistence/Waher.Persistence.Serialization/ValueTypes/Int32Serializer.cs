using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="Int32"/> value.
	/// </summary>
	public class Int32Serializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="Int32"/> value.
		/// </summary>
		public Int32Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(int);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (int)1 : (int)0;
				case ObjectSerializer.TYPE_BYTE: return (int)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (int)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (int)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (int)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (int)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (int)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (int)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (int)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (int)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (int)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return int.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return int.MinValue;
				case ObjectSerializer.TYPE_MAX: return int.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an Int32 value.");
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
				Writer.WriteBits(ObjectSerializer.TYPE_INT32, 6);

			Writer.Write((int)Value);
		}

	}
}

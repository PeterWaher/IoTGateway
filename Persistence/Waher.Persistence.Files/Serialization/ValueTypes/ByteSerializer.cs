using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class ByteSerializer : IObjectSerializer
	{
		public ByteSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(byte);
			}
		}

		public bool IsNullable
		{
			get { return false; }
		}

		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (byte)1 : (byte)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (byte)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (byte)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (byte)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (byte)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (byte)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (byte)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (byte)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (byte)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (byte)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (byte)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return byte.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a byte value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_BYTE, 6);

			Writer.Write((byte)Value);
		}

	}
}

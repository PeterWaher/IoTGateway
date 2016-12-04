using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class Int64Serializer : ValueTypeSerializer
	{
		public Int64Serializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(long);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (long)1 : (long)0;
				case ObjectSerializer.TYPE_BYTE: return (long)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (long)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (long)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (long)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (long)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (long)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (long)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (long)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (long)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (long)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return long.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an Int64 value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_INT64, 6);

			Writer.Write((long)Value);
		}

	}
}

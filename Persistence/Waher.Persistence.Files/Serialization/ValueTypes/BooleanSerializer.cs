using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class BooleanSerializer : IBinarySerializer
	{
		public BooleanSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(bool);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean();
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte() != 0;
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16() != 0;
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32() != 0;
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64() != 0;
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte() != 0;
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16() != 0;
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32() != 0;
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64() != 0;
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal() != 0;
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble() != 0;
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle() != 0;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a boolean value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_BOOLEAN, 6);

			Writer.Write((bool)Value);
		}
	}
}

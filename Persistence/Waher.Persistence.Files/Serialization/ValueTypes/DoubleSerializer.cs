using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class DoubleSerializer : IBinarySerializer
	{
		public DoubleSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(double);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (double)1 : (double)0;
				case ObjectSerializer.TYPE_BYTE: return (double)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (double)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (double)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (double)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (double)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (double)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (double)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (double)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (double)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (double)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return double.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a Double value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_DOUBLE, 6);

			Writer.Write((double)Value);
		}

	}
}

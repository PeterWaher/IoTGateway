using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class SingleSerializer : ValueTypeSerializer
	{
		public SingleSerializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(float);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (float)1 : (float)0;
				case ObjectSerializer.TYPE_BYTE: return (float)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (float)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (float)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (float)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (float)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (float)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (float)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (float)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (float)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (float)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return float.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return float.MinValue;
				case ObjectSerializer.TYPE_MAX: return float.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a Single value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_SINGLE, 6);

			Writer.Write((float)Value);
		}

	}
}

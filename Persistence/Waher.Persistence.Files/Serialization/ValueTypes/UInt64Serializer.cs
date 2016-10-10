using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class UInt64Serializer : IBinarySerializer
	{
		public UInt64Serializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(ulong);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ulong)1 : (ulong)0;
				case ObjectSerializer.TYPE_BYTE: return (ulong)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ulong)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ulong)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ulong)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ulong)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (ulong)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (ulong)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ulong)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ulong)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ulong)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return ulong.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an UInt64 value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_UINT64, 6);

			Writer.Write((ulong)Value);
		}

	}
}

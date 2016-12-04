using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableByteSerializer : NullableValueTypeSerializer
	{
		public NullableByteSerializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(byte?);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (byte?)1 : (byte?)0;
				case ObjectSerializer.TYPE_BYTE: return (byte?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (byte?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (byte?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (byte?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (byte?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (byte?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (byte?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (byte?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (byte?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (byte?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (byte?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (byte?)byte.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable byte value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			byte? Value2 = (byte?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_BYTE, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

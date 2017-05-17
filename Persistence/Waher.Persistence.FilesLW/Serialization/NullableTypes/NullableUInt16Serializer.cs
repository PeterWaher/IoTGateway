using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableUInt16Serializer : NullableValueTypeSerializer
	{
		public NullableUInt16Serializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(ushort?);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ushort?)1 : (ushort?)0;
				case ObjectSerializer.TYPE_BYTE: return (ushort?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ushort?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ushort?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ushort?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ushort?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (ushort?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (ushort?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (ushort?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ushort?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ushort?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ushort?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (ushort?)ushort.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return ushort.MinValue;
				case ObjectSerializer.TYPE_MAX: return ushort.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable UInt16 value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			ushort? Value2 = (ushort?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_UINT16, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

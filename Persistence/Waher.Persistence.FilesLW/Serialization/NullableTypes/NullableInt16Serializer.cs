using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableInt16Serializer : NullableValueTypeSerializer
	{
		public NullableInt16Serializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(short?);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (short?)1 : (short?)0;
				case ObjectSerializer.TYPE_BYTE: return (short?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (short?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (short?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (short?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (short?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (short?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (short?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (short?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (short?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (short?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (short?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (short?)short.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return short.MinValue;
				case ObjectSerializer.TYPE_MAX: return short.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Int16 value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			short? Value2 = (short?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_INT16, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

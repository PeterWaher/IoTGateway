using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableDoubleSerializer : NullableValueTypeSerializer
	{
		public NullableDoubleSerializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(double?);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (double?)1 : (double?)0;
				case ObjectSerializer.TYPE_BYTE: return (double?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (double?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (double?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (double?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (double?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (double?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (double?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (double?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (double?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (double?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (double?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (double?)double.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_MIN: return double.MinValue;
				case ObjectSerializer.TYPE_MAX: return double.MaxValue;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Double value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			double? Value2 = (double?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_DOUBLE, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableDecimalSerializer : IObjectSerializer
	{
		public NullableDecimalSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(decimal?);
			}
		}

		public bool IsNullable
		{
			get { return true; }
		}

		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (decimal?)1 : (decimal?)0;
				case ObjectSerializer.TYPE_BYTE: return (decimal?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (decimal?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (decimal?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (decimal?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (decimal?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (decimal?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (decimal?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (decimal?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (decimal?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (decimal?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (decimal?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (decimal?)decimal.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Decimal value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			decimal? Value2 = (decimal?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_DECIMAL, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

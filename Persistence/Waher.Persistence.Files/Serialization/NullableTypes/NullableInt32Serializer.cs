using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableInt32Serializer : IObjectSerializer
	{
		public NullableInt32Serializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(int?);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (int?)1 : (int?)0;
				case ObjectSerializer.TYPE_BYTE: return (int?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (int?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (int?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (int?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (int?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (int?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (int?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (int?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (int?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (int?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (int?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (int?)int.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Int32 value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			int? Value2 = (int?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_INT32, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

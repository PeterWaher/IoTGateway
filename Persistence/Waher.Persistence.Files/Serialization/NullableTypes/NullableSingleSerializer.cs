using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableSingleSerializer : IObjectSerializer
	{
		public NullableSingleSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(float?);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (float?)1 : (float?)0;
				case ObjectSerializer.TYPE_BYTE: return (float?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (float?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (float?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (float?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (float?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (float?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (float?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (float?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (float?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (float?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (float?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (float?)float.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable Single value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			float? Value2 = (float?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_SINGLE, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

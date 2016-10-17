using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableSByteSerializer : IObjectSerializer
	{
		public NullableSByteSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(sbyte?);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (sbyte?)1 : (sbyte?)0;
				case ObjectSerializer.TYPE_BYTE: return (sbyte?)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (sbyte?)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (sbyte?)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (sbyte?)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (sbyte?)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (sbyte?)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (sbyte?)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (sbyte?)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (sbyte?)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (sbyte?)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (sbyte?)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return (sbyte?)sbyte.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable sbyte value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			sbyte? Value2 = (sbyte?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_SBYTE, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}

	}
}

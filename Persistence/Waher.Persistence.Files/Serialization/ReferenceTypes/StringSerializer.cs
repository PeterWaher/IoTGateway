using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ReferenceTypes
{
	public class StringSerializer : IObjectSerializer
	{
		public StringSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(string);
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
				case ObjectSerializer.TYPE_ENUM:
				case ObjectSerializer.TYPE_STRING: return Reader.ReadString();
				case ObjectSerializer.TYPE_CHAR: return new string(Reader.ReadChar(), 1);
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean().ToString();
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadString().ToString();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16().ToString();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32().ToString();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64().ToString();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte().ToString();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16().ToString();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32().ToString();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64().ToString();
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal().ToString();
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble().ToString();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_GUID: return Reader.ReadGuid().ToString();
				case ObjectSerializer.TYPE_DATETIME: return Reader.ReadDateTime().ToString();
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadTimeSpan().ToString();
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a string value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value == null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_STRING, 6);

				Writer.Write((string)Value);
			}
		}

	}
}

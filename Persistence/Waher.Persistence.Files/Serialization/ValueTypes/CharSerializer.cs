using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class CharSerializer : IObjectSerializer
	{
		public CharSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(char);
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
				case ObjectSerializer.TYPE_CHAR: return Reader.ReadChar();
				case ObjectSerializer.TYPE_BYTE: return (char)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (char)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (char)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (char)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (char)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (char)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (char)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (char)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (char)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (char)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (char)Reader.ReadSingle();
				case ObjectSerializer.TYPE_NULL: return null;

				case ObjectSerializer.TYPE_STRING:
					string s = Reader.ReadString();
					return string.IsNullOrEmpty(s) ? (char)0 : s[0];

				default: throw new Exception("Expected a char value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_CHAR, 6);

			Writer.Write((char)Value);
		}

	}
}

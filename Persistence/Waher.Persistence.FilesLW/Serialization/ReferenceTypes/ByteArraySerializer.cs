using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ReferenceTypes
{
	public class ByteArraySerializer : IObjectSerializer
	{
		public ByteArraySerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(byte[]);
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
				case ObjectSerializer.TYPE_BYTEARRAY: return Reader.ReadByteArray();
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a byte array.");
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
					Writer.WriteBits(ObjectSerializer.TYPE_BYTEARRAY, 6);

				Writer.Write((byte[])Value);
			}
		}

		public bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			Value = null;
			return false;
		}

	}
}

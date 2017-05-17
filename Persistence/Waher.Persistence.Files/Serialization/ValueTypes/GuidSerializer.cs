using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class GuidSerializer : ValueTypeSerializer
	{
		public GuidSerializer()
		{
		}

		public override Type ValueType
		{
			get
			{
				return typeof(Guid);
			}
		}

		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_GUID: return Reader.ReadGuid();
				case ObjectSerializer.TYPE_STRING: return Guid.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a Guid value.");
			}
		}

		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_GUID, 6);

			Writer.Write((Guid)Value);
		}

	}
}

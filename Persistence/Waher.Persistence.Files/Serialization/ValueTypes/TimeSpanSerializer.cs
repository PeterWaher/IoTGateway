using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.ValueTypes
{
	public class TimeSpanSerializer : IObjectSerializer
	{
		public TimeSpanSerializer()
		{
		}

		public Type ValueType
		{
			get
			{
				return typeof(TimeSpan);
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
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadTimeSpan();
				case ObjectSerializer.TYPE_STRING: return TimeSpan.Parse(Reader.ReadString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a TimeSpan value.");
			}
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_TIMESPAN, 6);

			Writer.Write((TimeSpan)Value);
		}

	}
}

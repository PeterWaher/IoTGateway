using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	public interface IBinarySerializer
	{
		Type ValueType
		{
			get;
		}

		bool IsNullable
		{
			get;
		}

		object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded);

		void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value);
	}
}

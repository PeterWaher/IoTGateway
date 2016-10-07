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

		object Deserialize(BinaryDeserializer Reader);
	}
}

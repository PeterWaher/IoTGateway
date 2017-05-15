using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public abstract class NullableValueTypeSerializer : IObjectSerializer
	{
		public NullableValueTypeSerializer()
		{
		}

		public abstract Type ValueType
		{
			get;
		}

		public bool IsNullable
		{
			get { return true; }
		}

		public abstract object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded);

		public abstract void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value);

		public virtual bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			Value = null;
			return false;
		}
	}
}

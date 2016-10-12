using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Interface for object serializers.
	/// </summary>
	public interface IObjectSerializer
	{
		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		Type ValueType
		{
			get;
		}

		/// <summary>
		/// If the underlying object is nullable.
		/// </summary>
		bool IsNullable
		{
			get;
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded);

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value);
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="Boolean"/> value.
	/// </summary>
	public class BooleanSerializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="Boolean"/> value.
		/// </summary>
		public BooleanSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(bool);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>(Reader.ReadBoolean());
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>(Reader.ReadByte() != 0);
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>(Reader.ReadInt16() != 0);
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>(Reader.ReadInt32() != 0);
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>(Reader.ReadInt64() != 0);
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>(Reader.ReadSByte() != 0);
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>(Reader.ReadUInt16() != 0);
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>(Reader.ReadUInt32() != 0);
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>(Reader.ReadUInt64() != 0);
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>(Reader.ReadDecimal() != 0);
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>(Reader.ReadDouble() != 0);
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>(Reader.ReadSingle() != 0);
				case ObjectSerializer.TYPE_MIN: return Task.FromResult<object>(false);
				case ObjectSerializer.TYPE_MAX: return Task.FromResult<object>(true);
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a boolean value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
				Writer.WriteBits(ObjectSerializer.TYPE_BOOLEAN, 6);

			Writer.Write((bool)Value);

			return Task.CompletedTask;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="Boolean"/> value.
	/// </summary>
	public class NullableBooleanSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Boolean"/> value.
		/// </summary>
		public NullableBooleanSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(bool?);
			}
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return (bool?)(Reader.ReadBoolean());
				case ObjectSerializer.TYPE_BYTE: return (bool?)(Reader.ReadByte() != 0);
				case ObjectSerializer.TYPE_INT16: return (bool?)(Reader.ReadInt16() != 0);
				case ObjectSerializer.TYPE_INT32: return (bool?)(Reader.ReadInt32() != 0);
				case ObjectSerializer.TYPE_INT64: return (bool?)(Reader.ReadInt64() != 0);
				case ObjectSerializer.TYPE_SBYTE: return (bool?)(Reader.ReadSByte() != 0);
				case ObjectSerializer.TYPE_UINT16: return (bool?)(Reader.ReadUInt16() != 0);
				case ObjectSerializer.TYPE_UINT32: return (bool?)(Reader.ReadUInt32() != 0);
				case ObjectSerializer.TYPE_UINT64: return (bool?)(Reader.ReadUInt64() != 0);
				case ObjectSerializer.TYPE_DECIMAL: return (bool?)(Reader.ReadDecimal() != 0);
				case ObjectSerializer.TYPE_DOUBLE: return (bool?)(Reader.ReadDouble() != 0);
				case ObjectSerializer.TYPE_SINGLE: return (bool?)(Reader.ReadSingle() != 0);
				case ObjectSerializer.TYPE_MIN: return false;
				case ObjectSerializer.TYPE_MAX: return true;
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a nullable boolean value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			bool? Value2 = (bool?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_BOOLEAN, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);
		}
	}
}

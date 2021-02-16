using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="UInt32"/> value.
	/// </summary>
	public class NullableUInt32Serializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="UInt32"/> value.
		/// </summary>
		public NullableUInt32Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(uint?);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>(Reader.ReadBoolean() ? (uint?)1 : (uint?)0);
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>((uint?)Reader.ReadByte());
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>((uint?)Reader.ReadInt16());
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>((uint?)Reader.ReadInt32());
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>((uint?)Reader.ReadInt64());
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>((uint?)Reader.ReadSByte());
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>((uint?)Reader.ReadUInt16());
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>((uint?)Reader.ReadUInt32());
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>((uint?)Reader.ReadUInt64());
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>((uint?)Reader.ReadDecimal());
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>((uint?)Reader.ReadDouble());
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>((uint?)Reader.ReadSingle());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>((uint?)uint.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_MIN: return Task.FromResult<object>(uint.MinValue);
				case ObjectSerializer.TYPE_MAX: return Task.FromResult<object>(uint.MaxValue);
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a nullable UInt32 value.");
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
			uint? Value2 = (uint?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return Task.CompletedTask;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_UINT32, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);

			return Task.CompletedTask;
		}

	}
}

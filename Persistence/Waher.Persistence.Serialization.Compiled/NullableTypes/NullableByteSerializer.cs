using System;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable <see cref="Byte"/> value.
	/// </summary>
	public class NullableByteSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Byte"/> value.
		/// </summary>
		public NullableByteSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(byte?);
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
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>(Reader.ReadBoolean() ? (byte?)1 : (byte?)0);
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>((byte?)Reader.ReadByte());
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>((byte?)Reader.ReadInt16());
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>((byte?)Reader.ReadInt32());
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>((byte?)Reader.ReadInt64());
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>((byte?)Reader.ReadSByte());
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>((byte?)Reader.ReadUInt16());
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>((byte?)Reader.ReadUInt32());
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>((byte?)Reader.ReadUInt64());
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>((byte?)Reader.ReadDecimal());
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>((byte?)Reader.ReadDouble());
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>((byte?)Reader.ReadSingle());
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>((byte?)byte.Parse(Reader.ReadString()));
				case ObjectSerializer.TYPE_MIN: return Task.FromResult<object>(byte.MinValue);
				case ObjectSerializer.TYPE_MAX: return Task.FromResult<object>(byte.MaxValue);
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected a nullable byte value.");
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
			byte? Value2 = (byte?)Value;

			if (WriteTypeCode)
			{
				if (!Value2.HasValue)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return Task.CompletedTask;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_BYTE, 6);
			}
			else if (!Value2.HasValue)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write(Value2.Value);

			return Task.CompletedTask;
		}

	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes an enumerated value value.
	/// </summary>
	public class EnumSerializer : ValueTypeSerializer
	{
		private readonly Type enumType;
		private readonly TypeInfo enumTypeInfo;
		private readonly bool asInt;

		/// <summary>
		/// Serializes an enumerated value.
		/// </summary>
		/// <param name="EnumType">Enumerated type.</param>
		public EnumSerializer(Type EnumType)
		{
			this.enumType = EnumType;
			this.enumTypeInfo = EnumType.GetTypeInfo();
			this.asInt = this.enumTypeInfo.IsDefined(typeof(FlagsAttribute), false);
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return this.enumType;
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
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadBoolean() ? 1 : 0));
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadByte()));
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadInt16()));
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadInt32()));
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadInt64()));
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadSByte()));
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadUInt16()));
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadUInt32()));
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadUInt64()));
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadDecimal()));
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadDouble()));
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>(Enum.ToObject(this.enumType, Reader.ReadSingle()));
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>(Enum.Parse(this.enumType, Reader.ReadString()));
				case ObjectSerializer.TYPE_ENUM: return Task.FromResult<object>(Reader.ReadEnum(this.enumType));
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
				default: throw new Exception("Expected an enum value.");
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
			{
				if (this.asInt)
					Writer.WriteBits(ObjectSerializer.TYPE_INT32, 6);
				else
					Writer.WriteBits(ObjectSerializer.TYPE_ENUM, 6);
			}

			if (this.asInt)
				Writer.Write((int)Value);
			else
				Writer.Write((Enum)Value);

			return Task.CompletedTask;
		}

	}
}

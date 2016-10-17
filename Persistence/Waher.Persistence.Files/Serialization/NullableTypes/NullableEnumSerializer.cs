using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	public class NullableEnumSerializer : IObjectSerializer
	{
		private Type enumType;
		private Type genericType;
		private ConstructorInfo constructor;
		private PropertyInfo valueProperty;

		public NullableEnumSerializer(Type EnumType)
		{
			this.enumType = EnumType;
			this.genericType = typeof(Nullable<>).MakeGenericType(this.enumType);

			this.constructor = this.genericType.GetConstructor(new Type[] { this.enumType });
			if (this.constructor == null)
				throw new ArgumentException("Generic nullable type lacks required constructor.", "EnumType");

			this.valueProperty = this.genericType.GetProperty("Value");
			if (this.valueProperty == null)
				throw new ArgumentException("Generic nullable type lacks required Value property.", "EnumType");
		}

		public Type ValueType
		{
			get
			{
				return this.genericType;
			}
		}

		public bool IsNullable
		{
			get { return true; }
		}

		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadBoolean() ? 1 : 0));
				case ObjectSerializer.TYPE_BYTE: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadByte()));
				case ObjectSerializer.TYPE_INT16: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadInt16()));
				case ObjectSerializer.TYPE_INT32: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadInt32()));
				case ObjectSerializer.TYPE_INT64: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadInt64()));
				case ObjectSerializer.TYPE_SBYTE: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadSByte()));
				case ObjectSerializer.TYPE_UINT16: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadUInt16()));
				case ObjectSerializer.TYPE_UINT32: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadUInt32()));
				case ObjectSerializer.TYPE_UINT64: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadUInt64()));
				case ObjectSerializer.TYPE_DECIMAL: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadDecimal()));
				case ObjectSerializer.TYPE_DOUBLE: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadDouble()));
				case ObjectSerializer.TYPE_SINGLE: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadSingle()));
				case ObjectSerializer.TYPE_STRING: return this.ToNullable(Enum.Parse(this.enumType, Reader.ReadString()));
				case ObjectSerializer.TYPE_ENUM: return this.ToNullable(Reader.ReadEnum(this.enumType));
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an enum value.");
			}
		}

		private object ToNullable(object Value)
		{
			return this.constructor.Invoke(new object[] { Value });
		}

		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (WriteTypeCode)
			{
				if (Value == null)
				{
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					return;
				}
				else
					Writer.WriteBits(ObjectSerializer.TYPE_ENUM, 6);
			}
			else if (Value == null)
				throw new NullReferenceException("Value cannot be null.");

			Writer.Write((Enum)this.valueProperty.GetMethod.Invoke(Value, null));
		}

	}
}

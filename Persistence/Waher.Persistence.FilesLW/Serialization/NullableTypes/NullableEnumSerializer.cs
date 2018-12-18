using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization.NullableTypes
{
	/// <summary>
	/// Serializes a nullable enumerated value.
	/// </summary>
	public class NullableEnumSerializer : NullableValueTypeSerializer
	{
		private readonly Type enumType;
		private readonly Type genericType;
		private readonly ConstructorInfo constructor;
		private readonly PropertyInfo valueProperty;

		/// <summary>
		/// Serializes a nullable enumerated value.
		/// </summary>
		public NullableEnumSerializer(Type EnumType)
		{
			this.enumType = EnumType;
			this.genericType = typeof(Nullable<>).MakeGenericType(this.enumType);
			this.constructor = null;

			foreach (ConstructorInfo CI in this.genericType.GetTypeInfo().DeclaredConstructors)
			{
				ParameterInfo[] P = CI.GetParameters();
				if (P.Length == 1 && P[0].ParameterType == this.enumType)
				{
					this.constructor = CI;
					break;
				}
			}

			if (this.constructor == null)
				throw new ArgumentException("Generic nullable type lacks required constructor.", nameof(EnumType));

			this.valueProperty = this.genericType.GetRuntimeProperty("Value");
			if (this.valueProperty == null)
				throw new ArgumentException("Generic nullable type lacks required Value property.", nameof(EnumType));
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return this.genericType;
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
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return this.ToNullable(Enum.Parse(this.enumType, Reader.ReadString()));
				case ObjectSerializer.TYPE_ENUM: return this.ToNullable(Reader.ReadEnum(this.enumType));
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected an enum value.");
			}
		}

		private object ToNullable(object Value)
		{
			return this.constructor.Invoke(new object[] { Value });
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Waher.Persistence.MongoDB.Serialization.NullableTypes
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

			if (this.constructor is null)
				throw new ArgumentException("Generic nullable type lacks required constructor.", nameof(EnumType));

			this.valueProperty = this.genericType.GetRuntimeProperty("Value");
			if (this.valueProperty is null)
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
		public override object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBsonType();

			switch (DataType.Value)
			{
				case BsonType.Boolean: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadBoolean() ? 1 : 0));
				case BsonType.Decimal128: return this.ToNullable(Enum.ToObject(this.enumType, (int)Reader.ReadDecimal128()));
				case BsonType.Double: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadDouble()));
				case BsonType.Int32: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadInt32()));
				case BsonType.Int64: return this.ToNullable(Enum.ToObject(this.enumType, Reader.ReadInt64()));
				case BsonType.String: return this.ToNullable(Enum.Parse(this.enumType, Reader.ReadString()));
				case BsonType.Null: Reader.ReadNull(); return null;
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
		public override void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
				Writer.WriteNull();
			else
				Writer.WriteString(((Enum)this.valueProperty.GetMethod.Invoke(Value, null)).ToString());
		}

	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Waher.Persistence.MongoDB.Serialization.ValueTypes
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
				case BsonType.Boolean: return Enum.ToObject(this.enumType, Reader.ReadBoolean() ? 1 : 0);
				case BsonType.Decimal128: return Enum.ToObject(this.enumType, (int)Reader.ReadDecimal128());
				case BsonType.Double: return Enum.ToObject(this.enumType, Reader.ReadDouble());
				case BsonType.Int32: return Enum.ToObject(this.enumType, Reader.ReadInt32());
				case BsonType.Int64: return Enum.ToObject(this.enumType, Reader.ReadInt64());
				case BsonType.String: return Enum.Parse(this.enumType, Reader.ReadString());
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected an enum value.");
			}
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
			if (this.asInt)
				Writer.WriteInt32((int)Value);
			else
				Writer.WriteString(Value.ToString());
		}

	}
}

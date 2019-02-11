using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Waher.Persistence.MongoDB.Serialization.NullableTypes
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
		public override object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBsonType();

			switch (DataType.Value)
			{
				case BsonType.Boolean: return Reader.ReadBoolean();
				case BsonType.Decimal128: return Reader.ReadDecimal128() != 0;
				case BsonType.Double: return Reader.ReadDouble() != 0;
				case BsonType.Int32: return Reader.ReadInt32() != 0;
				case BsonType.Int64: return Reader.ReadInt64() != 0;
				case BsonType.MinKey: Reader.ReadMinKey(); return false;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return true;
				case BsonType.Null: Reader.ReadNull(); return null;
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
		public override void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			bool? Value2 = (bool?)Value;

			if (Value2.HasValue)
				Writer.WriteBoolean(Value2.Value);
			else
				Writer.WriteNull();
		}
	}
}

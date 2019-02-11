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
	/// Serializes a nullable <see cref="Decimal"/> value.
	/// </summary>
	public class NullableDecimalSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Decimal"/> value.
		/// </summary>
		public NullableDecimalSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(decimal?);
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
				case BsonType.Boolean: return Reader.ReadBoolean() ? (decimal?)1 : (decimal?)0;
				case BsonType.Decimal128: return (decimal?)Reader.ReadDecimal128();
				case BsonType.Double: return (decimal?)Reader.ReadDouble();
				case BsonType.Int32: return (decimal?)Reader.ReadInt32();
				case BsonType.Int64: return (decimal?)Reader.ReadInt64();
				case BsonType.String: return (decimal?)decimal.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return (decimal?)decimal.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return (decimal?)decimal.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected a nullable Decimal value.");
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
			decimal? Value2 = (decimal?)Value;

			if (Value2.HasValue)
				Writer.WriteDecimal128(Value2.Value);
			else
				Writer.WriteNull();
		}

	}
}

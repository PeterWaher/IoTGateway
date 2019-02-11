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
	/// Serializes a nullable <see cref="Int64"/> value.
	/// </summary>
	public class NullableInt64Serializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Int64"/> value.
		/// </summary>
		public NullableInt64Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(long?);
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
				case BsonType.Boolean: return Reader.ReadBoolean() ? (long?)1 : (long?)0;
				case BsonType.Decimal128: return (long?)Reader.ReadDecimal128();
				case BsonType.Double: return (long?)Reader.ReadDouble();
				case BsonType.Int32: return (long?)Reader.ReadInt32();
				case BsonType.Int64: return (long?)Reader.ReadInt64();
				case BsonType.String: return (long?)long.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return (long?)long.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return (long?)long.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected a nullable long value.");
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
			long? Value2 = (long?)Value;

			if (Value2.HasValue)
				Writer.WriteInt64(Value2.Value);
			else
				Writer.WriteNull();
		}

	}
}

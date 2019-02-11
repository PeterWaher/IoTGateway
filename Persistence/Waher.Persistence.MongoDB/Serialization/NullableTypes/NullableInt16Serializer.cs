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
	/// Serializes a nullable <see cref="Int16"/> value.
	/// </summary>
	public class NullableInt16Serializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="Int16"/> value.
		/// </summary>
		public NullableInt16Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(short?);
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
				case BsonType.Boolean: return Reader.ReadBoolean() ? (short?)1 : (short?)0;
				case BsonType.Decimal128: return (short?)Reader.ReadDecimal128();
				case BsonType.Double: return (short?)Reader.ReadDouble();
				case BsonType.Int32: return (short?)Reader.ReadInt32();
				case BsonType.Int64: return (short?)Reader.ReadInt64();
				case BsonType.String: return (short?)short.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return (short?)short.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return (short?)short.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected a nullable byte value.");
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
			short? Value2 = (short?)Value;

			if (Value2.HasValue)
				Writer.WriteInt32(Value2.Value);
			else
				Writer.WriteNull();
		}

	}
}

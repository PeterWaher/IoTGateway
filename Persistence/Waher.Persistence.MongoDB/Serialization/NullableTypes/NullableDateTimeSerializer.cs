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
	/// Serializes a nullable <see cref="DateTime"/> value.
	/// </summary>
	public class NullableDateTimeSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="DateTime"/> value.
		/// </summary>
		public NullableDateTimeSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(DateTime?);
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
				case BsonType.DateTime: return (DateTime?)ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
				case BsonType.String: return (DateTime?)DateTime.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return (DateTime?)DateTime.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return (DateTime?)DateTime.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected a nullable DateTime value.");
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
			DateTime? Value2 = (DateTime?)Value;

			if (Value2.HasValue)
				Writer.WriteDateTime((long)(Value2.Value - ObjectSerializer.UnixEpoch).TotalMilliseconds);
			else
				Writer.WriteNull();
		}

	}
}

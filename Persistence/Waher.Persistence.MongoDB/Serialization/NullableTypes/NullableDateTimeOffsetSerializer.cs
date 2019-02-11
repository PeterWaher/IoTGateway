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
	/// Serializes a nullable <see cref="DateTimeOffset"/> value.
	/// </summary>
	public class NullableDateTimeOffsetSerializer : NullableValueTypeSerializer
	{
		/// <summary>
		/// Serializes a nullable <see cref="DateTimeOffset"/> value.
		/// </summary>
		public NullableDateTimeOffsetSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(DateTimeOffset?);
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
				case BsonType.DateTime: return (DateTimeOffset?)ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
				case BsonType.String: return (DateTimeOffset?)DateTimeOffset.Parse(Reader.ReadString());
				case BsonType.Document:
					DateTime TP = DateTime.MinValue;
					TimeSpan TZ = TimeSpan.Zero;

					Reader.ReadStartDocument();

					while (Reader.State == BsonReaderState.Type)
					{
						BsonType BsonType = Reader.ReadBsonType();
						if (BsonType == BsonType.EndOfDocument)
							break;

						string FieldName = Reader.ReadName();
						switch (FieldName)
						{
							case "tp":
								TP = ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
								break;

							case "tz":
								TZ = TimeSpan.Parse(Reader.ReadString());
								break;
						}
					}

					Reader.ReadEndDocument();

					return (DateTimeOffset?)new DateTimeOffset(TP, TZ);

				case BsonType.MinKey: Reader.ReadMinKey(); return (DateTimeOffset?)DateTimeOffset.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return (DateTimeOffset?)DateTimeOffset.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected a nullable DateTimeOffset value.");
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
			DateTimeOffset? DTO = (DateTimeOffset?)Value;

			if (DTO.HasValue)
			{
				Writer.WriteStartDocument();
				Writer.WriteName("tp");
				Writer.WriteDateTime((long)(DTO.Value.DateTime - ObjectSerializer.UnixEpoch).TotalMilliseconds);
				Writer.WriteName("tz");
				Writer.WriteString(DTO.Value.Offset.ToString());
				Writer.WriteEndDocument();
			}
			else
				Writer.WriteNull();
		}

	}
}

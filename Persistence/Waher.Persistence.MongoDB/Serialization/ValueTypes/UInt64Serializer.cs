using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Waher.Persistence.MongoDB.Serialization.ValueTypes
{
	/// <summary>
	/// Serializes a <see cref="UInt64"/> value.
	/// </summary>
	public class UInt64Serializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="UInt64"/> value.
		/// </summary>
		public UInt64Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(ulong);
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
				case BsonType.Boolean: return Reader.ReadBoolean() ? (ulong)1 : (ulong)0;
				case BsonType.Decimal128: return (ulong)Reader.ReadDecimal128();
				case BsonType.Double: return (ulong)Reader.ReadDouble();
				case BsonType.Int32: return (ulong)Reader.ReadInt32();
				case BsonType.Int64: return (ulong)Reader.ReadInt64();
				case BsonType.String: return ulong.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return ulong.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return ulong.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected an UInt64 value.");
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
			Writer.WriteDecimal128((ulong)Value);
		}

	}
}

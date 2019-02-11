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
	/// Serializes a <see cref="UInt16"/> value.
	/// </summary>
	public class UInt16Serializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="UInt16"/> value.
		/// </summary>
		public UInt16Serializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(ushort);
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
				case BsonType.Boolean: return Reader.ReadBoolean() ? (ushort)1 : (ushort)0;
				case BsonType.Decimal128: return (ushort)Reader.ReadDecimal128();
				case BsonType.Double: return (ushort)Reader.ReadDouble();
				case BsonType.Int32: return (ushort)Reader.ReadInt32();
				case BsonType.Int64: return (ushort)Reader.ReadInt64();
				case BsonType.String: return ushort.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return ushort.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return ushort.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				default: throw new Exception("Expected an UInt16 value.");
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
			Writer.WriteInt32((ushort)Value);
		}

	}
}

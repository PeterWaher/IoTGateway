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
	/// Serializes a <see cref="Char"/> value.
	/// </summary>
	public class CharSerializer : ValueTypeSerializer
	{
		/// <summary>
		/// Serializes a <see cref="Char"/> value.
		/// </summary>
		public CharSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(char);
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
				case BsonType.Decimal128: return (char)Reader.ReadDecimal128();
				case BsonType.Double: return (char)Reader.ReadDouble();
				case BsonType.Int32: return (char)Reader.ReadInt32();
				case BsonType.Int64: return (char)Reader.ReadInt64();
				case BsonType.MinKey: Reader.ReadMinKey(); return char.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return char.MaxValue;
				case BsonType.Null: Reader.ReadNull(); return null;
				case BsonType.String:
					string s = Reader.ReadString();
					return string.IsNullOrEmpty(s) ? (char)0 : s[0];

				default: throw new Exception("Expected a char value.");
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
			Writer.WriteString(new string((char)Value, 1));
		}

	}
}

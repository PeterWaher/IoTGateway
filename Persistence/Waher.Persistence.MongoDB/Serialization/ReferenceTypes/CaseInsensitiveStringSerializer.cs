using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Waher.Persistence.MongoDB.Serialization.ReferenceTypes
{
	/// <summary>
	/// Serializes a <see cref="CaseInsensitiveString"/> value.
	/// </summary>
	public class CaseInsensitiveStringSerializer : IObjectSerializer
	{
		/// <summary>
		/// Serializes a <see cref="CaseInsensitiveString"/> value.
		/// </summary>
		public CaseInsensitiveStringSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public Type ValueType
		{
			get
			{
				return typeof(CaseInsensitiveString);
			}
		}

		/// <summary>
		/// If the value being serialized, can be null.
		/// </summary>
		public bool IsNullable
		{
			get { return true; }
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBsonType();

			switch (DataType.Value)
			{
				case BsonType.Boolean: return Reader.ReadBoolean().ToString();
				case BsonType.DateTime: return ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime()).ToString();
				case BsonType.Decimal128: return Reader.ReadDecimal128().ToString();
				case BsonType.Double: return Reader.ReadDouble().ToString();
				case BsonType.Int32: return Reader.ReadInt32().ToString();
				case BsonType.Int64: return Reader.ReadInt64().ToString();
				case BsonType.JavaScript: return Reader.ReadJavaScript();
				case BsonType.JavaScriptWithScope: return Reader.ReadJavaScriptWithScope();
				case BsonType.Null: Reader.ReadNull(); return null;
				case BsonType.ObjectId: return Reader.ReadObjectId().ToString();
				case BsonType.String: return Reader.ReadString();
				case BsonType.Symbol: return Reader.ReadSymbol();
				default: throw new Exception("Expected a case-insensitive string value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
				Writer.WriteNull();
			else
				Writer.WriteString(Value.ToString());
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Corresponding field or property value, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			Value = null;
			return false;
		}

		/// <summary>
		/// Gets the type of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="FieldType">Corresponding field or property type, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public virtual bool TryGetFieldType(string FieldName, object Object, out Type FieldType)
		{
			FieldType = null;
			return false;
		}
	}
}

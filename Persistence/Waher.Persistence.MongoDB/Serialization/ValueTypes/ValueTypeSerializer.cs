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
	/// Abstract base class for value type serializers.
	/// </summary>
	public abstract class ValueTypeSerializer : IObjectSerializer
	{
		/// <summary>
		/// Abstract base class for value type serializers.
		/// </summary>
		public ValueTypeSerializer()
		{
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public abstract Type ValueType
		{
			get;
		}

		/// <summary>
		/// If the underlying object is nullable.
		/// </summary>
		public bool IsNullable => false;

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="context">The deserialization context.</param>
		/// <param name="args">The deserialization args.</param>
		/// <returns>A deserialized value.</returns>
		public abstract object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded);

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public abstract void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value);

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="args">The serialization args.</param>
		/// <param name="value">The value.</param>
		public virtual bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			Value = null;
			return false;
		}
	}
}

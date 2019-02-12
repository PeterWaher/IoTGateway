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
	/// Generic serializer of array types.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class ArraySerializer<T> : GeneratedObjectSerializerBase
	{
		private readonly MongoDBProvider provider;

		/// <summary>
		/// Generic serializer of array types.
		/// </summary>
		/// <param name="Provider">Files provider.</param>
		public ArraySerializer(MongoDBProvider Provider)
		{
			this.provider = Provider;
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public override Type ValueType
		{
			get
			{
				return typeof(T[]);
			}
		}

		/// <summary>
		/// If the value being serialized, can be null.
		/// </summary>
		public override bool IsNullable
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
		public override object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBsonType();

			return ReadArray<T>(this.provider, Reader, DataType.Value);
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
			if (Value is null)
			{
				if (!WriteTypeCode)
					throw new NullReferenceException("Value cannot be null.");

				Writer.WriteNull();
			}
			else
			{
				T[] Array = (T[])Value;
				Type LastType = typeof(T);
				IObjectSerializer S = this.provider.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable = S.IsNullable;

				Writer.WriteStartArray();

				foreach (T Item in Array)
				{
					if (Item == null)
					{
						if (Nullable)
							Writer.WriteNull();
						else
							throw new Exception("Elements cannot be null.");
					}
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = this.provider.GetObjectSerializer(ItemType);
							LastType = ItemType;
							Nullable = S.IsNullable;
						}

						S.Serialize(Writer, Nullable, true, Item);
					}
				}

				Writer.WriteEndArray();
			}
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Corresponding field or property value, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public override bool TryGetFieldValue(string FieldName, object Object, out object Value)
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
		public override bool TryGetFieldType(string FieldName, object Object, out Type FieldType)
		{
			FieldType = null;
			return false;
		}

	}
}

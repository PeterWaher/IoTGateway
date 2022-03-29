using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Persistence.Serialization.ReferenceTypes
{
	/// <summary>
	/// Provides a generic object serializer.
	/// </summary>
	public class ObjectExNihiloObjectSerializer : ObjectSerializer
	{
		/// <summary>
		/// Provides a generic object serializer.
		/// </summary>
		/// <param name="Context">Serializer context.</param>
		public ObjectExNihiloObjectSerializer(ISerializerContext Context)
			: base(Context, typeof(Dictionary<string, IElement>))
		{
			this.ArchiveObjects = false;
			this.ArchiveTimeDynamic = false;
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public override Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			return this.Deserialize(Reader, DataType, Embedded, true);
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="CheckFieldNames">If field names are to be extended.</param>
		/// <returns>Deserialized object.</returns>
		public async Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded, bool CheckFieldNames)
		{
			StreamBookmark Bookmark = Reader.GetBookmark();
			uint? DataTypeBak = DataType;
			uint FieldDataType;
			ulong FieldCode;
			Guid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();
			string TypeName;
			string FieldName;
			string CollectionName;

			if (!Embedded)
				Reader.SkipVariableLengthUInt64();  // Content length.

			if (!DataType.HasValue)
			{
				DataType = Reader.ReadBits(6);
				if (DataType.Value == ObjectSerializer.TYPE_NULL)
					return null;
			}

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_OBJECT:
					if (Embedded && Reader.BitOffset > 0 && Reader.ReadBit())
						ObjectId = Reader.ReadGuid();
					break;

				case ObjectSerializer.TYPE_BOOLEAN:
					return Reader.ReadBit();

				case ObjectSerializer.TYPE_BYTE:
					return Reader.ReadByte();

				case ObjectSerializer.TYPE_INT16:
					return Reader.ReadInt16();

				case ObjectSerializer.TYPE_INT32:
					return Reader.ReadInt32();

				case ObjectSerializer.TYPE_INT64:
					return Reader.ReadInt64();

				case ObjectSerializer.TYPE_SBYTE:
					return Reader.ReadSByte();

				case ObjectSerializer.TYPE_UINT16:
					return Reader.ReadUInt16();

				case ObjectSerializer.TYPE_UINT32:
					return Reader.ReadUInt32();

				case ObjectSerializer.TYPE_UINT64:
					return Reader.ReadUInt64();

				case ObjectSerializer.TYPE_DECIMAL:
					return Reader.ReadDecimal();

				case ObjectSerializer.TYPE_DOUBLE:
					return Reader.ReadDouble();

				case ObjectSerializer.TYPE_SINGLE:
					return Reader.ReadSingle();

				case ObjectSerializer.TYPE_DATETIME:
					return Reader.ReadDateTime();

				case ObjectSerializer.TYPE_DATETIMEOFFSET:
					return Reader.ReadDateTimeOffset();

				case ObjectSerializer.TYPE_TIMESPAN:
					return Reader.ReadTimeSpan();

				case ObjectSerializer.TYPE_CHAR:
					return Reader.ReadChar();

				case ObjectSerializer.TYPE_STRING:
					return Reader.ReadString();

				case ObjectSerializer.TYPE_CI_STRING:
					return new CaseInsensitiveString(Reader.ReadString());

				case ObjectSerializer.TYPE_ENUM:
					return Reader.ReadString();

				case ObjectSerializer.TYPE_BYTEARRAY:
					return Reader.ReadByteArray();

				case ObjectSerializer.TYPE_GUID:
					return Reader.ReadGuid();

				case ObjectSerializer.TYPE_NULL:
					return null;

				case ObjectSerializer.TYPE_ARRAY:
					throw new Exception("Arrays must be embedded in objects.");

				default:
					throw new Exception("Object or value expected.");
			}

			bool Normalized = this.NormalizedNames;

			if (Normalized)
			{
				FieldCode = Reader.ReadVariableLengthUInt64();
				TypeName = null;
			}
			else
			{
				FieldCode = 0;
				TypeName = Reader.ReadString();
			}

			if (Embedded)
			{
				if (Normalized)
				{
					ulong CollectionCode = Reader.ReadVariableLengthUInt64();
					CollectionName = await this.Context.GetFieldName(null, CollectionCode);
				}
				else
					CollectionName = Reader.ReadString();
			}
			else
				CollectionName = Reader.CollectionName;

			if (Normalized)
			{
				if (FieldCode == 0)
					TypeName = string.Empty;
				else if (CheckFieldNames)
					TypeName = await this.Context.GetFieldName(CollectionName, FieldCode);
				else
					TypeName = CollectionName + "." + FieldCode.ToString();
			}

			if (!string.IsNullOrEmpty(TypeName))
			{
				Type DesiredType = Types.GetType(TypeName);
				if (DesiredType is null)
					DesiredType = typeof(Dictionary<string, IElement>);

				if (DesiredType != typeof(Dictionary<string, IElement>))
				{
					IObjectSerializer Serializer2 = await this.Context.GetObjectSerializer(DesiredType);
					Reader.SetBookmark(Bookmark);
					return await Serializer2.Deserialize(Reader, DataTypeBak, Embedded);
				}
			}

			Dictionary<string, IElement> Properties = new Dictionary<string, IElement>();

			if (ObjectId != Guid.Empty)
				Properties["ObjectId"] = new ObjectValue(ObjectId);

			if (!string.IsNullOrEmpty(TypeName))
				Properties["TypeName"] = new StringValue(TypeName);

			if (!string.IsNullOrEmpty(CollectionName))
				Properties["CollectionName"] = new StringValue(CollectionName);

			while (true)
			{
				if (Normalized)
				{
					FieldCode = Reader.ReadVariableLengthUInt64();
					if (FieldCode == 0)
						break;

					if (CheckFieldNames)
						FieldName = await this.Context.GetFieldName(CollectionName, FieldCode);
					else
						FieldName = CollectionName + "." + FieldCode.ToString();
				}
				else
				{
					FieldName = Reader.ReadString();
					if (string.IsNullOrEmpty(FieldName))
						break;
				}

				FieldDataType = Reader.ReadBits(6);

				switch (FieldDataType)
				{
					case ObjectSerializer.TYPE_BOOLEAN:
						Properties[FieldName] = new BooleanValue(Reader.ReadBoolean());
						break;

					case ObjectSerializer.TYPE_BYTE:
						Properties[FieldName] = new DoubleNumber(Reader.ReadByte());
						break;

					case ObjectSerializer.TYPE_INT16:
						Properties[FieldName] = new DoubleNumber(Reader.ReadInt16());
						break;

					case ObjectSerializer.TYPE_INT32:
						Properties[FieldName] = new DoubleNumber(Reader.ReadInt32());
						break;

					case ObjectSerializer.TYPE_INT64:
						Properties[FieldName] = new DoubleNumber(Reader.ReadInt64());
						break;

					case ObjectSerializer.TYPE_SBYTE:
						Properties[FieldName] = new DoubleNumber(Reader.ReadSByte());
						break;

					case ObjectSerializer.TYPE_UINT16:
						Properties[FieldName] = new DoubleNumber(Reader.ReadUInt16());
						break;

					case ObjectSerializer.TYPE_UINT32:
						Properties[FieldName] = new DoubleNumber(Reader.ReadUInt32());
						break;

					case ObjectSerializer.TYPE_UINT64:
						Properties[FieldName] = new DoubleNumber(Reader.ReadUInt64());
						break;

					case ObjectSerializer.TYPE_DECIMAL:
						Properties[FieldName] = new DoubleNumber((double)Reader.ReadDecimal());
						break;

					case ObjectSerializer.TYPE_DOUBLE:
						Properties[FieldName] = new DoubleNumber(Reader.ReadDouble());
						break;

					case ObjectSerializer.TYPE_SINGLE:
						Properties[FieldName] = new DoubleNumber(Reader.ReadSingle());
						break;

					case ObjectSerializer.TYPE_DATETIME:
						Properties[FieldName] = new DateTimeValue(Reader.ReadDateTime());
						break;

					case ObjectSerializer.TYPE_DATETIMEOFFSET:
						Properties[FieldName] = new ObjectValue(Reader.ReadDateTimeOffset());
						break;

					case ObjectSerializer.TYPE_TIMESPAN:
						Properties[FieldName] = new ObjectValue(Reader.ReadTimeSpan());
						break;

					case ObjectSerializer.TYPE_CHAR:
						Properties[FieldName] = new StringValue(new string(Reader.ReadChar(), 1));
						break;

					case ObjectSerializer.TYPE_STRING:
					case ObjectSerializer.TYPE_ENUM:
						Properties[FieldName] = new StringValue(Reader.ReadString());
						break;

					case ObjectSerializer.TYPE_CI_STRING:
						Properties[FieldName] = new StringValue(Reader.ReadString(), true);
						break;

					case ObjectSerializer.TYPE_BYTEARRAY:
						Properties[FieldName] = new ObjectValue(Reader.ReadByteArray());
						break;

					case ObjectSerializer.TYPE_GUID:
						Properties[FieldName] = new ObjectValue(Reader.ReadGuid());
						break;

					case ObjectSerializer.TYPE_NULL:
						Properties[FieldName] = ObjectValue.Null;
						break;

					case ObjectSerializer.TYPE_ARRAY:
						Properties[FieldName] = VectorDefinition.Encapsulate(await this.ReadGenericArray(Reader), false, null);
						break;

					case ObjectSerializer.TYPE_OBJECT:
						Properties[FieldName] = Expression.Encapsulate(await this.Deserialize(Reader, FieldDataType, true));
						break;

					default:
						throw new Exception("Unrecognized data type: " + FieldDataType.ToString());
				}
			}

			return Properties;
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public override async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State)
		{
			if (Value is null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else if (Value is Dictionary<string, IElement> TypedValue)
			{
				ISerializer WriterBak = Writer;
				IObjectSerializer Serializer;
				object Obj;

				if (!Embedded)
					Writer = Writer.CreateNew();

				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_OBJECT, 6);

				if (!TypedValue.TryGetValue("ObjectId", out IElement PropertyValue) ||
					!(PropertyValue.AssociatedObjectValue is Guid ObjectId))
				{
					ObjectId = Guid.Empty;
				}

				if (!TypedValue.TryGetValue("CollectionName", out PropertyValue) ||
					!(PropertyValue.AssociatedObjectValue is string CollectionName))
				{
					CollectionName = null;
				}

				if (!TypedValue.TryGetValue("TypeName", out PropertyValue) ||
					!(PropertyValue.AssociatedObjectValue is string TypeName))
				{
					TypeName = null;
				}

				if (Embedded && Writer.BitOffset > 0)
				{
					bool WriteObjectId = !ObjectId.Equals(Guid.Empty);
					Writer.WriteBit(WriteObjectId);
					if (WriteObjectId)
						Writer.Write(ObjectId);
				}

				bool Normalized = this.NormalizedNames;

				if (Normalized)
				{
					if (string.IsNullOrEmpty(TypeName))
						Writer.WriteVariableLengthUInt64(0);
					else
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(CollectionName, TypeName));

					if (Embedded)
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(null, string.IsNullOrEmpty(CollectionName) ? this.Context.DefaultCollectionName : CollectionName));
				}
				else
				{
					Writer.Write(TypeName);

					if (Embedded)
						Writer.Write(string.IsNullOrEmpty(CollectionName) ? this.Context.DefaultCollectionName : CollectionName);
				}

				foreach (KeyValuePair<string, IElement> Property in TypedValue)
				{
					switch (Property.Key)
					{
						case "ObjectId":
						case "CollectionName":
						case "TypeName":
							continue;
					}

					if (Normalized)
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(null, Property.Key));
					else
						Writer.Write(Property.Key);

					Obj = Property.Value.AssociatedObjectValue;

					if (Obj is null)
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					else if (Obj is ICollection<KeyValuePair<string, IElement>>)
						await this.Serialize(Writer, true, true, Obj, State);
					else
					{
						Serializer = await this.Context.GetObjectSerializer(Obj.GetType());
						await Serializer.Serialize(Writer, true, true, Obj, State);
					}
				}

				Writer.WriteVariableLengthUInt64(0);

				if (!Embedded)
				{
					Guid NewObjectId = this.Context.CreateGuid();
					WriterBak.Write(NewObjectId);

					byte[] Bin = Writer.GetSerialization();

					WriterBak.WriteVariableLengthUInt64((ulong)Bin.Length);
					WriterBak.WriteRaw(Bin);
				}
			}
			else
			{
				IObjectSerializer Serializer = await this.Context.GetObjectSerializer(Value?.GetType() ?? typeof(object));
				await Serializer.Serialize(Writer, WriteTypeCode, Embedded, Value, State);
			}
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public override async Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			if (Object is Dictionary<string, IElement> GenObj)
			{
				if (GenObj.TryGetValue(FieldName, out IElement Value))
					return Value.AssociatedObjectValue;
				else
					return null;
			}
			else if (!(Object is null))
			{
				IObjectSerializer Serializer2 = await this.Context.GetObjectSerializer(Object.GetType());
				return await Serializer2.TryGetFieldValue(FieldName, Object);
			}
			else
				return null;
		}

		/// <summary>
		/// Mamber name of the field or property holding the Object ID, if any. If there are no such member, this property returns null.
		/// </summary>
		public override string ObjectIdMemberName => "ObjectId";

		/// <summary>
		/// If the class has an Object ID field.
		/// </summary>
		public override bool HasObjectIdField => true;

		/// <summary>
		/// If the class has an Object ID.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		public override async Task<bool> HasObjectId(object Value)
		{
			if (Value is Dictionary<string, IElement> Obj)
			{
				return Obj.TryGetValue("ObjectId", out IElement Id) &&
					Id.AssociatedObjectValue is Guid Guid &&
					!Guid.Equals(Guid.Empty);
			}
			else if (!(Value is null))
			{
				if (await this.Context.GetObjectSerializer(Value.GetType()) is ObjectSerializer Serializer2)
					return await Serializer2.HasObjectId(Value);
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Tries to set the object id of an object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>If the object has an Object ID field or property that could be set.</returns>
		public override async Task<bool> TrySetObjectId(object Value, Guid ObjectId)
		{
			if (Value is Dictionary<string, IElement> Obj)
			{
				Obj["ObjectId"] = new ObjectValue(ObjectId);
				return true;
			}
			else if (!(Value is null))
			{
				if (await this.Context.GetObjectSerializer(Value.GetType()) is ObjectSerializer Serializer2)
					return await Serializer2.TrySetObjectId(Value, ObjectId);
				else
					return false;
			}
			else
				return false;
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public override async Task<Guid> GetObjectId(object Value, bool InsertIfNotFound, object State)
		{
			if (Value is Dictionary<string, IElement> Obj)
			{
				if (Obj.TryGetValue("ObjectId", out IElement Id) &&
					Id.AssociatedObjectValue is Guid Guid &&
					!Guid.Equals(Guid.Empty))
				{
					return Guid;
				}

				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Guid = await this.Context.SaveNewObject(Obj, State);

				Obj["ObjectId"] = new ObjectValue(Guid);

				return Guid;
			}
			else if (!(Value is null))
			{
				if (!(await this.Context.GetObjectSerializer(Value.GetType()) is ObjectSerializer Serializer2))
					throw new Exception("Unable to set Object ID");

				return await Serializer2.GetObjectId(Value, InsertIfNotFound, State);
			}
			else
				throw new NotSupportedException("Objects of type " + Value.GetType().FullName + " not supported.");
		}

		/// <summary>
		/// Checks if a given field value corresponds to the default value for the corresponding field.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Value">Field value.</param>
		/// <returns>If the field value corresponds to the default value of the corresponding field.</returns>
		public override bool IsDefaultValue(string FieldName, object Value)
		{
			return false;
		}

		/// <summary>
		/// Name of collection objects of this type is to be stored in, if available. If not available, this property returns null.
		/// </summary>
		/// <param name="Object">Object in the current context. If null, the default collection name is requested.</param>
		public override async Task<string> CollectionName(object Object)
		{
			if (Object is Dictionary<string, IElement> Obj)
			{
				if (Obj.TryGetValue("CollectionName", out IElement Result) &&
					Result.AssociatedObjectValue is string CollectionName)
				{
					return CollectionName;
				}
			}
			else if (!(Object is null))
			{
				if (await this.Context.GetObjectSerializer(Object.GetType()) is ObjectSerializer Serializer2)
					return await Serializer2.CollectionName(Object);
			}
				
			return await base.CollectionName(Object);
		}
	}
}

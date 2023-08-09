using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Provides a generic object serializer.
	/// </summary>
	public class GenericObjectSerializer : ObjectSerializer
	{
		private readonly bool returnTypedObjects;

		/// <summary>
		/// Provides a generic object serializer.
		/// </summary>
		/// <param name="Context">Serializer context.</param>
		public GenericObjectSerializer(ISerializerContext Context)
			: this(Context, false)
		{
		}

		/// <summary>
		/// Provides a generic object serializer.
		/// </summary>
		/// <param name="Context">Serializer context.</param>
		/// <param name="ReturnTypedObjects">If typed objects should be returned.</param>
		public GenericObjectSerializer(ISerializerContext Context, bool ReturnTypedObjects)
			: base(Context, typeof(GenericObject))
		{
			this.returnTypedObjects = ReturnTypedObjects;

			this.ArchiveObjects = true;
			this.ArchiveTimeDynamic = true;
			this.Prepared = true;
		}

		/// <summary>
		/// Number of days to archive objects of this type. If equal to <see cref="int.MaxValue"/>, no limit is defined.
		/// </summary>
		public override int GetArchivingTimeDays(object Object)
		{
			if (Object is GenericObject GenObj)
				return GenObj.ArchivingTime;
			else
				return base.GetArchivingTimeDays(Object);
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
				Reader.SkipVariableLengthInteger();  // Content length.

			if (!DataType.HasValue)
			{
				DataType = Reader.ReadBits(6);
				if (DataType.Value == TYPE_NULL)
					return null;
			}

			switch (DataType.Value)
			{
				case TYPE_OBJECT:
					if (Embedded && Reader.BitOffset > 0 && Reader.ReadBit())
						ObjectId = Reader.ReadGuid();
					break;

				case TYPE_BOOLEAN:
					return Reader.ReadBit();

				case TYPE_BYTE:
					return Reader.ReadByte();

				case TYPE_INT16:
					return Reader.ReadInt16();

				case TYPE_INT32:
					return Reader.ReadInt32();

				case TYPE_INT64:
					return Reader.ReadInt64();

				case TYPE_SBYTE:
					return Reader.ReadSByte();

				case TYPE_UINT16:
					return Reader.ReadUInt16();

				case TYPE_UINT32:
					return Reader.ReadUInt32();

				case TYPE_UINT64:
					return Reader.ReadUInt64();

				case TYPE_VARINT16:
					return Reader.ReadVariableLengthInt16();

				case TYPE_VARINT32:
					return Reader.ReadVariableLengthInt32();

				case TYPE_VARINT64:
					return Reader.ReadVariableLengthInt64();

				case TYPE_VARUINT16:
					return Reader.ReadVariableLengthUInt16();

				case TYPE_VARUINT32:
					return Reader.ReadVariableLengthUInt32();

				case TYPE_VARUINT64:
					return Reader.ReadVariableLengthUInt64();

				case TYPE_DECIMAL:
					return Reader.ReadDecimal();

				case TYPE_DOUBLE:
					return Reader.ReadDouble();

				case TYPE_SINGLE:
					return Reader.ReadSingle();

				case TYPE_DATETIME:
					return Reader.ReadDateTime();

				case TYPE_DATETIMEOFFSET:
					return Reader.ReadDateTimeOffset();

				case TYPE_TIMESPAN:
					return Reader.ReadTimeSpan();

				case TYPE_CHAR:
					return Reader.ReadChar();

				case TYPE_STRING:
					return Reader.ReadString();

				case TYPE_CI_STRING:
					return Reader.ReadCaseInsensitiveString();

				case TYPE_ENUM:
					return Reader.ReadString();

				case TYPE_BYTEARRAY:
					return Reader.ReadByteArray();

				case TYPE_GUID:
					return Reader.ReadGuid();

				case TYPE_NULL:
					return null;

				case TYPE_ARRAY:
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

			if (this.returnTypedObjects && !string.IsNullOrEmpty(TypeName))
			{
				Type DesiredType = Types.GetType(TypeName) ?? typeof(GenericObject);

				if (DesiredType != typeof(GenericObject))
				{
					IObjectSerializer Serializer2 = await this.Context.GetObjectSerializer(DesiredType);
					Reader.SetBookmark(Bookmark);
					return await Serializer2.Deserialize(Reader, DataTypeBak, Embedded);
				}
			}

			LinkedList<KeyValuePair<string, object>> Properties = new LinkedList<KeyValuePair<string, object>>();
			int? ArchivingTime = null;

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

				if (FieldDataType == TYPE_INT32 && FieldName == "ArchivingTime" && !ArchivingTime.HasValue)
					ArchivingTime = Reader.ReadInt32();
				else
				{
					switch (FieldDataType)
					{
						case TYPE_BOOLEAN:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadBoolean()));
							break;

						case TYPE_BYTE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadByte()));
							break;

						case TYPE_INT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt16()));
							break;

						case TYPE_INT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt32()));
							break;

						case TYPE_INT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt64()));
							break;

						case TYPE_SBYTE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadSByte()));
							break;

						case TYPE_UINT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt16()));
							break;

						case TYPE_UINT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt32()));
							break;

						case TYPE_UINT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt64()));
							break;

						case TYPE_VARINT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthInt16()));
							break;

						case TYPE_VARINT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthInt32()));
							break;

						case TYPE_VARINT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthInt64()));
							break;

						case TYPE_VARUINT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthUInt16()));
							break;

						case TYPE_VARUINT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthUInt32()));
							break;

						case TYPE_VARUINT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadVariableLengthUInt64()));
							break;

						case TYPE_DECIMAL:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDecimal()));
							break;

						case TYPE_DOUBLE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDouble()));
							break;

						case TYPE_SINGLE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadSingle()));
							break;

						case TYPE_DATETIME:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDateTime()));
							break;

						case TYPE_DATETIMEOFFSET:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDateTimeOffset()));
							break;

						case TYPE_TIMESPAN:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadTimeSpan()));
							break;

						case TYPE_CHAR:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadChar()));
							break;

						case TYPE_STRING:
						case TYPE_ENUM:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadString()));
							break;

						case TYPE_CI_STRING:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadCaseInsensitiveString()));
							break;

						case TYPE_BYTEARRAY:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadByteArray()));
							break;

						case TYPE_GUID:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadGuid()));
							break;

						case TYPE_NULL:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, null));
							break;

						case TYPE_ARRAY:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, await this.ReadGenericArray(Reader)));
							break;

						case TYPE_OBJECT:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, await this.Deserialize(Reader, FieldDataType, true)));
							break;

						default:
							throw new Exception("Unrecognized data type: " + FieldDataType.ToString());
					}
				}
			}

			GenericObject Result = new GenericObject(CollectionName, TypeName, ObjectId, Properties);

			if (ArchivingTime.HasValue)
				Result.ArchivingTime = ArchivingTime.Value;

			return Result;
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
					Writer.WriteBits(TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else if (Value is GenericObject TypedValue)
			{
				ISerializer WriterBak = Writer;
				IObjectSerializer Serializer;
				object Obj;

				if (!Embedded)
					Writer = Writer.CreateNew();

				if (WriteTypeCode)
					Writer.WriteBits(TYPE_OBJECT, 6);

				if (Embedded && Writer.BitOffset > 0)
				{
					bool WriteObjectId = !TypedValue.ObjectId.Equals(Guid.Empty);
					Writer.WriteBit(WriteObjectId);
					if (WriteObjectId)
						Writer.Write(TypedValue.ObjectId);
				}

				bool Normalized = this.NormalizedNames;

				if (Normalized)
				{
					if (string.IsNullOrEmpty(TypedValue.TypeName))
						Writer.WriteVariableLengthUInt64(0);
					else
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(TypedValue.CollectionName, TypedValue.TypeName));

					if (Embedded)
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(null, string.IsNullOrEmpty(TypedValue.CollectionName) ? this.Context.DefaultCollectionName : TypedValue.CollectionName));
				}
				else
				{
					Writer.Write(TypedValue.TypeName);

					if (Embedded)
						Writer.Write(string.IsNullOrEmpty(TypedValue.CollectionName) ? this.Context.DefaultCollectionName : TypedValue.CollectionName);
				}

				if (TypedValue.ArchivingTime != 0)
				{
					if (Normalized)
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(TypedValue.CollectionName, "ArchivingTime"));
					else
						Writer.Write("ArchivingTime");

					Writer.WriteBits(TYPE_INT32, 6);
					Writer.Write((int)TypedValue.ArchivingTime);
				}

				foreach (KeyValuePair<string, object> Property in TypedValue)
				{
					if (Normalized)
						Writer.WriteVariableLengthUInt64(await this.Context.GetFieldCode(TypedValue.CollectionName, Property.Key));
					else
						Writer.Write(Property.Key);

					Obj = Property.Value;
					if (Obj is null)
						Writer.WriteBits(TYPE_NULL, 6);
					else
					{
						if (Obj is ICollection<KeyValuePair<string, IElement>> || Obj is ICollection<KeyValuePair<string, object>>)
							await this.Serialize(Writer, true, true, Obj, State);
						else
						{
							Serializer = await this.Context.GetObjectSerializer(Obj.GetType());
							await Serializer.Serialize(Writer, true, true, Obj, State);
						}
					}
				}

				Writer.WriteVariableLengthUInt64(0);

				if (!Embedded)
				{
					if (!TypedValue.ObjectId.Equals(Guid.Empty))
						WriterBak.Write(TypedValue.ObjectId);
					else
					{
						Guid NewObjectId = this.Context.CreateGuid();
						WriterBak.Write(NewObjectId);
						TypedValue.ObjectId = NewObjectId;
					}

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
			if (Object is GenericObject GenObj)
			{
				if (GenObj.TryGetFieldValue(FieldName, out object Value))
					return Value;
				else
					return null;
			}
			else if (!(Object is null) && this.returnTypedObjects)
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
			if (Value is GenericObject Obj)
				return !Obj.ObjectId.Equals(Guid.Empty);
			else if (!(Value is null) && this.returnTypedObjects)
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
			if (Value is GenericObject Obj)
			{
				Obj.ObjectId = ObjectId;
				return true;
			}
			else if (!(Value is null) && this.returnTypedObjects)
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
			if (Value is GenericObject Obj)
			{
				if (!Obj.ObjectId.Equals(Guid.Empty))
					return Obj.ObjectId;

				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Guid ObjectId = await this.Context.SaveNewObject(Obj, State);

				Obj.ObjectId = ObjectId;

				return ObjectId;
			}
			else if (!(Value is null) && this.returnTypedObjects)
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
			if (Object is GenericObject Obj)
				return Obj.CollectionName;
			else if (!(Object is null) && this.returnTypedObjects)
			{
				if (await this.Context.GetObjectSerializer(Object.GetType()) is ObjectSerializer Serializer2)
					return await Serializer2.CollectionName(Object);
				else
					return await base.CollectionName(Object);
			}
			else
				return await base.CollectionName(Object);
		}
	}
}

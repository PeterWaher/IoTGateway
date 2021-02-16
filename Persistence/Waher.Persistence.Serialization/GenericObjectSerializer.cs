using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

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

			if (this.returnTypedObjects && !string.IsNullOrEmpty(TypeName))
			{
				Type DesiredType = Types.GetType(TypeName);
				if (DesiredType is null)
					DesiredType = typeof(GenericObject);

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

				if (FieldDataType == ObjectSerializer.TYPE_INT32 && FieldName == "ArchivingTime" && !ArchivingTime.HasValue)
					ArchivingTime = Reader.ReadInt32();
				else
				{
					switch (FieldDataType)
					{
						case ObjectSerializer.TYPE_BOOLEAN:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadBoolean()));
							break;

						case ObjectSerializer.TYPE_BYTE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadByte()));
							break;

						case ObjectSerializer.TYPE_INT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt16()));
							break;

						case ObjectSerializer.TYPE_INT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt32()));
							break;

						case ObjectSerializer.TYPE_INT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadInt64()));
							break;

						case ObjectSerializer.TYPE_SBYTE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadSByte()));
							break;

						case ObjectSerializer.TYPE_UINT16:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt16()));
							break;

						case ObjectSerializer.TYPE_UINT32:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt32()));
							break;

						case ObjectSerializer.TYPE_UINT64:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadUInt64()));
							break;

						case ObjectSerializer.TYPE_DECIMAL:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDecimal()));
							break;

						case ObjectSerializer.TYPE_DOUBLE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDouble()));
							break;

						case ObjectSerializer.TYPE_SINGLE:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadSingle()));
							break;

						case ObjectSerializer.TYPE_DATETIME:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDateTime()));
							break;

						case ObjectSerializer.TYPE_DATETIMEOFFSET:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadDateTimeOffset()));
							break;

						case ObjectSerializer.TYPE_TIMESPAN:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadTimeSpan()));
							break;

						case ObjectSerializer.TYPE_CHAR:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadChar()));
							break;

						case ObjectSerializer.TYPE_STRING:
						case ObjectSerializer.TYPE_ENUM:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadString()));
							break;

						case ObjectSerializer.TYPE_CI_STRING:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, new CaseInsensitiveString(Reader.ReadString())));
							break;

						case ObjectSerializer.TYPE_BYTEARRAY:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadByteArray()));
							break;

						case ObjectSerializer.TYPE_GUID:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, Reader.ReadGuid()));
							break;

						case ObjectSerializer.TYPE_NULL:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, null));
							break;

						case ObjectSerializer.TYPE_ARRAY:
							Properties.AddLast(new KeyValuePair<string, object>(FieldName, await this.ReadGenericArray(Reader)));
							break;

						case ObjectSerializer.TYPE_OBJECT:
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

		private async Task<Array> ReadGenericArray(IDeserializer Reader)
		{
			ulong NrElements = Reader.ReadVariableLengthUInt64();
			uint ElementDataType = Reader.ReadBits(6);

			switch (ElementDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return await this.ReadArray<bool>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_BYTE: return await this.ReadArray<byte>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT16: return await this.ReadArray<short>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT32: return await this.ReadArray<int>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT64: return await this.ReadArray<long>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_SBYTE: return await this.ReadArray<sbyte>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT16: return await this.ReadArray<ushort>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT32: return await this.ReadArray<uint>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT64: return await this.ReadArray<ulong>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DECIMAL: return await this.ReadArray<decimal>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DOUBLE: return await this.ReadArray<double>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_SINGLE: return await this.ReadArray<float>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DATETIME: return await this.ReadArray<DateTime>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return await this.ReadArray<DateTimeOffset>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_TIMESPAN: return await this.ReadArray<TimeSpan>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_CHAR: return await this.ReadArray<char>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_ENUM: return await this.ReadArray<string>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_CI_STRING: return await this.ReadArray<CaseInsensitiveString>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_BYTEARRAY: return await this.ReadArray<byte[]>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_GUID: return await this.ReadArray<Guid>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_ARRAY: return await this.ReadArrayOfArrays(Reader, NrElements);
				case ObjectSerializer.TYPE_OBJECT: return await this.ReadArrayOfObjects(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_NULL: return await this.ReadArrayOfNullableElements(Reader, NrElements);
				default: throw new Exception("Unrecognized data type: " + ElementDataType.ToString());
			}
		}

		private async Task<T[]> ReadArray<T>(IDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<T> Elements = new List<T>();
			IObjectSerializer S = await this.Context.GetObjectSerializer(typeof(T));

			while (NrElements > 0)
			{
				if (await S.Deserialize(Reader, ElementDataType, true) is T Item)
				{
					Elements.Add(Item);
					NrElements--;
				}
			}

			return Elements.ToArray();
		}

		private async Task<Array[]> ReadArrayOfArrays(IDeserializer Reader, ulong NrElements)
		{
			List<Array> Elements = new List<Array>();

			while (NrElements-- > 0)
				Elements.Add(await this.ReadGenericArray(Reader));

			return Elements.ToArray();
		}

		private async Task<GenericObject[]> ReadArrayOfObjects(IDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<GenericObject> Elements = new List<GenericObject>();

			while (NrElements-- > 0)
				Elements.Add((GenericObject)await this.Deserialize(Reader, ElementDataType, true));

			return Elements.ToArray();
		}

		private async Task<object[]> ReadArrayOfNullableElements(IDeserializer Reader, ulong NrElements)
		{
			List<object> Elements = new List<object>();
			uint ElementDataType;

			while (NrElements-- > 0)
			{
				ElementDataType = Reader.ReadBits(6);

				switch (ElementDataType)
				{
					case ObjectSerializer.TYPE_BOOLEAN:
						Elements.Add(Reader.ReadBoolean());
						break;

					case ObjectSerializer.TYPE_BYTE:
						Elements.Add(Reader.ReadByte());
						break;

					case ObjectSerializer.TYPE_INT16:
						Elements.Add(Reader.ReadInt16());
						break;

					case ObjectSerializer.TYPE_INT32:
						Elements.Add(Reader.ReadInt32());
						break;

					case ObjectSerializer.TYPE_INT64:
						Elements.Add(Reader.ReadInt64());
						break;

					case ObjectSerializer.TYPE_SBYTE:
						Elements.Add(Reader.ReadSByte());
						break;

					case ObjectSerializer.TYPE_UINT16:
						Elements.Add(Reader.ReadUInt16());
						break;

					case ObjectSerializer.TYPE_UINT32:
						Elements.Add(Reader.ReadUInt32());
						break;

					case ObjectSerializer.TYPE_UINT64:
						Elements.Add(Reader.ReadUInt64());
						break;

					case ObjectSerializer.TYPE_DECIMAL:
						Elements.Add(Reader.ReadDecimal());
						break;

					case ObjectSerializer.TYPE_DOUBLE:
						Elements.Add(Reader.ReadDouble());
						break;

					case ObjectSerializer.TYPE_SINGLE:
						Elements.Add(Reader.ReadSingle());
						break;

					case ObjectSerializer.TYPE_DATETIME:
						Elements.Add(Reader.ReadDateTime());
						break;

					case ObjectSerializer.TYPE_DATETIMEOFFSET:
						Elements.Add(Reader.ReadDateTimeOffset());
						break;

					case ObjectSerializer.TYPE_TIMESPAN:
						Elements.Add(Reader.ReadTimeSpan());
						break;

					case ObjectSerializer.TYPE_CHAR:
						Elements.Add(Reader.ReadChar());
						break;

					case ObjectSerializer.TYPE_STRING:
					case ObjectSerializer.TYPE_ENUM:
						Elements.Add(Reader.ReadString());
						break;

					case ObjectSerializer.TYPE_CI_STRING:
						Elements.Add(new CaseInsensitiveString(Reader.ReadString()));
						break;

					case ObjectSerializer.TYPE_BYTEARRAY:
						Elements.Add(Reader.ReadByteArray());
						break;

					case ObjectSerializer.TYPE_GUID:
						Elements.Add(Reader.ReadGuid());
						break;

					case ObjectSerializer.TYPE_ARRAY:
						Elements.Add(await this.ReadGenericArray(Reader));
						break;

					case ObjectSerializer.TYPE_OBJECT:
						Elements.Add(await this.Deserialize(Reader, ElementDataType, true));
						break;

					case ObjectSerializer.TYPE_NULL:
						Elements.Add(null);
						break;

					default:
						throw new Exception("Unrecognized data type: " + ElementDataType.ToString());
				}
			}

			return Elements.ToArray();
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public override async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
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
				{
					if (TypedValue is null)
					{
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
						return;
					}
					else
						Writer.WriteBits(ObjectSerializer.TYPE_OBJECT, 6);
				}
				else if (TypedValue is null)
					throw new NullReferenceException("Value cannot be null.");

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

					Writer.WriteBits(ObjectSerializer.TYPE_INT32, 6);
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
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					else
					{
						if (Obj is GenericObject)
							await this.Serialize(Writer, true, true, Obj);
						else
						{
							Serializer = await this.Context.GetObjectSerializer(Obj.GetType());
							await Serializer.Serialize(Writer, true, true, Obj);
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
				await Serializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
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
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public override async Task<Guid> GetObjectId(object Value, bool InsertIfNotFound)
		{
			if (Value is GenericObject Obj)
			{
				if (!Obj.ObjectId.Equals(Guid.Empty))
					return Obj.ObjectId;

				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Guid ObjectId = await this.Context.SaveNewObject(Obj);

				Obj.ObjectId = ObjectId;

				return ObjectId;
			}
			else if (!(Value is null) && this.returnTypedObjects)
			{
				if (!(await this.Context.GetObjectSerializer(Value.GetType()) is ObjectSerializer Serializer2))
					throw new Exception("Unable to set Object ID");

				return await Serializer2.GetObjectId(Value, InsertIfNotFound);
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

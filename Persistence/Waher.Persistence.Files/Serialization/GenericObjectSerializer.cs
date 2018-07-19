using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Provides a generic object serializer.
	/// </summary>
	public class GenericObjectSerializer : IObjectSerializer
	{
		private readonly FilesProvider provider;

		/// <summary>
		/// Provides a generic object serializer.
		/// </summary>
		public GenericObjectSerializer(FilesProvider Provider)
		{
			this.provider = Provider;
		}

		/// <summary>
		/// If the underlying object is nullable.
		/// </summary>
		public bool IsNullable
		{
			get { return true; }
		}

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public Type ValueType
		{
			get { return typeof(GenericObject); }
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			return this.Deserialize(Reader, DataType, Embedded, true);
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="CheckFieldNames">If field names are to be extended.</param>
		/// <returns>Deserialized object.</returns>
		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded, bool CheckFieldNames)
		{
			uint FieldDataType;
			ulong FieldCode;
			ulong CollectionCode;
			StreamBookmark Bookmark = Reader.GetBookmark();
			uint? DataTypeBak = DataType;
			Guid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();
			ulong ContentLen = Embedded ? 0 : Reader.ReadVariableLengthUInt64();
			string TypeName;
			string FieldName;
			string CollectionName;

			if (!DataType.HasValue)
			{
				DataType = Reader.ReadBits(6);
				if (DataType.Value == ObjectSerializer.TYPE_NULL)
					return null;
			}

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_OBJECT:
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

				case ObjectSerializer.TYPE_ENUM:
					return Reader.ReadString();

				case ObjectSerializer.TYPE_BYTEARRAY:
					return Reader.ReadByteArray();

				case ObjectSerializer.TYPE_GUID:
					return Reader.ReadGuid();

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Object or value expected.");
			}

			FieldCode = Reader.ReadVariableLengthUInt64();

			if (Embedded)
			{
				CollectionCode = Reader.ReadVariableLengthUInt64();
				CollectionName = this.provider.GetFieldName(null, CollectionCode);
			}
			else
				CollectionName = Reader.CollectionName;

			if (FieldCode == 0)
				TypeName = string.Empty;
			else if (CheckFieldNames)
				TypeName = this.provider.GetFieldName(CollectionName, FieldCode);
			else
				TypeName = CollectionName + "." + FieldCode.ToString();

			LinkedList<KeyValuePair<string, object>> Properties = new LinkedList<KeyValuePair<string, object>>();

			while ((FieldCode = Reader.ReadVariableLengthUInt64()) != 0)
			{
				if (CheckFieldNames)
					FieldName = this.provider.GetFieldName(CollectionName, FieldCode);
				else
					FieldName = CollectionName + "." + FieldCode.ToString();

				FieldDataType = Reader.ReadBits(6);

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
						Properties.AddLast(new KeyValuePair<string, object>(FieldName, this.ReadGenericArray(Reader)));
						break;

					case ObjectSerializer.TYPE_OBJECT:
						Properties.AddLast(new KeyValuePair<string, object>(FieldName, this.Deserialize(Reader, FieldDataType, true)));
						break;

					default:
						throw new Exception("Unrecognized data type: " + FieldDataType.ToString());
				}
			}

			return new GenericObject(CollectionName, TypeName, ObjectId, Properties);
		}

		private Array ReadGenericArray(BinaryDeserializer Reader)
		{
			ulong NrElements = Reader.ReadVariableLengthUInt64();
			uint ElementDataType = Reader.ReadBits(6);

			switch (ElementDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return this.ReadArray<bool>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_BYTE: return this.ReadArray<byte>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT16: return this.ReadArray<short>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT32: return this.ReadArray<int>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_INT64: return this.ReadArray<long>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_SBYTE: return this.ReadArray<sbyte>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT16: return this.ReadArray<ushort>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT32: return this.ReadArray<uint>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_UINT64: return this.ReadArray<ulong>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DECIMAL: return this.ReadArray<decimal>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DOUBLE: return this.ReadArray<double>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_SINGLE: return this.ReadArray<float>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DATETIME: return this.ReadArray<DateTime>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return this.ReadArray<DateTimeOffset>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_TIMESPAN: return this.ReadArray<TimeSpan>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_CHAR: return this.ReadArray<char>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_ENUM: return this.ReadArray<string>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_BYTEARRAY: return this.ReadArray<byte[]>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_GUID: return this.ReadArray<Guid>(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_ARRAY: return this.ReadArrayOfArrays(Reader, NrElements);
				case ObjectSerializer.TYPE_OBJECT: return this.ReadArrayOfObjects(Reader, NrElements, ElementDataType);
				case ObjectSerializer.TYPE_NULL: return this.ReadArrayOfNullableElements(Reader, NrElements);
				default: throw new Exception("Unrecognized data type: " + ElementDataType.ToString());
			}
		}

		private T[] ReadArray<T>(BinaryDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<T> Elements = new List<T>();
			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(T));

			while (NrElements > 0)
			{
				if (S.Deserialize(Reader, ElementDataType, true) is T Item)
				{
					Elements.Add(Item);
					NrElements--;
				}
			}

			return Elements.ToArray();
		}

		private Array[] ReadArrayOfArrays(BinaryDeserializer Reader, ulong NrElements)
		{
			List<Array> Elements = new List<Array>();

			while (NrElements-- > 0)
				Elements.Add(this.ReadGenericArray(Reader));

			return Elements.ToArray();
		}

		private GenericObject[] ReadArrayOfObjects(BinaryDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<GenericObject> Elements = new List<GenericObject>();

			while (NrElements-- > 0)
				Elements.Add((GenericObject)this.Deserialize(Reader, ElementDataType, true));

			return Elements.ToArray();
		}

		private object[] ReadArrayOfNullableElements(BinaryDeserializer Reader, ulong NrElements)
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

					case ObjectSerializer.TYPE_BYTEARRAY:
						Elements.Add(Reader.ReadByteArray());
						break;

					case ObjectSerializer.TYPE_GUID:
						Elements.Add(Reader.ReadGuid());
						break;

					case ObjectSerializer.TYPE_ARRAY:
						Elements.Add(this.ReadGenericArray(Reader));
						break;

					case ObjectSerializer.TYPE_OBJECT:
						Elements.Add(this.Deserialize(Reader, ElementDataType, true));
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
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is GenericObject TypedValue)
			{
				BinarySerializer WriterBak = Writer;
				IObjectSerializer Serializer;
				object Obj;

				if (!Embedded)
					Writer = new BinarySerializer(Writer.CollectionName, Writer.Encoding, true);

				if (WriteTypeCode)
				{
					if (TypedValue == null)
					{
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
						return;
					}
					else
						Writer.WriteBits(ObjectSerializer.TYPE_OBJECT, 6);
				}
				else if (TypedValue == null)
					throw new NullReferenceException("Value cannot be null.");

				if (string.IsNullOrEmpty(TypedValue.TypeName))
					Writer.WriteVariableLengthUInt64(0);
				else
					Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(TypedValue.CollectionName, TypedValue.TypeName));

				if (Embedded)
					Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(null, string.IsNullOrEmpty(TypedValue.CollectionName) ? this.provider.DefaultCollectionName : TypedValue.CollectionName));

				foreach (KeyValuePair<string, object> Property in TypedValue)
				{
					Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(TypedValue.CollectionName, Property.Key));

					Obj = Property.Value;
					if (Obj == null)
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					else
					{
						if (Obj is GenericObject)
							this.Serialize(Writer, true, true, Obj);
						else
						{
							Serializer = this.provider.GetObjectSerializer(Obj.GetType());
							Serializer.Serialize(Writer, true, true, Obj);
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
						Guid NewObjectId = ObjectBTreeFile.CreateDatabaseGUID();
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
				IObjectSerializer Serializer = this.provider.GetObjectSerializer(Value.GetType());
				Serializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
			}
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
			GenericObject Obj = (GenericObject)Object;
			return Obj.TryGetFieldValue(FieldName, out Value);
		}
	}
}

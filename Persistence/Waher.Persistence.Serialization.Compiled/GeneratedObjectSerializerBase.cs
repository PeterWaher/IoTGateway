﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Serialization.ReferenceTypes;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Abstract base class for generated object serializers.
	/// </summary>
	public abstract class GeneratedObjectSerializerBase : IObjectSerializer
	{
		/// <summary>
		/// First value that takes 2 bytes (or more). Values below this limit
		/// is encoded in a shorter manner using variable-length encoding compared
		/// to a fixed-length encoding.
		/// </summary>
		public const ushort UInt16VarSizeLimit = 1 << 7;

		/// <summary>
		/// First value that takes 4 bytes (or more). Values below this limit
		/// is encoded in a shorter manner using variable-length encoding compared
		/// to a fixed-length encoding.
		/// </summary>
		public const uint UInt32VarSizeLimit = 1 << (3 * 7);

		/// <summary>
		/// First value that takes 8 bytes (or more). Values below this limit
		/// is encoded in a shorter manner using variable-length encoding compared
		/// to a fixed-length encoding.
		/// </summary>
		public const ulong UInt64VarSizeLimit = 1UL << (7 * 7);

		/// <summary>
		/// First positive value that takes 8 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const long Int64VarSizeMaxLimit = (long)(UInt64VarSizeLimit >> 1);

		/// <summary>
		/// First negative value that takes 8 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const long Int64VarSizeMinLimit = -Int64VarSizeMaxLimit - 1;

		/// <summary>
		/// First positive value that takes 4 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const int Int32VarSizeMaxLimit = (int)(UInt32VarSizeLimit >> 1);

		/// <summary>
		/// First negative value that takes 4 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const int Int32VarSizeMinLimit = -Int32VarSizeMaxLimit - 1;

		/// <summary>
		/// First positive value that takes 2 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const short Int16VarSizeMaxLimit = (short)(UInt16VarSizeLimit >> 1);

		/// <summary>
		/// First negative value that takes 2 bytes (or more). Values above this 
		/// limit is encoded in a shorter manner using variable-length encoding 
		/// compared to a fixed-length encoding.
		/// </summary>
		public const short Int16VarSizeMinLimit = -Int16VarSizeMaxLimit - 1;

		/// <summary>
		/// What type of object is being serialized.
		/// </summary>
		public abstract bool IsNullable
		{
			get;
		}

		/// <summary>
		/// If the underlying object is nullable.
		/// </summary>
		public abstract Type ValueType
		{
			get;
		}

		/// <summary>
		/// Initializes the serializer before first-time use.
		/// </summary>
		public virtual Task Init()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public abstract Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded);

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public abstract Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State);

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public abstract Task<object> TryGetFieldValue(string FieldName, object Object);

		/// <summary>
		/// Reads a boolean value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static bool ReadBoolean(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean();
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte() != 0;
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16() != 0;
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32() != 0;
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64() != 0;
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte() != 0;
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16() != 0;
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32() != 0;
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64() != 0;
				case ObjectSerializer.TYPE_VARINT16: return Reader.ReadVariableLengthInt16() != 0;
				case ObjectSerializer.TYPE_VARINT32: return Reader.ReadVariableLengthInt32() != 0;
				case ObjectSerializer.TYPE_VARINT64: return Reader.ReadVariableLengthInt64() != 0;
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16() != 0;
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32() != 0;
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64() != 0;
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal() != 0;
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble() != 0;
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle() != 0;
				default:
					throw new ArgumentException("Expected a boolean value, but was a " + ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".",
						nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable boolean value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static bool? ReadNullableBoolean(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadBoolean(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a byte value.
		/// </summary>
		/// <param name="Reader">Serializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte ReadByte(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (byte)1 : (byte)0;
				case ObjectSerializer.TYPE_INT16: return (byte)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (byte)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (byte)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (byte)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (byte)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (byte)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (byte)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (byte)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (byte)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (byte)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (byte)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (byte)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (byte)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (byte)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (byte)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (byte)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return byte.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a byte value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable byte value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte? ReadNullableByte(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a signed byte value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Signed Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static sbyte ReadSByte(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_SBYTE: return (sbyte)Reader.ReadSByte();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (sbyte)1 : (sbyte)0;
				case ObjectSerializer.TYPE_BYTE: return (sbyte)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (sbyte)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (sbyte)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (sbyte)Reader.ReadInt64();
				case ObjectSerializer.TYPE_UINT16: return (sbyte)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (sbyte)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (sbyte)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (sbyte)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (sbyte)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (sbyte)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (sbyte)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (sbyte)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (sbyte)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (sbyte)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (sbyte)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (sbyte)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return sbyte.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a signed byte value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable signed byte value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable signed byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static sbyte? ReadNullableSByte(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadSByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static short ReadInt16(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (short)1 : (short)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT32: return (short)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (short)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (short)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (short)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (short)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (short)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (short)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (short)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (short)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (short)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (short)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (short)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (short)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (short)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return short.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 16-bit integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static short? ReadNullableInt16(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static int ReadInt32(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT64: return (int)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (int)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (int)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (int)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (int)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (int)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (int)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (int)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (int)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (int)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (int)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (int)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return int.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 32-bit integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static int? ReadNullableInt32(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a nullable enum stored as a nullable 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <param name="EnumType">Enumeration type.</param>
		/// <returns>Nullable enumeration value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static object ReadNullableEnum(IDeserializer Reader, uint FieldDataType, Type EnumType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BYTE: 
					return Enum.ToObject(EnumType, (int)ReadByte(Reader, FieldDataType));
				case ObjectSerializer.TYPE_INT16:
				case ObjectSerializer.TYPE_VARINT16:
					return Enum.ToObject(EnumType, (int)ReadInt16(Reader, FieldDataType));
				case ObjectSerializer.TYPE_INT32:
				case ObjectSerializer.TYPE_VARINT32:
					return Enum.ToObject(EnumType, ReadInt32(Reader, FieldDataType));
				case ObjectSerializer.TYPE_INT64:
				case ObjectSerializer.TYPE_VARINT64:
					return Enum.ToObject(EnumType, ReadInt64(Reader, FieldDataType));
				case ObjectSerializer.TYPE_SBYTE:
					return Enum.ToObject(EnumType, (int)ReadSByte(Reader, FieldDataType));
				case ObjectSerializer.TYPE_UINT16:
				case ObjectSerializer.TYPE_VARUINT16:
					return Enum.ToObject(EnumType, (int)ReadUInt16(Reader, FieldDataType));
				case ObjectSerializer.TYPE_UINT32:
				case ObjectSerializer.TYPE_VARUINT32:
					return Enum.ToObject(EnumType, (long)ReadUInt32(Reader, FieldDataType));
				case ObjectSerializer.TYPE_UINT64:
				case ObjectSerializer.TYPE_VARUINT64:
					return Enum.ToObject(EnumType, (long)ReadUInt64(Reader, FieldDataType));
				case ObjectSerializer.TYPE_ENUM:
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: 
					return Enum.Parse(EnumType, Reader.ReadString());
				case ObjectSerializer.TYPE_NULL:
					return null;
				default:
					throw new ArgumentException("Expected an enumerated value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static long ReadInt64(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (long)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (long)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (long)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (long)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (long)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (long)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (long)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (long)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (long)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (long)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return long.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 64-bit integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static long? ReadNullableInt64(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ushort ReadUInt16(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ushort)1 : (ushort)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ushort)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ushort)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ushort)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ushort)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT32: return (ushort)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (ushort)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (ushort)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (ushort)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (ushort)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (ushort)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (ushort)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (ushort)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ushort)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ushort)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ushort)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return ushort.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 16-bit unsigned integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ushort? ReadNullableUInt16(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadUInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static uint ReadUInt32(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (uint)1 : (uint)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (uint)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (uint)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (uint)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (uint)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT64: return (uint)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (uint)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (uint)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (uint)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (uint)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (uint)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (uint)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (uint)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (uint)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (uint)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return uint.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 32-bit unsigned integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static uint? ReadNullableUInt32(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadUInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ulong ReadUInt64(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ulong)1 : (ulong)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ulong)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ulong)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ulong)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ulong)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_VARINT16: return (ulong)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (ulong)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (ulong)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ulong)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ulong)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ulong)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return ulong.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 64-bit unsigned integer value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ulong? ReadNullableUInt64(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadUInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a decimal value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static decimal ReadDecimal(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DOUBLE: return (decimal)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (decimal)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return decimal.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a decimal value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable decimal value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static decimal? ReadNullableDecimal(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadDecimal(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a double value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static double ReadDouble(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (double)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return double.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a double value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable double value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static double? ReadNullableDouble(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadDouble(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a single value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static float ReadSingle(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (float)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (float)Reader.ReadDouble();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return float.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a single value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable single value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static float? ReadNullableSingle(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadSingle(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a char value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static char ReadChar(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_CHAR: return Reader.ReadChar();
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (char)1 : (char)0;
				case ObjectSerializer.TYPE_BYTE: return (char)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (char)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (char)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (char)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (char)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (char)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (char)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (char)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_VARINT16: return (char)Reader.ReadVariableLengthInt16();
				case ObjectSerializer.TYPE_VARINT32: return (char)Reader.ReadVariableLengthInt32();
				case ObjectSerializer.TYPE_VARINT64: return (char)Reader.ReadVariableLengthInt64();
				case ObjectSerializer.TYPE_VARUINT16: return (char)Reader.ReadVariableLengthUInt16();
				case ObjectSerializer.TYPE_VARUINT32: return (char)Reader.ReadVariableLengthUInt32();
				case ObjectSerializer.TYPE_VARUINT64: return (char)Reader.ReadVariableLengthUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (char)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (char)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (char)Reader.ReadSingle();

				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING:
					string s = Reader.ReadString();
					if (string.IsNullOrEmpty(s))
						return (char)0;
					else
						return s[0];

				default:
					throw new ArgumentException("Expected a char value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable char value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static char? ReadNullableChar(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadChar(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a date &amp; time value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTime ReadDateTime(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_DATETIME: return Reader.ReadDateTime();
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return Reader.ReadDateTimeOffset().DateTime;
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return DateTime.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a date & time value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable date &amp; time value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTime? ReadNullableDateTime(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadDateTime(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a date &amp; time value with offset.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>DateTimeOffset value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTimeOffset ReadDateTimeOffset(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return Reader.ReadDateTimeOffset();
				case ObjectSerializer.TYPE_DATETIME: return (DateTimeOffset)Reader.ReadDateTime();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return DateTimeOffset.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a date & time value with offset, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable date &amp; time value with offset.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable DateTimeOffset value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTimeOffset? ReadNullableDateTimeOffset(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadDateTimeOffset(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a time span value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static TimeSpan ReadTimeSpan(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadTimeSpan();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return TimeSpan.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a time span value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable time span value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static TimeSpan? ReadNullableTimeSpan(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadTimeSpan(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a GUID value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static Guid ReadGuid(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_GUID: return Reader.ReadGuid();
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING: return Guid.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a GUID value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable GUID value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static Guid? ReadNullableGuid(IDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return ReadGuid(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a string value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static string ReadString(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_STRING:
				case ObjectSerializer.TYPE_CI_STRING:
				case ObjectSerializer.TYPE_ENUM: return Reader.ReadString();
				case ObjectSerializer.TYPE_NULL: return null;
				case ObjectSerializer.TYPE_CHAR: return new string(Reader.ReadChar(), 1);
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean().ToString();
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte().ToString();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16().ToString();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32().ToString();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64().ToString();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte().ToString();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16().ToString();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32().ToString();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64().ToString();
				case ObjectSerializer.TYPE_VARINT16: return Reader.ReadVariableLengthInt16().ToString();
				case ObjectSerializer.TYPE_VARINT32: return Reader.ReadVariableLengthInt32().ToString();
				case ObjectSerializer.TYPE_VARINT64: return Reader.ReadVariableLengthInt64().ToString();
				case ObjectSerializer.TYPE_VARUINT16: return Reader.ReadVariableLengthUInt16().ToString();
				case ObjectSerializer.TYPE_VARUINT32: return Reader.ReadVariableLengthUInt32().ToString();
				case ObjectSerializer.TYPE_VARUINT64: return Reader.ReadVariableLengthUInt64().ToString();
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal().ToString();
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble().ToString();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_DATETIME: return Reader.ReadDateTime().ToString();
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return Reader.ReadDateTimeOffset().ToString();
				case ObjectSerializer.TYPE_GUID: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadSingle().ToString();
				default:
					throw new ArgumentException("Expected a char value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a case insensitive string value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static CaseInsensitiveString ReadCaseInsensitiveString(IDeserializer Reader, uint FieldDataType)
		{
			return (CaseInsensitiveString)ReadString(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a string value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte[] ReadByteArray(IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BYTEARRAY: return Reader.ReadByteArray();
				case ObjectSerializer.TYPE_NULL: return null;
				default:
					throw new ArgumentException("Expected a byte array value, but was a " +
						ObjectSerializer.GetFieldDataTypeName(FieldDataType) + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a tag array.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static async Task<KeyValuePair<string, object>[]> ReadTagArray(ISerializerContext Context, IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_OBJECT:
					TagsObjectSerializer Serializer = new TagsObjectSerializer(Context);
					return (KeyValuePair<string, object>[])await Serializer.Deserialize(Reader, FieldDataType, true);

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Object expected.");
			}
		}

		/// <summary>
		/// Reads a tag array.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static async Task<KeyValuePair<string, IElement>[]> ReadTagElementArray(ISerializerContext Context, IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_OBJECT:
					TagElementsObjectSerializer Serializer = new TagElementsObjectSerializer(Context);
					return (KeyValuePair<string, IElement>[])await Serializer.Deserialize(Reader, FieldDataType, true);

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Object expected.");
			}
		}

		/// <summary>
		/// Reads a typed array.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static async Task<T[]> ReadArray<T>(ISerializerContext Context, IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_ARRAY:
					ChunkedList<T> Elements = new ChunkedList<T>();
					IObjectSerializer S = await Context.GetObjectSerializer(typeof(T));
					ulong NrElements = Reader.ReadVariableLengthUInt64();
					uint ElementDataType = Reader.ReadBits(6);
					uint? ElementDataTypeN = ElementDataType == ObjectSerializer.TYPE_NULL ? (uint?)null : (uint?)ElementDataType;

					while (NrElements > 0)
					{
						if (await S.Deserialize(Reader, ElementDataTypeN, true) is T Item)
							Elements.Add(Item);
						else
							Elements.Add(default);

						NrElements--;
					}

					return Elements.ToArray();

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Array expected.");
			}
		}

		/// <summary>
		/// Reads a typed array.
		/// </summary>
		/// <param name="T">Element type.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static async Task<Array> ReadArray(Type T, ISerializerContext Context, IDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_ARRAY:
					IObjectSerializer S = await Context.GetObjectSerializer(T);
					ulong NrElements = Reader.ReadVariableLengthUInt64();
					if (NrElements > int.MaxValue)
						throw new Exception("Array too long.");

					int i, c = (int)NrElements;
					Array Result = Array.CreateInstance(T, c);

					uint ElementDataType = Reader.ReadBits(6);
					uint? ElementDataTypeN = ElementDataType == ObjectSerializer.TYPE_NULL ? (uint?)null : (uint?)ElementDataType;
					object Item;

					for (i = 0; i < c; i++)
					{
						Item = await S.Deserialize(Reader, ElementDataTypeN, true);
						Result.SetValue(Item, i);
					}

					return Result;

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Array expected.");
			}
		}

		/// <summary>
		/// Writes a tag array.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Writer">Serializer.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public static async Task WriteTagArray(ISerializerContext Context, ISerializer Writer, KeyValuePair<string, object>[] Value, object State)
		{
			if (Value is null)
				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			else
				await new TagsObjectSerializer(Context).Serialize(Writer, true, true, Value, State);
		}

		/// <summary>
		/// Writes a tag array.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Writer">Serializer.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public static async Task WriteTagElementArray(ISerializerContext Context, ISerializer Writer, KeyValuePair<string, IElement>[] Value, object State)
		{
			if (Value is null)
				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			else
				await new TagElementsObjectSerializer(Context).Serialize(Writer, true, true, Value, State);
		}

		/// <summary>
		/// Writes a typed array.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Writer">Serializer.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public static async Task WriteArray<T>(ISerializerContext Context, ISerializer Writer, T[] Value, object State)
		{
			if (Value is null)
				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			else
			{
				Type LastType = typeof(T);
				IObjectSerializer S = await Context.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable;

				Writer.WriteBits(ObjectSerializer.TYPE_ARRAY, 6);
				Writer.WriteVariableLengthUInt64((ulong)Value.Length);

				if (Nullable = S.IsNullable)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					Writer.WriteBits(ObjectSerializer.GetFieldDataTypeCode(LastType), 6);

				foreach (T Item in Value)
				{
					if (Item == null)
					{
						if (Nullable)
							Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
						else
							throw new Exception("Elements cannot be null.");
					}
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = await Context.GetObjectSerializer(ItemType);
							LastType = ItemType;
						}

						await S.Serialize(Writer, Nullable, true, Item, State);
					}
				}
			}
		}

		/// <summary>
		/// Writes an array.
		/// </summary>
		/// <param name="T">Element type.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Writer">Serializer.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public static async Task WriteArray(Type T, ISerializerContext Context, ISerializer Writer, Array Value, object State)
		{
			if (Value is null)
				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			else
			{
				Type LastType = T;
				IObjectSerializer S = await Context.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable;

				Writer.WriteBits(ObjectSerializer.TYPE_ARRAY, 6);
				Writer.WriteVariableLengthUInt64((ulong)Value.Length);

				if (Nullable = S.IsNullable)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					Writer.WriteBits(ObjectSerializer.GetFieldDataTypeCode(LastType), 6);

				foreach (object Item in Value)
				{
					if (Item is null)
					{
						if (Nullable)
							Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
						else
							throw new Exception("Elements cannot be null.");
					}
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = await Context.GetObjectSerializer(ItemType);
							LastType = ItemType;
						}

						await S.Serialize(Writer, Nullable, true, Item, State);
					}
				}
			}
		}

		/// <summary>
		/// Reads an embedded object.
		/// </summary>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <returns>Deserialized object.</returns>
		public static async Task<object> ReadEmbeddedObject(ISerializerContext Context, IDeserializer Reader, uint? DataType)
		{
			IObjectSerializer Serializer = await Context.GetObjectSerializer(typeof(object));
			return await Serializer.Deserialize(Reader, DataType, true);
		}

	}
}

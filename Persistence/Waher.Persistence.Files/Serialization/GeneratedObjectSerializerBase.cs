using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	public abstract class GeneratedObjectSerializerBase : IObjectSerializer
	{
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
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public abstract object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded);

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public abstract void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value);

		/// <summary>
		/// Reads a boolean value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected bool ReadBoolean(BinaryDeserializer Reader, uint FieldDataType)
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
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal() != 0;
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble() != 0;
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle() != 0;
				default:
					throw new ArgumentException("Expected a boolean value, but was a " + FilesProvider.GetFieldDataTypeName(FieldDataType) + ".",
						"FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable boolean value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected bool? ReadNullableBoolean(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadBoolean(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected byte ReadByte(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (byte)1 : (byte)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (byte)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (byte)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (byte)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (byte)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (byte)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (byte)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (byte)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (byte)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (byte)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (byte)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return byte.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a byte value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected byte? ReadNullableByte(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a signed byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Signed Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected sbyte ReadSByte(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (sbyte)1 : (sbyte)0;
				case ObjectSerializer.TYPE_BYTE: return (sbyte)Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (sbyte)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (sbyte)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (sbyte)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (sbyte)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (sbyte)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (sbyte)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (sbyte)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (sbyte)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (sbyte)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (sbyte)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return sbyte.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a signed byte value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable signed byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable signed byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected sbyte? ReadNullableSByte(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadSByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected short ReadInt16(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (short)1 : (short)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (short)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (short)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return (short)Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (short)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (short)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (short)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (short)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (short)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return short.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 16-bit integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected short? ReadNullableInt16(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected int ReadInt32(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (int)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (int)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (int)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (int)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (int)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (int)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return int.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 32-bit integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected int? ReadNullableInt32(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected long ReadInt64(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (long)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (long)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (long)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (long)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return long.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 64-bit integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected long? ReadNullableInt64(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected ushort ReadUInt16(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ushort)1 : (ushort)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ushort)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ushort)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ushort)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ushort)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return (ushort)Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (ushort)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ushort)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ushort)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ushort)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return ushort.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 16-bit unsigned integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected ushort? ReadNullableUInt16(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadUInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected uint ReadUInt32(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (uint)1 : (uint)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (uint)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (uint)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (uint)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (uint)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return (uint)Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (uint)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (uint)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (uint)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return uint.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 32-bit unsigned integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected uint? ReadNullableUInt32(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadUInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected ulong ReadUInt64(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? (ulong)1 : (ulong)0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return (ulong)Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return (ulong)Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return (ulong)Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return (ulong)Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (ulong)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (ulong)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (ulong)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return ulong.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a 64-bit unsigned integer value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected ulong? ReadNullableUInt64(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadUInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a decimal value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected decimal ReadDecimal(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (decimal)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (decimal)Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return decimal.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a decimal value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable decimal value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected decimal? ReadNullableDecimal(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadDecimal(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a double value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected double ReadDouble(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (double)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return double.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a double value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable double value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected double? ReadNullableDouble(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadDouble(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a single value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected float ReadSingle(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return Reader.ReadBoolean() ? 1 : 0;
				case ObjectSerializer.TYPE_BYTE: return Reader.ReadByte();
				case ObjectSerializer.TYPE_INT16: return Reader.ReadInt16();
				case ObjectSerializer.TYPE_INT32: return Reader.ReadInt32();
				case ObjectSerializer.TYPE_INT64: return Reader.ReadInt64();
				case ObjectSerializer.TYPE_SBYTE: return Reader.ReadSByte();
				case ObjectSerializer.TYPE_UINT16: return Reader.ReadUInt16();
				case ObjectSerializer.TYPE_UINT32: return Reader.ReadUInt32();
				case ObjectSerializer.TYPE_UINT64: return Reader.ReadUInt64();
				case ObjectSerializer.TYPE_DECIMAL: return (float)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (float)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle();
				case ObjectSerializer.TYPE_STRING: return float.Parse(Reader.ReadString());
				default:
					throw new ArgumentException("Expected a single value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable single value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected float? ReadNullableSingle(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadSingle(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a char value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected char ReadChar(BinaryDeserializer Reader, uint FieldDataType)
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
				case ObjectSerializer.TYPE_DECIMAL: return (char)Reader.ReadDecimal();
				case ObjectSerializer.TYPE_DOUBLE: return (char)Reader.ReadDouble();
				case ObjectSerializer.TYPE_SINGLE: return (char)Reader.ReadSingle();

				case ObjectSerializer.TYPE_STRING:
					string s = Reader.ReadString();
					if (string.IsNullOrEmpty(s))
						return (char)0;
					else
						return s[0];

				default:
					throw new ArgumentException("Expected a char value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable char value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected char? ReadNullableChar(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadChar(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a date & time value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected DateTime ReadDateTime(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_DATETIME: return Reader.ReadDateTime();
				case ObjectSerializer.TYPE_STRING: return DateTime.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a date & time value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable date & time value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected DateTime? ReadNullableDateTime(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadDateTime(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a time span value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected TimeSpan ReadTimeSpan(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadTimeSpan();
				case ObjectSerializer.TYPE_STRING: return TimeSpan.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a time span value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable time span value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected TimeSpan? ReadNullableTimeSpan(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadTimeSpan(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a GUID value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected Guid ReadGuid(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_GUID: return Reader.ReadGuid();
				case ObjectSerializer.TYPE_STRING: return Guid.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a GUID value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a nullable GUID value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected Guid? ReadNullableGuid(BinaryDeserializer Reader, uint FieldDataType)
		{
			if (FieldDataType == ObjectSerializer.TYPE_NULL)
				return null;
			else
				return this.ReadGuid(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a string value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected string ReadString(BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_STRING: return Reader.ReadString();
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
				case ObjectSerializer.TYPE_DECIMAL: return Reader.ReadDecimal().ToString();
				case ObjectSerializer.TYPE_DOUBLE: return Reader.ReadDouble().ToString();
				case ObjectSerializer.TYPE_SINGLE: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_DATETIME: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_GUID: return Reader.ReadSingle().ToString();
				case ObjectSerializer.TYPE_TIMESPAN: return Reader.ReadSingle().ToString();
				default:
					throw new ArgumentException("Expected a char value, but was a " +
						FilesProvider.GetFieldDataTypeName(FieldDataType) + ".", "FieldDataType");
			}
		}

		/// <summary>
		/// Reads a typed array.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Provider">Database provider object.</param>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		protected T[] ReadArray<T>(FilesProvider Provider, BinaryDeserializer Reader, uint FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_ARRAY:
					List<T> Elements = new List<T>();
					IObjectSerializer S = Provider.GetObjectSerializer(typeof(T));
					ulong NrElements = Reader.ReadVariableLengthUInt64();
					uint ElementDataType = Reader.ReadBits(6);
					uint? ElementDataTypeN = ElementDataType == ObjectSerializer.TYPE_NULL ? (uint?)null : (uint?)ElementDataType;

					while (NrElements-- > 0)
						Elements.Add((T)S.Deserialize(Reader, ElementDataTypeN, true));

					return Elements.ToArray();

				case ObjectSerializer.TYPE_NULL:
					return null;

				default:
					throw new Exception("Array expected.");
			}
		}

		/// <summary>
		/// Writes a typed array.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="Provider">Database provider object.</param>
		/// <param name="Writer">Binary writer.</param>
		/// <param name="Value">Value to serialize.</param>
		protected void WriteArray<T>(FilesProvider Provider, BinarySerializer Writer, T[] Value)
		{
			if (Value == null)
				Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
			else
			{
				Type LastType = typeof(T);
				IObjectSerializer S = Provider.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable;

				Writer.WriteBits(ObjectSerializer.TYPE_ARRAY, 6);
				Writer.WriteVariableLengthUInt64((ulong)Value.Length);

				if (Nullable = S.IsNullable)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					Writer.WriteBits(FilesProvider.GetFieldDataTypeCode(LastType), 6);

				foreach (T Item in Value)
				{
					if (Item == null)
						Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
					else
					{
						ItemType = Item.GetType();
						if (ItemType != LastType)
						{
							S = Provider.GetObjectSerializer(ItemType);
							LastType = ItemType;
						}

						if (Nullable)
							S.Serialize(Writer, true, true, Item);
						else
							S.Serialize(Writer, false, true, Item);
					}
				}
			}
		}

	}
}

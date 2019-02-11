using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.MongoDB.Serialization
{
	/// <summary>
	/// Abstract base class for generated object serializers.
	/// </summary>
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
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Corresponding field or property value, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public abstract bool TryGetFieldValue(string FieldName, object Object, out object Value);

		/// <summary>
		/// Reads a boolean value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static bool ReadBoolean(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean();
				case BsonType.Decimal128: return Reader.ReadDecimal128() != 0;
				case BsonType.Double: return Reader.ReadDouble() != 0;
				case BsonType.Int32: return Reader.ReadInt32() != 0;
				case BsonType.Int64: return Reader.ReadInt64() != 0;
				case BsonType.MinKey: Reader.ReadMinKey(); return false;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return true;
				default:
					throw new ArgumentException("Expected a boolean value, but was a " + FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable boolean value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable boolean value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static bool? ReadNullableBoolean(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadBoolean(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte ReadByte(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (byte)1 : (byte)0;
				case BsonType.Decimal128: return (byte)Reader.ReadDecimal128();
				case BsonType.Double: return (byte)Reader.ReadDouble();
				case BsonType.Int32: return (byte)Reader.ReadInt32();
				case BsonType.Int64: return (byte)Reader.ReadInt64();
				case BsonType.String: return byte.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return byte.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return byte.MaxValue;
				default:
					throw new ArgumentException("Expected a byte value, but was a " + FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte? ReadNullableByte(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a signed byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Signed Byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static sbyte ReadSByte(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (sbyte)1 : (sbyte)0;
				case BsonType.Decimal128: return (sbyte)Reader.ReadDecimal128();
				case BsonType.Double: return (sbyte)Reader.ReadDouble();
				case BsonType.Int32: return (sbyte)Reader.ReadInt32();
				case BsonType.Int64: return (sbyte)Reader.ReadInt64();
				case BsonType.String: return sbyte.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return sbyte.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return sbyte.MaxValue;
				default:
					throw new ArgumentException("Expected a signed byte value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable signed byte value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable signed byte value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static sbyte? ReadNullableSByte(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadSByte(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static short ReadInt16(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (short)1 : (short)0;
				case BsonType.Decimal128: return (short)Reader.ReadDecimal128();
				case BsonType.Double: return (short)Reader.ReadDouble();
				case BsonType.Int32: return (short)Reader.ReadInt32();
				case BsonType.Int64: return (short)Reader.ReadInt64();
				case BsonType.String: return short.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return short.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return short.MaxValue;
				default:
					throw new ArgumentException("Expected a 16-bit integer value, but was a " + FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static short? ReadNullableInt16(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static int ReadInt32(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (int)1 : (int)0;
				case BsonType.Decimal128: return (int)Reader.ReadDecimal128();
				case BsonType.Double: return (int)Reader.ReadDouble();
				case BsonType.Int32: return (int)Reader.ReadInt32();
				case BsonType.Int64: return (int)Reader.ReadInt64();
				case BsonType.String: return int.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return int.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return int.MaxValue;
				default:
					throw new ArgumentException("Expected a 32-bit integer value, but was a " + FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static int? ReadNullableInt32(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a nullable enum stored as a nullable 32-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <param name="EnumType">Enumeration type.</param>
		/// <returns>Nullable enumeration value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static object ReadNullableEnum(IBsonReader Reader, BsonType FieldDataType, Type EnumType)
		{
			switch (FieldDataType)
			{
				case BsonType.Int32: return Enum.ToObject(EnumType, ReadInt32(Reader, FieldDataType));
				case BsonType.Int64: return Enum.ToObject(EnumType, ReadInt64(Reader, FieldDataType));
				case BsonType.String: return Enum.Parse(EnumType, Reader.ReadString());
				case BsonType.Null: return null;
				default:
					throw new ArgumentException("Expected an enumerated value, but was a " + FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static long ReadInt64(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (long)1 : (long)0;
				case BsonType.Decimal128: return (long)Reader.ReadDecimal128();
				case BsonType.Double: return (long)Reader.ReadDouble();
				case BsonType.Int32: return (long)Reader.ReadInt32();
				case BsonType.Int64: return (long)Reader.ReadInt64();
				case BsonType.String: return long.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return long.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return long.MaxValue;
				default:
					throw new ArgumentException("Expected a 64-bit integer value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static long? ReadNullableInt64(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>16-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ushort ReadUInt16(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (ushort)1 : (ushort)0;
				case BsonType.Decimal128: return (ushort)Reader.ReadDecimal128();
				case BsonType.Double: return (ushort)Reader.ReadDouble();
				case BsonType.Int32: return (ushort)Reader.ReadInt32();
				case BsonType.Int64: return (ushort)Reader.ReadInt64();
				case BsonType.String: return ushort.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return ushort.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return ushort.MaxValue;
				default:
					throw new ArgumentException("Expected a 16-bit unsigned integer value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 16-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 16-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ushort? ReadNullableUInt16(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadUInt16(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>32-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static uint ReadUInt32(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (uint)1 : (uint)0;
				case BsonType.Decimal128: return (uint)Reader.ReadDecimal128();
				case BsonType.Double: return (uint)Reader.ReadDouble();
				case BsonType.Int32: return (uint)Reader.ReadInt32();
				case BsonType.Int64: return (uint)Reader.ReadInt64();
				case BsonType.String: return uint.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return uint.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return uint.MaxValue;
				default:
					throw new ArgumentException("Expected a 32-bit unsigned integer value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 32-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 32-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static uint? ReadNullableUInt32(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadUInt32(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>64-bit unsigned integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ulong ReadUInt64(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (ulong)1 : (ulong)0;
				case BsonType.Decimal128: return (ulong)Reader.ReadDecimal128();
				case BsonType.Double: return (ulong)Reader.ReadDouble();
				case BsonType.Int32: return (ulong)Reader.ReadInt32();
				case BsonType.Int64: return (ulong)Reader.ReadInt64();
				case BsonType.String: return ulong.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return ulong.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return ulong.MaxValue;
				default:
					throw new ArgumentException("Expected a 64-bit unsigned integer value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable 64-bit unsigned integer value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable unsigned 64-bit integer value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static ulong? ReadNullableUInt64(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadUInt64(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a decimal value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static decimal ReadDecimal(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (decimal)1 : (decimal)0;
				case BsonType.Decimal128: return (decimal)Reader.ReadDecimal128();
				case BsonType.Double: return (decimal)Reader.ReadDouble();
				case BsonType.Int32: return (decimal)Reader.ReadInt32();
				case BsonType.Int64: return (decimal)Reader.ReadInt64();
				case BsonType.String: return decimal.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return decimal.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return decimal.MaxValue;
				default:
					throw new ArgumentException("Expected a decimal value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable decimal value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable decimal value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static decimal? ReadNullableDecimal(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadDecimal(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a double value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static double ReadDouble(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (double)1 : (double)0;
				case BsonType.Decimal128: return (double)Reader.ReadDecimal128();
				case BsonType.Double: return (double)Reader.ReadDouble();
				case BsonType.Int32: return (double)Reader.ReadInt32();
				case BsonType.Int64: return (double)Reader.ReadInt64();
				case BsonType.String: return double.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return double.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return double.MaxValue;
				default:
					throw new ArgumentException("Expected a double value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable double value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable double value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static double? ReadNullableDouble(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadDouble(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a single value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static float ReadSingle(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Boolean: return Reader.ReadBoolean() ? (float)1 : (float)0;
				case BsonType.Decimal128: return (float)Reader.ReadDecimal128();
				case BsonType.Double: return (float)Reader.ReadDouble();
				case BsonType.Int32: return (float)Reader.ReadInt32();
				case BsonType.Int64: return (float)Reader.ReadInt64();
				case BsonType.String: return float.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return float.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return float.MaxValue;
				default:
					throw new ArgumentException("Expected a single value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable single value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable single value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static float? ReadNullableSingle(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadSingle(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a char value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static char ReadChar(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Decimal128: return (char)Reader.ReadDecimal128();
				case BsonType.Double: return (char)Reader.ReadDouble();
				case BsonType.Int32: return (char)Reader.ReadInt32();
				case BsonType.Int64: return (char)Reader.ReadInt64();
				case BsonType.MinKey: Reader.ReadMinKey(); return char.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return char.MaxValue;
				case BsonType.String:
					string s = Reader.ReadString();
					return string.IsNullOrEmpty(s) ? (char)0 : s[0];

				default:
					throw new ArgumentException("Expected a char value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable char value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable char value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static char? ReadNullableChar(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadChar(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a date &amp; time value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTime ReadDateTime(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.DateTime: return ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
				case BsonType.String: return DateTime.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return DateTime.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return DateTime.MaxValue;

				default:
					throw new ArgumentException("Expected a date & time value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable date &amp; time value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable DateTime value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTime? ReadNullableDateTime(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadDateTime(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a date &amp; time value with offset.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>DateTimeOffset value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTimeOffset ReadDateTimeOffset(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.DateTime: return (DateTimeOffset)ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
				case BsonType.String: return DateTimeOffset.Parse(Reader.ReadString());
				case BsonType.Document:
					DateTime TP = DateTime.MinValue;
					TimeSpan TZ = TimeSpan.Zero;

					Reader.ReadStartDocument();

					while (Reader.State == BsonReaderState.Type)
					{
						BsonType BsonType = Reader.ReadBsonType();
						if (BsonType == BsonType.EndOfDocument)
							break;

						string FieldName = Reader.ReadName();
						switch (FieldName)
						{
							case "tp":
								TP = ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());
								break;

							case "tz":
								TZ = TimeSpan.Parse(Reader.ReadString());
								break;
						}
					}

					Reader.ReadEndDocument();

					return new DateTimeOffset(TP, TZ);

				case BsonType.MinKey: Reader.ReadMinKey(); return DateTimeOffset.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return DateTimeOffset.MaxValue;

				default:
					throw new ArgumentException("Expected a date & time value with offset, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable date &amp; time value with offset.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable DateTimeOffset value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static DateTimeOffset? ReadNullableDateTimeOffset(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadDateTimeOffset(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a time span value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static TimeSpan ReadTimeSpan(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.String: return TimeSpan.Parse(Reader.ReadString());
				case BsonType.MinKey: Reader.ReadMinKey(); return TimeSpan.MinValue;
				case BsonType.MaxKey: Reader.ReadMaxKey(); return TimeSpan.MaxValue;

				default:
					throw new ArgumentException("Expected a time span value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable time span value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable TimeSpan value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static TimeSpan? ReadNullableTimeSpan(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadTimeSpan(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a GUID value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static Guid ReadGuid(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.String: return Guid.Parse(Reader.ReadString());

				default:
					throw new ArgumentException("Expected a GUID value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a nullable GUID value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Nullable Guid value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static Guid? ReadNullableGuid(IBsonReader Reader, BsonType FieldDataType)
		{
			if (FieldDataType == BsonType.Null)
				return null;
			else
				return ReadGuid(Reader, FieldDataType);
		}

		/// <summary>
		/// Reads a string value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static string ReadString(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
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
				default:
					throw new ArgumentException("Expected a char value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
			}
		}

		/// <summary>
		/// Reads a string value.
		/// </summary>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static byte[] ReadByteArray(IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Binary: return Reader.ReadBinaryData().Bytes;
				case BsonType.Null: Reader.ReadNull(); return null;
				default:
					throw new ArgumentException("Expected a byte array value, but was a " +
						FieldDataType.ToString() + ".", nameof(FieldDataType));
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
		public static T[] ReadArray<T>(MongoDBProvider Provider, IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Array:
					List<T> Elements = new List<T>();
					IObjectSerializer S = Provider.GetObjectSerializer(typeof(T));

					Reader.ReadStartArray();
					while (Reader.State != BsonReaderState.EndOfArray)
					{
						BsonType? ElementType = null;

						if (Reader.State == BsonReaderState.Type)
						{
							ElementType = Reader.ReadBsonType();
							if (ElementType == BsonType.EndOfDocument)
								break;
						}

						Elements.Add((T)S.Deserialize(Reader, ElementType, true));

						if (Reader.ReadBsonType() == BsonType.EndOfDocument)
							break;
					}

					Reader.ReadEndArray();

					return Elements.ToArray();

				case BsonType.Null:
					return null;

				default:
					throw new Exception("Array expected.");
			}
		}

		/// <summary>
		/// Reads a typed array.
		/// </summary>
		/// <param name="T">Element type.</param>
		/// <param name="Provider">Database provider object.</param>
		/// <param name="Reader">Binary reader.</param>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>String value.</returns>
		/// <exception cref="ArgumentException">If the <paramref name="FieldDataType"/> was invalid.</exception>
		public static Array ReadArray(Type T, MongoDBProvider Provider, IBsonReader Reader, BsonType FieldDataType)
		{
			switch (FieldDataType)
			{
				case BsonType.Array:
					List<object> Elements = new List<object>();
					IObjectSerializer S = Provider.GetObjectSerializer(T ?? typeof(GenericObject));

					Reader.ReadStartArray();
					while (Reader.State != BsonReaderState.EndOfArray)
					{
						BsonType? ElementType = null;

						if (Reader.State == BsonReaderState.Type)
						{
							ElementType = Reader.ReadBsonType();
							if (ElementType == BsonType.EndOfDocument)
								break;
						}

						Elements.Add(S.Deserialize(Reader, ElementType, true));

						if (Reader.ReadBsonType() == BsonType.EndOfDocument)
							break;
					}

					Reader.ReadEndArray();

					if (T is null)
						return Elements.ToArray();

					int c = Elements.Count;
					Array Result = Array.CreateInstance(T, c);
					Array.Copy(Elements.ToArray(), 0, Result, 0, c);

					return Result;

				case BsonType.Null:
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
		public static void WriteArray<T>(MongoDBProvider Provider, BsonWriter Writer, T[] Value)
		{
			if (Value is null)
				Writer.WriteNull();
			else
			{
				Type LastType = typeof(T);
				IObjectSerializer S = Provider.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable = S.IsNullable;

				Writer.WriteStartArray();

				foreach (T Item in Value)
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
							S = Provider.GetObjectSerializer(ItemType);
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
		/// Writes an array.
		/// </summary>
		/// <param name="T">Element type.</param>
		/// <param name="Provider">Database provider object.</param>
		/// <param name="Writer">Binary writer.</param>
		/// <param name="Value">Value to serialize.</param>
		public static void WriteArray(Type T, MongoDBProvider Provider, BsonWriter Writer, Array Value)
		{
			if (Value is null)
				Writer.WriteNull();
			else
			{
				Type LastType = T;
				IObjectSerializer S = Provider.GetObjectSerializer(LastType);
				Type ItemType;
				bool Nullable = S.IsNullable;

				Writer.WriteStartArray();

				foreach (object Item in Value)
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
							S = Provider.GetObjectSerializer(ItemType);
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
		/// Converts a GUID to a MongoDB Object ID
		/// </summary>
		/// <param name="Guid">GUID</param>
		/// <returns>Object ID</returns>
		/// <exception cref="Exception">If GUID contains more than 12 bytes of information.</exception>
		public static ObjectId GuidToObjectId(Guid Guid)
		{
			byte[] A = Guid.ToByteArray();
			int i;

			for (i = 12; i < 16; i++)
			{
				if (A[i] != 0)
					throw new Exception("MongoDB only supports 12-byte object IDs.");
			}

			Array.Resize<byte>(ref A, 12);

			return new ObjectId(A);
		}

		/// <summary>
		/// Converts a MongoDB Object ID to a GUID
		/// </summary>
		/// <param name="Guid">GUID</param>
		/// <returns>Object ID</returns>
		/// <exception cref="Exception">If GUID contains more than 12 bytes of information.</exception>
		public static Guid ObjectIdToGuid(ObjectId ObjectId)
		{
			byte[] A = new byte[16];
			Array.Copy(ObjectId.ToByteArray(), 0, A, 0, 12);
			return new Guid(A);
		}

	}
}

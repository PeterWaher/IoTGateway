using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization.ReferenceTypes
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
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public object Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_ENUM:
				case ObjectSerializer.TYPE_STRING: 
				case ObjectSerializer.TYPE_CI_STRING: return new CaseInsensitiveString(Reader.ReadString());
				case ObjectSerializer.TYPE_CHAR: return new CaseInsensitiveString(new string(Reader.ReadChar(), 1));
				case ObjectSerializer.TYPE_BOOLEAN: return new CaseInsensitiveString(Reader.ReadBoolean().ToString());
				case ObjectSerializer.TYPE_BYTE: return new CaseInsensitiveString(Reader.ReadString().ToString());
				case ObjectSerializer.TYPE_INT16: return new CaseInsensitiveString(Reader.ReadInt16().ToString());
				case ObjectSerializer.TYPE_INT32: return new CaseInsensitiveString(Reader.ReadInt32().ToString());
				case ObjectSerializer.TYPE_INT64: return new CaseInsensitiveString(Reader.ReadInt64().ToString());
				case ObjectSerializer.TYPE_SBYTE: return new CaseInsensitiveString(Reader.ReadSByte().ToString());
				case ObjectSerializer.TYPE_UINT16: return new CaseInsensitiveString(Reader.ReadUInt16().ToString());
				case ObjectSerializer.TYPE_UINT32: return new CaseInsensitiveString(Reader.ReadUInt32().ToString());
				case ObjectSerializer.TYPE_UINT64: return new CaseInsensitiveString(Reader.ReadUInt64().ToString());
				case ObjectSerializer.TYPE_DECIMAL: return new CaseInsensitiveString(Reader.ReadDecimal().ToString());
				case ObjectSerializer.TYPE_DOUBLE: return new CaseInsensitiveString(Reader.ReadDouble().ToString());
				case ObjectSerializer.TYPE_SINGLE: return new CaseInsensitiveString(Reader.ReadSingle().ToString());
				case ObjectSerializer.TYPE_GUID: return new CaseInsensitiveString(Reader.ReadGuid().ToString());
				case ObjectSerializer.TYPE_DATETIME: return new CaseInsensitiveString(Reader.ReadDateTime().ToString());
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return new CaseInsensitiveString(Reader.ReadDateTimeOffset().ToString());
				case ObjectSerializer.TYPE_TIMESPAN: return new CaseInsensitiveString(Reader.ReadTimeSpan().ToString());
				case ObjectSerializer.TYPE_NULL: return null;
				default: throw new Exception("Expected a case-insensitive string value.");
			}
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public void Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_CI_STRING, 6);

				Writer.Write(Value.ToString());
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
			Value = null;
			return false;
		}
	}
}

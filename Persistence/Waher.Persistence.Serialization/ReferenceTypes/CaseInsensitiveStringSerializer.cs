using System;
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
		/// Initializes the serializer before first-time use.
		/// </summary>
		public Task Init()
		{
			return Task.CompletedTask;
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
		public Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
			if (!DataType.HasValue)
				DataType = Reader.ReadBits(6);

			switch (DataType.Value)
			{
				case ObjectSerializer.TYPE_ENUM:
				case ObjectSerializer.TYPE_STRING: 
				case ObjectSerializer.TYPE_CI_STRING: return Task.FromResult<object>(Reader.ReadCaseInsensitiveString());
				case ObjectSerializer.TYPE_CHAR: return Task.FromResult<object>((CaseInsensitiveString)new string(Reader.ReadChar(), 1));
				case ObjectSerializer.TYPE_BOOLEAN: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadBoolean().ToString());
				case ObjectSerializer.TYPE_BYTE: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadByte().ToString());
				case ObjectSerializer.TYPE_INT16: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadInt16().ToString());
				case ObjectSerializer.TYPE_INT32: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadInt32().ToString());
				case ObjectSerializer.TYPE_INT64: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadInt64().ToString());
				case ObjectSerializer.TYPE_SBYTE: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadSByte().ToString());
				case ObjectSerializer.TYPE_UINT16: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadUInt16().ToString());
				case ObjectSerializer.TYPE_UINT32: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadUInt32().ToString());
				case ObjectSerializer.TYPE_UINT64: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadUInt64().ToString());
				case ObjectSerializer.TYPE_VARINT16: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthInt16().ToString());
				case ObjectSerializer.TYPE_VARINT32: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthInt32().ToString());
				case ObjectSerializer.TYPE_VARINT64: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthInt64().ToString());
				case ObjectSerializer.TYPE_VARUINT16: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthUInt16().ToString());
				case ObjectSerializer.TYPE_VARUINT32: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthUInt32().ToString());
				case ObjectSerializer.TYPE_VARUINT64: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadVariableLengthUInt64().ToString());
				case ObjectSerializer.TYPE_DECIMAL: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadDecimal().ToString());
				case ObjectSerializer.TYPE_DOUBLE: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadDouble().ToString());
				case ObjectSerializer.TYPE_SINGLE: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadSingle().ToString());
				case ObjectSerializer.TYPE_GUID: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadGuid().ToString());
				case ObjectSerializer.TYPE_DATETIME: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadDateTime().ToString());
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadDateTimeOffset().ToString());
				case ObjectSerializer.TYPE_TIMESPAN: return Task.FromResult<object>((CaseInsensitiveString)Reader.ReadTimeSpan().ToString());
				case ObjectSerializer.TYPE_NULL: return Task.FromResult<object>(null);
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
		/// <param name="State">State object, passed on in recursive calls.</param>
		public Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State)
		{
			if (Value is null)
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_NULL, 6);
				else
					throw new NullReferenceException("Value cannot be null.");
			}
			else if (Value is CaseInsensitiveString cis)
			{
				if (cis.Value is null)
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

					Writer.Write(cis.Value);
				}
			}
			else
			{
				if (WriteTypeCode)
					Writer.WriteBits(ObjectSerializer.TYPE_CI_STRING, 6);

				Writer.Write(Value.ToString());
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public Task<object> TryGetFieldValue(string FieldName, object Object)
		{
			return Task.FromResult<object>(null);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Serialization.ValueTypes;

namespace Waher.Persistence.Files
{
	public class FilesProvider
	{

		/// <summary>
		/// Returns the type name corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataType">Field data type code.</param>
		/// <returns>Corresponding data type name.</returns>
		public string GetFieldDataTypeName(int FieldDataType)
		{
			return GetFieldDataType(FieldDataType).FullName;
		}
		
		/// <summary>
		/// Returns the type corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataType">Field data type code.</param>
		/// <returns>Corresponding data type.</returns>
		public Type GetFieldDataType(int FieldDataType)
		{
			switch (FieldDataType)
			{
				case ObjectSerializer.TYPE_BOOLEAN: return typeof(bool);
				case ObjectSerializer.TYPE_BYTE: return typeof(byte);
				case ObjectSerializer.TYPE_INT16: return typeof(short);
				case ObjectSerializer.TYPE_INT32: return typeof(int);
				case ObjectSerializer.TYPE_INT64: return typeof(long);
				case ObjectSerializer.TYPE_SBYTE: return typeof(sbyte);
				case ObjectSerializer.TYPE_UINT16: return typeof(ushort);
				case ObjectSerializer.TYPE_UINT32: return typeof(uint);
				case ObjectSerializer.TYPE_UINT64: return typeof(ulong);
				case ObjectSerializer.TYPE_DECIMAL: return typeof(decimal);
				case ObjectSerializer.TYPE_DOUBLE: return typeof(double);
				case ObjectSerializer.TYPE_SINGLE: return typeof(float);
				case ObjectSerializer.TYPE_DATETIME: return typeof(DateTime);
				case ObjectSerializer.TYPE_TIMESPAN: return typeof(TimeSpan);
				case ObjectSerializer.TYPE_CHAR: return typeof(char);
				case ObjectSerializer.TYPE_STRING: return typeof(string);
				case ObjectSerializer.TYPE_ENUM: return typeof(Enum);
				case ObjectSerializer.TYPE_BYTEARRAY: return typeof(byte[]);
				case ObjectSerializer.TYPE_GUID: return typeof(Guid);
				case ObjectSerializer.TYPE_ARRAY: return typeof(Array);
				case ObjectSerializer.TYPE_OBJECT: return typeof(object);
				default: throw new Exception("Unrecognized field code: " + FieldDataType.ToString());
			}
		}

		public IBinarySerializer GetObjectSerializer(Type Type)
		{
			if (Type.IsEnum)
				return new EnumSerializer(Type);

			// TODO: Support normal value types as well.
			// TODO: Support Array-types.
			// TODO: Support nullable value types.

			throw new NotImplementedException();    // TODO
		}

		public string GetFieldName(string Collection, ulong FieldCode)
		{
			throw new NotImplementedException();    // TODO
		}

		public T LoadObject<T>(Guid ObjectId)
		{
			throw new NotImplementedException();    // TODO
		}
	}
}

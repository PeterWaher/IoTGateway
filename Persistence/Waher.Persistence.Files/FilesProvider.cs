using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Serialization.ValueTypes;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Persists objects into binary files.
	/// </summary>
	public class FilesProvider : IDisposable
	{
		private Dictionary<Type, IObjectSerializer> serializers;
		private Dictionary<string, Dictionary<string, ulong>> codeByFieldByCollection = new Dictionary<string, Dictionary<string, ulong>>();
		private Dictionary<string, Dictionary<ulong, string>> fieldByCodeByCollection = new Dictionary<string, Dictionary<ulong, string>>();
		private object synchObj = new object();
		private string defaultCollectionName;
		private string folder;

		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		public FilesProvider(string Folder, string DefaultCollectionName)
		{
			this.serializers = new Dictionary<Type, IObjectSerializer>();
			this.defaultCollectionName = DefaultCollectionName;
			this.folder = Folder;

			ConstructorInfo CI;
			IObjectSerializer S;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IObjectSerializer)))
			{
				if (T.IsAbstract)
					continue;

				CI = T.GetConstructor(Types.NoTypes);
				if (CI == null)
					continue;

				try
				{
					S = (IObjectSerializer)CI.Invoke(Types.NoParameters);
				}
				catch (Exception)
				{
					continue;
				}

				this.serializers[S.ValueType] = S;
			}
		}

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName
		{
			get { return this.defaultCollectionName; }
		}

		/// <summary>
		/// Base folder of where files will be stored.
		/// </summary>
		public string Folder
		{
			get { return this.folder; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.serializers != null)
			{
				IDisposable d;

				foreach (IObjectSerializer Serializer in this.serializers.Values)
				{
					d = Serializer as IDisposable;
					if (d != null)
					{
						try
						{
							d.Dispose();
						}
						catch (Exception)
						{
							// Ignore
						}
					}
				}

				this.serializers.Clear();
			}
		}

		/// <summary>
		/// Returns the type name corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataType">Field data type code.</param>
		/// <returns>Corresponding data type name.</returns>
		public static string GetFieldDataTypeName(uint FieldDataType)
		{
			return GetFieldDataType(FieldDataType).FullName;
		}

		/// <summary>
		/// Returns the type corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataTypeCode">Field data type code.</param>
		/// <returns>Corresponding data type.</returns>
		public static Type GetFieldDataType(uint FieldDataTypeCode)
		{
			switch (FieldDataTypeCode)
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
				default: throw new Exception("Unrecognized field code: " + FieldDataTypeCode.ToString());
			}
		}

		/// <summary>
		/// Returns the type code corresponding to a given field data type.
		/// </summary>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Corresponding data type code.</returns>
		public static uint GetFieldDataTypeCode(Type FieldDataType)
		{
			if (FieldDataType.IsEnum)
				return ObjectSerializer.TYPE_ENUM;

			switch (Type.GetTypeCode(FieldDataType))
			{
				case TypeCode.Boolean: return ObjectSerializer.TYPE_BOOLEAN;
				case TypeCode.Byte: return ObjectSerializer.TYPE_BYTE;
				case TypeCode.Int16: return ObjectSerializer.TYPE_INT16;
				case TypeCode.Int32: return ObjectSerializer.TYPE_INT32;
				case TypeCode.Int64: return ObjectSerializer.TYPE_INT64;
				case TypeCode.SByte: return ObjectSerializer.TYPE_SBYTE;
				case TypeCode.UInt16: return ObjectSerializer.TYPE_UINT16;
				case TypeCode.UInt32: return ObjectSerializer.TYPE_UINT32;
				case TypeCode.UInt64: return ObjectSerializer.TYPE_UINT64;
				case TypeCode.Decimal: return ObjectSerializer.TYPE_DECIMAL;
				case TypeCode.Double: return ObjectSerializer.TYPE_DOUBLE;
				case TypeCode.Single: return ObjectSerializer.TYPE_SINGLE;
				case TypeCode.DateTime: return ObjectSerializer.TYPE_DATETIME;
				case TypeCode.Char: return ObjectSerializer.TYPE_CHAR;
				case TypeCode.String: return ObjectSerializer.TYPE_STRING;

				case TypeCode.Object:
					if (FieldDataType == typeof(TimeSpan))
						return ObjectSerializer.TYPE_TIMESPAN;
					else if (FieldDataType == typeof(byte[]))
						return ObjectSerializer.TYPE_BYTEARRAY;
					else if (FieldDataType.IsArray)
						return ObjectSerializer.TYPE_ARRAY;
					else if (FieldDataType == typeof(Guid))
						return ObjectSerializer.TYPE_GUID;
					else
						return ObjectSerializer.TYPE_OBJECT;

				case TypeCode.DBNull:
				case TypeCode.Empty:
					return ObjectSerializer.TYPE_NULL;

				default:
					throw new ArgumentException("Unrecognized type code.", "FieldDataType");
			}
		}

		public IObjectSerializer GetObjectSerializer(Type Type)
		{
			if (Type.IsEnum)
				return new EnumSerializer(Type);

			// TODO: Support normal value types as well.
			// TODO: Support Array-types.
			// TODO: Support nullable value types.

			IObjectSerializer Result;

			lock (this.synchObj)
			{
				if (this.serializers.TryGetValue(Type, out Result))
					return Result;

				Result = new ObjectSerializer(Type, this);
				this.serializers[Type] = Result;
			}

			return Result;
		}

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public ulong GetFieldCode(string Collection, string FieldName)
		{
			// TODO: Use persisted dictionaries.

			if (string.IsNullOrEmpty(Collection))
				Collection = this.defaultCollectionName;

			Dictionary<string, ulong> List;
			Dictionary<ulong, string> List2;
			ulong Result;

			lock (this.synchObj)
			{
				if (!this.codeByFieldByCollection.TryGetValue(Collection, out List))
				{
					List = new Dictionary<string, ulong>();
					this.codeByFieldByCollection[Collection] = List;

					List2 = new Dictionary<ulong, string>();
					this.fieldByCodeByCollection[Collection] = List2;
				}
				else
					List2 = this.fieldByCodeByCollection[Collection];

				if (List.TryGetValue(FieldName, out Result))
					return Result;

				Result = (uint)List.Count + 1;
				List[FieldName] = Result;
				List2[Result] = FieldName;
			}

			Console.Out.WriteLine(Result + "=" + Collection + "." + FieldName);

			return Result;
		}

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public string GetFieldName(string Collection, ulong FieldCode)
		{
			// TODO: Use persisted dictionaries.

			if (string.IsNullOrEmpty(Collection))
				Collection = this.defaultCollectionName;

			Dictionary<ulong, string> List2;
			string Result;

			lock (this.synchObj)
			{
				if (!this.fieldByCodeByCollection.TryGetValue(Collection, out List2))
					throw new ArgumentException("Collection unknown: " + Collection, "Collection");

				if (!List2.TryGetValue(FieldCode, out Result))
					throw new ArgumentException("Field code unknown: " + FieldCode.ToString(), "FieldCode");
			}

			return Result;
		}

		public T LoadObject<T>(Guid ObjectId)
		{
			throw new NotImplementedException();    // TODO
		}

		public Guid GetObjectId(object Value, bool InsertIfNotFound)
		{
			throw new NotImplementedException();    // TODO
		}
	}
}

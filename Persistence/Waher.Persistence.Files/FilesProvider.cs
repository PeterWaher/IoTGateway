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
		private static Dictionary<Type, IObjectSerializer> serializers;
		private static object classSynchObj = new object();

		private Dictionary<string, Dictionary<string, ulong>> codeByFieldByCollection = new Dictionary<string, Dictionary<string, ulong>>();
		private Dictionary<string, Dictionary<ulong, string>> fieldByCodeByCollection = new Dictionary<string, Dictionary<ulong, string>>();
		private object instanceSynchObj = new object();

		private string defaultCollectionName;
		private string folder;
		private bool debug;


		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		public FilesProvider(string Folder, string DefaultCollectionName)
			: this(Folder, DefaultCollectionName, false)
		{
		}

		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, bool Debug)
		{
			this.defaultCollectionName = DefaultCollectionName;
			this.folder = Folder;
			this.debug = Debug;
		}

#if WINDOWS_UWP
		/// <summary>
		/// Must be called in UWP application when the application is terminated. Stops all dynamic modules that have been loaded
		/// </summary>
		public static void Terminate()
		{
			OnProcessExit(null, new EventArgs());
		}
#endif
		static FilesProvider()
		{
			serializers = GetSerializers();

#if !WINDOWS_UWP
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
#endif
		}

		private static Dictionary<Type, IObjectSerializer> GetSerializers()
		{
			Dictionary<Type, IObjectSerializer> Result = new Dictionary<Type, IObjectSerializer>();

			ConstructorInfo CI;
			IObjectSerializer S;

			lock (classSynchObj)
			{
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

					Result[S.ValueType] = S;
				}
			}

			return Result;
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

		private static void OnProcessExit(object Sender, EventArgs e)
		{
			if (serializers != null)
			{
				IDisposable d;

				foreach (IObjectSerializer Serializer in serializers.Values)
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

				serializers.Clear();
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
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
			// TODO: Support Array-types.

			IObjectSerializer Result;

			lock (classSynchObj)
			{
				if (serializers.TryGetValue(Type, out Result))
					return Result;

				if (Type.IsEnum)
					Result = new EnumSerializer(Type);
				else if (Type.IsGenericType)
				{
					Type GT = Type.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Type NullableType = Type.GenericTypeArguments[0];

						if (NullableType.IsEnum)
							Result = new Serialization.NullableTypes.NullableEnumSerializer(NullableType);
						else
							Result = new ObjectSerializer(Type, this, this.debug);
					}
					else
						Result = new ObjectSerializer(Type, this, this.debug);
				}
				else
					Result = new ObjectSerializer(Type, this, this.debug);

				serializers[Type] = Result;
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

			lock (this.instanceSynchObj)
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

			if (this.debug)
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

			lock (this.instanceSynchObj)
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

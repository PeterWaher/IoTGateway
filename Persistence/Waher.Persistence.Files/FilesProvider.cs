using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Serialization.ReferenceTypes;
using Waher.Persistence.Files.Serialization.ValueTypes;
using Waher.Persistence.Filters;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Persists objects into binary files.
	/// </summary>
	public class FilesProvider : IDisposable, IDatabaseProvider
	{
		private Dictionary<Type, IObjectSerializer> serializers;
		private Dictionary<string, Dictionary<string, ulong>> codeByFieldByCollection = new Dictionary<string, Dictionary<string, ulong>>();
		private Dictionary<string, Dictionary<ulong, string>> fieldByCodeByCollection = new Dictionary<string, Dictionary<ulong, string>>();
		private Dictionary<string, ObjectBTreeFile> files = new Dictionary<string, ObjectBTreeFile>();
		private object synchObj = new object();

		private Encoding encoding;
		private string id;
		private string defaultCollectionName;
		private string folder;
		private int blockSize;
		private int blobBlockSize;
		private int timeoutMilliseconds;
		private bool debug;
		private bool encypted;

		#region Constructors

		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
			: this(Folder, DefaultCollectionName, BlockSize, BlobBlockSize, Encoding, TimeoutMilliseconds, Encrypted, false)
		{
		}

		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Debug)
		{
			ObjectBTreeFile.CheckBlockSizes(BlockSize, BlobBlockSize);

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentException("The timeout must be positive.", "TimeoutMilliseconds");

			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.defaultCollectionName = DefaultCollectionName;
			this.folder = Path.GetFullPath(Folder);
			this.debug = Debug;
			this.blockSize = BlockSize;
			this.blobBlockSize = BlobBlockSize;
			this.encoding = Encoding;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.encypted = Encrypted;
			this.serializers = new Dictionary<Type, Serialization.IObjectSerializer>();

			if (!string.IsNullOrEmpty(this.folder) && this.folder[this.folder.Length - 1] != Path.DirectorySeparatorChar)
				this.folder += Path.DirectorySeparatorChar;

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

			GenericObjectSerializer GenericObjectSerializer = new GenericObjectSerializer(this);

			this.serializers[typeof(GenericObject)] = GenericObjectSerializer;
			this.serializers[typeof(object)] = GenericObjectSerializer;
		}

		#endregion

		#region Properties

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
		/// An ID of the files provider. It's unique, and constant during the life-time of the FilesProvider class.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="InlineObjectSizeLimit"/> will be persisted as BLOBs, with the bulk of the object stored as separate files. 
		/// Smallest block size = 1024, largest block size = 65536.
		/// </summary>
		public int BlockSize { get { return this.blockSize; } }

		/// <summary>
		/// Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.
		/// </summary>
		public int BlobBlockSize { get { return this.blobBlockSize; } }

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding { get { return this.encoding; } }

		/// <summary>
		/// Timeout, in milliseconds, for database operations.
		/// </summary>
		public int TimeoutMilliseconds
		{
			get { return this.timeoutMilliseconds; }
		}

		/// <summary>
		/// If the files should be encrypted or not.
		/// </summary>
		public bool Encrypted
		{
			get { return this.encypted; }
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.files != null)
			{
				foreach (ObjectBTreeFile File in this.files.Values)
					File.Dispose();

				this.files.Clear();
			}

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
				this.serializers = null;
			}
		}

		#endregion

		#region Types

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
			{
				if (FieldDataType.IsDefined(typeof(FlagsAttribute), false))
					return ObjectSerializer.TYPE_INT32;
				else
					return ObjectSerializer.TYPE_ENUM;
			}

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

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public IObjectSerializer GetObjectSerializer(Type Type)
		{
			IObjectSerializer Result;

			lock (synchObj)
			{
				if (this.serializers.TryGetValue(Type, out Result))
					return Result;

				if (Type.IsEnum)
					Result = new EnumSerializer(Type);
				else if (Type.IsArray)
				{
					Type ElementType = Type.GetElementType();
					Type T = Types.GetType(typeof(ByteArraySerializer).FullName.Replace("ByteArray", "Array"));
					Type SerializerType = T.MakeGenericType(new Type[] { ElementType });
					ConstructorInfo CI = SerializerType.GetConstructor(new Type[] { typeof(FilesProvider) });
					Result = (IObjectSerializer)CI.Invoke(new object[] { this });
				}
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

				this.serializers[Type] = Result;
			}

			return Result;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Object">Object to serialize</param>
		/// <returns>Object Serializer</returns>
		public ObjectSerializer GetObjectSerializerEx(object Object)
		{
			return this.GetObjectSerializerEx(Object.GetType());
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public ObjectSerializer GetObjectSerializerEx(Type Type)
		{
			ObjectSerializer Serializer = this.GetObjectSerializer(Type) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Objects of type " + Type.FullName + " must be embedded.");

			return Serializer;
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

			lock (this.synchObj)
			{
				if (!this.fieldByCodeByCollection.TryGetValue(Collection, out List2))
					throw new ArgumentException("Collection unknown: " + Collection, "Collection");

				if (!List2.TryGetValue(FieldCode, out Result))
					throw new ArgumentException("Field code unknown: " + FieldCode.ToString(), "FieldCode");
			}

			return Result;
		}

		#endregion

		#region Files

		/// <summary>
		/// Gets the 
		/// </summary>
		/// <param name="CollectionName"></param>
		/// <returns></returns>
		public ObjectBTreeFile GetFile(string CollectionName)
		{
			ObjectBTreeFile File;

			if (string.IsNullOrEmpty(CollectionName))
				CollectionName = this.defaultCollectionName;

			lock (this.files)
			{
				if (this.files.TryGetValue(CollectionName, out File))
					return File;

				string s = CollectionName;
				char[] ch = null;
				int i = 0;

				while ((i = s.IndexOfAny(forbiddenCharacters, i)) >= 0)
				{
					if (ch == null)
						ch = s.ToCharArray();

					ch[i] = '_';
				}

				if (ch != null)
					s = new string(ch);

				s = this.folder + s;

				File = new ObjectBTreeFile(s + ".btree", CollectionName, s + ".blob", this.blockSize, 1000, this.blobBlockSize, this,
					this.encoding, this.timeoutMilliseconds, this.encypted);

				// TODO: Centralize block cache.
				// TODO: Populate file with indices from master table.

				this.files[CollectionName] = File;
			}

			return File;
		}

		private static readonly char[] forbiddenCharacters = Path.GetInvalidFileNameChars();

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public async Task<T> LoadObject<T>(Guid ObjectId)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			return await File.LoadObject<T>(ObjectId);
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public Task<Guid> GetObjectId(object Value, bool InsertIfNotFound)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Value);
			return Serializer.GetObjectId(Value, InsertIfNotFound);
		}

		#endregion

		#region IDatabaseProvider

		/// <summary>
		/// Inserts an object into the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Insert(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object);
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			await File.SaveNewObject(Object, Serializer);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Insert(params object[] Objects)
		{
			foreach (object Object in Objects)
				await this.Insert(Object);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Insert(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				await this.Insert(Object);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			return await File.Find<T>(Offset, MaxCount, null, true, SortOrder);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			return await File.Find<T>(Offset, MaxCount, Filter, true, SortOrder);
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Update(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			return File.UpdateObject(Object, Serializer);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Update(params object[] Objects)
		{
			foreach (object Object in Objects)
				await this.Update(Object);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Update(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				await this.Update(Object);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public Task Delete(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = this.GetFile(Serializer.CollectionName);
			return File.DeleteObject(Object, Serializer);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Delete(params object[] Objects)
		{
			foreach (object Object in Objects)
				await this.Delete(Object);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Delete(IEnumerable<object> Objects)
		{
			foreach (object Object in Objects)
				await this.Delete(Object);
		}

		#endregion

	}
}

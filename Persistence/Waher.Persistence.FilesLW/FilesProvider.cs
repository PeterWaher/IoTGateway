using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Serialization.ReferenceTypes;
using Waher.Persistence.Files.Serialization.ValueTypes;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Delegate for embedded object value setter methods. Is used when loading embedded objects.
	/// </summary>
	/// <param name="EmbeddedObject">Embedded object.</param>
	public delegate void EmbeddedObjectSetter(object EmbeddedObject);

	/// <summary>
	/// Delegate for custom key callback methods.
	/// </summary>
	/// <param name="FileName">Name of file.</param>
	/// <param name="Key">Key to use</param>
	/// <param name="IV">Initiation vector to use</param>
	public delegate void CustomKeyHandler(string FileName, out byte[] Key, out byte[] IV);

	/// <summary>
	/// Index regeneration options.
	/// </summary>
	public enum RegenerationOptions
	{
		/// <summary>
		/// Don't regenerate index.
		/// </summary>
		DontRegenerate,

		/// <summary>
		/// Regenerate index if index file not found.
		/// </summary>
		RegenerateIfFileNotFound,

		/// <summary>
		/// Regenerate index if index object not instantiated.
		/// </summary>
		RegenerateIfIndexNotInstantiated,

		/// <summary>
		/// Regenerate file.
		/// </summary>
		Regenerate
	}

	/// <summary>
	/// Persists objects into binary files.
	/// </summary>
	public class FilesProvider : IDisposable, IDatabaseProvider
	{
		private static RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private Dictionary<Type, IObjectSerializer> serializers;
		private readonly Dictionary<string, ObjectBTreeFile> files = new Dictionary<string, ObjectBTreeFile>();
		private readonly Dictionary<string, LabelFile> labelFiles = new Dictionary<string, LabelFile>();
		private readonly Dictionary<ObjectBTreeFile, bool> hasUnsavedData = new Dictionary<ObjectBTreeFile, bool>();
		private AutoResetEvent serializerAdded = new AutoResetEvent(false);
		private StringDictionary master;
		private Cache<long, byte[]> blocks;
		private readonly object synchObj = new object();
		private readonly object synchObjNrFiles = new object();

		private readonly Encoding encoding;
		private readonly string id;
		private readonly string defaultCollectionName;
		private readonly string folder;
		private string autoRepairReportFolder = string.Empty;
		private readonly int blockSize;
		private readonly int blobBlockSize;
		private readonly int timeoutMilliseconds;
		private int nrFiles = 0;
		private int bulkCount = 0;
		private readonly bool debug;
		private readonly bool encrypted;
		private readonly CustomKeyHandler customKeyMethod;
#if NETSTANDARD1_5
		private readonly bool compiled;
		private bool deleteObsoleteKeys = true;
#endif

		#region Constructors

#if NETSTANDARD1_5
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  Encrypted, false, true, null)
		{
		}
#endif
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="CustomKeyMethod">Custom method to get keys for encrypted files. (Implies encrypted files)</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  !(CustomKeyMethod is null), false, true, CustomKeyMethod)
		{
		}

#if NETSTANDARD1_5
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Debug)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  Encrypted, Debug, true, null)
		{
		}
#endif
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="CustomKeyMethod">Custom method to get keys for encrypted files. (Implies encrypted files)</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod, bool Debug)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  !(CustomKeyMethod is null), Debug, true, CustomKeyMethod)
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
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				false, false, false, null)
		{
		}

#if NETSTANDARD1_5
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Debug, bool Compiled)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  Encrypted, Debug, Compiled, null)
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
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="CustomKeyMethod">Custom method to get keys for encrypted files. (Implies encrypted files)</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod, bool Debug, bool Compiled)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  !(CustomKeyMethod is null), Debug, Compiled, CustomKeyMethod)
		{
		}
#else
		/// <summary>
		/// Persists objects into binary files.
		/// </summary>
		/// <param name="Folder">Folder to store data files.</param>
		/// <param name="DefaultCollectionName">Default collection name.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache. This cache is used by all files governed by the
		/// database provider. The cache does not contain BLOB blocks.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Debug">If the provider is run in debug mode.</param>
		public FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Debug)
			: this(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds, 
				  false, Debug, false, null)
		{
		}
#endif

		private FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Debug, bool Compiled, CustomKeyHandler CustomKeyMethod)
		{
			ObjectBTreeFile.CheckBlockSizes(BlockSize, BlobBlockSize);

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentOutOfRangeException("The timeout must be positive.", nameof(TimeoutMilliseconds));

			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.defaultCollectionName = DefaultCollectionName;
			this.folder = Path.GetFullPath(Folder);
			this.debug = Debug;
			this.blockSize = BlockSize;
			this.blobBlockSize = BlobBlockSize;
			this.encoding = Encoding;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.serializers = new Dictionary<Type, Serialization.IObjectSerializer>();
			this.encrypted = Encrypted;
			this.customKeyMethod = CustomKeyMethod;
#if NETSTANDARD1_5
			this.compiled = Compiled;
#endif

			if (!string.IsNullOrEmpty(this.folder) && this.folder[this.folder.Length - 1] != Path.DirectorySeparatorChar)
				this.folder += Path.DirectorySeparatorChar;

			ConstructorInfo DefaultConstructor;
			IObjectSerializer S;
			TypeInfo TI;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IObjectSerializer)))
			{
				TI = T.GetTypeInfo();
				if (TI.IsAbstract || TI.IsGenericTypeDefinition)
					continue;

				DefaultConstructor = null;

				try
				{
					foreach (ConstructorInfo CI in TI.DeclaredConstructors)
					{
						if (CI.IsPublic && CI.GetParameters().Length == 0)
						{
							DefaultConstructor = CI;
							break;
						}
					}

					if (DefaultConstructor is null)
						continue;

					S = DefaultConstructor.Invoke(Types.NoParameters) as IObjectSerializer;
					if (S is null)
						continue;
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

			this.blocks = new Cache<long, byte[]>(BlocksInCache, TimeSpan.MaxValue, TimeSpan.FromHours(1));

			this.master = new StringDictionary(this.folder + "Files.master", string.Empty, string.Empty, this, false);

			Wait(this.GetFile(this.defaultCollectionName), this.timeoutMilliseconds);
			Wait(this.LoadConfiguration(), this.timeoutMilliseconds);
		}

		private static readonly char[] CRLF = new char[] { '\r', '\n' };

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
		/// Folder for Auto-Repair reports. If empty or null, <see cref="Folder"/>\AutoRepair will be used. 
		/// </summary>
		public string AutoRepairReportFolder
		{
			get => this.autoRepairReportFolder;
			set => this.autoRepairReportFolder = value;
		}

		/// <summary>
		/// An ID of the files provider. It's unique, and constant during the life-time of the FilesProvider class.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Number of bytes used by an Object ID.
		/// </summary>
		public int ObjectIdByteCount
		{
			get => 16;
		}

		/// <summary>
		/// Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> will be persisted as BLOBs, with the bulk of the object stored as separate files. 
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
			get { return this.encrypted; }
		}

#if NETSTANDARD1_5
		/// <summary>
		/// If object serializers should be compiled or not.
		/// </summary>
		public bool Compiled
		{
			get { return this.compiled; }
		}

		/// <summary>
		/// If old keys are to be removed, before a new encrypted file is created. (Default=true).
		/// </summary>
		public bool DeleteObsoleteKeys
		{
			get => this.deleteObsoleteKeys;
			set => this.deleteObsoleteKeys = value;
		}
#endif
		/// <summary>
		/// If the provider is run in debug mode.
		/// </summary>
		public bool Debug
		{
			get { return this.debug; }
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.master?.Dispose();
			this.master = null;

			if (!(this.files is null))
			{
				foreach (ObjectBTreeFile File in this.files.Values)
					File?.Dispose();

				this.files.Clear();
			}

			if (!(this.labelFiles is null))
			{
				foreach (LabelFile File in this.labelFiles.Values)
					File.Dispose();

				this.labelFiles.Clear();
			}

			this.blocks?.Dispose();
			this.blocks = null;

			if (!(this.serializers is null))
			{
				IDisposable d;

				foreach (IObjectSerializer Serializer in this.serializers.Values)
				{
					d = Serializer as IDisposable;
					if (!(d is null))
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

			this.serializerAdded?.Dispose();
			this.serializerAdded = null;
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
				case ObjectSerializer.TYPE_DATETIMEOFFSET: return typeof(DateTimeOffset);
				case ObjectSerializer.TYPE_TIMESPAN: return typeof(TimeSpan);
				case ObjectSerializer.TYPE_CHAR: return typeof(char);
				case ObjectSerializer.TYPE_STRING: return typeof(string);
				case ObjectSerializer.TYPE_CI_STRING: return typeof(CaseInsensitiveString);
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
		/// <param name="Value">Field data value.</param>
		/// <returns>Corresponding data type code.</returns>
		public static uint GetFieldDataTypeCode(object Value)
		{
			if (Value is null)
				return ObjectSerializer.TYPE_NULL;
			else
				return GetFieldDataTypeCode(Value.GetType());
		}

		/// <summary>
		/// Returns the type code corresponding to a given field data type.
		/// </summary>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Corresponding data type code.</returns>
		public static uint GetFieldDataTypeCode(Type FieldDataType)
		{
			TypeInfo TI = FieldDataType.GetTypeInfo();
			if (TI.IsEnum)
			{
				if (TI.IsDefined(typeof(FlagsAttribute), false))
					return ObjectSerializer.TYPE_INT32;
				else
					return ObjectSerializer.TYPE_ENUM;
			}

			if (FieldDataType == typeof(bool))
				return ObjectSerializer.TYPE_BOOLEAN;
			else if (FieldDataType == typeof(byte))
				return ObjectSerializer.TYPE_BYTE;
			else if (FieldDataType == typeof(short))
				return ObjectSerializer.TYPE_INT16;
			else if (FieldDataType == typeof(int))
				return ObjectSerializer.TYPE_INT32;
			else if (FieldDataType == typeof(long))
				return ObjectSerializer.TYPE_INT64;
			else if (FieldDataType == typeof(sbyte))
				return ObjectSerializer.TYPE_SBYTE;
			else if (FieldDataType == typeof(ushort))
				return ObjectSerializer.TYPE_UINT16;
			else if (FieldDataType == typeof(uint))
				return ObjectSerializer.TYPE_UINT32;
			else if (FieldDataType == typeof(ulong))
				return ObjectSerializer.TYPE_UINT64;
			else if (FieldDataType == typeof(decimal))
				return ObjectSerializer.TYPE_DECIMAL;
			else if (FieldDataType == typeof(double))
				return ObjectSerializer.TYPE_DOUBLE;
			else if (FieldDataType == typeof(float))
				return ObjectSerializer.TYPE_SINGLE;
			else if (FieldDataType == typeof(DateTime))
				return ObjectSerializer.TYPE_DATETIME;
			else if (FieldDataType == typeof(DateTimeOffset))
				return ObjectSerializer.TYPE_DATETIMEOFFSET;
			else if (FieldDataType == typeof(char))
				return ObjectSerializer.TYPE_CHAR;
			else if (FieldDataType == typeof(string))
				return ObjectSerializer.TYPE_STRING;
			else if (FieldDataType == typeof(CaseInsensitiveString))
				return ObjectSerializer.TYPE_CI_STRING;
			else if (FieldDataType == typeof(TimeSpan))
				return ObjectSerializer.TYPE_TIMESPAN;
			else if (FieldDataType == typeof(byte[]))
				return ObjectSerializer.TYPE_BYTEARRAY;
			else if (FieldDataType == typeof(Guid))
				return ObjectSerializer.TYPE_GUID;
			else if (FieldDataType == typeof(CaseInsensitiveString))
				return ObjectSerializer.TYPE_CI_STRING;
			else if (TI.IsArray)
				return ObjectSerializer.TYPE_ARRAY;
			else if (FieldDataType == typeof(void))
				return ObjectSerializer.TYPE_NULL;
			else
				return ObjectSerializer.TYPE_OBJECT;
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public IObjectSerializer GetObjectSerializer(Type Type)
		{
			IObjectSerializer Result;
			TypeInfo TI = Type.GetTypeInfo();

			lock (this.synchObj)
			{
				if (this.serializers.TryGetValue(Type, out Result))
					return Result;

				if (TI.IsEnum)
					Result = new EnumSerializer(Type);
				else if (Type.IsArray)
				{
					Type ElementType = Type.GetElementType();
					Type T = Types.GetType(typeof(ByteArraySerializer).FullName.Replace("ByteArray", "Array"));
					Type SerializerType = T.MakeGenericType(new Type[] { ElementType });
					Result = (IObjectSerializer)Activator.CreateInstance(SerializerType, this);
				}
				else if (TI.IsGenericType)
				{
					Type GT = Type.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Type NullableType = Type.GenericTypeArguments[0];

						if (NullableType.GetTypeInfo().IsEnum)
							Result = new Serialization.NullableTypes.NullableEnumSerializer(NullableType);
						else
							Result = null;
					}
					else
						Result = null;
				}
				else
					Result = null;

				if (!(Result is null))
				{
					this.serializers[Type] = Result;
					this.serializerAdded.Set();

					return Result;
				}
			}

			try
			{
#if NETSTANDARD1_5
				Result = new ObjectSerializer(Type, this, this.debug, this.compiled);
#else
				Result = new ObjectSerializer(Type, this, this.debug);
#endif
				lock (this.synchObj)
				{
					this.serializers[Type] = Result;
					this.serializerAdded.Set();
				}
			}
			catch (FileLoadException ex)
			{
				// Serializer in the process of being generated from another task or thread.

				while (true)
				{
					if (!this.serializerAdded.WaitOne(1000))
						ExceptionDispatchInfo.Capture(ex).Throw();

					lock (this.synchObj)
					{
						if (this.serializers.TryGetValue(Type, out Result))
							return Result;
					}
				}
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
			if (!(this.GetObjectSerializer(Type) is ObjectSerializer Serializer))
				throw new Exception("Objects of type " + Type.FullName + " must be embedded.");

			return Serializer;
		}

		internal static void Wait(Task Task, int TimeoutMilliseconds)
		{
			if (!Task.Wait(TimeoutMilliseconds))
				throw TimeoutException(null);
		}

		internal static TimeoutException TimeoutException(string Trace)
		{
			StringBuilder sb = new StringBuilder();
			string s;

			sb.Append("Unable to get access to underlying database.");

			if (!(Trace is null))
			{
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Database locked from:");
				sb.AppendLine();

				foreach (string Frame in Trace.Split(CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					s = Frame.TrimStart();
					if (s.Contains(" System.Runtime.CompilerServices") || s.Contains(" System.Threading"))
						continue;

					sb.AppendLine(s);
				}
			}

			return new TimeoutException(sb.ToString());
		}

		#endregion

		#region Fields

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public ulong GetFieldCode(string Collection, string FieldName)
		{
			Task<ulong> T = this.GetFieldCodeAsync(Collection, FieldName);
			FilesProvider.Wait(T, this.timeoutMilliseconds);
			return T.Result;
		}

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public async Task<ulong> GetFieldCodeAsync(string Collection, string FieldName)
		{
			if (string.IsNullOrEmpty(Collection))
				Collection = this.defaultCollectionName;

			LabelFile Labels;

			lock (this.files)
			{
				if (!this.labelFiles.TryGetValue(Collection, out Labels))
					Labels = null;
			}

			if (Labels is null)
			{
				await this.GetFile(Collection);     // Generates structures.

				lock (this.files)
				{
					Labels = this.labelFiles[Collection];
				}
			}

			return await Labels.GetFieldCodeAsync(FieldName);
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
			Task<string> T = this.GetFieldNameAsync(Collection, FieldCode);
			FilesProvider.Wait(T, this.timeoutMilliseconds);
			return T.Result;
		}

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public async Task<string> GetFieldNameAsync(string Collection, ulong FieldCode)
		{
			if (FieldCode > uint.MaxValue)
				throw new ArgumentOutOfRangeException("Field code too large.", nameof(FieldCode));

			if (string.IsNullOrEmpty(Collection))
				Collection = this.defaultCollectionName;

			LabelFile Labels;

			lock (this.files)
			{
				if (!this.labelFiles.TryGetValue(Collection, out Labels))
					Labels = null;
			}

			if (Labels is null)
			{
				await this.GetFile(Collection);     // Generates structures.

				lock (this.files)
				{
					Labels = this.labelFiles[Collection];
				}
			}

			return await Labels.GetFieldNameAsync((uint)FieldCode);
		}

		#endregion

		#region Keys

		internal void GetKeys(string FileName, bool FileExists, out byte[] Key, out byte[] IV)
		{
#if NETSTANDARD1_5
			if (!(this.customKeyMethod is null))
			{
#endif
				this.customKeyMethod(FileName, out Key, out IV);

				using (SHA256 Sha256 = SHA256.Create())
				{
					Key = Sha256.ComputeHash(Key);
					IV = Sha256.ComputeHash(IV);
				}
#if NETSTANDARD1_5
			}
			else
			{
				RSACryptoServiceProvider rsa = null;
				CspParameters CspParams = new CspParameters()
				{
					Flags =
						CspProviderFlags.UseMachineKeyStore |
						CspProviderFlags.NoPrompt |
						CspProviderFlags.UseExistingKey,
					KeyContainerName = FileName
				};

				RSAParameters Parameters;
				int KeyGenMode = 0;

				try
				{
					rsa = new RSACryptoServiceProvider(CspParams);
					if (!FileExists)
					{
						if (this.deleteObsoleteKeys)
							rsa.PersistKeyInCsp = false;    // Deletes key.
					}
					else if (rsa.KeySize > 1024)
						KeyGenMode |= 1;
					else if (rsa.KeySize == 768)
						KeyGenMode |= 2;
				}
				catch (CryptographicException ex)
				{
					if (FileExists)
					{
						throw new CryptographicException("Unable to get access to cryptographic key for database file " + FileName +
							". (" + ex.Message + ") Was the database created using another user?", ex);
					}
				}

				if (!FileExists)
				{
					rsa?.Dispose();
					rsa = null;

					CspParams.Flags =
						CspProviderFlags.UseMachineKeyStore |
						CspProviderFlags.NoPrompt;

					try
					{
						rsa = new RSACryptoServiceProvider(768, CspParams);

						if ((KeyGenMode & 2) == 0)
						{
							Parameters = rsa.ExportParameters(true);

							byte[] P, Q;

							lock (rnd)
							{
								P = new byte[48];
								rnd.GetBytes(P);

								Q = new byte[48];
								rnd.GetBytes(Q);
							}

							P[47] = 1;
							Q[47] = 1;

							Parameters.P = P;
							Parameters.Q = Q;

							rsa.ImportParameters(Parameters);

							Console.Out.WriteLine(FileName);

							KeyGenMode |= 2;
						}
					}
					catch (CryptographicException)
					{
						try
						{
							rsa = new RSACryptoServiceProvider(4096, CspParams);
						}
						catch (CryptographicException)
						{
							try
							{
								rsa = new RSACryptoServiceProvider(CspParams);
							}
							catch (CryptographicException ex2)
							{
								throw new CryptographicException("Unable to get access to cryptographic key for database file " + FileName +
									". (" + ex2.Message + ") Was the database created using another user?", ex2);
							}
						}

						if (rsa.KeySize > 1024)
							KeyGenMode |= 1;
					}
				}

				Parameters = rsa.ExportParameters(true);

				rsa.Dispose();
				rsa = null;


				if ((KeyGenMode & 2) != 0)
				{
					Key = Parameters.P;
					IV = Parameters.Q;

					Array.Resize<byte>(ref Key, 32);
					Array.Resize<byte>(ref IV, 32);
				}
				else
				{
					using (SHA256 Sha256 = SHA256.Create())
					{
						if ((KeyGenMode & 1) != 0)
						{
							int pLen = Parameters.P.Length;
							int qLen = Parameters.Q.Length;
							byte[] Bin = new byte[pLen + qLen];

							Array.Copy(Parameters.P, 0, Bin, 0, pLen);
							Array.Copy(Parameters.Q, 0, Bin, pLen, qLen);

							Key = Sha256.ComputeHash(Bin);
							IV = Sha256.ComputeHash(Parameters.Modulus);
						}
						else
						{
							IV = Sha256.ComputeHash(Parameters.P);
							Key = Sha256.ComputeHash(Parameters.Q);
						}
					}
				}
			}
#endif
		}

		#endregion

		#region Blocks

		/// <summary>
		/// Removes all blocks pertaining to a specific file.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		internal void RemoveBlocks(int FileId)
		{
			long Min = this.GetBlockKey(FileId, 0);
			long Max = this.GetBlockKey(FileId, uint.MaxValue);

			foreach (long Key in this.blocks.GetKeys())
			{
				if (Key >= Min && Key <= Max)
					this.blocks.Remove(Key);
			}
		}

		/// <summary>
		/// Removes a particular block from the cache.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		/// <param name="BlockIndex">Block index.</param>
		internal void RemoveBlock(int FileId, uint BlockIndex)
		{
			this.blocks.Remove(this.GetBlockKey(FileId, BlockIndex));
		}

		/// <summary>
		/// Calculates a block key value.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		/// <param name="BlockIndex">Block index.</param>
		/// <returns>Key value.</returns>
		private long GetBlockKey(int FileId, uint BlockIndex)
		{
			long Key = FileId;
			Key <<= 32;
			Key += BlockIndex;

			return Key;
		}

		/// <summary>
		/// Tries to get a cached block.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		/// <param name="BlockIndex">Block index.</param>
		/// <param name="Block">Cached block, if found.</param>
		/// <returns>If block was found in cache.</returns>
		internal bool TryGetBlock(int FileId, uint BlockIndex, out byte[] Block)
		{
			return this.blocks.TryGetValue(this.GetBlockKey(FileId, BlockIndex), out Block);
		}

		/// <summary>
		/// Adds a block to the cache.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		/// <param name="BlockIndex">Block index.</param>
		/// <param name="Block">Block.</param>
		internal void AddBlockToCache(int FileId, uint BlockIndex, byte[] Block)
		{
			this.blocks.Add(this.GetBlockKey(FileId, BlockIndex), Block);
		}

		/// <summary>
		/// Starts bulk-proccessing of data. Must be followed by a call to <see cref="EndBulk"/>.
		/// </summary>
		public Task StartBulk()
		{
			lock (this.synchObj)
			{
				this.bulkCount++;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Ends bulk-processing of data. Must be called once for every call to <see cref="StartBulk"/>.
		/// </summary>
		public Task EndBulk()
		{
			lock (this.synchObj)
			{
				if (this.bulkCount <= 0)
					throw new InvalidOperationException("Not in bulk mode.");

				this.bulkCount--;
				if (this.bulkCount > 0)
					return Task.CompletedTask;
			}

			return this.SaveUnsaved();
		}

		private async Task SaveUnsaved()
		{
			ObjectBTreeFile[] Files;

			lock (this.synchObj)
			{
				int c = this.hasUnsavedData.Count;
				if (c == 0)
					return;

				Files = new ObjectBTreeFile[c];
				this.hasUnsavedData.Keys.CopyTo(Files, 0);
				this.hasUnsavedData.Clear();
			}

			foreach (ObjectBTreeFile File in Files)
			{
				await File.LockWrite();
				await File.EndWrite();   // Saves unsaved data.
			}
		}

		internal bool InBulkMode(ObjectBTreeFile Caller)
		{
			lock (this.synchObj)
			{
				if (this.bulkCount <= 0)
					return false;
				else
				{
					this.hasUnsavedData[Caller] = true;
					return true;
				}
			}
		}

		#endregion

		#region Files

		/// <summary>
		/// Gets a new file ID.
		/// </summary>
		/// <returns></returns>
		internal int GetNewFileId()
		{
			lock (this.synchObjNrFiles)
			{
				return this.nrFiles++;
			}
		}

		/// <summary>
		/// Gets the BTree file corresponding to a named collection.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <returns>BTree file corresponding to the given collection.</returns>
		public Task<ObjectBTreeFile> GetFile(string CollectionName)
		{
			return this.GetFile(CollectionName, true);
		}

		/// <summary>
		/// Gets the BTree file corresponding to a named collection.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="CreateIfNotExists">If the physical file should be created if one does not already exist.</param>
		/// <returns>BTree file corresponding to the given collection. 
		/// If file did not exist, and <paramref name="CreateIfNotExists"/> is false, null is returned.</returns>
		public async Task<ObjectBTreeFile> GetFile(string CollectionName, bool CreateIfNotExists)
		{
			ObjectBTreeFile File;

			if (string.IsNullOrEmpty(CollectionName))
				CollectionName = this.defaultCollectionName;

			string s = this.GetFileName(CollectionName);

			if (!CreateIfNotExists && !System.IO.File.Exists(Path.GetFullPath(s + ".btree")))
				return null;

			bool Wait;

			do
			{
				Wait = false;

				lock (this.files)
				{
					if (this.files.TryGetValue(CollectionName, out File))
					{
						if (File is null)
							Wait = true;
						else
							return File;
					}
					else
						this.files[CollectionName] = null;
				}

				if (Wait)
					await Task.Delay(100);
			}
			while (Wait);

			LabelFile Labels = await LabelFile.Create(CollectionName, this.timeoutMilliseconds, this.encrypted, this);

			File = new ObjectBTreeFile(s + ".btree", CollectionName, s + ".blob", this.blockSize, this.blobBlockSize,
				this, this.encoding, this.timeoutMilliseconds, this.encrypted);

			lock (this.files)
			{
				this.files[CollectionName] = File;
				this.labelFiles[CollectionName] = Labels;
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Collection");
			sb.AppendLine(CollectionName);

			KeyValuePair<bool, KeyValuePair<string, object>> P2 = await this.master.TryGetValueAsync(File.FileName);
			string s2 = sb.ToString();

			if (!P2.Key || !(P2.Value.Value is string) || ((string)P2.Value.Value) != s2)
				await this.master.AddAsync(File.FileName, s2, true);

			await this.GetFieldCodeAsync(null, CollectionName);

			return File;
		}

		/// <summary>
		/// Tries to get the labels file for a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="Labels">Labels file, if found.</param>
		/// <returns>If a labels dictionary was found for the given collection.</returns>
		public bool TryGetLabelsFile(string CollectionName, out LabelFile Labels)
		{
			lock (this.files)
			{
				return this.labelFiles.TryGetValue(CollectionName, out Labels);
			}
		}

		/// <summary>
		/// Gets an index file.
		/// </summary>
		/// <param name="File">Object file.</param>
		/// <param name="RegenerationOptions">Index regeneration options.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <returns>Index file.</returns>
		public async Task<IndexBTreeFile> GetIndexFile(ObjectBTreeFile File, RegenerationOptions RegenerationOptions, params string[] FieldNames)
		{
			IndexBTreeFile IndexFile;
			IndexBTreeFile[] Indices = File.Indices;
			string[] Fields;
			int i, c;
			bool Regenerate = (RegenerationOptions == RegenerationOptions.Regenerate);

			foreach (IndexBTreeFile I in Indices)
			{
				if ((c = (Fields = I.FieldNames).Length) != FieldNames.Length)
					continue;

				for (i = 0; i < c; i++)
				{
					if (Fields[i] != FieldNames[i])
						break;
				}

				if (i < c)
					continue;

				if (Regenerate)
					await I.Regenerate();

				return I;
			}

			if (RegenerationOptions == RegenerationOptions.RegenerateIfIndexNotInstantiated)
				Regenerate = true;

			StringBuilder sb = new StringBuilder();

			string s;
			bool Exists;

#if NETSTANDARD1_5
			byte[] Hash;

			foreach (string FieldName in FieldNames)
			{
				sb.Append('.');
				sb.Append(FieldName);
			}

			s = File.FileName + sb.ToString() + ".index";
			Exists = System.IO.File.Exists(s);

			if (Exists)     // Index file named using the Waher.Pesistence.FilesLW library.
				sb.Insert(0, File.FileName);
			else
			{
				using (SHA1 Sha1 = SHA1.Create())
				{
					Hash = Sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
				}

				sb.Clear();

				sb.Append(File.FileName);
				sb.Append('.');

				foreach (byte b in Hash)
					sb.Append(b.ToString("x2"));
			}
#else
			sb.Append(File.FileName);

			foreach (string FieldName in FieldNames)
			{
				sb.Append('.');
				sb.Append(FieldName);
			}
#endif
			sb.Append(".index");

			s = sb.ToString();
			Exists = System.IO.File.Exists(s);

			if (!Exists && RegenerationOptions == RegenerationOptions.RegenerateIfFileNotFound)
				Regenerate = true;

			lock (this.synchObj)
			{
				IndexFile = new IndexBTreeFile(s, File, this, FieldNames);
			}

			await File.AddIndex(IndexFile, Regenerate);

			sb.Clear();

			sb.AppendLine("Index");
			sb.AppendLine(File.CollectionName);

			foreach (string FieldName in FieldNames)
				sb.AppendLine(FieldName);

			if (s.StartsWith(this.folder))
				s = s.Substring(this.folder.Length);

			KeyValuePair<bool, KeyValuePair<string, object>> P = await this.master.TryGetValueAsync(s);
			string s2 = sb.ToString();

			if (!P.Key || !(P.Value.Value is string) || ((string)P.Value.Value) != s2)
				await this.master.AddAsync(s, s2, true);

			return IndexFile;
		}

		/// <summary>
		/// Closes files related to a collection.
		/// </summary>
		/// <param name="CollectionName">Collection.</param>
		/// <returns>If a collection with the given name was found and closed.</returns>
		public bool CloseFile(string CollectionName)
		{
			ObjectBTreeFile File;
			LabelFile Labels;

			if (string.IsNullOrEmpty(CollectionName))
				CollectionName = this.defaultCollectionName;

			lock (this.files)
			{
				if (!this.files.TryGetValue(CollectionName, out File))
					return false;

				this.files.Remove(CollectionName);

				Labels = this.labelFiles[CollectionName];
				this.labelFiles.Remove(CollectionName);
			}

			int FileId = File.Id;

			File.Dispose();
			Labels.Dispose();

			this.RemoveBlocks(FileId);

			return true;
		}

		/// <summary>
		/// Gets the file name root that corresponds to a given collection.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <returns>File name root.</returns>
		public string GetFileName(string CollectionName)
		{
			string s = CollectionName;
			char[] ch = null;
			int i = 0;

			while ((i = s.IndexOfAny(forbiddenCharacters, i)) >= 0)
			{
				if (ch is null)
					ch = s.ToCharArray();

				ch[i] = '_';
			}

			if (!(ch is null))
				s = new string(ch);

			s = this.folder + s;

			return s;
		}

		private static readonly char[] forbiddenCharacters = Path.GetInvalidFileNameChars();

		/// <summary>
		/// Loads the configuration from the master file.
		/// </summary>
		/// <returns>Task object</returns>
		private async Task LoadConfiguration()
		{
			LinkedList<string> ToRemove = null;

			foreach (KeyValuePair<string, object> P in this.master)
			{
				string s = P.Value.ToString();
				string[] Rows = s.Split(CRLF, StringSplitOptions.RemoveEmptyEntries);

				switch (Rows[0])
				{
					case "Collection":
						if (Rows.Length < 2)
							break;

						string CollectionName = Rows[1];
						ObjectBTreeFile File = await this.GetFile(CollectionName, false);

						if (File is null)
						{
							if (ToRemove is null)
								ToRemove = new LinkedList<string>();

							ToRemove.AddLast(P.Key);
						}
						break;

					case "Index":
						if (Rows.Length < 3)
							break;

						CollectionName = Rows[1];
						string[] FieldNames = new string[Rows.Length - 2];
						Array.Copy(Rows, 2, FieldNames, 0, Rows.Length - 2);

						File = await this.GetFile(CollectionName, false);
						if (File is null)
						{
							if (ToRemove is null)
								ToRemove = new LinkedList<string>();

							ToRemove.AddLast(P.Key);
							break;
						}

						IndexBTreeFile Index = await this.GetIndexFile(File,
							RegenerationOptions.RegenerateIfFileNotFound, FieldNames);

						break;
				}
			}

			if (!(ToRemove is null))
			{
				foreach (string s in ToRemove)
					await this.master.RemoveAsync(s);
			}
		}

		#endregion

		#region Objects

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> LoadObject<T>(object ObjectId)
		{
			Guid OID;

			if (ObjectId is Guid)
				OID = (Guid)ObjectId;
			else if (ObjectId is string)
				OID = new Guid((string)ObjectId);
			else if (ObjectId is byte[])
				OID = new Guid((byte[])ObjectId);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.LoadObject<T>(OID);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> LoadObject<T>(Guid ObjectId)
		{
			return this.LoadObject<T>(ObjectId, null);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<T> LoadObject<T>(Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(null));

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return (T)await File.LoadObjectLocked(ObjectId, Serializer);
					}
					finally
					{
						await File.EndRead();
					}
				}
				else
				{
					File.QueueForLoad(ObjectId, Serializer, EmbeddedSetter);
					return default;
				}
			}
			else
				return (T)await File.LoadObject(ObjectId, Serializer);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <paramref name="T"/>.
		/// </summary>
		/// <param name="T">Base type.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<object> LoadObject(Type T, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(T);
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(null));

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return await File.LoadObjectLocked(ObjectId, Serializer);
					}
					finally
					{
						await File.EndRead();
					}
				}
				else
				{
					File.QueueForLoad(ObjectId, Serializer, EmbeddedSetter);
					return null;
				}
			}
			else
				return await File.LoadObject(ObjectId, Serializer);
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
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(Object));
			await File.SaveNewObject(Object, Serializer);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(params object[] Objects)
		{
			return this.Insert((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Insert(IEnumerable<object> Objects)
		{
			LinkedList<object> List = new LinkedList<object>();
			ObjectSerializer Serializer = null;
			ObjectBTreeFile File = null;
			string CollectionName = null;
			string CollectionName2;
			Type T = null;
			Type T2;

			foreach (object Object in Objects)
			{
				T2 = Object.GetType();
				if (Serializer is null || T != T2)
				{
					if (!(List.First is null))
					{
						await File.SaveNewObjects(List, Serializer);
						List.Clear();
					}

					T = T2;
					Serializer = this.GetObjectSerializerEx(Object);
				}

				CollectionName2 = Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (!(List.First is null))
					{
						await File.SaveNewObjects(List, Serializer);
						List.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
				}

				List.AddLast(Object);
			}

			if (!(List.First is null))
				await File.SaveNewObjects(List, Serializer);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(null));
			using (ICursor<T> ResultSet = await File.Find<T>(Offset, MaxCount, null, true, SortOrder))
			{
				return await this.LoadAll<T>(ResultSet);
			}
		}

		private async Task<IEnumerable<T>> LoadAll<T>(ICursor<T> ResultSet)
		{
			LinkedList<T> Result = new LinkedList<T>();

			while (await ResultSet.MoveNextAsync())
			{
				if (ResultSet.CurrentTypeCompatible)
					Result.AddLast(ResultSet.Current);
			}

			return Result;
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
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(null));
			using (ICursor<T> ResultSet = await File.Find<T>(Offset, MaxCount, Filter, true, SortOrder))
			{
				return await this.LoadAll<T>(ResultSet);
			}
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Update(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(Object));
			await File.UpdateObject(Object, Serializer);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(params object[] Objects)
		{
			return this.Update((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Update(IEnumerable<object> Objects)
		{
			LinkedList<object> List = new LinkedList<object>();
			LinkedList<Guid> ObjectIds = new LinkedList<Guid>();
			ObjectSerializer Serializer = null;
			ObjectBTreeFile File = null;
			string CollectionName = null;
			string CollectionName2;
			Type T = null;
			Type T2;

			foreach (object Object in Objects)
			{
				T2 = Object.GetType();
				if (Serializer is null || T != T2)
				{
					if (!(List.First is null))
					{
						await File.UpdateObjects(ObjectIds, List, Serializer);
						List.Clear();
						ObjectIds.Clear();
					}

					T = T2;
					Serializer = this.GetObjectSerializerEx(Object);
				}

				CollectionName2 = Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (!(List.First is null))
					{
						await File.UpdateObjects(ObjectIds, List, Serializer);
						List.Clear();
						ObjectIds.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
				}

				ObjectIds.AddLast(await Serializer.GetObjectId(Object, false));
				List.AddLast(Object);
			}

			if (!(List.First is null))
				await File.UpdateObjects(ObjectIds, List, Serializer);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Delete(object Object)
		{
			ObjectSerializer Serializer = this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(Serializer.CollectionName(Object));
			await File.DeleteObject(Object, Serializer);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(params object[] Objects)
		{
			return this.Delete((IEnumerable<object>)Objects);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public async Task Delete(IEnumerable<object> Objects)
		{
			LinkedList<Guid> List = new LinkedList<Guid>();
			ObjectSerializer Serializer = null;
			ObjectBTreeFile File = null;
			string CollectionName = null;
			string CollectionName2;
			Type T = null;
			Type T2;

			foreach (object Object in Objects)
			{
				T2 = Object.GetType();
				if (Serializer is null || T != T2)
				{
					if (!(List.First is null))
					{
						await File.DeleteObjects(List, Serializer);
						List.Clear();
					}

					T = T2;
					Serializer = this.GetObjectSerializerEx(Object);
				}

				CollectionName2 = Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (!(List.First is null))
					{
						await File.DeleteObjects(List, Serializer);
						List.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
				}

				List.AddLast(await Serializer.GetObjectId(Object, false));
			}

			if (!(List.First is null))
				await File.DeleteObjects(List, Serializer);
		}

		#endregion

		#region Export

		/// <summary>
		/// Exports the database to XML.
		/// </summary>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Graph XML.</returns>
		public async Task<string> ExportXml(bool Properties)
		{
			StringBuilder Output = new StringBuilder();
			await this.ExportXml(Output, Properties);
			return Output.ToString();
		}

		/// <summary>
		/// Exports the database to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportXml(StringBuilder Output, bool Properties)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter w = XmlWriter.Create(Output, Settings))
			{
				await this.ExportXml(w, Properties);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the database to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Asynhronous task object.</returns>
		public async Task ExportXml(XmlWriter Output, bool Properties)
		{
			Output.WriteStartElement("Database", "http://waher.se/Schema/Persistence/Files.xsd");

			foreach (ObjectBTreeFile File in this.Files)
				await File.ExportGraphXML(Output, Properties);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Available Object files.
		/// </summary>
		public ObjectBTreeFile[] Files
		{
			get
			{
				List<ObjectBTreeFile> Files = new List<ObjectBTreeFile>();

				lock (this.files)
				{
					foreach (ObjectBTreeFile File in this.files.Values)
					{
						if (!(File is null))
							Files.Add(File);
					}
				}

				return Files.ToArray();
			}
		}

		/// <summary>
		/// Performs an export of the entire database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public async Task Export(IDatabaseExport Output)
		{
			ObjectBTreeFile[] Files = this.Files;

			await Output.StartExport();
			try
			{
				foreach (ObjectBTreeFile File in Files)
				{
					await Output.StartCollection(File.CollectionName);
					try
					{
						foreach (IndexBTreeFile Index in File.Indices)
						{
							await Output.StartIndex();

							string[] FieldNames = Index.FieldNames;
							bool[] Ascending = Index.Ascending;
							int i, c = Math.Min(FieldNames.Length, Ascending.Length);

							for (i = 0; i < c; i++)
								await Output.ReportIndexField(FieldNames[i], Ascending[i]);

							await Output.EndIndex();
						}

						using (ObjectBTreeFileEnumerator<GenericObject> e = await File.GetTypedEnumeratorAsync<GenericObject>(true))
						{
							GenericObject Obj;

							while (await e.MoveNextAsync())
							{
								if (e.CurrentTypeCompatible)
								{
									Obj = e.Current;

									await Output.StartObject(Obj.ObjectId.ToString(), Obj.TypeName);
									try
									{
										foreach (KeyValuePair<string, object> P in Obj)
											await Output.ReportProperty(P.Key, P.Value);
									}
									catch (Exception ex)
									{
										this.ReportException(ex, Output);
									}
									finally
									{
										await Output.EndObject();
									}
								}
								else if (!(e.CurrentObjectId is null))
									await Output.ReportError("Unable to load object " + e.CurrentObjectId.ToString() + ".");
							}
						}
					}
					catch (Exception ex)
					{
						this.ReportException(ex, Output);
					}
					finally
					{
						await Output.EndCollection();
					}
				}
			}
			catch (Exception ex)
			{
				this.ReportException(ex, Output);
			}
			finally
			{
				await Output.EndExport();
			}
		}

		private void ReportException(Exception ex, IDatabaseExport Output)
		{
			ex = Waher.Events.Log.UnnestException(ex);

			if (ex is AggregateException ex2)
			{
				foreach (Exception ex3 in ex2.InnerExceptions)
					Output.ReportException(ex3);
			}
			else
				Output.ReportException(ex);
		}

		/// <summary>
		/// Clears a collection of all objects.
		/// </summary>
		/// <param name="CollectionName">Name of collection to clear.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public async Task Clear(string CollectionName)
		{
			ObjectBTreeFile File = await this.GetFile(CollectionName);
			await File.ClearAsync();
		}

		#endregion

		#region Analyze

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, false);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		public Task Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, true);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		public async Task Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			Output.WriteStartDocument();

			if (!string.IsNullOrEmpty(XsltPath))
				Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + Encode(XsltPath) + "\"");

			Output.WriteStartElement("DatabaseStatistics", "http://waher.se/Schema/Persistence/Statistics.xsd");

			foreach (ObjectBTreeFile File in this.Files)
			{
				KeyValuePair<FileStatistics, Dictionary<Guid, bool>> P = await File.ComputeStatistics();
				FileStatistics Stat = P.Key;
				Dictionary<Guid, bool> ObjectIds = P.Value;

				if (Repair && Stat.IsCorrupt)
				{
					LinkedList<Exception> Exceptions = null;
					string TempFileName = Path.GetTempFileName();
					string TempBtreeFileName = TempFileName + ".btree";
					string TempBlobFileName = TempFileName + ".blob";

					using (ObjectBTreeFile TempFile = new ObjectBTreeFile(TempBtreeFileName, File.CollectionName, TempBlobFileName,
						File.BlockSize, File.BlobBlockSize, this, File.Encoding, File.TimeoutMilliseconds, File.Encrypted))
					{
						int c = 0;

						await this.StartBulk();
						try
						{
							for (uint BlockIndex = 0; BlockIndex < Stat.NrBlocks; BlockIndex++)
							{
								long PhysicalPosition = BlockIndex;
								PhysicalPosition *= this.blockSize;

								byte[] Block = await File.LoadBlockLocked(PhysicalPosition, false);
								BinaryDeserializer Reader = new BinaryDeserializer(File.CollectionName, this.encoding, Block, File.BlockLimit);
								BinaryDeserializer Reader2;
								BlockHeader Header = new BlockHeader(Reader);
								int Pos = 14;

								while (Reader.BytesLeft >= 4)
								{
									Reader.ReadBlockLink();                    // Block link.
									object ObjectId = File.RecordHandler.GetKey(Reader);
									if (ObjectId is null)
										break;

									uint Len = File.RecordHandler.GetFullPayloadSize(Reader);

									if (Len > 0)
									{
										int Pos2 = 0;

										if (Reader.Position - Pos - 4 + Len > File.InlineObjectSizeLimit)
										{
											Reader2 = null;

											try
											{
												Reader2 = await File.LoadBlobLocked(Block, Pos + 4, null, null);
												Reader2.Position = 0;
												Pos2 = Reader2.Data.Length;
											}
											catch (Exception)
											{
												Reader2 = null;
											}

											Len = 4;
											Reader.Position += (int)Len;
										}
										else
										{
											if (Len > Reader.BytesLeft)
												break;
											else
											{
												Reader.Position += (int)Len;
												Pos2 = Reader.Position;

												Reader2 = Reader;
												Reader2.Position = Pos + 4;
											}
										}

										if (!(Reader2 is null))
										{
											object Obj;
											int Len2;

											try
											{
												Obj = File.GenericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false, true);
												Len2 = Pos2 - Reader2.Position;
											}
											catch (Exception)
											{
												Len2 = 0;
												Obj = null;
											}

											if (Len2 != 0)
												break;

											if (ObjectId is Guid Guid && !ObjectIds.ContainsKey(Guid))
											{
												ObjectIds[Guid] = true;

												if (!(Obj is null))
												{
													try
													{
														await TempFile.SaveNewObject(Obj);
													}
													catch (Exception ex)
													{
														if (Exceptions is null)
															Exceptions = new LinkedList<Exception>();

														Exceptions.AddLast(ex);
														continue;
													}


													c++;
													if (c >= 1000)
													{
														await this.EndBulk();
														await this.StartBulk();
														c = 0;
													}
												}
											}
										}
									}

									Pos = Reader.Position;
								}
							}

							await this.EndBulk();
							await File.ClearAsync();
							await this.StartBulk();
							c = 0;

							using (ObjectBTreeFileEnumerator<object> e = await TempFile.GetTypedEnumeratorAsync<object>(true))
							{
								while (await e.MoveNextAsync())
								{
									if (e.CurrentTypeCompatible)
									{
										await File.SaveNewObject(e.Current);

										c++;
										if (c >= 1000)
										{
											await this.EndBulk();
											await this.StartBulk();
											c = 0;
										}
									}
								}
							}
						}
						finally
						{
							await this.EndBulk();
						}
					}

#if NETSTANDARD1_5
					if (File.Encrypted && this.customKeyMethod is null)
					{
						try
						{
							CspParameters Csp = new CspParameters()
							{
								KeyContainerName = TempBtreeFileName,
								Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey
							};

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(Csp))
							{
								RSA.PersistKeyInCsp = false;    // Deletes key.
							}

							Csp.KeyContainerName = TempBlobFileName;

							using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(Csp))
							{
								RSA.PersistKeyInCsp = false;    // Deletes key.
							}
						}
						catch (Exception)
						{
							// Ignore
						}
					}
#endif
					P = await File.ComputeStatistics();
					Stat = P.Key;
					ObjectIds = P.Value;

					Stat.LogComment("File was regenerated due to errors found.");

					if (!(Exceptions is null))
					{
						foreach (Exception ex in Exceptions)
							Stat.LogError(ex.Message);
					}

					string[] Files = Directory.GetFiles(Path.GetDirectoryName(TempFileName), Path.GetFileName(TempFileName) + "*.*");

					foreach (string FileName in Files)
					{
						try
						{
							System.IO.File.Delete(FileName);
						}
						catch (Exception)
						{
							// Ignore
						}
					}
				}

				Output.WriteStartElement("File");
				Output.WriteAttributeString("id", File.Id.ToString());
				Output.WriteAttributeString("collectionName", File.CollectionName);
				Output.WriteAttributeString("fileName", GetRelativePath(ProgramDataFolder, File.FileName));
				Output.WriteAttributeString("blockSize", File.BlockSize.ToString());
				Output.WriteAttributeString("blobFileName", GetRelativePath(ProgramDataFolder, File.BlobFileName));
				Output.WriteAttributeString("blobBlockSize", File.BlobBlockSize.ToString());
				Output.WriteAttributeString("count", File.Count.ToString());
				Output.WriteAttributeString("encoding", File.Encoding.WebName);
				Output.WriteAttributeString("encrypted", Encode(File.Encrypted));
				Output.WriteAttributeString("inlineObjectSizeLimit", File.InlineObjectSizeLimit.ToString());
				Output.WriteAttributeString("isReadOnly", Encode(File.IsReadOnly));
				Output.WriteAttributeString("timeoutMs", File.TimeoutMilliseconds.ToString());

				WriteStat(Output, Stat);

				foreach (IndexBTreeFile Index in File.Indices)
				{
					Output.WriteStartElement("Index");
					Output.WriteAttributeString("id", Index.IndexFile.Id.ToString());
					Output.WriteAttributeString("fileName", GetRelativePath(ProgramDataFolder, Index.IndexFile.FileName));
					Output.WriteAttributeString("blockSize", Index.IndexFile.BlockSize.ToString());
					Output.WriteAttributeString("blobFileName", Index.IndexFile.BlobFileName);
					Output.WriteAttributeString("blobBlockSize", Index.IndexFile.BlobBlockSize.ToString());
					Output.WriteAttributeString("count", Index.IndexFile.Count.ToString());
					Output.WriteAttributeString("encoding", Index.IndexFile.Encoding.WebName);
					Output.WriteAttributeString("encrypted", Encode(Index.IndexFile.Encrypted));
					Output.WriteAttributeString("inlineObjectSizeLimit", Index.IndexFile.InlineObjectSizeLimit.ToString());
					Output.WriteAttributeString("isReadOnly", Encode(Index.IndexFile.IsReadOnly));
					Output.WriteAttributeString("timeoutMs", Index.IndexFile.TimeoutMilliseconds.ToString());

					foreach (string Field in Index.FieldNames)
						Output.WriteElementString("Field", Field);

					Stat = await Index.ComputeStatistics(ObjectIds);

					if (Repair && Stat.IsCorrupt)
					{
						await Index.Regenerate();

						Stat = await Index.ComputeStatistics(ObjectIds);
						Stat.LogComment("Index was regenerated due to errors found.");
					}

					WriteStat(Output, Stat);

					Output.WriteEndElement();
				}

				if (ExportData)
					await File.ExportGraphXML(Output, true);

				Output.WriteEndElement();
			}

			Output.WriteEndElement();
			Output.WriteEndDocument();
		}

		private static string Encode(bool b)
		{
			return b ? "true" : "false";
		}

		private static string Encode(double d)
		{
			return d.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		private static string Encode(string s)
		{
			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		private static string GetRelativePath(string ProgramDataFolder, string Path)
		{
			if (Path.StartsWith(ProgramDataFolder))
			{
				Path = Path.Substring(ProgramDataFolder.Length);
				if (Path.StartsWith(new string(System.IO.Path.DirectorySeparatorChar, 1)))
					Path = Path.Substring(1);
			}

			return Path;
		}

		private static void WriteStat(XmlWriter w, FileStatistics Stat)
		{
			w.WriteStartElement("Stat");

			if (!double.IsNaN(Stat.AverageBytesUsedPerBlock))
				w.WriteAttributeString("avgBytesPerBlock", Encode(Stat.AverageBytesUsedPerBlock));

			if (!double.IsNaN(Stat.AverageObjectSize))
				w.WriteAttributeString("avgObjSize", Encode(Stat.AverageObjectSize));

			if (!double.IsNaN(Stat.AverageObjectsPerBlock))
				w.WriteAttributeString("avgObjPerBlock", Encode(Stat.AverageObjectsPerBlock));

			w.WriteAttributeString("hasComments", Encode(Stat.HasComments));
			w.WriteAttributeString("isBalanced", Encode(Stat.IsBalanced));
			w.WriteAttributeString("isCorrupt", Encode(Stat.IsCorrupt));
			w.WriteAttributeString("maxBytesPerBlock", Stat.MaxBytesUsedPerBlock.ToString());
			w.WriteAttributeString("maxDepth", Stat.MaxDepth.ToString());
			w.WriteAttributeString("maxObjSize", Stat.MaxObjectSize.ToString());
			w.WriteAttributeString("maxObjPerBlock", Stat.MaxObjectsPerBlock.ToString());
			w.WriteAttributeString("minBytesPerBlock", Stat.MinBytesUsedPerBlock.ToString());
			w.WriteAttributeString("minDepth", Stat.MinDepth.ToString());
			w.WriteAttributeString("minObjSize", Stat.MinObjectSize.ToString());
			w.WriteAttributeString("minObjPerBlock", Stat.MinObjectsPerBlock.ToString());
			w.WriteAttributeString("nrBlobBlocks", Stat.NrBlobBlocks.ToString());
			w.WriteAttributeString("nrBlobBytes", Stat.NrBlobBytesTotal.ToString());
			w.WriteAttributeString("nrBlobBytesUnused", Stat.NrBlobBytesUnused.ToString());
			w.WriteAttributeString("nrBlobBytesUsed", Stat.NrBlobBytesUsed.ToString());
			w.WriteAttributeString("nrBlocks", Stat.NrBlocks.ToString());
			w.WriteAttributeString("nrBytes", Stat.NrBytesTotal.ToString());
			w.WriteAttributeString("nrBytesUnused", Stat.NrBytesUnused.ToString());
			w.WriteAttributeString("nrBytesUsed", Stat.NrBytesUsed.ToString());
			w.WriteAttributeString("nrObjects", Stat.NrObjects.ToString());
			w.WriteAttributeString("usage", Encode(Stat.Usage));

			if (Stat.NrBlobBytesTotal > 0)
				w.WriteAttributeString("blobUsage", Encode((100.0 * Stat.NrBlobBytesUsed) / Stat.NrBlobBytesTotal));

			if (Stat.HasComments)
			{
				foreach (string Comment in Stat.Comments)
					w.WriteElementString("Comment", Comment);
			}

			w.WriteEndElement();
		}

		/// <summary>
		/// Checks if the database needs repairing. This is done by checking the last start and stop timetamps to detect
		/// inproper shutdowns.
		/// </summary>
		/// <param name="XsltPath">Path to optional XSLT file for the resulting report.</param>
		public async Task RepairIfInproperShutdown(string XsltPath)
		{
			string StartFileName = this.folder + "Start.txt";
			string StopFileName = this.folder + "Stop.txt";
			long? Start = null;
			long? Stop = null;
			string s;

			if (File.Exists(StartFileName))
			{
				s = File.ReadAllText(StartFileName);
				if (long.TryParse(s, out long l))
					Start = l;
			}

			if (File.Exists(StopFileName))
			{
				s = File.ReadAllText(StopFileName);
				if (long.TryParse(s, out long l))
					Stop = l;
			}

			if (!Start.HasValue || !Stop.HasValue || Start.Value > Stop.Value)
			{
				if (string.IsNullOrEmpty(this.autoRepairReportFolder))
					s = this.folder + "AutoRepair";
				else
					s = this.autoRepairReportFolder;

				if (!Directory.Exists(s))
					Directory.CreateDirectory(s);

				s = Path.Combine(s, "AutoRepair " + DateTime.Now.ToString("yyyy-MM-ddTHH.mm.ss.ffffff") + ".xml");

				XmlWriterSettings Settings = new XmlWriterSettings()
				{
					Encoding = Encoding.UTF8,
					Indent = true,
					IndentChars = "\t",
					NewLineChars = "\r\n",
					NewLineHandling = NewLineHandling.Entitize,
					NewLineOnAttributes = false,
					OmitXmlDeclaration = false,
					WriteEndDocumentOnClose = true,
					CloseOutput = true
				};

				using (FileStream fs = File.Create(s))
				{
					using (XmlWriter w = XmlWriter.Create(fs, Settings))
					{
						if (!Start.HasValue)
						{
							Log.Notice("First time checking database for errors.",
								string.Empty, string.Empty, "AutoRepair", new KeyValuePair<string, object>("Report", s));
						}
						else
						{
							Log.Warning("Service not properly terminated. Checking database for errors.",
								string.Empty, string.Empty, "AutoRepair", new KeyValuePair<string, object>("Report", s));
						}

						await this.Repair(w, XsltPath, this.folder, false);
					}
				}
			}
		}

		#endregion

		#region Indices

		/// <summary>
		/// Adds an index to a collection, if one does not already exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public async Task AddIndex(string CollectionName, string[] FieldNames)
		{
			ObjectBTreeFile File = await this.GetFile(CollectionName);
			await GetIndexFile(File, RegenerationOptions.RegenerateIfFileNotFound, FieldNames);
		}

		#endregion

		#region Life-Cycle

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		public Task Start()
		{
			WriteTimestamp("Start.txt");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		public Task Stop()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		public async Task Flush()
		{
			await this.SaveUnsaved();

			WriteTimestamp("Stop.txt");
		}

		private void WriteTimestamp(string FileName)
		{
			try
			{
				File.WriteAllText(Path.Combine(this.folder, FileName), DateTime.Now.Ticks.ToString());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		#endregion
	}
}

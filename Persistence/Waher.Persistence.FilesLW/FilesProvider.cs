﻿#if COMPILED
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Runtime.Cache;
using Waher.Runtime.Profiling;
using Waher.Persistence.Exceptions;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Delegate for custom key callback methods.
	/// </summary>
	/// <param name="FileName">Name of file.</param>
	/// <returns>A pair of (Key, IV).</returns>
	public delegate Task<KeyValuePair<byte[], byte[]>> CustomKeyHandler(string FileName);

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
	public class FilesProvider : IDisposable, IDatabaseProvider, ISerializerContext
	{
		/// <summary>
		/// Name of environment variable containing salt for internal database encryption.
		/// </summary>
		public const string FILES_DB_SALT = nameof(FILES_DB_SALT);

#if COMPILED
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
#endif
		private static bool asyncFileIo = true;

		private SerializerCollection serializers;
		private readonly Dictionary<string, ObjectBTreeFile> files = new Dictionary<string, ObjectBTreeFile>();
		private readonly Dictionary<string, LabelFile> labelFiles = new Dictionary<string, LabelFile>();
		private readonly Dictionary<ObjectBTreeFile, bool> hasUnsavedData = new Dictionary<ObjectBTreeFile, bool>();
		private readonly Dictionary<string, StringDictionary> dictionaries = new Dictionary<string, StringDictionary>(StringComparer.CurrentCultureIgnoreCase);
		private StringDictionary master;
		private Cache<long, byte[]> blocks;
		private readonly object synchObj = new object();
		private readonly object synchObjNrFiles = new object();

		private readonly Encoding encoding;
		private readonly string id;
		private readonly string defaultCollectionName;
		private readonly string folder;
		private string xsltPath = string.Empty;
		private string autoRepairReportFolder = string.Empty;
		private readonly int blockSize;
		private readonly int blobBlockSize;
		private readonly int timeoutMilliseconds;
		private int nrFiles = 0;
		private int bulkCount = 0;
		private string salt = null;
		private readonly bool encrypted;
		private readonly CustomKeyHandler customKeyMethod;
#if COMPILED
		private bool rsaFailure = false;
		private readonly bool compiled;
		private bool deleteObsoleteKeys = true;
#endif

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
				  !(CustomKeyMethod is null), true, CustomKeyMethod)
		{
		}

		private FilesProvider(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Compiled, CustomKeyHandler CustomKeyMethod)
		{
			FileOfBlocks.CheckBlockSize(BlockSize);
			FileOfBlocks.CheckBlockSize(BlobBlockSize);

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentOutOfRangeException("The timeout must be positive.", nameof(TimeoutMilliseconds));

			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.defaultCollectionName = DefaultCollectionName;
			this.folder = Path.GetFullPath(Folder);
			this.blockSize = BlockSize;
			this.blobBlockSize = BlobBlockSize;
			this.encoding = Encoding;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.encrypted = Encrypted;
			this.customKeyMethod = CustomKeyMethod;
#if COMPILED
			this.compiled = Compiled;
			this.serializers = new SerializerCollection(this, this.compiled);
#else
			this.serializers = new SerializerCollection(this);
#endif

			if (!string.IsNullOrEmpty(this.folder) && this.folder[this.folder.Length - 1] != Path.DirectorySeparatorChar)
				this.folder += Path.DirectorySeparatorChar;

			this.blocks = new Cache<long, byte[]>(BlocksInCache, TimeSpan.MaxValue, TimeSpan.FromHours(1), true);
		}

#if COMPILED
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
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				Encrypted, true, null, null);
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
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				!(CustomKeyMethod is null), true, CustomKeyMethod, null);
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
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				false, false, null, null);
		}

#if COMPILED
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
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Compiled)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				Encrypted, Compiled, null, null);
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
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod, bool Compiled)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				!(CustomKeyMethod is null), Compiled, CustomKeyMethod, null);
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
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="Thread">Profiling thread. If provided, will be used to indicate events during setup of provider.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, ProfilerThread Thread)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				Encrypted, true, null, Thread);
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
		/// <param name="Thread">Profiling thread. If provided, will be used to indicate events during setup of provider.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod, ProfilerThread Thread)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				!(CustomKeyMethod is null), true, CustomKeyMethod, Thread);
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
		/// <param name="Thread">Profiling thread. If provided, will be used to indicate events during setup of provider.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, ProfilerThread Thread)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				false, false, null, Thread);
		}

#if COMPILED
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
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		/// <param name="Thread">Profiling thread. If provided, will be used to indicate events during setup of provider.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Compiled, ProfilerThread Thread)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				  Encrypted, Compiled, null, Thread);
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
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		/// <param name="Thread">Profiling thread. If provided, will be used to indicate events during setup of provider.</param>
		public static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, CustomKeyHandler CustomKeyMethod, bool Compiled, ProfilerThread Thread)
		{
			return CreateAsync(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize, Encoding, TimeoutMilliseconds,
				!(CustomKeyMethod is null), Compiled, CustomKeyMethod, Thread);
		}
#endif
		private async static Task<FilesProvider> CreateAsync(string Folder, string DefaultCollectionName, int BlockSize, int BlocksInCache, int BlobBlockSize,
			Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, bool Compiled, CustomKeyHandler CustomKeyMethod, ProfilerThread Thread)
		{
			Thread?.Start();
			Thread?.NewState("Create");

			FilesProvider Result = new FilesProvider(Folder, DefaultCollectionName, BlockSize, BlocksInCache, BlobBlockSize,
				Encoding, TimeoutMilliseconds, Encrypted, Compiled, CustomKeyMethod);

			Thread?.NewState("Master");
			Result.master = await StringDictionary.Create(Result.folder + "Files.master", string.Empty, string.Empty, Result, false);

			try
			{
				Thread?.NewState("Default");
				await Result.GetFile(Result.defaultCollectionName);

				Thread?.NewState("Config");
				await Result.LoadConfiguration();
			}
			catch (InconsistencyException ex)
			{
				Thread?.Exception(ex);
				Thread?.NewState("Clear");
				await Result.master.ClearAsync();

				Thread?.NewState("Default");
				await Result.GetFile(Result.defaultCollectionName);

				Thread?.NewState("Config");
				await Result.LoadConfiguration();
			}

			Thread?.Idle();
			Thread?.Stop();
			return Result;
		}

		private static readonly char[] CRLF = new char[] { '\r', '\n' };

		#endregion

		#region Properties

		/// <summary>
		/// Default collection name.
		/// </summary>
		public string DefaultCollectionName => this.defaultCollectionName;

		/// <summary>
		/// Base folder of where files will be stored.
		/// </summary>
		public string Folder => this.folder;

		/// <summary>
		/// If asynchronous file I/O is to be performed (true), or synchronous file I/O (false).
		/// </summary>
		public static bool AsyncFileIo
		{
			get => asyncFileIo;
			set => asyncFileIo = value;
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
		public string Id => this.id;

		/// <summary>
		/// Number of bytes used by an Object ID.
		/// </summary>
		public int ObjectIdByteCount => 16;

		/// <summary>
		/// Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="ObjectBTreeFile.InlineObjectSizeLimit"/> will be persisted as BLOBs, with the bulk of the object stored as separate files. 
		/// Smallest block size = 1024, largest block size = 65536.
		/// </summary>
		public int BlockSize => this.blockSize;

		/// <summary>
		/// Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.
		/// </summary>
		public int BlobBlockSize => this.blobBlockSize;

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding => this.encoding;

		/// <summary>
		/// Timeout, in milliseconds, for database operations.
		/// </summary>
		public int TimeoutMilliseconds => this.timeoutMilliseconds;

		/// <summary>
		/// If the files should be encrypted or not.
		/// </summary>
		public bool Encrypted => this.encrypted;

#if COMPILED
		/// <summary>
		/// If object serializers should be compiled or not.
		/// </summary>
		public bool Compiled => this.compiled;

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
		/// If normalized names are to be used or not. Normalized names reduces the number
		/// of bytes required to serialize objects, but do not work in a decentralized
		/// architecture.
		/// </summary>
		public bool NormalizedNames
		{
			get { return true; }
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			this.master?.Dispose();
			this.master = null;

			if (!(this.dictionaries is null))
			{
				StringDictionary[] Dictionaries;

				lock (this.files)
				{
					Dictionaries = new StringDictionary[this.dictionaries.Count];
					this.dictionaries.Values.CopyTo(Dictionaries, 0);
				}

				foreach (StringDictionary Dictionary in Dictionaries)
					Dictionary?.Dispose();

				this.dictionaries.Clear();
			}

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
				await this.serializers.DisposeAsync();
				this.serializers = null;
			}

			this.WriteTimestamp("Stop.txt");
		}

		#endregion

		#region Timing

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

		#region Serialization

		/// <summary>
		/// Gets the object serializer corresponding to a specific type.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public Task<IObjectSerializer> GetObjectSerializer(Type Type)
		{
			return this.serializers?.GetObjectSerializer(Type)
				?? throw new InvalidOperationException("Service is shutting down.");
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific type, if one exists.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer if exists, or null if not.</returns>
		public Task<IObjectSerializer> GetObjectSerializerNoCreate(Type Type)
		{
			return this.serializers?.GetObjectSerializerNoCreate(Type)
				?? throw new InvalidOperationException("Service is shutting down.");
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Object">Object to serialize</param>
		/// <returns>Object Serializer</returns>
		public Task<ObjectSerializer> GetObjectSerializerEx(object Object)
		{
			return this.GetObjectSerializerEx(Object.GetType());
		}

		/// <summary>
		/// Gets the object serializer corresponding to a specific object.
		/// </summary>
		/// <param name="Type">Type of object to serialize.</param>
		/// <returns>Object Serializer</returns>
		public async Task<ObjectSerializer> GetObjectSerializerEx(Type Type)
		{
			if (!(await this.GetObjectSerializer(Type) is ObjectSerializer Serializer))
				throw new SerializationException("Objects of type " + Type.FullName + " must be embedded.", Type);

			return Serializer;
		}

		#endregion

		#region Fields

		/// <summary>
		/// Gets the code for a specific field in a collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldName">Name of field.</param>
		/// <returns>Field code.</returns>
		public async Task<ulong> GetFieldCode(string Collection, string FieldName)
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

			return await Labels.GetFieldCode(FieldName);
		}

		/// <summary>
		/// Gets the name of a field in a collection, given its code.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="FieldCode">Field code.</param>
		/// <returns>Field name.</returns>
		/// <exception cref="ArgumentException">If the collection or field code are not known.</exception>
		public async Task<string> GetFieldName(string Collection, ulong FieldCode)
		{
			if (FieldCode > uint.MaxValue)
				throw Database.FlagForRepair(Collection, "Field code too large.");

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

			return await Labels.GetFieldName((uint)FieldCode);
		}

		/// <summary>
		/// Checks if a string is a label in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection.</param>
		/// <param name="Label">Label to check.</param>
		/// <returns>If <paramref name="Label"/> is a label in the collection
		/// defined by <paramref name="Collection"/>.</returns>
		public async Task<bool> IsLabel(string Collection, string Label)
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

			return (await Labels.TryGetFieldCode(Label)).HasValue;
		}

		/// <summary>
		/// Gets an array of available labels for a collection.
		/// </summary>
		/// <returns>Array of labels.</returns>
		public async Task<string[]> GetLabels(string Collection)
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

			return (await Labels.GetLabelsAsync());
		}

		/// <summary>
		/// Tries to get the Object ID of an object, if it exists.
		/// </summary>
		/// <param name="Object">Object whose Object ID is of interest.</param>
		/// <returns>Object ID, if found, null otherwise.</returns>
		public async Task<object> TryGetObjectId(object Object)
		{
			if (Object is null)
				return false;

			IObjectSerializer Serializer = await this.GetObjectSerializer(Object.GetType());
			if (Serializer is ObjectSerializer SerializerEx && await SerializerEx.HasObjectId(Object))
				return await SerializerEx.GetObjectId(Object, false, null);
			else
				return null;
		}

		#endregion

		#region Keys

		internal async Task<KeyValuePair<byte[], byte[]>> GetKeys(string FileName, bool FileExists)
		{
			byte[] Key, IV;

#if COMPILED
			if (!(this.customKeyMethod is null))
			{
#endif
				KeyValuePair<byte[], byte[]> P = await this.customKeyMethod(FileName);

				using (SHA256 Sha256 = SHA256.Create())
				{
					Key = Sha256.ComputeHash(P.Key);
					IV = Sha256.ComputeHash(P.Value);
				}
#if COMPILED
			}
			else if (this.rsaFailure)
				return this.GetKeysUsingSalt(FileName);
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
				catch (PlatformNotSupportedException)
				{
					this.rsaFailure = true;
					return this.GetKeysUsingSalt(FileName);
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

					Array.Resize(ref Key, 32);
					Array.Resize(ref IV, 32);
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
			return new KeyValuePair<byte[], byte[]>(Key, IV);
		}

		private KeyValuePair<byte[], byte[]> GetKeysUsingSalt(string FileName)
		{
			if (string.IsNullOrEmpty(this.salt))
			{
				this.salt = Environment.GetEnvironmentVariable(FILES_DB_SALT);
				if (string.IsNullOrEmpty(this.salt))
					throw new CryptographicException("Missing salt in environment variable FILES_DB_SALT.");
			}

			using (SHA512 H = SHA512.Create())
			{
				byte[] Bin = H.ComputeHash(Encoding.UTF8.GetBytes(this.salt + "|" + FileName));
				byte[] Key = new byte[32];
				byte[] IV = new byte[32];

				Array.Copy(Bin, 0, Key, 0, 32);
				Array.Copy(Bin, 32, IV, 0, 32);

				return new KeyValuePair<byte[], byte[]>(Key, IV);
			}
		}

		#endregion

		#region Blocks

		/// <summary>
		/// Removes all blocks pertaining to a specific file.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		internal void RemoveBlocks(int FileId)
		{
			if (!(this.blocks is null))
			{
				long Min = this.GetBlockKey(FileId, 0);
				long Max = this.GetBlockKey(FileId, uint.MaxValue);

				foreach (long Key in this.blocks.GetKeys())
				{
					if (Key >= Min && Key <= Max)
						this.blocks.Remove(Key);
				}
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
			if (!(this.blocks is null))
				return this.blocks.TryGetValue(this.GetBlockKey(FileId, BlockIndex), out Block);
			else
			{
				Block = null;
				return false;
			}
		}

		/// <summary>
		/// Adds a block to the cache.
		/// </summary>
		/// <param name="FileId">Internal file ID.</param>
		/// <param name="BlockIndex">Block index.</param>
		/// <param name="Block">Block.</param>
		internal void AddBlockToCache(int FileId, uint BlockIndex, byte[] Block)
		{
			this.blocks?.Add(this.GetBlockKey(FileId, BlockIndex), Block);
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
					return Task.CompletedTask;

				this.bulkCount--;
				if (this.bulkCount > 0)
					return Task.CompletedTask;
			}

			return this.SaveUnsaved(false);
		}

		/// <summary>
		/// Number of current bulk operations.
		/// </summary>
		public int BulkCount
		{
			get
			{
				lock (this.synchObj)
				{
					return this.bulkCount;
				}
			}
		}

		private async Task<bool> SaveUnsaved(bool AllFiles)
		{
			ObjectBTreeFile[] Files;
			int c;

			if (AllFiles)
			{
				lock (this.files)
				{
					ChunkedList<ObjectBTreeFile> List = new ChunkedList<ObjectBTreeFile>();
					List.AddRange(this.files.Values);

					if (!(this.master is null))
						List.Add(this.master.DictionaryFile);

					foreach (StringDictionary Dictionary in this.dictionaries.Values)
						List.Add(Dictionary.DictionaryFile);

					if (List.Count == 0)
						return false;

					Files = List.ToArray();
				}

				lock (this.synchObj)
				{
					this.hasUnsavedData.Clear();
				}
			}
			else
			{
				lock (this.synchObj)
				{
					c = this.hasUnsavedData.Count;
					if (c == 0)
						return false;

					Files = new ObjectBTreeFile[c];
					this.hasUnsavedData.Keys.CopyTo(Files, 0);

					this.hasUnsavedData.Clear();
				}
			}

			foreach (ObjectBTreeFile File in Files)
			{
				await File.BeginWrite();
				await File.EndWrite();   // Saves unsaved data.
			}

			return true;
		}

		internal bool InBulkMode(ObjectBTreeFile Caller)
		{
			lock (this.synchObj)
			{
				if (this.bulkCount <= 0)
					return false;
				else
				{
					if (Caller.MainSynch)
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
		/// <returns>New File ID.</returns>
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
					else if (this.master is null)
						throw new InvalidOperationException("Provider has been stopped.");
					else
						this.files[CollectionName] = null;
				}

				if (Wait)
					await Task.Delay(100);
			}
			while (Wait);

			LabelFile Labels = await LabelFile.Create(CollectionName, this.timeoutMilliseconds, this.encrypted, this);

			File = await ObjectBTreeFile.Create(s + ".btree", CollectionName, s + ".blob", this.blockSize, this.blobBlockSize,
				this, this.encoding, this.timeoutMilliseconds, this.encrypted);

			lock (this.files)
			{
				this.files[CollectionName] = File;
				this.labelFiles[CollectionName] = Labels;
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Collection");
			sb.AppendLine(CollectionName);

			KeyValuePair<bool, object> P2 = await this.master.TryGetValueAsync(File.FileName);
			string s2 = sb.ToString();

			if (this.NeedsMasterRegistryUpdate(P2, s2))
				await this.master.AddAsync(File.FileName, s2, true);

			await this.GetFieldCode(null, CollectionName);

			return File;
		}

		private bool NeedsMasterRegistryUpdate(KeyValuePair<bool, object> Record, string ExpectedValue)
		{
			if (!Record.Key)
				return true;

			if (Record.Value is string s)
				return s != ExpectedValue;
			else
				return true;
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
		public Task<IndexBTreeFile> GetIndexFile(ObjectBTreeFile File, RegenerationOptions RegenerationOptions, params string[] FieldNames)
		{
			return this.GetIndexFile(File, true, RegenerationOptions, FieldNames);
		}

		/// <summary>
		/// Gets an index file.
		/// </summary>
		/// <param name="File">Object file.</param>
		/// <param name="CanRetry">If a retry to access index file can be made.</param>
		/// <param name="RegenerationOptions">Index regeneration options.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <returns>Index file.</returns>
		private async Task<IndexBTreeFile> GetIndexFile(ObjectBTreeFile File, bool CanRetry, RegenerationOptions RegenerationOptions, params string[] FieldNames)
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
				{
					await File.BeginWrite();
					try
					{
						await I.RegenerateLocked();
					}
					finally
					{
						await File.EndWrite();
					}
				}

				return I;
			}

			if (RegenerationOptions == RegenerationOptions.RegenerateIfIndexNotInstantiated)
				Regenerate = true;

			StringBuilder sb = new StringBuilder();

			string s = this.GetIndexFileName(File, FieldNames);
			bool Exists = System.IO.File.Exists(s);

			if (!Exists && RegenerationOptions == RegenerationOptions.RegenerateIfFileNotFound)
				Regenerate = true;

			await File.BeginWrite();
			try
			{
				IndexFile = await IndexBTreeFile.Create(s, File, this, FieldNames);

				await File.AddIndexLocked(IndexFile, Regenerate);
			}
			catch (IOException ex)
			{
				if (!CanRetry)
					ExceptionDispatchInfo.Capture(ex).Throw();

				return await this.GetIndexFile(File, false, RegenerationOptions, FieldNames);
			}
			finally
			{
				await File.EndWrite();
			}

			sb.Clear();

			sb.AppendLine("Index");
			sb.AppendLine(File.CollectionName);

			foreach (string FieldName in FieldNames)
				sb.AppendLine(FieldName);

			if (s.StartsWith(this.folder))
				s = s.Substring(this.folder.Length);

			KeyValuePair<bool, object> P = await this.master.TryGetValueAsync(s);
			string s2 = sb.ToString();

			if (this.NeedsMasterRegistryUpdate(P, s2))
				await this.master.AddAsync(s, s2, true);

			return IndexFile;
		}

		/// <summary>
		/// Gets an index file name.
		/// </summary>
		/// <param name="File">Object file.</param>
		/// <param name="FieldNames">Field names to build the index on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <returns>Index file name.</returns>
		public string GetIndexFileName(ObjectBTreeFile File, params string[] FieldNames)
		{
			StringBuilder sb = new StringBuilder();

#if COMPILED
			string s;
			bool Exists;
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

			return sb.ToString();
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
			int i = -1;

			while ((i = s.IndexOfAny(forbiddenCharacters, i + 1)) >= 0)
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
			ChunkedList<string> ToRemove = null;
			KeyValuePair<string, object>[] Items = await this.master.ToArrayAsync();

			foreach (KeyValuePair<string, object> P in Items)
			{
				if (!(P.Value is string s))
				{
					if (P.Value is KeyValuePair<string, object> P2)
						s = P2.Value.ToString();
					else
						s = P.Value.ToString();
				}

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
								ToRemove = new ChunkedList<string>();

							ToRemove.Add(P.Key);
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
								ToRemove = new ChunkedList<string>();

							ToRemove.Add(P.Key);
							break;
						}

						await this.GetIndexFile(File, RegenerationOptions.RegenerateIfFileNotFound, FieldNames);
						break;
				}
			}

			if (!(ToRemove is null))
			{
				foreach (string s in ToRemove)
					await this.master.RemoveAsync(s);
			}
		}

		/// <summary>
		/// Gets an array of available collections.
		/// </summary>
		/// <returns>Array of collections.</returns>
		public Task<string[]> GetCollections()
		{
			string[] Result;

			lock (this.files)
			{
				Result = new string[this.files.Count];
				this.files.Keys.CopyTo(Result, 0);
			}

			return Task.FromResult<string[]>(Result);
		}

		/// <summary>
		/// Gets the collection corresponding to a given type.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <returns>Collection name.</returns>
		public async Task<string> GetCollection(Type Type)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Type);
			return await Serializer.CollectionName(null);
		}

		/// <summary>
		/// Gets the collection corresponding to a given object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Collection name.</returns>
		public async Task<string> GetCollection(Object Object)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object);
			return await Serializer.CollectionName(Object);
		}

		/// <summary>
		/// Drops a collection, if it exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		public async Task DropCollection(string CollectionName)
		{
			ObjectBTreeFile File = await this.GetFile(CollectionName, false);
			if (File is null)
				return;

			foreach (IndexBTreeFile Index in File.Indices)
				await this.RemoveIndex(File, Index);

			LabelFile Labels;
			string s, s2;

			lock (this.files)
			{
				this.files.Remove(CollectionName);

				if (this.labelFiles.TryGetValue(CollectionName, out Labels))
					this.labelFiles.Remove(CollectionName);
				else
					Labels = null;
			}

			s = File.FileName;
			s2 = File.BlobFileName;

			File.Dispose();

			if (System.IO.File.Exists(s))
				System.IO.File.Delete(s);

			if (System.IO.File.Exists(s2))
				System.IO.File.Delete(s2);

			if (!(Labels is null))
			{
				s = Labels.FileName;
				Labels.Dispose();

				if (System.IO.File.Exists(s))
					System.IO.File.Delete(s);
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
		public Task<T> TryLoadObject<T>(object ObjectId)
			where T : class
		{
			Guid OID;

			if (ObjectId is Guid Guid)
				OID = Guid;
			else if (ObjectId is string s)
				OID = new Guid(s);
			else if (ObjectId is byte[] ba)
				OID = new Guid(ba);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.TryLoadObject<T>(OID);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> TryLoadObject<T>(Guid ObjectId)
			where T : class
		{
			return this.TryLoadObject<T>(ObjectId, null);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<T> TryLoadObject<T>(Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null));

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return (T)await File.TryLoadObjectLocked(ObjectId, Serializer);
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
				return (T)await File.TryLoadObject(ObjectId, Serializer);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <paramref name="T"/>.
		/// </summary>
		/// <param name="T">Base type.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<object> TryLoadObject(Type T, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(T);
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null));

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return await File.TryLoadObjectLocked(ObjectId, Serializer);
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
				return await File.TryLoadObject(ObjectId, Serializer);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> TryLoadObject<T>(string CollectionName, object ObjectId)
			where T : class
		{
			Guid OID;

			if (ObjectId is Guid Guid)
				OID = Guid;
			else if (ObjectId is string s)
				OID = new Guid(s);
			else if (ObjectId is byte[] ba)
				OID = new Guid(ba);
			else
				throw new NotSupportedException("Unsupported type for Object ID: " + ObjectId.GetType().FullName);

			return this.TryLoadObject<T>(CollectionName, OID);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<T> TryLoadObject<T>(string CollectionName, Guid ObjectId)
			where T : class
		{
			return this.TryLoadObject<T>(CollectionName, ObjectId, null);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its base type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Base type.</typeparam>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<T> TryLoadObject<T>(string CollectionName, Guid ObjectId, EmbeddedObjectSetter EmbeddedSetter)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(CollectionName);

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return (T)await File.TryLoadObjectLocked(ObjectId, Serializer);
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
				return (T)await File.TryLoadObject(ObjectId, Serializer);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Loaded object.</returns>
		public Task<object> TryLoadObject(string CollectionName, object ObjectId)
		{
			return this.TryLoadObject(CollectionName, ObjectId, null);
		}

		/// <summary>
		/// Loads an object given its Object ID <paramref name="ObjectId"/> and its collection name <paramref name="CollectionName"/>.
		/// </summary>
		/// <param name="CollectionName">Name of collection in which the object resides.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="EmbeddedSetter">Setter method, used to set an embedded property during delayed loading.</param>
		/// <returns>Loaded object.</returns>
		public async Task<object> TryLoadObject(string CollectionName, object ObjectId, EmbeddedObjectSetter EmbeddedSetter)
		{
			Guid Id;

			if (ObjectId is Guid Guid)
				Id = Guid;
			else if (ObjectId is string s)
				Id = new Guid(s);
			else if (ObjectId is byte[] Bin)
				Id = new Guid(Bin);
			else
				throw new ArgumentException("Invalid type of Object ID: " + ObjectId.GetType().FullName, nameof(ObjectId));

			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(object));
			ObjectBTreeFile File = await this.GetFile(CollectionName);

			if (!(EmbeddedSetter is null))
			{
				if (await File.TryBeginRead(0))
				{
					try
					{
						return await File.TryLoadObjectLocked(ObjectId, Serializer);
					}
					finally
					{
						await File.EndRead();
					}
				}
				else
				{
					File.QueueForLoad(Id, Serializer, EmbeddedSetter);
					return null;
				}
			}
			else
				return await File.TryLoadObject(Id, Serializer);

		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public async Task<Guid> GetObjectId(object Value, bool InsertIfNotFound)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Value);
			return await Serializer.GetObjectId(Value, InsertIfNotFound, null);
		}

		/// <summary>
		/// Saves an unsaved object, and returns a new GUID identifying the saved object.
		/// </summary>
		/// <param name="Value">Object to save.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		/// <returns>GUID identifying the saved object.</returns>
		public async Task<Guid> SaveNewObject(object Value, object State)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Value);
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Value));

			if (!(State is NestedLocks NestedLocks))
				return await File.SaveNewObject(Value, Serializer, false, null);

			if (NestedLocks.HasLock(File, out bool WriteLock))
			{
				if (WriteLock)
					return await File.SaveNewObjectLocked(Value, Serializer, NestedLocks);
				else
					throw new InvalidOperationException("Not in a writing state.");
			}
			else
			{
				await File.BeginWrite();
				try
				{
					NestedLocks.AddLock(File, true);
					return await File.SaveNewObjectLocked(Value, Serializer, NestedLocks);
				}
				finally
				{
					NestedLocks.RemoveLock(File);
					await File.EndWrite();
				}
			}
		}

		#endregion

		#region IDatabaseProvider

		/// <summary>
		/// Inserts an object into the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Insert(object Object)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object);
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.CheckIndicesInitialized(Serializer);
			await File.SaveNewObject(Object, Serializer, false, null);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(params object[] Objects)
		{
			return this.Insert((IEnumerable<object>)Objects, false, null);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Insert(IEnumerable<object> Objects)
		{
			return this.Insert(Objects, false, null);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task InsertLazy(object Object, ObjectCallback Callback)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object);
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.CheckIndicesInitialized(Serializer);
			await File.SaveNewObject(Object, Serializer, true, Callback);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task InsertLazy(object[] Objects, ObjectsCallback Callback)
		{
			return this.Insert((IEnumerable<object>)Objects, true, Callback);
		}

		/// <summary>
		/// Inserts an object into the database, if unlocked. If locked, object will be inserted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task InsertLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			return this.Insert(Objects, true, Callback);
		}

		/// <summary>
		/// Inserts a collection of objects into the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Lazy">If Lazy insert is used, i.e. sufficiant that object is inserted at next opportuity.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		private async Task Insert(IEnumerable<object> Objects, bool Lazy, ObjectsCallback Callback)
		{
			ChunkedList<object> List = new ChunkedList<object>();
			ObjectSerializer Serializer = null;
			ObjectBTreeFile File = null;
			string CollectionName = null;
			string CollectionName2;
			Type T = null;
			Type T2;
			bool CheckIndices = false;

			foreach (object Object in Objects)
			{
				T2 = Object.GetType();
				if (Serializer is null || T != T2)
				{
					if (List.HasFirstItem)
					{
						await File.SaveNewObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					T = T2;
					Serializer = await this.GetObjectSerializerEx(Object);
					CheckIndices = true;
				}

				CollectionName2 = await Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (List.HasFirstItem)
					{
						await File.SaveNewObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
					CheckIndices = true;
				}

				if (CheckIndices)
				{
					await File.CheckIndicesInitialized(Serializer);
					CheckIndices = false;
				}

				List.Add(Object);
			}

			if (List.HasFirstItem)
				await File.SaveNewObjects(List, Serializer, Lazy, Callback);
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
		public Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			return this.Find<T>(Offset, MaxCount, null, SortOrder);
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
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null), false);

			if (File is null)
				return Array.Empty<T>();

			await File.CheckIndicesInitialized(Serializer);
			await File.BeginRead();
			try
			{
				ICursor<T> ResultSet = await File.FindLocked<T>(Offset, MaxCount, Filter, SortOrder);
				return await LoadAllLocked<T>(ResultSet);
			}
			finally
			{
				await File.EndRead();
			}
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(int Offset, int MaxCount, Filter Filter, 
			T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null), false);

			if (File is null)
				return Array.Empty<T>();

			await File.CheckIndicesInitialized(Serializer);
			await File.BeginRead();
			try
			{
				ICursor<T> ResultSet = await File.FindLocked<T>(Offset, MaxCount, Filter, SortOrder);
				await ResultSet.ContinueAfterLocked(ContinueAfter);
				return await LoadAllLocked<T>(ResultSet);
			}
			finally
			{
				await File.EndRead();
			}
		}

		internal static async Task<IEnumerable<T>> LoadAllLocked<T>(ICursor<T> ResultSet)
		{
			ChunkedList<T> Result = new ChunkedList<T>();

			while (await ResultSet.MoveNextAsyncLocked())
			{
				if (ResultSet.CurrentTypeCompatible)
					Result.Add(ResultSet.Current);
			}

			return Result;
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			return this.Find<object>(Collection, Offset, MaxCount, null, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> Find(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			return this.Find<object>(Collection, Offset, MaxCount, Filter, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectBTreeFile File = await this.GetFile(Collection, false);

			if (File is null)
				return Array.Empty<T>();

			await File.CheckIndicesInitialized<T>();
			await File.BeginRead();
			try
			{
				ICursor<T> ResultSet = await File.FindLocked<T>(Offset, MaxCount, Filter, SortOrder);
				return await LoadAllLocked<T>(ResultSet);
			}
			finally
			{
				await File.EndRead();
			}
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="ContinueAfter">Continue returning results after this object.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> Find<T>(string Collection, int Offset, int MaxCount, 
			Filter Filter, T ContinueAfter, params string[] SortOrder)
			where T : class
		{
			ObjectBTreeFile File = await this.GetFile(Collection, false);

			if (File is null)
				return Array.Empty<T>();

			await File.CheckIndicesInitialized<T>();
			await File.BeginRead();
			try
			{
				ICursor<T> ResultSet = await File.FindLocked<T>(Offset, MaxCount, Filter, SortOrder);
				await ResultSet.ContinueAfterLocked(ContinueAfter);
				return await LoadAllLocked<T>(ResultSet);
			}
			finally
			{
				await File.EndRead();
			}
		}

		/// <summary>
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(int PageSize, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(0, PageSize, SortOrder);
			return new Page<T>(PageSize, null, null, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(0, PageSize, Filter, SortOrder);
			return new Page<T>(PageSize, null, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<object>> FindFirst(string Collection, int PageSize, params string[] SortOrder)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(object));
			IEnumerable<object> Items = await this.Find(Collection, 0, PageSize, SortOrder);
			return new Page<object>(PageSize, Collection, null, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<object>> FindFirst(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(object));
			IEnumerable<object> Items = await this.Find(Collection, 0, PageSize, Filter, SortOrder);
			return new Page<object>(PageSize, Collection, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the first page of objects in a given collection.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="PageSize">Number of items on a page.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>First page of objects.</returns>
		public async Task<IPage<T>> FindFirst<T>(string Collection, int PageSize, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			IEnumerable<T> Items = await this.Find<T>(Collection, 0, PageSize, Filter, SortOrder);
			return new Page<T>(PageSize, Collection, Filter, SortOrder, Items, Serializer, this);
		}

		/// <summary>
		/// Finds the next page of objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<T>> FindNext<T>(IPage<T> Page)
			where T : class
		{
			if (Page is Page<T> CurrentPage)
				return CurrentPage.FindNext();
			else
				throw new IOException("Incompatible page.");
		}

		/// <summary>
		/// Finds the next page of objects in a given collection.
		/// </summary>
		/// <param name="Page">Page reference.</param>
		/// <returns>Next page, directly following <paramref name="Page"/>.</returns>
		public Task<IPage<object>> FindNext(IPage<object> Page)
		{
			if (Page is Page<object> CurrentPage)
				return CurrentPage.FindNext();
			else
				throw new IOException("Incompatible page.");
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
		public Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, params string[] SortOrder)
			where T : class
		{
			return this.FindDelete<T>(Offset, MaxCount, null, SortOrder);
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
		public async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null), false);

			if (File is null)
				return Array.Empty<T>();

			return await File.FindDelete<T>(Offset, MaxCount, Filter, Serializer, false, SortOrder, null);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, params string[] SortOrder)
		{
			return this.FindDelete(Collection, Offset, MaxCount, null, SortOrder);
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<object>> FindDelete(string Collection, int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ObjectBTreeFile File = await this.GetFile(Collection, false);

			if (File is null)
				return Array.Empty<object>();

			return await File.FindDelete(Offset, MaxCount, Filter, false, SortOrder, null);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy<T>(int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			return this.DeleteLazy<T>(Offset, MaxCount, null, SortOrder, Callback);
		}

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/> and deletes them in the same atomic operation.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task DeleteLazy<T>(int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(T));
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(null), false);

			if (!(File is null))
				await File.FindDelete<T>(Offset, MaxCount, Filter, Serializer, true, SortOrder, Callback);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(string Collection, int Offset, int MaxCount, string[] SortOrder, ObjectsCallback Callback)
		{
			return this.DeleteLazy(Collection, Offset, MaxCount, null, SortOrder, Callback);
		}

		/// <summary>
		/// Finds objects in a given collection and deletes them in the same atomic operation.
		/// </summary>
		/// <param name="Collection">Name of collection to search.</param>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task DeleteLazy(string Collection, int Offset, int MaxCount, Filter Filter, string[] SortOrder, ObjectsCallback Callback)
		{
			ObjectBTreeFile File = await this.GetFile(Collection, false);

			if (!(File is null))
				await File.FindDelete(Offset, MaxCount, Filter, true, SortOrder, Callback);
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Update(object Object)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.UpdateObject(Object, Serializer, false, null);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(params object[] Objects)
		{
			return this.Update((IEnumerable<object>)Objects, false, null);
		}

		/// <summary>
		/// Updates a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Update(IEnumerable<object> Objects)
		{
			return this.Update(Objects, false, null);
		}

		/// <summary>
		/// Updates an object in the database, if unlocked. If locked, object will be updated at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task UpdateLazy(object Object, ObjectCallback Callback)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.UpdateObject(Object, Serializer, true, Callback);
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task UpdateLazy(object[] Objects, ObjectsCallback Callback)
		{
			return this.Update((IEnumerable<object>)Objects, true, Callback);
		}

		/// <summary>
		/// Updates a collection of objects in the database, if unlocked. If locked, objects will be updated at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task UpdateLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			return this.Update(Objects, true, Callback);
		}

		private async Task Update(IEnumerable<object> Objects, bool Lazy, ObjectsCallback Callback)
		{
			ChunkedList<object> List = new ChunkedList<object>();
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
					if (List.HasFirstItem)
					{
						await File.UpdateObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					T = T2;
					Serializer = await this.GetObjectSerializerEx(Object);
				}

				CollectionName2 = await Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (List.HasFirstItem)
					{
						await File.UpdateObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
				}

				List.Add(Object);
			}

			if (List.HasFirstItem)
				await File.UpdateObjects(List, Serializer, Lazy, Callback);
		}

		/// <summary>
		/// Deletes an object in the database.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		public async Task Delete(object Object)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.DeleteObject(Object, Serializer, false, null);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(params object[] Objects)
		{
			return this.Delete((IEnumerable<object>)Objects, false, null);
		}

		/// <summary>
		/// Deletes a collection of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		public Task Delete(IEnumerable<object> Objects)
		{
			return this.Delete(Objects, false, null);
		}

		/// <summary>
		/// Deletes an object in the database, if unlocked. If locked, object will be deleted at next opportunity.
		/// </summary>
		/// <param name="Object">Object to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task DeleteLazy(object Object, ObjectCallback Callback)
		{
			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object.GetType());
			ObjectBTreeFile File = await this.GetFile(await Serializer.CollectionName(Object));
			await File.DeleteObject(Object, Serializer, true, Callback);
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(object[] Objects, ObjectsCallback Callback)
		{
			return this.Delete((IEnumerable<object>)Objects, true, Callback);
		}

		/// <summary>
		/// Deletes a collection of objects in the database, if unlocked. If locked, objects will be deleted at next opportunity.
		/// </summary>
		/// <param name="Objects">Objects to insert.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public Task DeleteLazy(IEnumerable<object> Objects, ObjectsCallback Callback)
		{
			return this.Delete(Objects, true, Callback);
		}

		private async Task Delete(IEnumerable<object> Objects, bool Lazy, ObjectsCallback Callback)
		{
			ChunkedList<object> List = new ChunkedList<object>();
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
					if (List.HasFirstItem)
					{
						await File.DeleteObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					T = T2;
					Serializer = await this.GetObjectSerializerEx(Object);
				}

				CollectionName2 = await Serializer.CollectionName(Object);
				if (File is null || CollectionName != CollectionName2)
				{
					if (List.HasFirstItem)
					{
						await File.DeleteObjects(List, Serializer, Lazy, Callback);
						List.Clear();
					}

					CollectionName = CollectionName2;
					File = await this.GetFile(CollectionName);
				}

				List.Add(Object);
			}

			if (List.HasFirstItem)
				await File.DeleteObjects(List, Serializer, Lazy, Callback);
		}

		/// <summary>
		/// Gets a persistent dictionary containing objects in a collection.
		/// </summary>
		/// <param name="Collection">Collection Name</param>
		/// <returns>Persistent dictionary</returns>
		public async Task<IPersistentDictionary> GetDictionary(string Collection)
		{
			lock (this.files)
			{
				if (this.dictionaries.TryGetValue(Collection, out StringDictionary Dictionary))
					return Dictionary;
			}

			string FileName = Path.Combine(this.folder, "Dictionaries");
			if (!Directory.Exists(FileName))
				Directory.CreateDirectory(FileName);

			FileName = Path.Combine(FileName, Collection);

			return await StringDictionary.Create(FileName + ".dict", FileName + ".dblob", Collection, this, false);
		}

		internal void Register(StringDictionary Dictionary)
		{
			lock (this.files)
			{
				this.dictionaries[Dictionary.CollectionName] = Dictionary;
			}
		}

		internal void Unregister(StringDictionary Dictionary)
		{
			lock (this.files)
			{
				this.dictionaries.Remove(Dictionary.CollectionName);
			}
		}

		/// <summary>
		/// Gets an array of available dictionary collections.
		/// </summary>
		/// <returns>Array of dictionary collections.</returns>
		public Task<string[]> GetDictionaries()
		{
			string Folder = Path.Combine(this.folder, "Dictionaries");
			if (!Directory.Exists(Folder))
				return Task.FromResult(Array.Empty<string>());

			ChunkedList<string> Collections = new ChunkedList<string>();

			foreach (string FullFileName in Directory.GetFiles(Folder, "*.dict", SearchOption.TopDirectoryOnly))
			{
				string LocalFileName = Path.GetFileName(FullFileName);

				if (LocalFileName.EndsWith(".dict", StringComparison.OrdinalIgnoreCase))
					Collections.Add(LocalFileName.Substring(0, LocalFileName.Length - 5));
			}

			return Task.FromResult(Collections.ToArray());
		}

		/// <summary>
		/// Creates a generalized representation of an object.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Generalized representation.</returns>
		public async Task<GenericObject> Generalize(object Object)
		{
			if (Object is null)
				return null;

			if (Object is GenericObject GenObj)
				return GenObj;

			ObjectSerializer Serializer = await this.GetObjectSerializerEx(Object);
			string CollectionName = await Serializer.CollectionName(Object);
			BinarySerializer Output = new BinarySerializer(CollectionName, Encoding.UTF8);

			await Serializer.Serialize(Output, true, false, Object, null);

			Output.FlushBits();
			byte[] Bin = Output.GetSerialization();

			ObjectSerializer Deserializer = await this.GetObjectSerializerEx(typeof(GenericObject));

			BinaryDeserializer Input = new BinaryDeserializer(CollectionName, Encoding.UTF8, Bin, 0);
			object Result = await Deserializer.Deserialize(Input, null, false);

			if (Result is GenericObject GenObj2)
			{
				GenObj2.ArchivingTime = Serializer.GetArchivingTimeDays(Object);
				return GenObj2;
			}
			else
				throw new InvalidOperationException("Unable to generalize object.");
		}

		/// <summary>
		/// Creates a specialized representation of a generic object.
		/// </summary>
		/// <param name="Object">Generic object.</param>
		/// <returns>Specialized representation.</returns>
		public async Task<object> Specialize(GenericObject Object)
		{
			if (Object is null)
				return null;

			Type T = Types.GetType(Object.TypeName);
			if (T is null)
				return Object;

			ObjectSerializer Serializer = await this.GetObjectSerializerEx(typeof(GenericObject));
			string CollectionName = await Serializer.CollectionName(Object);
			BinarySerializer Output = new BinarySerializer(CollectionName, Encoding.UTF8);

			await Serializer.Serialize(Output, true, false, Object, null);

			Output.FlushBits();
			byte[] Bin = Output.GetSerialization();

			Serializer = await this.GetObjectSerializerEx(T);

			BinaryDeserializer Input = new BinaryDeserializer(CollectionName, Encoding.UTF8, Bin, 0);
			object Result = await Serializer.Deserialize(Input, null, false);

			return Result;
		}

		/// <summary>
		/// Gets an array of collections that should be excluded from backups.
		/// </summary>
		/// <returns>Array of excluded collections.</returns>
		public string[] GetExcludedCollections()
		{
			return this.serializers?.GetExcludedCollections()
				?? throw new InvalidOperationException("Service is shutting down.");
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
				NewLineHandling = NewLineHandling.Replace,
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
				ChunkedList<ObjectBTreeFile> Files = new ChunkedList<ObjectBTreeFile>();

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
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public Task<bool> Export(IDatabaseExport Output, string[] CollectionNames)
		{
			return this.Export(Output, CollectionNames, null);
		}


		/// <summary>
		/// Performs an export of the database.
		/// </summary>
		/// <param name="Output">Database will be output to this interface.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>If export process was completed (true), or terminated by <paramref name="Output"/> (false).</returns>
		public async Task<bool> Export(IDatabaseExport Output, string[] CollectionNames, ProfilerThread Thread)
		{
			ObjectBTreeFile[] Files = this.Files;
			IDatabaseExportFilter Filter = Output as IDatabaseExportFilter;
			bool Continue;

			Thread?.Start();
			if (!await Output.StartDatabase())
				return false;
			try
			{
				foreach (ObjectBTreeFile File in Files)
				{
					if (!(CollectionNames is null) && Array.IndexOf(CollectionNames, File.CollectionName) < 0)
						continue;

					if (!(Filter is null) && !Filter.CanExportCollection(File.CollectionName))
						continue;

					Thread?.NewState(File.CollectionName);
					if (!await Output.StartCollection(File.CollectionName))
						return false;
					try
					{
						IndexBTreeFile[] Indices = File.Indices;

						if (!(Indices is null))
						{
							foreach (IndexBTreeFile Index in Indices)
							{
								if (!await Output.StartIndex())
									return false;

								string[] FieldNames = Index.FieldNames;
								bool[] Ascending = Index.Ascending;
								int i, c = Math.Min(FieldNames.Length, Ascending.Length);

								for (i = 0; i < c; i++)
								{
									if (!await Output.ReportIndexField(FieldNames[i], Ascending[i]))
										return false;
								}

								if (!await Output.EndIndex())
									return false;
							}
						}

						await File.BeginRead();
						try
						{
							ObjectBTreeFileCursor<GenericObject> e = await File.GetTypedEnumeratorAsyncLocked<GenericObject>();
							GenericObject Obj;

							while (await e.MoveNextAsyncLocked())
							{
								if (e.CurrentTypeCompatible)
								{
									Obj = e.Current;

									if (!(Filter is null) && !Filter.CanExportObject(Obj))
										continue;

									if (await Output.StartObject(ObjectIdToString(Obj.ObjectId), Obj.TypeName) is null)
										return false;
									try
									{
										foreach (KeyValuePair<string, object> P in Obj)
										{
											if (!await Output.ReportProperty(P.Key, P.Value))
												return false;
										}
									}
									catch (Exception ex)
									{
										if (!await this.ReportException(ex, Output))
											return false;
									}
									finally
									{
										Continue = await Output.EndObject();
									}

									if (!Continue)
										return false;
								}
								else if (!(e.CurrentObjectId is null))
								{
									if (!await Output.ReportError("Unable to load object " + ObjectIdToString(e.CurrentObjectId) + "."))
										return false;
								}
							}
						}
						finally
						{
							await File.EndRead();
						}
					}
					catch (Exception ex)
					{
						if (!await this.ReportException(ex, Output))
							return false;
					}
					finally
					{
						Continue = !await Output.EndCollection();
					}

					if (!Continue)
						return false;
				}
			}
			catch (Exception ex)
			{
				Thread?.Exception(ex);
				if (!await this.ReportException(ex, Output))
					return false;
			}
			finally
			{
				Continue = await Output.EndDatabase();
				Thread?.Idle();
				Thread?.Stop();
			}

			return Continue;
		}

		internal static string ObjectIdToString(object ObjectId)
		{
			if (ObjectId is byte[] Bin)
				return Convert.ToBase64String(Bin);
			else
				return ObjectId.ToString();
		}

		private async Task<bool> ReportException(Exception ex, IDatabaseExport Output)
		{
			ex = Log.UnnestException(ex);

			if (ex is AggregateException ex2)
			{
				foreach (Exception ex3 in ex2.InnerExceptions)
				{
					if (!await Output.ReportException(ex3))
						return false;
				}

				return true;
			}
			else
				return await Output.ReportException(ex);
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

		#region Iterate

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames)
			where T : class
		{
			return this.Iterate(Recipient, CollectionNames, null);
		}

		/// <summary>
		/// Performs an iteration of contents of the entire database.
		/// </summary>
		/// <typeparam name="T">Type of objects to iterate.</typeparam>
		/// <param name="Recipient">Recipient of iterated objects.</param>
		/// <param name="CollectionNames">Optional array of collections to export. If null, all collections will be exported.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Task object for synchronization purposes.</returns>
		public async Task Iterate<T>(IDatabaseIteration<T> Recipient, string[] CollectionNames, ProfilerThread Thread)
			where T : class
		{
			ObjectBTreeFile[] Files = this.Files;

			Thread?.Start();
			await Recipient.StartDatabase();
			try
			{
				foreach (ObjectBTreeFile File in Files)
				{
					if (!(CollectionNames is null) && Array.IndexOf(CollectionNames, File.CollectionName) < 0)
						continue;

					Thread?.NewState(File.CollectionName);
					await Recipient.StartCollection(File.CollectionName);
					try
					{
						await File.BeginRead();
						try
						{
							ObjectBTreeFileCursor<T> e = await File.GetTypedEnumeratorAsyncLocked<T>();

							while (await e.MoveNextAsyncLocked())
							{
								if (e.CurrentTypeCompatible)
									await Recipient.ProcessObject(e.Current);
								else if (!(e.CurrentObjectId is null))
									await Recipient.IncompatibleObject(e.CurrentObjectId);
							}
						}
						finally
						{
							await File.EndRead();
						}
					}
					catch (Exception ex)
					{
						this.ReportException(ex, Recipient);
					}
					finally
					{
						await Recipient.EndCollection();
					}
				}
			}
			catch (Exception ex)
			{
				Thread?.Exception(ex);
				this.ReportException(ex, Recipient);
			}
			finally
			{
				await Recipient.EndDatabase();
				Thread?.Idle();
				Thread?.Stop();
			}
		}

		private void ReportException<T>(Exception ex, IDatabaseIteration<T> Recipient)
			where T : class
		{
			ex = Log.UnnestException(ex);

			if (ex is AggregateException ex2)
			{
				foreach (Exception ex3 in ex2.InnerExceptions)
					Recipient.ReportException(ex3);
			}
			else
				Recipient.ReportException(ex);
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
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, false, null, null);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, false, null, Thread);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, true, null, null);
		}

		/// <summary>
		/// Analyzes the database and repairs it if necessary. Results are exported to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Repair(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, ProfilerThread Thread)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, true, null, Thread);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, Repair, null, null);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors.</returns>
		public Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair,
			ProfilerThread Thread)
		{
			return this.Analyze(Output, XsltPath, ProgramDataFolder, ExportData, Repair, null, Thread);
		}

		/// <summary>
		/// Analyzes the database and exports findings to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		/// <param name="XsltPath">Optional XSLT to use to view the output.</param>
		/// <param name="ProgramDataFolder">Program data folder. Can be removed from filenames used, when referencing them in the report.</param>
		/// <param name="ExportData">If data in database is to be exported in output.</param>
		/// <param name="Repair">If files should be repaired if corruptions are detected.</param>
		/// <param name="CollectionNames">If provided, lists collections to be repaired.</param>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <returns>Collections with errors.</returns>
		private async Task<string[]> Analyze(XmlWriter Output, string XsltPath, string ProgramDataFolder, bool ExportData, bool Repair,
			string[] CollectionNames, ProfilerThread Thread)
		{
			SortedDictionary<string, bool> CollectionsWithErrors = new SortedDictionary<string, bool>();

			Thread?.Start();
			Output.WriteStartDocument();

			if (!string.IsNullOrEmpty(XsltPath))
			{
				if (File.Exists(XsltPath))
				{
					try
					{
						byte[] XsltBin = File.ReadAllBytes(XsltPath);

						Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"data:text/xsl;base64," +
							Convert.ToBase64String(XsltBin) + "\"");
					}
					catch (Exception)
					{
						Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + Encode(XsltPath) + "\"");
					}
				}
				else
					Output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + Encode(XsltPath) + "\"");

				this.xsltPath = XsltPath;
			}

			Output.WriteStartElement("DatabaseStatistics", "http://waher.se/Schema/Persistence/Statistics.xsd");

			foreach (ObjectBTreeFile File in this.Files)
			{
				if (!(CollectionNames is null) && Array.IndexOf(CollectionNames, File.CollectionName) < 0)
					continue;

				Thread?.NewState(File.CollectionName);
				await File.BeginWrite();
				try
				{
					Database.BeginRepair(File.CollectionName);

					KeyValuePair<FileStatistics, Dictionary<Guid, bool>> P = await File.ComputeStatisticsLocked();
					FileStatistics FileStat = P.Key;
					FileStatistics OldFileStat = FileStat;
					Dictionary<Guid, bool> ObjectIds;
					FileStatistics IndexStat;

					if (FileStat.IsCorrupt)
					{
						CollectionsWithErrors[File.CollectionName] = true;
						Thread?.Event("Corruption");
					}

					if (Repair && FileStat.IsCorrupt)
					{
						string[] Comments = FileStat.Comments;
						ProfilerThread RepairThread = Thread?.CreateSubThread("Repair " + File.CollectionName, ProfilerThreadType.Sequential);
						ChunkedList<Exception> Exceptions = null;
						string TempFileName = Path.GetTempFileName();
						string TempBtreeFileName = TempFileName + ".btree";
						string TempBlobFileName = TempFileName + ".blob";

						RepairThread?.Start();

						using (ObjectBTreeFile TempFile = await ObjectBTreeFile.Create(TempBtreeFileName, File.CollectionName, TempBlobFileName,
							File.BlockSize, File.BlobBlockSize, this, File.Encoding, File.TimeoutMilliseconds, File.Encrypted))
						{
							int c = 0;

							RepairThread?.NewState("Scan Blocks");

							ObjectIds = new Dictionary<Guid, bool>();

							await this.StartBulk();
							try
							{
								for (uint BlockIndex = 0; BlockIndex < FileStat.NrBlocks; BlockIndex++)
								{
									byte[] Block = await File.LoadBlockLocked(BlockIndex, false);
									BinaryDeserializer Reader = new BinaryDeserializer(File.CollectionName, this.encoding, Block, File.BlockLimit);
									BinaryDeserializer Reader2;
									BlockHeader Header = new BlockHeader(Reader);
									int Pos = 14;

									while (Reader.BytesLeft >= 4)
									{
										Reader.SkipBlockLink();
										object ObjectId = File.RecordHandler.GetKey(Reader);
										if (ObjectId is null)
											break;

										uint Len = await File.RecordHandler.GetFullPayloadSize(Reader);

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
												catch (Exception ex)
												{
													RepairThread?.Exception(ex);
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
													Obj = await File.GenericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false, true);
													Len2 = Pos2 - Reader2.Position;
												}
												catch (Exception)
												{
													Len2 = 0;
													Obj = null;
												}

												if (Len2 != 0)
													break;

												if (ObjectId is Guid Guid && !(Obj is null))
												{
													if (ObjectIds.ContainsKey(Guid))
														FileStat.LogError("Object with Object ID " + Guid.ToString() + " occurred multiple times.");
													else
													{
														try
														{
															await TempFile.SaveNewObject(Obj, true, null);
															ObjectIds[Guid] = true;
														}
														catch (Exception ex)
														{
															RepairThread?.Exception(ex);

															if (Exceptions is null)
																Exceptions = new ChunkedList<Exception>();

															Exceptions.Add(ex);
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
								await File.ClearAsyncLocked();

								foreach (IndexBTreeFile Index in File.Indices)
									await Index.ClearAsyncLocked();

								await this.StartBulk();
								c = 0;

								RepairThread?.NewState("Regenerate");

								await TempFile.BeginRead();
								try
								{
									ObjectBTreeFileCursor<object> e = await TempFile.GetTypedEnumeratorAsyncLocked<object>();

									while (await e.MoveNextAsyncLocked())
									{
										if (e.CurrentTypeCompatible)
										{
											object State = NestedLocks.CreateIfNested(File, true, e.CurrentSerializer);

											await File.SaveNewObjectLocked(e.Current, State);

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
								finally
								{
									await TempFile.EndRead();
								}
							}
							finally
							{
								await this.EndBulk();
							}

							foreach (Guid Guid in P.Value.Keys)
							{
								if (!ObjectIds.ContainsKey(Guid))
									FileStat.LogError("Unable to recover object with Object ID " + Guid.ToString());
							}
						}

#if COMPILED
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
						P = await File.ComputeStatisticsLocked();
						FileStat = P.Key;
						ObjectIds = P.Value;

						FileStat.LogComment("File was regenerated due to errors found:");

						foreach (string Comment in Comments)
							FileStat.LogComment(Comment);

						if (!(Exceptions is null))
						{
							foreach (Exception ex in Exceptions)
								FileStat.LogError(ex.Message);
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

						RepairThread?.Idle();
						RepairThread?.Stop();
					}
					else
						ObjectIds = P.Value;

					Output.WriteStartElement("File");
					Output.WriteAttributeString("id", File.Id.ToString());
					Output.WriteAttributeString("collectionName", File.CollectionName);
					Output.WriteAttributeString("fileName", GetRelativePath(ProgramDataFolder, File.FileName));
					Output.WriteAttributeString("blockSize", File.BlockSize.ToString());
					Output.WriteAttributeString("blobFileName", GetRelativePath(ProgramDataFolder, File.BlobFileName));
					Output.WriteAttributeString("blobBlockSize", File.BlobBlockSize.ToString());
					Output.WriteAttributeString("encoding", File.Encoding.WebName);
					Output.WriteAttributeString("encrypted", Encode(File.Encrypted));
					Output.WriteAttributeString("inlineObjectSizeLimit", File.InlineObjectSizeLimit.ToString());
					Output.WriteAttributeString("isReadOnly", Encode(File.IsReadOnly));
					Output.WriteAttributeString("timeoutMs", File.TimeoutMilliseconds.ToString());

					try
					{
						Output.WriteAttributeString("count", (await File.CountAsyncLocked).ToString());
					}
					catch (Exception ex)
					{
						Thread?.Exception(ex);
						Log.Alert(ex);
					}

					WriteStat(Output, FileStat);

					foreach (IndexBTreeFile Index in File.Indices)
					{
						Output.WriteStartElement("Index");
						Output.WriteAttributeString("id", Index.Id.ToString());
						Output.WriteAttributeString("fileName", GetRelativePath(ProgramDataFolder, Index.FileName));
						Output.WriteAttributeString("blockSize", Index.BlockSize.ToString());
						Output.WriteAttributeString("blobFileName", Index.BlobFileName);
						Output.WriteAttributeString("blobBlockSize", Index.BlobBlockSize.ToString());
						Output.WriteAttributeString("encoding", Index.Encoding.WebName);
						Output.WriteAttributeString("encrypted", Encode(Index.Encrypted));
						Output.WriteAttributeString("inlineObjectSizeLimit", Index.InlineObjectSizeLimit.ToString());
						Output.WriteAttributeString("isReadOnly", Encode(Index.IsReadOnly));
						Output.WriteAttributeString("timeoutMs", Index.TimeoutMilliseconds.ToString());

						try
						{
							Output.WriteAttributeString("count", (await Index.CountAsyncLocked).ToString());
						}
						catch (Exception ex)
						{
							Thread?.Exception(ex);
							Log.Alert(ex);
						}

						foreach (string Field in Index.FieldNames)
							Output.WriteElementString("Field", Field);

						IndexStat = await Index.ComputeStatisticsLocked(ObjectIds);

						if (IndexStat.NrObjects > FileStat.NrObjects)
							IndexStat.LogError("Too many objects in index.");

						if (Repair && (
							IndexStat.IsCorrupt ||
							OldFileStat.IsCorrupt ||
							FileStat.IsCorrupt))
						{
							await Index.RegenerateLocked();

							FileStatistics OldIndexStat = IndexStat;

							IndexStat = await Index.ComputeStatisticsLocked(ObjectIds);

							IndexStat.LogComment("Index was regenerated due to errors found.");
						}

						WriteStat(Output, IndexStat);

						Output.WriteEndElement();
					}

					if (ExportData)
						await File.ExportGraphXML(Output, true);

					Output.WriteEndElement();
				}
				catch (Exception ex)
				{
					Thread?.Exception(ex);
					Log.Alert(ex);
				}
				finally
				{
					Database.EndRepair(File.CollectionName);
					await File.EndWrite();
				}
			}

			Output.WriteEndElement();
			Output.WriteEndDocument();

			string[] Result = new string[CollectionsWithErrors.Count];
			CollectionsWithErrors.Keys.CopyTo(Result, 0);

			Thread?.Idle();
			Thread?.Stop();

			return Result;
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
		/// <returns>Collections with errors.</returns>
		public async Task<string[]> RepairIfInproperShutdown(string XsltPath)
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

			string[] Collections = Database.GetFlaggedCollectionNames();

			if (!Start.HasValue || !Stop.HasValue || Start.Value > Stop.Value)
			{
				s = this.GetReportFileName();

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

				return await this.Repair(s, XsltPath, (string[])null);
			}
			else if (Collections.Length > 0)
			{
				s = this.GetReportFileName();

				Log.Notice("Repairing flagged collections.",
					string.Empty, string.Empty, "AutoRepair", new KeyValuePair<string, object>("Report", s));

				return await this.Repair(s, XsltPath, Collections);
			}
			else
				return Array.Empty<string>();
		}

		private string GetReportFileName()
		{
			string s;

			if (string.IsNullOrEmpty(this.autoRepairReportFolder))
				s = this.folder + "AutoRepair";
			else
				s = this.autoRepairReportFolder;

			if (!Directory.Exists(s))
				Directory.CreateDirectory(s);

			return Path.Combine(s, "AutoRepair " + DateTime.Now.ToString("yyyy-MM-ddTHH.mm.ss.ffffff") + ".xml");
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(params string[] CollectionNames)
		{
			string ReportFileName = this.GetReportFileName();
			return this.Repair(ReportFileName, this.xsltPath, CollectionNames);
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(ProfilerThread Thread, params string[] CollectionNames)
		{
			string ReportFileName = this.GetReportFileName();
			return this.Repair(Thread, ReportFileName, this.xsltPath, CollectionNames);
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="XsltPath">Path to XSLT transform formatting the report.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(string XsltPath, params string[] CollectionNames)
		{
			string ReportFileName = this.GetReportFileName();
			return this.Repair(ReportFileName, XsltPath, CollectionNames);
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="ReportFileName">Filename of repair report.</param>
		/// <param name="XsltPath">Path to XSLT transform formatting the report.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public Task<string[]> Repair(string ReportFileName, string XsltPath, params string[] CollectionNames)
		{
			return this.Repair(null, ReportFileName, XsltPath, CollectionNames);
		}

		/// <summary>
		/// Repairs a set of collections.
		/// </summary>
		/// <param name="Thread">Optional Profiler thread.</param>
		/// <param name="ReportFileName">Filename of repair report.</param>
		/// <param name="XsltPath">Path to XSLT transform formatting the report.</param>
		/// <param name="CollectionNames">Set of collections to repair.</param>
		/// <returns>Collections repaired.</returns>
		public async Task<string[]> Repair(ProfilerThread Thread, string ReportFileName, string XsltPath, params string[] CollectionNames)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true,
				CloseOutput = true
			};

			using (FileStream fs = File.Create(ReportFileName))
			{
				using (XmlWriter w = XmlWriter.Create(fs, Settings))
				{
					return await this.Analyze(w, XsltPath, this.folder, false, true, CollectionNames, Thread);
				}
			}
		}

		/// <summary>
		/// Creates a new GUID.
		/// </summary>
		/// <returns>New GUID.</returns>
		public Guid CreateGuid()
		{
			return ObjectBTreeFile.CreateDatabaseGUID();
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
			await this.GetIndexFile(File, RegenerationOptions.RegenerateIfFileNotFound, FieldNames);
		}

		/// <summary>
		/// Removes an index from a collection, if one exist.
		/// </summary>
		/// <param name="CollectionName">Name of collection.</param>
		/// <param name="FieldNames">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		public async Task RemoveIndex(string CollectionName, string[] FieldNames)
		{
			ObjectBTreeFile File = await this.GetFile(CollectionName);
			IndexBTreeFile[] Indices = File.Indices;
			string[] Fields;
			int i, c;

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

				await this.RemoveIndex(File, I);
				break;
			}
		}

		private async Task RemoveIndex(ObjectBTreeFile File, IndexBTreeFile IndexFile)
		{
			string s = IndexFile.FileName;
			bool Exists = System.IO.File.Exists(s);

			if (!(IndexFile is null))
			{
				File.RemoveIndex(IndexFile);
				IndexFile.Dispose();
			}

			if (Exists)
				System.IO.File.Delete(s);

			StringBuilder sb = new StringBuilder();
			int i, c = IndexFile.FieldNames.Length;

			sb.AppendLine("Index");
			sb.AppendLine(File.CollectionName);

			for (i = 0; i < c; i++)
			{
				if (!IndexFile.Ascending[i])
					sb.Append('-');

				sb.AppendLine(IndexFile.FieldNames[i]);
			}

			if (s.StartsWith(this.folder))
				s = s.Substring(this.folder.Length);

			await this.master.RemoveAsync(s);
		}

		#endregion

		#region Life-Cycle

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		public Task Start()
		{
			this.WriteTimestamp("Start.txt");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		public async Task Stop()
		{
			lock (this.synchObj)
			{
				this.bulkCount = 0;
			}

			bool First = true;

			while (await this.SaveUnsaved(First))
				First = false;

			await this.DisposeAsync();
		}

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		public async Task Flush()
		{
			bool First = true;

			while (await this.SaveUnsaved(First))
				First = false;
		}

		private void WriteTimestamp(string FileName)
		{
			try
			{
				File.WriteAllText(Path.Combine(this.folder, FileName), DateTime.Now.Ticks.ToString());
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		#endregion

	}
}

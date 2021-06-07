#define ASSERT_LOCKS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Exceptions;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a binary file where objects are persisted in a B-tree.
	/// </summary>
	public class ObjectBTreeFile : IDisposable
	{
		internal const int BlockHeaderSize = 14;

		internal readonly MultiReadSingleWriteObject fileAccess;
		private IndexBTreeFile[] indices = new IndexBTreeFile[0];
		private List<IndexBTreeFile> indexList = new List<IndexBTreeFile>();
		private SortedDictionary<uint, bool> emptyBlocks = null;
		private readonly GenericObjectSerializer genericSerializer;
		private readonly FilesProvider provider;
		private readonly FileOfBlocks file;
		private readonly FileOfBlocks blobFile;
		private readonly Encoding encoding;
		private SortedDictionary<uint, byte[]> blocksToSave = null;
		private LinkedList<SaveRec> objectsToSave = null;
		private LinkedList<LoadRec> objectsToLoad = null;
		private readonly object synchObject = new object();
		private readonly IRecordHandler recordHandler;
		private long lockToken = long.MinValue;
		private ulong nrFullFileScans = 0;
		private ulong nrSearches = 0;
		private uint blocksAdded = 0;
		private ulong nrBlockLoads = 0;
		private ulong nrCacheLoads = 0;
		private ulong nrBlockSaves = 0;
		private ulong nrBlobBlockLoads = 0;
		private ulong nrBlobBlockSaves = 0;
		private ulong blockUpdateCounter = 0;
		private readonly string fileName;
		private readonly string collectionName;
		private readonly string blobFileName;
		private readonly int blockSize;
		private readonly int blobBlockSize;
		private readonly int inlineObjectSizeLimit;
		private readonly int timeoutMilliseconds;
		private readonly int id;
		private uint blockLimit;
		private uint blobBlockLimit;
		private bool emptyRoot = false;
		private readonly Aes aes;
		private byte[] aesKey;
		private byte[] ivSeed;
		private int ivSeedLen;
		private readonly bool encrypted;
		private readonly bool mainSynch;
		private bool indicesCreated = false;

		private enum WriteOp
		{
			Insert,
			Update,
			Delete,
			FindDelete
		}

		private class SaveRec
		{
			public object Object;
			public ObjectSerializer Serializer;
			public WriteOp Operation;
			public ObjectCallback ObjectCallback;
			public ObjectsCallback ObjectsCallback;

			public void Raise(object Object)
			{
				if (this.ObjectCallback is null)
					this.ObjectsCallback?.Invoke(new object[] { Object });
				else
					this.ObjectCallback(Object);
			}

			public void Raise(IEnumerable<object> Objects)
			{
				if (this.ObjectCallback is null)
					this.ObjectsCallback?.Invoke(Objects);
				else
				{
					foreach (object Object in Objects)
						this.ObjectCallback(Object);
				}
			}
		}

		private class LoadRec
		{
			public Guid ObjectId;
			public ObjectSerializer Serializer;
			public EmbeddedObjectSetter Setter;
		}

		private ObjectBTreeFile(string FileName, string CollectionName, string BlobFileName, int BlockSize,
			int BlobBlockSize, FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted,
			IRecordHandler RecordHandler, MultiReadSingleWriteObject FileAccess)
		{
			this.provider = Provider;
			this.id = Provider.GetNewFileId();
			this.fileName = Path.GetFullPath(FileName);
			this.collectionName = CollectionName;
			this.blobFileName = string.IsNullOrEmpty(BlobFileName) ? string.Empty : Path.GetFullPath(BlobFileName);
			this.blockSize = BlockSize;
			this.blobBlockSize = BlobBlockSize;
			this.inlineObjectSizeLimit = (BlockSize - BlockHeaderSize) / 2 - 4;
			this.encoding = Encoding;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.genericSerializer = new GenericObjectSerializer(Provider);
			this.encrypted = Encrypted;
			this.mainSynch = FileAccess is null;
			this.fileAccess = FileAccess ?? new MultiReadSingleWriteObject();

			if (RecordHandler is null)
				this.recordHandler = new PrimaryRecords(this.inlineObjectSizeLimit);
			else
				this.recordHandler = RecordHandler;

			if (this.encrypted)
			{
				this.aes = Aes.Create();
				this.aes.BlockSize = 128;
				this.aes.KeySize = 256;
				this.aes.Mode = CipherMode.CBC;
				this.aes.Padding = PaddingMode.None;
			}

			this.file = new FileOfBlocks(this.collectionName, FileName, this.blockSize);

			if (string.IsNullOrEmpty(this.blobFileName))
			{
				this.blobFile = null;
				this.blobBlockLimit = 0;
			}
			else
			{
				this.blobFile = new FileOfBlocks(this.collectionName, this.blobFileName, this.blobBlockSize);
				this.blobBlockLimit = this.blobFile.BlockLimit;
			}
		}

		/// <summary>
		/// This class manages a binary file where objects are persisted in a B-tree.
		/// </summary>
		/// <param name="FileName">Name of binary file. File will be created if it does not exist. The class will require
		/// unique read/write access to the file.</param>
		/// <param name="CollectionName">Name of collection corresponding to the file.</param>
		/// <param name="BlobFileName">Name of file in which BLOBs are stored.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Provider">Reference to the files provider.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		internal static Task<ObjectBTreeFile> Create(string FileName, string CollectionName, string BlobFileName, int BlockSize, int BlobBlockSize,
			FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
		{
			return Create(FileName, CollectionName, BlobFileName, BlockSize, BlobBlockSize, Provider, Encoding, TimeoutMilliseconds, Encrypted, null, null);
		}

		/// <summary>
		/// This class manages a binary file where objects are persisted in a B-tree.
		/// </summary>
		/// <param name="FileName">Name of binary file. File will be created if it does not exist. The class will require
		/// unique read/write access to the file.</param>
		/// <param name="CollectionName">Name of collection corresponding to the file.</param>
		/// <param name="BlobFileName">Name of file in which BLOBs are stored.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Objects larger than
		/// <see cref="InlineObjectSizeLimit"/> bytes will be stored as BLOBs.</param>
		/// <param name="BlobBlockSize">Size of a block in the BLOB file. The size must be a power of two. The BLOB file will consist
		/// of a doubly linked list of blocks of this size.</param>
		/// <param name="Provider">Reference to the files provider.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		/// <param name="RecordHandler">Record handler to use.</param>
		/// <param name="FileAccess">File Access synchronization object.</param>
		internal static async Task<ObjectBTreeFile> Create(string FileName, string CollectionName, string BlobFileName, int BlockSize,
			int BlobBlockSize, FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted,
			IRecordHandler RecordHandler, MultiReadSingleWriteObject FileAccess)
		{
			FileOfBlocks.CheckBlockSize(BlockSize);
			FileOfBlocks.CheckBlockSize(BlobBlockSize);

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentOutOfRangeException("The timeout must be positive.", nameof(TimeoutMilliseconds));

			ObjectBTreeFile Result = new ObjectBTreeFile(FileName, CollectionName, BlobFileName, BlockSize, BlobBlockSize, Provider,
				Encoding, TimeoutMilliseconds, Encrypted, RecordHandler, FileAccess);

			if (Result.encrypted)
			{
				KeyValuePair<byte[], byte[]> P = await Result.provider.GetKeys(Result.fileName, Result.file.FilePreExisting);
				Result.aesKey = P.Key;
				Result.ivSeed = P.Value;
				Result.ivSeedLen = Result.ivSeed.Length;
			}

			if (!Result.file.FilePreExisting || Result.file.Length == 0)
				await Result.CreateFirstBlock();

			Result.blockLimit = Result.file.BlockLimit;

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.file?.Dispose();

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile IndexFile in this.indices)
					IndexFile.Dispose();

				this.indices = null;
				this.indexList = null;
			}

			this.blobFile?.Dispose();

			this.provider.RemoveBlocks(this.id);

			this.fileAccess?.Dispose();
		}

		internal IRecordHandler RecordHandler => this.recordHandler;
		internal GenericObjectSerializer GenericSerializer => this.genericSerializer;
		internal MultiReadSingleWriteObject FileAccess => this.fileAccess;

		/// <summary>
		/// Identifier of the file.
		/// </summary>
		public int Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Reference to files provider.
		/// </summary>
		public FilesProvider Provider { get { return this.provider; } }

		/// <summary>
		/// Name of binary file.
		/// </summary>
		public string FileName { get { return this.fileName; } }

		/// <summary>
		/// Name of corresponding collection name.
		/// </summary>
		public string CollectionName { get { return this.collectionName; } }

		/// <summary>
		/// Name of file in which BLOBs are stored.
		/// </summary>
		public string BlobFileName { get { return this.blobFileName; } }

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding { get { return this.encoding; } }

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
		/// Maximum size of objects that are stored in-line. Larger objects will be stored as BLOBs.
		/// </summary>
		public int InlineObjectSizeLimit { get { return this.inlineObjectSizeLimit; } }

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

		internal GenericObjectSerializer GenericObjectSerializer
		{
			get { return this.genericSerializer; }
		}

		/// <summary>
		/// Block limit
		/// </summary>
		internal uint BlockLimit => this.blockLimit;

		/// <summary>
		/// BLOB Block Limit
		/// </summary>
		internal uint BlobBlockLimit => this.blobBlockLimit;

		/// <summary>
		/// If the file is the main synchronization file of a collection (true) or a secondary file (false).
		/// </summary>
		internal bool MainSynch => this.mainSynch;

		#region GUIDs for databases

		/// <summary>
		/// Creates a new GUID suitable for use in databases.
		/// </summary>
		/// <returns>New GUID.</returns>
		public static Guid CreateDatabaseGUID()
		{
			return guidGenerator.CreateGuid();
		}

		private readonly static SequentialGuidGenerator guidGenerator = new SequentialGuidGenerator();

		#endregion

		#region Locks

		/// <summary>
		/// Waits until object ready for reading.
		/// Each call to <see cref="BeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <exception cref="TimeoutException">If read access could not be given within the <see cref="TimeoutMilliseconds"/> time.</exception>
		public async Task BeginRead()
		{
			if (this.mainSynch)
			{
				if (!await this.fileAccess.TryBeginRead(this.timeoutMilliseconds))
					throw new TimeoutException("Unable to get read access to " + this.collectionName);

				this.lockToken = this.fileAccess.Token;
			}
			else
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for reading.
		/// Each successful call to <see cref="TryBeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public async Task<bool> TryBeginRead(int Timeout)
		{
			if (this.mainSynch)
			{
				bool Result = await this.fileAccess.TryBeginRead(Timeout);

				if (Result)
					this.lockToken = this.fileAccess.Token;

				return Result;
			}
			else
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");
		}

		/// <summary>
		/// Waits until object ready for writing.
		/// Each call to <see cref="BeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <exception cref="TimeoutException">If write access could not be given within the <see cref="TimeoutMilliseconds"/> time.</exception>
		public async Task BeginWrite()
		{
			if (this.mainSynch)
			{
				if (!await this.fileAccess.TryBeginWrite(this.timeoutMilliseconds))
					throw new TimeoutException("Unable to get write access to " + this.collectionName);

				this.lockToken = this.fileAccess.Token;
			}
			else
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until object ready for writing.
		/// Each successful call to <see cref="TryBeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public async Task<bool> TryBeginWrite(int Timeout)
		{
			if (this.mainSynch)
			{
				bool Result = await this.fileAccess.TryBeginWrite(Timeout);

				if (Result)
					this.lockToken = this.fileAccess.Token;

				return Result;
			}
			else
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");
		}

		/// <summary>
		/// Ends a reading session of the object.
		/// Must be called once for each call to <see cref="MultiReadSingleWriteObject.BeginRead"/> or successful call to 
		/// <see cref="MultiReadSingleWriteObject.TryBeginRead(int)"/>.
		/// </summary>
		/// <returns>Number of concurrent readers when returning from locked section of call.</returns>
		public async Task<int> EndRead()
		{
			if (this.mainSynch)
			{
				int Result = await this.fileAccess.EndRead();

				if (Result == 0)
					await this.CheckPending();

				return Result;
			}
			else
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");
		}

		/// <summary>
		/// Ends a writing session of the object.
		/// Must be called once for each call to <see cref="MultiReadSingleWriteObject.BeginWrite"/> or successful call to 
		/// <see cref="MultiReadSingleWriteObject.TryBeginWrite(int)"/>.
		/// </summary>
		public async Task EndWrite()
		{
			if (!this.mainSynch)
				throw new InvalidOperationException("Secondary files are automatically locked with the primary file.");

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile Index in this.indices)
					await Index.EndWritePriv();
			}

			await this.EndWritePriv();
			await this.CheckPending();
		}

		private async Task CheckPending()
		{
			LinkedList<SaveRec> ToSave;
			LinkedList<LoadRec> ToLoad;

			lock (this.synchObject)
			{
				ToSave = this.objectsToSave;
				this.objectsToSave = null;

				ToLoad = this.objectsToLoad;
				this.objectsToLoad = null;
			}

			if (!(ToSave is null))
			{
				foreach (SaveRec Rec in ToSave)
				{
					switch (Rec.Operation)
					{
						case WriteOp.Insert:
							await this.SaveNewObject(Rec.Object, Rec.Serializer, true, Rec.Raise);
							break;

						case WriteOp.Update:
							await this.UpdateObject(Rec.Object, Rec.Serializer, true, Rec.Raise);
							break;

						case WriteOp.Delete:
							await this.DeleteObject(Rec.Object, Rec.Serializer, true, Rec.Raise);
							break;

						case WriteOp.FindDelete:
							if (Rec.Object is FindDeleteLazyRec FindDeleteLazyRec)
							{
								int Offset = FindDeleteLazyRec.Offset;
								int MaxCount = FindDeleteLazyRec.MaxCount;
								Filter Filter = FindDeleteLazyRec.Filter;
								string[] SortOrder = FindDeleteLazyRec.SortOrder;
								ObjectSerializer Serializer = FindDeleteLazyRec.Serializer;

								if (Serializer is null)
									await this.FindDelete(Offset, MaxCount, Filter, true, SortOrder, Rec.Raise);
								else
								{
									if (await this.TryBeginWrite(0))
									{
										try
										{
											await this.FindDeleteLocked(FindDeleteLazyRec.T, Offset, MaxCount, Filter, Serializer, SortOrder);
										}
										finally
										{
											await this.EndWrite();
										}
									}
									else if (Rec.ObjectCallback is null)
										this.QueueForSave(FindDeleteLazyRec, Serializer, Rec.ObjectsCallback, Rec.Operation);
									else
										this.QueueForSave(FindDeleteLazyRec, Serializer, Rec.ObjectCallback, Rec.Operation);
								}
							}
							break;
					}
				}
			}

			if (!(ToLoad is null))
			{
				foreach (LoadRec Rec in ToLoad)
					Rec.Setter(await this.LoadObject(Rec.ObjectId, Rec.Serializer));
			}
		}

		internal async Task EndWritePriv()
		{
			bool EmptyBlocks = !(this.emptyBlocks is null);
			bool SaveBlocks = !(this.blocksToSave is null) && this.blocksToSave.Count > 0 && !this.provider.InBulkMode(this);

			if (this.emptyRoot || EmptyBlocks || SaveBlocks)
			{
				try
				{
					if (this.emptyRoot)
					{
						this.emptyRoot = false;

						byte[] Block = await this.LoadBlockLocked(0, true);
						BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
						BlockHeader Header = new BlockHeader(Reader);
						uint BlockIndex;

						while (Header.BytesUsed == 0 && (BlockIndex = Header.LastBlockIndex) != 0)
						{
							Block = await this.LoadBlockLocked(BlockIndex, true);
							Reader.Restart(Block, 0);
							Header = new BlockHeader(Reader);

							this.RegisterEmptyBlockLocked(BlockIndex);
						}

						Array.Clear(Block, 10, 4);
						this.QueueSaveBlockLocked(0, Block);

						await this.UpdateParentLinksLocked(0, Block);
					}

					if (EmptyBlocks)
						await this.RemoveEmptyBlocksLocked();

					if (SaveBlocks)
						await this.SaveUnsavedLocked();
				}
				finally
				{
					if (this.mainSynch)
						await this.fileAccess.EndWrite();
				}
			}
			else if (this.mainSynch)
				await this.fileAccess.EndWrite();
		}

		private async Task SaveUnsavedLocked()
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (!(this.blocksToSave is null))
			{
				bool Changed = false;

				foreach (KeyValuePair<uint, byte[]> Rec in this.blocksToSave)
				{
					await this.DoSaveBlockLocked(Rec.Key, Rec.Value);
					Changed = true;
				}

				if (Changed)
				{
					this.blocksToSave.Clear();
					this.blocksAdded = 0;
					this.blockLimit = this.file.BlockLimit;
					await this.file.FlushAsync();
				}
			}
		}

		#endregion

		#region Blocks

		private async Task<Tuple<uint, byte[]>> CreateNewBlockLocked()
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = null;
			uint BlockIndex = uint.MaxValue;

			if (!(this.emptyBlocks is null))
			{
				foreach (uint BlockIndex2 in this.emptyBlocks.Keys)
				{
					this.emptyBlocks.Remove(BlockIndex2);
					if (this.emptyBlocks.Count == 0)
						this.emptyBlocks = null;

					Block = await this.LoadBlockLocked(BlockIndex2, true);
					BlockIndex = BlockIndex2;

					Array.Clear(Block, 0, this.blockSize);

					break;
				}
			}

			if (Block is null)
			{
				Block = new byte[this.blockSize];
				BlockIndex = this.blockLimit;

				this.blocksAdded++;
				this.blockLimit++;
			}

			this.QueueSaveBlockLocked(BlockIndex, Block);

			return new Tuple<uint, byte[]>(BlockIndex, Block);
		}

		private async Task CreateFirstBlock()
		{
			if (this.mainSynch)
				await this.BeginWrite();

			try
			{
				await this.CreateNewBlockLocked();
			}
			finally
			{
				if (this.mainSynch)
					await this.EndWrite();
			}
		}

		/// <summary>
		/// Clears the internal memory cache.
		/// </summary>
		public void ClearCache()
		{
			this.provider.RemoveBlocks(this.id);
		}

		/// <summary>
		/// Loads a block from the file.
		/// </summary>
		/// <param name="BlockIndex">Index of block to load.</param>
		/// <returns>Loaded block.</returns>
		public async Task<byte[]> LoadBlock(uint BlockIndex)
		{
			bool NeedLock = this.lockToken != this.fileAccess.Token;

			if (NeedLock)
				await this.BeginRead();
			try
			{
				return await this.LoadBlockLocked(BlockIndex, true);
			}
			finally
			{
				if (NeedLock)
					await this.EndRead();
			}
		}

		internal async Task<byte[]> LoadBlockLocked(uint BlockIndex, bool AddToCache)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			if (this.provider.TryGetBlock(this.id, BlockIndex, out byte[] Block))
			{
				this.nrCacheLoads++;
				return Block;
			}

			if (!(this.blocksToSave is null) && this.blocksToSave.TryGetValue(BlockIndex, out Block))
			{
				this.nrCacheLoads++;
				return Block;
			}

			Block = await this.file.LoadBlock(BlockIndex);
			this.nrBlockLoads++;

			if (this.encrypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)BlockIndex) * this.blockSize)))
				{
					Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}
			}

			if (AddToCache)
				this.provider.AddBlockToCache(this.id, BlockIndex, Block);

			return Block;
		}

		/// <summary>
		/// Saves a block to the file.
		/// </summary>
		/// <param name="BlockIndex">Block index of block in file.</param>
		/// <param name="Block">Block to save.</param>
		/// <returns>Block to save.</returns>
		public async Task SaveBlock(uint BlockIndex, byte[] Block)
		{
			await this.BeginWrite();
			try
			{
				this.QueueSaveBlockLocked(BlockIndex, Block);
			}
			finally
			{
				await this.EndWrite();
			}
		}

		internal void QueueSaveBlockLocked(uint BlockIndex, byte[] Block)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (Block is null || Block.Length != this.blockSize)
				throw Database.FlagForRepair(this.collectionName, "Block not of the correct block size.");

			if (this.provider.TryGetBlock(this.id, BlockIndex, out byte[] PrevBlock) && PrevBlock != Block)
			{
				if (Array.Equals(PrevBlock, Block))
				{
					this.provider.AddBlockToCache(this.id, BlockIndex, Block);   // Update to new reference.
					return;     // No need to save.
				}
			}

			if (this.blocksToSave is null)
				this.blocksToSave = new SortedDictionary<uint, byte[]>();

			this.blocksToSave[BlockIndex] = Block;
			this.blockUpdateCounter++;

			this.provider.AddBlockToCache(this.id, BlockIndex, Block);
		}

		/// <summary>
		/// This counter gets updated each time a block is updated in the file.
		/// </summary>
		internal ulong BlockUpdateCounter
		{
			get { return this.blockUpdateCounter; }
		}

		internal async Task DoSaveBlockLocked(uint BlockIndex, byte[] Block)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] EncryptedBlock;

			if (this.encrypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(((long)BlockIndex) * this.blockSize)))
				{
					EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}
			}
			else
				EncryptedBlock = (byte[])Block.Clone();

			await this.file.SaveBlock(BlockIndex, EncryptedBlock);

			this.nrBlockSaves++;
		}

		private byte[] GetIV(long Position)
		{
			byte[] Input = new byte[this.ivSeedLen + 8];
			Array.Copy(this.ivSeed, 0, Input, 0, this.ivSeedLen);
			Array.Copy(BitConverter.GetBytes(Position), 0, Input, this.ivSeedLen, 8);
			byte[] Hash;

			using (SHA1 Sha1 = SHA1.Create())
			{
				Hash = Sha1.ComputeHash(Input);
			}

			Array.Resize<byte>(ref Hash, 16);

			return Hash;
		}

		private void RegisterEmptyBlockLocked(uint Block)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (this.emptyBlocks is null)
				this.emptyBlocks = new SortedDictionary<uint, bool>(new ReverseOrder());

			this.emptyBlocks[Block] = true;
		}

		private class ReverseOrder : IComparer<uint>
		{
			public int Compare(uint x, uint y)
			{
				return y.CompareTo(x);
			}
		}

		private async Task RemoveEmptyBlocksLocked()
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (!(this.emptyBlocks is null))
			{
				BinaryDeserializer Reader;
				BlockHeader Header;
				uint DestinationIndex;
				uint SourceIndex;
				byte[] Block;
				uint PrevBlockIndex;
				uint ParentBlockIndex;

				foreach (uint BlockIndex in this.emptyBlocks.Keys)
				{
					DestinationIndex = BlockIndex;
					SourceIndex = (uint)(this.file.BlockLimit + this.blocksAdded - 1);

					if (DestinationIndex < SourceIndex)
					{
						PrevBlockIndex = SourceIndex;

						Block = await this.LoadBlockLocked(SourceIndex, false);

						if (!(this.blocksToSave is null))
							this.blocksToSave.Remove(SourceIndex);

						this.provider.RemoveBlock(this.id, SourceIndex);

						this.QueueSaveBlockLocked(DestinationIndex, Block);
						await this.UpdateParentLinksLocked(BlockIndex, Block);

						ParentBlockIndex = BitConverter.ToUInt32(Block, 10);
						SourceIndex = ParentBlockIndex;
						Block = await this.LoadBlockLocked(SourceIndex, true);
						Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
						Header = new BlockHeader(Reader);

						if (Header.LastBlockIndex == PrevBlockIndex)
							Array.Copy(BitConverter.GetBytes(BlockIndex), 0, Block, 6, 4);
						else
						{
							await this.ForEachObject(Block, (Link, ObjectId, Pos, Len) =>
							{
								if (Link == PrevBlockIndex)
								{
									Array.Copy(BitConverter.GetBytes(BlockIndex), 0, Block, Pos - 4, 4);
									return false;
								}
								else
									return true;
							});
						}

						this.QueueSaveBlockLocked(SourceIndex, Block);
					}
					else
					{
						if (!(this.blocksToSave is null))
							this.blocksToSave.Remove(DestinationIndex);

						this.provider.RemoveBlock(this.id, DestinationIndex);

						if (SourceIndex != DestinationIndex)
						{
							if (!(this.blocksToSave is null))
								this.blocksToSave.Remove(SourceIndex);

							this.provider.RemoveBlock(this.id, SourceIndex);
						}
					}

					if (this.blocksAdded > 0)
						this.blocksAdded--;
					else
						await this.file.Truncate(this.file.BlockLimit - 1);

					this.blockLimit--;
				}

				this.emptyBlocks = null;
			}
		}

		#endregion

		#region BLOBs

		internal async Task<byte[]> SaveBlobLocked(byte[] Bin)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (this.blobFile is null)
				throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.blockLimit);
			this.recordHandler.SkipKey(Reader);
			int KeySize = Reader.Position;
			int Len = (int)await this.recordHandler.GetFullPayloadSize(Reader);
			int HeaderSize = Reader.Position;

			if (Len != Bin.Length - Reader.Position)
				throw Database.FlagForRepair(this.collectionName, "Invalid serialization of object");

			this.blobBlockLimit = this.blobFile.BlockLimit;

			byte[] Result = new byte[HeaderSize + 4];
			byte[] EncryptedBlock;
			Array.Copy(Bin, 0, Result, 0, HeaderSize);
			Array.Copy(BitConverter.GetBytes(this.blobBlockLimit), 0, Result, HeaderSize, 4);
			byte[] Block = new byte[this.blobBlockSize];
			int Left;
			uint Prev = uint.MaxValue;
			int Limit = this.blobBlockSize - KeySize - 8;
			int Pos = HeaderSize;
			uint BlobBlockIndex = this.blobFile.BlockLimit;

			Array.Copy(Bin, 0, Block, 0, KeySize);

			Len += HeaderSize;
			while (Pos < Len)
			{
				Array.Copy(BitConverter.GetBytes(Prev), 0, Block, KeySize, 4);
				Prev = this.blobBlockLimit;

				Left = Len - Pos;
				if (Left <= Limit)
				{
					Array.Copy(BitConverter.GetBytes(uint.MaxValue), 0, Block, KeySize + 4, 4);
					Array.Copy(Bin, Pos, Block, KeySize + 8, Left);
					if (Left < Limit)
						Array.Clear(Block, (int)(KeySize + 8 + Left), (int)(Limit - Left));

					Pos += Left;
				}
				else
				{
					Array.Copy(BitConverter.GetBytes(++this.blobBlockLimit), 0, Block, KeySize + 4, 4);
					Array.Copy(Bin, Pos, Block, KeySize + 8, Limit);
					Pos += Limit;
				}

				if (this.encrypted)
				{
					using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
					{
						EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
					}
				}
				else
					EncryptedBlock = (byte[])Block.Clone();

				await this.blobFile.SaveBlock(BlobBlockIndex++, EncryptedBlock);
				this.nrBlobBlockSaves++;
			}

			return Result;
		}

		internal async Task<BinaryDeserializer> LoadBlobLocked(byte[] Block, int Pos, BitArray BlobBlocksReferenced, FileStatistics Statistics)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			if (this.blobFile is null)
				throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

			try
			{
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, Pos);
				object ObjectId = this.recordHandler.GetKey(Reader);
				object ObjectId2;
				int KeySize = Reader.Position - Pos;
				uint Len = await this.recordHandler.GetFullPayloadSize(Reader);
				int Bookmark = Reader.Position - Pos;
				uint BlobBlockIndex = Reader.ReadUInt32();
				uint ExpectedPrev = uint.MaxValue;
				uint Prev;
				byte[] Result = new byte[Bookmark + Len];
				byte[] BlobBlock = new byte[this.blobBlockSize];
				byte[] DecryptedBlock;
				int i = Bookmark;
				int NrRead;
				bool ChainError = false;

				Array.Copy(Block, Pos, Result, 0, Bookmark);
				Len += (uint)Bookmark;

				while (i < Len)
				{
					if (BlobBlockIndex == uint.MaxValue)
						throw Database.FlagForRepair(this.collectionName, "BLOB " + ObjectId.ToString() + " ended prematurely.");

					await this.blobFile.LoadBlock(BlobBlockIndex, BlobBlock);
					this.nrBlobBlockLoads++;

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
						{
							DecryptedBlock = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
						}
					}
					else
						DecryptedBlock = (byte[])BlobBlock.Clone();

					Reader.Restart(DecryptedBlock, 0);
					ObjectId2 = this.recordHandler.GetKey(Reader);
					if (ObjectId2 is null || this.recordHandler.Compare(ObjectId2, ObjectId) != 0)
					{
						throw Database.FlagForRepair(this.collectionName, "Block linked to by BLOB " + ObjectId.ToString() + " (" + this.collectionName +
							") was actually marked as " + ObjectId2.ToString() + ".");
					}

					Prev = Reader.ReadUInt32();
					if (Prev != ExpectedPrev)
						ChainError = true;

					ExpectedPrev = BlobBlockIndex;

					if (!(BlobBlocksReferenced is null))
						BlobBlocksReferenced[(int)BlobBlockIndex] = true;

					BlobBlockIndex = Reader.ReadUInt32();

					NrRead = Math.Min(this.blobBlockSize - KeySize - 8, (int)(Len - i));

					Array.Copy(DecryptedBlock, KeySize + 8, Result, i, NrRead);
					i += NrRead;

					if (!(Statistics is null))
						Statistics.ReportBlobBlockStatistics((uint)(KeySize + 8 + NrRead), (uint)(this.blobBlockSize - NrRead - KeySize - 8));
				}

				if (BlobBlockIndex != uint.MaxValue)
					throw Database.FlagForRepair(this.collectionName, "BLOB " + ObjectId.ToString() + " did not end when expected.");

				if (!(BlobBlocksReferenced is null) && ChainError)
					throw Database.FlagForRepair(this.collectionName, "Doubly linked list for BLOB " + ObjectId.ToString() + " is corrupt.");

				Reader.Restart(Result, Bookmark);

				return Reader;
			}
			catch (OutOfMemoryException ex)
			{
				throw Database.FlagForRepair(this.collectionName, ex.Message);
			}
		}

		internal async Task DeleteBlobLocked(byte[] Bin, int Offset)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (this.blobFile is null)
				throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

			SortedDictionary<uint, bool> BlocksToRemoveSorted = new SortedDictionary<uint, bool>();
			LinkedList<Tuple<uint, int, byte[]>> ReplacementBlocks = new LinkedList<Tuple<uint, int, byte[]>>();
			Dictionary<uint, uint> TranslationFromTo = new Dictionary<uint, uint>();
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.blobBlockLimit, Offset);
			uint[] BlocksToRemove;
			byte[] BlobBlock = new byte[this.blobBlockSize];
			byte[] DecryptedBlock;
			byte[] DecryptedBlock2;
			byte[] EncryptedBlock;
			object ObjectId = this.recordHandler.GetKey(Reader);
			object ObjectId2;
			BlockInfo Info;
			uint BlobBlockIndex;
			uint Index, Prev, Next, To, Len;
			int i, c;
			int KeySize1 = Reader.Position - Offset;
			int KeySize2;

			await this.recordHandler.GetFullPayloadSize(Reader);
			BlobBlockIndex = Reader.ReadUInt32();

			while (BlobBlockIndex != uint.MaxValue)
			{
				await this.blobFile.LoadBlock(BlobBlockIndex, BlobBlock);
				this.nrBlockLoads++;

				if (this.encrypted)
				{
					using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
					{
						DecryptedBlock = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
					}
				}
				else
					DecryptedBlock = (byte[])BlobBlock.Clone();

				Reader.Restart(DecryptedBlock, 0);
				ObjectId2 = this.recordHandler.GetKey(Reader);
				if (ObjectId2 is null || this.recordHandler.Compare(ObjectId2, ObjectId) != 0)
					break;

				BlocksToRemoveSorted[BlobBlockIndex] = true;

				BlobBlockIndex = BitConverter.ToUInt32(DecryptedBlock, KeySize1 + 4);
			}

			c = BlocksToRemoveSorted.Count;
			BlocksToRemove = new uint[c];
			BlocksToRemoveSorted.Keys.CopyTo(BlocksToRemove, 0);

			BlobBlockIndex = this.blobFile.BlockLimit;

			for (i = c - 1; i >= 0; i--)
			{
				if (BlobBlockIndex == 0)
					break;

				BlobBlockIndex--;
				await this.blobFile.LoadBlock(BlobBlockIndex, BlobBlock);
				this.nrBlockLoads++;

				if (this.encrypted)
				{
					using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
					{
						DecryptedBlock = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
					}
				}
				else
					DecryptedBlock = (byte[])BlobBlock.Clone();

				Reader.Restart(DecryptedBlock, 0);
				ObjectId2 = this.recordHandler.GetKey(Reader);
				if (ObjectId2 is null || this.recordHandler.Compare(ObjectId2, ObjectId) != 0)
					ReplacementBlocks.AddFirst(new Tuple<uint, int, byte[]>(BlobBlockIndex, Reader.Position, DecryptedBlock));
			}

			i = 0;
			foreach (Tuple<uint, int, byte[]> ReplacementBlock in ReplacementBlocks)
			{
				BlobBlockIndex = BlocksToRemove[i++];   // To
				Index = ReplacementBlock.Item1;         // From

				TranslationFromTo[Index] = BlobBlockIndex;
			}

			i = 0;
			foreach (Tuple<uint, int, byte[]> ReplacementBlock in ReplacementBlocks)
			{
				BlobBlockIndex = BlocksToRemove[i++];   // To

				Index = ReplacementBlock.Item1;           // From
				KeySize2 = ReplacementBlock.Item2;
				DecryptedBlock = ReplacementBlock.Item3;

				Prev = BitConverter.ToUInt32(DecryptedBlock, KeySize2);
				Next = BitConverter.ToUInt32(DecryptedBlock, KeySize2 + 4);

				if (Prev == uint.MaxValue)
				{
					Reader.Restart(DecryptedBlock, 0);
					ObjectId2 = this.recordHandler.GetKey(Reader);

					Info = await this.FindNodeLocked(ObjectId2);
					if (!(Info is null))
					{
						Reader.Restart(Info.Block, Info.InternalPosition + 4);
						if (this.recordHandler.Compare(ObjectId2, this.recordHandler.GetKey(Reader)) == 0)
						{
							Len = await this.recordHandler.GetFullPayloadSize(Reader);
							if (Reader.Position - Info.InternalPosition - 4 + Len > this.inlineObjectSizeLimit)
							{
								Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, Info.Block, Reader.Position, 4);
								this.QueueSaveBlockLocked(Info.BlockIndex, Info.Block);
							}
						}
					}
				}
				else if (TranslationFromTo.TryGetValue(Prev, out To))
					Array.Copy(BitConverter.GetBytes(To), 0, DecryptedBlock, KeySize2, 4);
				else
				{
					await this.blobFile.LoadBlock(Prev, BlobBlock);
					this.nrBlockLoads++;

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)Prev) * this.blobBlockSize)))
						{
							DecryptedBlock2 = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
						}
					}
					else
						DecryptedBlock2 = BlobBlock;

					Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, DecryptedBlock2, KeySize2 + 4, 4);

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(((long)Prev) * this.blobBlockSize)))
						{
							EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock2, 0, DecryptedBlock2.Length);
						}
					}
					else
						EncryptedBlock = (byte[])DecryptedBlock2.Clone();

					await this.blobFile.SaveBlock(Prev, EncryptedBlock);
					this.nrBlockSaves++;
				}

				if (TranslationFromTo.TryGetValue(Next, out To))
					Array.Copy(BitConverter.GetBytes(To), 0, DecryptedBlock, KeySize2 + 4, 4);
				else if (Next != uint.MaxValue)
				{
					await this.blobFile.LoadBlock(Next, BlobBlock);
					this.nrBlockLoads++;

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)Next) * this.blobBlockSize)))
						{
							DecryptedBlock2 = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
						}
					}
					else
						DecryptedBlock2 = BlobBlock;

					Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, DecryptedBlock2, KeySize2, 4);

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(((long)Next) * this.blobBlockSize)))
						{
							EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock2, 0, DecryptedBlock2.Length);
						}
					}
					else
						EncryptedBlock = (byte[])DecryptedBlock2.Clone();

					await this.blobFile.SaveBlock(Next, EncryptedBlock);
					this.nrBlockSaves++;
				}

				if (this.encrypted)
				{
					using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
					{
						EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock, 0, DecryptedBlock.Length);
					}
				}
				else
					EncryptedBlock = (byte[])DecryptedBlock.Clone();

				await this.blobFile.SaveBlock(BlobBlockIndex, EncryptedBlock);
				this.nrBlockSaves++;
			}

			await this.blobFile.Truncate((uint)(this.blobFile.BlockLimit - BlocksToRemove.Length));
			this.blobBlockLimit = this.blobFile.BlockLimit;
		}

		#endregion

		#region Save new objects

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Lazy">If Lazy insert is used, i.e. sufficiant that object is inserted at next opportuity.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task<Guid> SaveNewObject(object Object, bool Lazy, ObjectCallback Callback)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(ObjectType);

			return await this.SaveNewObject(Object, Serializer, Lazy, Callback);
		}

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
		/// <param name="Lazy">If Lazy insert is used, i.e. sufficiant that object is inserted at next opportuity.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task<Guid> SaveNewObject(object Object, ObjectSerializer Serializer, bool Lazy, ObjectCallback Callback)
		{
			Guid ObjectId;

			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						ObjectId = await this.SaveNewObjectLocked(Object, Serializer, NestedLocks.CreateIfNested(this, true, Serializer));
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Object, Serializer, Callback, WriteOp.Insert);
					return Guid.Empty;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					ObjectId = await this.SaveNewObjectLocked(Object, Serializer, NestedLocks.CreateIfNested(this, true, Serializer));
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Object);

			return ObjectId;
		}

		/// <summary>
		/// Saves a new object to the file (which is locked).
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		internal async Task<Guid> SaveNewObjectLocked(object Object, object State)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(ObjectType);

			return await this.SaveNewObjectLocked(Object, Serializer, State);
		}

		/// <summary>
		/// Saves a new set of objects to the file.
		/// </summary>
		/// <param name="Objects">Objects to persist.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
		/// <param name="Lazy">If Lazy insert is used, i.e. sufficiant that object is inserted at next opportuity.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		public async Task SaveNewObjects(IEnumerable<object> Objects, ObjectSerializer Serializer, bool Lazy, ObjectsCallback Callback)
		{
			LinkedList<Guid> ObjectIds = new LinkedList<Guid>();

			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					NestedLocks State = NestedLocks.CreateIfNested(this, true, Serializer);

					try
					{
						foreach (object Object in Objects)
							await this.SaveNewObjectLocked(Object, Serializer, State);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Objects, Serializer, Callback, WriteOp.Insert);
					return;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					NestedLocks State = NestedLocks.CreateIfNested(this, true, Serializer);

					foreach (object Object in Objects)
						ObjectIds.AddLast(await this.SaveNewObjectLocked(Object, Serializer, State));
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Objects);
		}

		internal async Task<Guid> SaveNewObjectLocked(object Object, ObjectSerializer Serializer, object State)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			BinarySerializer Writer;
			Guid ObjectId;
			byte[] Bin;

			Tuple<Guid, BlockInfo> Rec = await this.PrepareObjectIdForSaveLocked(Object, Serializer);
			ObjectId = Rec.Item1;
			BlockInfo Leaf = Rec.Item2;
			bool Nested = Serializer.HasByReference;

			if (Nested && State is null)
				State = NestedLocks.CreateIfNested(this, true, Serializer);

			Writer = new BinarySerializer(this.collectionName, this.encoding);
			await Serializer.Serialize(Writer, false, false, Object, State);
			Bin = Writer.GetSerialization();

			if (Serializer.HasByReference && State is NestedLocks Locks && Locks.HasBeenTouched(this))
				Leaf = await this.FindLeafNodeLocked(ObjectId);

			await this.SaveNewObjectLocked(Bin, Leaf);

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile Index in this.indices)
					await Index.SaveNewObjectLocked(ObjectId, Object, Serializer);
			}

			return ObjectId;
		}

		internal async Task SaveNewObjectLocked(byte[] Bin, BlockInfo Info)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (Bin.Length > this.inlineObjectSizeLimit)
			{
				byte[] BlobReference = await this.SaveBlobLocked(Bin);
				BlobReference = this.recordHandler.EncodeBlobReference(BlobReference, Bin);

				if (BlobReference.Length > this.inlineObjectSizeLimit)
				{
					await this.DeleteBlobLocked(BlobReference, 0);
					throw new ArgumentException("Key too long.");
				}

				Bin = BlobReference;
			}

			await this.InsertObjectLocked(Info.BlockIndex, Info.Header, Info.Block, Bin, Info.InternalPosition, 0, 0, true, Info.LastObject);
		}

		private void QueueForSave(object Object, ObjectSerializer Serializer, ObjectCallback Callback, WriteOp Operation)
		{
			lock (this.synchObject)
			{
				if (this.objectsToSave is null)
					this.objectsToSave = new LinkedList<SaveRec>();

				this.objectsToSave.AddLast(new SaveRec()
				{
					Object = Object,
					Serializer = Serializer,
					Operation = Operation,
					ObjectCallback = Callback
				});
			}
		}

		private void QueueForSave(IEnumerable<object> Objects, ObjectSerializer Serializer, ObjectsCallback Callback, WriteOp Operation)
		{
			lock (this.synchObject)
			{
				if (this.objectsToSave is null)
					this.objectsToSave = new LinkedList<SaveRec>();

				foreach (object Object in Objects)
				{
					this.objectsToSave.AddLast(new SaveRec()
					{
						Object = Object,
						Serializer = Serializer,
						Operation = Operation,
						ObjectsCallback = Callback
					});
				}
			}
		}

		private void QueueForSave(FindDeleteLazyRec Rec, ObjectSerializer Serializer, ObjectsCallback Callback, WriteOp Operation)
		{
			lock (this.synchObject)
			{
				if (this.objectsToSave is null)
					this.objectsToSave = new LinkedList<SaveRec>();

				this.objectsToSave.AddLast(new SaveRec()
				{
					Object = Rec,
					Serializer = Serializer,
					Operation = Operation,
					ObjectsCallback = Callback
				});
			}
		}

		private async Task<Tuple<Guid, BlockInfo>> PrepareObjectIdForSaveLocked(object Object, ObjectSerializer Serializer)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			bool HasObjectId = await Serializer.HasObjectId(Object);
			BlockInfo Leaf;
			Guid ObjectId;

			if (HasObjectId)
			{
				ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
				Leaf = await this.FindLeafNodeLocked(ObjectId);

				if (Leaf is null)
					throw new KeyAlreadyExistsException("Object with same Object ID already exists.", this.collectionName);
			}
			else
			{
				do
				{
					ObjectId = CreateDatabaseGUID();
				}
				while ((Leaf = await this.FindLeafNodeLocked(ObjectId)) is null);

				if (!await Serializer.TrySetObjectId(Object, ObjectId))
					throw new NotSupportedException("Unable to set Object ID.");
			}

			return new Tuple<Guid, BlockInfo>(ObjectId, Leaf);
		}

		internal async Task InsertObjectLocked(uint BlockIndex, BlockHeader Header, byte[] Block, byte[] Bin, int InsertAt,
			uint ChildRightLink, uint ChildRightLinkSize, bool IncSize, bool LastObject)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			int Used = Header.BytesUsed;
			int PayloadSize = (int)(Used + 4 + Bin.Length);

			if (BlockHeaderSize + PayloadSize <= this.blockSize)      // Add object to current node
			{
				if (InsertAt < BlockHeaderSize + Used)
				{
					InsertAt += 4;
					Array.Copy(Block, InsertAt, Block, InsertAt + 4 + Bin.Length, BlockHeaderSize + Used - InsertAt);
					Array.Copy(Bin, 0, Block, InsertAt, Bin.Length);
					InsertAt += Bin.Length;
					Array.Copy(BitConverter.GetBytes(ChildRightLink), 0, Block, InsertAt, 4);
				}
				else
				{
					Array.Copy(Block, 6, Block, InsertAt, 4);   // Last block link
					InsertAt += 4;
					Array.Copy(Bin, 0, Block, InsertAt, Bin.Length);
					Array.Copy(BitConverter.GetBytes(ChildRightLink), 0, Block, 6, 4);  // New last block link
				}

				Header.BytesUsed += (ushort)(4 + Bin.Length);
				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

				if (IncSize && Header.SizeSubtree < uint.MaxValue)
				{
					Header.SizeSubtree++;
					Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);
				}

				this.QueueSaveBlockLocked(BlockIndex, Block);

				if (IncSize)
				{
					while (BlockIndex != 0)
					{
						BlockIndex = Header.ParentBlockIndex;

						Block = await this.LoadBlockLocked(BlockIndex, true);
						BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
						Header = new BlockHeader(Reader);

						if (Header.SizeSubtree >= uint.MaxValue)
							break;

						Header.SizeSubtree++;

						Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);
						this.QueueSaveBlockLocked(BlockIndex, Block);
					}
				}
			}
			else                                                                    // Split node.
			{
				BlockSplitter Splitter;

				if (LastObject)
					Splitter = new BlockSplitterLast(this.blockSize);
				else
					Splitter = new BlockSplitterMiddle(PayloadSize);

				Tuple<uint, byte[]> Left;
				Tuple<uint, byte[]> Right = await this.CreateNewBlockLocked();
				uint LeftLink;
				uint RightLink = Right.Item1;
				bool CheckParentLinksLeft = false;
				bool CheckParentLinksRight = true;

				Splitter.RightBlock = Right.Item2;

				if (BlockIndex == 0)   // Create new root
				{
					Left = await this.CreateNewBlockLocked();
					LeftLink = Left.Item1;
					Splitter.LeftBlock = Left.Item2;
					CheckParentLinksLeft = true;
				}
				else                        // Reuse current node for new left node.
				{
					LeftLink = BlockIndex;
					Splitter.LeftBlock = new byte[this.blockSize];
				}

				int Len;
				int Pos;
				int c;
				uint BlockLink;
				uint ChildSize;
				object ObjectId;
				bool IsEmpty;
				bool Leaf = true;

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, BlockHeaderSize);

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                  // Block link.
					ObjectId = this.recordHandler.GetKey(Reader);
					IsEmpty = (ObjectId is null);
					if (IsEmpty)
						break;

					if (BlockLink != 0)
					{
						Leaf = false;
						ChildSize = await this.GetObjectSizeOfBlockLocked(BlockLink);
					}
					else
						ChildSize = 0;

					Len = await this.recordHandler.GetPayloadSize(Reader);
					c = Reader.Position - Pos + Len;

					if (Pos == InsertAt)
					{
						Splitter.NextBlock(BlockLink, Bin, 0, Bin.Length, ChildSize);
						Splitter.NextBlock(ChildRightLink, Block, Pos + 4, c - 4, ChildRightLinkSize);
					}
					else
						Splitter.NextBlock(BlockLink, Block, Pos + 4, c - 4, ChildSize);

					Reader.Position += Len;
				}
				while (Reader.BytesLeft >= 4);

				if (!IsEmpty)
					Pos = Reader.Position;

				if (Pos == InsertAt)
				{
					BlockLink = BitConverter.ToUInt32(Block, 6);
					ChildSize = BlockLink == 0 ? 0 : await this.GetObjectSizeOfBlockLocked(BlockLink);
					Splitter.NextBlock(BlockLink, Bin, 0, Bin.Length, ChildSize);

					Splitter.RightLastBlockIndex = ChildRightLink;
					if (ChildRightLink != 0)
						Splitter.RightSizeSubtree += ChildRightLinkSize;
				}
				else
				{
					BlockLink = BitConverter.ToUInt32(Block, 6);
					Splitter.RightLastBlockIndex = BlockLink;
					if (BlockLink != 0)
						Splitter.RightSizeSubtree += await this.GetObjectSizeOfBlockLocked(BlockLink);
				}

				ushort LeftBytesUsed = (ushort)(Splitter.LeftPos - BlockHeaderSize);
				ushort RightBytesUsed = (ushort)(Splitter.RightPos - BlockHeaderSize);

				uint ParentLink = BlockIndex == 0 ? 0 : Header.ParentBlockIndex;

				Array.Copy(BitConverter.GetBytes(LeftBytesUsed), 0, Splitter.LeftBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.LeftSizeSubtree), 0, Splitter.LeftBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.LeftBlock, 10, 4);

				Array.Copy(BitConverter.GetBytes(RightBytesUsed), 0, Splitter.RightBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.RightSizeSubtree), 0, Splitter.RightBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.RightBlock, 10, 4);

				this.QueueSaveBlockLocked(LeftLink, Splitter.LeftBlock);
				this.QueueSaveBlockLocked(RightLink, Splitter.RightBlock);

				if (BlockIndex == 0)
				{
					ushort NewParentBytesUsed = (ushort)(4 + Splitter.ParentObject.Length);
					uint NewParentSizeSubtree = 1 + Splitter.LeftSizeSubtree + Splitter.RightSizeSubtree;
					byte[] NewParentBlock = new byte[this.blockSize];

					if (NewParentSizeSubtree <= Splitter.LeftSizeSubtree || NewParentSizeSubtree <= Splitter.RightSizeSubtree)
						NewParentSizeSubtree = uint.MaxValue;

					Array.Copy(BitConverter.GetBytes(NewParentBytesUsed), 0, NewParentBlock, 0, 2);
					Array.Copy(BitConverter.GetBytes(NewParentSizeSubtree), 0, NewParentBlock, 2, 4);
					Array.Copy(BitConverter.GetBytes(RightLink), 0, NewParentBlock, 6, 4);
					Array.Copy(BitConverter.GetBytes(LeftLink), 0, NewParentBlock, 14, 4);
					Array.Copy(Splitter.ParentObject, 0, NewParentBlock, 18, Splitter.ParentObject.Length);

					this.QueueSaveBlockLocked(0, NewParentBlock);
				}
				else
				{
					byte[] ParentBlock = await this.LoadBlockLocked(ParentLink, true);
					BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock, this.blockLimit);

					BlockHeader ParentHeader = new BlockHeader(ParentReader);
					int ParentLen;
					int ParentPos;
					uint ParentBlockIndex;

					if (ParentHeader.LastBlockIndex == LeftLink)
						ParentPos = BlockHeaderSize + ParentHeader.BytesUsed;
					else
					{
						do
						{
							ParentPos = ParentReader.Position;

							ParentBlockIndex = ParentReader.ReadBlockLink();                  // Block link.
							if (!this.recordHandler.SkipKey(ParentReader))
								break;

							ParentLen = await this.recordHandler.GetPayloadSize(ParentReader);
							ParentReader.Position += ParentLen;
						}
						while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 4);

						if (ParentBlockIndex != LeftLink)
						{
							throw Database.FlagForRepair(this.collectionName, "Parent link points to parent block (" + ParentLink.ToString() +
								") with no reference to child block (" + LeftLink.ToString() + ").");
						}
					}

					await this.InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
						Splitter.RightSizeSubtree, IncSize, LastObject);
				}

				if (!Leaf)
				{
					if (CheckParentLinksLeft)
						await this.UpdateParentLinksLocked(LeftLink, Splitter.LeftBlock);

					if (CheckParentLinksRight)
						await this.UpdateParentLinksLocked(RightLink, Splitter.RightBlock);
				}
			}
		}

		private async Task UpdateParentLinksLocked(uint BlockIndex, byte[] Block)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			object ObjectId;
			int Len;
			uint ChildLink;

			BlockHeader.SkipHeader(Reader);

			do
			{
				ChildLink = Reader.ReadBlockLink();                  // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				Len = await this.recordHandler.GetPayloadSize(Reader);

				if (ChildLink != 0)
					await this.CheckChildParentLinkLocked(ChildLink, BlockIndex);

				Reader.Position += Len;
			}
			while (Reader.BytesLeft >= 4);

			ChildLink = BitConverter.ToUInt32(Block, 6);
			if (ChildLink != 0)
				await this.CheckChildParentLinkLocked(ChildLink, BlockIndex);
		}

		private async Task<uint> GetObjectSizeOfBlockLocked(uint BlockIndex)
		{
			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);

			return BitConverter.ToUInt32(Block, 2);
		}

		private async Task CheckChildParentLinkLocked(uint ChildLink, uint NewParentLink)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] ChildBlock = await this.LoadBlockLocked(ChildLink, true);

			uint ChildParentLink = BitConverter.ToUInt32(ChildBlock, 10);
			if (ChildParentLink != NewParentLink)
			{
				Array.Copy(BitConverter.GetBytes(NewParentLink), 0, ChildBlock, 10, 4);
				this.QueueSaveBlockLocked(ChildLink, ChildBlock);
			}
		}

		internal async Task<BlockInfo> FindLeafNodeLocked(object ObjectId)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			uint BlockIndex = 0;
			bool LastObject = true;

			while (true)
			{
				byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);

				BlockHeader Header = new BlockHeader(Reader);
				object ObjectId2;
				int Len;
				int Pos;
				uint BlockLink;
				int Comparison;
				bool IsEmpty;

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                  // Block link.
					ObjectId2 = this.recordHandler.GetKey(Reader);

					IsEmpty = (ObjectId2 is null);
					if (IsEmpty)
					{
						Comparison = 1;
						break;
					}

					Comparison = this.recordHandler.Compare(ObjectId, ObjectId2);

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
				while (Comparison > 0 && Reader.BytesLeft >= 4);

				if (Comparison == 0)                                       // Object ID already found.
					return null;
				else if (IsEmpty || Comparison > 0)
				{
					if (Header.LastBlockIndex == 0)
						return new BlockInfo(Header, Block, BlockIndex, IsEmpty ? Pos : Reader.Position, LastObject);
					else
						BlockIndex = Header.LastBlockIndex;
				}
				else
				{
					LastObject = false;
					if (BlockLink == 0)
						return new BlockInfo(Header, Block, BlockIndex, Pos, false);
					else
						BlockIndex = BlockLink;
				}
			}
		}

		#endregion

		#region Load Object

		/// <summary>
		/// Loads an object from the file.
		/// </summary>
		/// <param name="ObjectId">ID of object to load.</param>
		public Task<object> LoadObject(Guid ObjectId)
		{
			return this.LoadObject(ObjectId, null);
		}

		/// <summary>
		/// Loads an object from the file.
		/// </summary>
		/// <typeparam name="T">Type of object to load.</typeparam>
		/// <param name="ObjectId">ID of object to load.</param>
		public async Task<T> LoadObject<T>(Guid ObjectId)
		{
			return (T)await this.LoadObject(ObjectId, await this.provider.GetObjectSerializer(typeof(T)));
		}

		/// <summary>
		/// Loads an object from the file.
		/// </summary>
		/// <param name="ObjectId">ID of object to load.</param>
		/// <param name="Type">Type of object to load.</param>
		public async Task<T> LoadObject<T>(Guid ObjectId, Type Type)
		{
			return (T)await this.LoadObject(ObjectId, await this.provider.GetObjectSerializer(Type));
		}

		/// <summary>
		/// Loads an object from the file.
		/// </summary>
		/// <param name="ObjectId">ID of object to load.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer will be deduced from information stored in the file.</param>
		public async Task<object> LoadObject(Guid ObjectId, IObjectSerializer Serializer)
		{
			await this.BeginRead();
			try
			{
				return await this.LoadObjectLocked(ObjectId, Serializer);
			}
			finally
			{
				await this.EndRead();
			}
		}

		internal async Task<object> LoadObjectLocked(object ObjectId, IObjectSerializer Serializer)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info is null)
				throw new KeyNotFoundException("Object not found.");

			return await this.ParseObjectLocked(Info, Serializer);
		}

		internal async Task<object> TryLoadObject(Guid ObjectId, IObjectSerializer Serializer)
		{
			await this.BeginRead();
			try
			{
				return await this.TryLoadObjectLocked(ObjectId, Serializer);
			}
			finally
			{
				await this.EndRead();
			}
		}

		internal async Task<object> TryLoadObjectLocked(object ObjectId, IObjectSerializer Serializer)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info is null)
				return null;
			else
				return await this.ParseObjectLocked(Info, Serializer);
		}

		internal void QueueForLoad(Guid ObjectId, ObjectSerializer Serializer, EmbeddedObjectSetter Setter)
		{
			lock (this.synchObject)
			{
				if (this.objectsToLoad is null)
					this.objectsToLoad = new LinkedList<LoadRec>();

				this.objectsToLoad.AddLast(new LoadRec()
				{
					ObjectId = ObjectId,
					Serializer = Serializer,
					Setter = Setter
				});
			}
		}

		private async Task<object> ParseObjectLocked(BlockInfo Info, IObjectSerializer Serializer)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			int Pos = Info.InternalPosition + 4;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, this.blockLimit, Pos);

			this.recordHandler.SkipKey(Reader);
			bool IsBlob = await this.recordHandler.IsBlob(Reader);

			if (IsBlob)
			{
				Reader = await this.LoadBlobLocked(Info.Block, Pos, null, null);
				Pos = 0;
			}

			if (Serializer is null)
			{
				string TypeName = this.recordHandler.GetPayloadType(Reader);
				if (string.IsNullOrEmpty(TypeName))
					Serializer = this.genericSerializer;
				else
				{
					Type T = Types.GetType(TypeName);
					if (!(T is null))
						Serializer = await this.provider.GetObjectSerializer(T);
					else
						Serializer = this.genericSerializer;
				}
			}

			Reader.Position = Pos;

			return await Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
		}

		internal async Task<BlockInfo> FindNodeLocked(object ObjectId)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			uint BlockIndex = 0;

			if (ObjectId is null || (ObjectId is Guid && ObjectId.Equals(Guid.Empty)))
				return null;

			while (true)
			{
				byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);

				BlockHeader Header = new BlockHeader(Reader);
				object ObjectId2;
				int Len;
				int Pos;
				uint BlockLink;
				int Comparison;
				bool IsEmpty;

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                  // Block link.
					ObjectId2 = this.recordHandler.GetKey(Reader);

					IsEmpty = (ObjectId2 is null);
					if (IsEmpty)
					{
						Comparison = 1;
						break;
					}

					Comparison = this.recordHandler.Compare(ObjectId, ObjectId2);

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
				while (Comparison > 0 && Reader.BytesLeft >= 4);

				if (Comparison == 0)                                       // Object ID found.
					return new BlockInfo(Header, Block, BlockIndex, Pos, false);
				else if (IsEmpty || Comparison > 0)
				{
					if (Header.LastBlockIndex == 0)
						return null;
					else
						BlockIndex = Header.LastBlockIndex;
				}
				else
				{
					if (BlockLink == 0)
						return null;
					else
						BlockIndex = BlockLink;
				}
			}
		}

		#endregion

		#region Update Object

		/// <summary>
		/// Updates an object in the database, using the object serializer corresponding to the type of object being updated.
		/// </summary>
		/// <param name="Object">Object to update.</param>
		/// <param name="Lazy">If update can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task UpdateObject(object Object, bool Lazy, ObjectCallback Callback)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(ObjectType);
			await this.UpdateObject(Object, Serializer, Lazy, Callback);
		}

		/// <summary>
		/// Updates an object in the database.
		/// </summary>
		/// <param name="Object">Object to update.</param>
		/// <param name="Serializer">Object serializer to use.</param>
		/// <param name="Lazy">If update can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task UpdateObject(object Object, ObjectSerializer Serializer, bool Lazy, ObjectCallback Callback)
		{
			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						await this.UpdateObjectLocked(Object, Serializer);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Object, Serializer, Callback, WriteOp.Update);
					return;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					await this.UpdateObjectLocked(Object, Serializer);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Object);
		}

		private async Task UpdateObjectLocked(object Object, ObjectSerializer Serializer)
		{
			Guid ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info is null)
				throw new KeyNotFoundException("Object not found.");

			object Old = await this.ParseObjectLocked(Info, Serializer);

			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
			await Serializer.Serialize(Writer, false, false, Object, NestedLocks.CreateIfNested(this, true, Serializer));
			byte[] Bin = Writer.GetSerialization();

			await this.ReplaceObjectLocked(Bin, Info, true);

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile Index in this.indices)
					await Index.UpdateObjectLocked(ObjectId, Old, Object, Serializer);
			}
		}

		/// <summary>
		/// Updates a set of objects in the database.
		/// </summary>
		/// <param name="Objects">Objects to update.</param>
		/// <param name="Serializer">Object serializer to use.</param>
		/// <param name="Lazy">If update can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task UpdateObjects(IEnumerable<object> Objects, ObjectSerializer Serializer, bool Lazy, ObjectsCallback Callback)
		{
			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						foreach (object Object in Objects)
							await this.UpdateObjectLocked(Object, Serializer);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Objects, Serializer, Callback, WriteOp.Update);
					return;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					foreach (object Object in Objects)
						await this.UpdateObjectLocked(Object, Serializer);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Objects);
		}

		internal async Task ReplaceObjectLocked(byte[] Bin, BlockInfo Info, bool DeleteBlob)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = Info.Block;
			BlockHeader Header = Info.Header;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, Info.InternalPosition + 4);
			this.recordHandler.SkipKey(Reader);

			uint BlockIndex = Info.BlockIndex;
			KeyValuePair<int, bool> P = await this.recordHandler.GetPayloadSizeEx(Reader);
			int Len = P.Key;
			bool IsBlob = P.Value;

			if (IsBlob && DeleteBlob)
				await this.DeleteBlobLocked(Block, Info.InternalPosition + 4);

			if (DeleteBlob && Bin.Length > this.inlineObjectSizeLimit)
			{
				byte[] BlobReference = await this.SaveBlobLocked(Bin);
				BlobReference = this.recordHandler.EncodeBlobReference(BlobReference, Bin);

				if (BlobReference.Length > this.inlineObjectSizeLimit)
				{
					await this.DeleteBlobLocked(BlobReference, 0);
					throw new ArgumentException("Key too long.");
				}

				Bin = BlobReference;
			}

			int NewSize = Bin.Length;
			int OldSize = Reader.Position + Len - (Info.InternalPosition + 4);
			int DeltaSize = NewSize - OldSize;
			int i;

			if (Header.BytesUsed + BlockHeaderSize + DeltaSize <= this.blockSize)
			{
				if (DeltaSize != 0)
				{
					i = Header.BytesUsed + BlockHeaderSize - (Info.InternalPosition + 4 + OldSize);
					if (i > 0)
						Array.Copy(Block, Info.InternalPosition + 4 + OldSize, Block, Info.InternalPosition + 4 + NewSize, i);

					Header.BytesUsed += (ushort)DeltaSize;
					Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

					if (DeltaSize < 0)
						Array.Clear(Block, Header.BytesUsed + BlockHeaderSize, -DeltaSize);
				}

				Array.Copy(Bin, 0, Block, Info.InternalPosition + 4, NewSize);

				this.QueueSaveBlockLocked(BlockIndex, Block);
			}
			else
			{
				BlockSplitter Splitter = new BlockSplitterMiddle(this.blockSize);
				Tuple<uint, byte[]> Left;
				Tuple<uint, byte[]> Right = await this.CreateNewBlockLocked();
				uint LeftLink;
				uint RightLink = Right.Item1;
				bool CheckParentLinksLeft = false;
				bool CheckParentLinksRight = true;

				Splitter.RightBlock = Right.Item2;

				if (BlockIndex == 0)   // Create new root
				{
					Left = await this.CreateNewBlockLocked();
					LeftLink = Left.Item1;
					Splitter.LeftBlock = Left.Item2;
					CheckParentLinksLeft = true;
				}
				else                        // Reuse current node for new left node.
				{
					LeftLink = BlockIndex;
					Splitter.LeftBlock = new byte[this.blockSize];
				}

				int Pos;
				int c;
				uint BlockLink;
				uint ChildSize;
				object ObjectId;
				bool IsEmpty;
				bool Leaf = true;

				Reader.Restart(Block, BlockHeaderSize);

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                    // Block link.
					ObjectId = this.recordHandler.GetKey(Reader);
					IsEmpty = ObjectId is null;
					if (IsEmpty)
						break;

					if (BlockLink != 0)
					{
						Leaf = false;
						ChildSize = await this.GetObjectSizeOfBlockLocked(BlockLink);
					}
					else
						ChildSize = 0;

					Len = await this.recordHandler.GetPayloadSize(Reader);
					c = Reader.Position - Pos + Len;

					if (Pos == Info.InternalPosition)
						Splitter.NextBlock(BlockLink, Bin, 0, Bin.Length, ChildSize);
					else
						Splitter.NextBlock(BlockLink, Block, Pos + 4, c - 4, ChildSize);

					Reader.Position += Len;
				}
				while (Reader.BytesLeft >= 4);

				BlockLink = BitConverter.ToUInt32(Block, 6);
				Splitter.RightLastBlockIndex = BlockLink;
				if (BlockLink != 0)
					Splitter.RightSizeSubtree += await this.GetObjectSizeOfBlockLocked(BlockLink);

				ushort LeftBytesUsed = (ushort)(Splitter.LeftPos - BlockHeaderSize);
				ushort RightBytesUsed = (ushort)(Splitter.RightPos - BlockHeaderSize);

				uint ParentLink = BlockIndex == 0 ? 0 : Header.ParentBlockIndex;

				Array.Copy(BitConverter.GetBytes(LeftBytesUsed), 0, Splitter.LeftBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.LeftSizeSubtree), 0, Splitter.LeftBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.LeftBlock, 10, 4);

				Array.Copy(BitConverter.GetBytes(RightBytesUsed), 0, Splitter.RightBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.RightSizeSubtree), 0, Splitter.RightBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.RightBlock, 10, 4);

				this.QueueSaveBlockLocked(LeftLink, Splitter.LeftBlock);
				this.QueueSaveBlockLocked(RightLink, Splitter.RightBlock);

				if (BlockIndex == 0)
				{
					ushort NewParentBytesUsed = (ushort)(4 + Splitter.ParentObject.Length);
					uint NewParentSizeSubtree = 1 + Splitter.LeftSizeSubtree + Splitter.RightSizeSubtree;
					byte[] NewParentBlock = new byte[this.blockSize];

					if (NewParentSizeSubtree <= Splitter.LeftSizeSubtree || NewParentSizeSubtree <= Splitter.RightSizeSubtree)
						NewParentSizeSubtree = uint.MaxValue;

					Array.Copy(BitConverter.GetBytes(NewParentBytesUsed), 0, NewParentBlock, 0, 2);
					Array.Copy(BitConverter.GetBytes(NewParentSizeSubtree), 0, NewParentBlock, 2, 4);
					Array.Copy(BitConverter.GetBytes(RightLink), 0, NewParentBlock, 6, 4);
					Array.Copy(BitConverter.GetBytes(LeftLink), 0, NewParentBlock, 14, 4);
					Array.Copy(Splitter.ParentObject, 0, NewParentBlock, 18, Splitter.ParentObject.Length);

					this.QueueSaveBlockLocked(0, NewParentBlock);
				}
				else
				{
					byte[] ParentBlock = await this.LoadBlockLocked(ParentLink, true);
					BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock, this.blockLimit);

					BlockHeader ParentHeader = new BlockHeader(ParentReader);
					int ParentLen;
					int ParentPos;
					uint ParentBlockIndex;

					if (ParentHeader.LastBlockIndex == LeftLink)
						ParentPos = BlockHeaderSize + ParentHeader.BytesUsed;
					else
					{
						do
						{
							ParentPos = ParentReader.Position;

							ParentBlockIndex = ParentReader.ReadBlockLink();                  // Block link.
							if (!this.recordHandler.SkipKey(ParentReader))
								break;

							ParentLen = await this.recordHandler.GetPayloadSize(ParentReader);
							ParentReader.Position += ParentLen;
						}
						while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 4);

						if (ParentBlockIndex != LeftLink)
						{
							throw Database.FlagForRepair(this.collectionName, "Parent link points to parent block (" + ParentLink.ToString() +
								") with no reference to child block (" + LeftLink.ToString() + ").");
						}
					}

					await this.InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
						Splitter.RightSizeSubtree, false, Info.LastObject);
				}

				if (!Leaf)
				{
					if (CheckParentLinksLeft)
						await this.UpdateParentLinksLocked(LeftLink, Splitter.LeftBlock);

					if (CheckParentLinksRight)
						await this.UpdateParentLinksLocked(RightLink, Splitter.RightBlock);
				}
			}
		}

		#endregion

		#region Delete Object

		/// <summary>
		/// Deletes an object from the database, using the object serializer corresponding to the type of object being updated, to find
		/// the Object ID of the object.
		/// </summary>
		/// <param name="Object">Object to delete.</param>
		/// <param name="Lazy">If deletion can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task DeleteObject(object Object, bool Lazy, ObjectCallback Callback)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(ObjectType);

			await this.DeleteObject(Object, Serializer, Lazy, Callback);
		}

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="Object">Object to delete.</param>
		/// <param name="Serializer">Binary serializer.</param>
		/// <param name="Lazy">If deletion can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task DeleteObject(object Object, ObjectSerializer Serializer, bool Lazy, ObjectCallback Callback)
		{
			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						await this.DeleteObjectLocked(Object, Serializer);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Object, Serializer, Callback, WriteOp.Delete);
					return;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					await this.DeleteObjectLocked(Object, Serializer);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Object);
		}

		private async Task DeleteObjectLocked(object Object, ObjectSerializer Serializer)
		{
			Guid ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
			await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null, 0);

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile Index in this.indices)
					await Index.DeleteObjectLocked(ObjectId, Object, Serializer);
			}
		}

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="ObjectId">Object ID of the object to delete.</param>
		/// <returns>Object that was deleted.</returns>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task<object> DeleteObject(Guid ObjectId)
		{
			await this.BeginWrite();
			try
			{
				object DeletedObject = await this.DeleteObjectLocked(ObjectId, false, true, this.genericSerializer, null, 0);

				if (!(this.indices is null))
				{
					foreach (IndexBTreeFile Index in this.indices)
						await Index.DeleteObjectLocked(ObjectId, DeletedObject, this.genericSerializer);
				}

				return DeletedObject;
			}
			finally
			{
				await this.EndWrite();
			}
		}

		/// <summary>
		/// Deletes a set of objects from the database.
		/// </summary>
		/// <param name="Objects">Objects to delete.</param>
		/// <param name="Serializer">Binary serializer.</param>
		/// <param name="Lazy">If deletion can be done at next convenient time.</param>
		/// <param name="Callback">Method to call when operation completed.</param>
		/// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
		public async Task DeleteObjects(IEnumerable<object> Objects, ObjectSerializer Serializer, bool Lazy, ObjectsCallback Callback)
		{
			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						foreach (object Object in Objects)
							await this.DeleteObjectLocked(Object, Serializer);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(Objects, Serializer, Callback, WriteOp.Delete);
					return;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					foreach (object Object in Objects)
						await this.DeleteObjectLocked(Object, Serializer);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Objects);
		}

		internal async Task<object> DeleteObjectLocked(object ObjectId, bool MergeNodes, bool DeleteAnyBlob,
			IObjectSerializer Serializer, object OldObject, int MergeCount)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info is null)
				throw new KeyNotFoundException("Object not found.");

			byte[] Block = Info.Block;
			BlockHeader Header = Info.Header;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, Info.InternalPosition);
			uint BlockIndex = Info.BlockIndex;
			uint LeftBlockLink = Reader.ReadBlockLink();
			int Len;
			int i, c;

			this.recordHandler.SkipKey(Reader);
			KeyValuePair<int, bool> P = await this.recordHandler.GetPayloadSizeEx(Reader);
			Len = P.Key;
			bool IsBlob = P.Value;

			if (DeleteAnyBlob)
			{
				if (Len == 0)
					OldObject = null;
				else
					OldObject = await this.ParseObjectLocked(Info, Serializer);

				if (IsBlob)
					await this.DeleteBlobLocked(Block, Info.InternalPosition + 4);
			}

			i = Reader.Position + Len;
			c = Header.BytesUsed + BlockHeaderSize - i;

			if (LeftBlockLink == 0)
			{
				if (c > 0)
					Array.Copy(Block, i, Block, Info.InternalPosition, c);

				Header.BytesUsed -= (ushort)(i - Info.InternalPosition);
				Header.SizeSubtree--;

				Array.Clear(Block, Header.BytesUsed + BlockHeaderSize, i - Info.InternalPosition);
				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
				Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

				if (Header.BytesUsed == 0)
				{
					if (BlockIndex == 0)
					{
						if (Header.LastBlockIndex != 0)
						{
							Block = await this.LoadBlockLocked(Header.LastBlockIndex, true);
							this.QueueSaveBlockLocked(0, Block);

							this.RegisterEmptyBlockLocked(Header.LastBlockIndex);

							await this.UpdateParentLinksLocked(0, Block);
						}
						else
							this.QueueSaveBlockLocked(0, Block);
					}
					else
					{
						Header.SizeSubtree = 0;
						Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
						Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

						this.QueueSaveBlockLocked(BlockIndex, Block);
						await this.DecreaseSizeLocked(Header.ParentBlockIndex);

						if (BlockIndex != 0)
							await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);
						else
							await this.RebalanceEmptyBlockLocked(BlockIndex, Block, Header);
					}
				}
				else
				{
					this.QueueSaveBlockLocked(BlockIndex, Block);

					if (BlockIndex != 0)
						await this.DecreaseSizeLocked(Header.ParentBlockIndex);
				}
			}
			else
			{
				Reader.Position += Len;

				int ObjLen = Reader.Position - (Info.InternalPosition + 4);
				uint RightBlockLink;
				bool Last;

				if (Reader.Position >= Header.BytesUsed + BlockHeaderSize)
				{
					RightBlockLink = Header.LastBlockIndex;
					Last = true;
				}
				else
				{
					RightBlockLink = Reader.ReadBlockLink();
					Last = false;
				}

				if (MergeNodes)
				{
					if (MergeCount > 100)
						throw Database.FlagForRepair(this.collectionName, "Suspected circular reference found in database.");

					byte[] Separator = new byte[ObjLen];
					Array.Copy(Block, Info.InternalPosition + 4, Separator, 0, ObjLen);

					MergeResult MergeResult = await this.MergeBlocksLocked(LeftBlockLink, Separator, RightBlockLink);

					this.RegisterEmptyBlockLocked(RightBlockLink);

					if (Last)
					{
						if ((i = Info.InternalPosition) == BlockHeaderSize && BlockIndex == 0)
						{
							this.QueueSaveBlockLocked(0, MergeResult.ResultBlock);
							this.RegisterEmptyBlockLocked(LeftBlockLink);

							await this.UpdateParentLinksLocked(0, MergeResult.ResultBlock);

							return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject, MergeCount + 1);  // This time, the object will be lower in the tree.
						}
						else
						{
							c = Header.BytesUsed + BlockHeaderSize - i;

							Header.LastBlockIndex = LeftBlockLink;
							Array.Copy(BitConverter.GetBytes(Header.LastBlockIndex), 0, Block, 6, 4);
						}
					}
					else
					{
						c = Header.BytesUsed + BlockHeaderSize - Reader.Position;
						Array.Copy(Block, Reader.Position, Block, Info.InternalPosition + 4, c);
						i = Info.InternalPosition + 4 + c;
						c = Header.BytesUsed + BlockHeaderSize - i;
					}

					Array.Clear(Block, i, c);

					Header.BytesUsed -= (ushort)c;
					Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

					this.QueueSaveBlockLocked(BlockIndex, Block);
					this.QueueSaveBlockLocked(LeftBlockLink, MergeResult.ResultBlock);

					await this.UpdateParentLinksLocked(LeftBlockLink, MergeResult.ResultBlock);

					if (Header.BytesUsed == 0 && BlockIndex != 0)
						await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);

					if (!(MergeResult.Separator is null))
						await this.ReinsertMergeOverflow(MergeResult, BlockIndex);

					return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject, MergeCount + 1);  // This time, the object will be lower in the tree.
				}
				else
				{
					bool Reshuffled = false;
					byte[] NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, false, false);

					if (NewSeparator is null)
					{
						NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, false, false);

						if (NewSeparator is null)
						{
							Reshuffled = true;
							NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, true, false);

							if (NewSeparator is null)
							{
								NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, true, false);

								if (NewSeparator is null)
								{
									NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, false, true);

									if (NewSeparator is null)
									{
										NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, false, true);

										if (NewSeparator is null)
											return await this.DeleteObjectLocked(ObjectId, true, false, Serializer, OldObject, MergeCount);
									}
								}
							}
						}
					}

					Info.Block = await this.LoadBlockLocked(BlockIndex, true);    // Refresh object count.

					if (Reshuffled)
					{
						Reader.Restart(Info.Block, 0);
						Info.Header = new BlockHeader(Reader);

						if (await this.ForEachObject(Info.Block, (Link, ObjectId2, Pos, Len2) =>
						{
							if (this.recordHandler.Compare(ObjectId, ObjectId2) == 0)
							{
								Info.InternalPosition = Pos - 4;
								return false;
							}
							else
								return true;
						}))
						{
							object ObjectId2 = this.GetObjectId(NewSeparator);
							Info = await this.FindLeafNodeLocked(ObjectId2);
							await this.InsertObjectLocked(Info.BlockIndex, Info.Header, Info.Block, NewSeparator, Info.InternalPosition, 0, 0, true, Info.LastObject);
							return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject, MergeCount);
						}
					}

					await this.ReplaceObjectLocked(NewSeparator, Info, false);
				}
			}

			return OldObject;
		}

		private async Task ReinsertMergeOverflow(MergeResult MergeResult, uint BlockIndex)
		{
			if (MergeResult.Separator is null)
				return;

			// Update block object counts:

			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			ulong Size = 0;
			ulong PrevSize;

			await this.ForEachObjectAsync(Block, async (Link, ObjectId2, Pos, Len) =>
			{
				Size++;

				if (Link != 0)
					Size += await this.GetObjectCountLocked(Link, true);

				return true;
			});

			if (Header.LastBlockIndex != 0)
				Size += await this.GetObjectCountLocked(Header.LastBlockIndex, true);

			if (Header.SizeSubtree == uint.MaxValue)
				PrevSize = await this.GetObjectCountLocked(BlockIndex, true);
			else
				PrevSize = Header.SizeSubtree;

			if (Size > uint.MaxValue)
				Header.SizeSubtree = uint.MaxValue;
			else
				Header.SizeSubtree = (uint)Size;

			Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);
			this.QueueSaveBlockLocked(BlockIndex, Block);

			if (BlockIndex != 0)
				await this.DecreaseSizeLocked(Header.ParentBlockIndex, (uint)(PrevSize - Size));

			// Reinsert residual objects:

			object ObjectId = this.GetObjectId(MergeResult.Separator);
			BlockInfo Leaf = await this.FindLeafNodeLocked(ObjectId);
			await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, MergeResult.Separator, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);

			if (!(MergeResult.Residue is null))
			{
				LinkedList<uint> Links = null;
				Block = MergeResult.Residue;

				while (!(Block is null))
				{
					Block = (byte[])Block.Clone();

					await this.ForEachObjectAsync(Block, async (Link, ObjectId2, Pos, Len) =>
					{
						byte[] Obj = new byte[Len];
						Array.Copy(Block, Pos, Obj, 0, Len);
						BlockInfo Leaf2 = await this.FindLeafNodeLocked(ObjectId2);
						await this.InsertObjectLocked(Leaf2.BlockIndex, Leaf2.Header, Leaf2.Block, Obj, Leaf2.InternalPosition, 0, 0, true, Leaf2.LastObject);

						if (Link != 0)
						{
							if (Links is null)
								Links = new LinkedList<uint>();

							Links.AddLast(Link);
						}

						return true;
					});

					BlockIndex = BitConverter.ToUInt32(Block, 6);
					if (BlockIndex != 0)
					{
						if (Links is null)
							Links = new LinkedList<uint>();

						Links.AddLast(BlockIndex);
					}

					if (Links is null || Links.First is null)
						Block = null;
					else
					{
						BlockIndex = Links.First.Value;
						Block = await this.LoadBlockLocked(BlockIndex, true);
						Links.RemoveFirst();
						this.RegisterEmptyBlockLocked(BlockIndex);
					}
				}
			}
		}

		private async Task RebalanceEmptyBlockLocked(uint BlockIndex, byte[] Block, BlockHeader Header)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Object;

			if (BlockIndex == 0)
			{
				this.emptyRoot = true;
				return;
			}

			if (Header.LastBlockIndex != 0)
			{
				Object = await this.RotateLeftLocked(Header.LastBlockIndex, BlockIndex);

				if (Object is null)
					Object = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);
			}
			else
			{
				Object = await this.RotateLeftLocked(BlockIndex, Header.ParentBlockIndex);
				if (Object is null)
					Object = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);
			}

			if (Object is null)
			{
				if (BlockIndex != 0)
					await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);
			}
			else
			{
				Array.Clear(Block, BlockHeaderSize, 4);
				Array.Copy(Object, 0, Block, BlockHeaderSize + 4, Object.Length);

				Header.BytesUsed = (ushort)(4 + Object.Length);
				Header.SizeSubtree++;
				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
				Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

				if (Header.LastBlockIndex == 0)
					this.QueueSaveBlockLocked(BlockIndex, Block);
				else
				{
					Tuple<uint, byte[]> NewChild = await this.CreateNewBlockLocked();
					byte[] NewChildBlock = NewChild.Item2;
					uint NewChildBlockIndex = NewChild.Item1;

					Array.Copy(BitConverter.GetBytes(NewChildBlockIndex), 0, Block, BlockHeaderSize, 4);

					this.QueueSaveBlockLocked(BlockIndex, Block);

					byte[] Object2 = await this.RotateLeftLocked(NewChildBlockIndex, BlockIndex);
					if (Object2 is null && BlockIndex != 0)
						Object2 = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);

					if (!(Object2 is null))
					{
						Array.Copy(Object2, 0, NewChildBlock, BlockHeaderSize + 4, Object2.Length);
						Array.Copy(BitConverter.GetBytes((ushort)(Object2.Length + 4)), 0, NewChildBlock, 0, 2);
						Array.Copy(BitConverter.GetBytes((uint)1), 0, NewChildBlock, 2, 4);
					}

					Array.Copy(BitConverter.GetBytes(BlockIndex), 0, NewChildBlock, 10, 4);

					this.QueueSaveBlockLocked(NewChildBlockIndex, NewChildBlock);

					if (!(Object2 is null))
						await this.IncreaseSizeLocked(BlockIndex);
				}

				await this.IncreaseSizeLocked(BitConverter.ToUInt32(Block, 10));    // Note that Header.ParentBlockIndex might no longer be correct.
			}
		}

		private async Task MergeEmptyBlockWithSiblingLocked(uint ChildBlockIndex, uint ParentBlockIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] ParentBlock = await this.LoadBlockLocked(ParentBlockIndex, true);
			BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock, this.blockLimit);
			BlockHeader ParentHeader = new BlockHeader(ParentReader);
			int LastPos = 0;
			int LastLen = 0;
			uint LastLink = 0;
			int i, c;

			if (await this.ForEachObject(ParentBlock, (Link, ObjectId2, Pos, Len2) =>
			{
				LastPos = Pos;
				LastLink = Link;
				LastLen = Len2;
				return (Link != ChildBlockIndex);
			}))
			{
				if (LastPos != 0)
				{
					// BlockIndex is Parent.Last, and LastPos contains the position of last object separator in parent block.

					c = ParentHeader.BytesUsed + BlockHeaderSize - LastPos;
					byte[] Separator = new byte[c];
					Array.Copy(ParentBlock, LastPos, Separator, 0, c);
					MergeResult MergeResult = await this.MergeBlocksLocked(LastLink, Separator, ChildBlockIndex);

					Array.Clear(ParentBlock, LastPos - 4, ParentHeader.BytesUsed + BlockHeaderSize - (LastPos - 4));
					ParentHeader.BytesUsed = (ushort)(LastPos - 4 - BlockHeaderSize);
					ParentHeader.LastBlockIndex = LastLink;
					Array.Copy(BitConverter.GetBytes(ParentHeader.BytesUsed), 0, ParentBlock, 0, 2);
					Array.Copy(BitConverter.GetBytes(ParentHeader.LastBlockIndex), 0, ParentBlock, 6, 2);

					this.QueueSaveBlockLocked(LastLink, MergeResult.ResultBlock);
					this.QueueSaveBlockLocked(ParentBlockIndex, ParentBlock);

					await this.UpdateParentLinksLocked(LastLink, MergeResult.ResultBlock);

					this.RegisterEmptyBlockLocked(ChildBlockIndex);

					if (ParentHeader.BytesUsed == 0)
					{
						if (ParentBlockIndex != 0)
							await this.MergeEmptyBlockWithSiblingLocked(ParentBlockIndex, ParentHeader.ParentBlockIndex);
						else
							await this.RebalanceEmptyBlockLocked(ParentBlockIndex, ParentBlock, ParentHeader);
					}

					if (!(MergeResult.Separator is null))
						await this.ReinsertMergeOverflow(MergeResult, ParentBlockIndex);
				}
				else
				{
					// Empty node.

					if (ParentBlockIndex != 0)
						await this.MergeEmptyBlockWithSiblingLocked(ParentBlockIndex, ParentHeader.ParentBlockIndex);
					else
						await this.RebalanceEmptyBlockLocked(ParentBlockIndex, ParentBlock, ParentHeader);
				}
			}
			else
			{
				// BlockIndex is a left child, and LastPos points to the object separator.

				uint RightLink;
				bool Last;

				i = LastPos + LastLen;
				if (i >= ParentHeader.BytesUsed + BlockHeaderSize)
				{
					Last = true;
					RightLink = ParentHeader.LastBlockIndex;
				}
				else
				{
					Last = false;
					ParentReader.Position = i;
					RightLink = ParentReader.ReadBlockLink();
				}

				byte[] Separator = new byte[c = i - LastPos];
				Array.Copy(ParentBlock, LastPos, Separator, 0, c);

				MergeResult MergeResult = await this.MergeBlocksLocked(ChildBlockIndex, Separator, RightLink);

				if (Last)
				{
					this.RegisterEmptyBlockLocked(ParentHeader.LastBlockIndex);

					Array.Clear(ParentBlock, LastPos - 4, ParentHeader.BytesUsed + BlockHeaderSize - (LastPos - 4));
					ParentHeader.LastBlockIndex = ChildBlockIndex;
					ParentHeader.BytesUsed = (ushort)(LastPos - BlockHeaderSize - 4);
					Array.Copy(BitConverter.GetBytes(ParentHeader.BytesUsed), 0, ParentBlock, 0, 2);
					Array.Copy(BitConverter.GetBytes(ParentHeader.LastBlockIndex), 0, ParentBlock, 6, 4);
				}
				else
				{
					this.RegisterEmptyBlockLocked(RightLink);

					c = ParentHeader.BytesUsed + BlockHeaderSize - i - 4;
					Array.Copy(ParentBlock, i + 4, ParentBlock, LastPos, c);
					c += LastPos;

					Array.Clear(ParentBlock, c, ParentHeader.BytesUsed + BlockHeaderSize - c);
					ParentHeader.BytesUsed = (ushort)(c - BlockHeaderSize);
					Array.Copy(BitConverter.GetBytes(ParentHeader.BytesUsed), 0, ParentBlock, 0, 2);
				}

				this.QueueSaveBlockLocked(ParentBlockIndex, ParentBlock);
				this.QueueSaveBlockLocked(ChildBlockIndex, MergeResult.ResultBlock);

				await this.UpdateParentLinksLocked(ChildBlockIndex, MergeResult.ResultBlock);

				if (ParentHeader.BytesUsed == 0)
				{
					if (ParentBlockIndex != 0)
						await this.MergeEmptyBlockWithSiblingLocked(ParentBlockIndex, ParentHeader.ParentBlockIndex);
					else
						await this.RebalanceEmptyBlockLocked(ParentBlockIndex, ParentBlock, ParentHeader);
				}

				if (!(MergeResult.Separator is null))
					await this.ReinsertMergeOverflow(MergeResult, ParentBlockIndex);
			}
		}

		private async Task<MergeResult> MergeBlocksLocked(uint LeftIndex, byte[] Separator, uint RightIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] LeftBlock = await this.LoadBlockLocked(LeftIndex, true);
			byte[] RightBlock = await this.LoadBlockLocked(RightIndex, true);

			return await this.MergeBlocksLocked(LeftBlock, Separator, RightBlock);
		}

		private async Task<MergeResult> MergeBlocksLocked(byte[] Left, byte[] Separator, byte[] Right)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			BlockSplitterLast Splitter = new BlockSplitterLast(this.blockSize);
			uint LeftLastLink = BitConverter.ToUInt32(Left, 6);
			uint RightLastLink = BitConverter.ToUInt32(Right, 6);
			uint Size, Size2;

			Splitter.LeftBlock = new byte[this.blockSize];
			Splitter.RightBlock = new byte[this.blockSize];

			await this.ForEachObjectAsync(Left, async (Link, ObjectId, Pos, Len) =>
			{
				if (Link == 0)
					Size = 0;
				else
					Size = await this.GetObjectCount32Locked(Link);

				Splitter.NextBlock(Link, Left, Pos, Len, Size);
				return true;
			});

			if (LeftLastLink == 0)
				Size = 0;
			else
				Size = await this.GetObjectCount32Locked(LeftLastLink);

			Splitter.NextBlock(LeftLastLink, Separator, 0, Separator.Length, Size);

			await this.ForEachObjectAsync(Right, async (Link, ObjectId, Pos, Len) =>
			{
				if (Link == 0)
					Size = 0;
				else
					Size = await this.GetObjectCount32Locked(Link);

				Splitter.NextBlock(Link, Right, Pos, Len, Size);
				return true;
			});

			Array.Copy(BitConverter.GetBytes((ushort)(Splitter.LeftPos - BlockHeaderSize)), 0, Splitter.LeftBlock, 0, 2);
			Array.Copy(BitConverter.GetBytes(Splitter.LeftSizeSubtree), 0, Splitter.LeftBlock, 2, 4);
			Array.Copy(BitConverter.GetBytes(Splitter.LeftLastBlockIndex), 0, Splitter.LeftBlock, 6, 4);
			Array.Copy(Left, 10, Splitter.LeftBlock, 10, 4);

			if (RightLastLink == 0)
				Size = 0;
			else
				Size = await this.GetObjectCount32Locked(RightLastLink);

			MergeResult Result = new MergeResult()
			{
				ResultBlock = Splitter.LeftBlock,
				ResultBlockSize = Splitter.LeftSizeSubtree,
				Separator = Splitter.ParentObject
			};

			if (Splitter.ParentObject is null)
			{
				if (Size != 0)
				{
					Size2 = Result.ResultBlockSize + Size;
					if (Size2 < Size)
						Size2 = uint.MaxValue;

					Result.ResultBlockSize = Size2;
				}

				Result.Residue = null;
				Array.Copy(Right, 6, Result.ResultBlock, 6, 4);
				Array.Copy(BitConverter.GetBytes(Result.ResultBlockSize), 0, Result.ResultBlock, 2, 4);
			}
			else
			{
				if (Size != 0)
				{
					Size2 = Splitter.RightSizeSubtree + Size;
					if (Size2 < Size)
						Size2 = uint.MaxValue;

					Splitter.RightSizeSubtree = Size2;
				}

				Array.Copy(BitConverter.GetBytes((ushort)(Splitter.RightPos - BlockHeaderSize)), 0, Splitter.RightBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.RightSizeSubtree), 0, Splitter.RightBlock, 2, 4);
				Array.Copy(Right, 6, Splitter.RightBlock, 6, 8);

				Result.Residue = Splitter.RightBlock;
				Result.ResidueSize = Splitter.RightSizeSubtree;
			}

			return Result;
		}

		private delegate Task<bool> ForEachAsyncDelegate(uint Link, object ObjectId, int Pos, int Len);

		private async Task<bool> ForEachObjectAsync(byte[] Block, ForEachAsyncDelegate Method)
		{
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			object ObjectId;
			uint Link;
			int Pos, Len, c;

			BlockHeader.SkipHeader(Reader);

			do
			{
				Pos = Reader.Position;

				Link = Reader.ReadBlockLink();
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				Len = await this.recordHandler.GetPayloadSize(Reader);
				Reader.Position += Len;

				c = Reader.Position - Pos - 4;

				if (!await Method(Link, ObjectId, Pos + 4, c))
					return false;
			}
			while (Reader.BytesLeft >= 4);

			return true;
		}

		private delegate bool ForEachDelegate(uint Link, object ObjectId, int Pos, int Len);

		private async Task<bool> ForEachObject(byte[] Block, ForEachDelegate Method)
		{
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			object ObjectId;
			uint Link;
			int Pos, Len, c;

			BlockHeader.SkipHeader(Reader);

			do
			{
				Pos = Reader.Position;

				Link = Reader.ReadBlockLink();
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				Len = await this.recordHandler.GetPayloadSize(Reader);
				Reader.Position += Len;

				c = Reader.Position - Pos - 4;

				if (!Method(Link, ObjectId, Pos + 4, c))
					return false;
			}
			while (Reader.BytesLeft >= 4);

			return true;
		}

		private async Task<byte[]> TryExtractLargestObjectLocked(uint BlockIndex, bool AllowRotation, bool AllowMerge)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			byte[] Result = null;
			object ObjectId;
			uint Link;
			int Len, c;
			int Pos, LastPos;

			if (Header.LastBlockIndex != 0)
			{
				Result = await this.TryExtractLargestObjectLocked(Header.LastBlockIndex, AllowRotation, AllowMerge);

				if (Result is null && AllowMerge)
				{
					LastPos = 0;
					do
					{
						Pos = Reader.Position;
						Reader.SkipBlockLink();
						ObjectId = this.recordHandler.GetKey(Reader);
						if (ObjectId is null)
							break;

						Len = await this.recordHandler.GetPayloadSize(Reader);
						Reader.Position += Len;
						LastPos = Pos;
					}
					while (Reader.BytesLeft >= 4);

					if (LastPos != 0)
					{
						Reader.Position = LastPos;
						Link = Reader.ReadBlockLink();
						this.recordHandler.GetKey(Reader);
						Len = await this.recordHandler.GetPayloadSize(Reader);
						c = Reader.Position + Len - LastPos - 4;

						byte[] Separator = new byte[c];
						Array.Copy(Block, LastPos + 4, Separator, 0, c);

						MergeResult MergeResult = await this.MergeBlocksLocked(Link, Separator, Header.LastBlockIndex);

						if (MergeResult.Separator is null)
						{
							this.RegisterEmptyBlockLocked(Header.LastBlockIndex);
							Header.LastBlockIndex = Link;

							Array.Clear(Block, LastPos, c + 4);
							Header.BytesUsed -= (ushort)(c + 4);
							Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
							Array.Copy(BitConverter.GetBytes(Header.LastBlockIndex), 0, Block, 6, 4);

							this.QueueSaveBlockLocked(BlockIndex, Block);
							this.QueueSaveBlockLocked(Link, MergeResult.ResultBlock);

							await this.UpdateParentLinksLocked(Link, MergeResult.ResultBlock);

							return await this.TryExtractLargestObjectLocked(BlockIndex, AllowRotation, AllowMerge);
						}
					}
				}
			}
			else
			{
				int Prev = 0;
				int Prev2 = 0;

				do
				{
					Pos = Reader.Position;
					Link = Reader.ReadBlockLink();
					if (Link != 0)
						return null;

					ObjectId = this.recordHandler.GetKey(Reader);
					if (ObjectId is null)
					{
						Reader.Position = Pos;
						break;
					}

					Prev2 = Prev;
					Prev = Pos;

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
				while (Reader.BytesLeft >= 4);

				if (Prev2 == 0)
				{
					if (BlockIndex != 0 && AllowRotation)
					{
						Result = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);

						if (!(Result is null))
						{
							if (Prev != 0)
							{
								byte[] Result2 = new byte[Header.BytesUsed - 4];
								Array.Copy(Block, Prev + 4, Result2, 0, Header.BytesUsed - 4);

								Array.Copy(Result, 0, Block, Prev + 4, Result.Length);

								c = Result.Length + 4;
								if (c < Header.BytesUsed)
									Array.Clear(Block, BlockHeaderSize + c, Header.BytesUsed - c);

								Header.BytesUsed = (ushort)c;
								Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

								this.QueueSaveBlockLocked(BlockIndex, Block);

								Result = Result2;
							}
						}
					}
				}
				else
				{
					c = Reader.Position - Prev - 4;
					Result = new byte[c];
					Array.Copy(Block, Prev + 4, Result, 0, c);
					Array.Clear(Block, Prev, c + 4);

					Header.BytesUsed -= (ushort)(c + 4);
					Header.SizeSubtree--;
					Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
					Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

					this.QueueSaveBlockLocked(BlockIndex, Block);

					await this.DecreaseSizeLocked(Header.ParentBlockIndex);
				}
			}

			return Result;
		}

		private async Task<byte[]> TryExtractSmallestObjectLocked(uint BlockIndex, bool AllowRotation, bool AllowMerge)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			byte[] Result = null;
			object ObjectId;
			uint Link;
			int Len;
			int Pos;
			int First = 0;
			int Second = 0;
			int c;

			do
			{
				Pos = Reader.Position;
				Link = Reader.ReadBlockLink();
				if (Link != 0)
				{
					if (First == 0)
					{
						Result = await this.TryExtractSmallestObjectLocked(Link, AllowRotation, AllowMerge);

						if (Result is null && AllowMerge)
						{
							uint RightLink;

							First = Pos;
							ObjectId = this.recordHandler.GetKey(Reader);
							if (!(ObjectId is null))
							{
								Len = await this.recordHandler.GetPayloadSize(Reader);
								Reader.Position += Len;

								Second = Reader.Position;
								RightLink = Reader.ReadBlockLink();
								ObjectId = this.recordHandler.GetKey(Reader);
								if (ObjectId is null)
									RightLink = Header.LastBlockIndex;

								c = Second - First - 4;
								byte[] Separator = new byte[c];
								Array.Copy(Block, First + 4, Separator, 0, c);

								MergeResult MergeResult = await this.MergeBlocksLocked(Link, Separator, RightLink);

								if (MergeResult.Separator is null)
								{
									this.RegisterEmptyBlockLocked(RightLink);

									c = Header.BytesUsed + BlockHeaderSize - Second - 4;
									if (c > 0)
										Array.Copy(Block, Second + 4, Block, First + 4, c);
									else
										Array.Copy(BitConverter.GetBytes(Link), 0, Block, 6, 4);

									c = Second - First;
									Header.BytesUsed -= (ushort)c;
									Array.Clear(Block, Header.BytesUsed + BlockHeaderSize, c);
									Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

									this.QueueSaveBlockLocked(BlockIndex, Block);
									this.QueueSaveBlockLocked(Link, MergeResult.ResultBlock);

									await this.UpdateParentLinksLocked(Link, MergeResult.ResultBlock);

									return await this.TryExtractSmallestObjectLocked(BlockIndex, AllowRotation, AllowMerge);
								}
							}
						}

						return Result;
					}
					else
						return null;
				}
				else
				{
					ObjectId = this.recordHandler.GetKey(Reader);
					if (ObjectId is null)
					{
						Reader.Position = Pos;
						break;
					}

					if (First == 0)
						First = Pos;
					else if (Second == 0)
					{
						Second = Pos;
						break;
					}

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
			}
			while (Reader.BytesLeft >= 4);

			if (First == 0)
			{
				if (Header.LastBlockIndex != 0)
					return await this.TryExtractSmallestObjectLocked(Header.LastBlockIndex, AllowRotation, AllowMerge);
				else
					return null;
			}
			else if (Second == 0)
			{
				if (BlockIndex != 0 && AllowRotation)
				{
					byte[] NewChild = await this.RotateLeftLocked(BlockIndex, Header.ParentBlockIndex);

					if (!(NewChild is null))
					{
						c = Header.BytesUsed - 4;
						Result = new byte[c];
						Array.Copy(Block, First + 4, Result, 0, c);

						Array.Copy(NewChild, 0, Block, First + 4, NewChild.Length);

						c -= NewChild.Length;
						if (c != 0)
						{
							if (c > 0)
								Array.Clear(Block, First + 4 + NewChild.Length, c);

							Header.BytesUsed -= (ushort)c;
							Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
						}

						this.QueueSaveBlockLocked(BlockIndex, Block);
					}
				}
			}
			else
			{
				c = Second - First - 4;
				Result = new byte[c];
				Array.Copy(Block, First + 4, Result, 0, c);
				Array.Copy(Block, Second, Block, First, Header.BytesUsed + BlockHeaderSize - Second);

				Header.BytesUsed -= (ushort)(c + 4);
				Header.SizeSubtree--;
				Array.Clear(Block, Header.BytesUsed + BlockHeaderSize, c + 4);
				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
				Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

				this.QueueSaveBlockLocked(BlockIndex, Block);

				await this.DecreaseSizeLocked(Header.ParentBlockIndex);
			}

			return Result;
		}

		private async Task<byte[]> RotateLeftLocked(uint ChildIndex, uint BlockIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			int Pos, PrevPos, Len;
			uint BlockLink;
			uint PrevBlockLink;
			object ObjectId;
			bool IsEmpty;

			if (ChildIndex == Header.LastBlockIndex)
			{
				if (BlockIndex == 0)
					return null;
				else
					return await this.RotateLeftLocked(BlockIndex, Header.ParentBlockIndex);
			}

			BlockLink = 0;
			Pos = 0;
			do
			{
				PrevBlockLink = BlockLink;
				PrevPos = Pos;

				Pos = Reader.Position;

				BlockLink = Reader.ReadBlockLink();                  // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				IsEmpty = ObjectId is null;
				if (IsEmpty)
					return null;

				Len = await this.recordHandler.GetPayloadSize(Reader);
				Reader.Position += Len;
			}
			while (PrevBlockLink != ChildIndex && Reader.BytesLeft >= 4);

			if (PrevBlockLink != ChildIndex)
				return null;

			bool Reshuffled = false;
			byte[] Object = await this.TryExtractSmallestObjectLocked(BlockLink, false, false);
			if (Object is null)
			{
				Reshuffled = true;
				Object = await this.TryExtractSmallestObjectLocked(BlockLink, true, false);
				if (Object is null)
				{
					Object = await this.TryExtractSmallestObjectLocked(BlockLink, false, true);
					if (Object is null)
						return null;
				}
			}

			if (Reshuffled)
			{
				Block = await this.LoadBlockLocked(BlockIndex, true); // Refresh
				Reader.Restart(Block, 0);
				Header = new BlockHeader(Reader);

				BlockLink = 0;
				Pos = 0;
				do
				{
					PrevBlockLink = BlockLink;
					PrevPos = Pos;

					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                  // Block link.
					ObjectId = this.recordHandler.GetKey(Reader);
					IsEmpty = ObjectId is null;
					if (IsEmpty)
					{
						BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectId(Object));
						await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);

						return null;
					}

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
				while (PrevBlockLink != ChildIndex && Reader.BytesLeft >= 4);

				if (PrevBlockLink != ChildIndex)
				{
					BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectId(Object));
					await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
					return null;
				}
			}

			int c;
			byte[] OldSeparator = new byte[c = Pos - PrevPos - 4];

			Array.Copy(Block, PrevPos + 4, OldSeparator, 0, c);

			await this.ReplaceObjectLocked(Object, new BlockInfo(Header, Block, BlockIndex, PrevPos, false), false);

			return OldSeparator;
		}

		private object GetObjectId(byte[] Object)
		{
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Object, this.blockLimit);
			return this.recordHandler.GetKey(Reader);
		}

		private async Task<byte[]> RotateRightLocked(uint ChildIndex, uint BlockIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			int Pos, PrevPos, Len;
			uint BlockLink;
			uint PrevBlockLink;
			object ObjectId;
			bool IsEmpty;

			BlockLink = 0;
			Pos = 0;
			do
			{
				PrevBlockLink = BlockLink;
				PrevPos = Pos;

				Pos = Reader.Position;

				BlockLink = Reader.ReadBlockLink();                  // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				IsEmpty = ObjectId is null;
				if (IsEmpty)
				{
					BlockLink = PrevBlockLink;
					Reader.Position = Pos;
					Pos = PrevPos;
					break;
				}

				Len = await this.recordHandler.GetPayloadSize(Reader);
				Reader.Position += Len;
			}
			while (BlockLink != ChildIndex && Reader.BytesLeft >= 4);

			if (BlockLink != ChildIndex)
			{
				PrevBlockLink = BlockLink;
				BlockLink = Header.LastBlockIndex;
				PrevPos = Pos;
				Pos = Reader.Position;

				if (BlockLink != ChildIndex)
					return null;
			}

			if (PrevPos == 0)
				return null;

			bool Reshuffled = false;
			byte[] Object = await this.TryExtractLargestObjectLocked(PrevBlockLink, false, false);
			if (Object is null)
			{
				Reshuffled = true;
				Object = await this.TryExtractLargestObjectLocked(PrevBlockLink, true, false);
				if (Object is null)
				{
					Object = await this.TryExtractLargestObjectLocked(PrevBlockLink, false, true);
					if (Object is null)
						return null;
				}
			}

			if (Reshuffled)
			{
				Block = await this.LoadBlockLocked(BlockIndex, true); // Refresh
				Reader.Restart(Block, 0);
				Header = new BlockHeader(Reader);

				BlockLink = 0;
				Pos = 0;
				do
				{
					PrevBlockLink = BlockLink;
					PrevPos = Pos;

					Pos = Reader.Position;

					BlockLink = Reader.ReadBlockLink();                  // Block link.
					ObjectId = this.recordHandler.GetKey(Reader);
					IsEmpty = ObjectId is null;
					if (IsEmpty)
					{
						BlockLink = PrevBlockLink;
						Reader.Position = Pos;
						Pos = PrevPos;
						break;
					}

					Len = await this.recordHandler.GetPayloadSize(Reader);
					Reader.Position += Len;
				}
				while (BlockLink != ChildIndex && Reader.BytesLeft >= 4);

				if (BlockLink != ChildIndex)
				{
					BlockLink = Header.LastBlockIndex;
					PrevPos = Pos;
					Pos = Reader.Position;

					if (BlockLink != ChildIndex)
					{
						BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectId(Object));
						await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);
						return null;
					}
				}
			}

			int c;
			byte[] OldSeparator = new byte[c = Pos - PrevPos - 4];

			Array.Copy(Block, PrevPos + 4, OldSeparator, 0, c);

			await this.ReplaceObjectLocked(Object, new BlockInfo(Header, Block, BlockIndex, PrevPos, false), false);

			return OldSeparator;
		}

		private Task DecreaseSizeLocked(uint BlockIndex)
		{
			return this.DecreaseSizeLocked(BlockIndex, 1);
		}

		private async Task DecreaseSizeLocked(uint BlockIndex, uint Delta)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			if (Delta == 0)
				return;

			while (true)
			{
				byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
				uint Size = BitConverter.ToUInt32(Block, 2);

				if (Size == uint.MaxValue)
				{
					ulong TotCount = await this.GetObjectCountLocked(BlockIndex, true);
					if (TotCount >= uint.MaxValue)
						return;

					Size = (uint)TotCount;
				}
				else
					Size -= Delta;

				Array.Copy(BitConverter.GetBytes(Size), 0, Block, 2, 4);

				this.QueueSaveBlockLocked(BlockIndex, Block);

				if (BlockIndex == 0)
					return;

				BlockIndex = BitConverter.ToUInt32(Block, 10);
			}
		}

		private async Task IncreaseSizeLocked(uint BlockIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			while (true)
			{
				byte[] Block = await this.LoadBlockLocked(BlockIndex, true);
				uint Size = BitConverter.ToUInt32(Block, 2);

				if (Size < uint.MaxValue)
				{
					Size++;
					Array.Copy(BitConverter.GetBytes(Size), 0, Block, 2, 4);
					this.QueueSaveBlockLocked(BlockIndex, Block);

					if (BlockIndex == 0)
						return;

					BlockIndex = BitConverter.ToUInt32(Block, 10);
				}
			}
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Provides a report on the current state of the file.
		/// </summary>
		/// <param name="WriteStat">If statistics is to be included in the report.</param>
		/// <param name="Properties">If object properties should be exported as well, in case the database is corrupt or unbalanced.</param>
		/// <returns>Report</returns>
		public async Task<string> GetCurrentStateReport(bool WriteStat, bool Properties)
		{
			StringBuilder Output = new StringBuilder();
			Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
			FileStatistics Statistics;

			await this.BeginWrite();
			try
			{
				Statistics = await this.ComputeStatisticsLocked(ObjectIds, null);
			}
			finally
			{
				await this.EndWrite();
			}

			if (Statistics.IsCorrupt)
				Output.AppendLine("Database is corrupt.");

			if (!Statistics.IsBalanced)
				Output.AppendLine("Database is unbalanced.");

			if (Statistics.IsCorrupt || !Statistics.IsBalanced)
				Output.AppendLine();

			Statistics.ToString(Output, WriteStat);

			if (Statistics.IsCorrupt || !Statistics.IsBalanced)
			{
				Output.AppendLine();
				await this.ExportGraphXML(Output, Properties);
				Output.AppendLine();
			}

			return Output.ToString();
		}

		/// <summary>
		/// Goes through the entire file and computes statistics abouts its composition.
		/// </summary>
		/// <returns>File statistics and found Object IDs.</returns>
		public virtual async Task<KeyValuePair<FileStatistics, Dictionary<Guid, bool>>> ComputeStatistics()
		{
			await this.BeginWrite();
			try
			{
				return await this.ComputeStatisticsLocked();
			}
			finally
			{
				await this.EndWrite();
			}
		}

		/// <summary>
		/// Goes through the entire file and computes statistics abouts its composition.
		/// </summary>
		/// <returns>File statistics and found Object IDs.</returns>
		public virtual async Task<KeyValuePair<FileStatistics, Dictionary<Guid, bool>>> ComputeStatisticsLocked()
		{
			Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
			FileStatistics Result = await this.ComputeStatisticsLocked(ObjectIds, null);
			return new KeyValuePair<FileStatistics, Dictionary<Guid, bool>>(Result, ObjectIds);
		}

		internal async Task<FileStatistics> ComputeStatisticsLocked(Dictionary<Guid, bool> ObjectIds, Dictionary<Guid, bool> ExistingIds)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			this.blockLimit = this.file.BlockLimit + this.blocksAdded;
			this.blobBlockLimit = this.blobFile is null ? 0 : this.blobFile.BlockLimit;

			FileStatistics Statistics = new FileStatistics((uint)this.blockSize, this.nrBlockLoads, this.nrCacheLoads, this.nrBlockSaves,
				this.nrBlobBlockLoads, this.nrBlobBlockSaves, this.nrFullFileScans, this.nrSearches);
			BitArray BlocksReferenced = new BitArray((int)this.blockLimit);
			BitArray BlobBlocksReferenced = new BitArray((int)this.blobBlockLimit);
			int i;

			try
			{
				byte[] Block = await this.LoadBlockLocked(0, false);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);

				BlockHeader.SkipHeader(Reader);

				await this.AnalyzeBlock(1, 0, 0, Statistics, BlocksReferenced, BlobBlocksReferenced, ObjectIds, ExistingIds, null, null);

				List<int> Blocks = new List<int>();

				for (i = 0; i < this.blockLimit; i++)
				{
					if (!BlocksReferenced[i])
					{
						Statistics.LogError("Block " + i.ToString() + " is not referenced.");
						Blocks.Add(i);
					}
				}

				Statistics.UnreferencedBlocks = Blocks.ToArray();
				Blocks.Clear();

				for (i = 0; i < this.blobBlockLimit; i++)
				{
					if (!BlobBlocksReferenced[i])
						Statistics.LogError("BLOB Block " + i.ToString() + " is not referenced.");
				}

				Statistics.UnreferencedBlobBlocks = Blocks.ToArray();
			}
			catch (Exception ex)
			{
				Statistics.LogError(ex.Message);
			}

			if (Statistics.NrObjects == 0)
			{
				Statistics.MinObjectSize = 0;
			}

			return Statistics;
		}

		private async Task<ulong> AnalyzeBlock(uint Depth, uint ParentIndex, uint BlockIndex, FileStatistics Statistics,
			BitArray BlocksReferenced, BitArray BlobBlocksReferenced, Dictionary<Guid, bool> ObjectIds, Dictionary<Guid, bool> ExistingIds,
			object MinExclusive, object MaxExclusive)
		{
			if (BlockIndex >= BlocksReferenced.Length)
			{
				Statistics.LogError("Referenced block not available in file: " + BlockIndex.ToString());
				return 0;
			}

			lock (BlocksReferenced)
			{
				if (BlocksReferenced[(int)BlockIndex])
				{
					Statistics.LogError("Block referenced multiple times: " + BlockIndex.ToString());
					return 0;
				}
				else
					BlocksReferenced[(int)BlockIndex] = true;
			}

			byte[] Block = await this.LoadBlockLocked(BlockIndex, false);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BinaryDeserializer Reader2;
			BlockHeader Header = new BlockHeader(Reader);

			if (Header.ParentBlockIndex != ParentIndex)
			{
				Statistics.LogError("Parent link in block " + BlockIndex.ToString() + " invalid. Should point to " + ParentIndex.ToString() +
					" but points to " + Header.ParentBlockIndex.ToString() + ".");
			}

			object ObjectId;
			uint Len;
			int Pos = 14;
			int Pos2;
			uint BlockLink;
			ulong NrObjects = 0;
			uint ObjectsInBlock = 0;
			bool Leaf = true;
			object MinObjectId = null;
			object LastObjectId = MinExclusive;
			int NrChildLinks = 0;
			int NrSeparators = 0;

			while (Reader.BytesLeft >= 4)
			{
				BlockLink = Reader.ReadBlockLink();                    // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				if (ObjectId is Guid Guid)
				{
					if (ObjectIds.ContainsKey(Guid))
						Statistics.LogError("Object ID " + Guid.ToString() + " occurred multiple times in file.");
					else
						ObjectIds[Guid] = true;

					if (!(ExistingIds is null))
					{
						if (!ExistingIds.ContainsKey(Guid))
							Statistics.LogError("Object ID " + Guid.ToString() + " referenced in file, but no such object exists in master file.");
					}
				}

				NrSeparators++;

				if (!(MinExclusive is null) && this.recordHandler.Compare(ObjectId, MinExclusive) <= 0)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + ", contains an object with an Object ID (" + ObjectId.ToString() +
						") that is smaller or equal to the smallest allowed value (" + MinExclusive.ToString() + ").");
				}

				if (!(MaxExclusive is null) && this.recordHandler.Compare(ObjectId, MaxExclusive) >= 0)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + ", contains an object with an Object ID (" + ObjectId.ToString() +
						") that is larger or equal to the largest allowed value (" + MaxExclusive.ToString() + ").");
				}

				if (!(LastObjectId is null) && this.recordHandler.Compare(LastObjectId, ObjectId) >= 0)
					Statistics.LogError("Objects in block " + BlockIndex.ToString() + " are not sorted correctly.");

				LastObjectId = ObjectId;

				ObjectsInBlock++;
				NrObjects++;
				if (BlockLink != 0)
				{
					NrChildLinks++;
					Leaf = false;
					NrObjects += await this.AnalyzeBlock(Depth + 1, BlockIndex, BlockLink, Statistics,
						BlocksReferenced, BlobBlocksReferenced, ObjectIds, ExistingIds, MinObjectId, ObjectId);
				}

				Len = await this.recordHandler.GetFullPayloadSize(Reader);
				Statistics.ReportObjectStatistics((uint)(Reader.Position - Pos - 4 + Len));

				if (Len == 0)
				{
					if (!(this.recordHandler is IndexRecords))
						Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object of length 0.");
				}
				else
				{
					Pos2 = 0;

					if (Reader.Position - Pos - 4 + Len > this.inlineObjectSizeLimit)
					{
						try
						{
							Reader2 = await this.LoadBlobLocked(Block, Pos + 4, BlobBlocksReferenced, Statistics);
							Reader2.Position = 0;
							Pos2 = Reader2.Data.Length;
						}
						catch (Exception ex)
						{
							Statistics.LogError(ex.Message);
							Reader2 = null;
						}

						Len = 4;
						Reader.Position += (int)Len;
					}
					else
					{
						if (Len > Reader.BytesLeft)
						{
							Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object of length " + Len.ToString() + ", which does not fit in the block.");
							break;
						}
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
						int Len2;

						try
						{
							object Obj = await this.genericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false, false);
							Len2 = Pos2 - Reader2.Position;
						}
						catch (Exception ex)
						{
							Statistics.LogError(ex.Message);
							Len2 = 0;
						}

						if (Len2 != 0)
						{
							Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object (" + ObjectId.ToString() +
								") that is not serialized correctly.");
							break;
						}
					}
				}

				MinObjectId = ObjectId;
				Pos = Reader.Position;
			}

			if (Header.BytesUsed != Pos - BlockHeaderSize)
			{
				Statistics.LogError("Number of bytes used as recorded in block " + BlockIndex.ToString() +
					" is wrong. Reported size: " + Header.BytesUsed.ToString() + ", Actual size: " + (Pos - BlockHeaderSize).ToString());
			}

			int i;

			for (i = Reader.Position; i < this.blockSize; i++)
			{
				if (Block[i] != 0)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + " contains garbage after end of objects, starting at byte " + i.ToString() + ".");
					break;
				}
			}

			if (Header.LastBlockIndex != 0)
			{
				NrChildLinks++;
				Leaf = false;
				NrObjects += await this.AnalyzeBlock(Depth + 1, BlockIndex, Header.LastBlockIndex, Statistics,
					BlocksReferenced, BlobBlocksReferenced, ObjectIds, ExistingIds, MinObjectId, null);
			}

			if (!Leaf && NrChildLinks != NrSeparators + 1)
			{
				Statistics.IsBalanced = false;
				Statistics.LogError("Block " + BlockIndex.ToString() + " has " + NrSeparators.ToString() + " separator(s), but only " +
					NrChildLinks.ToString() + " child node link(s), should be " + (NrSeparators + 1).ToString() + ".");
			}

			int Used = Header.BytesUsed + BlockHeaderSize;
			int Unused = this.blockSize - Used;

			if (Unused < 0)
			{
				Unused = 0;
				Statistics.LogError("Block " + BlockIndex.ToString() + " uses more bytes than the block size.");
			}

			/* Allow empty nodes, since it's required for optimal storage of an increasing sequence of Object IDs.
             *
             *	if (ObjectsInBlock == 0)
             *		Statistics.LogError("Block " + BlockIndex.ToString() + " is empty.");
             */

			Statistics.ReportBlockStatistics((uint)Used, (uint)Unused, ObjectsInBlock);

			if (Leaf)
				Statistics.ReportDepthStatistics(Depth);

			if (NrObjects != Header.SizeSubtree)
			{
				if (Header.SizeSubtree != uint.MaxValue)
					Statistics.LogError("Size of subtree rooted at block " + BlockIndex.ToString() + " is wrong. Stated count: " + Header.SizeSubtree.ToString() + ", Actual count: " + NrObjects.ToString());
				else
					Statistics.LogComment("Size field of block " + BlockIndex.ToString() + " cannot hold actual subtree size: " + NrObjects.ToString());
			}

			return NrObjects;
		}

		#endregion

		#region Graphs

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Graph XML.</returns>
		public Task<string> ExportGraphXML(bool Properties)
		{
			return this.ExportGraphXML(Properties, false);
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <param name="Locked">If a write lock has been taken already.</param>
		/// <returns>Graph XML.</returns>
		public async Task<string> ExportGraphXML(bool Properties, bool Locked)
		{
			StringBuilder Output = new StringBuilder();
			await this.ExportGraphXML(Output, Properties, Locked);
			return Output.ToString();
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Asynchronous task object.</returns>
		public Task ExportGraphXML(StringBuilder Output, bool Properties)
		{
			return this.ExportGraphXML(Output, Properties, false);
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <param name="Locked">If a write lock has been taken already.</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXML(StringBuilder Output, bool Properties, bool Locked)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = System.Text.Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter w = XmlWriter.Create(Output, Settings))
			{
				await this.ExportGraphXML(w, Properties, Locked);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="XmlOutput">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Asynchronous task object.</returns>
		public Task ExportGraphXML(XmlWriter XmlOutput, bool Properties)
		{
			return this.ExportGraphXML(XmlOutput, Properties, false);
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="XmlOutput">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <param name="Locked">If a write lock has been taken already.</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXML(XmlWriter XmlOutput, bool Properties, bool Locked)
		{
			if (!Locked)
				await this.BeginWrite();
			try
			{
				await this.ExportGraphXMLLocked(XmlOutput, Properties);
			}
			finally
			{
				if (!Locked)
					await this.EndWrite();
			}
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="XmlOutput">XML Output</param>
		/// <param name="Properties">If object properties should be exported as well.</param>
		/// <returns>Asynchronous task object.</returns>
		internal async Task ExportGraphXMLLocked(XmlWriter XmlOutput, bool Properties)
		{
			BinaryDeserializer Reader = null;
			long NrBlocks = this.file.BlockLimit + this.blocksAdded;
			byte[] BlobBlock = new byte[this.blobBlockSize];
			byte[] DecryptedBlock;
			uint Link;

			this.blockLimit = (uint)NrBlocks;

			XmlOutput.WriteStartElement("Collection", "http://waher.se/Schema/Persistence/Files.xsd");
			XmlOutput.WriteAttributeString("name", this.collectionName);

			XmlOutput.WriteStartElement("BTreeFile");
			XmlOutput.WriteAttributeString("fileName", this.fileName);

			await this.ExportGraphXMLLocked(0, XmlOutput, Properties);

			XmlOutput.WriteEndElement();

			if (!(this.blobFile is null))
			{
				XmlOutput.WriteStartElement("BlobFile");
				XmlOutput.WriteAttributeString("fileName", this.blobFileName);

				for (uint BlobBlockIndex = 0; BlobBlockIndex < this.blobFile.BlockLimit; BlobBlockIndex++)
				{
					await this.blobFile.LoadBlock(BlobBlockIndex, BlobBlock);
					this.nrBlobBlockLoads++;

					if (this.encrypted)
					{
						using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(((long)BlobBlockIndex) * this.blobBlockSize)))
						{
							DecryptedBlock = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
						}
					}
					else
						DecryptedBlock = (byte[])BlobBlock.Clone();

					if (Reader is null)
						Reader = new BinaryDeserializer(this.collectionName, this.encoding, DecryptedBlock, this.blockLimit);
					else
						Reader.Restart(DecryptedBlock, 0);

					XmlOutput.WriteStartElement("Block");
					XmlOutput.WriteAttributeString("index", BlobBlockIndex.ToString());
					this.recordHandler.ExportKey(this.recordHandler.GetKey(Reader), XmlOutput);

					Link = Reader.ReadUInt32();
					if (Link != uint.MaxValue)
						XmlOutput.WriteAttributeString("prev", Link.ToString());

					Link = Reader.ReadUInt32();
					if (Link != uint.MaxValue)
						XmlOutput.WriteAttributeString("next", Link.ToString());

					XmlOutput.WriteEndElement();
				}

				XmlOutput.WriteEndElement();
			}

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile Index in this.indices)
				{
					XmlOutput.WriteStartElement("IndexFile");
					XmlOutput.WriteAttributeString("fileName", Index.FileName);

					await Index.ExportGraphXMLLocked(XmlOutput, false);

					XmlOutput.WriteEndElement();
				}
			}

			XmlOutput.WriteEndElement();
		}

		private async Task ExportGraphXMLLocked(uint BlockIndex, XmlWriter XmlOutput, bool Properties)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, false);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BinaryDeserializer Reader2;
			BinaryDeserializer BlobReader;
			BlockHeader Header = new BlockHeader(Reader);
			GenericObject Obj;
			object ObjectId;
			int Pos;
			uint Len;
			uint BlockLink;
			uint BlobLink = 0;
			long? BlobSize;

			XmlOutput.WriteStartElement("Block");
			XmlOutput.WriteAttributeString("index", BlockIndex.ToString());
			XmlOutput.WriteAttributeString("bytes", Header.BytesUsed.ToString());
			XmlOutput.WriteAttributeString("size", Header.SizeSubtree.ToString());

			while (Reader.BytesLeft >= 4)
			{
				Pos = Reader.Position;
				BlockLink = Reader.ReadBlockLink();                    // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				Len = await this.recordHandler.GetFullPayloadSize(Reader);
				if (Reader.Position - Pos - 4 + Len > this.inlineObjectSizeLimit)
				{
					BlobLink = Reader.ReadBlockLink();
					Len = 0;

					BlobReader = await this.LoadBlobLocked(Block, Pos + 4, null, null);
					BlobSize = BlobReader.Data.Length;

					Reader2 = BlobReader;
					Reader2.Position = 0;
				}
				else
				{
					BlobSize = null;
					Reader2 = Reader;

					if (Properties)
						Reader2.Position = Pos + 4;
				}

				XmlOutput.WriteStartElement("Object");
				this.recordHandler.ExportKey(ObjectId, XmlOutput);
				XmlOutput.WriteAttributeString("pos", Pos.ToString());

				if (BlobSize.HasValue)
				{
					XmlOutput.WriteAttributeString("blob", BlobSize.Value.ToString());
					XmlOutput.WriteAttributeString("blobLink", BlobLink.ToString());
				}

				if (Properties)
				{
					Obj = (GenericObject)await this.genericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);
					Len = (uint)(Reader.Position - Pos);
					XmlOutput.WriteAttributeString("len", Len.ToString());

					this.ExportGraphXMLLocked(XmlOutput, Obj);
				}
				else
				{
					Reader.Position += (int)Len;
					Len = (uint)(Reader.Position - Pos);
					XmlOutput.WriteAttributeString("len", Len.ToString());
				}

				if (BlockLink != 0)
					await this.ExportGraphXMLLocked(BlockLink, XmlOutput, Properties);

				XmlOutput.WriteEndElement();
			}

			if (Header.LastBlockIndex != 0)
				await this.ExportGraphXMLLocked(Header.LastBlockIndex, XmlOutput, Properties);

			XmlOutput.WriteEndElement();
		}

		private void ExportGraphXMLLocked(XmlWriter XmlOutput, GenericObject Object)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			LinkedList<KeyValuePair<string, Array>> Arrays = null;
			LinkedList<KeyValuePair<string, GenericObject>> Objects = null;
			object Value;
			uint TypeCode;
			string s;

			if (!string.IsNullOrEmpty(Object.TypeName))
				XmlOutput.WriteAttributeString("type", Object.TypeName);

			foreach (KeyValuePair<string, object> Property in Object)
			{
				s = Property.Key;
				Value = Property.Value;
				TypeCode = ObjectSerializer.GetFieldDataTypeCode(Value);

				if (s == "pos" || s == "len" || s == "blob" || s == "blobLink" || s == "objectId" || s == "type")
					s = "obj-" + s;

				switch (TypeCode)
				{
					case ObjectSerializer.TYPE_ARRAY:
						if (Arrays is null)
							Arrays = new LinkedList<KeyValuePair<string, Array>>();

						Arrays.AddLast(new KeyValuePair<string, Array>(s, (Array)Value));
						break;

					case ObjectSerializer.TYPE_OBJECT:
						if (Objects is null)
							Objects = new LinkedList<KeyValuePair<string, GenericObject>>();

						Objects.AddLast(new KeyValuePair<string, GenericObject>(s, (GenericObject)Value));
						break;

					default:
						XmlOutput.WriteAttributeString(s, Searching.Comparison.ToString(Property.Value));
						break;
				}
			}

			if (!(Arrays is null))
			{
				foreach (KeyValuePair<string, Array> P in Arrays)
				{
					XmlOutput.WriteStartElement(P.Key);

					foreach (object Item in P.Value)
						this.ExportGraphXMLLocked(XmlOutput, Item);

					XmlOutput.WriteEndElement();
				}
			}

			if (!(Objects is null))
			{
				foreach (KeyValuePair<string, GenericObject> P in Objects)
				{
					XmlOutput.WriteStartElement(P.Key);
					this.ExportGraphXMLLocked(XmlOutput, P.Value);
					XmlOutput.WriteEndElement();
				}
			}
		}

		private void ExportGraphXMLLocked(XmlWriter XmlOutput, object Item)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			uint TypeCode = ObjectSerializer.GetFieldDataTypeCode(Item);

			switch (TypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					XmlOutput.WriteElementString("Boolean", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_BYTE:
					XmlOutput.WriteElementString("Byte", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_BYTEARRAY:
					XmlOutput.WriteElementString("ByteArray", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_CHAR:
					XmlOutput.WriteElementString("Char", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_DATETIME:
					XmlOutput.WriteElementString("DateTime", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_DATETIMEOFFSET:
					XmlOutput.WriteElementString("DateTimeOffset", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					XmlOutput.WriteElementString("Decimal", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					XmlOutput.WriteElementString("Double", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_ENUM:
					XmlOutput.WriteElementString("Enum", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_GUID:
					XmlOutput.WriteElementString("Guid", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_INT16:
					XmlOutput.WriteElementString("Int16", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_INT32:
					XmlOutput.WriteElementString("Int32", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_INT64:
					XmlOutput.WriteElementString("Int64", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_NULL:
					XmlOutput.WriteElementString("Null", string.Empty);
					break;

				case ObjectSerializer.TYPE_SBYTE:
					XmlOutput.WriteElementString("SByte", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_SINGLE:
					XmlOutput.WriteElementString("Single", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_STRING:
					XmlOutput.WriteElementString("String", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_CI_STRING:
					XmlOutput.WriteElementString("CiString", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_TIMESPAN:
					XmlOutput.WriteElementString("TimeSpan", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_UINT16:
					XmlOutput.WriteElementString("UInt16", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_UINT32:
					XmlOutput.WriteElementString("UInt32", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_UINT64:
					XmlOutput.WriteElementString("UInt64", Searching.Comparison.ToString(Item, TypeCode));
					break;

				case ObjectSerializer.TYPE_ARRAY:
					XmlOutput.WriteStartElement("Array");

					foreach (object Item2 in (Array)Item)
						this.ExportGraphXMLLocked(XmlOutput, Item2);

					XmlOutput.WriteEndElement();
					break;

				case ObjectSerializer.TYPE_OBJECT:
					XmlOutput.WriteStartElement("Object");
					this.ExportGraphXMLLocked(XmlOutput, (GenericObject)Item);
					XmlOutput.WriteEndElement();
					break;

				default:
					XmlOutput.WriteAttributeString("Value", Searching.Comparison.ToString(Item));
					break;
			}
		}

		#endregion

		#region Order Statistic Tree

		/// <summary>
		/// Get number of objects in subtree spanned by <paramref name="BlockIndex">BlockIndex</paramref>.
		/// </summary>
		/// <param name="BlockIndex">Block index of root of subtree.</param>
		/// <param name="IncludeChildren">If objects in children are to be included in count.</param>
		/// <returns>Total number of objects in subtree.</returns>
		public async Task<ulong> GetObjectCount(uint BlockIndex, bool IncludeChildren)
		{
			await this.BeginRead();
			try
			{
				return await this.GetObjectCountLocked(BlockIndex, IncludeChildren);
			}
			finally
			{
				await this.EndRead();
			}
		}

		internal async Task<ulong> GetObjectCountLocked(uint BlockIndex, bool IncludeChildren)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, false);
			uint BlockSize;

			if (!IncludeChildren)
			{
				BlockSize = 0;

				await this.ForEachObject(Block, (Link, ObjectId2, Pos2, Len2) =>
				{
					BlockSize++;
					return true;
				});

				return BlockSize;
			}

			BlockSize = BitConverter.ToUInt32(Block, 2);
			if (BlockSize < uint.MaxValue)
				return BlockSize;

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
			BlockHeader Header = new BlockHeader(Reader);
			object ObjectId;
			uint Len;
			int Pos;
			uint BlockLink;
			ulong NrObjects = 0;

			while (Reader.BytesLeft >= 4)
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadBlockLink();                    // Block link.
				ObjectId = this.recordHandler.GetKey(Reader);
				if (ObjectId is null)
					break;

				NrObjects++;
				if (IncludeChildren && BlockLink != 0)
					NrObjects += await this.GetObjectCountLocked(BlockLink, IncludeChildren);

				Len = (uint)await this.recordHandler.GetPayloadSize(Reader);
				Reader.Position += (int)Len;
			}

			if (IncludeChildren && Header.LastBlockIndex != 0)
				NrObjects += await this.GetObjectCountLocked(Header.LastBlockIndex, IncludeChildren);

			return NrObjects;
		}

		private async Task<uint> GetObjectCount32Locked(uint BlockIndex)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			byte[] Block = await this.LoadBlockLocked(BlockIndex, false);
			return BitConverter.ToUInt32(Block, 2);
		}

		/// <summary>
		/// Calculates the rank of an object in the database, given its Object ID.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>Rank of object in database.</returns>
		/// <exception cref="KeyNotFoundException">If the object is not found.</exception>
		public async Task<ulong> GetRank(Guid ObjectId)
		{
			await this.BeginRead();
			try
			{
				return await this.GetRankLocked(ObjectId);
			}
			finally
			{
				await this.EndRead();
			}
		}

		internal async Task<ulong> GetRankLocked(object ObjectId)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertReadingOrWriting();
#endif
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info is null)
				throw new KeyNotFoundException("Object not found.");

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, this.blockLimit, BlockHeaderSize);
			ulong Rank = 0;
			int Len;
			uint BlockLink;
			object ObjectId2;
			bool IsEmpty;

			do
			{
				BlockLink = Reader.ReadBlockLink();                  // Block link.
				ObjectId2 = this.recordHandler.GetKey(Reader);
				IsEmpty = ObjectId2 is null;
				if (IsEmpty)
					break;

				if (BlockLink != 0)
					Rank += await this.GetObjectSizeOfBlockLocked(BlockLink);

				Len = await this.recordHandler.GetPayloadSize(Reader);

				if (this.recordHandler.Compare(ObjectId2, ObjectId) == 0)
				{
					BlockHeader Header = Info.Header;
					uint BlockIndex = Info.BlockIndex;

					while (BlockIndex != 0)
					{
						uint ParentIndex = Header.ParentBlockIndex;
						byte[] Block = await this.LoadBlockLocked(ParentIndex, true);
						Reader.Restart(Block, 0);
						Header = new BlockHeader(Reader);

						do
						{
							BlockLink = Reader.ReadBlockLink();                  // Block link.
							if (BlockLink == BlockIndex)
								break;

							ObjectId2 = this.recordHandler.GetKey(Reader);
							IsEmpty = ObjectId2 is null;
							if (IsEmpty)
								break;

							if (BlockLink != 0)
								Rank += await this.GetObjectSizeOfBlockLocked(BlockLink);

							Len = await this.recordHandler.GetPayloadSize(Reader);

							Rank++;
							Reader.Position += Len;
						}
						while (Reader.BytesLeft >= 4);

						BlockIndex = ParentIndex;
					}

					return Rank;
				}

				Rank++;
				Reader.Position += Len;
			}
			while (Reader.BytesLeft >= 4);

			throw new KeyNotFoundException("Object not found.");
		}

		#endregion

		#region ICollection<object>

		/// <summary>
		/// Checks if an item is stored in the file.
		/// </summary>
		/// <param name="Item">Object to check for.</param>
		/// <returns>If the object is stored in the file.</returns>
		public async Task<bool> ContainsAsync(object Item)
		{
			if (Item is null)
				return false;

			if (!(await this.provider.GetObjectSerializer(Item.GetType()) is ObjectSerializer Serializer))
				return false;

			if (!await Serializer.HasObjectId(Item))
				return false;

			Guid ObjectId = await Serializer.GetObjectId(Item, false, null);
			GenericObject Obj;

			await this.BeginRead();
			try
			{
				Obj = await this.TryLoadObjectLocked(ObjectId, this.genericSerializer) as GenericObject;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				await this.EndRead();
			}

			if (Obj is null)
				return false;

			try
			{
				BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
				await Serializer.Serialize(Writer, false, false, Item, null);
				byte[] Bin = Writer.GetSerialization();

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.blockLimit);
				if (!(await this.genericSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false) is GenericObject Obj2))
					return false;

				return Obj.Equals(Obj2);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Number of objects in file.
		/// </summary>
		public Task<ulong> CountAsync => this.GetObjectCount(0, true);

		/// <summary>
		/// Number of objects in file.
		/// </summary>
		internal Task<ulong> CountAsyncLocked => this.GetObjectCountLocked(0, true);

		/// <summary>
		/// <see cref="ICollection{Object}.IsReadOnly"/>
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Clears the database of all objects.
		/// </summary>
		/// <returns>Task object.</returns>
		public async Task ClearAsync()
		{
			await this.BeginWrite();
			try
			{
				await this.ClearAsyncLocked();
			}
			finally
			{
				await this.EndWrite();
			}
		}

		internal async Task ClearAsyncLocked()
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif
			await this.file.Truncate(0);

			if (!(this.blobFile is null))
				await this.blobFile.Truncate(0);

			if (!(this.indices is null))
			{
				foreach (IndexBTreeFile IndexFile in this.indices)
					await IndexFile.ClearAsyncLocked();
			}

			this.provider.RemoveBlocks(this.id);

			this.emptyBlocks?.Clear();
			this.blocksToSave?.Clear();
			this.objectsToSave?.Clear();
			this.objectsToLoad?.Clear();
			this.blocksAdded = 0;
			this.blockLimit = 0;
			this.blobBlockLimit = 0;

			await this.CreateNewBlockLocked();
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumeratorAsyncLocked{T}()"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public async Task<ObjectBTreeFileCursor<object>> GetEnumeratorAsyncLocked()
		{
			return await ObjectBTreeFileCursor<object>.CreateLocked(this, this.recordHandler, null);
		}

		/// <summary>
		/// Returns an typed enumerator that iterates through the collection. The typed enumerator uses
		/// the object serializer of <typeparamref name="T"/> to deserialize objects by default.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public Task<ObjectBTreeFileCursor<T>> GetTypedEnumeratorAsyncLocked<T>()
		{
			return ObjectBTreeFileCursor<T>.CreateLocked(this, this.recordHandler, null);
		}

		#endregion

		#region Indices

		/// <summary>
		/// Adds an index to the file. When objects are added, updated or deleted from the file, the corresponding references in the
		/// index file will be updated as well. The index files will be disposed together with the main file as well.
		/// </summary>
		/// <param name="Index">Index file to add.</param>
		/// <param name="Regenerate">If the index is to be regenerated.</param>
		internal async Task AddIndexLocked(IndexBTreeFile Index, bool Regenerate)
		{
#if ASSERT_LOCKS
			this.fileAccess.AssertWriting();
#endif

			lock (this.synchObject)
			{
				this.indexList.Add(Index);
				this.indices = this.indexList.ToArray();
			}

			if (Regenerate)
				await Index.RegenerateLocked();
		}

		/// <summary>
		/// Removes an index from the file.
		/// </summary>
		/// <param name="Index">Index file to add.</param>
		/// <returns>If the index was found and removed.</returns>
		public bool RemoveIndex(IndexBTreeFile Index)
		{
			bool Result;

			lock (this.synchObject)
			{
				Result = this.indexList.Remove(Index);

				if (Result)
					this.indices = this.indexList.ToArray();
			}

			return Result;
		}

		/// <summary>
		/// Available indices.
		/// </summary>
		public IndexBTreeFile[] Indices
		{
			get { return this.indices; }
		}

		/// <summary>
		/// Finds the best index for finding objects using  a given property.
		/// </summary>
		/// <param name="BestNrFields">Number of index fields used in best index.</param>
		/// <param name="Property">Property to search on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <param name="SortOrder">Sort order result is to be presented with.</param>
		/// <returns>Best index to use for the search. If no index is found matching the properties, null is returned.</returns>
		internal IndexBTreeFile FindBestIndex(out int BestNrFields, string Property, string[] SortOrder)
		{
			string s, s2;

			if (SortOrder is null || SortOrder.Length == 0)
				return this.FindBestIndex(out BestNrFields, 1, 1, Property);

			s = SortOrder[0];
			if (s.StartsWith("-"))
				s = s.Substring(1);

			if (Property.StartsWith("-"))
				s2 = Property.Substring(1);
			else
				s2 = Property;

			if (s2 == s)
				return this.FindBestIndex(out BestNrFields, 1, 1, SortOrder);

			string[] Properties = new string[SortOrder.Length + 1];

			Properties[0] = Property;
			Array.Copy(SortOrder, 0, Properties, 1, SortOrder.Length);

			return this.FindBestIndex(out BestNrFields, 1, 1, Properties);
		}

		/// <summary>
		/// Finds the best index for finding objects using  a given set of properties. The method assumes the most restrictive
		/// property is mentioned first in <paramref name="Properties"/>.
		/// </summary>
		/// <param name="BestNrFields">Number of index fields used in best index.</param>
		/// <param name="Properties">Properties to search on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <param name="SortOrder">Sort order result is to be presented with.</param>
		/// <returns>Best index to use for the search. If no index is found matching the properties, null is returned.</returns>
		internal IndexBTreeFile FindBestIndex(out int BestNrFields, string[] Properties, string[] SortOrder)
		{
			string s2;
			int NrProperties;

			if (Properties is null || (NrProperties = Properties.Length) == 0)
				return this.FindBestIndex(out BestNrFields, 0, 0, SortOrder);

			if (SortOrder is null || SortOrder.Length == 0)
				return this.FindBestIndex(out BestNrFields, 1, NrProperties, Properties);

			List<string> TotProperties = new List<string>();
			Dictionary<string, bool> Added = new Dictionary<string, bool>();

			TotProperties.AddRange(Properties);

			foreach (string s in Properties)
			{
				if (s.StartsWith("-"))
					s2 = s.Substring(1);
				else
					s2 = s;

				Added[s2] = true;
			}

			foreach (string s in SortOrder)
			{
				if (s.StartsWith("-"))
					s2 = s.Substring(1);
				else
					s2 = s;

				if (!Added.ContainsKey(s2))
				{
					Added[s2] = true;
					TotProperties.Add(s);
				}
			}

			return this.FindBestIndex(out BestNrFields, 1, Properties.Length, TotProperties.ToArray());
		}

		/// <summary>
		/// Finds the best index for finding objects using  a given set of properties. The method assumes the most restrictive
		/// property is mentioned first in <paramref name="Properties"/>.
		/// </summary>
		/// <param name="Properties">Properties to search on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <returns>Best index to use for the search. If no index is found matching the properties, null is returned.</returns>
		internal IndexBTreeFile FindBestIndex(params string[] Properties)
		{
			int c = Properties?.Length ?? 0;

			return this.FindBestIndex(out int _, c > 0 ? 1 : 0, c, Properties);
		}

		/// <summary>
		/// Finds the best index for finding objects using  a given set of properties. The method assumes the most restrictive
		/// property is mentioned first in <paramref name="Properties"/>.
		/// </summary>
		/// <param name="BestNrFields">Number of index fields used in best index.</param>
		/// <param name="FirstRequired">Number of field names in index that must exist among properties.</param>
		/// <param name="RequiredProperties">Number of properties that required field names may choose from.</param>
		/// <param name="Properties">Properties to search on. By default, sort order is ascending.
		/// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
		/// <returns>Best index to use for the search. If no index is found matching the properties, null is returned.</returns>
		internal IndexBTreeFile FindBestIndex(out int BestNrFields, int FirstRequired, int RequiredProperties, params string[] Properties)
		{
			Dictionary<string, int> PropertyOrder = new Dictionary<string, int>();
			IndexBTreeFile Best = null;
			int i, c = Properties?.Length ?? 0;
			int MinOrdinal, NrFields;
			int BestMinOrdinal = int.MaxValue;
			bool RequiredMismatch;
			string s;

			BestNrFields = int.MinValue;

			for (i = 0; i < c; i++)
			{
				s = Properties[i];
				if (s.StartsWith("-"))
					s = s.Substring(1);

				PropertyOrder[s] = i;
			}

			foreach (IndexBTreeFile Index in this.indices)
			{
				MinOrdinal = int.MaxValue;
				NrFields = 0;
				RequiredMismatch = false;

				foreach (string FieldName in Index.FieldNames)
				{
					if (!PropertyOrder.TryGetValue(FieldName, out int PropertyOrdinal))
						break;

					if (NrFields < FirstRequired && PropertyOrdinal >= RequiredProperties)
					{
						RequiredMismatch = true;
						break;
					}

					NrFields++;

					if (PropertyOrdinal < MinOrdinal)
						MinOrdinal = PropertyOrdinal;
				}

				if (NrFields == 0 || RequiredMismatch)
					continue;

				if (NrFields > BestNrFields || (NrFields == BestNrFields && MinOrdinal < BestMinOrdinal))
				{
					Best = Index;
					BestNrFields = NrFields;
					BestMinOrdinal = MinOrdinal;
				}
			}

			return Best;
		}

		#endregion

		#region Searching

		/// <summary>
		/// Checks that indices have been loaded and are active for searching.
		/// </summary>
		public async Task CheckIndicesInitialized<T>()
		{
			if (!this.indicesCreated)
			{
				ObjectSerializer Serializer = await this.provider.GetObjectSerializerEx(typeof(T));
				string CollectionName = await Serializer.CollectionName(null) ?? this.collectionName;
				ObjectBTreeFile File = await this.provider.GetFile(CollectionName);
				if (File == this)
				{
					foreach (string[] Index in Serializer.Indices)
						await this.provider.GetIndexFile(File, RegenerationOptions.RegenerateIfIndexNotInstantiated, Index);

					this.indicesCreated = true;
				}
			}
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
		public async Task<ICursor<T>> FindLocked<T>(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ICursor<T> Result;

			this.nrSearches++;

			if (!this.indicesCreated)
				throw new InvalidOperationException("Indices not initialized.");

			if (Filter is null)
			{
				Result = null;

				if (!(SortOrder is null) && SortOrder.Length > 0)
				{
					IndexBTreeFile Index = this.FindBestIndex(SortOrder);

					if (Index is null)
					{
						if (await this.provider.GetObjectSerializer(typeof(T)) is ObjectSerializer Serializer &&
							!(Serializer is null) &&
							Serializer.HasObjectIdField)
						{
							if (SortOrder[0] == Serializer.ObjectIdMemberName)
							{
								Result = await this.GetTypedEnumeratorAsyncLocked<T>();
								Result = new Searching.ObjectIdCursor<T>(Result, Serializer.ObjectIdMemberName);
							}
							else if (SortOrder[0] == "-" + Serializer.ObjectIdMemberName)
							{
								Result = await this.GetTypedEnumeratorAsyncLocked<T>();
								Result = new Searching.ObjectIdCursor<T>(Result, Serializer.ObjectIdMemberName);
								Result = new Searching.ReversedCursor<T>(Result);
							}
						}
					}
					else
					{
						if (Index.SameSortOrder(null, SortOrder))
							Result = await Index.GetTypedEnumeratorLocked<T>();
						else if (Index.ReverseSortOrder(null, SortOrder))
							Result = new Searching.ReversedCursor<T>(await Index.GetTypedEnumeratorLocked<T>());
					}
				}

				if (Result is null)
				{
					this.nrFullFileScans++;
					Result = await this.GetTypedEnumeratorAsyncLocked<T>();

					if (!(SortOrder is null) && SortOrder.Length > 0)
						Result = await this.SortLocked<T>(Result, this.ConvertFilter(Filter)?.ConstantFields, SortOrder, true, false);
				}

				if (Offset > 0 || MaxCount < int.MaxValue)
					Result = new Searching.PagesCursor<T>(Offset, MaxCount, Result);
			}
			else
			{
				Result = await this.ConvertFilterToCursorLocked<T>(Filter.Normalize(), SortOrder);

				if (!(SortOrder is null) && SortOrder.Length > 0)
					Result = await this.SortLocked<T>(Result, this.ConvertFilter(Filter)?.ConstantFields, SortOrder, true, true);

				if (Offset > 0 || MaxCount < int.MaxValue)
					Result = new Searching.PagesCursor<T>(Offset, MaxCount, Result);
			}

			return Result;
		}

		private async Task<ICursor<T>> SortLocked<T>(ICursor<T> Result, string[] ConstantFields, string[] SortOrder, bool CanReverse, bool IndexFound)
		{
			if (Result.SameSortOrder(ConstantFields, SortOrder))
				return Result;

			if (CanReverse && Result.ReverseSortOrder(ConstantFields, SortOrder))
				return new Searching.ReversedCursor<T>(Result);

			if (!IndexFound)
			{
				Log.Notice("Sort order in search result did not match index.",
					this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
					string.Empty, Log.CleanStackTrace(Environment.StackTrace),
					new KeyValuePair<string, object>("Collection", this.collectionName),
					new KeyValuePair<string, object>("ConstantFields", ToString(ConstantFields)),
					new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));
			}

			SortedDictionary<Searching.SortedReference<T>, bool> SortedObjects;
			IndexRecords Records;
			byte[] Key;

			Records = new IndexRecords(this.collectionName, this.encoding, int.MaxValue, SortOrder);
			SortedObjects = new SortedDictionary<Searching.SortedReference<T>, bool>();

			while (await Result.MoveNextAsyncLocked())
			{
				if (!Result.CurrentTypeCompatible)
					continue;

				Key = await Records.Serialize(Result.CurrentObjectId, Result.Current, Result.CurrentSerializer, MissingFieldAction.Null);
				SortedObjects[new Searching.SortedReference<T>(Key, Records, Result.Current, Result.CurrentSerializer, Result.CurrentObjectId)] = true;
			}

			return new Searching.SortedCursor<T>(SortedObjects, Records);
		}

		private string ToString(string[] SortOrder)
		{
			if (SortOrder is null)
				return string.Empty;
			else
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (string s in SortOrder)
				{
					if (First)
						First = false;
					else
						sb.Append(", ");

					sb.Append(s);
				}

				return sb.ToString();
			}
		}

		internal async Task<ICursor<T>> ConvertFilterToCursorLocked<T>(Filter Filter, string[] SortOrder)
		{
			Searching.IApplicableFilter ApplicableFilter;

			if (Filter is FilterChildren FilterChildren)
			{
				Filter[] ChildFilters = FilterChildren.ChildFilters;

				if (Filter is FilterAnd)
				{
					List<string> Properties = null;
					LinkedList<KeyValuePair<Searching.FilterFieldLikeRegEx, string>> RegExFields = null;
					string FieldName;

					foreach (Filter ChildFilter in ChildFilters)
					{
						if (ChildFilter is FilterFieldValue FilterFieldValue)
						{
							if (!(FilterFieldValue is FilterFieldNotEqualTo))
							{
								if (Properties is null)
									Properties = new List<string>();

								FieldName = FilterFieldValue.FieldName;
								if (!Properties.Contains(FieldName))
									Properties.Add(FieldName);
							}
						}
						else if (ChildFilter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
						{
							Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(ChildFilter);
							string ConstantPrefix = this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

							if (RegExFields is null)
								RegExFields = new LinkedList<KeyValuePair<Searching.FilterFieldLikeRegEx, string>>();

							RegExFields.AddLast(new KeyValuePair<Searching.FilterFieldLikeRegEx, string>(FilterFieldLikeRegEx2, ConstantPrefix));

							if (!string.IsNullOrEmpty(ConstantPrefix))
							{
								if (Properties is null)
									Properties = new List<string>();

								FieldName = FilterFieldLikeRegEx2.FieldName;
								if (!Properties.Contains(FieldName))
									Properties.Add(FieldName);
							}
						}
					}

					IndexBTreeFile Index;
					int NrFields;
					int i;

					if (Properties is null)
					{
						NrFields = 0;
						Index = null;
					}
					else
					{
						Index = this.FindBestIndex(out NrFields, Properties.ToArray(), SortOrder);

						if (NrFields > Properties.Count)
							NrFields = Properties.Count;
					}

					if (Index is null)
					{
						if (await this.provider.GetObjectSerializer(typeof(T)) is ObjectSerializer Serializer &&
							!(Serializer is null) &&
							Serializer.HasObjectIdField)
						{
							ICursor<T> Cursor;
							int c = ChildFilters.Length;
							int j;

							for (i = 0; i < c; i++)
							{
								if (ChildFilters[i] is FilterFieldValue FilterFieldValue &&
									FilterFieldValue.FieldName == Serializer.ObjectIdMemberName &&
									!((Cursor = await this.TryGetObjectIdCursorLocked<T>(FilterFieldValue, Serializer)) is null))
								{
									Filter Rest;

									if (c == 2)
										Rest = ChildFilters[1 - i];
									else
									{
										Filter[] ChildrenLeft = new Filter[c - 1];

										for (j = 0; j < c; j++)
										{
											if (j < i)
												ChildrenLeft[j] = ChildFilters[j];
											else if (j > i)
												ChildrenLeft[j - 1] = ChildFilters[j];
										}

										Rest = new FilterAnd(ChildFilters);
									}

									ApplicableFilter = this.ConvertFilter(Rest);
									return new Searching.FilteredCursor<T>(Cursor, ApplicableFilter,
										false, true, this.provider);
								}
							}
						}

						this.nrFullFileScans++;
						Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
							this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
							string.Empty, Log.CleanStackTrace(Environment.StackTrace),
							new KeyValuePair<string, object>("Collection", this.collectionName),
							new KeyValuePair<string, object>("Filter", Filter?.ToString()),
							new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

						ApplicableFilter = this.ConvertFilter(Filter);
						return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(),
							ApplicableFilter, false, true, this.provider);
					}

					Searching.RangeInfo[] RangeInfo = new Searching.RangeInfo[NrFields];
					Dictionary<string, int> FieldOrder = new Dictionary<string, int>();
					List<Searching.IApplicableFilter> AdditionalFields = null;

					i = 0;

					foreach (string FieldName2 in Index.FieldNames)
					{
						RangeInfo[i] = new Searching.RangeInfo(FieldName2);
						FieldOrder[FieldName2] = i++;
						if (i >= NrFields)
							break;
					}

					bool Consistent = true;
					bool Smaller;

					foreach (Filter ChildFilter in ChildFilters)
					{
						if (ChildFilter is FilterFieldValue FilterFieldValue)
						{
							if (!FieldOrder.TryGetValue(FilterFieldValue.FieldName, out i) || ChildFilter is FilterFieldNotEqualTo)
							{
								if (AdditionalFields is null)
									AdditionalFields = new List<Searching.IApplicableFilter>();

								AdditionalFields.Add(this.ConvertFilter(FilterFieldValue));
								continue;
							}

							if (FilterFieldValue is FilterFieldEqualTo)
							{
								if (!RangeInfo[i].SetPoint(FilterFieldValue.Value))
								{
									Consistent = false;
									break;
								}
							}
							else if (FilterFieldValue is FilterFieldGreaterOrEqualTo)
							{
								if (!RangeInfo[i].SetMin(FilterFieldValue.Value, true, out Smaller))
								{
									Consistent = false;
									break;
								}
							}
							else if (FilterFieldValue is FilterFieldLesserOrEqualTo)
							{
								if (!RangeInfo[i].SetMax(FilterFieldValue.Value, true, out Smaller))
								{
									Consistent = false;
									break;
								}
							}
							else if (FilterFieldValue is FilterFieldGreaterThan)
							{
								if (!RangeInfo[i].SetMin(FilterFieldValue.Value, false, out Smaller))
								{
									Consistent = false;
									break;
								}
							}
							else if (FilterFieldValue is FilterFieldLesserThan)
							{
								if (!RangeInfo[i].SetMax(FilterFieldValue.Value, false, out Smaller))
								{
									Consistent = false;
									break;
								}
							}
							else
								throw this.UnknownFilterType(Filter);
						}
						else if (ChildFilter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
						{
							Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = RegExFields.First.Value.Key;
							string ConstantPrefix = RegExFields.First.Value.Value;

							RegExFields.RemoveFirst();

							if (AdditionalFields is null)
								AdditionalFields = new List<Searching.IApplicableFilter>();

							AdditionalFields.Add(FilterFieldLikeRegEx2);

							if (!FieldOrder.TryGetValue(FilterFieldLikeRegEx.FieldName, out i) || string.IsNullOrEmpty(ConstantPrefix))
								continue;

							if (!RangeInfo[i].SetMin(ConstantPrefix, true, out Smaller))
							{
								Consistent = false;
								break;
							}
						}
						else if (ChildFilter is ICustomFilter CustomFilter)
						{
							if (AdditionalFields is null)
								AdditionalFields = new List<Searching.IApplicableFilter>();

							AdditionalFields.Add(new Searching.FilterCustom(CustomFilter));
						}
						else
							throw this.UnknownFilterType(Filter);
					}

					if (Consistent)
						return new Searching.RangesCursor<T>(Index, RangeInfo, AdditionalFields?.ToArray(), this.provider);
					else
						return new Searching.UnionCursor<T>(new Filter[0], this);   // Empty result set.
				}
				else if (Filter is FilterOr)
				{
					bool DoFullScan = false;

					foreach (Filter F in ChildFilters)
					{
						if (this.GeneratesFullFileScan(F))
						{
							DoFullScan = true;
							break;
						}
					}

					if (DoFullScan)
					{
						this.nrFullFileScans++;
						Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
							this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
							string.Empty, Log.CleanStackTrace(Environment.StackTrace),
							new KeyValuePair<string, object>("Collection", this.collectionName),
							new KeyValuePair<string, object>("Filter", Filter?.ToString()),
							new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

						ApplicableFilter = this.ConvertFilter(Filter);
						return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(),
							ApplicableFilter, false, true, this.provider);
					}
					else
						return new Searching.UnionCursor<T>(ChildFilters, this);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterChild FilterChild)
			{
				if (Filter is FilterNot)
				{
					Filter NegatedFilter = FilterChild.ChildFilter.Negate();

					if (NegatedFilter is FilterNot)
					{
						this.nrFullFileScans++;
						Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
							this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
							string.Empty, Log.CleanStackTrace(Environment.StackTrace),
							new KeyValuePair<string, object>("Collection", this.collectionName),
							new KeyValuePair<string, object>("Filter", Filter?.ToString()),
							new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

						ApplicableFilter = this.ConvertFilter(Filter);
						return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(),
							ApplicableFilter, false, true, this.provider);
					}
					else
						return await this.ConvertFilterToCursorLocked<T>(NegatedFilter, SortOrder);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldValue FilterFieldValue)
			{
				object Value = FilterFieldValue.Value;
				IndexBTreeFile Index = this.FindBestIndex(out int _, FilterFieldValue.FieldName, SortOrder);
				ICursor<T> Cursor;

				if (Index is null)
				{
					if (await this.provider.GetObjectSerializer(typeof(T)) is ObjectSerializer Serializer &&
						!(Serializer is null) &&
						Serializer.HasObjectIdField &&
						Serializer.ObjectIdMemberName == FilterFieldValue.FieldName)
					{
						Cursor = await this.TryGetObjectIdCursorLocked<T>(FilterFieldValue, Serializer);
						if (!(Cursor is null))
							return Cursor;
					}

					this.nrFullFileScans++;
					Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
						this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
						string.Empty, Log.CleanStackTrace(Environment.StackTrace),
						new KeyValuePair<string, object>("Collection", this.collectionName),
						new KeyValuePair<string, object>("Filter", Filter?.ToString()),
						new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

					ApplicableFilter = this.ConvertFilter(Filter);
					return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(),
						ApplicableFilter, false, true, this.provider);
				}
				else if (Filter is FilterFieldEqualTo)
				{
					bool UntilFirstFail;

					ApplicableFilter = this.ConvertFilter(Filter);

					if (Index.SameSortOrder(ApplicableFilter.ConstantFields, SortOrder))
					{
						UntilFirstFail = true;
						Cursor = await Index.FindFirstGreaterOrEqualToLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value));
					}
					else if (Index.ReverseSortOrder(ApplicableFilter.ConstantFields, SortOrder))
					{
						UntilFirstFail = true;
						Cursor = new Searching.ReversedCursor<T>(await Index.FindLastLesserOrEqualToLocked<T>(
							new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)));
					}
					else
					{
						Log.Notice("Search resulted in large part of the file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
							this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
							string.Empty, Log.CleanStackTrace(Environment.StackTrace),
							new KeyValuePair<string, object>("Collection", this.collectionName),
							new KeyValuePair<string, object>("Filter", Filter?.ToString()),
							new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

						UntilFirstFail = false;
						Cursor = await Index.FindFirstGreaterOrEqualToLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value));
					}

					return new Searching.FilteredCursor<T>(Cursor, ApplicableFilter, UntilFirstFail, true, this.provider);
				}
				else if (Filter is FilterFieldNotEqualTo)
				{
					this.nrFullFileScans++;
					Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
						this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
						string.Empty, Log.CleanStackTrace(Environment.StackTrace),
						new KeyValuePair<string, object>("Collection", this.collectionName),
						new KeyValuePair<string, object>("Filter", Filter?.ToString()),
						new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

					ApplicableFilter = this.ConvertFilter(Filter);
					return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(),
						ApplicableFilter, false, true, this.provider);
				}
				else
				{
					bool IsSorted = (!(SortOrder is null) && SortOrder.Length > 0);

					ApplicableFilter = this.ConvertFilter(Filter);

					if (Filter is FilterFieldGreaterOrEqualTo)
					{
						if (IsSorted && Index.ReverseSortOrder(ApplicableFilter.ConstantFields, SortOrder))
						{
							return new Searching.FilteredCursor<T>(
								await Index.GetTypedEnumeratorLocked<T>(),
								ApplicableFilter, true, false, this.provider);
						}
						else
						{
							return new Searching.FilteredCursor<T>(
								await Index.FindFirstGreaterOrEqualToLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
								null, false, true, this.provider);
						}
					}
					else if (Filter is FilterFieldLesserOrEqualTo)
					{
						if (IsSorted && Index.SameSortOrder(ApplicableFilter.ConstantFields, SortOrder))
						{
							return new Searching.FilteredCursor<T>(
								await Index.GetTypedEnumeratorLocked<T>(),
								ApplicableFilter, true, true, this.provider);
						}
						else
						{
							return new Searching.FilteredCursor<T>(
								await Index.FindLastLesserOrEqualToLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
								null, false, false, this.provider);
						}
					}
					else if (Filter is FilterFieldGreaterThan)
					{
						if (IsSorted && Index.ReverseSortOrder(ApplicableFilter.ConstantFields, SortOrder))
						{
							return new Searching.FilteredCursor<T>(
								await Index.GetTypedEnumeratorLocked<T>(),
								ApplicableFilter, true, false, this.provider);
						}
						else
						{
							return new Searching.FilteredCursor<T>(
								await Index.FindFirstGreaterThanLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
								null, false, true, this.provider);
						}
					}
					else if (Filter is FilterFieldLesserThan)
					{
						if (IsSorted && Index.SameSortOrder(ApplicableFilter.ConstantFields, SortOrder))
						{
							return new Searching.FilteredCursor<T>(
								await Index.GetTypedEnumeratorLocked<T>(),
								ApplicableFilter, true, true, this.provider);
						}
						else
						{
							return new Searching.FilteredCursor<T>(
							await Index.FindLastLesserThanLocked<T>(new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
							null, false, false, this.provider);
						}
					}
					else
						throw this.UnknownFilterType(Filter);
				}
			}
			else if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
			{
				Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter);
				IndexBTreeFile Index = this.FindBestIndex(out int _, FilterFieldLikeRegEx.FieldName, SortOrder);

				string ConstantPrefix = Index is null ? string.Empty : this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

				if (string.IsNullOrEmpty(ConstantPrefix))
				{
					this.nrFullFileScans++;
					Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
						this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
						string.Empty, Log.CleanStackTrace(Environment.StackTrace),
						new KeyValuePair<string, object>("Collection", this.collectionName),
						new KeyValuePair<string, object>("Filter", Filter?.ToString()),
						new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

					return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(), FilterFieldLikeRegEx2,
						false, true, this.provider);
				}
				else
				{
					ICursor<T> Cursor = await Index.FindFirstGreaterOrEqualToLocked<T>(
						new KeyValuePair<string, object>(FilterFieldLikeRegEx.FieldName, ConstantPrefix));

					int c = ConstantPrefix.Length - 1;
					char LastChar = ConstantPrefix[c];

					if (LastChar < char.MaxValue)
					{
						string ConstantPrefix2 = ConstantPrefix.Substring(0, c) + new string((char)(LastChar + 1), 1);

						Cursor = new Searching.FilteredCursor<T>(Cursor,
							new Searching.FilterFieldLesserThan(FilterFieldLikeRegEx.FieldName, ConstantPrefix2),
							true, true, Provider);
					}

					return new Searching.FilteredCursor<T>(Cursor, FilterFieldLikeRegEx2, false, true, this.provider);
				}
			}
			else if (Filter is ICustomFilter CustomFilter)
			{
				this.nrFullFileScans++;
				Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.",
					this.fileName, string.Empty, "DBOpt", EventLevel.Minor, string.Empty,
					string.Empty, Log.CleanStackTrace(Environment.StackTrace),
					new KeyValuePair<string, object>("Collection", this.collectionName),
					new KeyValuePair<string, object>("Filter", Filter?.ToString()),
					new KeyValuePair<string, object>("SortOrder", ToString(SortOrder)));

				return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsyncLocked<T>(), new Searching.FilterCustom(CustomFilter),
					false, true, this.provider);
			}
			else
				throw this.UnknownFilterType(Filter);
		}

		private async Task<ICursor<T>> TryGetObjectIdCursorLocked<T>(FilterFieldValue FilterFieldValue,
			ObjectSerializer Serializer)
		{
			object Value = FilterFieldValue.Value;
			Guid ObjectId;

			if (Value is Guid Guid)
				ObjectId = Guid;
			else if (Value is string s)
				ObjectId = new Guid(s);
			else if (Value is byte[] ba)
				ObjectId = new Guid(ba);
			else
				return new Searching.EmptyCursor<T>();

			if (FilterFieldValue is FilterFieldEqualTo)
			{
				try
				{

					T Obj = await this.LoadObject<T>(ObjectId);
					return new Searching.ObjectIdCursor<T>(
						new Searching.SingletonCursor<T>(Obj, Serializer, ObjectId),
						FilterFieldValue.FieldName);
				}
				catch (Exception)
				{
					return new Searching.EmptyCursor<T>();
				}
			}
			else if (FilterFieldValue is FilterFieldGreaterThan)
			{
				if (Searching.Comparison.Increment(ref ObjectId))
				{
					BlockInfo Info = (await this.FindNodeLocked(ObjectId)) ?? await this.FindLeafNodeLocked(ObjectId);
					ObjectBTreeFileCursor<T> e = await this.GetTypedEnumeratorAsyncLocked<T>();
					e.SetStartingPoint(Info);

					return new Searching.ObjectIdCursor<T>(e, FilterFieldValue.FieldName);
				}
			}
			else if (FilterFieldValue is FilterFieldGreaterOrEqualTo)
			{
				BlockInfo Info = (await this.FindNodeLocked(ObjectId)) ?? await this.FindLeafNodeLocked(ObjectId);
				ObjectBTreeFileCursor<T> e = await this.GetTypedEnumeratorAsyncLocked<T>();
				e.SetStartingPoint(Info);

				return new Searching.ObjectIdCursor<T>(e, FilterFieldValue.FieldName);
			}
			else if (FilterFieldValue is FilterFieldLesserThan)
			{
				BlockInfo Info = (await this.FindNodeLocked(ObjectId)) ?? await this.FindLeafNodeLocked(ObjectId);
				ObjectBTreeFileCursor<T> e = await this.GetTypedEnumeratorAsyncLocked<T>();
				e.SetStartingPoint(Info);

				return new Searching.ReversedCursor<T>(
					new Searching.ObjectIdCursor<T>(e, FilterFieldValue.FieldName));
			}
			else if (FilterFieldValue is FilterFieldLesserOrEqualTo)
			{
				if (Searching.Comparison.Increment(ref ObjectId))
				{
					BlockInfo Info = (await this.FindNodeLocked(ObjectId)) ?? await this.FindLeafNodeLocked(ObjectId);
					ObjectBTreeFileCursor<T> e = await this.GetTypedEnumeratorAsyncLocked<T>();
					e.SetStartingPoint(Info);

					return new Searching.ReversedCursor<T>(
						new Searching.ObjectIdCursor<T>(e, FilterFieldValue.FieldName));
				}
			}

			return null;
		}

		private bool GeneratesFullFileScan(Filter Filter)
		{
			if (Filter is FilterChildren FilterChildren)
			{
				Filter[] ChildFilters = FilterChildren.ChildFilters;

				if (Filter is FilterAnd)
				{
					List<string> Properties = null;
					LinkedList<KeyValuePair<Searching.FilterFieldLikeRegEx, string>> RegExFields = null;

					foreach (Filter ChildFilter in ChildFilters)
					{
						if (ChildFilter is FilterFieldValue FilterFieldValue)
						{
							if (!(FilterFieldValue is FilterFieldNotEqualTo))
							{
								if (Properties is null)
									Properties = new List<string>();

								Properties.Add(FilterFieldValue.FieldName);
							}
						}
						else if (ChildFilter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
						{
							Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(ChildFilter);
							string ConstantPrefix = this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

							if (RegExFields is null)
								RegExFields = new LinkedList<KeyValuePair<Searching.FilterFieldLikeRegEx, string>>();

							RegExFields.AddLast(new KeyValuePair<Searching.FilterFieldLikeRegEx, string>(FilterFieldLikeRegEx2, ConstantPrefix));

							if (!string.IsNullOrEmpty(ConstantPrefix))
							{
								if (Properties is null)
									Properties = new List<string>();

								Properties.Add(FilterFieldLikeRegEx2.FieldName);
							}
						}
					}

					IndexBTreeFile Index = Properties is null ? null : this.FindBestIndex(out int _, 1, Properties.Count, Properties.ToArray());
					return !(Index is null);
				}
				else if (Filter is FilterOr)
				{
					foreach (Filter F in ChildFilters)
					{
						if (this.GeneratesFullFileScan(F))
							return true;
					}

					return false;
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterChild FilterChild)
			{
				if (Filter is FilterNot)
				{
					Filter NegatedFilter = FilterChild.ChildFilter.Negate();

					if (NegatedFilter is FilterNot)
						return true;
					else
						return this.GeneratesFullFileScan(NegatedFilter);
				}
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldValue FilterFieldValue)
			{
				IndexBTreeFile Index = this.FindBestIndex(FilterFieldValue.FieldName);

				if (Index is null)
					return true;

				if (Filter is FilterFieldEqualTo ||
					Filter is FilterFieldGreaterOrEqualTo ||
					Filter is FilterFieldLesserOrEqualTo ||
					Filter is FilterFieldGreaterThan ||
					Filter is FilterFieldLesserThan)
				{
					return false;
				}
				else if (Filter is FilterFieldNotEqualTo)
					return true;
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
			{
				Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter);
				IndexBTreeFile Index = this.FindBestIndex(FilterFieldLikeRegEx.FieldName);

				string ConstantPrefix = Index is null ? string.Empty : this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

				if (string.IsNullOrEmpty(ConstantPrefix))
					return true;
				else
					return false;
			}
			else if (Filter is ICustomFilter)
				return true;
			else
				throw this.UnknownFilterType(Filter);
		}

		private string GetRegExConstantPrefix(string RegularExpression, Regex Regex)
		{
			StringBuilder Result = new StringBuilder();
			int[] GroupNumbers = null;
			int i, j, k, l, c = RegularExpression.Length;
			char ch;

			for (i = 0; i < c; i++)
			{
				ch = RegularExpression[i];
				if (ch == '\\')
				{
					i++;
					if (i < c)
					{
						ch = RegularExpression[i];

						switch (ch)
						{
							case 'a':
								Result.Append('\a');
								break;

							case 'b':
								Result.Append('\b');
								break;

							case 't':
								Result.Append('\t');
								break;

							case 'r':
								Result.Append('\r');
								break;

							case 'v':
								Result.Append('\v');
								break;

							case 'f':
								Result.Append('\f');
								break;

							case 'n':
								Result.Append('\n');
								break;

							case '.':
							case '$':
							case '^':
							case '{':
							case '[':
							case '(':
							case '|':
							case ')':
							case '*':
							case '+':
							case '?':
							case '\\':
								Result.Append(ch);
								break;

							case 'e':
								Result.Append('\u001B');
								break;

							case 'c':
								i++;
								if (i < c)
								{
									ch = RegularExpression[i++];

									if (ch == '@')
										Result.Append((char)ch);
									else if (ch >= 'A' && ch <= 'Z')
										Result.Append((char)(ch - 64));
									else
									{
										switch (ch)
										{
											case '[': Result.Append((char)27); break;
											case '\\': Result.Append((char)28); break;
											case ']': Result.Append((char)29); break;
											case '^': Result.Append((char)30); break;
											case '_': Result.Append((char)31); break;
											default: return Result.ToString();
										}
									}
								}
								else
									return Result.ToString();

								break;

							case 'x':
								i++;
								if (i + 2 <= c && int.TryParse(RegularExpression.Substring(i, 2), NumberStyles.HexNumber, null, out j))
								{
									i += 2;
									Result.Append((char)j);
								}
								else
									return Result.ToString();
								break;

							case 'u':
								i++;
								if (i + 4 <= c && int.TryParse(RegularExpression.Substring(i, 4), NumberStyles.HexNumber, null, out j))
								{
									i += 4;
									Result.Append((char)j);
								}
								else
									return Result.ToString();
								break;

							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
								j = i++;
								while ((k = i - j) < 3 && i < c && (ch = RegularExpression[i]) >= '0' && ch <= '9')
									i++;

								if (k == 1)
									return Result.ToString();

								l = int.Parse(RegularExpression.Substring(j, k));

								if (GroupNumbers is null)
									GroupNumbers = Regex.GetGroupNumbers();

								if (Array.IndexOf<int>(GroupNumbers, l) >= 0)
									return Result.ToString();

								k = 0;
								j = 0;
								while (l > 0)
								{
									k += (l % 10) << j;
									j += 3;
									l /= 10;
								}

								Result.Append((char)k);
								break;

							default:
								return Result.ToString();
						}
					}
				}
				else if (".$^{[(|)*+?".IndexOf(ch) >= 0)
					return Result.ToString();
				else
					Result.Append(ch);
			}

			return Result.ToString();
		}

		private Searching.IApplicableFilter ConvertFilter(Filter Filter)
		{
			if (Filter is null)
				return null;

			if (!(Filter.Tag is null) && Filter.Tag is Searching.IApplicableFilter Result)
				return Result;

			if (Filter is FilterFieldValue FilterFieldValue)
			{
				object Value = FilterFieldValue.Value;

				if (Filter is FilterFieldEqualTo)
					Result = new Searching.FilterFieldEqualTo(FilterFieldValue.FieldName, Value);
				else if (Filter is FilterFieldNotEqualTo)
					Result = new Searching.FilterFieldNotEqualTo(FilterFieldValue.FieldName, Value);
				else if (Filter is FilterFieldGreaterThan)
					Result = new Searching.FilterFieldGreaterThan(FilterFieldValue.FieldName, Value);
				else if (Filter is FilterFieldGreaterOrEqualTo)
					Result = new Searching.FilterFieldGreaterOrEqualTo(FilterFieldValue.FieldName, Value);
				else if (Filter is FilterFieldLesserThan)
					Result = new Searching.FilterFieldLesserThan(FilterFieldValue.FieldName, Value);
				else if (Filter is FilterFieldLesserOrEqualTo)
					Result = new Searching.FilterFieldLesserOrEqualTo(FilterFieldValue.FieldName, Value);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
				Result = new Searching.FilterFieldLikeRegEx(FilterFieldLikeRegEx.FieldName, FilterFieldLikeRegEx.RegularExpression);
			else if (Filter is FilterChildren FilterChildren)
			{
				Filter[] ChildFilters = FilterChildren.ChildFilters;
				int i, c = ChildFilters.Length;
				Searching.IApplicableFilter[] ApplicableFilters = new Searching.IApplicableFilter[c];
				Filter[] ConvertedChildFilters = new Filter[c];

				for (i = 0; i < c; i++)
				{
					ApplicableFilters[i] = this.ConvertFilter(ChildFilters[i]);
					ConvertedChildFilters[i] = (Filter)ApplicableFilters[i];
				}

				if (Filter is FilterAnd)
					Result = new Searching.FilterAnd(ApplicableFilters, ConvertedChildFilters);
				else if (Filter is FilterOr)
					Result = new Searching.FilterOr(ApplicableFilters, ConvertedChildFilters);
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is FilterChild FilterChild)
			{
				if (Filter is FilterNot)
					Result = new Searching.FilterNot(this.ConvertFilter(FilterChild.ChildFilter));
				else
					throw this.UnknownFilterType(Filter);
			}
			else if (Filter is ICustomFilter CustomFilter)
				return new Searching.FilterCustom(CustomFilter);
			else
				throw this.UnknownFilterType(Filter);

			Filter.Tag = Result;

			return Result;
		}

		private Exception UnknownFilterType(Filter Filter)
		{
			return new NotSupportedException("Filters of type " + Filter.GetType().FullName + " not supported.");
		}

		#endregion

		#region Finding and Deleting

		/// <summary>
		/// Finds objects of a given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="Serializer">Object serializer.</param>
		/// <param name="Lazy">If operation can be performed at next opportune time.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// <param name="Callback">Method to call when operation completed.</param>
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<T>> FindDelete<T>(int Offset, int MaxCount, Filter Filter, ObjectSerializer Serializer, bool Lazy, string[] SortOrder, ObjectsCallback Callback)
			where T : class
		{
			await this.CheckIndicesInitialized<T>();

			IEnumerable<T> Objects;

			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						Objects = await this.FindDeleteLocked<T>(Offset, MaxCount, Filter, Serializer, SortOrder);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(new FindDeleteLazyRec()
					{
						T = typeof(T),
						Offset = Offset,
						MaxCount = MaxCount,
						Filter = Filter,
						Serializer = Serializer,
						SortOrder = SortOrder
					}, Serializer, Callback, WriteOp.FindDelete);

					return null;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					Objects = await this.FindDeleteLocked<T>(Offset, MaxCount, Filter, Serializer, SortOrder);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Objects);

			return Objects;
		}

		private class FindDeleteLazyRec
		{
			public Type T;
			public int Offset;
			public int MaxCount;
			public Filter Filter;
			public ObjectSerializer Serializer;
			public string[] SortOrder;
		}

		private async Task<IEnumerable<T>> FindDeleteLocked<T>(int Offset, int MaxCount, Filter Filter, ObjectSerializer Serializer, params string[] SortOrder)
			where T : class
		{
			ICursor<T> ResultSet = await this.FindLocked<T>(Offset, MaxCount, Filter, SortOrder);
			IEnumerable<T> Result = await FilesProvider.LoadAllLocked<T>(ResultSet);

			foreach (T Object in Result)
			{
				Guid ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
				if (ObjectId != Guid.Empty)
					await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null, 0);
			}

			return Result;
		}

		/// <summary>
		/// Finds objects in a given collection.
		/// </summary>
		/// <param name="Offset">Result offset.</param>
		/// <param name="MaxCount">Maximum number of objects to return.</param>
		/// <param name="Filter">Optional filter. Can be null.</param>
		/// <param name="Lazy">If operation can be performed at next opportune time.</param>
		/// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
		/// <param name="Callback">Method to call when operation completed.</param>
		/// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
		/// <returns>Objects found.</returns>
		public async Task<IEnumerable<object>> FindDelete(int Offset, int MaxCount, Filter Filter, bool Lazy, string[] SortOrder, ObjectsCallback Callback)
		{
			await this.CheckIndicesInitialized<object>();

			IEnumerable<object> Objects;

			if (Lazy)
			{
				if (await this.TryBeginWrite(0))
				{
					try
					{
						Objects = await this.FindDeleteLocked(Offset, MaxCount, Filter, SortOrder);
					}
					finally
					{
						await this.EndWrite();
					}
				}
				else
				{
					this.QueueForSave(new FindDeleteLazyRec()
					{
						T = typeof(object),
						Offset = Offset,
						MaxCount = MaxCount,
						Filter = Filter,
						Serializer = null,
						SortOrder = SortOrder
					}, null, Callback, WriteOp.FindDelete);

					return null;
				}
			}
			else
			{
				await this.BeginWrite();
				try
				{
					Objects = await this.FindDeleteLocked(Offset, MaxCount, Filter, SortOrder);
				}
				finally
				{
					await this.EndWrite();
				}
			}

			Callback?.Invoke(Objects);

			return Objects;
		}

		private async Task<IEnumerable<object>> FindDeleteLocked(int Offset, int MaxCount, Filter Filter, params string[] SortOrder)
		{
			ICursor<object> ResultSet = await this.FindLocked<object>(Offset, MaxCount, Filter, SortOrder);
			IEnumerable<object> Result = await FilesProvider.LoadAllLocked<object>(ResultSet);
			ObjectSerializer Serializer = null;
			Type LastType = null;
			Type Type;

			foreach (object Object in Result)
			{
				Type = Object.GetType();

				if (Serializer is null || Type != LastType)
				{
					Serializer = await this.provider.GetObjectSerializerEx(Type);
					LastType = Type;
				}

				Guid ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
				if (ObjectId != Guid.Empty)
					await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null, 0);
			}

			return Result;
		}

		private async Task<IEnumerable<object>> FindDeleteLocked(Type T, int Offset, int MaxCount, Filter Filter, ObjectSerializer Serializer, params string[] SortOrder)
		{
			TypeInfo TI = T.GetTypeInfo();
			FilterCustom<object> TypeFilter = new FilterCustom<object>(o => TI.IsAssignableFrom(o.GetType().GetTypeInfo()));

			if (Filter is null)
				Filter = TypeFilter;
			else
				Filter = new FilterAnd(Filter, TypeFilter);

			ICursor<object> ResultSet = await this.FindLocked<object>(Offset, MaxCount, Filter, SortOrder);
			IEnumerable<object> Result = await FilesProvider.LoadAllLocked<object>(ResultSet);

			foreach (object Object in Result)
			{
				Guid ObjectId = await Serializer.GetObjectId(Object, false, NestedLocks.CreateIfNested(this, true, Serializer));
				if (ObjectId != Guid.Empty)
					await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null, 0);
			}

			return Result;
		}

		#endregion

	}
}

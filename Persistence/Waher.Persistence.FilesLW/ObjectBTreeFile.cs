using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
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
    public class ObjectBTreeFile : MultiReadSingleWriteObject, ICollection<object>
    {
        internal const int BlockHeaderSize = 14;

        private IndexBTreeFile[] indices = new IndexBTreeFile[0];
        private List<IndexBTreeFile> indexList = new List<IndexBTreeFile>();
        private SortedDictionary<uint, bool> emptyBlocks = null;
        private readonly GenericObjectSerializer genericSerializer;
        private readonly FilesProvider provider;
        private FileStream file;
        private FileStream blobFile;
        private readonly Encoding encoding;
        private SortedDictionary<long, byte[]> blocksToSave = null;
        private LinkedList<KeyValuePair<object, ObjectSerializer>> objectsToSave = null;
        private LinkedList<Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter>> objectsToLoad = null;
        private readonly object synchObject = new object();
        private readonly IRecordHandler recordHandler;
        private ulong nrFullFileScans = 0;
        private ulong nrSearches = 0;
        private long bytesAdded = 0;
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
        private bool isCorrupt = false;
        private bool emptyRoot = false;
        private readonly Aes aes;
        private readonly byte[] aesKey;
        private readonly byte[] ivSeed;
        private readonly int ivSeedLen;
        private readonly bool encrypted;
        private bool indicesCreated = false;

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
        internal ObjectBTreeFile(string FileName, string CollectionName, string BlobFileName, int BlockSize, int BlobBlockSize,
            FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
            : this(FileName, CollectionName, BlobFileName, BlockSize, BlobBlockSize, Provider, Encoding,
                  TimeoutMilliseconds, Encrypted, null)
        {
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
        internal ObjectBTreeFile(string FileName, string CollectionName, string BlobFileName, int BlockSize,
            int BlobBlockSize, FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted, 
            IRecordHandler RecordHandler)
        {
            CheckBlockSizes(BlockSize, BlobBlockSize);

            if (TimeoutMilliseconds <= 0)
                throw new ArgumentOutOfRangeException("The timeout must be positive.", nameof(TimeoutMilliseconds));

            this.provider = Provider;
            this.id = this.provider.GetNewFileId();
            this.fileName = Path.GetFullPath(FileName);
            this.collectionName = CollectionName;
            this.blobFileName = string.IsNullOrEmpty(BlobFileName) ? string.Empty : Path.GetFullPath(BlobFileName);
            this.blockSize = BlockSize;
            this.blobBlockSize = BlobBlockSize;
            this.inlineObjectSizeLimit = (this.blockSize - BlockHeaderSize) / 2 - 4;
            this.encoding = Encoding;
            this.timeoutMilliseconds = TimeoutMilliseconds;
            this.genericSerializer = new GenericObjectSerializer(this.provider);
            this.encrypted = Encrypted;

            if (RecordHandler is null)
                this.recordHandler = new PrimaryRecords(this.inlineObjectSizeLimit);
            else
                this.recordHandler = RecordHandler;

            bool FileExists = File.Exists(this.fileName);

            if (this.encrypted)
            {
                this.aes = Aes.Create();
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;

                this.provider.GetKeys(this.fileName, FileExists, out this.aesKey, out this.ivSeed);
                this.ivSeedLen = this.ivSeed.Length;
            }

            string Folder = Path.GetDirectoryName(this.fileName);
            if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            if (FileExists)
                this.file = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            else
            {
                this.file = File.Open(this.fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                Task _ = this.CreateFirstBlock();
            }

            this.blockLimit = (uint)(this.file.Length / this.blockSize);

            if (string.IsNullOrEmpty(this.blobFileName))
            {
                this.blobFile = null;
                this.blobBlockLimit = 0;
            }
            else
            {
                Folder = Path.GetDirectoryName(this.blobFileName);
                if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);

                if (File.Exists(this.blobFileName))
                    this.blobFile = File.Open(this.blobFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                else
                    this.blobFile = File.Open(this.blobFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                this.blobBlockLimit = (uint)(this.blobFile.Length / this.blobBlockSize);
            }
        }

        internal static void CheckBlockSizes(int BlockSize, int BlobBlockSize)
        {
            if (BlockSize < 1024)
                throw new ArgumentOutOfRangeException("Block size too small.", nameof(BlockSize));

            if (BlockSize > 65536)
                throw new ArgumentOutOfRangeException("Block size too large.", nameof(BlockSize));

            if (BlobBlockSize < 1024)
                throw new ArgumentOutOfRangeException("BLOB Block size too small.", nameof(BlobBlockSize));

            if (BlobBlockSize > 65536)
                throw new ArgumentOutOfRangeException("BLOB Block size too large.", nameof(BlobBlockSize));

            int i = BlockSize;
            while (i != 0 && (i & 1) == 0)
                i >>= 1;

            if (i != 1)
                throw new ArgumentException("The block size must be a power of 2.", nameof(BlockSize));

            i = BlobBlockSize;
            while (i != 0 && (i & 1) == 0)
                i >>= 1;

            if (i != 1)
                throw new ArgumentException("The BLOB block size must be a power of 2.", nameof(BlobBlockSize));
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public override void Dispose()
        {
            this.file?.Dispose();
            this.file = null;

            if (!(this.indices is null))
            {
                foreach (IndexBTreeFile IndexFile in this.indices)
                    IndexFile.Dispose();

                this.indices = null;
                this.indexList = null;
            }

            this.blobFile?.Dispose();
            this.blobFile = null;

            this.provider.RemoveBlocks(this.id);

            base.Dispose();
        }

        internal IRecordHandler RecordHandler => this.recordHandler;
        internal GenericObjectSerializer GenericSerializer => this.genericSerializer;

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

        /// <summary>
        /// If the file has been detected to contain corruptions.
        /// </summary>
        public bool IsCorrupt
        {
            get { return this.isCorrupt; }
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

        internal async Task LockRead()
        {
            if (!await this.TryBeginRead(this.timeoutMilliseconds))
                throw new TimeoutException("Unable to get read access to database.");
        }

        internal async Task LockWrite()
        {
            if (!await this.TryBeginWrite(this.timeoutMilliseconds))
                throw new TimeoutException("Unable to get write access to database.");
        }

        /// <summary>
        /// Ends a reading session of the object.
        /// Must be called once for each call to <see cref="MultiReadSingleWriteObject.BeginRead"/> or successful call to 
        /// <see cref="MultiReadSingleWriteObject.TryBeginRead(int)"/>.
        /// </summary>
        public override async Task EndRead()
        {
            await base.EndRead();

            LinkedList<Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter>> ToLoad;

            lock (this.synchObject)
            {
                ToLoad = this.objectsToLoad;
                this.objectsToLoad = null;
            }

            if (!(ToLoad is null))
            {
                foreach (Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter> P in ToLoad)
                    P.Item3(await this.LoadObject(P.Item1, P.Item2));
            }
        }

        /// <summary>
        /// Ends a writing session of the object.
        /// Must be called once for each call to <see cref="MultiReadSingleWriteObject.BeginWrite"/> or successful call to 
        /// <see cref="MultiReadSingleWriteObject.TryBeginWrite(int)"/>.
        /// </summary>
        public override async Task EndWrite()
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
                    Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, true);
                    Reader.Restart(Block, 0);
                    Header = new BlockHeader(Reader);

                    this.RegisterEmptyBlockLocked(BlockIndex);
                }

                Array.Clear(Block, 10, 4);
                this.QueueSaveBlockLocked(0, Block);

                await this.UpdateParentLinksLocked(0, Block);
            }

            if (!(this.emptyBlocks is null))
                await this.RemoveEmptyBlocksLocked();

            if (!(this.blocksToSave is null) && this.blocksToSave.Count > 0 && !this.provider.InBulkMode(this))
                await this.SaveUnsaved();

            await base.EndWrite();

            LinkedList<KeyValuePair<object, ObjectSerializer>> ToSave;
            LinkedList<Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter>> ToLoad;

            lock (this.synchObject)
            {
                ToSave = this.objectsToSave;
                this.objectsToSave = null;

                ToLoad = this.objectsToLoad;
                this.objectsToLoad = null;
            }

            if (!(ToSave is null))
            {
                foreach (KeyValuePair<object, ObjectSerializer> P in ToSave)
                    await this.SaveNewObject(P.Key, P.Value);
            }

            if (!(ToLoad is null))
            {
                foreach (Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter> P in ToLoad)
                    P.Item3(await this.LoadObject(P.Item1, P.Item2));
            }
        }

        private async Task SaveUnsaved()
        {
            if (!(this.blocksToSave is null))
            {
                bool Changed = false;

                foreach (KeyValuePair<long, byte[]> Rec in this.blocksToSave)
                {
                    await this.DoSaveBlockLocked(Rec.Key, Rec.Value);
                    Changed = true;
                }

                if (Changed)
                {
                    this.blocksToSave.Clear();
                    this.bytesAdded = 0;
                    this.blockLimit = (uint)(this.file.Length / this.blockSize);
                }
            }
        }

        #endregion

        #region Blocks

        private async Task<Tuple<uint, byte[]>> CreateNewBlockLocked()
        {
            byte[] Block = null;
            long PhysicalPosition = long.MaxValue;

            if (!(this.emptyBlocks is null))
            {
                foreach (uint BlockIndex in this.emptyBlocks.Keys)
                {
                    this.emptyBlocks.Remove(BlockIndex);
                    if (this.emptyBlocks.Count == 0)
                        this.emptyBlocks = null;

                    PhysicalPosition = ((long)BlockIndex) * this.blockSize;
                    Block = await this.LoadBlockLocked(PhysicalPosition, true);

                    Array.Clear(Block, 0, this.blockSize);

                    break;
                }
            }

            if (Block is null)
            {
                Block = new byte[this.blockSize];
                PhysicalPosition = this.file.Length + this.bytesAdded;

                this.bytesAdded += this.blockSize;
                this.blockLimit++;
            }

            this.QueueSaveBlockLocked(PhysicalPosition, Block);

            return new Tuple<uint, byte[]>((uint)(PhysicalPosition / this.blockSize), Block);
        }

        private async Task CreateFirstBlock()
        {
            await this.LockWrite();
            try
            {
                await this.CreateNewBlockLocked();
            }
            finally
            {
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
        /// <param name="PhysicalPosition">Physical position of block in file.</param>
        /// <returns>Loaded block.</returns>
        public async Task<byte[]> LoadBlock(long PhysicalPosition)
        {
            await this.LockRead();
            try
            {
                return await this.LoadBlockLocked(PhysicalPosition, true);
            }
            finally
            {
                await this.EndRead();
            }
        }

        internal async Task<byte[]> LoadBlockLocked(long PhysicalPosition, bool AddToCache)
        {
            if ((PhysicalPosition % this.blockSize) != 0)
                throw new ArgumentException("Block positions must be multiples of the block size.", nameof(PhysicalPosition));

            if (this.provider.TryGetBlock(this.id, (uint)(PhysicalPosition / this.blockSize), out byte[] Block))
            {
                this.nrCacheLoads++;
                return Block;
            }

            if (!(this.blocksToSave is null) && this.blocksToSave.TryGetValue(PhysicalPosition, out Block))
            {
                this.nrCacheLoads++;
                return Block;
            }

            if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
                throw new ArgumentOutOfRangeException("Invalid file position.", nameof(PhysicalPosition));

            Block = new byte[this.blockSize];

            int NrRead = await this.file.ReadAsync(Block, 0, this.blockSize);
            if (this.blockSize != NrRead)
				throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

			this.nrBlockLoads++;

            if (this.encrypted)
            {
                using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
                {
                    Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
                }
            }

            if (AddToCache)
                this.provider.AddBlockToCache(this.id, (uint)(PhysicalPosition / this.blockSize), Block);

            return Block;
        }

        /// <summary>
        /// Saves a block to the file.
        /// </summary>
        /// <param name="PhysicalPosition">Physical position of block in file.</param>
        /// <param name="Block">Block to save.</param>
        /// <returns>Block to save.</returns>
        public async Task SaveBlock(long PhysicalPosition, byte[] Block)
        {
            await this.LockWrite();
            try
            {
                this.QueueSaveBlockLocked(PhysicalPosition, Block);
            }
            finally
            {
                await this.EndWrite();
            }
        }

        internal void QueueSaveBlockLocked(long PhysicalPosition, byte[] Block)
        {
            if ((PhysicalPosition % this.blockSize) != 0)
                throw new ArgumentException("Block positions must be multiples of the block size.", nameof(PhysicalPosition));

            if (Block is null || Block.Length != this.blockSize)
                throw new ArgumentException("Block not of the correct block size.", nameof(Block));

            uint BlockIndex = (uint)(PhysicalPosition / this.blockSize);

            if (this.provider.TryGetBlock(this.id, BlockIndex, out byte[] PrevBlock) && PrevBlock != Block)
            {
                if (Array.Equals(PrevBlock, Block))
                {
                    this.provider.AddBlockToCache(this.id, BlockIndex, Block);   // Update to new reference.
                    return;     // No need to save.
                }
            }

            if (this.blocksToSave is null)
                this.blocksToSave = new SortedDictionary<long, byte[]>();

            this.blocksToSave[PhysicalPosition] = Block;
            this.blockUpdateCounter++;

            this.provider.AddBlockToCache(this.id, (uint)(PhysicalPosition / this.blockSize), Block);
        }

        /// <summary>
        /// This counter gets updated each time a block is updated in the file.
        /// </summary>
        internal ulong BlockUpdateCounter
        {
            get { return this.blockUpdateCounter; }
        }

        internal async Task DoSaveBlockLocked(long PhysicalPosition, byte[] Block)
        {
            byte[] EncryptedBlock;

            if (this.encrypted)
            {
                using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition)))
                {
                    EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
                }
            }
            else
                EncryptedBlock = (byte[])Block.Clone();

            if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
                throw new ArgumentException("Invalid file position.", nameof(PhysicalPosition));

            await this.file.WriteAsync(EncryptedBlock, 0, this.blockSize);

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
            if (!(this.emptyBlocks is null))
            {
                BinaryDeserializer Reader;
                BlockHeader Header;
                long DestinationLocation;
                long SourceLocation;
                byte[] Block;
                uint PrevBlockIndex;
                uint ParentBlockIndex;

                foreach (uint BlockIndex in this.emptyBlocks.Keys)
                {
                    DestinationLocation = ((long)BlockIndex) * this.blockSize;
                    SourceLocation = this.file.Length + this.bytesAdded - this.blockSize;

                    if (DestinationLocation < SourceLocation)
                    {
                        PrevBlockIndex = (uint)(SourceLocation / this.blockSize);

                        Block = await this.LoadBlockLocked(SourceLocation, false);

                        if (!(this.blocksToSave is null))
                            this.blocksToSave.Remove(SourceLocation);

                        this.provider.RemoveBlock(this.id, (uint)(SourceLocation / this.blockSize));

                        this.QueueSaveBlockLocked(DestinationLocation, Block);
                        await this.UpdateParentLinksLocked(BlockIndex, Block);

                        ParentBlockIndex = BitConverter.ToUInt32(Block, 10);
                        SourceLocation = ((long)ParentBlockIndex) * this.blockSize;
                        Block = await this.LoadBlockLocked(SourceLocation, true);
                        Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
                        Header = new BlockHeader(Reader);

                        if (Header.LastBlockIndex == PrevBlockIndex)
                            Array.Copy(BitConverter.GetBytes(BlockIndex), 0, Block, 6, 4);
                        else
                        {
                            this.ForEachObject(Block, (Link, ObjectId, Pos, Len) =>
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

                        this.QueueSaveBlockLocked(SourceLocation, Block);
                    }
                    else
                    {
                        if (!(this.blocksToSave is null))
                            this.blocksToSave.Remove(DestinationLocation);

                        this.provider.RemoveBlock(this.id, (uint)(DestinationLocation / this.blockSize));

                        if (SourceLocation != DestinationLocation)
                        {
                            if (!(this.blocksToSave is null))
                                this.blocksToSave.Remove(SourceLocation);

                            this.provider.RemoveBlock(this.id, (uint)(SourceLocation / this.blockSize));
                        }
                    }

                    if (this.bytesAdded > 0)
                        this.bytesAdded -= this.blockSize;
                    else
                        this.file.SetLength(this.file.Length - this.blockSize);

                    this.blockLimit--;
                }

                this.emptyBlocks = null;
            }
        }

        #endregion

        #region BLOBs

        internal async Task<byte[]> SaveBlobLocked(byte[] Bin)
        {
            if (this.blobFile is null)
                throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

            BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.blockLimit);
            this.recordHandler.SkipKey(Reader);
            int KeySize = Reader.Position;
            int Len = (int)this.recordHandler.GetFullPayloadSize(Reader);
            int HeaderSize = Reader.Position;

            if (Len != Bin.Length - Reader.Position)
                throw new ArgumentException("Invalid serialization of object", nameof(Bin));

            this.blobBlockLimit = (uint)(this.blobFile.Length / this.blobBlockSize);

            byte[] Result = new byte[HeaderSize + 4];
            byte[] EncryptedBlock;
            Array.Copy(Bin, 0, Result, 0, HeaderSize);
            Array.Copy(BitConverter.GetBytes(this.blobBlockLimit), 0, Result, HeaderSize, 4);
            byte[] Block = new byte[this.blobBlockSize];
            int Left;
            uint Prev = uint.MaxValue;
            int Limit = this.blobBlockSize - KeySize - 8;
            int Pos = HeaderSize;

            Array.Copy(Bin, 0, Block, 0, KeySize);

            this.blobFile.Position = this.blobFile.Length;

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
                    using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(this.blobFile.Position)))
                    {
                        EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
                    }
                }
                else
                    EncryptedBlock = (byte[])Block.Clone();

                await this.blobFile.WriteAsync(EncryptedBlock, 0, this.blobBlockSize);
                this.nrBlobBlockSaves++;
            }

            return Result;
        }

        internal async Task<BinaryDeserializer> LoadBlobLocked(byte[] Block, int Pos, BitArray BlobBlocksReferenced, FileStatistics Statistics)
        {
            if (this.blobFile is null)
                throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

            BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, Pos);
            object ObjectId = this.recordHandler.GetKey(Reader);
            object ObjectId2;
            int KeySize = Reader.Position - Pos;
            uint Len = this.recordHandler.GetFullPayloadSize(Reader);
            int Bookmark = Reader.Position - Pos;
            uint BlobBlockIndex = Reader.ReadUInt32();
            uint ExpectedPrev = uint.MaxValue;
            uint Prev;
            byte[] Result = new byte[Bookmark + Len];
            byte[] BlobBlock = new byte[this.blobBlockSize];
            byte[] DecryptedBlock;
            long PhysicalPosition;
            int i = Bookmark;
            int NrRead;
            bool ChainError = false;

            Array.Copy(Block, Pos, Result, 0, Bookmark);
            Len += (uint)Bookmark;

            while (i < Len)
            {
                if (BlobBlockIndex == uint.MaxValue)
                    throw new FileException("BLOB " + ObjectId.ToString() + " ended prematurely.", this.fileName, this.collectionName);

                PhysicalPosition = ((long)BlobBlockIndex) * this.blobBlockSize;

                if (this.blobFile.Position != PhysicalPosition)
                    this.blobFile.Position = PhysicalPosition;

                NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                if (NrRead != this.blobBlockSize)
					throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

				this.nrBlobBlockLoads++;

                if (this.encrypted)
                {
                    using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
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
                    throw new FileException("Block linked to by BLOB " + ObjectId.ToString() + " (" + this.collectionName +
                        ") was actually marked as " + ObjectId2.ToString() + ".", this.fileName, this.collectionName);
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
                throw new FileException("BLOB " + ObjectId.ToString() + " did not end when expected.", this.fileName, this.collectionName);

            if (!(BlobBlocksReferenced is null) && ChainError)
                throw new FileException("Doubly linked list for BLOB " + ObjectId.ToString() + " is corrupt.", this.fileName, this.collectionName);

            Reader.Restart(Result, Bookmark);

            return Reader;
        }

        private async Task DeleteBlobLocked(byte[] Bin, int Offset)
        {
            if (this.blobFile is null)
                throw new FileException("BLOBs not supported in this file.", this.fileName, this.collectionName);

            SortedDictionary<uint, bool> BlocksToRemoveSorted = new SortedDictionary<uint, bool>();
            LinkedList<KeyValuePair<uint, byte[]>> ReplacementBlocks = new LinkedList<KeyValuePair<uint, byte[]>>();
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
            long PhysicalPosition;
            long PhysicalPosition2;
            int NrRead;
            int i, c;
            int KeySize = Reader.Position - Offset;

            Reader.SkipVariableLengthUInt64();
            BlobBlockIndex = Reader.ReadUInt32();

            while (BlobBlockIndex != uint.MaxValue)
            {
                PhysicalPosition = ((long)BlobBlockIndex) * this.blobBlockSize;

                if (this.blobFile.Position != PhysicalPosition)
                    this.blobFile.Position = PhysicalPosition;

                NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                if (NrRead != this.blobBlockSize)
					throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

				this.nrBlockLoads++;

                if (this.encrypted)
                {
                    using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
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

                BlobBlockIndex = BitConverter.ToUInt32(DecryptedBlock, KeySize + 4);
            }

            c = BlocksToRemoveSorted.Count;
            BlocksToRemove = new uint[c];
            BlocksToRemoveSorted.Keys.CopyTo(BlocksToRemove, 0);

            PhysicalPosition = this.blobFile.Length;
            BlobBlockIndex = (uint)(PhysicalPosition / this.blobBlockSize);

            for (i = c - 1; i >= 0; i--)
            {
                if (BlobBlockIndex == 0)
                    break;

                BlobBlockIndex--;
                PhysicalPosition -= this.blobBlockSize;

                this.blobFile.Position = PhysicalPosition;

                NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                if (NrRead != this.blobBlockSize)
					throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

				this.nrBlockLoads++;

                if (this.encrypted)
                {
                    using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
                    {
                        DecryptedBlock = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
                    }
                }
                else
                    DecryptedBlock = (byte[])BlobBlock.Clone();

                Reader.Restart(DecryptedBlock, 0);
                ObjectId2 = this.recordHandler.GetKey(Reader);
                if (ObjectId2 is null || this.recordHandler.Compare(ObjectId2, ObjectId) != 0)
                    ReplacementBlocks.AddFirst(new KeyValuePair<uint, byte[]>(BlobBlockIndex, DecryptedBlock));
            }

            i = 0;
            foreach (KeyValuePair<uint, byte[]> ReplacementBlock in ReplacementBlocks)
            {
                BlobBlockIndex = BlocksToRemove[i++];   // To
                Index = ReplacementBlock.Key;           // From

                TranslationFromTo[Index] = BlobBlockIndex;
            }

            i = 0;
            foreach (KeyValuePair<uint, byte[]> ReplacementBlock in ReplacementBlocks)
            {
                BlobBlockIndex = BlocksToRemove[i++];   // To
                PhysicalPosition = ((long)BlobBlockIndex) * this.blobBlockSize;

                Index = ReplacementBlock.Key;           // From
                DecryptedBlock = ReplacementBlock.Value;

                Prev = BitConverter.ToUInt32(DecryptedBlock, KeySize);
                Next = BitConverter.ToUInt32(DecryptedBlock, KeySize + 4);

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
                            Len = this.recordHandler.GetFullPayloadSize(Reader);
                            if (Reader.Position - Info.InternalPosition - 4 + Len > this.inlineObjectSizeLimit)
                            {
                                Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, Info.Block, Reader.Position, 4);
                                this.QueueSaveBlockLocked(((long)Info.BlockIndex) * this.blockSize, Info.Block);
                            }
                        }
                    }
                }
                else if (TranslationFromTo.TryGetValue(Prev, out To))
                    Array.Copy(BitConverter.GetBytes(To), 0, DecryptedBlock, KeySize, 4);
                else
                {
                    PhysicalPosition2 = ((long)Prev) * this.blobBlockSize;
                    this.blobFile.Position = PhysicalPosition2;

                    NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                    if (NrRead != this.blobBlockSize)
						throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

					this.nrBlockLoads++;

                    if (this.encrypted)
                    {
                        using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition2)))
                        {
                            DecryptedBlock2 = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
                        }
                    }
                    else
                        DecryptedBlock2 = BlobBlock;

                    Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, DecryptedBlock2, KeySize + 4, 4);

                    if (this.encrypted)
                    {
                        using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition2)))
                        {
                            EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock2, 0, DecryptedBlock2.Length);
                        }
                    }
                    else
                        EncryptedBlock = (byte[])DecryptedBlock2.Clone();

                    this.blobFile.Position = PhysicalPosition2;
                    await this.blobFile.WriteAsync(EncryptedBlock, 0, this.blobBlockSize);
                    this.nrBlockSaves++;
                }

                if (TranslationFromTo.TryGetValue(Next, out To))
                    Array.Copy(BitConverter.GetBytes(To), 0, DecryptedBlock, KeySize + 4, 4);
                else if (Next != uint.MaxValue)
                {
                    PhysicalPosition2 = ((long)Next) * this.blobBlockSize;
                    this.blobFile.Position = PhysicalPosition2;

                    NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                    if (NrRead != this.blobBlockSize)
						throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

					this.nrBlockLoads++;

                    if (this.encrypted)
                    {
                        using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition2)))
                        {
                            DecryptedBlock2 = Aes.TransformFinalBlock(BlobBlock, 0, BlobBlock.Length);
                        }
                    }
                    else
                        DecryptedBlock2 = BlobBlock;

                    Array.Copy(BitConverter.GetBytes(BlobBlockIndex), 0, DecryptedBlock2, KeySize, 4);

                    if (this.encrypted)
                    {
                        using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition2)))
                        {
                            EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock2, 0, DecryptedBlock2.Length);
                        }
                    }
                    else
                        EncryptedBlock = (byte[])DecryptedBlock2.Clone();

                    this.blobFile.Position = PhysicalPosition2;
                    await this.blobFile.WriteAsync(EncryptedBlock, 0, this.blobBlockSize);
                    this.nrBlockSaves++;
                }

                if (this.encrypted)
                {
                    using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition)))
                    {
                        EncryptedBlock = Aes.TransformFinalBlock(DecryptedBlock, 0, DecryptedBlock.Length);
                    }
                }
                else
                    EncryptedBlock = (byte[])DecryptedBlock.Clone();

                this.blobFile.Position = PhysicalPosition;
                await this.blobFile.WriteAsync(EncryptedBlock, 0, this.blobBlockSize);
                this.nrBlockSaves++;
            }

            this.blobFile.SetLength(this.blobFile.Length - BlocksToRemove.Length * this.blobBlockSize);
            this.blobBlockLimit = (uint)(this.blobFile.Length / this.blobBlockSize);
        }

        #endregion

        #region Save new objects

        /// <summary>
        /// Saves a new object to the file.
        /// </summary>
        /// <param name="Object">Object to persist.</param>
        public Task<Guid> SaveNewObject(object Object)
        {
            Type ObjectType = Object.GetType();
            ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(ObjectType);

            return this.SaveNewObject(Object, Serializer);
        }

        /// <summary>
        /// Saves a new object to the file.
        /// </summary>
        /// <param name="Object">Object to persist.</param>
        /// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
        public async Task<Guid> SaveNewObject(object Object, ObjectSerializer Serializer)
        {
            Guid ObjectId;

            await this.LockWrite();
            try
            {
                ObjectId = await this.SaveNewObjectLocked(Object, Serializer);
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.SaveNewObject(ObjectId, Object, Serializer);

            return ObjectId;
        }

        /// <summary>
        /// Saves a new set of objects to the file.
        /// </summary>
        /// <param name="Objects">Objects to persist.</param>
        /// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
        public async Task SaveNewObjects(IEnumerable<object> Objects, ObjectSerializer Serializer)
        {
            LinkedList<Guid> ObjectIds = new LinkedList<Guid>();

            await this.LockWrite();
            try
            {
                foreach (object Object in Objects)
                    ObjectIds.AddLast(await this.SaveNewObjectLocked(Object, Serializer));
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.SaveNewObjects(ObjectIds, Objects, Serializer);
        }

        internal async Task<Guid> SaveNewObjectLocked(object Object, ObjectSerializer Serializer)
        {
            BinarySerializer Writer;
            Guid ObjectId;
            byte[] Bin;

            Tuple<Guid, BlockInfo> Rec = await this.PrepareObjectIdForSaveLocked(Object, Serializer);
            ObjectId = Rec.Item1;
            BlockInfo Leaf = Rec.Item2;

            Writer = new BinarySerializer(this.collectionName, this.encoding);
            Serializer.Serialize(Writer, false, false, Object);
            Bin = Writer.GetSerialization();

            if (Bin.Length > this.inlineObjectSizeLimit)
                Bin = await this.SaveBlobLocked(Bin);

            await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true, Leaf.LastObject);

            return ObjectId;
        }

        internal void QueueForSave(object Object, ObjectSerializer Serializer)
        {
            lock (this.synchObject)
            {
                if (this.objectsToSave is null)
                    this.objectsToSave = new LinkedList<KeyValuePair<object, ObjectSerializer>>();

                this.objectsToSave.AddLast(new KeyValuePair<object, ObjectSerializer>(Object, Serializer));
            }
        }

        internal async Task<Tuple<Guid, BlockInfo>> PrepareObjectIdForSaveLocked(object Object, ObjectSerializer Serializer)
        {
            bool HasObjectId = Serializer.HasObjectId(Object);
            BlockInfo Leaf;
            Guid ObjectId;

            if (HasObjectId)
            {
                ObjectId = await Serializer.GetObjectId(Object, false);
                Leaf = await this.FindLeafNodeLocked(ObjectId);

                if (Leaf is null)
                    throw new FileException("Object with same Object ID already exists.", this.fileName, this.collectionName);
            }
            else
            {
                do
                {
                    ObjectId = CreateDatabaseGUID();
                }
                while ((Leaf = await this.FindLeafNodeLocked(ObjectId)) is null);

                if (!Serializer.TrySetObjectId(Object, ObjectId))
                    throw new NotSupportedException("Unable to set Object ID.");
            }

            return new Tuple<Guid, BlockInfo>(ObjectId, Leaf);
        }

        internal async Task InsertObjectLocked(uint BlockIndex, BlockHeader Header, byte[] Block, byte[] Bin, int InsertAt,
            uint ChildRightLink, uint ChildRightLinkSize, bool IncSize, bool LastObject)
        {
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

                this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);

                if (IncSize)
                {
                    while (BlockIndex != 0)
                    {
                        BlockIndex = Header.ParentBlockIndex;

                        long PhysicalPosition = BlockIndex;
                        PhysicalPosition *= this.blockSize;

                        Block = await this.LoadBlockLocked(PhysicalPosition, true);
                        BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
                        Header = new BlockHeader(Reader);

                        if (Header.SizeSubtree >= uint.MaxValue)
                            break;

                        Header.SizeSubtree++;

                        Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);
                        this.QueueSaveBlockLocked(PhysicalPosition, Block);
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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

                this.QueueSaveBlockLocked(((long)LeftLink) * this.blockSize, Splitter.LeftBlock);
                this.QueueSaveBlockLocked(((long)RightLink) * this.blockSize, Splitter.RightBlock);

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
                    long PhysicalPosition = ParentLink;
                    PhysicalPosition *= this.blockSize;

                    byte[] ParentBlock = await this.LoadBlockLocked(PhysicalPosition, true);
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

                            ParentLen = this.recordHandler.GetPayloadSize(ParentReader);
                            ParentReader.Position += ParentLen;
                        }
                        while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 4);

                        if (ParentBlockIndex != LeftLink)
                        {
                            this.isCorrupt = true;

                            throw new FileException("Parent link points to parent block (" + ParentLink.ToString() +
                                ") with no reference to child block (" + LeftLink.ToString() + ").", this.fileName, this.collectionName);
                        }
                    }

                    await InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
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

                Len = this.recordHandler.GetPayloadSize(Reader);

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
            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);

            return BitConverter.ToUInt32(Block, 2);
        }

        private async Task CheckChildParentLinkLocked(uint ChildLink, uint NewParentLink)
        {
            long PhysicalPosition = ((long)ChildLink) * this.blockSize;
            byte[] ChildBlock = await this.LoadBlockLocked(PhysicalPosition, true);

            uint ChildParentLink = BitConverter.ToUInt32(ChildBlock, 10);
            if (ChildParentLink != NewParentLink)
            {
                Array.Copy(BitConverter.GetBytes(NewParentLink), 0, ChildBlock, 10, 4);
                this.QueueSaveBlockLocked(PhysicalPosition, ChildBlock);
            }
        }

        internal async Task<BlockInfo> FindLeafNodeLocked(object ObjectId)
        {
            uint BlockIndex = 0;
            bool LastObject = true;

            while (true)
            {
                long PhysicalPosition = BlockIndex;
                PhysicalPosition *= this.blockSize;

                byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
                    Reader.Position += Len;
                }
                while (Comparison > 0 && Reader.BytesLeft >= 4);

                if (Comparison == 0)                                       // Object ID already found.
                    return null;
                else if (IsEmpty || Comparison > 0)
                {
                    if (Header.LastBlockIndex == 0)
                        return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), IsEmpty ? Pos : Reader.Position, LastObject);
                    else
                        BlockIndex = Header.LastBlockIndex;
                }
                else
                {
                    LastObject = false;
                    if (BlockLink == 0)
                        return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos, false);
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
            return (T)await this.LoadObject(ObjectId, this.provider.GetObjectSerializer(typeof(T)));
        }

        /// <summary>
        /// Loads an object from the file.
        /// </summary>
        /// <param name="ObjectId">ID of object to load.</param>
        /// <param name="Type">Type of object to load.</param>
        public async Task<T> LoadObject<T>(Guid ObjectId, Type Type)
        {
            return (T)await this.LoadObject(ObjectId, this.provider.GetObjectSerializer(Type));
        }

        /// <summary>
        /// Loads an object from the file.
        /// </summary>
        /// <param name="ObjectId">ID of object to load.</param>
        /// <param name="Serializer">Object serializer. If not provided, the serializer will be deduced from information stored in the file.</param>
        public async Task<object> LoadObject(Guid ObjectId, IObjectSerializer Serializer)
        {
            await this.LockRead();
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
            await this.LockRead();
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
                    this.objectsToLoad = new LinkedList<Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter>>();

                this.objectsToLoad.AddLast(new Tuple<Guid, ObjectSerializer, EmbeddedObjectSetter>(ObjectId, Serializer, Setter));
            }
        }

        private async Task<object> ParseObjectLocked(BlockInfo Info, IObjectSerializer Serializer)
        {
            int Pos = Info.InternalPosition + 4;
            BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, this.blockLimit, Pos);

            this.recordHandler.SkipKey(Reader);
            this.recordHandler.GetPayloadSize(Reader, out bool IsBlob);

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
                        Serializer = this.provider.GetObjectSerializer(T);
                    else
                        Serializer = this.genericSerializer;
                }
            }

            Reader.Position = Pos;

            return Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
        }

        internal async Task<BlockInfo> FindNodeLocked(object ObjectId)
        {
            uint BlockIndex = 0;

            if (ObjectId is null || (ObjectId is Guid && ObjectId.Equals(Guid.Empty)))
                return null;

            while (true)
            {
                long PhysicalPosition = BlockIndex;
                PhysicalPosition *= this.blockSize;

                byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
                    Reader.Position += Len;
                }
                while (Comparison > 0 && Reader.BytesLeft >= 4);

                if (Comparison == 0)                                       // Object ID found.
                    return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos, false);
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
        /// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public Task UpdateObject(object Object)
        {
            Type ObjectType = Object.GetType();
            ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(ObjectType);
            return this.UpdateObject(Object, Serializer);
        }

        /// <summary>
        /// Updates an object in the database.
        /// </summary>
        /// <param name="Object">Object to update.</param>
        /// <param name="Serializer">Object serializer to use.</param>
        /// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task UpdateObject(object Object, ObjectSerializer Serializer)
        {
            Guid ObjectId = await Serializer.GetObjectId(Object, false);
            await this.UpdateObject(ObjectId, Object, Serializer);
        }

        /// <summary>
        /// Updates an object in the database.
        /// </summary>
        /// <param name="ObjectId">Object ID of object to update.</param>
        /// <param name="Object">Object to update.</param>
        /// <param name="Serializer">Object serializer to use.</param>
        /// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task UpdateObject(Guid ObjectId, object Object, IObjectSerializer Serializer)
        {
            object Old;

            await this.LockWrite();
            try
            {
                BlockInfo Info = await this.FindNodeLocked(ObjectId);
                if (Info is null)
                    throw new KeyNotFoundException("Object not found.");

                Old = await this.ParseObjectLocked(Info, Serializer);

                BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
                Serializer.Serialize(Writer, false, false, Object);
                byte[] Bin = Writer.GetSerialization();

                await this.ReplaceObjectLocked(Bin, Info, true);
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.UpdateObject(ObjectId, Old, Object, Serializer);
        }

        /// <summary>
        /// Updates a set of objects in the database.
        /// </summary>
        /// <param name="ObjectIds">Object IDs of objects to update.</param>
        /// <param name="Objects">Objects to update.</param>
        /// <param name="Serializer">Object serializer to use.</param>
        /// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task UpdateObjects(IEnumerable<Guid> ObjectIds, IEnumerable<object> Objects, IObjectSerializer Serializer)
        {
            LinkedList<object> Olds = new LinkedList<object>();
            object Old;

            await this.LockWrite();
            try
            {
                IEnumerator<Guid> e1 = ObjectIds.GetEnumerator();
                IEnumerator<object> e2 = Objects.GetEnumerator();

                while (e1.MoveNext() && e2.MoveNext())
                {
                    BlockInfo Info = await this.FindNodeLocked(e1.Current);
                    if (Info is null)
                        throw new KeyNotFoundException("Object not found.");

                    Old = await this.ParseObjectLocked(Info, Serializer);
                    Olds.AddLast(Old);

                    BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
                    Serializer.Serialize(Writer, false, false, e2.Current);
                    byte[] Bin = Writer.GetSerialization();

                    await this.ReplaceObjectLocked(Bin, Info, true);
                }
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.UpdateObjects(ObjectIds, Olds, Objects, Serializer);
        }

        internal async Task ReplaceObjectLocked(byte[] Bin, BlockInfo Info, bool DeleteBlob)
        {
            byte[] Block = Info.Block;
            BlockHeader Header = Info.Header;
            BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit, Info.InternalPosition + 4);
            this.recordHandler.SkipKey(Reader);

            uint BlockIndex = Info.BlockIndex;
            int Len = this.recordHandler.GetPayloadSize(Reader, out bool IsBlob);

            if (IsBlob && DeleteBlob)
                await this.DeleteBlobLocked(Block, Info.InternalPosition + 4);

            if (DeleteBlob && Bin.Length > this.inlineObjectSizeLimit)
                Bin = await this.SaveBlobLocked(Bin);

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

                this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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

                this.QueueSaveBlockLocked(((long)LeftLink) * this.blockSize, Splitter.LeftBlock);
                this.QueueSaveBlockLocked(((long)RightLink) * this.blockSize, Splitter.RightBlock);

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
                    long PhysicalPosition = ParentLink;
                    PhysicalPosition *= this.blockSize;

                    byte[] ParentBlock = await this.LoadBlockLocked(PhysicalPosition, true);
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

                            ParentLen = this.recordHandler.GetPayloadSize(ParentReader);
                            ParentReader.Position += ParentLen;
                        }
                        while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 4);

                        if (ParentBlockIndex != LeftLink)
                        {
                            this.isCorrupt = true;

                            throw new FileException("Parent link points to parent block (" + ParentLink.ToString() +
                                ") with no reference to child block (" + LeftLink.ToString() + ").", this.fileName, this.collectionName);
                        }
                    }

                    await InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
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
        /// <returns>Object that was deleted.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public Task<object> DeleteObject(object Object)
        {
            Type ObjectType = Object.GetType();
            ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(ObjectType);

            return this.DeleteObject(Object, Serializer);
        }

        /// <summary>
        /// Deletes an object from the database, using the object serializer corresponding to the type of object being updated, to find
        /// the Object ID of the object.
        /// </summary>
        /// <param name="Object">Object to delete.</param>
        /// <param name="Serializer">Object serializer to use.</param>
        /// <returns>Object that was deleted.</returns>
        /// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
        /// or if the corresponding property type is not supported.</exception>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task<object> DeleteObject(object Object, ObjectSerializer Serializer)
        {
            Guid ObjectId = await Serializer.GetObjectId(Object, false);
            return await this.DeleteObject(ObjectId, (IObjectSerializer)Serializer);
        }

        /// <summary>
        /// Deletes an object from the database.
        /// </summary>
        /// <param name="ObjectId">Object ID of the object to delete.</param>
        /// <returns>Object that was deleted.</returns>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public Task<object> DeleteObject(Guid ObjectId)
        {
            return this.DeleteObject(ObjectId, (IObjectSerializer)this.genericSerializer);
        }

        /// <summary>
        /// Deletes an object from the database.
        /// </summary>
        /// <param name="ObjectId">Object ID of the object to delete.</param>
        /// <param name="Serializer">Binary serializer.</param>
        /// <returns>Object that was deleted.</returns>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task<object> DeleteObject(Guid ObjectId, IObjectSerializer Serializer)
        {
            object DeletedObject;

            await this.LockWrite();
            try
            {
                DeletedObject = await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null);
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.DeleteObject(ObjectId, DeletedObject, Serializer);

            return DeletedObject;
        }

        /// <summary>
        /// Deletes a set of objects from the database.
        /// </summary>
        /// <param name="ObjectIds">Object IDs of the objects to delete.</param>
        /// <param name="Serializer">Binary serializer.</param>
        /// <returns>Object that was deleted.</returns>
        /// <exception cref="KeyNotFoundException">If the object is not found in the database.</exception>
        public async Task<IEnumerable<object>> DeleteObjects(IEnumerable<Guid> ObjectIds, IObjectSerializer Serializer)
        {
            LinkedList<object> DeletedObjects = new LinkedList<object>();

            await this.LockWrite();
            try
            {
                foreach (Guid ObjectId in ObjectIds)
                    DeletedObjects.AddLast(await this.DeleteObjectLocked(ObjectId, false, true, Serializer, null));
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.DeleteObjects(ObjectIds, DeletedObjects, Serializer);

            return DeletedObjects;
        }

        internal async Task<object> DeleteObjectLocked(object ObjectId, bool MergeNodes, bool DeleteAnyBlob,
            IObjectSerializer Serializer, object OldObject)
        {
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
            Len = this.recordHandler.GetPayloadSize(Reader, out bool IsBlob);
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
                            Block = await this.LoadBlockLocked(((long)Header.LastBlockIndex) * this.blockSize, true);
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

                        this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
                        await this.DecreaseSizeLocked(Header.ParentBlockIndex);

                        if (BlockIndex != 0)
                            await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);
                        else
                            await this.RebalanceEmptyBlockLocked(BlockIndex, Block, Header);
                    }
                }
                else
                {
                    this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);

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

                            return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject);  // This time, the object will be lower in the tree.
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

                    this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
                    this.QueueSaveBlockLocked(((long)LeftBlockLink) * this.blockSize, MergeResult.ResultBlock);

                    await this.UpdateParentLinksLocked(LeftBlockLink, MergeResult.ResultBlock);

                    if (Header.BytesUsed == 0 && BlockIndex != 0)
                        await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);

                    if (!(MergeResult.Separator is null))
                        await this.ReinsertMergeOverflow(MergeResult, BlockIndex);

                    return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject);  // This time, the object will be lower in the tree.
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
                                            return await this.DeleteObjectLocked(ObjectId, true, false, Serializer, OldObject);
                                    }
                                }
                            }
                        }
                    }

                    long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
                    Info.Block = await this.LoadBlockLocked(PhysicalPosition, true);    // Refresh object count.

                    if (Reshuffled)
                    {
                        Reader.Restart(Info.Block, 0);
                        Info.Header = new BlockHeader(Reader);

                        if (this.ForEachObject(Info.Block, (Link, ObjectId2, Pos, Len2) =>
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
                            return await this.DeleteObjectLocked(ObjectId, false, false, Serializer, OldObject);
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

            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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
            this.QueueSaveBlockLocked(PhysicalPosition, Block);

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
                        Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, true);
                        Links.RemoveFirst();
                        this.RegisterEmptyBlockLocked(BlockIndex);
                    }
                }
            }
        }

        private async Task RebalanceEmptyBlockLocked(uint BlockIndex, byte[] Block, BlockHeader Header)
        {
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
                    this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
                else
                {
                    Tuple<uint, byte[]> NewChild = await this.CreateNewBlockLocked();
                    byte[] NewChildBlock = NewChild.Item2;
                    uint NewChildBlockIndex = NewChild.Item1;

                    Array.Copy(BitConverter.GetBytes(NewChildBlockIndex), 0, Block, BlockHeaderSize, 4);

                    this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);

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

                    this.QueueSaveBlockLocked(((long)NewChildBlockIndex) * this.blockSize, NewChildBlock);

                    if (!(Object2 is null))
                        await this.IncreaseSizeLocked(BlockIndex);
                }

                await this.IncreaseSizeLocked(BitConverter.ToUInt32(Block, 10));    // Note that Header.ParentBlockIndex might no longer be correct.
            }
        }

        private async Task MergeEmptyBlockWithSiblingLocked(uint ChildBlockIndex, uint ParentBlockIndex)
        {
            byte[] ParentBlock = await this.LoadBlockLocked(((long)ParentBlockIndex) * this.blockSize, true);
            BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock, this.blockLimit);
            BlockHeader ParentHeader = new BlockHeader(ParentReader);
            int LastPos = 0;
            int LastLen = 0;
            uint LastLink = 0;
            int i, c;

            if (this.ForEachObject(ParentBlock, (Link, ObjectId2, Pos, Len2) =>
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

                    this.QueueSaveBlockLocked(((long)LastLink) * this.blockSize, MergeResult.ResultBlock);
                    this.QueueSaveBlockLocked(((long)ParentBlockIndex) * this.blockSize, ParentBlock);

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

                this.QueueSaveBlockLocked(((long)ParentBlockIndex) * this.blockSize, ParentBlock);
                this.QueueSaveBlockLocked(((long)ChildBlockIndex) * this.blockSize, MergeResult.ResultBlock);

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
            byte[] LeftBlock = await this.LoadBlockLocked(((long)LeftIndex) * this.blockSize, true);
            byte[] RightBlock = await this.LoadBlockLocked(((long)RightIndex) * this.blockSize, true);

            return await this.MergeBlocksLocked(LeftBlock, Separator, RightBlock);
        }

        private async Task<MergeResult> MergeBlocksLocked(byte[] Left, byte[] Separator, byte[] Right)
        {
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

                Len = this.recordHandler.GetPayloadSize(Reader);
                Reader.Position += Len;

                c = Reader.Position - Pos - 4;

                if (!await Method(Link, ObjectId, Pos + 4, c))
                    return false;
            }
            while (Reader.BytesLeft >= 4);

            return true;
        }

        private delegate bool ForEachDelegate(uint Link, object ObjectId, int Pos, int Len);

        private bool ForEachObject(byte[] Block, ForEachDelegate Method)
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

                Len = this.recordHandler.GetPayloadSize(Reader);
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
            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                        Len = this.recordHandler.GetPayloadSize(Reader);
                        Reader.Position += Len;
                        LastPos = Pos;
                    }
                    while (Reader.BytesLeft >= 4);

                    if (LastPos != 0)
                    {
                        Reader.Position = LastPos;
                        Link = Reader.ReadBlockLink();
                        this.recordHandler.GetKey(Reader);
                        Len = this.recordHandler.GetPayloadSize(Reader);
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

                            this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
                            this.QueueSaveBlockLocked(((long)Link) * this.blockSize, MergeResult.ResultBlock);

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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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

                                this.QueueSaveBlockLocked(PhysicalPosition, Block);

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

                    this.QueueSaveBlockLocked(PhysicalPosition, Block);

                    await this.DecreaseSizeLocked(Header.ParentBlockIndex);
                }
            }

            return Result;
        }

        private async Task<byte[]> TryExtractSmallestObjectLocked(uint BlockIndex, bool AllowRotation, bool AllowMerge)
        {
            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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
                                Len = this.recordHandler.GetPayloadSize(Reader);
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

                                    this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
                                    this.QueueSaveBlockLocked(((long)Link) * this.blockSize, MergeResult.ResultBlock);

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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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

                        this.QueueSaveBlockLocked(PhysicalPosition, Block);
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

                this.QueueSaveBlockLocked(PhysicalPosition, Block);

                await this.DecreaseSizeLocked(Header.ParentBlockIndex);
            }

            return Result;
        }

        private async Task<byte[]> RotateLeftLocked(uint ChildIndex, uint BlockIndex)
        {
            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                Len = this.recordHandler.GetPayloadSize(Reader);
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
                Block = await this.LoadBlockLocked(PhysicalPosition, true); // Refresh
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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
            long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                Len = this.recordHandler.GetPayloadSize(Reader);
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
                Block = await this.LoadBlockLocked(PhysicalPosition, true); // Refresh
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

                    Len = this.recordHandler.GetPayloadSize(Reader);
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
            if (Delta == 0)
                return;

            while (true)
            {
                long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
                byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
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

                this.QueueSaveBlockLocked(PhysicalPosition, Block);

                if (BlockIndex == 0)
                    return;

                BlockIndex = BitConverter.ToUInt32(Block, 10);
            }
        }

        private async Task IncreaseSizeLocked(uint BlockIndex)
        {
            while (true)
            {
                long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
                byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
                uint Size = BitConverter.ToUInt32(Block, 2);

                if (Size < uint.MaxValue)
                {
                    Size++;
                    Array.Copy(BitConverter.GetBytes(Size), 0, Block, 2, 4);
                    this.QueueSaveBlockLocked(PhysicalPosition, Block);

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
        public string GetCurrentStateReport(bool WriteStat, bool Properties)
        {
			Task<string> T = this.GetCurrentStateReportAsync(WriteStat, Properties);
			FilesProvider.Wait(T, this.timeoutMilliseconds);
			return T.Result;
        }

        /// <summary>
        /// Provides a report on the current state of the file.
        /// </summary>
        /// <param name="WriteStat">If statistics is to be included in the report.</param>
        /// <param name="Properties">If object properties should be exported as well, in case the database is corrupt or unbalanced.</param>
        /// <returns>Report</returns>
        public async Task<string> GetCurrentStateReportAsync(bool WriteStat, bool Properties)
        {
            await this.LockWrite();
            try
            {
                return await this.GetCurrentStateReportAsyncLocked(WriteStat, Properties);
            }
            finally
            {
                await this.EndWrite();
            }
        }

        private async Task<string> GetCurrentStateReportAsyncLocked(bool WriteStat, bool Properties)
        {
            StringBuilder Output = new StringBuilder();
            Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
            FileStatistics Statistics = await this.ComputeStatisticsLocked(ObjectIds, null);

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
                await this.ExportGraphXMLLocked(Output, Properties);
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
            await this.LockWrite();
            try
            {
                Dictionary<Guid, bool> ObjectIds = new Dictionary<Guid, bool>();
                FileStatistics Result = await this.ComputeStatisticsLocked(ObjectIds, null);
                return new KeyValuePair<FileStatistics, Dictionary<Guid, bool>>(Result, ObjectIds);
            }
            finally
            {
                await this.EndWrite();
            }
        }

        internal async Task<FileStatistics> ComputeStatisticsLocked(Dictionary<Guid, bool> ObjectIds, Dictionary<Guid, bool> ExistingIds)
        {
            long FileSize = this.file.Length + this.bytesAdded;
            int NrBlocks = (int)(FileSize / this.blockSize);
            this.blockLimit = (uint)NrBlocks;

            long BlobFileSize = this.blobFile is null ? 0 : this.blobFile.Length;
            int NrBlobBlocks = (int)(BlobFileSize / this.blobBlockSize);
            this.blobBlockLimit = (uint)NrBlobBlocks;

            byte[] Block = await this.LoadBlockLocked(0, false);
            BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, this.blockLimit);
            FileStatistics Statistics = new FileStatistics((uint)this.blockSize, this.nrBlockLoads, this.nrCacheLoads, this.nrBlockSaves,
                this.nrBlobBlockLoads, this.nrBlobBlockSaves, this.nrFullFileScans, this.nrSearches);
            BitArray BlocksReferenced = new BitArray(NrBlocks);
            BitArray BlobBlocksReferenced = new BitArray(NrBlobBlocks);
            int i;

            BlockHeader.SkipHeader(Reader);

            try
            {
                await this.AnalyzeBlock(1, 0, 0, Statistics, BlocksReferenced, BlobBlocksReferenced, ObjectIds, ExistingIds, null, null);

                List<int> Blocks = new List<int>();

                for (i = 0; i < NrBlocks; i++)
                {
                    if (!BlocksReferenced[i])
                    {
                        Statistics.LogError("Block " + i.ToString() + " is not referenced.");
                        Blocks.Add(i);
                    }
                }

                Statistics.UnreferencedBlocks = Blocks.ToArray();
                Blocks.Clear();

                for (i = 0; i < NrBlobBlocks; i++)
                {
                    if (!BlobBlocksReferenced[i])
                    {
                        Statistics.LogError("BLOB Block " + i.ToString() + " is not referenced.");
                    }
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

                BlocksReferenced[(int)BlockIndex] = true;
            }

            long PhysicalPosition = BlockIndex;
            PhysicalPosition *= this.blockSize;

            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, false);
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

                Len = this.recordHandler.GetFullPayloadSize(Reader);
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
                            object Obj = this.genericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false, false);
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
        public async Task<string> ExportGraphXML(bool Properties)
        {
            StringBuilder Output = new StringBuilder();
            await this.ExportGraphXML(Output, Properties);
            return Output.ToString();
        }

        /// <summary>
        /// Exports the structure of the file to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        /// <param name="Properties">If object properties should be exported as well.</param>
        /// <returns>Asynchronous task object.</returns>
        public async Task ExportGraphXML(StringBuilder Output, bool Properties)
        {
            await this.LockWrite();
            try
            {
                await this.ExportGraphXMLLocked(Output, Properties);
            }
            finally
            {
                await this.EndWrite();
            }
        }

        /// <summary>
        /// Exports the structure of the file to XML.
        /// </summary>
        /// <param name="Properties">If object properties should be exported as well.</param>
        /// <returns>Graph XML.</returns>
        public async Task<string> ExportGraphXMLLocked(bool Properties)
        {
            StringBuilder Output = new StringBuilder();
            await this.ExportGraphXMLLocked(Output, Properties);
            return Output.ToString();
        }

        /// <summary>
        /// Exports the structure of the file to XML.
        /// </summary>
        /// <param name="Output">XML Output</param>
        /// <param name="Properties">If object properties should be exported as well.</param>
        /// <returns>Asynchronous task object.</returns>
        public async Task ExportGraphXMLLocked(StringBuilder Output, bool Properties)
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
                await this.ExportGraphXMLLocked(w, Properties);
                w.Flush();
            }
        }

        /// <summary>
        /// Exports the structure of the file to XML.
        /// </summary>
        /// <param name="XmlOutput">XML Output</param>
        /// <param name="Properties">If object properties should be exported as well.</param>
        /// <returns>Asynchronous task object.</returns>
        public async Task ExportGraphXML(XmlWriter XmlOutput, bool Properties)
        {
            await this.LockWrite();
            try
            {
                await this.ExportGraphXMLLocked(XmlOutput, Properties);
            }
            finally
            {
                await this.EndWrite();
            }
        }

        /// <summary>
        /// Exports the structure of the file to XML.
        /// </summary>
        /// <param name="XmlOutput">XML Output</param>
        /// <param name="Properties">If object properties should be exported as well.</param>
        /// <returns>Asynchronous task object.</returns>
        public async Task ExportGraphXMLLocked(XmlWriter XmlOutput, bool Properties)
        {
            BinaryDeserializer Reader = null;
            long NrBlocks = (this.file.Length + this.bytesAdded) / this.blockSize;
            byte[] BlobBlock = new byte[this.blobBlockSize];
            byte[] DecryptedBlock;
            long PhysicalPosition;
            uint Link;
            int NrRead;

            this.blockLimit = (uint)NrBlocks;

            XmlOutput.WriteStartElement("Collection", "http://waher.se/Schema/Persistence/Files.xsd");
            XmlOutput.WriteAttributeString("name", this.collectionName);

            XmlOutput.WriteStartElement("BTreeFile");
            XmlOutput.WriteAttributeString("fileName", this.fileName);
            await this.ExportGraphXMLLocked(0, XmlOutput, Properties);
            XmlOutput.WriteEndElement();

            foreach (IndexBTreeFile Index in this.indices)
            {
                XmlOutput.WriteStartElement("IndexFile");
                XmlOutput.WriteAttributeString("fileName", Index.IndexFile.fileName);

                await Index.IndexFile.ExportGraphXMLLocked(XmlOutput, false);

                XmlOutput.WriteEndElement();
            }

            if (!(this.blobFile is null))
            {
                XmlOutput.WriteStartElement("BlobFile");
                XmlOutput.WriteAttributeString("fileName", this.blobFileName);

                this.blobFile.Position = 0;
                while ((PhysicalPosition = this.blobFile.Position) < this.blobFile.Length)
                {
                    NrRead = await this.blobFile.ReadAsync(BlobBlock, 0, this.blobBlockSize);
                    if (NrRead != this.blobBlockSize)
						throw new FileException("Read past end of file " + this.fileName + ".", this.fileName, this.collectionName);

					this.nrBlobBlockLoads++;

                    if (this.encrypted)
                    {
                        using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
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
                    XmlOutput.WriteAttributeString("index", (PhysicalPosition / this.blobBlockSize).ToString());
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

            XmlOutput.WriteEndElement();
        }

        private async Task ExportGraphXMLLocked(uint BlockIndex, XmlWriter XmlOutput, bool Properties)
        {
            long PhysicalPosition = BlockIndex;
            PhysicalPosition *= this.blockSize;

            byte[] Block = await this.LoadBlockLocked(PhysicalPosition, false);
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

                Len = this.recordHandler.GetFullPayloadSize(Reader);
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
                    Obj = (GenericObject)this.genericSerializer.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);
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
            await this.LockRead();
            try
            {
                return await this.GetObjectCountLocked(BlockIndex, IncludeChildren);
            }
            finally
            {
                await this.EndRead();
            }
        }

        private async Task<ulong> GetObjectCountLocked(uint BlockIndex, bool IncludeChildren)
        {
            byte[] Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, false);
            uint BlockSize;

            if (!IncludeChildren)
            {
                BlockSize = 0;

                this.ForEachObject(Block, (Link, ObjectId2, Pos2, Len2) =>
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

                Len = (uint)this.recordHandler.GetPayloadSize(Reader);
                Reader.Position += (int)Len;
            }

            if (IncludeChildren && Header.LastBlockIndex != 0)
                NrObjects += await this.GetObjectCountLocked(Header.LastBlockIndex, IncludeChildren);

            return NrObjects;
        }

        private async Task<uint> GetObjectCount32Locked(uint BlockIndex)
        {
            byte[] Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, false);
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
            await this.LockRead();
            try
            {
                return await GetRankLocked(ObjectId);
            }
            finally
            {
                await this.EndRead();
            }
        }

        internal async Task<ulong> GetRankLocked(object ObjectId)
        {
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

                Len = this.recordHandler.GetPayloadSize(Reader);

                if (this.recordHandler.Compare(ObjectId2, ObjectId) == 0)
                {
                    BlockHeader Header = Info.Header;
                    uint BlockIndex = Info.BlockIndex;

                    while (BlockIndex != 0)
                    {
                        uint ParentIndex = Header.ParentBlockIndex;
                        byte[] Block = await this.LoadBlockLocked(((long)ParentIndex) * this.blockSize, true);
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

                            Len = this.recordHandler.GetPayloadSize(Reader);

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
        /// <see cref="ICollection{Object}.Add(Object)"/>
        /// </summary>
        public void Add(object item)
        {
            this.SaveNewObject(item);
        }

        /// <summary>
        /// <see cref="ICollection{Object}.Contains(Object)"/>
        /// </summary>
        public bool Contains(object item)
        {
            Task<bool> Task = this.ContainsAsync(item);
            FilesProvider.Wait(Task, this.timeoutMilliseconds);
            return Task.Result;
        }

        /// <summary>
        /// Checks if an item is stored in the file.
        /// </summary>
        /// <param name="Item">Object to check for.</param>
        /// <returns>If the object is stored in the file.</returns>
        public async Task<bool> ContainsAsync(object Item)
        {
            if (Item is null)
                return false;

            if (!(this.provider.GetObjectSerializer(Item.GetType()) is ObjectSerializer Serializer))
                return false;

            if (!Serializer.HasObjectId(Item))
                return false;

            Guid ObjectId = await Serializer.GetObjectId(Item, false);
            GenericObject Obj;

            await this.LockRead();
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
                Serializer.Serialize(Writer, false, false, Item);
                byte[] Bin = Writer.GetSerialization();

                BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin, this.blockLimit);
                if (!(this.genericSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false) is GenericObject Obj2))
                    return false;

                return Obj.Equals(Obj2);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="ICollection{Object}.CopyTo(Object[], int)"/>
        /// </summary>
        public void CopyTo(object[] array, int arrayIndex)
        {
            foreach (Object Item in this)
                array[arrayIndex++] = Item;
        }

        /// <summary>
        /// <see cref="ICollection{Object}.Count"/>
        /// </summary>
        public int Count
        {
            get
            {
                Task<ulong> Task = this.GetObjectCount(0, true);
                FilesProvider.Wait(Task, this.timeoutMilliseconds);

                ulong Result = Task.Result;
                if (Result > int.MaxValue)
                    return int.MaxValue;
                else
                    return (int)Result;
            }
        }

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
        /// <see cref="ICollection{Object}.Clear"/>
        /// </summary>
        public void Clear()
        {
            FilesProvider.Wait(this.ClearAsync(), this.timeoutMilliseconds);
        }

        /// <summary>
        /// Clears the database of all objects.
        /// </summary>
        /// <returns>Task object.</returns>
        public async Task ClearAsync()
        {
            await this.LockWrite();
            try
            {
                this.file.Dispose();
                this.file = null;

                File.Delete(this.fileName);
                this.file = File.Open(this.fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                if (!(this.blobFile is null))
                {
                    this.blobFile.Dispose();
                    this.blobFile = null;

                    File.Delete(this.blobFileName);

                    this.blobFile = File.Open(this.blobFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                }

                this.provider.RemoveBlocks(this.id);

                this.emptyBlocks?.Clear();
                this.blocksToSave?.Clear();
                this.objectsToSave?.Clear();
                this.objectsToLoad?.Clear();
                this.bytesAdded = 0;
                this.blockLimit = 0;
                this.blobBlockLimit = 0;

                await this.CreateNewBlockLocked();
            }
            finally
            {
                await this.EndWrite();
            }

            foreach (IndexBTreeFile Index in this.indices)
                await Index.ClearAsync();
        }

        /// <summary>
        /// Returns an untyped enumerator that iterates through the collection.
        /// 
        /// For a typed enumerator, call the <see cref="GetTypedEnumeratorAsync{T}(bool)"/> method.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<object> GetEnumerator()
        {
            return new ObjectBTreeFileEnumerator<object>(this, this.recordHandler, null);
        }

        /// <summary>
        /// Returns an untyped enumerator that iterates through the collection.
        /// 
        /// For a typed enumerator, call the <see cref="GetTypedEnumeratorAsync{T}(bool)"/> method.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ObjectBTreeFileEnumerator<object>(this, this.recordHandler, null);
        }

        /// <summary>
        /// Returns an typed enumerator that iterates through the collection. The typed enumerator uses
        /// the object serializer of <typeparamref name="T"/> to deserialize objects by default.
        /// </summary>
        /// <param name="Locked">If locked access to the file is requested.
        /// 
        /// If unlocked access is desired, any change to the database will invalidate the enumerator, and further access to the
        /// enumerator will cause an <see cref="InvalidOperationException"/> to be thrown.
        /// 
        /// If locked access is desired, the database cannot be updated, until the enumerator has been dispose. Make sure to call
        /// the <see cref="ObjectBTreeFileEnumerator{T}.Dispose"/> method when done with the enumerator, to release the database
        /// after use.</param>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public async Task<ObjectBTreeFileEnumerator<T>> GetTypedEnumeratorAsync<T>(bool Locked)
        {
            ObjectBTreeFileEnumerator<T> e = new ObjectBTreeFileEnumerator<T>(this, this.recordHandler, null);
            if (Locked)
                await e.LockRead();

            return e;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the System.Collections.Generic.ICollection{T}.
        /// </summary>
        /// <param name="item">The object to remove from the System.Collections.Generic.ICollection{T}.</param>
        /// <returns>true if item was successfully removed from the System.Collections.Generic.ICollection{T}; 
        /// otherwise, false. This method also returns false if item is not found in the original 
        /// System.Collections.Generic.ICollection{T}.</returns>
        public bool Remove(object item)
        {
            try
            {
				Task<object> T = this.DeleteObject(item);
				FilesProvider.Wait(T, this.timeoutMilliseconds);
				return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Indices

        /// <summary>
        /// Adds an index to the file. When objects are added, updated or deleted from the file, the corresponding references in the
        /// index file will be updated as well. The index files will be disposed together with the main file as well.
        /// </summary>
        /// <param name="Index">Index file to add.</param>
        /// <param name="Regenerate">If the index is to be regenerated.</param>
        public async Task AddIndex(IndexBTreeFile Index, bool Regenerate)
        {
            lock (this.synchObject)
            {
                this.indexList.Add(Index);
                this.indices = this.indexList.ToArray();
            }

            if (Regenerate)
                await Index.Regenerate();
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
                return this.FindBestIndex(out BestNrFields, Property);

            s = SortOrder[0];
            if (s.StartsWith("-"))
                s = s.Substring(1);

            if (Property.StartsWith("-"))
                s2 = Property.Substring(1);
            else
                s2 = Property;

            if (s2 == s)
                return this.FindBestIndex(out BestNrFields, SortOrder);

            string[] Properties = new string[SortOrder.Length + 1];

            Properties[0] = Property;
            Array.Copy(SortOrder, 0, Properties, 1, SortOrder.Length);

            return this.FindBestIndex(out BestNrFields, Properties);
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

            if (SortOrder is null || SortOrder.Length == 0)
                return this.FindBestIndex(out BestNrFields, Properties);

            if (Properties is null || Properties.Length == 0)
                return this.FindBestIndex(out BestNrFields, SortOrder);

            List<string> Order = new List<string>();
            Dictionary<string, bool> Added = new Dictionary<string, bool>();

            Order.AddRange(SortOrder);

            foreach (string s in SortOrder)
            {
                if (s.StartsWith("-"))
                    s2 = s.Substring(1);
                else
                    s2 = s;

                Added[s2] = true;
            }

            foreach (string s in Properties)
            {
                if (s.StartsWith("-"))
                    s2 = s.Substring(1);
                else
                    s2 = s;

                if (!Added.ContainsKey(s2))
                {
                    Added[s2] = true;
                    Order.Insert(0, s);
                }
            }

            return this.FindBestIndex(out BestNrFields, Order.ToArray());
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
            return this.FindBestIndex(out int _, Properties);
        }

        /// <summary>
        /// Finds the best index for finding objects using  a given set of properties. The method assumes the most restrictive
        /// property is mentioned first in <paramref name="Properties"/>.
        /// </summary>
        /// <param name="BestNrFields">Number of index fields used in best index.</param>
        /// <param name="Properties">Properties to search on. By default, sort order is ascending.
        /// If descending sort order is desired, prefix the corresponding field name by a hyphen (minus) sign.</param>
        /// <returns>Best index to use for the search. If no index is found matching the properties, null is returned.</returns>
        internal IndexBTreeFile FindBestIndex(out int BestNrFields, params string[] Properties)
        {
            Dictionary<string, int> PropertyOrder = new Dictionary<string, int>();
            IndexBTreeFile Best = null;
            int i, c = Properties.Length;
            int MinOrdinal, NrFields;
            int BestMinOrdinal = int.MaxValue;
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
                foreach (string FieldName in Index.FieldNames)
                {
                    if (!PropertyOrder.TryGetValue(FieldName, out int PropertyOrdinal))
                        break;

                    NrFields++;

                    if (PropertyOrdinal < MinOrdinal)
                        MinOrdinal = PropertyOrdinal;
                }

                if (NrFields == 0)
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
        /// Finds objects of a given class <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
        /// <param name="Offset">Result offset.</param>
        /// <param name="MaxCount">Maximum number of objects to return.</param>
        /// <param name="Filter">Optional filter. Can be null.</param>
        /// <param name="Locked">If locked access to the file is requested.
        /// 
        /// If unlocked access is desired, any change to the database will invalidate the enumerator, and further access to the
        /// enumerator will cause an <see cref="InvalidOperationException"/> to be thrown.
        /// 
        /// If locked access is desired, the database cannot be updated, until the enumerator has been disposed. Make sure to call
        /// the <see cref="IDisposable.Dispose"/> method when done with the enumerator, to release the database
        /// after use.</param>
        /// <param name="SortOrder">Sort order. Each string represents a field name. By default, sort order is ascending.
        /// If descending sort order is desired, prefix the field name by a hyphen (minus) sign.</param>
        /// <returns>Objects found.</returns>
        public async Task<ICursor<T>> Find<T>(int Offset, int MaxCount, Filter Filter, bool Locked, params string[] SortOrder)
        {
            this.nrSearches++;

            if (!this.indicesCreated)
            {
                ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(typeof(T));
                string CollectionName = Serializer.CollectionName(null);
                ObjectBTreeFile File = await this.provider.GetFile(CollectionName);
                if (File != this)
                {
                    throw new ArgumentException("Objects of type " + typeof(T).FullName + " are stored in collection " + CollectionName +
                        ",  not " + this.collectionName + ".", nameof(T));
                }

                foreach (string[] Index in Serializer.Indices)
                    await this.provider.GetIndexFile(File, RegenerationOptions.RegenerateIfIndexNotInstantiated, Index);

                this.indicesCreated = true;
            }

            ICursor<T> Result = null;

            try
            {
                if (Filter is null)
                {
                    Result = null;

                    if (!(SortOrder is null) && SortOrder.Length > 0)
                    {
                        IndexBTreeFile Index = this.FindBestIndex(SortOrder);

                        if (!(Index is null))
                        {
                            if (Index.SameSortOrder(null, SortOrder))
                                Result = await Index.GetTypedEnumerator<T>(Locked);
                            else if (Index.ReverseSortOrder(null, SortOrder))
                                Result = new Searching.ReversedCursor<T>(await Index.GetTypedEnumerator<T>(Locked), this.timeoutMilliseconds);
                        }
                    }

                    if (Result is null)
                    {
                        this.nrFullFileScans++;
                        Result = await this.GetTypedEnumeratorAsync<T>(Locked);

                        if (!(SortOrder is null) && SortOrder.Length > 0)
                            Result = await this.Sort<T>(Result, this.ConvertFilter(Filter)?.ConstantFields, SortOrder, true);
                    }

                    if (Offset > 0 || MaxCount < int.MaxValue)
                        Result = new Searching.PagesCursor<T>(Offset, MaxCount, Result, this.timeoutMilliseconds);
                }
                else
                {
                    Result = await this.ConvertFilterToCursor<T>(Filter.Normalize(), Locked, SortOrder);

                    if (!(SortOrder is null) && SortOrder.Length > 0)
                        Result = await this.Sort<T>(Result, this.ConvertFilter(Filter)?.ConstantFields, SortOrder, true);   // false);

                    if (Offset > 0 || MaxCount < int.MaxValue)
                        Result = new Searching.PagesCursor<T>(Offset, MaxCount, Result, this.timeoutMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Result?.Dispose();

                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return Result;
        }

        private async Task<ICursor<T>> Sort<T>(ICursor<T> Result, string[] ConstantFields, string[] SortOrder, bool CanReverse)
        {
            if (Result.SameSortOrder(ConstantFields, SortOrder))
                return Result;
            else if (CanReverse && Result.ReverseSortOrder(ConstantFields, SortOrder))
                return new Searching.ReversedCursor<T>(Result, this.timeoutMilliseconds);

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

            Log.Notice("Sort order in search result did not match index.", this.fileName, string.Empty, "DBOpt",
                new KeyValuePair<string, object>("Collection", this.collectionName),
                new KeyValuePair<string, object>("SortOrder", sb.ToString()));

            SortedDictionary<Searching.SortedReference<T>, bool> SortedObjects;
            IndexRecords Records;
            byte[] Key;

            try
            {
                Records = new IndexRecords(this.collectionName, this.encoding, int.MaxValue, SortOrder);
                SortedObjects = new SortedDictionary<Searching.SortedReference<T>, bool>();

                while (await Result.MoveNextAsync())
                {
                    if (!Result.CurrentTypeCompatible)
                        continue;

                    Key = Records.Serialize(Result.CurrentObjectId, Result.Current, Result.CurrentSerializer, MissingFieldAction.Null);
                    SortedObjects[new Searching.SortedReference<T>(Key, Records, Result.Current, Result.CurrentSerializer, Result.CurrentObjectId)] = true;
                }
            }
            finally
            {
                Result.Dispose();
            }

            return new Searching.SortedCursor<T>(SortedObjects, Records, this.timeoutMilliseconds);
        }

        internal async Task<ICursor<T>> ConvertFilterToCursor<T>(Filter Filter, bool Locked, string[] SortOrder)
        {
            if (Filter is FilterChildren FilterChildren)
            {
                Filter[] ChildFilters = FilterChildren.ChildFilters;

                if (Filter is FilterAnd)
                {
                    List<string> Properties = null;
                    LinkedList<KeyValuePair<Searching.FilterFieldLikeRegEx, string>> RegExFields = null;
                    string FieldName;

                    foreach (Filter Filter2 in ChildFilters)
                    {
                        if (Filter2 is FilterFieldValue)
                        {
                            if (!(Filter2 is FilterFieldNotEqualTo))
                            {
                                if (Properties is null)
                                    Properties = new List<string>();

                                FieldName = ((FilterFieldValue)Filter2).FieldName;
                                if (!Properties.Contains(FieldName))
                                    Properties.Add(FieldName);
                            }
                        }
                        else if (Filter2 is FilterFieldLikeRegEx FilterFieldLikeRegEx)
                        {
                            Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter2);
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
                        this.nrFullFileScans++;
                        Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                        return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked),
                            this.ConvertFilter(Filter), false, true, this.timeoutMilliseconds, this.provider);
                    }

                    Searching.RangeInfo[] RangeInfo = new Searching.RangeInfo[NrFields];
                    Dictionary<string, int> FieldOrder = new Dictionary<string, int>();
                    List<Searching.IApplicableFilter> AdditionalFields = null;
                    List<Filter> AdditionalFields2 = null;
                    int i = 0;

                    foreach (string FieldName2 in Index.FieldNames)
                    {
                        RangeInfo[i] = new Searching.RangeInfo(FieldName2);
                        FieldOrder[FieldName2] = i++;
                        if (i >= NrFields)
                            break;
                    }

                    bool Consistent = true;
                    bool Smaller;

                    foreach (Filter Filter2 in ChildFilters)
                    {
                        if (Filter2 is FilterFieldValue FilterFieldValue)
                        {
                            if (!FieldOrder.TryGetValue(FilterFieldValue.FieldName, out i) || Filter2 is FilterFieldNotEqualTo)
                            {
                                if (AdditionalFields is null)
                                {
                                    AdditionalFields = new List<Searching.IApplicableFilter>();
                                    AdditionalFields2 = new List<Filter>();
                                }

                                AdditionalFields.Add(this.ConvertFilter(FilterFieldValue));
                                AdditionalFields2.Add(FilterFieldValue);
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
                        else if (Filter2 is FilterFieldLikeRegEx FilterFieldLikeRegEx)
                        {
                            Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = RegExFields.First.Value.Key;
                            string ConstantPrefix = RegExFields.First.Value.Value;

                            RegExFields.RemoveFirst();

                            if (AdditionalFields is null)
                            {
                                AdditionalFields = new List<Searching.IApplicableFilter>();
                                AdditionalFields2 = new List<Filter>();
                            }

                            AdditionalFields.Add(FilterFieldLikeRegEx2);
                            AdditionalFields2.Add(FilterFieldLikeRegEx);

                            if (!FieldOrder.TryGetValue(FilterFieldLikeRegEx.FieldName, out i) || string.IsNullOrEmpty(ConstantPrefix))
                                continue;

                            if (!RangeInfo[i].SetMin(ConstantPrefix, true, out Smaller))
                            {
                                Consistent = false;
                                break;
                            }
                        }
                    }

                    if (Consistent)
                    {
                        if (AdditionalFields is null)
                            return new Searching.RangesCursor<T>(Index, RangeInfo, null, Locked, this.provider);
                        else
                            return new Searching.RangesCursor<T>(Index, RangeInfo, AdditionalFields.ToArray(), Locked, this.provider);
                    }
                    else
                        return new Searching.UnionCursor<T>(new Filter[0], this, Locked);   // Empty result set.
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
                        Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                        return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked),
                            this.ConvertFilter(Filter), false, true, this.timeoutMilliseconds, this.provider);
                    }
                    else
                        return new Searching.UnionCursor<T>(ChildFilters, this, Locked);
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
                        Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                        return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked),
                            this.ConvertFilter(Filter), false, true, this.timeoutMilliseconds, this.provider);
                    }
                    else
                        return await this.ConvertFilterToCursor<T>(NegatedFilter, Locked, SortOrder);
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
                    if (Filter is FilterFieldEqualTo)
                    {
                        if (this.provider.GetObjectSerializer(typeof(T)) is ObjectSerializer Serializer &&
                            !(Serializer is null) &&
                            Serializer.HasObjectIdField && Serializer.ObjectIdMemberName == FilterFieldValue.FieldName)
                        {
                            try
                            {
                                Guid ObjectId;

                                if (Value is Guid)
                                    ObjectId = (Guid)Value;
                                else if (Value is string)
                                    ObjectId = new Guid((string)Value);
                                else if (Value is byte[])
                                    ObjectId = new Guid((byte[])Value);
                                else
                                    return new Searching.EmptyCursor<T>(Serializer);

                                T Obj = await this.LoadObject<T>(ObjectId);
                                return new Searching.SingletonCursor<T>(Obj, Serializer, ObjectId);
                            }
                            catch (Exception)
                            {
                                return new Searching.EmptyCursor<T>(Serializer);
                            }
                        }
                    }

                    this.nrFullFileScans++;
                    Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                    return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked),
                        this.ConvertFilter(Filter), false, true, this.timeoutMilliseconds, this.provider);
                }


                if (Filter is FilterFieldEqualTo)
                {
                    Searching.IApplicableFilter Filter2 = this.ConvertFilter(Filter);
                    bool UntilFirstFail;

                    if (!(SortOrder is null) && SortOrder.Length > 0 && Index.ReverseSortOrder(Filter2.ConstantFields, SortOrder))
                    {
                        UntilFirstFail = true;
                        Cursor = new Searching.ReversedCursor<T>(await Index.FindLastLesserOrEqualTo<T>(Locked,
                            new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)), this.timeoutMilliseconds);
                    }
                    else
                    {
                        Cursor = await Index.FindFirstGreaterOrEqualTo<T>(Locked, new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value));
                        UntilFirstFail = Index.SameSortOrder(Filter2.ConstantFields, SortOrder);

                        if (!UntilFirstFail)
                            Log.Notice("Search resulted in large part of the file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                    }

                    return new Searching.FilteredCursor<T>(Cursor, Filter2, UntilFirstFail, true, this.timeoutMilliseconds, this.provider);
                }
                else if (Filter is FilterFieldNotEqualTo)
                {
                    this.nrFullFileScans++;
                    Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                    return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked),
                        this.ConvertFilter(Filter), false, true, this.timeoutMilliseconds, this.provider);
                }
                else
                {
                    Searching.IApplicableFilter Filter2 = this.ConvertFilter(Filter);
                    bool IsSorted = (!(SortOrder is null) && SortOrder.Length > 0);

                    if (Filter is FilterFieldGreaterOrEqualTo)
                    {
                        if (IsSorted && Index.ReverseSortOrder(Filter2.ConstantFields, SortOrder))
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.GetTypedEnumerator<T>(Locked),
                                Filter2, true, false, this.timeoutMilliseconds, this.provider);
                        }
                        else
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.FindFirstGreaterOrEqualTo<T>(Locked, new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
                                null, false, true, this.timeoutMilliseconds, this.provider);
                        }
                    }
                    else if (Filter is FilterFieldLesserOrEqualTo)
                    {
                        if (IsSorted && Index.SameSortOrder(Filter2.ConstantFields, SortOrder))
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.GetTypedEnumerator<T>(Locked),
                                Filter2, true, true, this.timeoutMilliseconds, this.provider);
                        }
                        else
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.FindLastLesserOrEqualTo<T>(Locked, new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
                                null, false, false, this.timeoutMilliseconds, this.provider);
                        }
                    }
                    else if (Filter is FilterFieldGreaterThan)
                    {
                        if (IsSorted && Index.ReverseSortOrder(Filter2.ConstantFields, SortOrder))
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.GetTypedEnumerator<T>(Locked),
                                Filter2, true, false, this.timeoutMilliseconds, this.provider);
                        }
                        else
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.FindFirstGreaterThan<T>(Locked, new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
                                null, false, true, this.timeoutMilliseconds, this.provider);
                        }
                    }
                    else if (Filter is FilterFieldLesserThan)
                    {
                        if (IsSorted && Index.SameSortOrder(Filter2.ConstantFields, SortOrder))
                        {
                            return new Searching.FilteredCursor<T>(
                                await Index.GetTypedEnumerator<T>(Locked),
                                Filter2, true, true, this.timeoutMilliseconds, this.provider);
                        }
                        else
                        {
                            return new Searching.FilteredCursor<T>(
                            await Index.FindLastLesserThan<T>(Locked, new KeyValuePair<string, object>(FilterFieldValue.FieldName, Value)),
                            null, false, false, this.timeoutMilliseconds, this.provider);
                        }
                    }
                    else
                        throw this.UnknownFilterType(Filter);
                }
            }
            else
            {
                if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
                {
                    Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter);
                    IndexBTreeFile Index = this.FindBestIndex(out int _, FilterFieldLikeRegEx.FieldName, SortOrder);

                    string ConstantPrefix = Index is null ? string.Empty : this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

                    if (string.IsNullOrEmpty(ConstantPrefix))
                    {
                        this.nrFullFileScans++;
                        Log.Notice("Search resulted in entire file to be scanned. Consider either adding indices, or enumerate objects using an object enumerator.", this.fileName, string.Empty, "DBOpt");
                        return new Searching.FilteredCursor<T>(await this.GetTypedEnumeratorAsync<T>(Locked), FilterFieldLikeRegEx2,
                            false, true, this.timeoutMilliseconds, this.provider);
                    }
                    else
                    {
                        ICursor<T> Cursor = await Index.FindFirstGreaterOrEqualTo<T>(Locked,
                            new KeyValuePair<string, object>(FilterFieldLikeRegEx.FieldName, ConstantPrefix));
                        int c = ConstantPrefix.Length - 1;
                        char LastChar = ConstantPrefix[c];

                        if (LastChar < char.MaxValue)
                        {
                            string ConstantPrefix2 = ConstantPrefix.Substring(0, c) + new string((char)(LastChar + 1), 1);
                            Cursor = new Searching.FilteredCursor<T>(Cursor,
                                new Searching.FilterFieldLesserThan(FilterFieldLikeRegEx.FieldName, ConstantPrefix2),
                                true, true, this.timeoutMilliseconds, Provider);
                        }

                        return new Searching.FilteredCursor<T>(Cursor, FilterFieldLikeRegEx2, false, true,
                            this.timeoutMilliseconds, this.provider);
                    }
                }
                else
                    throw this.UnknownFilterType(Filter);
            }
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

                    foreach (Filter Filter2 in ChildFilters)
                    {
                        if (Filter2 is FilterFieldValue)
                        {
                            if (!(Filter2 is FilterFieldNotEqualTo))
                            {
                                if (Properties is null)
                                    Properties = new List<string>();

                                Properties.Add(((FilterFieldValue)Filter2).FieldName);
                            }
                        }
                        else if (Filter2 is FilterFieldLikeRegEx FilterFieldLikeRegEx)
                        {
                            Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter2);
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

                    IndexBTreeFile Index = Properties is null ? null : this.FindBestIndex(out int _, Properties.ToArray());
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
            else
            {
                if (Filter is FilterFieldLikeRegEx FilterFieldLikeRegEx)
                {
                    Searching.FilterFieldLikeRegEx FilterFieldLikeRegEx2 = (Searching.FilterFieldLikeRegEx)this.ConvertFilter(Filter);
                    IndexBTreeFile Index = this.FindBestIndex(FilterFieldLikeRegEx.FieldName);

                    string ConstantPrefix = Index is null ? string.Empty : this.GetRegExConstantPrefix(FilterFieldLikeRegEx.RegularExpression, FilterFieldLikeRegEx2.Regex);

                    if (string.IsNullOrEmpty(ConstantPrefix))
                        return true;
                    else
                        return false;
                }
                else
                    throw this.UnknownFilterType(Filter);
            }
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

                            case '\\':
                                Result.Append('\\');
                                break;

                            case 'e':
                                Result.Append('\u001B');
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

    }
}

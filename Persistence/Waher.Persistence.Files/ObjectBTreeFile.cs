using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Cache;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Files.Storage;
using Waher.Script;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a binary encrypted file where objects are persisted in a B-tree.
	/// </summary>
	public class ObjectBTreeFile : IDisposable, ICollection<object>
	{
		internal const int BlockHeaderSize = 14;

		private LinkedList<uint> emptyBlocks = null;
		private GenericObjectSerializer genericSerializer;
		private FilesProvider provider;
		private AesCryptoServiceProvider aes;
		private Cache<long, byte[]> blocks;
		private FileStream file;
		private Encoding encoding;
		private SemaphoreSlim fileAccessSemaphore = new SemaphoreSlim(1, 1);
		private SortedDictionary<long, byte[]> toSave = null;
		private long bytesAdded = 0;
		private ulong nrBlockLoads = 0;
		private ulong nrCacheLoads = 0;
		private ulong nrBlockSaves = 0;
		private ulong blockUpdateCounter = 0;
		private byte[] aesKey;
		private byte[] p;
		private string fileName;
		private string collectionName;
		private string blobFolder;
		private int blockSize;
		private int timeoutMilliseconds;
		private bool isCorrupt = false;
		private bool encypted;
		private bool emptyRoot = false;

		/// <summary>
		/// This class manages a binary encrypted file where objects are persisted in a B-tree.
		/// </summary>
		/// <param name="FileName">Name of binary file. File will be created if it does not exist. The class will require
		/// unique read/write access to the file.</param>
		/// <param name="CollectionName">Name of collection corresponding to the file.</param>
		/// <param name="BlobFolder">Folder in which BLOBs are stored.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Larger objects will be persisted as BLOBs, 
		/// with the bulk of the object stored as separate files. Smallest block size = 1024, largest block size = 65536.</param>
		/// <param name="BlocksInCache">Maximum number of blocks in in-memory cache.</param>
		/// <param name="Provider">Reference to the files provider.</param>
		/// <param name="Encoding">Encoding to use for text properties.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds, to wait for access to the database layer.</param>
		/// <param name="Encrypted">If the files should be encrypted or not.</param>
		public ObjectBTreeFile(string FileName, string CollectionName, string BlobFolder, int BlockSize, int BlocksInCache,
			FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds, bool Encrypted)
		{
			if (BlockSize < 1024)
				throw new ArgumentException("Block size too small.");

			if (BlockSize > 65536)
				throw new ArgumentException("Block size too large.");

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentException("The timeout must be positive.", "TimeoutMilliseconds");

			int i = BlockSize;
			while (i != 0 && (i & 1) == 0)
				i >>= 1;

			if (i != 1)
				throw new ArgumentException("The block size must be a power of 2.");

			this.provider = Provider;
			this.fileName = FileName;
			this.collectionName = CollectionName;
			this.blobFolder = BlobFolder;
			this.blockSize = BlockSize;
			this.encoding = Encoding;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.genericSerializer = new GenericObjectSerializer(this.provider);
			this.encypted = Encrypted;

			if (this.encypted)
			{
				RSACryptoServiceProvider rsa;
				CspParameters cspParams = new CspParameters();
				cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
				cspParams.KeyContainerName = this.fileName;

				try
				{
					rsa = new RSACryptoServiceProvider(cspParams);
				}
				catch (CryptographicException ex)
				{
					throw new CryptographicException("Unable to get access to cryptographic key to unlock database. Was the database created using another user?", ex);
				}

				string Xml = rsa.ToXmlString(true);

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml);
				this.p = Convert.FromBase64String(Doc.DocumentElement["P"].InnerText);
				byte[] Q = Convert.FromBase64String(Doc.DocumentElement["Q"].InnerText);

				this.aes = new AesCryptoServiceProvider();
				aes.BlockSize = 128;
				aes.KeySize = 256;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.None;

				using (SHA256Managed Sha256 = new SHA256Managed())
				{
					this.aesKey = Sha256.ComputeHash(Q);
				}
			}

			string Folder = Path.GetDirectoryName(FileName);
			if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			this.blocks = new Cache<long, byte[]>(BlocksInCache, TimeSpan.MaxValue, new TimeSpan(0, 1, 0, 0, 0));

			if (File.Exists(FileName))
				this.file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			else
			{
				this.file = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
				Task Task = this.CreateFirstBlock();
				Task.Wait();
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.file != null)
			{
				this.file.Dispose();
				this.file = null;
			}

			if (this.blocks != null)
			{
				this.blocks.Dispose();
				this.blocks = null;
			}
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
		/// Folder in which BLOBs are stored.
		/// </summary>
		public string BlobFolder { get { return this.blobFolder; } }

		/// <summary>
		/// Encoding to use for text properties.
		/// </summary>
		public Encoding Encoding { get { return this.encoding; } }

		/// <summary>
		/// Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Larger objects will be persisted as BLOBs, 
		/// with the bulk of the object stored as separate files. Smallest block size = 1024, largest block size = 65536.
		/// </summary>
		public int BlockSize { get { return this.blockSize; } }

		/// <summary>
		/// Timeout, in milliseconds, for database operations.
		/// </summary>
		public int TimeoutMillisecondsx
		{
			get { return this.timeoutMilliseconds; }
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

		#region GUIDs for databases

		/// <summary>
		/// Creates a new GUID suitable for use in databases.
		/// </summary>
		/// <returns>New GUID.</returns>
		public static Guid CreateDatabaseGUID()
		{
			return guidGenerator.CreateGuid();
		}

		private static SequentialGuidGenerator guidGenerator = new SequentialGuidGenerator();

		#endregion

		#region Locks

		/// <summary>
		/// Locks access to the file.
		/// </summary>
		/// <returns>Task object.</returns>
		internal async Task Lock()
		{
			if (!await this.fileAccessSemaphore.WaitAsync(this.timeoutMilliseconds))
				throw new TimeoutException("Unable to get access to underlying database.");
		}

		/// <summary>
		/// Releases the file for access.
		/// </summary>
		/// <returns>Task object.</returns>
		internal async Task Release()
		{
			if (this.emptyRoot)
			{
				this.emptyRoot = false;

				/*byte[] Block = await this.LoadBlockLocked(0, true);
				BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
				BlockHeader Header = new Storage.BlockHeader(Reader);
				uint BlockIndex;

				while (Header.BytesUsed == 0 && (BlockIndex = Header.LastBlockIndex) != 0)
				{
					Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, true);
					Reader.Restart(Block, 0);
					Header = new Storage.BlockHeader(Reader);

					this.RegisterEmptyBlockLocked(BlockIndex);
				}

				this.QueueSaveBlockLocked(0, Block);

				await this.UpdateParentLinks(0, Block);*/
			}

			if (this.toSave != null)
				await this.SaveUnsaved();

			this.fileAccessSemaphore.Release();
		}

		private async Task SaveUnsaved()
		{
			if (this.toSave != null)
			{
				foreach (KeyValuePair<long, byte[]> Rec in this.toSave)
					await this.DoSaveBlockLocked(Rec.Key, Rec.Value);

				this.toSave.Clear();
				this.bytesAdded = 0;
			}
		}

		#endregion

		#region Blocks

		private async Task<Tuple<uint, byte[]>> CreateNewBlockLocked()
		{
			byte[] Block;
			long PhysicalPosition;

			if (this.emptyBlocks != null)
			{
				uint BlockIndex = this.emptyBlocks.First.Value;

				this.emptyBlocks.RemoveFirst();
				if (this.emptyBlocks.First == null)
					this.emptyBlocks = null;

				PhysicalPosition = ((long)BlockIndex) * this.blockSize;
				Block = await this.LoadBlockLocked(PhysicalPosition, true);

				Array.Clear(Block, 0, this.blockSize);
			}
			else
			{
				Block = new byte[this.blockSize];
				PhysicalPosition = this.file.Length + this.bytesAdded;

				this.bytesAdded += this.blockSize;
			}

			this.QueueSaveBlockLocked(PhysicalPosition, Block);

			return new Tuple<uint, byte[]>((uint)(PhysicalPosition / this.blockSize), Block);
		}

		private async Task CreateFirstBlock()
		{
			await this.Lock();
			try
			{
				await this.CreateNewBlockLocked();
			}
			finally
			{
				await this.Release();
			}
		}

		/// <summary>
		/// Clears the internal memory cache.
		/// </summary>
		public void ClearCache()
		{
			this.blocks.Clear();
		}

		/// <summary>
		/// Loads a block from the file.
		/// </summary>
		/// <param name="PhysicalPosition">Physical position of block in file.</param>
		/// <returns>Loaded block.</returns>
		public async Task<byte[]> LoadBlock(long PhysicalPosition)
		{
			await this.Lock();
			try
			{
				return await this.LoadBlockLocked(PhysicalPosition, true);
			}
			finally
			{
				await this.Release();
			}
		}

		internal async Task<byte[]> LoadBlockLocked(long PhysicalPosition, bool AddToCache)
		{
			byte[] Block;

			if ((PhysicalPosition % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "PhysicalPosition");

			if (this.blocks.TryGetValue(PhysicalPosition, out Block))
			{
				this.nrCacheLoads++;
				return Block;
			}

			if (this.toSave != null && this.toSave.TryGetValue(PhysicalPosition, out Block))
			{
				this.nrCacheLoads++;
				return Block;
			}

			if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
				throw new ArgumentException("Invalid file position.", "Position");

			Block = new byte[this.blockSize];

			int NrRead = await this.file.ReadAsync(Block, 0, this.blockSize);
			if (this.blockSize != NrRead)
				throw new IOException("Read past end of file.");

			this.nrBlockLoads++;

			if (this.encypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
				{
					Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}
			}

			if (AddToCache)
				this.blocks.Add(PhysicalPosition, Block);

			return Block;
		}

		/// <summary>
		/// Saves a block to the file.
		/// </summary>
		/// <param name="PhysicalPosition">Physical position of block in file.</param>
		/// <returns>Block to save.</returns>
		public async Task SaveBlock(long PhysicalPosition, byte[] Block)
		{
			await this.Lock();
			try
			{
				this.QueueSaveBlockLocked(PhysicalPosition, Block);
			}
			finally
			{
				await this.Release();
			}
		}

		internal void QueueSaveBlockLocked(long PhysicalPosition, byte[] Block)
		{
			if ((PhysicalPosition % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "PhysicalPosition");

			if (Block == null || Block.Length != this.blockSize)
				throw new ArgumentException("Block not of the correct block size.", "Block");

			byte[] PrevBlock;

			if (this.blocks.TryGetValue(PhysicalPosition, out PrevBlock) && PrevBlock != Block)
			{
				if (Array.Equals(PrevBlock, Block))
				{
					this.blocks.Add(PhysicalPosition, Block);   // Update to new reference.
					return;     // No need to save.
				}
			}

			if (this.toSave == null)
				this.toSave = new SortedDictionary<long, byte[]>();

			this.toSave[PhysicalPosition] = Block;
			this.blockUpdateCounter++;

			this.blocks.Add(PhysicalPosition, Block);
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

			if (this.encypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition)))
				{
					EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}
			}
			else
				EncryptedBlock = Block;

			if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
				throw new ArgumentException("Invalid file position.", "Position");

			await this.file.WriteAsync(EncryptedBlock, 0, this.blockSize);

			this.nrBlockSaves++;
		}

		private byte[] GetIV(long Position)
		{
			byte[] Input = new byte[72];
			Array.Copy(this.p, 0, Input, 0, 64);
			Array.Copy(BitConverter.GetBytes(Position), 0, Input, 64, 8);
			byte[] Hash;

			using (SHA1Managed Sha1 = new SHA1Managed())
			{
				Hash = Sha1.ComputeHash(Input);
			}

			Array.Resize<byte>(ref Hash, 16);

			return Hash;
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
			ObjectSerializer Serializer = this.provider.GetObjectSerializer(ObjectType) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Cannot store objects of type " + ObjectType.FullName + " directly. They need to be embedded.");

			return this.SaveNewObject(Object, Serializer);
		}

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
		public async Task<Guid> SaveNewObject(object Object, ObjectSerializer Serializer)
		{
			await this.Lock();
			try
			{
				bool HasObjectId = Serializer.HasObjectId(Object);
				BlockInfo Leaf;
				BinarySerializer Writer;
				Guid ObjectId;
				byte[] Bin;

				if (HasObjectId)
				{
					ObjectId = Serializer.GetObjectId(Object, false);
					Leaf = await this.FindLeafNodeLocked(ObjectId);

					if (Leaf == null)
						throw new IOException("Object with same Object ID already exists.");
				}
				else
				{
					do
					{
						ObjectId = CreateDatabaseGUID();
					}
					while ((Leaf = await this.FindLeafNodeLocked(ObjectId)) == null);

					if (!Serializer.TrySetObjectId(Object, ObjectId))
						throw new NotSupportedException("Unable to set Object ID: Unsupported type.");
				}

				Writer = new BinarySerializer(this.collectionName, this.encoding);
				Serializer.Serialize(Writer, false, false, Object);
				Bin = Writer.GetSerialization();

				// TODO: BLOBs: Objects so large that not two fits in the same block, must be stored separately, and encoded as a BLOB. Include count & size in statistics. Also list BLOB files that are not referenced.

				await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0, true);

				return ObjectId;
			}
			finally
			{
				await this.Release();
			}
		}

		internal async Task InsertObjectLocked(uint BlockIndex, BlockHeader Header, byte[] Block, byte[] Bin, int InsertAt,
			uint ChildRightLink, uint ChildRightLinkSize, bool IncSize)
		{
			uint Used = Header.BytesUsed;
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
						BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
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
				BlockSplitter Splitter = new BlockSplitterLast(this.blockSize);         // Since GUIDs are mostly an increasing sequence, we put as many nodes into the left child node as possible.
																						//BlockSplitter Splitter = new BlockSplitterMiddle(PayloadSize);
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
					Left = null;
					LeftLink = BlockIndex;
					Splitter.LeftBlock = new byte[this.blockSize];
				}

				int Len;
				int Pos;
				int c;
				uint BlockLink;
				uint ChildSize;
				Guid Guid;
				bool IsEmpty;
				bool Leaf = true;

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, BlockHeaderSize);

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					IsEmpty = Guid.Equals(Guid.Empty);
					if (IsEmpty)
						break;

					if (BlockLink != 0)
					{
						Leaf = false;
						ChildSize = await this.GetObjectSizeOfBlock(BlockLink);
					}
					else
						ChildSize = 0;

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
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
				while (Reader.BytesLeft >= 21);

				if (!IsEmpty)
					Pos = Reader.Position;

				if (Pos == InsertAt)
				{
					BlockLink = BitConverter.ToUInt32(Block, 6);
					ChildSize = BlockLink == 0 ? 0 : await this.GetObjectSizeOfBlock(BlockLink);
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
						Splitter.RightSizeSubtree += await this.GetObjectSizeOfBlock(BlockLink);
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
					BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock);

					BlockHeader ParentHeader = new BlockHeader(ParentReader);
					int ParentLen;
					int ParentPos;
					uint ParentBlockIndex;

					if (ParentHeader.LastBlockIndex == LeftLink)
						ParentPos = BlockHeaderSize + ParentHeader.BytesUsed;
					else
					{
						ParentBlockIndex = 0;
						do
						{
							ParentPos = ParentReader.Position;

							ParentBlockIndex = ParentReader.ReadUInt32();                  // Block link.
							ParentReader.Position += 16;
							ParentLen = (int)ParentReader.ReadVariableLengthUInt64();     // Remaining length of object.

							ParentReader.Position += ParentLen;
						}
						while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 21);

						if (ParentBlockIndex != LeftLink)
						{
							this.isCorrupt = true;

							throw new IOException("Parent link points to parent block (" + ParentLink.ToString() +
								") with no reference to child block (" + LeftLink.ToString() + ").");
						}
					}

					await InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
						Splitter.RightSizeSubtree, IncSize);
				}

				if (!Leaf)
				{
					if (CheckParentLinksLeft)
						await this.UpdateParentLinks(LeftLink, Splitter.LeftBlock);

					if (CheckParentLinksRight)
						await this.UpdateParentLinks(RightLink, Splitter.RightBlock);
				}
			}
		}

		private async Task UpdateParentLinks(uint BlockIndex, byte[] Block)
		{
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);

			Guid Guid;
			int Len;
			int Pos;
			uint ChildLink;

			while (Reader.BytesLeft >= 21)
			{
				Pos = Reader.Position;

				ChildLink = Reader.ReadUInt32();                  // Block link.
				Guid = Reader.ReadGuid();
				if (Guid.Equals(Guid.Empty))
					break;

				Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.

				if (ChildLink != 0)
					await this.CheckChildParentLink(ChildLink, BlockIndex);

				Reader.Position += Len;
			}

			ChildLink = BitConverter.ToUInt32(Block, 6);
			if (ChildLink != 0)
				await this.CheckChildParentLink(ChildLink, BlockIndex);
		}

		private async Task<uint> GetObjectSizeOfBlock(uint BlockIndex)
		{
			long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);

			return BitConverter.ToUInt32(Block, 2);
		}

		private async Task CheckChildParentLink(uint ChildLink, uint NewParentLink)
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

		private async Task<BlockInfo> FindLeafNodeLocked(Guid ObjectId)
		{
			uint BlockIndex = 0;

			while (true)
			{
				long PhysicalPosition = BlockIndex;
				PhysicalPosition *= this.blockSize;

				byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);

				BlockHeader Header = new BlockHeader(Reader);
				Guid Guid;
				int Len;
				int Pos;
				uint BlockLink;
				int Comparison;
				bool IsEmpty;

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.

					IsEmpty = Guid.Equals(Guid.Empty);
					Comparison = ObjectId.CompareTo(Guid);

					if (IsEmpty)
						break;

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					Reader.Position += Len;
				}
				while (Comparison > 0 && Reader.BytesLeft >= 21);

				if (Comparison == 0)                                       // Object ID already found.
					return null;
				else if (IsEmpty || Comparison > 0)
				{
					if (Header.LastBlockIndex == 0)
						return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), IsEmpty ? Pos : Reader.Position);
					else
						BlockIndex = Header.LastBlockIndex;
				}
				else
				{
					if (BlockLink == 0)
						return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos);
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
		public Task<object> LoadObject<T>(Guid ObjectId, Type Type)
		{
			return this.LoadObject(ObjectId, this.provider.GetObjectSerializer(Type));
		}

		/// <summary>
		/// Loads an object from the file.
		/// </summary>
		/// <param name="ObjectId">ID of object to load.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer will be deduced from information stored in the file.</param>
		public async Task<object> LoadObject(Guid ObjectId, IObjectSerializer Serializer)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info == null)
				throw new IOException("Object not found.");

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, Info.InternalPosition + 4);

			if (Serializer == null)
			{
				Reader.Position += 16;

				Reader.ReadVariableLengthUInt64();  // Length
				string TypeName = Reader.ReadString();
				if (string.IsNullOrEmpty(TypeName))
					Serializer = this.genericSerializer;
				else
				{
					Type T = Types.GetType(TypeName);
					if (T != null)
						Serializer = this.provider.GetObjectSerializer(T);
					else
						Serializer = this.genericSerializer;
				}

				Reader.Position = Info.InternalPosition + 4;
			}

			return Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);
		}

		private async Task<BlockInfo> FindNodeLocked(Guid ObjectId)
		{
			uint BlockIndex = 0;

			if (ObjectId.Equals(Guid.Empty))
				return null;

			while (true)
			{
				long PhysicalPosition = BlockIndex;
				PhysicalPosition *= this.blockSize;

				byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);

				BlockHeader Header = new BlockHeader(Reader);
				Guid Guid;
				int Len;
				int Pos;
				uint BlockLink;
				int Comparison;
				bool IsEmpty;

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.

					IsEmpty = Guid.Equals(Guid.Empty);
					Comparison = ObjectId.CompareTo(Guid);

					if (IsEmpty)
						break;

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					Reader.Position += Len;
				}
				while (Comparison > 0 && Reader.BytesLeft >= 21);

				if (Comparison == 0)                                       // Object ID found.
					return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos);
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
		/// <exception cref="IOException">If the object is not found in the database.</exception>
		public Task UpdateObject(object Object)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = this.provider.GetObjectSerializer(ObjectType) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Cannot update objects of type " + ObjectType.FullName + " directly. They need to be embedded.");

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
		/// <exception cref="IOException">If the object is not found in the database.</exception>
		public async Task UpdateObject(object Object, ObjectSerializer Serializer)
		{
			Guid ObjectId = Serializer.GetObjectId(Object, false);

			await this.Lock();
			try
			{
				await this.UpdateObjectLocked(ObjectId, Object, Serializer);
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task UpdateObjectLocked(Guid ObjectId, object Object, ObjectSerializer Serializer)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info == null)
				throw new IOException("Object not found.");

			BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
			Serializer.Serialize(Writer, false, false, Object);
			byte[] Bin = Writer.GetSerialization();

			await this.ReplaceObjectLocked(Bin, Info);
		}

		private async Task ReplaceObjectLocked(byte[] Bin, BlockInfo Info)
		{
			int NewSize = Bin.Length;

			byte[] Block = Info.Block;
			BlockHeader Header = Info.Header;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, Info.InternalPosition + 20);
			uint BlockIndex = Info.BlockIndex;
			int Len = (int)Reader.ReadVariableLengthUInt64();
			int OldSize = Reader.Position + Len - (Info.InternalPosition + 4);
			int DeltaSize = NewSize - OldSize;
			int i;

			// TODO: BLOBs	Max object size = (blockSize - headerSize)/3 - 4

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
					Left = null;
					LeftLink = BlockIndex;
					Splitter.LeftBlock = new byte[this.blockSize];
				}

				int Pos;
				int c;
				uint BlockLink;
				uint ChildSize;
				Guid Guid;
				bool IsEmpty;
				bool Leaf = true;

				Reader.Restart(Block, BlockHeaderSize);

				do
				{
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					IsEmpty = Guid.Equals(Guid.Empty);
					if (IsEmpty)
						break;

					if (BlockLink != 0)
					{
						Leaf = false;
						ChildSize = await this.GetObjectSizeOfBlock(BlockLink);
					}
					else
						ChildSize = 0;

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					c = Reader.Position - Pos + Len;

					if (Pos == Info.InternalPosition)
						Splitter.NextBlock(BlockLink, Bin, 0, Bin.Length, ChildSize);
					else
						Splitter.NextBlock(BlockLink, Block, Pos + 4, c - 4, ChildSize);

					Reader.Position += Len;
				}
				while (Reader.BytesLeft >= 21);

				BlockLink = BitConverter.ToUInt32(Block, 6);
				Splitter.RightLastBlockIndex = BlockLink;
				if (BlockLink != 0)
					Splitter.RightSizeSubtree += await this.GetObjectSizeOfBlock(BlockLink);

				ushort LeftBytesUsed = (ushort)(Splitter.LeftPos - BlockHeaderSize);
				ushort RightBytesUsed = (ushort)(Splitter.RightPos - BlockHeaderSize);

				uint ParentLink = BlockIndex == 0 ? 0 : Header.ParentBlockIndex;

				Array.Copy(BitConverter.GetBytes(LeftBytesUsed), 0, Splitter.LeftBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.LeftSizeSubtree), 0, Splitter.LeftBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.LeftBlock, 10, 4);

				Array.Copy(BitConverter.GetBytes(RightBytesUsed), 0, Splitter.RightBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(Splitter.RightSizeSubtree), 0, Splitter.RightBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, Splitter.RightBlock, 10, 4);

				string s = await this.GetCurrentStateReportAsyncLocked(false);

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
					BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock);

					BlockHeader ParentHeader = new BlockHeader(ParentReader);
					int ParentLen;
					int ParentPos;
					uint ParentBlockIndex;

					if (ParentHeader.LastBlockIndex == LeftLink)
						ParentPos = BlockHeaderSize + ParentHeader.BytesUsed;
					else
					{
						ParentBlockIndex = 0;
						do
						{
							ParentPos = ParentReader.Position;

							ParentBlockIndex = ParentReader.ReadUInt32();                  // Block link.
							ParentReader.Position += 16;
							ParentLen = (int)ParentReader.ReadVariableLengthUInt64();     // Remaining length of object.

							ParentReader.Position += ParentLen;
						}
						while (ParentBlockIndex != LeftLink && ParentReader.BytesLeft >= 21);

						if (ParentBlockIndex != LeftLink)
						{
							this.isCorrupt = true;

							throw new IOException("Parent link points to parent block (" + ParentLink.ToString() +
								") with no reference to child block (" + LeftLink.ToString() + ").");
						}
					}

					await InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, Splitter.ParentObject, ParentPos, RightLink,
						Splitter.RightSizeSubtree, false);
				}

				if (!Leaf)
				{
					if (CheckParentLinksLeft)
						await this.UpdateParentLinks(LeftLink, Splitter.LeftBlock);

					if (CheckParentLinksRight)
						await this.UpdateParentLinks(RightLink, Splitter.RightBlock);
				}

				s = await this.GetCurrentStateReportAsyncLocked(false);
			}
		}

		#endregion

		#region Delete Object


		/// <summary>
		/// Deletes an object from the database, using the object serializer corresponding to the type of object being updated, to find
		/// the Object ID of the object.
		/// </summary>
		/// <param name="Object">Object to delete.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="IOException">If the object is not found in the database.</exception>
		public Task DeleteObject(object Object)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = this.provider.GetObjectSerializer(ObjectType) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Cannot delete objects of type " + ObjectType.FullName + " directly. They need to be embedded.");

			return this.DeleteObject(Object, Serializer);
		}

		/// <summary>
		/// Deletes an object from the database, using the object serializer corresponding to the type of object being updated, to find
		/// the Object ID of the object.
		/// </summary>
		/// <param name="Object">Object to delete.</param>
		/// <param name="Serializer">Object serializer to use.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		/// <exception cref="IOException">If the object is not found in the database.</exception>
		public Task DeleteObject(object Object, ObjectSerializer Serializer)
		{
			Guid ObjectId = Serializer.GetObjectId(Object, false);
			return this.DeleteObject(ObjectId);
		}

		/// <summary>
		/// Deletes an object from the database.
		/// </summary>
		/// <param name="ObjectId">Object ID of the object to delete.</param>
		/// <returns>Task object that can be used to wait for the asynchronous method to complete.</returns>
		/// <exception cref="IOException">If the object is not found in the database.</exception>
		public async Task DeleteObject(Guid ObjectId)
		{
			await this.Lock();
			try
			{
				await this.DeleteObjectLocked(ObjectId);
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task DeleteObjectLocked(Guid ObjectId)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info == null)
				throw new IOException("Object not found.");

			byte[] Block = Info.Block;
			BlockHeader Header = Info.Header;
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block, Info.InternalPosition);
			uint BlockIndex = Info.BlockIndex;
			uint LeftBlockLink = Reader.ReadUInt32();
			int Len;
			int i, c;

			Reader.Position += 16;
			Len = (int)Reader.ReadVariableLengthUInt64();

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

							await this.UpdateParentLinks(0, Block);
						}
					}
					else
					{
						Header.SizeSubtree = 0;
						Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
						Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

						this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
						await this.DecreaseSizeLocked(Header.ParentBlockIndex);

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
					RightBlockLink = Reader.ReadUInt32();
					Last = false;
				}

				string s = await this.GetCurrentStateReportAsyncLocked(false);
				bool Reshuffled = false;
				byte[] NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, false, false);

				if (NewSeparator == null)
				{
					NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, false, false);

					if (NewSeparator == null)
					{
						Reshuffled = true;
						NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, true, false);

						if (NewSeparator == null)
						{
							NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, true, false);

							if (NewSeparator == null)
							{
								NewSeparator = await this.TryExtractLargestObjectLocked(LeftBlockLink, false, true);

								if (NewSeparator == null)
								{
									NewSeparator = await this.TryExtractSmallestObjectLocked(RightBlockLink, false, true);

									if (NewSeparator == null)
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

												await this.UpdateParentLinks(0, MergeResult.ResultBlock);

												await this.DeleteObjectLocked(ObjectId);  // This time, the object will be lower in the tree.
												return;
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

										s = await this.GetCurrentStateReportAsyncLocked(false);

										this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
										this.QueueSaveBlockLocked(((long)LeftBlockLink) * this.blockSize, MergeResult.ResultBlock);

										await this.UpdateParentLinks(LeftBlockLink, MergeResult.ResultBlock);

										s = await this.GetCurrentStateReportAsyncLocked(false);

										await this.DeleteObjectLocked(ObjectId);  // This time, the object will be lower in the tree.

										if (Header.BytesUsed == 0 && BlockIndex != 0)
											await this.MergeEmptyBlockWithSiblingLocked(BlockIndex, Header.ParentBlockIndex);

										s = await this.GetCurrentStateReportAsyncLocked(false);

										if (MergeResult.Separator != null)
											await this.ReinsertMergeOverflow(MergeResult, BlockIndex);

										return;
									}
								}
							}
						}
					}
				}

				s = await this.GetCurrentStateReportAsyncLocked(false);

				long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
				Info.Block = await this.LoadBlockLocked(PhysicalPosition, true);    // Refresh object count.

				if (Reshuffled)
				{
					Reader.Restart(Info.Block, 0);
					Info.Header = new BlockHeader(Reader);

					if (this.ForEachObject(Info.Block, (Link, ObjectId2, Pos, Len2) =>
					{
						if (ObjectId.Equals(ObjectId2))
						{
							Info.InternalPosition = Pos - 4;
							return false;
						}
						else
							return true;
					}))
					{
						throw new IOException("Database seems to be corrupt.");
					}
				}

				await this.ReplaceObjectLocked(NewSeparator, Info);

				s = await this.GetCurrentStateReportAsyncLocked(false);
			}
		}

		private async Task ReinsertMergeOverflow(MergeResult MergeResult, uint BlockIndex)
		{
			if (MergeResult.Separator == null)
				return;

			// Update block object counts:

			long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			ulong Size = 0;
			ulong PrevSize;

			await this.ForEachObjectAsync(Block, async (Link, ObjectId, Pos, Len) =>
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

			BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectGuid(MergeResult.Separator));
			await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, MergeResult.Separator, Leaf.InternalPosition, 0, 0, true);

			if (MergeResult.Residue != null)
			{
				LinkedList<uint> Links = null;
				Block = MergeResult.Residue;

				while (Block != null)
				{
					await this.ForEachObjectAsync(Block, async (Link, ObjectId, Pos, Len) =>
					{
						byte[] Obj = new byte[Len];
						Array.Copy(MergeResult.Residue, Pos, Obj, 0, Len);
						BlockInfo Leaf2 = await this.FindLeafNodeLocked(this.GetObjectGuid(Obj));
						await this.InsertObjectLocked(Leaf2.BlockIndex, Leaf2.Header, Leaf2.Block, Obj, Leaf2.InternalPosition, 0, 0, true);

						if (Link != 0)
						{
							if (Links == null)
								Links = new LinkedList<uint>();

							Links.AddLast(Link);
						}

						return true;
					});

					BlockIndex = BitConverter.ToUInt32(Block, 6);
					if (BlockIndex != 0)
					{
						if (Links == null)
							Links = new LinkedList<uint>();

						Links.AddLast(BlockIndex);
					}

					if (Links == null || Links.First == null)
						Block = null;
					else
					{
						Block = await this.LoadBlockLocked(((long)Links.First.Value) * this.blockSize, true);
						Links.RemoveFirst();
					}
				}
			}
		}

		private string ToString(byte[] Block)
		{
			StringBuilder sb = new StringBuilder();

			this.ForEachObject(Block, (Link, ObjectId, Pos, Len) =>
			{
				sb.AppendLine(Link.ToString());
				sb.AppendLine(ObjectId.ToString() + ", " + Pos.ToString() + ", " + Len.ToString());
				return true;
			});

			sb.AppendLine(BitConverter.ToUInt32(Block, 6).ToString());

			return sb.ToString();
		}

		private async Task RebalanceEmptyBlockLocked(uint BlockIndex, byte[] Block, BlockHeader Header)
		{
			byte[] Object;

			string s = await this.GetCurrentStateReportAsyncLocked(false);

			if (BlockIndex == 0)
			{
				this.emptyRoot = true;
				return;
			}

			if (Header.LastBlockIndex != 0)
			{
				Object = await this.RotateLeftLocked(Header.LastBlockIndex, BlockIndex);
				if (Object == null)
					Object = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);
			}
			else
			{
				Object = await this.RotateLeftLocked(BlockIndex, Header.ParentBlockIndex);
				s = await this.GetCurrentStateReportAsyncLocked(false);
				if (Object == null)
				{
					Object = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);
					s = await this.GetCurrentStateReportAsyncLocked(false);

					// TODO: If Object==null, also do RotateLeft. But object needs to be inserted in last object, and then rotated again.
				}
			}

			if (Object == null)
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
					if (Object2 == null && BlockIndex != 0)
					{
						Object2 = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);
						// TODO: If Object==null, also do RotateLeft. But object needs to be inserted in last object, and then rotated again.
					}

					if (Object2 != null)
					{
						Array.Copy(Object2, 0, NewChildBlock, BlockHeaderSize + 4, Object2.Length);
						Array.Copy(BitConverter.GetBytes((ushort)(Object2.Length + 4)), 0, NewChildBlock, 0, 2);
						Array.Copy(BitConverter.GetBytes((uint)1), 0, NewChildBlock, 2, 4);
					}

					Array.Copy(BitConverter.GetBytes(BlockIndex), 0, NewChildBlock, 10, 4);

					this.QueueSaveBlockLocked(((long)NewChildBlockIndex) * this.blockSize, NewChildBlock);

					if (Object2 != null)
						await this.IncreaseSizeLocked(BlockIndex);
				}

				await this.IncreaseSizeLocked(BitConverter.ToUInt32(Block, 10));    // Note that Header.ParentBlockIndex might no longer be correct.
			}
		}

		private async Task MergeEmptyBlockWithSiblingLocked(uint ChildBlockIndex, uint ParentBlockIndex)
		{
			byte[] ParentBlock = await this.LoadBlockLocked(((long)ParentBlockIndex) * this.blockSize, true);
			BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock);
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

					await this.UpdateParentLinks(LastLink, MergeResult.ResultBlock);

					this.RegisterEmptyBlockLocked(ChildBlockIndex);

					if (ParentHeader.BytesUsed == 0)
						await this.RebalanceEmptyBlockLocked(ParentBlockIndex, ParentBlock, ParentHeader);

					if (MergeResult.Separator != null)
						await this.ReinsertMergeOverflow(MergeResult, ParentBlockIndex);
				}
				else
				{
					// Empty node.

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
					RightLink = ParentReader.ReadUInt32();
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

				await this.UpdateParentLinks(ChildBlockIndex, MergeResult.ResultBlock);

				if (ParentHeader.BytesUsed == 0)
					await this.RebalanceEmptyBlockLocked(ParentBlockIndex, ParentBlock, ParentHeader);

				if (MergeResult.Separator != null)
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

			MergeResult Result = new MergeResult();
			Result.ResultBlock = Splitter.LeftBlock;
			Result.ResultBlockSize = Splitter.LeftSizeSubtree;
			Result.Separator = Splitter.ParentObject;

			if (Splitter.ParentObject == null)
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

		private delegate Task<bool> ForEachAsyncDelegate(uint Link, Guid ObjectId, int Pos, int Len);

		private async Task<bool> ForEachObjectAsync(byte[] Block, ForEachAsyncDelegate Method)
		{
			BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			Guid Guid;
			uint Link;
			int Pos, Len, c;

			do
			{
				Pos = Reader.Position;

				Link = Reader.ReadUInt32();
				Guid = Reader.ReadGuid();
				if (Guid.Equals(Guid.Empty))
					break;

				Len = (int)Reader.ReadVariableLengthUInt64();
				Reader.Position += Len;

				c = Reader.Position - Pos - 4;

				if (!await Method(Link, Guid, Pos + 4, c))
					return false;
			}
			while (Reader.BytesLeft >= 21);

			return true;
		}

		private delegate bool ForEachDelegate(uint Link, Guid ObjectId, int Pos, int Len);

		private bool ForEachObject(byte[] Block, ForEachDelegate Method)
		{
			BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			Guid Guid;
			uint Link;
			int Pos, Len, c;

			do
			{
				Pos = Reader.Position;

				Link = Reader.ReadUInt32();
				Guid = Reader.ReadGuid();
				if (Guid.Equals(Guid.Empty))
					break;

				Len = (int)Reader.ReadVariableLengthUInt64();
				Reader.Position += Len;

				c = Reader.Position - Pos - 4;

				if (!Method(Link, Guid, Pos + 4, c))
					return false;
			}
			while (Reader.BytesLeft >= 21);

			return true;
		}

		private void RegisterEmptyBlockLocked(uint Block)
		{
			if (this.emptyBlocks == null)
				this.emptyBlocks = new LinkedList<uint>();

			this.emptyBlocks.AddLast(Block);

			long PhysicalLocation = ((long)Block) * this.blockSize;

			if (this.toSave != null)
				this.toSave.Remove(PhysicalLocation);

			this.blocks.Remove(PhysicalLocation);
		}

		private async Task<byte[]> TryExtractLargestObjectLocked(uint BlockIndex, bool AllowRotation, bool AllowMerge)
		{
			long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
			BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new Storage.BlockHeader(Reader);
			byte[] Result = null;
			Guid Guid;
			uint Link;
			int Len, c;
			int Pos, LastPos;

			if (Header.LastBlockIndex != 0)
			{
				Result = await this.TryExtractLargestObjectLocked(Header.LastBlockIndex, AllowRotation, AllowMerge);

				if (Result == null && AllowMerge)
				{
					LastPos = 0;
					do
					{
						Pos = Reader.Position;
						Link = Reader.ReadUInt32();
						Guid = Reader.ReadGuid();
						if (Guid.Equals(Guid.Empty))
							break;

						Len = (int)Reader.ReadVariableLengthUInt64();
						Reader.Position += Len;
						LastPos = Pos;
					}
					while (Reader.BytesLeft >= 21);

					if (LastPos != 0)
					{
						Reader.Position = LastPos;
						Link = Reader.ReadUInt32();
						Guid = Reader.ReadGuid();
						Len = (int)Reader.ReadVariableLengthUInt64();
						c = Reader.Position + Len - LastPos - 4;

						byte[] Separator = new byte[c];
						Array.Copy(Block, LastPos + 4, Separator, 0, c);

						MergeResult MergeResult = await this.MergeBlocksLocked(Link, Separator, Header.LastBlockIndex);

						if (MergeResult.Separator == null)
						{
							this.RegisterEmptyBlockLocked(Header.LastBlockIndex);
							Header.LastBlockIndex = Link;

							Array.Clear(Block, LastPos, c + 4);
							Header.BytesUsed -= (ushort)(c + 4);
							Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
							Array.Copy(BitConverter.GetBytes(Header.LastBlockIndex), 0, Block, 6, 4);

							this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
							this.QueueSaveBlockLocked(((long)Link) * this.blockSize, MergeResult.ResultBlock);

							await this.UpdateParentLinks(Link, MergeResult.ResultBlock);

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
					Link = Reader.ReadUInt32();
					if (Link != 0)
						return null;

					Guid = Reader.ReadGuid();
					if (Guid.Equals(Guid.Empty))
					{
						Reader.Position -= 20;
						break;
					}

					Prev2 = Prev;
					Prev = Pos;

					Len = (int)Reader.ReadVariableLengthUInt64();
					Reader.Position += Len;
				}
				while (Reader.BytesLeft >= 21);

				if (Prev2 == 0)
				{
					if (BlockIndex != 0 && AllowRotation)
					{
						Result = await this.RotateRightLocked(BlockIndex, Header.ParentBlockIndex);

						if (Result != null)
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
			BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new Storage.BlockHeader(Reader);
			byte[] Result = null;
			Guid Guid;
			uint Link;
			int Len;
			int Pos;
			int First = 0;
			int Second = 0;
			int c;

			do
			{
				Pos = Reader.Position;
				Link = Reader.ReadUInt32();
				if (Link != 0)
				{
					if (First == 0)
					{
						Result = await this.TryExtractSmallestObjectLocked(Link, AllowRotation, AllowMerge);

						if (Result == null && AllowMerge)
						{
							uint RightLink;

							First = Pos;
							Guid = Reader.ReadGuid();
							if (!Guid.Equals(Guid.Empty))
							{
								Len = (int)Reader.ReadVariableLengthUInt64();
								Reader.Position += Len;

								Second = Reader.Position;
								RightLink = Reader.ReadUInt32();
								Guid = Reader.ReadGuid();

								if (Guid.Equals(Guid.Empty))
									RightLink = Header.LastBlockIndex;

								c = Second - First - 4;
								byte[] Separator = new byte[c];
								Array.Copy(Block, First + 4, Separator, 0, c);

								MergeResult MergeResult = await this.MergeBlocksLocked(Link, Separator, RightLink);

								if (MergeResult.Separator == null)
								{
									this.RegisterEmptyBlockLocked(RightLink);

									c = Header.BytesUsed + BlockHeaderSize - Second - 4;
									if (c > 0)
										Array.Copy(Block, Second + 4, Block, First + 4, c);

									c = Second - First;
									Header.BytesUsed -= (ushort)c;
									Array.Clear(Block, Header.BytesUsed + BlockHeaderSize, c);
									Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);

									this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);
									this.QueueSaveBlockLocked(((long)Link) * this.blockSize, MergeResult.ResultBlock);

									await this.UpdateParentLinks(Link, MergeResult.ResultBlock);

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
					Guid = Reader.ReadGuid();
					if (Guid.Equals(Guid.Empty))
					{
						Reader.Position -= 20;
						break;
					}

					if (First == 0)
						First = Pos;
					else if (Second == 0)
					{
						Second = Pos;
						break;
					}

					Len = (int)Reader.ReadVariableLengthUInt64();
					Reader.Position += Len;
				}
			}
			while (Reader.BytesLeft >= 21);

			if (Second == 0)
			{
				if (BlockIndex != 0 && AllowRotation)
				{
					string s = await this.GetCurrentStateReportAsyncLocked(false);

					byte[] NewChild = await this.RotateLeftLocked(BlockIndex, Header.ParentBlockIndex);

					s = await this.GetCurrentStateReportAsyncLocked(false);

					if (NewChild != null)
					{
						c = Header.BytesUsed - 4;
						Result = new byte[c];
						Array.Copy(Block, First + 4, Result, 0, c);

						Array.Copy(NewChild, 0, Block, First + 4, NewChild.Length);

						c = c - NewChild.Length;
						if (c != 0)
						{
							if (c > 0)
								Array.Clear(Block, First + 4 + NewChild.Length, c);

							Header.BytesUsed -= (ushort)c;
							Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
						}

						this.QueueSaveBlockLocked(PhysicalPosition, Block);
					}

					s = await this.GetCurrentStateReportAsyncLocked(false);
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
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			int Pos, PrevPos, Len;
			uint BlockLink;
			uint PrevBlockLink;
			Guid Guid;
			bool IsEmpty;

			if (ChildIndex == Header.LastBlockIndex)
				return null;

			BlockLink = 0;
			Pos = 0;
			do
			{
				PrevBlockLink = BlockLink;
				PrevPos = Pos;

				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                  // Block link.
				Guid = Reader.ReadGuid();                         // Object ID of object.
				IsEmpty = Guid.Equals(Guid.Empty);
				if (IsEmpty)
					return null;
				/*{
					BlockLink = Header.LastBlockIndex;
					break;
				}*/

				Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
				Reader.Position += Len;
			}
			while (PrevBlockLink != ChildIndex && Reader.BytesLeft >= 21);

			if (PrevBlockLink != ChildIndex)
				return null;
			/*{
				PrevBlockLink = BlockLink;
				BlockLink = Header.LastBlockIndex;
				PrevPos = Pos;
				Pos = Reader.Position;

				if (PrevBlockLink != ChildIndex)
					return null;
			}*/

			string s = await this.GetCurrentStateReportAsyncLocked(false);

			bool Reshuffled = false;
			byte[] Object = await this.TryExtractSmallestObjectLocked(BlockLink, false, false);
			if (Object == null)
			{
				Reshuffled = true;
				Object = await this.TryExtractSmallestObjectLocked(BlockLink, true, false);
				if (Object == null)
				{
					Object = await this.TryExtractSmallestObjectLocked(BlockLink, false, true);
					if (Object == null)
						return null;
				}
			}

			s = await this.GetCurrentStateReportAsyncLocked(false);

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

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					IsEmpty = Guid.Equals(Guid.Empty);
					if (IsEmpty)
					{
						BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectGuid(Object));
						await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true);

						return null;
						/*BlockLink = Header.LastBlockIndex;
						break;*/
					}

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					Reader.Position += Len;
				}
				while (PrevBlockLink != ChildIndex && Reader.BytesLeft >= 21);

				if (PrevBlockLink != ChildIndex)
				{
					BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectGuid(Object));
					await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true);
					return null;
					/*PrevBlockLink = BlockLink;
					BlockLink = Header.LastBlockIndex;
					PrevPos = Pos;
					Pos = Reader.Position;*/
				}
			}

			int c;
			byte[] OldSeparator = new byte[c = Pos - PrevPos - 4];

			Array.Copy(Block, PrevPos + 4, OldSeparator, 0, c);

			s = await this.GetCurrentStateReportAsyncLocked(false);

			StringBuilder sb = new StringBuilder();
			this.ForEachObject(Block, (Link, ObjectId, Pos2, Len2) =>
			{
				sb.AppendLine(string.Format("{0} {1} {2} {3}", Link, ObjectId, Pos2, Len2));
				return true;
			});

			s = sb.ToString();

			await this.ReplaceObjectLocked(Object, new BlockInfo(Header, Block, BlockIndex, PrevPos));

			s = await this.GetCurrentStateReportAsyncLocked(false);

			return OldSeparator;
		}

		private Guid GetObjectGuid(byte[] Object)
		{
			byte[] b = new byte[16];
			Array.Copy(Object, 0, b, 0, 16);
			return new Guid(b);
		}

		private async Task<byte[]> RotateRightLocked(uint ChildIndex, uint BlockIndex)
		{
			long PhysicalPosition = ((long)BlockIndex) * this.blockSize;
			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, true);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			int Pos, PrevPos, Len;
			uint BlockLink;
			uint PrevBlockLink;
			Guid Guid;
			bool IsEmpty;

			BlockLink = 0;
			Pos = 0;
			do
			{
				PrevBlockLink = BlockLink;
				PrevPos = Pos;

				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                  // Block link.
				Guid = Reader.ReadGuid();                         // Object ID of object.
				IsEmpty = Guid.Equals(Guid.Empty);
				if (IsEmpty)
				{
					BlockLink = PrevBlockLink;
					Pos = PrevPos;
					Reader.Position -= 20;
					break;
				}

				Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
				Reader.Position += Len;
			}
			while (BlockLink != ChildIndex && Reader.BytesLeft >= 21);

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
			if (Object == null)
			{
				Reshuffled = true;
				Object = await this.TryExtractLargestObjectLocked(PrevBlockLink, true, false);
				if (Object == null)
				{
					Object = await this.TryExtractLargestObjectLocked(PrevBlockLink, false, true);
					if (Object == null)
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

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					IsEmpty = Guid.Equals(Guid.Empty);
					if (IsEmpty)
					{
						BlockLink = PrevBlockLink;
						Pos = PrevPos;
						Reader.Position -= 20;
						break;
					}

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					Reader.Position += Len;
				}
				while (BlockLink != ChildIndex && Reader.BytesLeft >= 21);

				if (BlockLink != ChildIndex)
				{
					PrevBlockLink = BlockLink;
					BlockLink = Header.LastBlockIndex;
					PrevPos = Pos;
					Pos = Reader.Position;

					if (BlockLink != ChildIndex)
					{
						BlockInfo Leaf = await this.FindLeafNodeLocked(this.GetObjectGuid(Object));
						await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Object, Leaf.InternalPosition, 0, 0, true);
						return null;
					}
				}
			}

			int c;
			byte[] OldSeparator = new byte[c = Pos - PrevPos - 4];

			Array.Copy(Block, PrevPos + 4, OldSeparator, 0, c);

			await this.ReplaceObjectLocked(Object, new BlockInfo(Header, Block, BlockIndex, PrevPos));

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
		/// <param name="Stat">If statistics is to be included in the report.</param>
		/// <returns>Report</returns>
		public string GetCurrentStateReport(bool WriteStat)
		{
			Task<string> Result = this.GetCurrentStateReportAsync(WriteStat);
			Result.Wait();
			return Result.Result;
		}

		/// <summary>
		/// Provides a report on the current state of the file.
		/// </summary>
		/// <param name="Stat">If statistics is to be included in the report.</param>
		/// <returns>Report</returns>
		public async Task<string> GetCurrentStateReportAsync(bool WriteStat)
		{
			await this.Lock();
			try
			{
				return await this.GetCurrentStateReportAsyncLocked(WriteStat);
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task<string> GetCurrentStateReportAsyncLocked(bool WriteStat)
		{
			StringBuilder Output = new StringBuilder();
			FileStatistics Statistics = await this.ComputeStatisticsLocked();

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
				await this.ExportGraphXMLLocked(Output);
				Output.AppendLine();
			}

			return Output.ToString();
		}

		/// <summary>
		/// Goes through the entire file and computes statistics abouts its composition.
		/// </summary>
		/// <returns>File statistics.</returns>
		public async Task<FileStatistics> ComputeStatistics()
		{
			await this.Lock();
			try
			{
				return await this.ComputeStatisticsLocked();
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task<FileStatistics> ComputeStatisticsLocked()
		{
			if (this.toSave != null)
				await this.SaveUnsaved();

			long FileSize = this.file.Length;
			int NrBlocks = (int)(FileSize / this.blockSize);

			byte[] Block = await this.LoadBlockLocked(0, false);
			BinaryDeserializer Reader = new Serialization.BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			FileStatistics Statistics = new FileStatistics((uint)this.blockSize, this.nrBlockLoads, this.nrCacheLoads, this.nrBlockSaves);
			BitArray BlocksReferenced = new BitArray(NrBlocks);

			try
			{
				await this.AnalyzeBlock(1, 0, 0, Statistics, BlocksReferenced, null, null);
			}
			catch (Exception ex)
			{
				Statistics.LogError(ex.Message);
			}

			return Statistics;
		}

		private async Task<ulong> AnalyzeBlock(uint Depth, uint ParentIndex, uint BlockIndex, FileStatistics Statistics,
			BitArray BlocksReferenced, Guid? MinExclusive, Guid? MaxExclusive)
		{
			if (BlockIndex >= BlocksReferenced.Count)
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
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);

			if (Header.ParentBlockIndex != ParentIndex)
			{
				Statistics.LogError("Parent link in block " + BlockIndex.ToString() + " invalid. Should point to " + ParentIndex.ToString() +
					" but points to " + Header.ParentBlockIndex.ToString() + ".");
			}

			Guid Guid;
			uint Len;
			int Pos = 14;
			uint BlockLink;
			ulong NrObjects = 0;
			uint ObjectsInBlock = 0;
			bool Leaf = true;
			Guid? MinGuid = null;
			Guid LastGuid = MinExclusive.HasValue ? MinExclusive.Value : Guid.Empty;
			int NrChildLinks = 0;
			int NrSeparators = 0;

			while (Reader.BytesLeft >= 21)
			{
				BlockLink = Reader.ReadUInt32();                    // Block link.
				Guid = Reader.ReadGuid();                           // Object ID of object.
				if (Guid.Equals(Guid.Empty))
					break;

				NrSeparators++;

				if (MinExclusive.HasValue && Guid.CompareTo(MinExclusive.Value) <= 0)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + ", contains an object with an Object ID (" + Guid.ToString() +
						") that is smaller or equal to the smallest allowed value (" + MinExclusive.ToString() + ").");
				}

				if (MaxExclusive.HasValue && Guid.CompareTo(MaxExclusive.Value) >= 0)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + ", contains an object with an Object ID (" + Guid.ToString() +
						") that is larger or equal to the largest allowed value (" + MaxExclusive.ToString() + ").");
				}

				if (LastGuid.CompareTo(Guid) >= 0)
					Statistics.LogError("Objects in block " + BlockIndex.ToString() + " are not sorted correctly.");

				LastGuid = Guid;

				ObjectsInBlock++;
				NrObjects++;
				if (BlockLink != 0)
				{
					NrChildLinks++;
					Leaf = false;
					NrObjects += await this.AnalyzeBlock(Depth + 1, BlockIndex, BlockLink, Statistics, BlocksReferenced, MinGuid, Guid);
				}

				Len = (uint)Reader.ReadVariableLengthUInt64();      // Remaining length of object.
				Statistics.ReportObjectStatistics((uint)(Reader.Position - Pos - 4 + Len));

				if (Len == 0)
					Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object of length 0.");
				else if (Len > Reader.BytesLeft)
				{
					Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object of length " + Len.ToString() + ", which does not fit in the block.");
					break;
				}
				else
				{
					Reader.Position += (int)Len;

					int Len2;

					try
					{
						int Pos2 = Reader.Position;
						Reader.Position = Pos + 4;
						object Obj = this.genericSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false, false);
						Len2 = Pos2 - Reader.Position;
					}
					catch (Exception ex)
					{
						Statistics.LogError(ex.Message);
						Len2 = 0;
					}

					if (Len2 != 0)
					{
						Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object (" + Guid.ToString() +
							") that is not serialized correctly.");
						break;
					}
				}

				MinGuid = Guid;
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
				NrObjects += await this.AnalyzeBlock(Depth + 1, BlockIndex, Header.LastBlockIndex, Statistics, BlocksReferenced, MinGuid, null);
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
		/// <param name="Output">XML Output</param>
		/// <returns>Graph XML.</returns>
		public async Task<string> ExportGraphXML()
		{
			StringBuilder Output = new StringBuilder();
			await this.ExportGraphXML(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXML(StringBuilder Output)
		{
			await this.Lock();
			try
			{
				await this.ExportGraphXMLLocked(Output);
			}
			finally
			{
				await this.Release();
			}
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXMLLocked(StringBuilder Output)
		{
			XmlWriterSettings Settings = new XmlWriterSettings();
			Settings.CloseOutput = false;
			Settings.ConformanceLevel = ConformanceLevel.Document;
			Settings.Encoding = System.Text.Encoding.UTF8;
			Settings.Indent = true;
			Settings.IndentChars = "\t";
			Settings.NewLineChars = "\r\n";
			Settings.NewLineHandling = NewLineHandling.Entitize;
			Settings.NewLineOnAttributes = false;
			Settings.OmitXmlDeclaration = true;

			using (XmlWriter w = XmlWriter.Create(Output, Settings))
			{
				await this.ExportGraphXMLLocked(w);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="XmlOutput">XML Output</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXML(XmlWriter XmlOutput)
		{
			await this.Lock();
			try
			{
				await this.ExportGraphXMLLocked(XmlOutput);
			}
			finally
			{
				await this.Release();
			}
		}

		/// <summary>
		/// Exports the structure of the file to XML.
		/// </summary>
		/// <param name="XmlOutput">XML Output</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXMLLocked(XmlWriter XmlOutput)
		{
			long NrBlocks = this.file.Length / this.blockSize;

			XmlOutput.WriteStartElement("FileStructure", "http://waher.se/ObjectBTreeFile.xsd");
			XmlOutput.WriteAttributeString("fileName", this.fileName);

			await this.ExportGraphXMLLocked(0, XmlOutput);

			XmlOutput.WriteEndElement();
		}

		private async Task ExportGraphXMLLocked(uint BlockIndex, XmlWriter XmlOutput)
		{
			long PhysicalPosition = BlockIndex;
			PhysicalPosition *= this.blockSize;

			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, false);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			Guid Guid;
			int Pos;
			uint Len;
			uint BlockLink;

			XmlOutput.WriteStartElement("Block");
			XmlOutput.WriteAttributeString("index", BlockIndex.ToString());
			XmlOutput.WriteAttributeString("bytes", Header.BytesUsed.ToString());
			XmlOutput.WriteAttributeString("size", Header.SizeSubtree.ToString());

			while (Reader.BytesLeft >= 21)
			{
				Pos = Reader.Position;
				BlockLink = Reader.ReadUInt32();                    // Block link.
				Guid = Reader.ReadGuid();                           // Object ID of object.
				if (Guid.Equals(Guid.Empty))
					break;

				Len = (uint)Reader.ReadVariableLengthUInt64();      // Remaining length of object.
				Reader.Position += (int)Len;

				Len = (uint)(Reader.Position - Pos);

				XmlOutput.WriteStartElement("Object");
				XmlOutput.WriteAttributeString("id", Guid.ToString());
				XmlOutput.WriteAttributeString("pos", Pos.ToString());
				XmlOutput.WriteAttributeString("len", Len.ToString());

				if (BlockLink != 0)
					await this.ExportGraphXMLLocked(BlockLink, XmlOutput);

				XmlOutput.WriteEndElement();
			}

			if (Header.LastBlockIndex != 0)
				await this.ExportGraphXMLLocked(Header.LastBlockIndex, XmlOutput);

			XmlOutput.WriteEndElement();
		}

		#endregion

		#region Order Statistic Tree

		/// <summary>
		/// Get number of objects in subtree spanned by <param name="BlockIndex">BlockIndex</param>.
		/// </summary>
		/// <param name="BlockIndex">Block index of root of subtree.</param>
		/// <param name="IncludeChildren">If objects in children are to be included in count.</param>
		/// <returns>Total number of objects in subtree.</returns>
		public async Task<ulong> GetObjectCount(uint BlockIndex, bool IncludeChildren)
		{
			await this.Lock();
			try
			{
				return await this.GetObjectCountLocked(BlockIndex, IncludeChildren);
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task<ulong> GetObjectCountLocked(uint BlockIndex, bool IncludeChildren)
		{
			byte[] Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, false);
			uint BlockSize;

			if (!IncludeChildren)
			{
				BlockSize = 0;

				this.ForEachObject(Block, (Link, ObjectId, Pos2, Len2) =>
				{
					BlockSize++;
					return true;
				});

				return BlockSize;
			}

			BlockSize = BitConverter.ToUInt32(Block, 2);
			if (BlockSize < uint.MaxValue)
				return BlockSize;

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			Guid Guid;
			uint Len;
			int Pos;
			uint BlockLink;
			ulong NrObjects = 0;

			while (Reader.BytesLeft >= 21)
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                    // Block link.
				Guid = Reader.ReadGuid();                           // Object ID of object.
				if (Guid.Equals(Guid.Empty))
					break;

				NrObjects++;
				if (IncludeChildren && BlockLink != 0)
					NrObjects += await this.GetObjectCountLocked(BlockLink, IncludeChildren);

				Len = (uint)Reader.ReadVariableLengthUInt64();      // Remaining length of object.
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
		/// <exception cref="IOException">If the object is not found.</exception>
		public async Task<ulong> GetRank(Guid ObjectId)
		{
			await this.Lock();
			try
			{
				return await GetRankLocked(ObjectId);
			}
			finally
			{
				await this.Release();
			}
		}

		internal async Task<ulong> GetRankLocked(Guid ObjectId)
		{
			BlockInfo Info = await this.FindNodeLocked(ObjectId);
			if (Info == null)
				throw new IOException("Object not found.");

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, BlockHeaderSize);
			ulong Rank = 0;
			int Len;
			int Pos;
			int c;
			uint BlockLink;
			Guid Guid;
			bool IsEmpty;

			do
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                  // Block link.
				Guid = Reader.ReadGuid();                         // Object ID of object.
				IsEmpty = Guid.Equals(Guid.Empty);
				if (IsEmpty)
					break;

				if (BlockLink != 0)
					Rank += await this.GetObjectSizeOfBlock(BlockLink);

				Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
				c = Reader.Position - Pos + Len;

				if (Guid.Equals(ObjectId))
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
							Pos = Reader.Position;

							BlockLink = Reader.ReadUInt32();                  // Block link.
							if (BlockLink == BlockIndex)
								break;

							Guid = Reader.ReadGuid();                         // Object ID of object.
							IsEmpty = Guid.Equals(Guid.Empty);
							if (IsEmpty)
								break;

							if (BlockLink != 0)
								Rank += await this.GetObjectSizeOfBlock(BlockLink);

							Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
							c = Reader.Position - Pos + Len;

							Rank++;
							Reader.Position += Len;
						}
						while (Reader.BytesLeft >= 21);

						BlockIndex = ParentIndex;
					}

					return Rank;
				}

				Rank++;
				Reader.Position += Len;
			}
			while (Reader.BytesLeft >= 21);

			throw new IOException("Object not found.");
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
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Checks if an item is stored in the file.
		/// </summary>
		/// <param name="Item">Object to check for.</param>
		/// <returns>If the object is stored in the file.</returns>
		public async Task<bool> ContainsAsync(object Item)
		{
			if (Item == null)
				return false;

			ObjectSerializer Serializer = this.provider.GetObjectSerializer(Item.GetType()) as ObjectSerializer;
			if (Serializer == null)
				return false;

			if (!Serializer.HasObjectId(Item))
				return false;

			Guid ObjectId = Serializer.GetObjectId(Item, false);
			GenericObject Obj;

			await this.Lock();
			try
			{
				Obj = await this.LoadObject(ObjectId, this.genericSerializer) as GenericObject;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				await this.Release();
			}

			if (Obj == null)
				return false;

			try
			{
				BinarySerializer Writer = new BinarySerializer(this.collectionName, this.encoding);
				Serializer.Serialize(Writer, false, false, Item);
				byte[] Bin = Writer.GetSerialization();

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Bin);
				GenericObject Obj2 = this.genericSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false) as GenericObject;
				if (Obj2 == null)
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
				Task.Wait();

				ulong Result = Task.Result;
				if (Result > int.MaxValue)
					throw new OverflowException("File contains " + Result.ToString() + " objects, which is more than can be represented by an Int32 value. Use the GetObjectCount() method instead of the Count property.");

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
			this.ClearAsync().Wait();
		}

		/// <summary>
		/// Clears the database of all objects.
		/// </summary>
		/// <returns>Task object.</returns>
		public async Task ClearAsync()
		{
			await this.Lock();
			try
			{
				this.file.Dispose();
				this.file = null;

				this.blocks.Clear();

				File.Delete(FileName);
				this.file = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
				await this.CreateNewBlockLocked();
			}
			finally
			{
				await this.Release();
			}
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<object> GetEnumerator()
		{
			return new ObjectBTreeFileEnumerator<object>(this, false);
		}

		/// <summary>
		/// Returns an untyped enumerator that iterates through the collection.
		/// 
		/// For a typed enumerator, call the <see cref="GetTypedEnumerator{T}(bool)"/> method.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ObjectBTreeFileEnumerator<object>(this, false);
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
		public ObjectBTreeFileEnumerator<T> GetTypedEnumerator<T>(bool Locked)
		{
			return new ObjectBTreeFileEnumerator<T>(this, false);
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
				this.DeleteObject(item).Wait();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion

	}
}

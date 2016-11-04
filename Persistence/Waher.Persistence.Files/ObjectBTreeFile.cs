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
		private byte[] aesKey;
		private byte[] p;
		private string fileName;
		private string collectionName;
		private string blobFolder;
		private int blockSize;
		private int timeoutMilliseconds;
		private bool isCorrupt = false;
		private bool encypted;

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
				CspParameters cspParams = new CspParameters();
				cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
				cspParams.KeyContainerName = this.fileName;

				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspParams);
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

		private async Task CreateFirstBlock()
		{
			await this.Lock();
			try
			{
				this.CreateNewBlockLocked();
			}
			finally
			{
				await this.Release();
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

		private async Task Lock()
		{
			if (!await this.fileAccessSemaphore.WaitAsync(this.timeoutMilliseconds))
				throw new TimeoutException("Unable to get access to underlying database.");
		}

		private async Task Release()
		{
			if (this.toSave != null)
			{
				foreach (KeyValuePair<long, byte[]> Rec in this.toSave)
					await this.DoSaveBlockLocked(Rec.Key, Rec.Value);

				this.toSave.Clear();
				this.bytesAdded = 0;
			}

			this.fileAccessSemaphore.Release();
		}

		#endregion

		#region Blocks

		private Tuple<uint, byte[]> CreateNewBlockLocked()
		{
			byte[] Block = new byte[this.blockSize];
			long PhysicalPosition = this.file.Length + this.bytesAdded;

			this.bytesAdded += this.blockSize;
			this.QueueSaveBlockLocked(PhysicalPosition, Block);

			return new Tuple<uint, byte[]>((uint)(PhysicalPosition / this.blockSize), Block);
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

		private async Task<byte[]> LoadBlockLocked(long PhysicalPosition, bool AddToCache)
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

			if (this.blockSize != await this.file.ReadAsync(Block, 0, this.blockSize))
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

		private void QueueSaveBlockLocked(long PhysicalPosition, byte[] Block)
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

			this.blocks.Add(PhysicalPosition, Block);
		}

		private async Task DoSaveBlockLocked(long PhysicalPosition, byte[] Block)
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
		public Task<Guid> SaveNew(object Object)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = this.provider.GetObjectSerializer(ObjectType) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Cannot store store objects of type " + ObjectType.FullName + " directly. They need to be embedded.");

			return this.SaveNew(Object, Serializer);
		}

		/// <summary>
		/// Saves a new object to the file.
		/// </summary>
		/// <param name="Object">Object to persist.</param>
		/// <param name="Serializer">Object serializer. If not provided, the serializer registered for the corresponding type will be used.</param>
		public async Task<Guid> SaveNew(object Object, ObjectSerializer Serializer)
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
					Leaf = await this.FindLeafNode(ObjectId);

					if (Leaf == null)
						throw new IOException("Object with same Object ID already exists.");
				}
				else
				{
					do
					{
						ObjectId = CreateDatabaseGUID();
					}
					while ((Leaf = await this.FindLeafNode(ObjectId)) == null);

					if (!Serializer.TrySetObjectId(Object, ObjectId))
						throw new NotSupportedException("Unable to set Object ID: Unsupported type.");
				}

				Writer = new BinarySerializer(this.collectionName, this.encoding);
				Serializer.Serialize(Writer, false, false, Object);
				Bin = Writer.GetSerialization();

				// TODO: BLOBs: Objects so large that not two fits in the same block, must be stored separately, and encoded as a BLOB. Include count & size in statistics. Also list BLOB files that are not referenced.

				await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0, 0);

				return ObjectId;
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task InsertObjectLocked(uint BlockIndex, BlockHeader Header, byte[] Block, byte[] Bin, int InsertAt,
			uint ChildRightLink, uint ChildRightLinkSize)
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
				if (Header.SizeSubtree < uint.MaxValue)
					Header.SizeSubtree++;

				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
				Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

				this.QueueSaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);

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
			else                                                                    // Split node.
			{
				BlockSplitter Splitter = new BlockSplitterLast(this.blockSize);         // Since GUIDs are mostly an increasing sequence, we put as many nodes into the left child node as possible.
																						//BlockSplitter Splitter = new BlockSplitterMiddle(PayloadSize);
				Tuple<uint, byte[]> Left;
				Tuple<uint, byte[]> Right = this.CreateNewBlockLocked();
				uint LeftLink;
				uint RightLink = Right.Item1;
				bool CheckParentLinksLeft = false;
				bool CheckParentLinksRight = true;

				Splitter.RightBlock = Right.Item2;

				if (BlockIndex == 0)   // Create new root
				{
					Left = this.CreateNewBlockLocked();
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
						Splitter.RightSizeSubtree);
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

		private async Task<BlockInfo> FindLeafNode(Guid ObjectId)
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
			BlockInfo Info = await this.FindNode(ObjectId);
			if (Info == null)
				throw new IOException("Object not found.");

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Info.Block, Info.InternalPosition + 4);

			if (Serializer == null)
			{
				Reader.Position += 16;

				Reader.ReadVariableLengthUInt64();
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

		private async Task<BlockInfo> FindNode(Guid ObjectId)
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

		#region Statistics

		public async Task<FileStatistics> ComputeStatistics()
		{
			await this.Lock();
			try
			{
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
			finally
			{
				await this.Release();
			}
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
			int Pos;
			uint BlockLink;
			ulong NrObjects = 0;
			uint ObjectsInBlock = 0;
			bool Leaf = true;
			Guid? MinGuid = null;
			Guid LastGuid = MinExclusive.HasValue ? MinExclusive.Value : Guid.Empty;

			while (Reader.BytesLeft >= 21)
			{
				Pos = Reader.Position;

				BlockLink = Reader.ReadUInt32();                    // Block link.
				Guid = Reader.ReadGuid();                           // Object ID of object.
				if (Guid.Equals(Guid.Empty))
				{
					if (Header.BytesUsed != Pos - BlockHeaderSize)
					{
						Statistics.LogError("Number of bytes used as recorded in block " + BlockIndex.ToString() +
							" is wrong. Reported size: " + Header.BytesUsed.ToString() + ", Actual size: " + Pos.ToString());
					}

					break;
				}

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

					// TODO: Deserialize, when field names are persisted
					/*
					int Len2;

					try
					{
						int Pos2 = Reader.Position;
						object Obj = this.genericSerializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, true);
						Len2 = Reader.Position - Pos2;
					}
					catch (Exception ex)
					{
						Statistics.LogError(ex.Message);
						Len2 = 0;
					}

					if (Len != Len2)
					{
						Statistics.LogError("Block " + BlockIndex.ToString() + " contains an object (" + Guid.ToString() +
							") that is not serialized correctly.");
						break;
					}*/
				}

				MinGuid = Guid;
			}

			if (Header.LastBlockIndex != 0)
			{
				Leaf = false;
				NrObjects += await this.AnalyzeBlock(Depth + 1, BlockIndex, Header.LastBlockIndex, Statistics, BlocksReferenced, MinGuid, null);
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
		/// <param name="XmlOutput">XML Output</param>
		/// <returns>Asynchronous task object.</returns>
		public async Task ExportGraphXML(XmlWriter XmlOutput)
		{
			await this.Lock();
			try
			{
				long NrBlocks = this.file.Length / this.blockSize;

				XmlOutput.WriteStartElement("FileStructure", "http://waher.se/ObjectBTreeFile.xsd");
				XmlOutput.WriteAttributeString("fileName", this.fileName);

				await this.ExportGraphXML(0, XmlOutput);

				XmlOutput.WriteEndElement();
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task ExportGraphXML(uint BlockIndex, XmlWriter XmlOutput)
		{
			long PhysicalPosition = BlockIndex;
			PhysicalPosition *= this.blockSize;

			byte[] Block = await this.LoadBlockLocked(PhysicalPosition, false);
			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);
			BlockHeader Header = new BlockHeader(Reader);
			Guid Guid;
			uint Len;
			uint BlockLink;

			XmlOutput.WriteStartElement("Block");
			XmlOutput.WriteAttributeString("index", BlockIndex.ToString());
			XmlOutput.WriteAttributeString("bytes", Header.BytesUsed.ToString());
			XmlOutput.WriteAttributeString("size", Header.SizeSubtree.ToString());

			while (Reader.BytesLeft >= 21)
			{
				BlockLink = Reader.ReadUInt32();                    // Block link.
				Guid = Reader.ReadGuid();                           // Object ID of object.
				if (Guid.Equals(Guid.Empty))
					break;

				XmlOutput.WriteStartElement("Object");
				XmlOutput.WriteAttributeString("id", Guid.ToString());

				if (BlockLink != 0)
					await this.ExportGraphXML(BlockLink, XmlOutput);

				XmlOutput.WriteEndElement();

				Len = (uint)Reader.ReadVariableLengthUInt64();      // Remaining length of object.
				Reader.Position += (int)Len;
			}

			if (Header.LastBlockIndex != 0)
				await this.ExportGraphXML(Header.LastBlockIndex, XmlOutput);

			XmlOutput.WriteEndElement();
		}

		#endregion

		#region ICollection<object>

		/// <summary>
		/// <see cref="ICollection{Object}.Add(Object)"/>
		/// </summary>
		public void Add(object item)
		{
			this.SaveNew(item);
		}

		/// <summary>
		/// <see cref="ICollection{Object}.Contains(Object)"/>
		/// </summary>
		public bool Contains(object item)
		{
			Task<bool> Task = this.ContainsObject(item);
			Task.Wait();
			return Task.Result;
		}

		/// <summary>
		/// Checks if an item is stored in the file.
		/// </summary>
		/// <param name="Item">Object to check for.</param>
		/// <returns>If the object is stored in the file.</returns>
		public async Task<bool> ContainsObject(object Item)
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
				BinarySerializer Writer = new BinarySerializer(this.collectionName, Encoding.UTF8);
				Serializer.Serialize(Writer, false, false, Item);
				byte[] Bin = Writer.GetSerialization();

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, Encoding.UTF8, Bin);
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
				Task<ulong> Task = this.GetTotalObjectCount(0);
				Task.Wait();

				ulong Result = Task.Result;
				if (Result > int.MaxValue)
					throw new OverflowException("File contains " + Result.ToString() + " objects, which is more than can be represented by an Int32 value. Use the GetTotalObjectCount() method instead of the Count property.");

				return (int)Result;
			}
		}

		/// <summary>
		/// Get number of objects in subtree spanned by <param name="BlockIndex">BlockIndex</param>.
		/// </summary>
		/// <param name="BlockIndex">Block index of root of subtree.</param>
		/// <returns>Total number of objects in subtree.</returns>
		public async Task<ulong> GetTotalObjectCount(uint BlockIndex)
		{
			await this.Lock();
			try
			{
				return await this.GetTotalObjectCountLocked(BlockIndex);
			}
			finally
			{
				await this.Release();
			}
		}

		private async Task<ulong> GetTotalObjectCountLocked(uint BlockIndex)
		{ 
			byte[] Block = await this.LoadBlockLocked(((long)BlockIndex) * this.blockSize, false);
			uint BlockSize = BitConverter.ToUInt32(Block, 2);
			if (BlockSize < uint.MaxValue)
				return BlockSize;

			BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, Encoding.UTF8, Block);
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
				if (BlockLink != 0)
					NrObjects += await this.GetTotalObjectCountLocked(BlockLink);

				Len = (uint)Reader.ReadVariableLengthUInt64();      // Remaining length of object.
				Reader.Position += (int)Len;
			}

			if (Header.LastBlockIndex != 0)
				NrObjects += await this.GetTotalObjectCountLocked(Header.LastBlockIndex);

			return NrObjects;
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

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Remove(object item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<object> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Cache;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a binary encrypted file where objects are persisted in a B-tree.
	/// </summary>
	public class ObjectBTreeFile : IDisposable
	{
		private const int BlockHeaderSize = 14;

		private FilesProvider provider;
		private AesCryptoServiceProvider aes;
		private Cache<long, byte[]> blocks;
		private FileStream file;
		private Encoding encoding;
		private SemaphoreSlim fileAccessSemaphore = new SemaphoreSlim(1, 1);
		private byte[] aesKey;
		private byte[] p;
		private string fileName;
		private string collectionName;
		private string blobFolder;
		private int blockSize;
		private int timeoutMilliseconds;
		private bool isCorrupt = false;

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
		public ObjectBTreeFile(string FileName, string CollectionName, string BlobFolder, int BlockSize, int BlocksInCache,
			FilesProvider Provider, Encoding Encoding, int TimeoutMilliseconds)
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

			string Folder = Path.GetDirectoryName(FileName);
			if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			this.blocks = new Cache<long, byte[]>(BlocksInCache, TimeSpan.MaxValue, new TimeSpan(0, 1, 0, 0, 0));

			if (File.Exists(FileName))
				this.file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			else
			{
				this.file = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
				Task Task = this.CreateNewBlockLocked();
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

		#region Locks

		private async Task Lock()
		{
			if (!await this.fileAccessSemaphore.WaitAsync(this.timeoutMilliseconds))
				throw new IOException("Unable to get access to underlying database.");
		}

		private void Release()
		{
			this.fileAccessSemaphore.Release();
		}

		#endregion

		#region Blocks

		private async Task<Tuple<uint, byte[]>> CreateNewBlockLocked()
		{
			byte[] Block = new byte[this.blockSize];
			long PhysicalPosition = this.file.Length;

			await this.SaveBlockLocked(PhysicalPosition, Block);

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
			if ((PhysicalPosition % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "PhysicalPosition");

			await this.Lock();
			try
			{
				return await this.LoadBlockLocked(PhysicalPosition);
			}
			finally
			{
				this.Release();
			}
		}

		private async Task<byte[]> LoadBlockLocked(long PhysicalPosition)
		{
			byte[] Block;

			if (this.blocks.TryGetValue(PhysicalPosition, out Block))
				return Block;

			if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
				throw new ArgumentException("Invalid file position.", "Position");

			Block = new byte[this.blockSize];

			if (this.blockSize != await this.file.ReadAsync(Block, 0, this.blockSize))
				throw new IOException("Read past end of file.");

			using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(PhysicalPosition)))
			{
				Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
			}

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
			if ((PhysicalPosition % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "PhysicalPosition");

			if (Block == null || Block.Length != this.blockSize)
				throw new ArgumentException("Block not of the correct block size.", "Block");

			await this.Lock();
			try
			{
				await this.SaveBlockLocked(PhysicalPosition, Block);
			}
			finally
			{
				this.Release();
			}
		}

		private async Task SaveBlockLocked(long PhysicalPosition, byte[] Block)
		{
			byte[] EncryptedBlock;

			using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(PhysicalPosition)))
			{
				EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
			}

			if (PhysicalPosition != this.file.Seek(PhysicalPosition, SeekOrigin.Begin))
				throw new ArgumentException("Invalid file position.", "Position");

			await this.file.WriteAsync(EncryptedBlock, 0, this.blockSize);

			this.blocks.Add(PhysicalPosition, Block);
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
		public async Task<Guid> SaveNew(object Object)
		{
			Type ObjectType = Object.GetType();
			ObjectSerializer Serializer = this.provider.GetObjectSerializer(ObjectType) as ObjectSerializer;
			if (Serializer == null)
				throw new Exception("Cannot store store objects of type " + ObjectType.FullName + " directly. They need to be embedded.");

			if (Serializer.HasObjectId(Object))
				throw new Exception("Object has already been saved.");

			await this.Lock();
			try
			{
				BlockInfo Leaf;
				BinarySerializer Writer;
				Guid ObjectId;
				byte[] Bin;

				do
				{
					ObjectId = Guid.NewGuid();
				}
				while ((Leaf = await this.FindLeafNode(ObjectId)) == null);

				if (!Serializer.TrySetObjectId(Object, ObjectId))
					throw new NotSupportedException("Unable to set Object ID: Unsupported type.");

				Writer = new BinarySerializer(this.collectionName, this.encoding);
				Serializer.Serialize(Writer, false, false, Object);
				Bin = Writer.GetSerialization();

				// TODO: BLOBs: Objects so large that not two fits in the same block, must be stored separately, and encoded as a BLOB.

				await this.InsertObjectLocked(Leaf.BlockIndex, Leaf.Header, Leaf.Block, Bin, Leaf.InternalPosition, 0);

				return ObjectId;
			}
			finally
			{
				this.Release();
			}
		}

		private async Task InsertObjectLocked(uint BlockIndex, BlockHeader Header, byte[] Block, byte[] Bin, int InsertAt, uint ChildRightLink)
		{
			uint Used = Header.BytesUsed;
			int PayloadSize = (int)(Header.BytesUsed + 4 + Bin.Length);

			if (BlockHeaderSize + PayloadSize <= this.blockSize)      // Add object to current node
			{
				if (InsertAt < Used)
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
				Header.SizeSubtree++;
				Array.Copy(BitConverter.GetBytes(Header.BytesUsed), 0, Block, 0, 2);
				Array.Copy(BitConverter.GetBytes(Header.SizeSubtree), 0, Block, 2, 4);

				await this.SaveBlockLocked(((long)BlockIndex) * this.blockSize, Block);

				// TODO: Inc size for all parent nodes.
			}
			else                                                                    // Split node.
			{
				int MiddlePos = PayloadSize / 2;    // Instead of mean value, select the value residing in the middle of the block. These are not the same, since object values might be of different sizes.
				Tuple<uint, byte[]> Left;
				Tuple<uint, byte[]> Right = await this.CreateNewBlockLocked();
				uint LeftLink;
				uint RightLink = Right.Item1;
				byte[] LeftBlock;
				byte[] RightBlock = Right.Item2;

				if (BlockIndex == 0)   // Create new root
				{
					Left = await this.CreateNewBlockLocked();
					LeftLink = Left.Item1;
					LeftBlock = Left.Item2;
				}
				else                        // Reuse current node for new left node.
				{
					Left = null;
					LeftLink = BlockIndex;
					LeftBlock = new byte[this.blockSize];
				}

				byte[] MiddleObject = null;
				ushort LeftBytesUsed = 0;
				uint LeftSizeSubtree = 0;
				int LeftPos = BlockHeaderSize;
				ushort RightBytesUsed = 0;
				uint RightSizeSubtree = 0;
				int RightPos = BlockHeaderSize;
				int Len;
				int Pos;
				int c;
				uint BlockLink;
				Guid Guid;
				bool IsEmpty;

				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);

				Array.Copy(Block, 6, RightBlock, 6, 4);     // Right last link = last link of original node.

				Len = 0;
				do
				{
					Reader.Position += Len;
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					IsEmpty = Guid.Equals(Guid.Empty);
					if (IsEmpty)
					{
						if (Pos == InsertAt)
							Array.Copy(BitConverter.GetBytes(ChildRightLink), 0, RightBlock, 6, 4);

						break;
					}

					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.
					c = Reader.Position - Pos - 4 + Len;

					if (Pos < MiddlePos)
					{
						if (Pos == InsertAt)
						{
							Array.Copy(Block, Pos, LeftBlock, LeftPos, 4);
							LeftPos += 4;
							Array.Copy(Bin, 0, LeftBlock, LeftPos, Bin.Length);
							LeftPos += Bin.Length;
							LeftSizeSubtree++;

							Array.Copy(BitConverter.GetBytes(ChildRightLink), 0, LeftBlock, LeftPos, 4);
							LeftPos += 4;
							Array.Copy(Block, Pos + 4, LeftBlock, LeftPos, c - 4);
							LeftPos += c - 4;
						}
						else
						{
							Array.Copy(Block, Pos, LeftBlock, LeftPos, c);
							LeftPos += c;
						}

						LeftSizeSubtree++;
					}
					else if (MiddleObject == null)
					{
						if (Pos == InsertAt)
							MiddleObject = Bin;
						else
						{
							MiddleObject = new byte[c];
							Array.Copy(Block, Pos + 4, MiddleObject, 0, c - 4);
						}
					}
					else
					{
						if (Pos == InsertAt)
						{
							Array.Copy(Block, Pos, RightBlock, RightPos, 4);
							RightPos += 4;
							Array.Copy(Bin, 0, RightBlock, RightPos, Bin.Length);
							RightPos += Bin.Length;
							RightSizeSubtree++;

							Array.Copy(BitConverter.GetBytes(ChildRightLink), 0, RightBlock, RightPos, 4);
							RightPos += 4;
							Array.Copy(Block, Pos + 4, RightBlock, RightPos, c - 4);
							RightPos += c - 4;
						}
						else
						{
							Array.Copy(Block, Pos, RightBlock, RightPos, c);
							RightPos += c;
						}

						RightSizeSubtree++;
					}
				}
				while (Reader.BytesLeft >= 22);

				LeftBytesUsed = (ushort)(LeftPos - BlockHeaderSize);
				RightBytesUsed = (ushort)(RightPos - BlockHeaderSize);

				uint ParentLink;

				if (BlockIndex == 0)
				{
					ushort NewParentBytesUsed = (ushort)(4 + MiddleObject.Length);
					uint NewParentSizeSubtree = 1 + LeftSizeSubtree + RightSizeSubtree;
					byte[] NewParentBlock = new byte[this.blockSize];

					Array.Copy(BitConverter.GetBytes(NewParentBytesUsed), 0, NewParentBlock, 0, 2);
					Array.Copy(BitConverter.GetBytes(NewParentSizeSubtree), 0, NewParentBlock, 2, 4);
					Array.Copy(BitConverter.GetBytes(RightLink), 0, NewParentBlock, 6, 4);
					Array.Copy(BitConverter.GetBytes(LeftLink), 0, NewParentBlock, 14, 4);
					Array.Copy(MiddleObject, 0, NewParentBlock, 18, MiddleObject.Length);

					await this.SaveBlockLocked(0, NewParentBlock);

					ParentLink = 0;
				}
				else
				{
					ParentLink = BlockIndex;

					long PhysicalPosition = ParentLink;
					PhysicalPosition *= this.blockSize;

					byte[] ParentBlock = await this.LoadBlockLocked(PhysicalPosition);
					BinaryDeserializer ParentReader = new BinaryDeserializer(this.collectionName, this.encoding, ParentBlock);

					BlockHeader ParentHeader = new Storage.BlockHeader(ParentReader);
					int ParentLen;
					int ParentPos;
					uint ParentBlockLink;

					if (ParentHeader.LastBlockLink == LeftLink)
						ParentPos = BlockHeaderSize + ParentHeader.BytesUsed;
					else
					{
						ParentBlockLink = 0;
						ParentLen = 0;
						do
						{
							ParentReader.Position += ParentLen;
							ParentPos = ParentReader.Position;

							ParentBlockLink = ParentReader.ReadUInt32();                  // Block link.
							ParentReader.Position += 16;
							ParentLen = (int)ParentReader.ReadVariableLengthUInt64();     // Remaining length of object.
						}
						while (ParentBlockLink != LeftLink && ParentReader.BytesLeft >= 22);

						if (ParentBlockLink != LeftLink)
						{
							this.isCorrupt = true;
							throw new IOException("Link not found.");
						}
					}

					await InsertObjectLocked(ParentLink, ParentHeader, ParentBlock, MiddleObject, ParentPos, RightLink);
				}

				Array.Copy(BitConverter.GetBytes(LeftBytesUsed), 0, LeftBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(LeftSizeSubtree), 0, LeftBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, LeftBlock, 10, 4);

				Array.Copy(BitConverter.GetBytes(RightBytesUsed), 0, RightBlock, 0, 2);
				Array.Copy(BitConverter.GetBytes(RightSizeSubtree), 0, RightBlock, 2, 4);
				Array.Copy(BitConverter.GetBytes(ParentLink), 0, RightBlock, 10, 4);

				await this.SaveBlockLocked(LeftLink, LeftBlock);
				await this.SaveBlockLocked(RightLink, RightBlock);
			}
		}

		private async Task<BlockInfo> FindLeafNode(Guid ObjectId)
		{
			uint BlockPosition = 0;

			while (true)
			{
				long PhysicalPosition = BlockPosition;
				PhysicalPosition *= this.blockSize;

				byte[] Block = await this.LoadBlockLocked(PhysicalPosition);
				BinaryDeserializer Reader = new BinaryDeserializer(this.collectionName, this.encoding, Block);

				BlockHeader Header = new Storage.BlockHeader(Reader);
				Guid Guid;
				int Len;
				int Pos;
				uint BlockLink;
				int Comparison;
				bool IsEmpty;

				Len = 0;
				do
				{
					Reader.Position += Len;
					Pos = Reader.Position;

					BlockLink = Reader.ReadUInt32();                  // Block link.
					Guid = Reader.ReadGuid();                         // Object ID of object.
					Len = (int)Reader.ReadVariableLengthUInt64();     // Remaining length of object.

					IsEmpty = Guid.Equals(Guid.Empty);
					Comparison = ObjectId.CompareTo(Guid);
				}
				while (!IsEmpty && Comparison >= 0 && Reader.BytesLeft >= 22);

				if (Comparison == 0)                                       // Object ID already found.
					return null;
				else if (IsEmpty || Comparison >= 0)
				{
					if (Header.LastBlockLink == 0)
						return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos);
					else
						BlockPosition = Header.LastBlockLink;
				}
				else
				{
					if (BlockLink == 0)
						return new BlockInfo(Header, Block, (uint)(PhysicalPosition / this.blockSize), Pos);
					else
						BlockPosition = BlockLink;
				}
			}
		}


		#endregion


	}
}

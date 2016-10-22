using System;
using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Cache;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// This class manages a binary encrypted file where objects are persisted in a B-tree.
	/// </summary>
	public class ObjectBTreeFile : IDisposable
	{
		private AesCryptoServiceProvider aes;
		private Cache<long, byte[]> blocks;
		private FileStream file;
		private object fileAccessSynchObj = new object();
		private byte[] aesKey;
		private byte[] p;
		private string fileName;
		private string blobFolder;
		private int blockSize;

		/// <summary>
		/// This class manages a binary encrypted file where objects are persisted in a B-tree.
		/// </summary>
		/// <param name="FileName">Name of binary file. File will be created if it does not exist. The class will require
		/// unique read/write access to the file.</param>
		/// <param name="BlobFolder">Folder in which BLOBs are stored.</param>
		/// <param name="BlockSize">Size of a block in the B-tree. The size must be a power of two, and should be at least the same
		/// size as a sector on the storage device. Smaller block sizes (2, 4 kB) are suitable for online transaction processing, where
		/// a lot of updates to the database occurs. Larger block sizes (8, 16, 32 kB) are suitable for decision support systems.
		/// The block sizes also limit the size of objects stored directly in the file. Larger objects will be persisted as BLOBs, 
		/// with the bulk of the object stored as separate files.</param>
		public ObjectBTreeFile(string FileName, string BlobFolder, int BlockSize, int BlocksInCache)
		{
			if (BlockSize < 1024)
				throw new ArgumentException("Block size too small.");

			int i = BlockSize;
			while (i != 0 && (i & 1) == 0)
				i >>= 1;

			if (i != 1)
				throw new ArgumentException("The block size must be a power of 2.");

			this.fileName = FileName;
			this.blobFolder = BlobFolder;
			this.blockSize = BlockSize;

			if (File.Exists(FileName))
				this.file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			else
				this.file = File.Open(FileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

			this.blocks = new Cache<long, byte[]>(BlocksInCache, TimeSpan.MaxValue, new TimeSpan(0, 1, 0, 0, 0));

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
		/// <param name="Position">Position of block.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Loaded block.</returns>
		public async Task<byte[]> LoadBlock(long Position, int TimeoutMilliseconds)
		{
			if ((Position % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "Position");

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentException("The timeout must be positive.", "TimeoutMilliseconds");

			byte[] Block;

			if (!Monitor.TryEnter(this.fileAccessSynchObj, TimeoutMilliseconds))
				throw new IOException("Unable to get access to underlying database.");
			
			try
			{
				if (this.blocks.TryGetValue(Position, out Block))
					return Block;

				if (Position != this.file.Seek(Position, SeekOrigin.Begin))
					throw new ArgumentException("Invalid file position.", "Position");

				Block = new byte[this.blockSize];

				if (this.blockSize != await this.file.ReadAsync(Block, 0, this.blockSize))
					throw new IOException("Read past end of file.");

				using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(Position)))
				{
					Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}

				this.blocks.Add(Position, Block);
			}
			finally
			{
				Monitor.Exit(this.fileAccessSynchObj);
			}

			return Block;
		}

		/// <summary>
		/// Saves a block to the file.
		/// </summary>
		/// <param name="Position">Position of block.</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <returns>Block to save.</returns>
		public async Task SaveBlock(long Position, byte[] Block, int TimeoutMilliseconds)
		{
			if ((Position % this.blockSize) != 0)
				throw new ArgumentException("Block positions must be multiples of the block size.", "Position");

			if (Block == null || Block.Length != this.blockSize)
				throw new ArgumentException("Block not of the correct block size.", "Block");

			if (TimeoutMilliseconds <= 0)
				throw new ArgumentException("The timeout must be positive.", "TimeoutMilliseconds");

			if (!Monitor.TryEnter(this.fileAccessSynchObj, TimeoutMilliseconds))
				throw new IOException("Unable to get access to underlying database.");

			try
			{
				// TODO: Lock blocks.

				byte[] EncryptedBlock;

				using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(Position)))
				{
					EncryptedBlock = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}

				if (Position != this.file.Seek(Position, SeekOrigin.Begin))
					throw new ArgumentException("Invalid file position.", "Position");

				await this.file.WriteAsync(EncryptedBlock, 0, this.blockSize);

				this.blocks.Add(Position, Block);
			}
			finally
			{
				Monitor.Exit(this.fileAccessSynchObj);
			}
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


	}
}

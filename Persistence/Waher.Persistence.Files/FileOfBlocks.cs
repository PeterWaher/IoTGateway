using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Runtime.Threading;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// File of sequential blocks.
	/// </summary>
	public class FileOfBlocks : IDisposable
	{
		private readonly MultiReadSingleWriteObject fileAccess;
		private readonly FileStream file;
		private readonly string collectionName;
		private readonly string fileName;
		private readonly bool filePreExisting;
		private readonly int blockSize;
		private readonly bool asyncFileIo;
		private long length;
		private uint blockLimit;

		/// <summary>
		/// File of sequential blocks.
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="FileName">File name. If the file or folder does not exist
		/// prior to the creation of the object, they are created accordingly.</param>
		/// <param name="BlockSize">Block Size</param>
		public FileOfBlocks(string CollectionName, string FileName, int BlockSize)
		{
			CheckBlockSize(BlockSize);

			this.collectionName = CollectionName;
			this.fileName = FileName;
			this.filePreExisting = File.Exists(this.fileName);
			this.blockSize = BlockSize;
			this.fileAccess = new MultiReadSingleWriteObject();
			this.asyncFileIo = FilesProvider.AsyncFileIo;

			string Folder = Path.GetDirectoryName(FileName);
			if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			if (this.filePreExisting)
				this.file = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			else
				this.file = File.Open(this.fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

			this.length = this.file.Length;
			this.blockLimit = (uint)(this.length / this.blockSize);
		}

		/// <summary>
		/// If the file existed before the construction of the object (true),
		/// or if the constructor created the file.
		/// </summary>
		public bool FilePreExisting => this.filePreExisting;

		/// <summary>
		/// Collection name.
		/// </summary>
		public string CollectionName => this.collectionName;

		/// <summary>
		/// File length.
		/// </summary>
		public long Length => this.length;

		/// <summary>
		/// Number of blocks in file.
		/// </summary>
		public uint BlockLimit => this.blockLimit;

		internal static void CheckBlockSize(int BlockSize)
		{
			if (BlockSize < 1024)
				throw new ArgumentOutOfRangeException("Block size too small.", nameof(BlockSize));

			if (BlockSize > 65536)
				throw new ArgumentOutOfRangeException("Block size too large.", nameof(BlockSize));

			int i = BlockSize;
			while (i != 0 && (i & 1) == 0)
				i >>= 1;

			if (i != 1)
				throw new ArgumentException("The block size must be a power of 2.", nameof(BlockSize));
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.file?.Dispose();
			this.fileAccess.Dispose();
		}

		/// <summary>
		/// Loads a block from the file.
		/// </summary>
		/// <param name="BlockIndex">Zero-based block index.</param>
		/// <returns>Loaded block.</returns>
		public async Task<byte[]> LoadBlock(uint BlockIndex)
		{
			byte[] Block = new byte[this.blockSize];
			await this.LoadBlock(BlockIndex, Block);
			return Block;
		}

		/// <summary>
		/// Loads a block from the file.
		/// </summary>
		/// <param name="BlockIndex">Zero-based block index.</param>
		/// <param name="Block"></param>
		/// <returns>Loaded block.</returns>
		public async Task LoadBlock(uint BlockIndex, byte[] Block)
		{
			if (Block.Length < this.blockSize)
				throw new ArgumentException("Block too small.", nameof(Block));

			int NrRead;

			await this.fileAccess.BeginWrite(); // Unique access required
			try
			{
				long Pos = ((long)BlockIndex) * this.blockSize;

				if (Pos != this.file.Seek(Pos, SeekOrigin.Begin))
					throw Database.FlagForRepair(this.collectionName, "Invalid file position.");

				if (this.asyncFileIo)
					NrRead = await this.file.ReadAsync(Block, 0, this.blockSize);
				else
					NrRead = this.file.Read(Block, 0, this.blockSize);

				if (this.blockSize != NrRead)
					throw Database.FlagForRepair(this.collectionName, "Read past end of file " + this.fileName + ".");
			}
			finally
			{
				await this.fileAccess.EndWrite();
			}
		}

		/// <summary>
		/// Saves a block to the file.
		/// </summary>
		/// <param name="BlockIndex">Zero-based block index.</param>
		/// <param name="Block">Block to save.</param>
		public async Task SaveBlock(uint BlockIndex, byte[] Block)
		{
			if (Block is null || Block.Length != this.blockSize)
				throw Database.FlagForRepair(this.collectionName, "Block not of the correct block size.");

			await this.fileAccess.BeginWrite();
			try
			{
				long Pos = ((long)BlockIndex) * this.blockSize;

				if (Pos != this.file.Seek(Pos, SeekOrigin.Begin))
					throw Database.FlagForRepair(this.collectionName, "Invalid file position.");

				if (this.asyncFileIo)
					await this.file.WriteAsync(Block, 0, this.blockSize);
				else
					this.file.Write(Block, 0, this.blockSize);

				if (BlockIndex == this.blockLimit)
				{
					this.blockLimit++;
					this.length += this.blockSize;
				}
			}
			finally
			{
				await this.fileAccess.EndWrite();
			}
		}

		/// <summary>
		/// Truncates the file to a given number of blocks.
		/// </summary>
		/// <param name="BlockLimit">Number of blocks in file.</param>
		public async Task Truncate(uint BlockLimit)
		{
			await this.fileAccess.BeginWrite();
			try
			{
				long Length = ((long)BlockLimit) * this.blockSize;
			
				this.file.SetLength(Length);
				
				this.length = Length;
				this.blockLimit = BlockLimit;
			}
			finally
			{
				await this.fileAccess.EndWrite();
			}
		}

		/// <summary>
		/// Flushes changes to the underlying device.
		/// </summary>
		public async Task FlushAsync()
		{
			await this.fileAccess.BeginWrite();
			try
			{
				await this.file.FlushAsync();
			}
			finally
			{
				await this.fileAccess.EndWrite();
			}
		}

	}
}

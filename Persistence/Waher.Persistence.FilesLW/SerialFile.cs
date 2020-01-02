using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Threading;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Serializes binary blocks into a file, possibly encrypted. Blocks are accessed in the order they were persisted.
	/// </summary>
	public class SerialFile : MultiReadSingleWriteObject, IDisposable
	{
		private const int MinBlockSize = 64;

		private readonly FileStream file;
		private readonly FilesProvider provider;
		private readonly Aes aes;
		private readonly string fileName;
		private readonly byte[] aesKey;
		private readonly byte[] ivSeed;
		private readonly int ivSeedLen;
		private readonly bool encrypted;
		private bool disposed = false;

		/// <summary>
		/// Maximum time to wait for access to underlying database (ms)
		/// </summary>
		protected readonly int timeoutMilliseconds;

		/// <summary>
		/// Serializes binary blocks into a file, possibly encrypted. Blocks are accessed in the order they were persisted.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		public SerialFile(string FileName, int TimeoutMilliseconds)
			: this(FileName, TimeoutMilliseconds, false, null)
		{
		}

		/// <summary>
		/// Serializes binary blocks into a file, possibly encrypted. Blocks are accessed in the order they were persisted.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="TimeoutMilliseconds">Timeout, in milliseconds.</param>
		/// <param name="Encrypted">If file is encrypted.</param>
		/// <param name="Provider">Provider of encryption keys.</param>
		public SerialFile(string FileName, int TimeoutMilliseconds, bool Encrypted, FilesProvider Provider)
		{
			if (TimeoutMilliseconds <= 0)
				throw new ArgumentOutOfRangeException("The timeout must be positive.", nameof(TimeoutMilliseconds));

			this.provider = Provider;
			this.fileName = FileName;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.encrypted = Encrypted;

			bool FileExists = File.Exists(this.fileName);

			string Folder = Path.GetDirectoryName(this.fileName);
			if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			if (FileExists)
				this.file = File.Open(this.fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			else
				this.file = File.Open(this.fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

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
		}

		/// <summary>
		/// File name.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Gets the length of the file, in bytes.
		/// </summary>
		/// <returns>Length of file.</returns>
		public async Task<long> GetLength()
		{
			await this.LockRead();
			try
			{
				return this.file.Length;
			}
			finally
			{
				await this.EndRead();
			}
		}

		/// <summary>
		/// Locks the file for reading.
		/// </summary>
		protected async Task LockRead()
		{
			if (!await this.TryBeginRead(this.timeoutMilliseconds))
				throw new TimeoutException("Unable to get read access to database.");
		}

		/// <summary>
		/// Locks the file for writing.
		/// </summary>
		protected async Task LockWrite()
		{
			if (!await this.TryBeginWrite(this.timeoutMilliseconds))
				throw new TimeoutException("Unable to get write access to database.");
		}

		/// <summary>
		/// Reads a binary block.
		/// </summary>
		/// <param name="Position">Position in file.</param>
		/// <param name="NrBytes">Block size, in bytes. (Can be smaller than persisted block.)</param>
		/// <returns>Block</returns>
		private async Task<byte[]> ReadBlock(long Position, int NrBytes)
		{
			byte[] Result = new byte[NrBytes];

			await this.LockRead();
			try
			{
				this.file.Position = Position;

				int NrRead = await this.file.ReadAsync(Result, 0, NrBytes);
				if (NrRead < NrBytes)
					throw new EndOfStreamException("Unexpected end of file " + this.fileName + ".");
			}
			finally
			{
				await this.EndRead();
			}

			if (this.encrypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateDecryptor(this.aesKey, this.GetIV(Position)))
				{
					Result = Aes.TransformFinalBlock(Result, 0, Result.Length);
				}
			}

			return Result;
		}

		/// <summary>
		/// Reads a binary block from the file, starting at a given position.
		/// </summary>
		/// <param name="Position">Position of block.</param>
		/// <returns>Binary block (decrypted if file is encrypted), and the position of the following block.</returns>
		public async Task<KeyValuePair<byte[], long>> ReadBlock(long Position)
		{
			byte[] Block = await this.ReadBlock(Position, MinBlockSize);
			int Pos = 0;
			int c = 0;
			int Offset = 0;
			byte b;

			do
			{
				b = Block[Pos++];

				c |= (b & 0x7f) << Offset;
				Offset += 7;

				if (Offset > 31)
					throw new IOException("Invalid block length. Possible corruption of file: " + this.fileName);
			}
			while ((b & 0x80) != 0);

			if (c <= 0 || c > int.MaxValue)
				throw new IOException("Invalid length. Possible corruption of file: " + this.fileName);

			int BlockSize = c + Pos;
			int Tail = BlockSize % MinBlockSize;

			if (Tail > 0)
				BlockSize += MinBlockSize - Tail;

			if (BlockSize > MinBlockSize)
				Block = await this.ReadBlock(Position, BlockSize);

			byte[] Data = new byte[c];
			Array.Copy(Block, Pos, Data, 0, c);

			return new KeyValuePair<byte[], long>(Data, Position + BlockSize);
		}

		/// <summary>
		/// Writes a binary block to the end of the file.
		/// </summary>
		/// <param name="Data">Binary data to write.</param>
		/// <returns>Position of data block.</returns>
		public async Task<long> WriteBlock(byte[] Data)
		{
			await this.LockWrite();
			try
			{
				return await this.WriteBlockLocked(Data);
			}
			finally
			{
				await this.EndWrite();
			}
		}

		/// <summary>
		/// Writes a binary block to the end of the file.
		/// </summary>
		/// <param name="Data">Binary data to write.</param>
		/// <returns>Position of data block.</returns>
		protected async Task<long> WriteBlockLocked(byte[] Data)
		{
			int c = 0;
			int i = Data.Length;

			if (i == 0)
				throw new ArgumentException("Zero-length blocks not allowed.", nameof(Data));

			while (i > 0)
			{
				i >>= 7;
				c++;
			}

			int BlockSize = Data.Length + c;
			int Tail = BlockSize % MinBlockSize;

			if (Tail > 0)
				BlockSize += MinBlockSize - Tail;

			byte[] Block = new byte[BlockSize];
			c = 0;

			i = Data.Length;

			while (i > 0)
			{
				Block[c] = (byte)(i & 127);
				i >>= 7;
				if (i > 0)
					Block[c] |= 0x80;

				c++;
			}

			Array.Copy(Data, 0, Block, c, Data.Length);

			long Position;

			Position = this.file.Length;

			this.file.Position = Position;

			if (this.encrypted)
			{
				using (ICryptoTransform Aes = this.aes.CreateEncryptor(this.aesKey, this.GetIV(Position)))
				{
					Block = Aes.TransformFinalBlock(Block, 0, Block.Length);
				}
			}

			await this.file.WriteAsync(Block, 0, Block.Length);
			await this.file.FlushAsync();

			return Position;
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

		/// <summary>
		/// Truncates the file.
		/// </summary>
		/// <param name="Length">Length at which the file will be truncated.</param>
		protected async Task Truncate(long Length)
		{
			await this.BeginWrite();
			try
			{
				this.file.SetLength(Length);
				this.file.Position = Length;
			}
			finally
			{
				await this.EndWrite();
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			if (!this.disposed)
			{
				this.file.Dispose();
				this.aes?.Dispose();
				this.disposed = true;
			}

			base.Dispose();
		}
	}
}

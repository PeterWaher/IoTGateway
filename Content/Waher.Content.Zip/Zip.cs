using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.IO;

namespace Waher.Content.Zip
{
	/// <summary>
	/// Static class for creating ZIP files.
	/// 
	/// Reference:
	/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
	/// https://www.rfc-editor.org/rfc/rfc1952.html
	/// </summary>
	public static class Zip
	{
		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		public static Task CreateZipFile(string SourceFileName, string OutputFileName)
		{
			return CreateZipFile(SourceFileName, OutputFileName, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static Task CreateZipFile(string SourceFileName,
			string OutputFileName, string Password, ZipEncryption EncryptionMethod)
		{
			return CreateZipFile(SourceFileName, OutputFileName, false, Password,
				EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="CreateFolder">Whether to create a folder for the zip file
		/// if it does not exist.</param>
		public static Task CreateZipFile(string SourceFileName,
			string OutputFileName, bool CreateFolder)
		{
			return CreateZipFile(SourceFileName, OutputFileName, CreateFolder, null,
				ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="CreateFolder">Whether to create a folder for the zip file
		/// if it does not exist.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static async Task CreateZipFile(string SourceFileName,
			string OutputFileName, bool CreateFolder, string Password,
			ZipEncryption EncryptionMethod)
		{
			if (CreateFolder)
			{
				string Folder = Path.GetDirectoryName(OutputFileName);
				if (!Directory.Exists(Folder))
					Directory.CreateDirectory(Folder);
			}

			DateTime LastWriteTime = File.GetLastWriteTime(SourceFileName);
			using FileStream fs = File.OpenRead(SourceFileName);
			using FileStream Output = File.Create(OutputFileName);

			await CreateZipFile(SourceFileName, fs, LastWriteTime,
				Output, Password, EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Contents of file.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string SourceFileName,
			byte[] SourceFileContents)
		{
			return CreateZipFile(SourceFileName, SourceFileContents, null,
				ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Contents of file.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string SourceFileName,
			byte[] SourceFileContents, string Password, ZipEncryption EncryptionMethod)
		{
			return CreateZipFile(SourceFileName, SourceFileContents,
				DateTime.Now, Password, EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string SourceFileName,
			byte[] SourceFileContents, DateTime SourceLastWriteTime)
		{
			return CreateZipFile(SourceFileName, SourceFileContents,
				SourceLastWriteTime, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static async Task<byte[]> CreateZipFile(string SourceFileName,
			byte[] SourceFileContents, DateTime SourceLastWriteTime, string Password,
			ZipEncryption EncryptionMethod)
		{
			using MemoryStream SourceFile = new MemoryStream(SourceFileContents);
			using MemoryStream ZipFile = new MemoryStream();

			await CreateZipFile(SourceFileName, SourceFile, SourceLastWriteTime,
				ZipFile, Password, EncryptionMethod);

			return ZipFile.ToArray();
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <param name="Output">Output stream to receive the protected zip file.</param>
		public static Task CreateZipFile(string SourceFileName,
			Stream SourceFileContents, DateTime SourceLastWriteTime, Stream Output)
		{
			return CreateZipFile(SourceFileName, SourceFileContents,
				SourceLastWriteTime, Output, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <param name="Output">Output stream to receive the protected zip file.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static Task CreateZipFile(string SourceFileName,
			Stream SourceFileContents, DateTime SourceLastWriteTime, Stream Output,
			string Password, ZipEncryption EncryptionMethod)
		{
			return CreateZipFile(new string[] { SourceFileName },
				new Stream[] { SourceFileContents },
				new DateTime[] { SourceLastWriteTime }, Output, Password, EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		public static Task CreateZipFile(string[] SourceFileNames, string OutputFileName)
		{
			return CreateZipFile(SourceFileNames, OutputFileName, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static Task CreateZipFile(string[] SourceFileNames,
			string OutputFileName, string Password, ZipEncryption EncryptionMethod)
		{
			return CreateZipFile(SourceFileNames, OutputFileName, false, Password,
				EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="CreateFolder">Whether to create a folder for the zip file
		/// if it does not exist.</param>
		public static Task CreateZipFile(string[] SourceFileNames,
			string OutputFileName, bool CreateFolder)
		{
			return CreateZipFile(SourceFileNames, OutputFileName, CreateFolder, null,
				ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="CreateFolder">Whether to create a folder for the zip file
		/// if it does not exist.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static async Task CreateZipFile(string[] SourceFileNames,
			string OutputFileName, bool CreateFolder, string Password,
			ZipEncryption EncryptionMethod)
		{
			if (CreateFolder)
			{
				string Folder = Path.GetDirectoryName(OutputFileName);
				if (!Directory.Exists(Folder))
					Directory.CreateDirectory(Folder);
			}

			int c = SourceFileNames.Length;
			FileStream[] SourceFiles = new FileStream[c];
			DateTime[] LastWriteTimes = new DateTime[c];
			try
			{
				for (int i = 0; i < c; i++)
				{
					SourceFiles[i] = File.OpenRead(SourceFileNames[i]);
					LastWriteTimes[i] = File.GetLastWriteTime(SourceFileNames[i]);
				}

				using FileStream Output = File.Create(OutputFileName);

				await CreateZipFile(SourceFileNames, SourceFiles, LastWriteTimes,
					Output, Password, EncryptionMethod);
			}
			finally
			{
				foreach (FileStream fs in SourceFiles)
					fs?.Dispose();
			}
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Contents of file.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string[] SourceFileNames,
			byte[][] SourceFileContents)
		{
			return CreateZipFile(SourceFileNames, SourceFileContents, null,
				ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Contents of file.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string[] SourceFileNames,
			byte[][] SourceFileContents, string Password, ZipEncryption EncryptionMethod)
		{
			int c = SourceFileNames.Length;
			DateTime[] LastWriteTimes = new DateTime[c];
			DateTime Now = DateTime.Now;

			for (int i = 0; i < c; i++)
				LastWriteTimes[i] = Now;

			return CreateZipFile(SourceFileNames, SourceFileContents,
				LastWriteTimes, Password, EncryptionMethod);
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTimes">Last write times of source files.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateZipFile(string[] SourceFileNames,
			byte[][] SourceFileContents, DateTime[] SourceLastWriteTimes)
		{
			return CreateZipFile(SourceFileNames, SourceFileContents,
				SourceLastWriteTimes, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTimes">Last write times of source files.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static async Task<byte[]> CreateZipFile(string[] SourceFileNames,
			byte[][] SourceFileContents, DateTime[] SourceLastWriteTimes, string Password,
			ZipEncryption EncryptionMethod)
		{
			MemoryStream[] SourceFiles = new MemoryStream[SourceFileContents.Length];

			try
			{
				for (int i = 0; i < SourceFileContents.Length; i++)
					SourceFiles[i] = new MemoryStream(SourceFileContents[i]);

				using MemoryStream ZipFile = new MemoryStream();

				await CreateZipFile(SourceFileNames, SourceFiles, SourceLastWriteTimes,
					ZipFile, Password, EncryptionMethod);

				return ZipFile.ToArray();
			}
			finally
			{
				foreach (MemoryStream ms in SourceFiles)
					ms?.Dispose();
			}
		}

		/// <summary>
		/// Creates a ZIP file containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTimes">Last write times of source files.</param>
		/// <param name="Output">Output stream to receive the protected zip file.</param>
		public static Task CreateZipFile(string[] SourceFileNames,
			Stream[] SourceFileContents, DateTime[] SourceLastWriteTimes, Stream Output)
		{
			return CreateZipFile(SourceFileNames, SourceFileContents,
				SourceLastWriteTimes, Output, null, ZipEncryption.None);
		}

		/// <summary>
		/// Creates a ZIP file, possible password-protected, containing a single file.
		/// </summary>
		/// <param name="SourceFileNames">File names of source files to include.</param>
		/// <param name="SourceFileContents">Streams containing contents of files.</param>
		/// <param name="SourceLastWriteTimes">Last write times of source files.</param>
		/// <param name="Output">Output stream to receive the protected zip file.</param>
		/// <param name="Password">Optional password to use to protect file name.</param>
		/// <param name="EncryptionMethod">ZIP Encryption method to use.</param>
		public static async Task CreateZipFile(string[] SourceFileNames,
			Stream[] SourceFileContents, DateTime[] SourceLastWriteTimes, Stream Output,
			string Password, ZipEncryption EncryptionMethod)
		{
			if (SourceFileNames is null)
				throw new ArgumentNullException(nameof(SourceFileNames));

			if (SourceFileContents is null)
				throw new ArgumentNullException(nameof(SourceFileContents));

			if (SourceLastWriteTimes is null)
				throw new ArgumentNullException(nameof(SourceLastWriteTimes));

			int Count = SourceFileNames.Length;
			if (SourceFileContents.Length != Count)
				throw new ArgumentException("Number of source file contents differ from number of source file names.", nameof(SourceFileContents));

			if (SourceLastWriteTimes.Length != Count)
				throw new ArgumentException("Number of source file last write times differ from number of source file names.", nameof(SourceLastWriteTimes));

			if (Count > ushort.MaxValue)
				throw new IOException("Too many entries for non-ZIP64 archive.");

			bool Encrypt;

			if (string.IsNullOrEmpty(Password))
			{
				if (EncryptionMethod != ZipEncryption.None)
					throw new ArgumentException("Password not specified.", nameof(Password));

				Encrypt = false;
			}
			else
			{
				if (EncryptionMethod == ZipEncryption.None)
					throw new ArgumentException("Encryption method not specified.", nameof(EncryptionMethod));

				Encrypt = true;
			}

			EntryMeta[] Entries = new EntryMeta[Count];
			ushort VersionToExtract = EncryptionMethod switch
			{
				ZipEncryption.Aes128Ae1 => 51,	// Version needed to extract (5.1 -> 51)
				ZipEncryption.Aes192Ae1 => 51,
				ZipEncryption.Aes256Ae1 => 51,
				ZipEncryption.Aes128Ae2 => 51,
				ZipEncryption.Aes192Ae2 => 51,
				ZipEncryption.Aes256Ae2 => 51,
				_ => 20                         // Version needed to extract (2.0 -> 20 in hex 0x0014)
			};

			for (int FileIndex = 0; FileIndex < Count; FileIndex++)
			{
				string SourceFileName = SourceFileNames[FileIndex];
				Stream SourceFileContent = SourceFileContents[FileIndex];
				DateTime SourceLastWriteTime = SourceLastWriteTimes[FileIndex];

				SourceFileContent.Position = 0;

				byte[] Bin = await SourceFileContent.ReadAllAsync();
				int UncompressedSize = Bin.Length;
				uint FileCrc32 = Crc32.Compute(Bin);
				byte[] Compressed;

				using (MemoryStream ms = new MemoryStream())
				{
					using (DeflateStream ds = new DeflateStream(ms, CompressionLevel.Optimal, true))
					{
						ds.Write(Bin, 0, UncompressedSize);
					}

					Compressed = ms.ToArray();
				}

				int CompressedSize0 = Compressed.Length;
				int CompressedSizeTot = CompressedSize0;

				string FileName = Path.GetFileName(SourceFileName);
				byte[] FileNameBytes = Encoding.UTF8.GetBytes(FileName);
				ushort FileNameLength = (ushort)FileNameBytes.Length;

				if (FileNameLength > ushort.MaxValue)
					throw new ArgumentException("Zip entry name too long: " + FileName, nameof(SourceFileNames));

				ushort Flags = 0x0800;      // UTF-8 names.

				byte[] Extra = null;
				ushort ExtraLen = 0;
				ushort Method = 8;          // Compression method: 8 = Deflate
				bool DataDescriptor = false;

				await Output.FlushAsync();
				long HeaderOffset = Output.Position;
				if (HeaderOffset > int.MaxValue)
					throw new IOException("ZIP output too large (>2GB). ZIP64 is not implemented.");

				int SaltLen = 0;
				bool Ae2 = EncryptionMethod switch
				{
					ZipEncryption.Aes128Ae1 => false,
					ZipEncryption.Aes192Ae1 => false,
					ZipEncryption.Aes256Ae1 => false,
					ZipEncryption.Aes128Ae2 => true,
					ZipEncryption.Aes192Ae2 => true,
					ZipEncryption.Aes256Ae2 => true,
					_ => false,
				};

				if (Encrypt)
				{
					Flags |= 0x0001;        // ZIP encryption.

					if (EncryptionMethod == ZipEncryption.ZipCrypto)
						CompressedSizeTot += 12;
					else
					{
						Method = 99;            // PKWARE AES: local header method must be 99
						Flags |= 0x0008;		// Signals use of a data descriptor (CRC/sizes written after data).
						DataDescriptor = true;

						ushort AesVersion = EncryptionMethod switch
						{
							ZipEncryption.Aes128Ae1 => 0x0001,
							ZipEncryption.Aes192Ae1 => 0x0001,
							ZipEncryption.Aes256Ae1 => 0x0001,
							ZipEncryption.Aes128Ae2 => 0x0002,
							ZipEncryption.Aes192Ae2 => 0x0002,
							ZipEncryption.Aes256Ae2 => 0x0002,
							_ => throw new ArgumentException("Invalid ZIP encryption method.", nameof(EncryptionMethod))
						};

						byte Strength = EncryptionMethod switch
						{
							ZipEncryption.Aes128Ae1 => 1,
							ZipEncryption.Aes128Ae2 => 1,
							ZipEncryption.Aes192Ae1 => 2,
							ZipEncryption.Aes192Ae2 => 2,
							ZipEncryption.Aes256Ae1 => 3,
							ZipEncryption.Aes256Ae2 => 3,
							_ => 1
						};

						SaltLen = Strength switch
						{
							1 => 8,     // 128 bits
							2 => 12,    // 192 bits
							3 => 16,    // 256 bits
							_ => 8
						};

						Extra = new byte[10]
						{
							0x01,
							0x99,	// ID = 0x9901
							0x07,
							0x00,	// Data size = 7,
							(byte)AesVersion,
							0x00,	// AES Version
							0x41,
							0x45,	// Vendor "AE" (0x4541)
							Strength,
							0x08	// Deflate
						};

						ExtraLen = 10;

						if (Ae2)
							CompressedSizeTot += 10;
					}

					if (CompressedSizeTot < 0)
						throw new IOException("ZIP output too large (>2GB). ZIP64 is not implemented.");
				}

				EntryMeta Entry = new EntryMeta()
				{
					FileNameBytes = FileNameBytes,
					Flags = Flags,
					CompressionMethod = Method,
					LastWriteTime = SourceLastWriteTime.ToDosTime(),
					LastWriteDate = SourceLastWriteTime.ToDosDate(),
					Crc32 = FileCrc32,
					CompressedSize = CompressedSizeTot,
					UncompressedSize = UncompressedSize,
					HeaderOffset = (int)HeaderOffset,
					Extra = Extra,
					ExtraLen = ExtraLen
				};
				Entries[FileIndex] = Entry;

				Output.WriteUInt32(0x04034b50); // Local file header signature 0x04034b50
				Output.WriteUInt16(VersionToExtract);
				Output.WriteUInt16(Flags);      // General purpose bit flag: bit0=1 (encrypted), others 0 (0x0001)
				Output.WriteUInt16(Method);
				Output.WriteUInt16(Entry.LastWriteTime);
				Output.WriteUInt16(Entry.LastWriteDate);

				if (DataDescriptor)
				{
					Output.WriteUInt32(0);
					Output.WriteUInt32(0);
					Output.WriteUInt32(0);
				}
				else
				{
					Output.WriteUInt32(FileCrc32);
					Output.WriteUInt32((uint)CompressedSizeTot);  // include encryption header in compressed size
					Output.WriteUInt32((uint)UncompressedSize);
				}

				Output.WriteUInt16(FileNameLength);             // File name length
				Output.WriteUInt16(ExtraLen);                   // Extra field length
				Output.Write(FileNameBytes, 0, FileNameBytes.Length);

				if (ExtraLen > 0)
					Output.Write(Extra, 0, ExtraLen);

				if (Encrypt)
				{
					if (EncryptionMethod == ZipEncryption.ZipCrypto)
					{
						uint Key0 = 0x12345678;
						uint Key1 = 0x23456789;
						uint Key2 = 0x34567890;

						foreach (char ch in Password)
						{
							if (ch > 255)
								throw new ArgumentException("Password contains invalid character: " + ch.ToString(), nameof(Password));

							UpdateKeys(ref Key0, ref Key1, ref Key2, (byte)ch);
						}

						using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
						byte[] EncryptionHeader = new byte[12];

						Rnd.GetBytes(EncryptionHeader, 0, 11);
						EncryptionHeader[11] = (byte)(FileCrc32 >> 24);

						// --- Write encrypted data: 12-byte header + compressed content ---

						// Encrypt and write the 12-byte encryption header
						foreach (byte b in EncryptionHeader)
						{
							byte EncryptedByte = EncryptByte(ref Key0, ref Key1, ref Key2, b);
							Output.WriteByte(EncryptedByte);
						}

						// Encrypt and write the compressed file bytes
						foreach (byte b in Compressed)
						{
							byte EncryptedByte = EncryptByte(ref Key0, ref Key1, ref Key2, b);
							Output.WriteByte(EncryptedByte);
						}
					}
					else    // PKWARE AES (AE-1/AE-2)
					{
						int KeyLen = EncryptionMethod switch
						{
							ZipEncryption.Aes128Ae1 => 16,
							ZipEncryption.Aes128Ae2 => 16,
							ZipEncryption.Aes192Ae1 => 24,
							ZipEncryption.Aes192Ae2 => 24,
							ZipEncryption.Aes256Ae1 => 32,
							ZipEncryption.Aes256Ae2 => 32,
							_ => 16
						};

						byte[] Salt = new byte[SaltLen];
						using (RandomNumberGenerator rnd = RandomNumberGenerator.Create())
							rnd.GetBytes(Salt);

						// Derive key material with PBKDF2-HMAC-SHA1 (1000 iterations)
						// Total length: EncryptionKey (KeyLen) + PasswordVerifier (2) + MacKey (16)

						int KdfLen = KeyLen + 2 + 16;
						byte[] PasswordBytes = Encoding.UTF8.GetBytes(Password);
						using Rfc2898DeriveBytes Pbkdf2 = new Rfc2898DeriveBytes(
							PasswordBytes, Salt, 1000, HashAlgorithmName.SHA1);
						byte[] KeyMaterial = Pbkdf2.GetBytes(KdfLen);

						byte[] EncryptionKey = new byte[KeyLen];
						Buffer.BlockCopy(KeyMaterial, 0, EncryptionKey, 0, KeyLen);

						byte[] PasswordVerifier = new byte[2];
						Buffer.BlockCopy(KeyMaterial, KeyLen, PasswordVerifier, 0, 2);

						byte[] MacKey = new byte[16];
						Buffer.BlockCopy(KeyMaterial, KeyLen + 2, MacKey, 0, 16);

						// Write salt and password verifier
						Output.Write(Salt, 0, SaltLen);
						Output.Write(PasswordVerifier, 0, 2);

						// AES-CTR encryption:
						// Use AES-ECB to generate keystream blocks, XOR with payload.
						// Counter block: 16 bytes, start counter at 1 in the last 4 bytes, big-endian increment.
						byte[] EncryptedPayload = new byte[CompressedSize0];
						using (Aes Aes = Aes.Create())
						{
							Aes.KeySize = KeyLen * 8;
							Aes.BlockSize = 128;
							Aes.Mode = CipherMode.ECB;
							Aes.Padding = PaddingMode.None;
							Aes.Key = EncryptionKey;
							Aes.IV = new byte[16];

							using ICryptoTransform Ecb = Aes.CreateEncryptor();
							byte[] CounterBin = new byte[16];
							uint Counter = 1;
							CounterBin[15] = 1;

							byte[] KeyStream = new byte[16];
							int Offset = 0;
							int BytesLeft=CompressedSize0;

							while (Offset < CompressedSize0)
							{
								Ecb.TransformBlock(CounterBin, 0, 16, KeyStream, 0);

								int BlockSize = Math.Min(16, BytesLeft);
								for (int i = 0; i < BlockSize; i++)
									EncryptedPayload[Offset + i] = (byte)(Compressed[Offset + i] ^ KeyStream[i]);

								// Increment counter (last 4 bytes, big-endian)
								Counter++;
								CounterBin[12] = (byte)(Counter >> 24);
								CounterBin[13] = (byte)(Counter >> 16);
								CounterBin[14] = (byte)(Counter >> 8);
								CounterBin[15] = (byte)Counter;

								Offset += BlockSize;
								BytesLeft -= BlockSize;
							}
						}

						// Write encrypted payload
						Output.Write(EncryptedPayload, 0, CompressedSize0);

						// AE-2: Write authentication code (first 10 bytes of HMAC-SHA1 over encrypted payload)
						if (Ae2)
						{
							using HMACSHA1 Hmac = new HMACSHA1(MacKey);
							byte[] Auth = Hmac.ComputeHash(EncryptedPayload);
							Output.Write(Auth, 0, 10);
						}
					}
				}
				else
					Output.Write(Compressed, 0, Compressed.Length);

				if (DataDescriptor)
				{
					// Optional signature 0x08074b50 + CRC32 + compressed size + uncompressed size (all little-endian)
					Output.WriteUInt32(0x08074b50);
					Output.WriteUInt32(FileCrc32);
					Output.WriteUInt32((uint)CompressedSizeTot);  // include encryption header in compressed size
					Output.WriteUInt32((uint)UncompressedSize);
				}
			}

			// --- Write Central Directory File Header ---

			// Record the offset where the central directory will start
			await Output.FlushAsync();
			long CentralDirOffset = Output.Position;
			if (CentralDirOffset > int.MaxValue)
				throw new IOException("ZIP output too large (>2GB). ZIP64 is not implemented.");

			foreach (EntryMeta Entry in Entries)
			{
				Output.WriteUInt32(0x02014b50);     // Central dir file header signature 0x02014b50
				Output.WriteUInt16(VersionToExtract);
				Output.WriteUInt16(VersionToExtract);
				Output.WriteUInt16(Entry.Flags);    // General purpose bit flag (same as local: 0x0001 for encryption)
				Output.WriteUInt16(Entry.CompressionMethod);
				Output.WriteUInt16(Entry.LastWriteTime);
				Output.WriteUInt16(Entry.LastWriteDate);
				Output.WriteUInt32(Entry.Crc32);
				Output.WriteUInt32((uint)Entry.CompressedSize);
				Output.WriteUInt32((uint)Entry.UncompressedSize);

				Output.WriteUInt16((ushort)Entry.FileNameBytes.Length); // File name length
				Output.WriteUInt16(Entry.ExtraLen); // extra field length, file comment length
				Output.WriteUInt16(0);              // file comment length

				Output.WriteUInt16(0);              // Disk number start
				Output.WriteUInt16(0);              // internal file attrs, external file attrs
				Output.WriteUInt32(0);              // external attrs (0 for default)

				Output.WriteUInt32((uint)Entry.HeaderOffset);   // Relative offset of local header (start at 0 for this file)

				Output.Write(Entry.FileNameBytes, 0, Entry.FileNameBytes.Length);

				if (Entry.ExtraLen > 0)
					Output.Write(Entry.Extra, 0, Entry.ExtraLen);
			}


			// --- Output.Write End of Central Directory (EOCD) record ---

			await Output.FlushAsync();
			uint CentralDirSize = (uint)(Output.Position - CentralDirOffset);

			Output.WriteUInt32(0x06054b50);     // EOCD signature 0x06054b50
			Output.WriteUInt16(0);              // Disk numbers (for single-disk archive, both 0)
			Output.WriteUInt16(0);
			Output.WriteUInt16((ushort)Count);  // Number of entries on this disk and total number of entries
			Output.WriteUInt16((ushort)Count);
			Output.WriteUInt32(CentralDirSize); // Size of central directory
			Output.WriteUInt32((uint)CentralDirOffset); // Offset of start of central directory
			Output.WriteUInt16(0);              // .ZIP file comment length (0 for no comment)

			await Output.FlushAsync();
		}

		private struct EntryMeta
		{
			public byte[] FileNameBytes;
			public ushort Flags;
			public ushort CompressionMethod;
			public ushort LastWriteTime;
			public ushort LastWriteDate;
			public uint Crc32;
			public int CompressedSize;
			public int UncompressedSize;
			public int HeaderOffset;
			public byte[] Extra;
			public ushort ExtraLen;
		}

		/// <summary>
		/// Update the ZipCrypto keys with a byte (typically a plaintext byte).
		/// </summary>
		/// <param name="Key0">First key</param>
		/// <param name="Key1">Second key</param>
		/// <param name="Key2">Third key</param>
		/// <param name="Value">Byte value</param>
		private static void UpdateKeys(ref uint Key0, ref uint Key1, ref uint Key2, byte Value)
		{
			Key0 = Crc32.Update(Key0, Value);
			Key1 += (byte)Key0;
			Key1 = Key1 * 134775813 + 1;
			Key2 = Crc32.Update(Key2, (byte)(Key1 >> 24));
		}

		/// <summary>
		/// Encrypt a byte and update keys. (XOR plaintext byte with key stream)
		/// </summary>
		/// <param name="Key0">First key</param>
		/// <param name="Key1">Second key</param>
		/// <param name="Key2">Third key</param>
		/// <param name="Value">Byte value</param>
		/// <returns>Encrypted byte.</returns>
		private static byte EncryptByte(ref uint Key0, ref uint Key1, ref uint Key2, byte Value)
		{
			uint Temp = Key2 | 3;
			byte KeyStreamByte = (byte)((Temp * (Temp ^ 1)) >> 8);
			byte CipherByte = (byte)(Value ^ KeyStreamByte);
			UpdateKeys(ref Key0, ref Key1, ref Key2, Value);
			return CipherByte;
		}

		/// <summary>
		/// Write a 2-byte word in little-endian format to a stream.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value</param>
		private static void WriteUInt16(this Stream Output, ushort Value)
		{
			Output.WriteByte((byte)Value);
			Value >>= 8;
			Output.WriteByte((byte)Value);
		}

		/// <summary>
		/// Write a 4-byte integer in little-endian format to a stream.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value</param>
		private static void WriteUInt32(this Stream Output, uint Value)
		{
			Output.WriteByte((byte)Value);
			Value >>= 8;
			Output.WriteByte((byte)Value);
			Value >>= 8;
			Output.WriteByte((byte)Value);
			Value >>= 8;
			Output.WriteByte((byte)Value);
		}
	}
}

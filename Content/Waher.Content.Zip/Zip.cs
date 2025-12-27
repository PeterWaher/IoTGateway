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
	/// Static class for creating password-protected ZIP files.
	/// 
	/// Reference:
	/// https://pkwaredownloads.blob.core.windows.net/pem/APPNOTE.txt
	/// https://www.rfc-editor.org/rfc/rfc1952.html
	/// </summary>
	public static class Zip
	{
		/// <summary>
		/// Creates a password-protected ZIP containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="Password">Password to use to protect file name.</param>
		public static Task CreateProtectedZipFile(string SourceFileName,
			string OutputFileName, string Password)
		{
			return CreateProtectedZipFile(SourceFileName, OutputFileName, false, Password);
		}

		/// <summary>
		/// Creates a password-protected ZIP containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="OutputFileName">File name of the protected zip file to create.</param>
		/// <param name="CreateFolder">Whether to create a folder for the zip file
		/// if it does not exist.</param>
		/// <param name="Password">Password to use to protect file name.</param>
		public static async Task CreateProtectedZipFile(string SourceFileName,
			string OutputFileName, bool CreateFolder, string Password)
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

			await CreateProtectedZipFile(SourceFileName, fs, LastWriteTime,
				Output, Password);
		}

		/// <summary>
		/// Creates a password-protected ZIP containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Contents of file.</param>
		/// <param name="Password">Password to use to protect file name.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static Task<byte[]> CreateProtectedZipFile(string SourceFileName,
			byte[] SourceFileContents, string Password)
		{
			return CreateProtectedZipFile(SourceFileName, SourceFileContents,
				DateTime.Now, Password);
		}

		/// <summary>
		/// Creates a password-protected ZIP containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <param name="Password">Password to use to protect file name.</param>
		/// <returns>Contents of Password-protected zip file.</returns>
		public static async Task<byte[]> CreateProtectedZipFile(string SourceFileName,
			byte[] SourceFileContents, DateTime SourceLastWriteTime, string Password)
		{
			using MemoryStream SourceFile = new MemoryStream(SourceFileContents);
			using MemoryStream ZipFile = new MemoryStream();

			await CreateProtectedZipFile(SourceFileName, SourceFile, SourceLastWriteTime,
				ZipFile, Password);

			return ZipFile.ToArray();
		}

		/// <summary>
		/// Creates a password-protected ZIP containing a single file.
		/// </summary>
		/// <param name="SourceFileName">File name of source file to include.</param>
		/// <param name="SourceFileContents">Stream containing contents of file.</param>
		/// <param name="SourceLastWriteTime">Last write time of source file.</param>
		/// <param name="Output">Output stream to receive the protected zip file.</param>
		/// <param name="Password">Password to use to protect file name.</param>
		public static async Task CreateProtectedZipFile(string SourceFileName,
			Stream SourceFileContents, DateTime SourceLastWriteTime, Stream Output,
			string Password)
		{
			SourceFileContents.Position = 0;

			byte[] Bin = await SourceFileContents.ReadAllAsync();
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

			int CompressedSize = Compressed.Length;

			uint Key0 = 0x12345678;
			uint Key1 = 0x23456789;
			uint Key2 = 0x34567890;

			foreach (char ch in Password)
			{
				if (ch > 255)
					throw new ArgumentException("Password contains invalid characters.", nameof(Password));

				UpdateKeys(ref Key0, ref Key1, ref Key2, (byte)ch);
			}

			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			byte[] EncryptionHeader = new byte[12];

			Rnd.GetBytes(EncryptionHeader, 0, 11);
			EncryptionHeader[11] = (byte)(FileCrc32 >> 24);

			string FileName = Path.GetFileName(SourceFileName);
			byte[] FileNameBytes = Encoding.UTF8.GetBytes(FileName);
			ushort FilenameLength = (ushort)FileNameBytes.Length;


			Output.WriteUInt32(0x04034b50); // Local file header signature 0x04034b50
			Output.WriteUInt16(20);         // Version needed to extract (2.0 -> 20 in hex 0x0014)
			Output.WriteUInt16(0x0001);     // General purpose bit flag: bit0=1 (encrypted), others 0 (0x0001)
			Output.WriteUInt16(8);          // Compression method: 8 = Deflate (or 0 for store if no compression)

			Output.WriteUInt16(SourceLastWriteTime.ToDosTime());
			Output.WriteUInt16(SourceLastWriteTime.ToDosDate());
			Output.WriteUInt32(FileCrc32);
			Output.WriteUInt32((uint)(CompressedSize + 12));  // include encryption header in compressed size
			Output.WriteUInt32((uint)UncompressedSize);
			Output.WriteUInt16(FilenameLength);             // File name length
			Output.WriteUInt16(0);                          // Extra field length (no extra)
			Output.Write(FileNameBytes, 0, FileNameBytes.Length);

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

			// Record the offset where the central directory will start
			long CentralDirOffset = Output.Position;

			// --- Write Central Directory File Header ---


			Output.WriteUInt32(0x02014b50);     // Central dir file header signature 0x02014b50
			Output.WriteUInt16(0x0014);         // Version made by (use 2.0 on DOS/Windows = 20 + OS indicator in high byte, e.g. 0x0314 for NTFS)
			Output.WriteUInt16(20);             // Version needed to extract
			Output.WriteUInt16(0x0001);         // General purpose bit flag (same as local: 0x0001 for encryption)
			Output.WriteUInt16(8);              // Compression method
			Output.WriteUInt16(SourceLastWriteTime.ToDosTime());
			Output.WriteUInt16(SourceLastWriteTime.ToDosDate());
			Output.WriteUInt32(FileCrc32);
			Output.WriteUInt32((uint)(CompressedSize + 12));
			Output.WriteUInt32((uint)UncompressedSize);

			Output.WriteUInt16(FilenameLength); // File name length
			Output.WriteUInt16(0);              // extra field length, file comment length
			Output.WriteUInt16(0);              // file comment length

			Output.WriteUInt16(0);              // Disk number start
			Output.WriteUInt16(0);              // internal file attrs, external file attrs
			Output.WriteUInt32(0);              // external attrs (0 for default)

			Output.WriteUInt32(0);              // Relative offset of local header (start at 0 for this file)

			Output.Write(FileNameBytes, 0, FileNameBytes.Length);

			// --- Output.Write End of Central Directory (EOCD) record ---


			Output.WriteUInt32(0x06054b50);     // EOCD signature 0x06054b50
			Output.WriteUInt16(0);              // Disk numbers (for single-disk archive, both 0)
			Output.WriteUInt16(0);

			Output.WriteUInt16(1);              // Number of entries on this disk and total number of entries
			Output.WriteUInt16(1);


			uint CentralDirSize = (uint)(Output.Position - CentralDirOffset);
			Output.WriteUInt32(CentralDirSize); // Size of central directory


			Output.WriteUInt32((uint)CentralDirOffset); // Offset of start of central directory

			Output.WriteUInt16(0);              // .ZIP file comment length (0 for no comment)

			await Output.FlushAsync();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Waher.Networking
{
	/// <summary>
	/// Hash method enumeration.
	/// </summary>
	public enum HashFunction
	{
		/// <summary>
		/// MD5 Hash function
		/// </summary>
		MD5,

		/// <summary>
		/// SHA-1 Hash function
		/// </summary>
		SHA1,

		/// <summary>
		/// SHA-256 Hash function
		/// </summary>
		SHA256,

		/// <summary>
		/// SHA-384 Hash function
		/// </summary>
		SHA384,

		/// <summary>
		/// SHA-512 Hash function
		/// </summary>
		SHA512
	}

	/// <summary>
	/// Contains methods for simple hash calculations.
	/// </summary>
	public static class Hashes
	{
		/// <summary>
		/// Converts an array of bytes to a string with their hexadecimal representations (in lower case).
		/// </summary>
		/// <param name="Data"></param>
		/// <returns></returns>
		public static string BinaryToString(byte[] Data)
		{
			StringBuilder Response = new StringBuilder();

			foreach (byte b in Data)
				Response.Append(b.ToString("x2"));

			return Response.ToString();
		}

		/// <summary>
		/// Computes a hash of a block of binary data.
		/// </summary>
		/// <param name="Function">Hash function.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHashString(HashFunction Function, byte[] Data)
		{
			switch (Function)
			{
				case HashFunction.MD5:
					return ComputeMD5HashString(Data);

				case HashFunction.SHA1:
					return ComputeSHA1HashString(Data);

				case HashFunction.SHA256:
					return ComputeSHA256HashString(Data);

				case HashFunction.SHA384:
					return ComputeSHA384HashString(Data);

				case HashFunction.SHA512:
					return ComputeSHA512HashString(Data);

				default:
					throw new ArgumentException("Unrecognized hash function", "Function");
			}
		}

		/// <summary>
		/// Computes a hash of a block of binary data.
		/// </summary>
		/// <param name="Function">Hash function.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHashString(HashFunction Function, Stream Data)
		{
			switch (Function)
			{
				case HashFunction.MD5:
					return BinaryToString(ComputeMD5Hash(Data));

				case HashFunction.SHA1:
					return BinaryToString(ComputeSHA1Hash(Data));

				case HashFunction.SHA256:
					return BinaryToString(ComputeSHA256Hash(Data));

				case HashFunction.SHA384:
					return BinaryToString(ComputeSHA384Hash(Data));

				case HashFunction.SHA512:
					return BinaryToString(ComputeSHA512Hash(Data));

				default:
					throw new ArgumentException("Unrecognized hash function", "Function");
			}
		}

		/// <summary>
		/// Computes a hash of a block of binary data.
		/// </summary>
		/// <param name="Function">Hash function.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHash(HashFunction Function, byte[] Data)
		{
			switch (Function)
			{
				case HashFunction.MD5:
					return ComputeMD5Hash(Data);

				case HashFunction.SHA1:
					return ComputeSHA1Hash(Data);

				case HashFunction.SHA256:
					return ComputeSHA256Hash(Data);

				case HashFunction.SHA384:
					return ComputeSHA384Hash(Data);

				case HashFunction.SHA512:
					return ComputeSHA512Hash(Data);

				default:
					throw new ArgumentException("Unrecognized hash function", "Function");
			}
		}

		/// <summary>
		/// Computes a hash of a block of binary data.
		/// </summary>
		/// <param name="Function">Hash function.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHash(HashFunction Function, Stream Data)
		{
			switch (Function)
			{
				case HashFunction.MD5:
					return ComputeMD5Hash(Data);

				case HashFunction.SHA1:
					return ComputeSHA1Hash(Data);

				case HashFunction.SHA256:
					return ComputeSHA256Hash(Data);

				case HashFunction.SHA384:
					return ComputeSHA384Hash(Data);

				case HashFunction.SHA512:
					return ComputeSHA512Hash(Data);

				default:
					throw new ArgumentException("Unrecognized hash function", "Function");
			}
		}


		/// <summary>
		/// Computes the SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA1HashString(byte[] Data)
		{
			return BinaryToString(ComputeSHA1Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA1HashString(Stream Data)
		{
			return BinaryToString(ComputeSHA1Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA1Hash(byte[] Data)
		{
			byte[] Result;

			using (SHA1 SHA1 = SHA1.Create())
			{
				Result = SHA1.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA1Hash(Stream Data)
		{
			byte[] Result;

			using (SHA1 SHA1 = SHA1.Create())
			{
				Result = SHA1.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA256HashString(byte[] Data)
		{
			return BinaryToString(ComputeSHA256Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA256HashString(Stream Data)
		{
			return BinaryToString(ComputeSHA256Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA256Hash(byte[] Data)
		{
			byte[] Result;

			using (SHA256 SHA256 = SHA256.Create())
			{
				Result = SHA256.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA256Hash(Stream Data)
		{
			byte[] Result;

			using (SHA256 SHA256 = SHA256.Create())
			{
				Result = SHA256.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA384HashString(byte[] Data)
		{
			return BinaryToString(ComputeSHA384Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA384HashString(Stream Data)
		{
			return BinaryToString(ComputeSHA384Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA384Hash(byte[] Data)
		{
			byte[] Result;

			using (SHA384 SHA384 = SHA384.Create())
			{
				Result = SHA384.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA384Hash(Stream Data)
		{
			byte[] Result;

			using (SHA384 SHA384 = SHA384.Create())
			{
				Result = SHA384.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA512HashString(byte[] Data)
		{
			return BinaryToString(ComputeSHA512Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeSHA512HashString(Stream Data)
		{
			return BinaryToString(ComputeSHA512Hash(Data));
		}

		/// <summary>
		/// Computes the SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA512Hash(byte[] Data)
		{
			byte[] Result;

			using (SHA512 SHA512 = SHA512.Create())
			{
				Result = SHA512.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA512Hash(Stream Data)
		{
			byte[] Result;

			using (SHA512 SHA512 = SHA512.Create())
			{
				Result = SHA512.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the MD5 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeMD5HashString(byte[] Data)
		{
			return BinaryToString(ComputeMD5Hash(Data));
		}

		/// <summary>
		/// Computes the MD5 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeMD5HashString(Stream Data)
		{
			return BinaryToString(ComputeMD5Hash(Data));
		}

		/// <summary>
		/// Computes the MD5 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeMD5Hash(byte[] Data)
		{
			byte[] Result;

			using (MD5 MD5 = MD5.Create())
			{
				Result = MD5.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the MD5 hash of a block of binary data.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeMD5Hash(Stream Data)
		{
			byte[] Result;

			using (MD5 MD5 = MD5.Create())
			{
				Result = MD5.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the HMAC-SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHMACSHA1HashString(byte[] Key, byte[] Data)
		{
			return BinaryToString(ComputeHMACSHA1Hash(Key, Data));
		}

		/// <summary>
		/// Computes the HMAC-SHA-1 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHMACSHA1Hash(byte[] Key, byte[] Data)
		{
			byte[] Result;

			using (HMACSHA1 HMACSHA1 = new HMACSHA1(Key))
			{
				Result = HMACSHA1.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the HMAC-SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHMACSHA256HashString(byte[] Key, byte[] Data)
		{
			return BinaryToString(ComputeHMACSHA256Hash(Key, Data));
		}

		/// <summary>
		/// Computes the HMAC-SHA-256 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHMACSHA256Hash(byte[] Key, byte[] Data)
		{
			byte[] Result;

			using (HMACSHA256 HMACSHA256 = new HMACSHA256(Key))
			{
				Result = HMACSHA256.ComputeHash(Data);
			}

			return Result;
		}


	}
}

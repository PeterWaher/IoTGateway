﻿using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Security
{
	/// <summary>
	/// Delegate to hash function.
	/// </summary>
	/// <param name="Data">Data to be hashed.</param>
	/// <returns>Hash digest</returns>
	public delegate byte[] HashFunctionArray(byte[] Data);

	/// <summary>
	/// Delegate to hash function.
	/// </summary>
	/// <param name="Data">Data to be hashed.</param>
	/// <returns>Hash digest</returns>
	public delegate byte[] HashFunctionStream(Stream Data);

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
		/// <param name="Data">Binary Data</param>
		/// <returns>Hexadecimal string representation.</returns>
		public static string BinaryToString(byte[] Data)
		{
			return BinaryToString(Data, false);
		}

		/// <summary>
		/// Converts an array of bytes to a string with their hexadecimal representations (in lower case).
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="SpaceDelimiter">If bytes are to be separated by space characters.</param>
		/// <returns>Hexadecimal string representation.</returns>
		public static string BinaryToString(byte[] Data, bool SpaceDelimiter)
		{
			StringBuilder Response = new StringBuilder();
			bool First = true;

			foreach (byte b in Data)
			{
				if (SpaceDelimiter)
				{
					if (First)
						First = false;
					else
						Response.Append(' ');
				}

				Response.Append(b.ToString("x2"));
			}

			return Response.ToString();
		}

		/// <summary>
		/// Parses a hex string.
		/// </summary>
		/// <param name="s">String.</param>
		/// <returns>Array of bytes found in the string, or null if not a hex string.</returns>
		public static byte[] StringToBinary(string s)
		{
			using (MemoryStream Bytes = new MemoryStream())
			{
				int i, c = s.Length;
				byte b = 0, b2;
				bool First = true;
				char ch;

				for (i = 0; i < c; i++)
				{
					ch = s[i];

					if (ch >= '0' && ch <= '9')
						b2 = (byte)(ch - '0');
					else if (ch >= 'a' && ch <= 'f')
						b2 = (byte)(ch - 'a' + 10);
					else if (ch >= 'A' && ch <= 'F')
						b2 = (byte)(ch - 'A' + 10);
					else if (ch == ' ' || ch == 160)
						continue;
					else
						return null;

					if (First)
					{
						b = b2;
						First = false;
					}
					else
					{
						b <<= 4;
						b |= b2;

						Bytes.WriteByte(b);
						First = true;
					}
				}

				if (!First)
					return null;

				return Bytes.ToArray();
			}
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
					throw new ArgumentException("Unrecognized hash function", nameof(Function));
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
					throw new ArgumentException("Unrecognized hash function", nameof(Function));
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
					throw new ArgumentException("Unrecognized hash function", nameof(Function));
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
					throw new ArgumentException("Unrecognized hash function", nameof(Function));
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

		/// <summary>
		/// Computes the HMAC-SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHMACSHA384HashString(byte[] Key, byte[] Data)
		{
			return BinaryToString(ComputeHMACSHA384Hash(Key, Data));
		}

		/// <summary>
		/// Computes the HMAC-SHA-384 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHMACSHA384Hash(byte[] Key, byte[] Data)
		{
			byte[] Result;

			using (HMACSHA384 HMACSHA384 = new HMACSHA384(Key))
			{
				Result = HMACSHA384.ComputeHash(Data);
			}

			return Result;
		}

		/// <summary>
		/// Computes the HMAC-SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>String representation of hash.</returns>
		public static string ComputeHMACSHA512HashString(byte[] Key, byte[] Data)
		{
			return BinaryToString(ComputeHMACSHA512Hash(Key, Data));
		}

		/// <summary>
		/// Computes the HMAC-SHA-512 hash of a block of binary data.
		/// </summary>
		/// <param name="Key">Binary key.</param>
		/// <param name="Data">Binary data.</param>
		/// <returns>Hash value.</returns>
		public static byte[] ComputeHMACSHA512Hash(byte[] Key, byte[] Data)
		{
			byte[] Result;

			using (HMACSHA512 HMACSHA512 = new HMACSHA512(Key))
			{
				Result = HMACSHA512.ComputeHash(Data);
			}

			return Result;
		}


	}
}

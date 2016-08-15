using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Security;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
#else
using System.Security.Cryptography;
#endif

namespace Waher.Networking
{
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
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA1Hash(byte[] Data)
		{
			byte[] Result;

#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);
#else
			using (SHA1 SHA1 = SHA1.Create())
			{
				Result = SHA1.ComputeHash(Data);
			}
#endif
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
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA256Hash(byte[] Data)
		{
			byte[] Result;

#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);
#else
			using (SHA256 SHA256 = SHA256.Create())
			{
				Result = SHA256.ComputeHash(Data);
			}
#endif
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
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA384Hash(byte[] Data)
		{
			byte[] Result;

#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha384);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);
#else
			using (SHA384 SHA384 = SHA384.Create())
			{
				Result = SHA384.ComputeHash(Data);
			}
#endif
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
		/// <returns>Hash value.</returns>
		public static byte[] ComputeSHA512Hash(byte[] Data)
		{
			byte[] Result;

#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);
#else
			using (SHA512 SHA512 = SHA512.Create())
			{
				Result = SHA512.ComputeHash(Data);
			}
#endif
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
		/// <returns>Hash value.</returns>
		public static byte[] ComputeMD5Hash(byte[] Data)
		{
			byte[] Result;

#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);
#else
			using (MD5 MD5 = MD5.Create())
			{
				Result = MD5.ComputeHash(Data);
			}
#endif
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

#if WINDOWS_UWP
			MacAlgorithmProvider MacProvider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
			CryptographicKey HMAC = MacProvider.CreateKey(CryptographicBuffer.CreateFromByteArray(Key));
			IBuffer Signature = CryptographicEngine.Sign(HMAC, 
				CryptographicBuffer.CreateFromByteArray(Data));

			CryptographicBuffer.CopyToByteArray(Signature, out Result);
#else
			using (HMACSHA1 HMACSHA1 = new HMACSHA1(Key, true))
			{
				Result = HMACSHA1.ComputeHash(Data);
			}
#endif
			return Result;
		}


	}
}

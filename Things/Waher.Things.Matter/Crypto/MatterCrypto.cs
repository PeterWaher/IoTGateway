using System;
using System.Security.Cryptography;
using System.Text;

namespace Waher.Things.Matter.Crypto
{
	/// <summary>
	/// Cryptographic primitives used by the Matter protocol (Matter Core Specification, chapter 3).
	/// </summary>
	public static class MatterCrypto
	{
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		/// <summary>
		/// Length of a symmetric key, in bytes (CRYPTO_SYMMETRIC_KEY_LENGTH_BYTES).
		/// </summary>
		public const int SymmetricKeyLength = 16;

		/// <summary>
		/// Length of an AEAD MIC, in bytes (CRYPTO_AEAD_MIC_LENGTH_BYTES).
		/// </summary>
		public const int AeadMicLength = 16;

		/// <summary>
		/// Length of an AEAD nonce, in bytes (CRYPTO_AEAD_NONCE_LENGTH_BYTES).
		/// </summary>
		public const int AeadNonceLength = 13;

		/// <summary>
		/// Length of a hash, in bytes (CRYPTO_HASH_LEN_BYTES).
		/// </summary>
		public const int HashLength = 32;

		/// <summary>
		/// Length of an uncompressed public key, in bytes (CRYPTO_PUBLIC_KEY_SIZE_BYTES).
		/// </summary>
		public const int PublicKeyLength = 65;

		/// <summary>
		/// Length of a raw ECDSA signature, in bytes.
		/// </summary>
		public const int SignatureLength = 64;

		/// <summary>
		/// Generates random bytes.
		/// </summary>
		/// <param name="NrBytes">Number of bytes.</param>
		/// <returns>Random bytes.</returns>
		public static byte[] RandomBytes(int NrBytes)
		{
			byte[] Result = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Result);
			}

			return Result;
		}

		/// <summary>
		/// Generates a random unsigned 16-bit integer.
		/// </summary>
		public static ushort RandomUInt16()
		{
			return BitConverter.ToUInt16(RandomBytes(2), 0);
		}

		/// <summary>
		/// Generates a random unsigned 32-bit integer.
		/// </summary>
		public static uint RandomUInt32()
		{
			return BitConverter.ToUInt32(RandomBytes(4), 0);
		}

		/// <summary>
		/// Generates a random unsigned 64-bit integer.
		/// </summary>
		public static ulong RandomUInt64()
		{
			return BitConverter.ToUInt64(RandomBytes(8), 0);
		}

		/// <summary>
		/// Computes a SHA-256 hash (Crypto_Hash).
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash digest.</returns>
		public static byte[] Sha256(params byte[][] Data)
		{
			using (IncrementalHash H = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
			{
				foreach (byte[] Part in Data)
				{
					if (!(Part is null))
						H.AppendData(Part);
				}

				return H.GetHashAndReset();
			}
		}

		/// <summary>
		/// Computes a HMAC-SHA256 (Crypto_HMAC).
		/// </summary>
		/// <param name="Key">Key.</param>
		/// <param name="Data">Data to authenticate.</param>
		/// <returns>MAC.</returns>
		public static byte[] HmacSha256(byte[] Key, params byte[][] Data)
		{
			using (IncrementalHash H = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, Key))
			{
				foreach (byte[] Part in Data)
				{
					if (!(Part is null))
						H.AppendData(Part);
				}

				return H.GetHashAndReset();
			}
		}

		/// <summary>
		/// HKDF-SHA256 key derivation function (Crypto_KDF), as defined in RFC 5869.
		/// </summary>
		/// <param name="InputKey">Input key material.</param>
		/// <param name="Salt">Salt (may be null or empty).</param>
		/// <param name="Info">Context information.</param>
		/// <param name="Length">Number of bytes to derive.</param>
		/// <returns>Derived key material.</returns>
		public static byte[] Hkdf(byte[] InputKey, byte[] Salt, byte[] Info, int Length)
		{
			if (Length <= 0 || Length > 255 * HashLength)
				throw new ArgumentOutOfRangeException(nameof(Length));

			if (Salt is null || Salt.Length == 0)
				Salt = new byte[HashLength];

			byte[] Prk = HmacSha256(Salt, InputKey);
			byte[] Result = new byte[Length];
			byte[] T = Array.Empty<byte>();
			byte[] Counter = new byte[1];
			int Pos = 0;
			byte i = 1;

			while (Pos < Length)
			{
				Counter[0] = i++;
				T = HmacSha256(Prk, T, Info ?? Array.Empty<byte>(), Counter);

				int c = Math.Min(T.Length, Length - Pos);
				Array.Copy(T, 0, Result, Pos, c);
				Pos += c;
			}

			return Result;
		}

		/// <summary>
		/// HKDF-SHA256 key derivation function (Crypto_KDF), using an ASCII info string.
		/// </summary>
		/// <param name="InputKey">Input key material.</param>
		/// <param name="Salt">Salt (may be null or empty).</param>
		/// <param name="Info">Context information (ASCII string).</param>
		/// <param name="Length">Number of bytes to derive.</param>
		/// <returns>Derived key material.</returns>
		public static byte[] Hkdf(byte[] InputKey, byte[] Salt, string Info, int Length)
		{
			return Hkdf(InputKey, Salt, Encoding.ASCII.GetBytes(Info), Length);
		}

		/// <summary>
		/// PBKDF2 using HMAC-SHA256 (Crypto_PBKDF).
		/// </summary>
		/// <param name="Password">Password.</param>
		/// <param name="Salt">Salt.</param>
		/// <param name="Iterations">Number of iterations.</param>
		/// <param name="Length">Number of bytes to derive.</param>
		/// <returns>Derived key material.</returns>
		public static byte[] Pbkdf2Sha256(byte[] Password, byte[] Salt, int Iterations, int Length)
		{
			if (Iterations <= 0)
				throw new ArgumentOutOfRangeException(nameof(Iterations));

			byte[] Result = new byte[Length];
			byte[] SaltAndIndex = new byte[Salt.Length + 4];
			Array.Copy(Salt, 0, SaltAndIndex, 0, Salt.Length);

			using (HMACSHA256 Hmac = new HMACSHA256(Password))
			{
				int Pos = 0;
				uint BlockIndex = 1;

				while (Pos < Length)
				{
					SaltAndIndex[Salt.Length] = (byte)(BlockIndex >> 24);
					SaltAndIndex[Salt.Length + 1] = (byte)(BlockIndex >> 16);
					SaltAndIndex[Salt.Length + 2] = (byte)(BlockIndex >> 8);
					SaltAndIndex[Salt.Length + 3] = (byte)BlockIndex;

					byte[] U = Hmac.ComputeHash(SaltAndIndex);
					byte[] T = (byte[])U.Clone();
					int i, j;

					for (i = 1; i < Iterations; i++)
					{
						U = Hmac.ComputeHash(U);

						for (j = 0; j < T.Length; j++)
							T[j] ^= U[j];
					}

					int c = Math.Min(T.Length, Length - Pos);
					Array.Copy(T, 0, Result, Pos, c);
					Pos += c;
					BlockIndex++;
				}
			}

			return Result;
		}

		/// <summary>
		/// Concatenates byte arrays.
		/// </summary>
		/// <param name="Parts">Parts.</param>
		/// <returns>Concatenated array.</returns>
		public static byte[] Concat(params byte[][] Parts)
		{
			int c = 0;

			foreach (byte[] Part in Parts)
				c += Part?.Length ?? 0;

			byte[] Result = new byte[c];
			int Pos = 0;

			foreach (byte[] Part in Parts)
			{
				if (!(Part is null))
				{
					Array.Copy(Part, 0, Result, Pos, Part.Length);
					Pos += Part.Length;
				}
			}

			return Result;
		}

		/// <summary>
		/// Compares two byte arrays in constant time.
		/// </summary>
		/// <param name="A">First array.</param>
		/// <param name="B">Second array.</param>
		/// <returns>If arrays are equal.</returns>
		public static bool AreEqual(byte[] A, byte[] B)
		{
			if (A is null || B is null)
				return A is null && B is null;

			if (A.Length != B.Length)
				return false;

			int Diff = 0;
			int i, c = A.Length;

			for (i = 0; i < c; i++)
				Diff |= A[i] ^ B[i];

			return Diff == 0;
		}

		/// <summary>
		/// Gets a slice of a byte array.
		/// </summary>
		/// <param name="Data">Data.</param>
		/// <param name="Offset">Offset.</param>
		/// <param name="Count">Number of bytes.</param>
		/// <returns>Slice.</returns>
		public static byte[] Slice(byte[] Data, int Offset, int Count)
		{
			byte[] Result = new byte[Count];
			Array.Copy(Data, Offset, Result, 0, Count);
			return Result;
		}

		/// <summary>
		/// Converts a byte array to a hexadecimal string.
		/// </summary>
		/// <param name="Data">Data.</param>
		/// <returns>Hexadecimal string (lower case).</returns>
		public static string ToHex(byte[] Data)
		{
			if (Data is null)
				return string.Empty;

			StringBuilder sb = new StringBuilder(Data.Length * 2);

			foreach (byte b in Data)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}

		/// <summary>
		/// Converts a hexadecimal string to a byte array.
		/// </summary>
		/// <param name="Hex">Hexadecimal string (whitespace and colons are ignored).</param>
		/// <returns>Bytes.</returns>
		public static byte[] FromHex(string Hex)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char ch in Hex)
			{
				if (char.IsWhiteSpace(ch) || ch == ':' || ch == '-')
					continue;

				sb.Append(ch);
			}

			string s = sb.ToString();
			if ((s.Length & 1) != 0)
				throw new ArgumentException("Hexadecimal string must have an even number of characters.", nameof(Hex));

			byte[] Result = new byte[s.Length / 2];
			int i;

			for (i = 0; i < Result.Length; i++)
				Result[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);

			return Result;
		}
	}
}

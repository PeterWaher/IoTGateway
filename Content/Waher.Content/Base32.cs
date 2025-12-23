using System;
using System.IO;

namespace Waher.Content
{
	/// <summary>
	/// Static class that does BASE32 encoding and decoding as defined in RFC4648:
	/// https://datatracker.ietf.org/doc/html/rfc4648
	/// </summary>
	public static class Base32
	{
		private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		/// <summary>
		/// Converts a Base32-encoded string to its binary representation.
		/// </summary>
		/// <param name="Base32">Base32-encoded string.</param>
		/// <returns>Binary representation.</returns>
		public static byte[] Decode(string Base32)
		{
			MemoryStream ms = new MemoryStream();
			ushort Buffer = 0;
			int Offset = 0;

			foreach (char ch in Base32)
			{
				Buffer <<= 5;
				Offset += 5;

				if (ch >= 'A' && ch <= 'Z')
					 Buffer |= (byte)(ch - 'A');
				else if (ch >= '2' && ch <= '7')
					Buffer |= (byte)(ch - '2' + 26);
				else if (ch == '=')
					break;
				else
					throw new FormatException("Invalid Base32 character: " + ch.ToString());

				if (Offset >= 8)
				{
					ms.WriteByte((byte)(Buffer >> (Offset - 8)));
					Offset -= 8;
				}
			}

			return ms.ToArray();
		}

		/// <summary>
		/// Converts a binary block of data to a Base32-encoded string.
		/// </summary>
		/// <param name="Data">Data to encode.</param>
		/// <returns>Base32-encoded string.</returns>
		public static string Encode(byte[] Data)
		{
			if (Data == null || Data.Length == 0)
				return string.Empty;

			int Nr8CharBlocks = ((Data.Length << 3) + 39) / 40;
			int Len = Nr8CharBlocks << 3;
			char[] Result = new char[Len];
			ushort Buffer = 0;
			int NrBits = 0;
			int Pos = 0;
			byte b;

			for (int i = 0; i < Data.Length; i++)
			{
				Buffer <<= 8;
				Buffer |= Data[i];
				NrBits += 8;

				while (NrBits >= 5)
				{
					NrBits -= 5;
					b = (byte)(Buffer >> NrBits);
					Result[Pos++] = Base32Chars[b & 0x1f];
				}
			}

			if (NrBits > 0)
			{
				b = (byte)(Buffer << (5 - NrBits));
				Result[Pos++] = Base32Chars[b & 0x1f];
			}

			while (Pos < Len)
				Result[Pos++] = '=';

			return new string(Result);
		}
	}
}

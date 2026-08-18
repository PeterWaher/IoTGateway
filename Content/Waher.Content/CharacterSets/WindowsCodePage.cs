using System.Collections.Generic;

namespace Waher.Content.CharacterSets
{
	/// <summary>
	/// Abstract base class for Windows code pages.
	/// </summary>
	public abstract class WindowsCodePage : System.Text.Encoding
	{
		/// <summary>
		/// Character that is not available or defined.
		/// </summary>
		protected readonly static char? _NA = null;

		/// <summary>
		/// WINDOWS-1257 encoding.
		/// </summary>
		public readonly static Windows1257 Windows1257 = new Windows1257();

		/// <summary>
		/// WINDOWS-1252 encoding.
		/// </summary>
		public readonly static Windows1252 Windows1252 = new Windows1252();

		private readonly Dictionary<char, byte> bytes;
		private readonly char?[] chars;

		/// <summary>
		/// Computes a reverse dictionary from a character array.
		/// </summary>
		/// <param name="Chars">The character array to reverse.</param>
		/// <returns>A dictionary mapping characters to their byte values.</returns>
		protected static Dictionary<char, byte> ReverseDictionary(char?[] Chars)
		{
			Dictionary<char, byte> Result = new Dictionary<char, byte>();
			
			for (int i = 0; i < Chars.Length; i++)
			{
				if (Chars[i].HasValue)
					Result[Chars[i].Value] = (byte)i;
			}

			return Result;
		}

		/// <summary>
		/// Abstract base class for Windows code pages.
		/// </summary>
		/// <param name="CharacterFromByte">Character from byte array.</param>
		/// <param name="ByteFromCharacter">Byte from character dictionary.</param>
		public WindowsCodePage(char?[] CharacterFromByte, 
			Dictionary<char, byte> ByteFromCharacter)
		{
			this.chars = CharacterFromByte;
			this.bytes = ByteFromCharacter;
		}

		/// <summary>
		/// Calculates the number of bytes produced by encoding the characters in the 
		/// specified string.
		/// </summary>
		/// <param name="Chars">The characters to encode.</param>
		/// <param name="Index">The index of the first character to encode.</param>
		/// <param name="Count">The number of characters to encode.</param>
		/// <returns>The number of bytes produced by encoding the characters.</returns>
		public override int GetByteCount(char[] Chars, int Index, int Count)
		{
			return Count;
		}

		/// <summary>
		/// Encodes a set of characters from the specified character array into the 
		/// specified byte array.
		/// </summary>
		/// <param name="Chars">The characters to encode.</param>
		/// <param name="CharIndex">The index of the first character to encode.</param>
		/// <param name="CharCount">The number of characters to encode.</param>
		/// <param name="Bytes">The byte array to receive the encoded characters.</param>
		/// <param name="ByteIndex">The index of the first byte to write.</param>
		/// <returns>The number of bytes written.</returns>
		public override int GetBytes(char[] Chars, int CharIndex, int CharCount,
			byte[] Bytes, int ByteIndex)
		{
			int Start = ByteIndex;

			foreach (char ch in Chars)
			{
				if (bytes.TryGetValue(ch, out byte b))
					Bytes[ByteIndex++] = b;
			}

			return ByteIndex - Start;
		}

		/// <summary>
		/// Calculates the number of characters produced by decoding a sequence of bytes 
		/// from the specified byte array.
		/// </summary>
		/// <param name="Bytes">The byte array containing the sequence of bytes to decode.</param>
		/// <param name="Index">The index of the first byte to decode.</param>
		/// <param name="Count">The number of bytes to decode.</param>
		/// <returns>The number of characters produced by decoding the sequence of bytes.</returns>
		public override int GetCharCount(byte[] Bytes, int Index, int Count)
		{
			int Result = 0;

			while (Count-- > 0)
			{
				if (chars[Bytes[Index++]].HasValue)
					Result++;
			}

			return Result;
		}

		/// <summary>
		/// Decodes a sequence of bytes from the specified byte array into the specified 
		/// character array.
		/// </summary>
		/// <param name="Bytes">The byte array containing the sequence of bytes to decode.</param>
		/// <param name="ByteIndex">The index of the first byte to decode.</param>
		/// <param name="ByteCount">The number of bytes to decode.</param>
		/// <param name="Chars">The character array to receive the decoded characters.</param>
		/// <param name="CharIndex">The index of the first character to write.</param>
		/// <returns>The number of characters written.</returns>
		public override int GetChars(byte[] Bytes, int ByteIndex, int ByteCount, 
			char[] Chars, int CharIndex)
		{
			int Start = CharIndex;

			while (ByteCount-- > 0)
			{
				char? ch = chars[Bytes[ByteIndex++]];
				if (ch.HasValue)
					Chars[CharIndex++] = ch.Value;
			}

			return CharIndex - Start;
		}

		/// <summary>
		/// Calculates the maximum number of bytes produced by encoding the specified 
		/// number of characters.
		/// </summary>
		/// <param name="CharCount">The number of characters to encode.</param>
		/// <returns>The maximum number of bytes produced.</returns>
		public override int GetMaxByteCount(int CharCount)
		{
			return CharCount;
		}

		/// <summary>
		/// Calculates the maximum number of characters produced by decoding the specified
		/// number of bytes.
		/// </summary>
		/// <param name="ByteCount">The number of bytes to decode.</param>
		/// <returns>The maximum number of characters produced.</returns>
		public override int GetMaxCharCount(int ByteCount)
		{
			return ByteCount;
		}
	}
}

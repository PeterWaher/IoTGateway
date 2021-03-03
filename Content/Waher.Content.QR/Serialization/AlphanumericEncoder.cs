using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Serialization
{
	/// <summary>
	/// Encodes alphanumeric strings
	/// </summary>
	public class AlphanumericEncoder : ITextEncoder
	{
		/// <summary>
		/// 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:
		/// </summary>
		public const string AlphanumericCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

		private readonly BitWriter output;

		/// <summary>
		/// Encodes alphanumeric strings
		/// </summary>
		/// <param name="Output">Binary output.</param>
		public AlphanumericEncoder(BitWriter Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Checks if a text string can be encoded using the alphanumeric encoding.
		/// </summary>
		/// <param name="Text">Text string to encode.</param>
		/// <returns>If the encoder can be used.</returns>
		public static bool CanEncode(string Text)
		{
			foreach (char ch in Text)
			{
				if (AlphanumericCharacters.IndexOf(ch) < 0)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Encodes a string.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		public void Encode(string Text)
		{
			int i = 0;
			int j;
			int c = 0;

			foreach (char ch in Text)
			{
				j = AlphanumericCharacters.IndexOf(ch);
				if (j < 0)
					continue;

				i *= 45;
				i += j;

				if (++c >= 2)
				{
					this.output.WriteBits((uint)i, 11);
					i = c = 0;
				}
			}

			if (c == 1)
				this.output.WriteBits((uint)i, 6);
		}

		/// <summary>
		/// Gets the number of bytes required to encode an alphanumeric message
		/// containing a specific number of characters.
		/// </summary>
		/// <param name="NrCharacters">Number of alphanumeric characters.</param>
		/// <returns>Number of bytes required.</returns>
		public static int GetByteLength(int NrCharacters)
		{
			int NrBits = 11 * (NrCharacters / 2);
			if ((NrCharacters & 1) != 0)
				NrBits += 6;

			return (NrBits + 7) / 8;
		}
	}
}

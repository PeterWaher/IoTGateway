using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Serialization
{
	/// <summary>
	/// Encodes numeric strings
	/// </summary>
	public class NumericEncoder : ITextEncoder
	{
		private readonly BitWriter output;

		/// <summary>
		/// Encodes numeric strings
		/// </summary>
		/// <param name="Output">Binary output.</param>
		public NumericEncoder(BitWriter Output)
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
				if (ch < '0' || ch > '9')
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
			int c = 0;

			foreach (char ch in Text)
			{
				if (ch < '0' || ch > '9')
					continue;

				i *= 10;
				i += ch - '0';

				if (++c >= 3)
				{
					this.output.WriteBits((uint)i, 10);
					i = c = 0;
				}
			}

			switch (c)
			{
				case 1:
					this.output.WriteBits((uint)i, 4);
					break;

				case 2:
					this.output.WriteBits((uint)i, 7);
					break;
			}
		}

		/// <summary>
		/// Gets the number of bytes required to encode an alphanumeric message
		/// containing a specific number of characters.
		/// </summary>
		/// <param name="NrCharacters">Number of alphanumeric characters.</param>
		/// <returns>Number of bytes required.</returns>
		public static int GetByteLength(int NrCharacters)
		{
			int NrBits = 10 * (NrCharacters / 3);

			switch ((NrCharacters & 3))
			{
				case 1:
					NrBits += 4;
					break;

				case 2:
					NrBits += 7;
					break;
			}

			return (NrBits + 7) / 8;
		}

	}
}

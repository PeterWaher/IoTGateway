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
		private const string alphanumericCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

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
				if (alphanumericCharacters.IndexOf(ch) < 0)
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
				j = alphanumericCharacters.IndexOf(ch);
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
	}
}

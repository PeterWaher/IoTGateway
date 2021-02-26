using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Serialization
{
	/// <summary>
	/// Encodes alphanumeric strings from the ISO 8859-1 character set (by default).
	/// If that fails, UTF-8 encoding will be used.
	/// </summary>
	public class ByteEncoder : ITextEncoder
	{
		private readonly System.Text.Encoding iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");
		private readonly BitWriter output;

		/// <summary>
		/// Encodes alphanumeric strings from the ISO 8859-1 character set (by default).
		/// If that fails, UTF-8 encoding will be used.
		/// </summary>
		/// <param name="Output">Binary output.</param>
		public ByteEncoder(BitWriter Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Encodes a string.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <returns>If encoding was possible.</returns>
		public bool Encode(string Text)
		{
			byte[] Bin = iso_8859_1.GetBytes(Text);
			if (iso_8859_1.GetString(Bin) != Text)
				Bin = System.Text.Encoding.UTF8.GetBytes(Text);

			foreach (byte b in Bin)
				this.output.WriteBits(b, 8);

			return true;
		}
	}
}

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
		/// Checks if a text string can be encoded using the alphanumeric encoding.
		/// </summary>
		/// <param name="Text">Text string to encode.</param>
		/// <returns>If the encoder can be used.</returns>
		public static bool CanEncode(string _)
		{
			return true;
		}

		/// <summary>
		/// Gets the byte representation of a string.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <returns>Byte representation.</returns>
		public byte[] GetBytes(string Text)
		{
			byte[] Bin = iso_8859_1.GetBytes(Text);
			if (iso_8859_1.GetString(Bin) != Text)
				Bin = System.Text.Encoding.UTF8.GetBytes(Text);

			return Bin;
		}

		/// <summary>
		/// Encodes a string.
		/// </summary>
		/// <param name="Bin">Binary messate to encode.</param>
		public void Encode(byte[] Bin)
		{
			foreach (byte b in Bin)
				this.output.WriteBits(b, 8);
		}

		/// <summary>
		/// Encodes a string.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		public void Encode(string Text)
		{
			byte[] Bin = iso_8859_1.GetBytes(Text);
			if (iso_8859_1.GetString(Bin) != Text)
				Bin = System.Text.Encoding.UTF8.GetBytes(Text);

			this.Encode(Bin);
		}
	}
}

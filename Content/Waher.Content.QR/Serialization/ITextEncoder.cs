using System;

namespace Waher.Content.QR.Serialization
{
	/// <summary>
	/// Interface for text encoders.
	/// </summary>
	public interface ITextEncoder
	{
		/// <summary>
		/// Encodes a string.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		/// <returns>If encoding was possible.</returns>
		bool Encode(string Text);
	}
}

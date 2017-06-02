using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content encoders. A class implementing this interface and having a default constructor, will be able
	/// to partake in object encodings through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentEncoder
	{
		/// <summary>
		/// Supported content types.
		/// </summary>
		string[] ContentTypes
		{
			get;
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		string[] FileExtensions
		{
			get;
		}

		/// <summary>
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes);

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="ContentType">Content Type of encoding. Includes information about any text encodings used.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes);

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		bool TryGetContentType(string FileExtension, out string ContentType);

	}
}

using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Binary
{
	/// <summary>
	/// Encodes custom-encoded objects
	/// </summary>
	public class CustomEncoder : IContentEncoder
	{
		/// <summary>
		/// Encodes custom-encoded objects
		/// </summary>
		public CustomEncoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => Array.Empty<string>();

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => Array.Empty<string>();

		/// <summary>
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is CustomEncoding CustomEncoding && InternetContent.IsAccepted(CustomEncoding.ContentType, AcceptedContentTypes))
			{
				Grade = Grade.Perfect;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (Object is CustomEncoding CustomEncoding)
				return Task.FromResult(new ContentResponse(CustomEncoding.ContentType, CustomEncoding, CustomEncoding.Encoded));
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object))));
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			ContentType = string.Empty;
			return false;
		}

		/// <summary>
		/// Tries to get the file extension of an item, given its Content-Type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			FileExtension = string.Empty;
			return false;
		}

	}
}

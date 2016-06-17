using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Binary
{
	/// <summary>
	/// Binary decoder.
	/// </summary>
	public class BinaryDecoder : IContentDecoder
	{
		/// <summary>
		/// Binary decoder.
		/// </summary>
		public BinaryDecoder()
		{
		}

		/// <summary>
		/// Binary content types.
		/// </summary>
		public static readonly string[] BinaryContentTypes = new string[]
		{
			"binary",
			"application/octet-stream"
		};

		/// <summary>
		/// Binary content types.
		/// </summary>
		public static readonly string[] BinaryFileExtensions = new string[]
		{
			"bin"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return BinaryContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return BinaryFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf<string>(BinaryContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Ok;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields)
		{
			return Data;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "bin")
			{
				ContentType = "application/octet-stream";
				return true;
			}
			else
			{
				ContentType = string.Empty;
				return false;
			}
		}
	}
}

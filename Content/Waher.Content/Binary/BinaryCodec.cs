using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Binary
{
	/// <summary>
	/// Binary codec.
	/// </summary>
	public class BinaryCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Binary decoder.
		/// </summary>
		public BinaryCodec()
		{
		}

		/// <summary>
		/// text/plain
		/// </summary>
		public const string DefaultContentType = "application/octet-stream";

		/// <summary>
		/// Binary content types.
		/// </summary>
		public static readonly string[] BinaryContentTypes = new string[]
		{
			DefaultContentType,
			"binary"
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
		public string[] ContentTypes => BinaryContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => BinaryFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(BinaryContentTypes, ContentType) >= 0)
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
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			return Task.FromResult(new ContentResponse(ContentType, Data, Data));
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, "bin", true) == 0)
			{
				ContentType = DefaultContentType;
				return true;
			}
			else
			{
				ContentType = string.Empty;
				return false;
			}
		}

		/// <summary>
		/// Tries to get the file extension of an item, given its Content-Type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			switch (ContentType.ToLower())
			{
				case DefaultContentType:
				case "binary":
					FileExtension = "bin";
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

		/// <summary>
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (InternetContent.IsAccepted(BinaryContentTypes, AcceptedContentTypes) &&
				(Object is byte[]))
			{
				Grade = Grade.Ok;
				return true;
			}
			else if (Object is EncodedObject Obj && InternetContent.IsAccepted(new string[] { Obj.ContentType }, AcceptedContentTypes))
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
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (Object is byte[] Bin)
				return Task.FromResult(new ContentResponse(DefaultContentType, Bin, Bin));
			else if (Object is EncodedObject Obj)
				return Task.FromResult(new ContentResponse(Obj.ContentType, Obj.Data, Obj.Data));
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode as binary.", nameof(Object))));
		}
	}
}

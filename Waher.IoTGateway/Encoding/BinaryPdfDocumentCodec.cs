using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Encoding
{
	/// <summary>
	/// Encoder and Decoder of Binary PDF Documents.
	/// </summary>
	public class BinaryPdfDocumentCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// application/pdf
		/// </summary>
		public const string DefaultContentType = "application/pdf";

		/// <summary>
		/// pdf
		/// </summary>
		public const string DefaultFileExtension = "pdf";

		/// <summary>
		/// Binary content types.
		/// </summary>
		public static readonly string[] PdfContentTypes = new string[]
		{
			DefaultContentType
		};

		/// <summary>
		/// Binary content types.
		/// </summary>
		public static readonly string[] PdfFileExtensions = new string[]
		{
			DefaultFileExtension
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => PdfContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => PdfFileExtensions;

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, DefaultFileExtension, true) == 0)
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
					FileExtension = DefaultFileExtension;
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(PdfContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Barely;
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
		///	<param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, System.Text.Encoding Encoding,
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			if (IsPdf(Data))
				return Task.FromResult(new ContentResponse(ContentType, new BinaryPdfDocument(Data), Data));
			else
				return Task.FromResult(new ContentResponse(new Exception("Not a valid PDF document.")));
		}

		/// <summary>
		/// Checks if a Binary BLOB is a PDF document.
		/// </summary>
		/// <param name="Data">Binary representation of PDF document.s</param>
		/// <returns>If the data is a PDF document.</returns>
		public static bool IsPdf(byte[] Data)
		{
			if (Data is null || Data.Length < 12)
				return false;

			string Header = System.Text.Encoding.ASCII.GetString(Data, 0, 10);
			if (!Header.TrimStart().StartsWith("%PDF-"))
				return false;

			string Footer = System.Text.Encoding.ASCII.GetString(
				Data, Data.Length - 10, 10);

			if (!Footer.TrimEnd().EndsWith("%%EOF"))
				return false;

			return true;
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
			if (InternetContent.IsAccepted(PdfContentTypes, AcceptedContentTypes) &&
				(Object is BinaryPdfDocument))
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
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		public Task<ContentResponse> EncodeAsync(object Object, System.Text.Encoding Encoding, ICodecProgress Progress,
			params string[] AcceptedContentTypes)
		{
			if (Object is BinaryPdfDocument Doc)
				return Task.FromResult(new ContentResponse(DefaultContentType, Doc, Doc.Document));
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode as PDF document.", nameof(Object))));
		}
	}
}

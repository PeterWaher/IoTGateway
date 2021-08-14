using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Markdown encoder.
	/// </summary>
	public class MarkdownCodec : IContentDecoder, IContentEncoder
	{
		private static bool allowEncoding = true;
		private static bool locked = false;

		/// <summary>
		/// If raw encoding of web script should be allowed.
		/// </summary>
		/// <param name="Allow">If Raw encoding should be allowed.</param>
		/// <param name="Lock">If settings should be locked.</param>
		public static void AllowRawEncoding(bool Allow, bool Lock)
		{
			if (locked)
				throw new InvalidOperationException("Setting has been locked.");

			allowEncoding = Allow;
			locked = Lock;
		}

		/// <summary>
		/// If Raw encoding is allowed. Can be changed calling <see cref="AllowRawEncoding(bool, bool)"/>.
		/// </summary>
		public static bool IsRawEncodingAllowed => allowEncoding;

		/// <summary>
		/// If the <see cref="IsRawEncodingAllowed"/> setting is locked.
		/// </summary>
		public static bool IsRawEncodingAllowedLocked => locked;

		/// <summary>
		/// Markdown encoder/decoder.
		/// </summary>
		public MarkdownCodec()
		{
		}

		/// <summary>
		/// Markdown content type.
		/// </summary>
		public const string ContentType = "text/markdown";

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return new string[] { ContentType }; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get
			{
				return new string[]
				{
					"md",
					"markdown"
				};
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
			if (Array.IndexOf<string>(ContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Excellent;
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
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			string s = CommonTypes.GetString(Data, Encoding ?? Encoding.UTF8);

			if (BaseUri is null)
				return new MarkdownDocument(s);
			else
				return new MarkdownDocument(s, new MarkdownSettings(), string.Empty, string.Empty, BaseUri.ToString());
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
			if (allowEncoding && Object is MarkdownDocument && InternetContent.IsAccepted(ContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Excellent;
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
		/// <param name="ContentType">Content Type of encoding. Includes information about any text encodings used.</param>
		/// <returns>Encoded object.</returns>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes)
		{
			if (allowEncoding && Object is MarkdownDocument MarkdownDocument)
			{
				if (Encoding is null)
				{
					ContentType = "text/markdown; charset=utf-8";
					return Encoding.UTF8.GetBytes(MarkdownDocument.MarkdownText);
				}
				else
				{
					ContentType = "text/markdown; charset=" + Encoding.WebName;
					return Encoding.GetBytes(MarkdownDocument.MarkdownText);
				}
			}

			throw new ArgumentException("Object not a markdown document.", nameof(Object));
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			switch (FileExtension.ToLower())
			{
				case "md":
				case "markdown":
					ContentType = MarkdownCodec.ContentType;
					return true;

				default:
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
				case MarkdownCodec.ContentType:
					FileExtension = "md";
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

	}
}

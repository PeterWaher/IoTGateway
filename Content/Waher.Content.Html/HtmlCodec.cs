using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML encoder/decoder.
	/// </summary>
	public class HtmlCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// HTML encoder/decoder.
		/// </summary>
		public HtmlCodec()
		{
		}

		/// <summary>
		/// Default Content-Type for HTML: text/html
		/// </summary>
		public const string DefaultContentType = "text/html";

		/// <summary>
		/// HTML content types.
		/// </summary>
		public static readonly string[] HtmlContentTypes = new string[]
		{
			DefaultContentType,
			"application/xhtml+xml"
		};

		/// <summary>
		/// HTML file extensions.
		/// </summary>
		public static readonly string[] HtmlFileExtensions = new string[]
		{
			"htm",
			"html",
			"xhtml",
			"xhtm"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => HtmlContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => HtmlFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(HtmlContentTypes, ContentType) >= 0)
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
			string Html = CommonTypes.GetString(Data, Encoding);
			return Task.FromResult(new ContentResponse(ContentType, new HtmlDocument(Html), Data));
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
				case "htm":
				case "html":
					ContentType = DefaultContentType;
					return true;

				case "xhtml":
				case "xhtm":
					ContentType = "application/xhtml+xml";
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
				case DefaultContentType:
					FileExtension = "html";
					return true;

				case "application/xhtml+xml":
					FileExtension = "xhtml";
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
			if (Object is HtmlDocument || (Object is string s && s.TrimEnd().EndsWith("</html>", StringComparison.OrdinalIgnoreCase)))
			{
				if (InternetContent.IsAccepted(HtmlContentTypes, AcceptedContentTypes))
				{
					Grade = Grade.Ok;
					return true;
				}
			}

			Grade = Grade.NotAtAll;
			return false;
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
			if (!InternetContent.IsAccepted(HtmlContentTypes, out string ContentType, AcceptedContentTypes))
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));

			string Html = null;
			byte[] Bin;

			if (Object is HtmlDocument HtmlDoc)
				Html = HtmlDoc.HtmlText;
			else if (Object is string s)
				Html = s;

			if (Html is null)
				Bin = null;
			else
			{
				if (Encoding is null)
				{
					ContentType += "; charset=utf-8";
					Bin = Encoding.UTF8.GetBytes(Html);
				}
				else
				{
					ContentType += "; charset=" + Encoding.WebName;
					Bin = Encoding.GetBytes(Html);
				}
			}

			return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
		}
	}
}

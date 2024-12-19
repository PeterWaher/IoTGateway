using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Encodes and Decodes RSS documents.
	/// 
	/// Ref:
	/// https://www.rssboard.org/rss-specification
	/// https://www.rssboard.org/rss-profile
	/// </summary>
	public class RssCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// application/rss+xml
		/// 
		/// Ref:
		/// https://www.rssboard.org/rss-mime-type-application.txt
		/// </summary>
		public const string ContentType = "application/rss+xml";

		/// <summary>
		/// rss
		/// </summary>
		public const string FileExtension = "rss";

		private static readonly string[] contentTypes = new string[] { ContentType };
		private static readonly string[] fileExtensions = new string[] { FileExtension };

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => contentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => fileExtensions;

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, RssCodec.FileExtension, true) == 0)
			{
				ContentType = RssCodec.ContentType;
				return true;
			}
			else
			{
				ContentType = null;
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
			if (string.Compare(ContentType, RssCodec.ContentType, true) == 0)
			{
				FileExtension = RssCodec.FileExtension;
				return true;
			}
			else
			{
				FileExtension = null;
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
			if (string.Compare(ContentType, RssCodec.ContentType, true) == 0)
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
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is RssDocument)
			{
				if (InternetContent.IsAccepted(contentTypes, AcceptedContentTypes))
				{
					Grade = Grade.Excellent;
					return true;
				}
			}

			Grade = Grade.NotAtAll;
			return false;
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		///	<param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			string Xml = CommonTypes.GetString(Data, Encoding);
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			Doc.LoadXml(Xml);

			return Task.FromResult(new ContentResponse(ContentType, new RssDocument(Doc, BaseUri), Data));
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
			if (!(Object is RssDocument Doc) ||
				!InternetContent.IsAccepted(contentTypes, out string ContentType, AcceptedContentTypes))
			{
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));
			}

			return XmlCodec.EncodeXmlAsync(Doc.Xml, Encoding, ContentType);
		}
	}
}

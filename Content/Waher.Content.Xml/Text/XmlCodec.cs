using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Xml.Text
{
	/// <summary>
	/// XML encoder/decoder.
	/// </summary>
	public class XmlCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// XML encoder/decoder.
		/// </summary>
		public XmlCodec()
		{
		}

		/// <summary>
		/// Default content type for XML documents.
		/// </summary>
		public const string DefaultContentType = "text/xml";

		/// <summary>
		/// Default content type for XML schema documents.
		/// </summary>
		public const string SchemaContentType = "application/xml";

		/// <summary>
		/// XML content types.
		/// </summary>
		public static readonly string[] XmlContentTypes = new string[]
		{
			DefaultContentType,
			SchemaContentType
		};

		/// <summary>
		/// XML file extensions.
		/// </summary>
		public static readonly string[] XmlFileExtensions = new string[]
		{
			"xml",
			"xsd"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => XmlContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => XmlFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(XmlContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (ContentType.StartsWith("application/") && ContentType.EndsWith("+xml"))
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
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			if (Encoding is null)
			{
				using (MemoryStream ms = new MemoryStream(Data))
				{
					Doc.Load(ms);
				}
			}
			else
			{
				string s = CommonTypes.GetString(Data, Encoding);
				Doc.LoadXml(s);
			}

			return Task.FromResult(new ContentResponse(ContentType, Doc, Data));
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
				case "xml":
					ContentType = DefaultContentType;
					return true;

				case "xsd":
					ContentType = SchemaContentType;
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
			ContentType = ContentType.ToLower();

			switch (ContentType.ToLower())
			{
				case DefaultContentType:
					FileExtension = "xml";
					return true;

				case SchemaContentType:
					FileExtension = "xsd";
					return true;

				default:
					if (ContentType.StartsWith("application/") && ContentType.EndsWith("+xml"))
					{
						FileExtension = ContentType.Substring(12, ContentType.Length - 4 - 12);
						return true;
					}
					else
					{
						FileExtension = string.Empty;
						return false;
					}
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
			if ((Object is XmlDocument || Object is XmlElement || Object is NamedDictionary<string, object>) &&
				InternetContent.IsAccepted(XmlContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Ok;
				return true;
			}

			if ((Object is NamedDictionary<string, IElement>) &&
				InternetContent.IsAccepted(XmlContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Barely;
				return true;
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
			if (InternetContent.IsAccepted(XmlContentTypes, out string ContentType, AcceptedContentTypes))
			{
				if (Object is XmlDocument Doc)
					return EncodeXmlAsync(Doc, Encoding, ContentType);
				else if (Object is XmlElement E)
				{
					Doc = new XmlDocument();
					Doc.AppendChild(Doc.ImportNode(E, true));
					return EncodeXmlAsync(Doc, Encoding, ContentType);
				}
				else if (Object is NamedDictionary<string, object> Obj)
				{
					string Xml = XML.Encode(Obj);
					return EncodeXmlAsync(Xml, Encoding, ContentType);
				}
				else if (Object is NamedDictionary<string, IElement> Obj2)
				{
					string Xml = XML.Encode(NamedDictionary<string, object>.ToNamedDictionary(Obj2));
					return EncodeXmlAsync(Xml, Encoding, ContentType);
				}
			}

			throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));
		}

		/// <summary>
		/// Encodes an XML Document.
		/// </summary>
		/// <param name="Xml">XML Document to encode.</param>
		/// <param name="Encoding">Character encoding to use.</param>
		/// <param name="ContentType">Internet Content-Type to use.</param>
		/// <returns>Encoded document.</returns>
		public static Task<ContentResponse> EncodeXmlAsync(XmlDocument Xml, Encoding Encoding, string ContentType)
		{
			MemoryStream ms = null;
			XmlWriterSettings Settings;
			XmlWriter w = null;
			byte[] Result;

			try
			{
				ms = new MemoryStream();
				Settings = XML.WriterSettings(false, false);

				if (Encoding is null)
				{
					Settings.Encoding = Encoding.UTF8;
					ContentType += "; charset=utf-8";
				}
				else
				{
					Settings.Encoding = Encoding;
					ContentType += "; charset=" + Encoding.WebName;
				}

				w = XmlWriter.Create(ms, Settings);

				Xml.Save(w);
				w.Flush();

				Result = ms.ToArray();
			}
			finally
			{
				w?.Dispose();
				ms?.Dispose();
			}

			return Task.FromResult(new ContentResponse(ContentType, Xml, Result));
		}

		/// <summary>
		/// Encodes an XML Document.
		/// </summary>
		/// <param name="Xml">XML Document to encode.</param>
		/// <param name="Encoding">Character encoding to use.</param>
		/// <param name="ContentType">Internet Content-Type to use.</param>
		/// <returns>Encoded document.</returns>
		public static Task<ContentResponse> EncodeXmlAsync(string Xml, Encoding Encoding, string ContentType)
		{
			byte[] Bin;

			if (Encoding is null)
			{
				ContentType += "; charset=utf-8";
				Bin = Encoding.UTF8.GetBytes(Xml);
			}
			else
			{
				ContentType += "; charset=" + Encoding.WebName;
				Bin = Encoding.GetBytes(Xml);
			}

			return Task.FromResult(new ContentResponse(ContentType, Xml, Bin));
		}
	}
}

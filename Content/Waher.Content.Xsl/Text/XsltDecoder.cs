using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Waher.Runtime.Inventory;

namespace Waher.Content.Xml.Text
{
	/// <summary>
	/// XSLT decoder.
	/// </summary>
	public class XsltDecoder : IContentDecoder
	{
		/// <summary>
		/// XSLT decoder.
		/// </summary>
		public XsltDecoder()
		{
		}

		/// <summary>
		/// XML content types.
		/// </summary>
		public static readonly string[] XmlContentTypes = new string[]
		{
			"text/xsl",
			"application/xslt+xml"
		};

		/// <summary>
		/// XML file extensions.
		/// </summary>
		public static readonly string[] XmlFileExtensions = new string[]
		{
			"xsl",
			"xslt"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return XmlContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return XmlFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf<string>(XmlContentTypes, ContentType) >= 0)
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
			using (Stream f = new MemoryStream(Data))
			{
				using (XmlReader r = XmlReader.Create(f))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return Xslt;
				}
			}
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
				case "xsl":
				case "xslt":
					ContentType = "application/xslt+xml";
					return true;

				default:
					ContentType = string.Empty;
					return false;
			}
		}

	}
}

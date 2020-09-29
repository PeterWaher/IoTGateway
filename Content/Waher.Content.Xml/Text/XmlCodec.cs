using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Waher.Runtime.Inventory;

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
		/// XML content types.
		/// </summary>
		public static readonly string[] XmlContentTypes = new string[]
		{
			"text/xml",
			"application/xml"
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
				string s = Encoding.GetString(Data);
				Doc.LoadXml(s);
			}

			return Doc;
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
					ContentType = "text/xml";
					return true;

				case "xsd":
					ContentType = "application/xml";
					return true;

				default:
					ContentType = string.Empty;
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
			if (Object is XmlDocument)
			{
				if (InternetContent.IsAccepted(XmlContentTypes, AcceptedContentTypes))
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
		/// <param name="ContentType">Content Type of encoding. Includes information about any text encodings used.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes)
		{
			if (this.Encodes(Object, out Grade _, AcceptedContentTypes))
			{
				XmlDocument Doc = (XmlDocument)Object;
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
						ContentType = "text/xml; charset=utf-8";
					}
					else
					{
						Settings.Encoding = Encoding;
						ContentType = "text/xml; charset=" + Encoding.WebName;
					}

					w = XmlWriter.Create(ms, Settings);

					Doc.Save(w);
					w.Flush();

					Result = ms.ToArray();
				}
				finally
				{
					w?.Dispose();
					ms?.Dispose();
				}

				return Result;
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));
		}
	}
}

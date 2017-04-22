using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Text
{
	/// <summary>
	/// Plain text encoder/decoder.
	/// </summary>
	public class PlainTextCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Plain text encoder/decoder.
		/// </summary>
		public PlainTextCodec()
		{
		}

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] PlainTextContentTypes = new string[] 
		{
			"text/plain",
 			"text/css",
			"text/html",
			"application/xhtml+xml",
			"text/sgml",
			"text/tab-separated-values",
			"application/javascript",
			"text/richtext"
		};

		/// <summary>
		/// Plain text file extensions.
		/// </summary>
		public static readonly string[] PlainTextFileExtensions = new string[] 
		{ 
			"txt",
			"text",
			"css",
			"htm",
			"html",
			"xhtml",
			"xhtm",
			"sgml",
			"tsv",
			"js",
			"rtx"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return PlainTextContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return PlainTextFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == "text/plain")
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (ContentType.StartsWith("text/"))
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
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			if (Encoding == null)
				return System.Text.Encoding.UTF8.GetString(Data);
			else
				return Encoding.GetString(Data);
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
				case "txt":
				case "text":
					ContentType = "text/plain";
					return true;

				case "css":
					ContentType = "text/css";
					return true;

				case "htm":
				case "html":
					ContentType = "text/html";
					return true;

				case "xhtml":
				case "xhtm":
					ContentType = "application/xhtml+xml";
					return true;

				case "sgml":
					ContentType = "text/sgml";
					return true;

				case "tsv":
					ContentType = "text/tab-separated-values";
					return true;

				case "js":
					ContentType = "application/javascript";
					return true;

				case "rtx":
					ContentType = "text/richtext";
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
			if (Object is string)
			{
				if (InternetContent.IsAccepted("text/plain", AcceptedContentTypes))
				{
					Grade = Grade.Ok;
					return true;
				}
				else
				{
					foreach (string s in AcceptedContentTypes)
					{
						if (s.StartsWith("text/"))
						{
							Grade = Grade.Barely;
							return true;
						}
					}
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
			if (this.Encodes(Object, out Grade Grade, AcceptedContentTypes))
			{
				if (Encoding == null)
				{
					ContentType = "text/plain; charset=utf-8";
					return System.Text.Encoding.UTF8.GetBytes(Object.ToString());
				}
				else
				{
					ContentType = "text/plain; charset=" + Encoding.WebName;
					return Encoding.GetBytes(Object.ToString());
				}
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", "Object");
		}
	}
}

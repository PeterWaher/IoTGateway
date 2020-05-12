using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Content.Markdown.Web.WebScript
{
	/// <summary>
	/// Web Script encoder/decoder.
	/// </summary>
	public class WsCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Web Script encoder/decoder.
		/// </summary>
		public WsCodec()
		{
		}

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] WsContentTypes = new string[] 
		{
 			"application/x-webscript"
		};

		/// <summary>
		/// Plain text file extensions.
		/// </summary>
		public static readonly string[] WsFileExtensions = new string[] 
		{ 
			"ws"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return WsContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return WsFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == "application/x-webscript")
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
			string s = CommonTypes.GetString(Data, Encoding);
			return new Expression(s);
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
				case "ws":
					ContentType = "application/x-webscript";
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
			if (Object is Expression)
			{
				if (InternetContent.IsAccepted(WsContentTypes, AcceptedContentTypes))
				{
					Grade = Grade.Excellent;
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
			if (InternetContent.IsAccepted(WsContentTypes, out ContentType, AcceptedContentTypes))
			{
				string s = null;

				if (Object is Expression Expression)
					s = Expression.Script;
				else if (Object is string s2)
					s = s2;

				if (s != null)
				{
					if (Encoding is null)
					{
						ContentType += "; charset=utf-8";
						return Encoding.UTF8.GetBytes(s);
					}
					else
					{
						ContentType += "; charset=" + Encoding.WebName;
						return Encoding.GetBytes(s);
					}
				}
			}

			throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));

		}
	}
}

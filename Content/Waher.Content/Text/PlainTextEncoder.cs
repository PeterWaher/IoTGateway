using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Text
{
	/// <summary>
	/// Plain text encoder.
	/// </summary>
	public class PlainTextEncoder : IContentEncoder
	{
		/// <summary>
		/// Plain text encoder.
		/// </summary>
		public PlainTextEncoder()
		{
		}

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] PlainTextContentTypes = new string[] { "text/plain" };

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] PlainTextFileExtensions = new string[] { "txt", "text" };

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
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is string && InternetContent.IsAccepted("text/plain", AcceptedContentTypes))
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
		/// <param name="ContentType">Content Type of encoding. Includes information about any text encodings used.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes)
		{
			if (Object is string && InternetContent.IsAccepted("text/plain", AcceptedContentTypes))
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

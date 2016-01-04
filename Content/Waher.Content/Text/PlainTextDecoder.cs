using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Text
{
	/// <summary>
	/// Plain text encoder.
	/// </summary>
	public class PlainTextDecoder : IContentDecoder
	{
		/// <summary>
		/// Plain text encoder.
		/// </summary>
		public PlainTextDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return PlainTextEncoder.PlainTextContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return PlainTextEncoder.PlainTextFileExtensions; }
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
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding)
		{
			if (Encoding == null)
				return System.Text.Encoding.UTF8.GetString(Data);
			else
				return Encoding.GetString(Data);
		}
	}
}

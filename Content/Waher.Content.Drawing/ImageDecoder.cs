using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Drawing
{
	/// <summary>
	/// Image encoder.
	/// </summary>
	public class ImageDecoder : IContentDecoder
	{
		/// <summary>
		/// Image encoder.
		/// </summary>
		public ImageDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return ImageEncoder.ImageContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return ImageEncoder.ImageFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf<string>(ImageEncoder.ImageContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (ContentType.StartsWith("image/"))
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
			using (MemoryStream ms = new MemoryStream(Data))
			{
				return Image.FromStream(ms);
			}
		}
	}
}

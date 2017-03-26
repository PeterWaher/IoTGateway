using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Drawing
{
	/// <summary>
	/// Image encoder/decoder.
	/// </summary>
	public class ImageCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Image encoder/decoder.
		/// </summary>
		public ImageCodec()
		{
		}

		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageContentTypes = new string[] 
		{
			"image/png", 
			"image/bmp", 
			"image/gif", 
			"image/jpeg", 
			"image/tiff", 
			"image/x-wmf", 
			"image/x-emf", 
			"image/x-icon"
		};

		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageFileExtensions = new string[] 
		{
			"png", 
			"bmp", 
			"gif", 
			"jpg", 
			"jpeg", 
			"tif", 
			"tiff", 
			"wmf", 
			"emf", 
			"ico"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return ImageContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return ImageFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf<string>(ImageContentTypes, ContentType) >= 0)
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
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			using (MemoryStream ms = new MemoryStream(Data))
			{
				return Image.FromStream(ms);
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
			if (Object is Image && InternetContent.IsAccepted(ContentTypes, AcceptedContentTypes))
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
		/// <returns>Encoded object.</returns>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public byte[] Encode(object Object, Encoding Encoding, out string ContentType, params string[] AcceptedContentTypes)
		{
			Image Image = Object as Image;
			if (Image == null)
				throw new ArgumentException("Object not an image derived from System.Drawing.Image.", "Object");

			MemoryStream Output = new MemoryStream();

			if (Image.RawFormat == ImageFormat.Png && InternetContent.IsAccepted("image/png", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/png";
			}
			else if (Image.RawFormat == ImageFormat.Bmp && InternetContent.IsAccepted("image/bmp", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/bmp";
			}
			else if (Image.RawFormat == ImageFormat.Jpeg && InternetContent.IsAccepted("image/jpeg", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/jpeg";
			}
			else if (Image.RawFormat == ImageFormat.Emf && InternetContent.IsAccepted("image/x-emf", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/x-emf";
			}
			else if (Image.RawFormat == ImageFormat.Gif && InternetContent.IsAccepted("image/gif", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/gif";
			}
			else if (Image.RawFormat == ImageFormat.Icon && InternetContent.IsAccepted("image/x-icon", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/x-icon";
			}
			else if (Image.RawFormat == ImageFormat.Tiff && InternetContent.IsAccepted("image/tiff", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/tiff";
			}
			else if (Image.RawFormat == ImageFormat.Wmf && InternetContent.IsAccepted("image/x-wmf", AcceptedContentTypes))
			{
				Image.Save(Output, Image.RawFormat);
				ContentType = "image/x-wmf";
			}
			else if (InternetContent.IsAccepted("image/png", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Png);
				ContentType = "image/png";
			}
			else if (InternetContent.IsAccepted("image/bmp", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Bmp);
				ContentType = "image/bmp";
			}
			else if (InternetContent.IsAccepted("image/jpeg", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Jpeg);
				ContentType = "image/jpeg";
			}
			else if (InternetContent.IsAccepted("image/x-emf", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Emf);
				ContentType = "image/x-emf";
			}
			else if (InternetContent.IsAccepted("image/gif", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Gif);
				ContentType = "image/gif";
			}
			else if (InternetContent.IsAccepted("image/x-icon", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Icon);
				ContentType = "image/x-icon";
			}
			else if (InternetContent.IsAccepted("image/tiff", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Tiff);
				ContentType = "image/tiff";
			}
			else if (InternetContent.IsAccepted("image/x-wmf", AcceptedContentTypes))
			{
				Image.Save(Output, ImageFormat.Wmf);
				ContentType = "image/x-wmf";
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", "Object");

			Output.Capacity = (int)Output.Position;

			return Output.GetBuffer();
		}

		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			switch (FileExtension.ToLower())
			{
				case "png":
					ContentType = "image/png";
					return true;

				case "bmp":
					ContentType = "image/bmp";
					return true;

				case "gif":
					ContentType = "image/gif";
					return true;

				case "jpg":
				case "jpeg":
					ContentType = "image/jpeg";
					return true;

				case "tif":
				case "tiff":
					ContentType = "image/tiff";
					return true;

				case "wmf":
					ContentType = "image/x-wmf";
					return true;

				case "emf":
					ContentType = "image/x-emf";
					return true;
	
				case "ico":
					ContentType = "image/x-icon";
					return true;

				case "svg":
					ContentType = "image/svg+xml";
					return true;

				default:
					ContentType = string.Empty;
					return false;
			}
		}
	}
}

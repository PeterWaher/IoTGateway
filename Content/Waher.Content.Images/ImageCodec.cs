using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content.Images
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
		/// image/webp
		/// </summary>
		public const string ContentTypeWebP = "image/webp";

		/// <summary>
		/// image/png
		/// </summary>
		public const string ContentTypePng = "image/png";

		/// <summary>
		/// image/jpeg
		/// </summary>
		public const string ContentTypeJpg = "image/jpeg";

		/// <summary>
		/// image/x-icon
		/// </summary>
		public const string ContentTypeIcon = "image/x-icon";

		/// <summary>
		/// image/svg+xml
		/// </summary>
		public const string ContentTypeSvg = "image/svg+xml";

		/// <summary>
		/// image/bmp
		/// </summary>
		public const string ContentTypeBmp = "image/bmp";

		/// <summary>
		/// image/gif
		/// </summary>
		public const string ContentTypeGif = "image/gif";

		/// <summary>
		/// image/tiff
		/// </summary>
		public const string ContentTypeTiff = "image/tiff";

		/// <summary>
		/// image/x-wmf
		/// </summary>
		public const string ContentTypeWmf = "image/x-wmf";

		/// <summary>
		/// image/x-emf
		/// </summary>
		public const string ContentTypeEmf = "image/x-emf";

		/// <summary>
		/// png
		/// </summary>
		public const string FileExtensionPng = "png";

		/// <summary>
		/// bmp
		/// </summary>
		public const string FileExtensionBmp = "bmp";

		/// <summary>
		/// gif
		/// </summary>
		public const string FileExtensionGif = "gif";

		/// <summary>
		/// jpg
		/// </summary>
		public const string FileExtensionJpg = "jpg";

		/// <summary>
		/// webp
		/// </summary>
		public const string FileExtensionWebP = "webp";

		/// <summary>
		/// tif
		/// </summary>
		public const string FileExtensionTiff = "tif";

		/// <summary>
		/// wmf
		/// </summary>
		public const string FileExtensionWmf = "wmf";

		/// <summary>
		/// emf
		/// </summary>
		public const string FileExtensionEmf = "emf";

		/// <summary>
		/// ico
		/// </summary>
		public const string FileExtensionIcon = "ico";

		/// <summary>
		/// svg
		/// </summary>
		public const string FileExtensionSvg = "svg";


		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageContentTypes = new string[] 
		{
			ContentTypeWebP, 
			ContentTypePng,
			ContentTypeJpg,
			ContentTypeIcon,
			ContentTypeBmp,
			ContentTypeGif, 
			ContentTypeTiff, 
			ContentTypeWmf, 
			ContentTypeEmf
		};

		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageFileExtensions = new string[] 
		{
			FileExtensionWebP, 
			FileExtensionPng, 
			FileExtensionBmp,
			FileExtensionGif, 
			FileExtensionJpg, 
			"jpeg", 
			FileExtensionTiff, 
			"tiff",
			FileExtensionWmf, 
			FileExtensionEmf, 
			FileExtensionIcon
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => ImageContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => ImageFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(ImageContentTypes, ContentType) >= 0)
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
		public Task<object> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			SKBitmap Bitmap = SKBitmap.Decode(Data);
			return Task.FromResult<object>(SKImage.FromBitmap(Bitmap));
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
			if ((Object is SKImage || Object is SKBitmap) && InternetContent.IsAccepted(this.ContentTypes, AcceptedContentTypes))
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
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public Task<KeyValuePair<byte[], string>> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			SKData Data;
			bool Dispose = false;
			string ContentType;
			byte[] Bin;

			if (!(Object is SKImage Image))
			{
				if (Object is SKBitmap Bitmap)
				{
					Image = SKImage.FromBitmap(Bitmap);
					Dispose = true;
				}
				else
					throw new ArgumentException("Object not an image derived from SkiaSharp.SKImage or SkiaSharp.SKBitmap.", nameof(Object));
			}

			if (InternetContent.IsAccepted(ContentTypeWebP, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Webp, 100);
				ContentType = ContentTypeWebP;
			}
			else if (InternetContent.IsAccepted(ContentTypePng, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Png, 100);
				ContentType = ContentTypePng;
			}
			else if (InternetContent.IsAccepted(ContentTypeBmp, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Bmp, 100);
				ContentType = ContentTypeBmp;
			}
			else if (InternetContent.IsAccepted(ContentTypeJpg, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Jpeg, 90);
				ContentType = ContentTypeJpg;
			}
			else if (InternetContent.IsAccepted(ContentTypeGif, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Gif, 100);
				ContentType = ContentTypeGif;
			}
			else if (InternetContent.IsAccepted(ContentTypeIcon, AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Ico, 100);
				ContentType = ContentTypeIcon;
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));

			Bin = Data.ToArray();
			Data.Dispose();

			if (Dispose)
				Image.Dispose();

			return Task.FromResult(new KeyValuePair<byte[], string>(Bin, ContentType));
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
				case FileExtensionPng:
					ContentType = ContentTypePng;
					return true;

				case FileExtensionBmp:
					ContentType = ContentTypeBmp;
					return true;

				case FileExtensionGif:
					ContentType = ContentTypeGif;
					return true;

				case FileExtensionJpg:
				case "jpeg":
					ContentType = ContentTypeJpg;
					return true;

				case FileExtensionWebP:
					ContentType = ContentTypeWebP;
					return true;

				case FileExtensionTiff:
				case "tiff":
					ContentType = ContentTypeTiff;
					return true;

				case FileExtensionWmf:
					ContentType = ContentTypeWmf;
					return true;

				case FileExtensionEmf:
					ContentType = ContentTypeEmf;
					return true;
	
				case FileExtensionIcon:
					ContentType = ContentTypeIcon;
					return true;

				case FileExtensionSvg:
					ContentType = ContentTypeSvg;
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
			switch (ContentType.ToLower())
			{
				case ContentTypePng:
					FileExtension = FileExtensionPng;
					return true;

				case ContentTypeBmp:
					FileExtension = FileExtensionBmp;
					return true;

				case ContentTypeGif:
					FileExtension = FileExtensionGif;
					return true;

				case ContentTypeJpg:
					FileExtension = FileExtensionJpg;
					return true;

				case ContentTypeWebP:
					FileExtension = FileExtensionWebP;
					return true;

				case ContentTypeTiff:
					FileExtension = FileExtensionTiff;
					return true;

				case ContentTypeWmf:
					FileExtension = FileExtensionWmf;
					return true;

				case ContentTypeEmf:
					FileExtension = FileExtensionEmf;
					return true;

				case ContentTypeIcon:
					FileExtension = FileExtensionIcon;
					return true;

				case ContentTypeSvg:
					FileExtension = FileExtensionSvg;
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

	}
}

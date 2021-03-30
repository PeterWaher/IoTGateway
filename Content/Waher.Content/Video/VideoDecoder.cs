using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Video
{
	/// <summary>
	/// Binary video decoder. Is used to identify video content, but does not have actual decoding of corresponding video formats.
	/// </summary>
	public class VideoDecoder : IContentDecoder
	{
		/// <summary>
		/// Binary video decoder. Is used to identify video content, but does not have actual decoding of corresponding video formats.
		/// </summary>
		public VideoDecoder()
		{
		}

		/// <summary>
		/// Video content types.
		/// </summary>
		public static readonly string[] VideoContentTypes = new string[] 
		{
			"video/mp4", 
			"video/mpeg", 
			"video/ogg", 
			"video/quicktime", 
			"video/webm", 
			"video/3gpp", 
			"video/3gpp2", 
			"video/x-la-asf", 
			"video/x-ms-asf", 
			"video/x-msvideo", 
			"video/x-sgi-movie",
			"video/x-flv",
			"video/x-ms-wmv"
		};

		/// <summary>
		/// Video content types.
		/// </summary>
		public static readonly string[] VideoFileExtensions = new string[] 
		{
			"mp4",
			"m4a",
			"m4p",
			"m4b",
			"m4r",
			"m4v",
			"mp2", 
			"mpa", 
			"mpe", 
			"mpeg", 
			"mpg", 
			"mpv2", 
			"ogv", 
			"mov", 
			"qt", 
			"webm", 
			"lsf", 
			"lsx", 
			"asf",
			"asr",
			"asx",
			"avi",
			"movie",
			"3gp",
			"3g2",
			"flv",
			"wmv"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return VideoContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return VideoFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType.StartsWith("video/"))
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
			return Data;
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
				case "mp4":
				case "m4a":
				case "m4p":
				case "m4b":
				case "m4r":
				case "m4v":
					ContentType = "video/mp4";
					return true;
				
				case "mp2":
				case "mpa":
				case "mpe":
				case "mpeg":
				case "mpg":
				case "mpv2":
					ContentType = "video/mpeg";
					return true;

				case "ogv":
					ContentType = "video/ogg";
					return true;

				case "mov":
				case "qt":
					ContentType = "video/quicktime";
					return true;

				case "webm":
					ContentType = "video/webm";
					return true;

				case "lsf":
				case "lsx":
					ContentType = "video/x-la-asf";
					return true;

				case "asf":
				case "asr":
				case "asx":
					ContentType = "video/x-ms-asf";
					return true;

				case "avi":
					ContentType = "video/x-msvideo";
					return true;

				case "movie":
					ContentType = "video/x-sgi-movie";
					return true;
					
				case "3gp":
					ContentType = "video/3gpp";
					return true;

				case "3g2":
					ContentType = "video/3gpp2";
					return true;

				case "flv":
					ContentType = "video/x-flv";
					return true;

				case "wmv":
					ContentType = "video/x-wmv";
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
				case "video/mp4":
					FileExtension = "mp4";
					return true;

				case "video/mpeg":
					FileExtension = "mpg";
					return true;

				case "video/ogg":
					FileExtension = "ogv";
					return true;

				case "video/quicktime":
					FileExtension = "mov";
					return true;

				case "video/webm":
					FileExtension = "webm";
					return true;

				case "video/x-la-asf":
					FileExtension = "lsf";
					return true;

				case "video/x-ms-asf":
					FileExtension = "asf";
					return true;

				case "video/x-msvideo":
					FileExtension = "avi";
					return true;

				case "video/x-sgi-movie":
					FileExtension = "movie";
					return true;

				case "video/3gpp":
					FileExtension = "3gp";
					return true;

				case "video/3gpp2":
					FileExtension = "3g2";
					return true;

				case "video/x-flv":
					FileExtension = "flv";
					return true;

				case "video/x-wmv":
					FileExtension = "wmv";
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content.Audio
{
	/// <summary>
	/// Binary audio decoder. Is used to identify audio content, but does not have actual decoding of corresponding audio formats.
	/// </summary>
	public class AudioDecoder : IContentDecoder
	{
		/// <summary>
		/// Binary audio decoder. Is used to identify audio content, but does not have actual decoding of corresponding audio formats.
		/// </summary>
		public AudioDecoder()
		{
		}

		/// <summary>
		/// Audio content types.
		/// </summary>
		public static readonly string[] AudioContentTypes = new string[] 
		{
			"audio/basic", 
			"audio/mid", 
			"audio/mpeg", 
			"audio/ogg", 
			"audio/x-aiff", 
			"audio/x-mpegurl", 
			"audio/x-pn-realaudio", 
			"audio/x-pn-realaudio", 
			"audio/x-wav"
		};

		/// <summary>
		/// Audio content types.
		/// </summary>
		public static readonly string[] AudioFileExtensions = new string[] 
		{
			"au", 
			"snd", 
			"mid", 
			"rmi", 
			"mp3", 
			"ogg", 
			"oga", 
			"aif", 
			"aifc", 
			"aiff", 
			"m3u", 
			"ra", 
			"ram",
			"wav"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return AudioContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return AudioFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType.StartsWith("audio/"))
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
				case "au":
				case "snd":
					ContentType = "audio/basic";
					return true;

				case "mid":
				case "rmi":
					ContentType = "audio/mid";
					return true;

				case "mp3":
					ContentType = "audio/mpeg";
					return true;

				case "ogg":
				case "oga":
					ContentType = "audio/ogg";
					return true;

				case "aif":
				case "aifc":
				case "aiff":
					ContentType = "audio/x-aiff";
					return true;

				case "m3u":
					ContentType = "audio/x-mpegurl";
					return true;

				case "ra":
					ContentType = "audio/x-pn-realaudio";
					return true;

				case "ram":
					ContentType = "audio/x-pn-realaudio";
					return true;

				case "wav":
					ContentType = "audio/x-wav";
					return true;

				default:
					ContentType = string.Empty;
					return false;
			}
		}
	}
}

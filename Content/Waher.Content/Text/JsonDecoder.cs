using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Text
{
	/// <summary>
	/// JSON decoder.
	/// </summary>
	public class JsonDecoder : IContentDecoder
	{
		/// <summary>
		/// JSON decoder.
		/// </summary>
		public JsonDecoder()
		{
		}

		/// <summary>
		/// JSON content types.
		/// </summary>
		public static readonly string[] JsonContentTypes = new string[]
		{
			"application/json",
			"text/x-json"
		};

		/// <summary>
		/// JSON file extensions.
		/// </summary>
		public static readonly string[] JsonFileExtensions = new string[]
		{
			"json"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return JsonContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return JsonFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == "application/json" || ContentType == "text/x-json")
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
			string s;

			if (Encoding == null)
				s = Encoding.UTF8.GetString(Data);
			else
				s = Encoding.GetString(Data);

			return JSON.Parse(s);
		}


		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "json")
			{
				ContentType = "application/json";
				return true;
			}
			else
			{
				ContentType = null;
				return false;
			}
		}
	}
}
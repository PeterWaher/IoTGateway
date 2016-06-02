using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Script;

namespace Waher.Networking.HTTP.Multipart
{
	/// <summary>
	/// Decoder of URL encoded web forms.
	/// </summary>
	public class WwwFormDecoder : IContentDecoder
	{
		public const string ContentType = "application/x-www-form-urlencoded";

		/// <summary>
		/// Decoder of URL encoded web forms.
		/// </summary>
		public WwwFormDecoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get
			{
				return new string[]
				{
					ContentType
				};
			}
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get
			{
				return new string[]
				{
					"webform"
				};
			}
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == WwwFormDecoder.ContentType)
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
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public object Decode(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields)
		{
			Dictionary<string, string> Form = new Dictionary<string, string>();
			string s = Encoding.GetString(Data);
			string Key, Value;
			int i;

			foreach (string Parameter in s.Split('&'))
			{
				if (string.IsNullOrEmpty(Parameter))
					continue;

				i = Parameter.IndexOf('=');

				Key = Uri.UnescapeDataString(Parameter.Substring(0, i));
				Value = Uri.UnescapeDataString(Parameter.Substring(i + 1));

				Form[Key] = Value;
			}

			return Form;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "webform")
			{
				ContentType = WwwFormDecoder.ContentType;
				return true;
			}
			else
			{
				ContentType = string.Empty;
				return false;
			}
		}
	}
}

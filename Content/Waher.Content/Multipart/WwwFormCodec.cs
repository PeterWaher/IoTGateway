using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Decoder of URL encoded web forms.
	/// </summary>
	public class WwwFormCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// application/x-www-form-urlencoded
		/// </summary>
		public const string ContentType = "application/x-www-form-urlencoded";

		/// <summary>
		/// Decoder of URL encoded web forms.
		/// </summary>
		public WwwFormCodec()
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
			if (ContentType == WwwFormCodec.ContentType)
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
			Dictionary<string, string> Form = new Dictionary<string, string>();
			string s = CommonTypes.GetString(Data, Encoding);
			string Key, Value;
			int i;

			foreach (string Parameter in s.Split('&'))
			{
				if (string.IsNullOrEmpty(Parameter))
					continue;

				i = Parameter.IndexOf('=');

				Key = Uri.UnescapeDataString(Parameter.Substring(0, i).Replace("+", " "));
				Value = Uri.UnescapeDataString(Parameter.Substring(i + 1).Replace("+", " "));

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
				ContentType = WwwFormCodec.ContentType;
				return true;
			}
			else
			{
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
				case WwwFormCodec.ContentType:
					FileExtension = "webform";
					return true;

				default:
					FileExtension = string.Empty;
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
			if (Object is Dictionary<string, string> &&
				InternetContent.IsAccepted(ContentTypes, AcceptedContentTypes))
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
			if (Object is Dictionary<string, string> Form &&
				InternetContent.IsAccepted(ContentTypes, AcceptedContentTypes))
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (KeyValuePair<string, string> Pair in Form)
				{
					if (First)
						First = false;
					else
						sb.Append('&');

					sb.Append(Uri.EscapeDataString(Pair.Key));
					sb.Append('=');
					sb.Append(Uri.EscapeDataString(Pair.Value));
				}

				if (Encoding is null)
				{
					ContentType = WwwFormCodec.ContentType + "; charset=utf-8";
					return Encoding.UTF8.GetBytes(sb.ToString());
				}
				else
				{
					ContentType = WwwFormCodec.ContentType + "; charset=" + Encoding.WebName;
					return Encoding.GetBytes(sb.ToString());
				}
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));
		}

	}
}

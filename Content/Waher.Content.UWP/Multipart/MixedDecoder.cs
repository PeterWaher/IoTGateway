using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Script;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Decoder of mixed data.
	/// 
	/// http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
	/// </summary>
	public class MixedDecoder : IContentDecoder
	{
		public const string ContentType = "multipart/mixed";

		/// <summary>
		/// Decoder of mixed data.
		/// 
		/// http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
		/// </summary>
		public MixedDecoder()
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
					"mixed"
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
			if (ContentType == FormDataDecoder.ContentType)
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
			List<object> List = new List<object>();

			FormDataDecoder.Decode(Data, Fields, null, List);

			return List;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (FileExtension.ToLower() == "mixed")
			{
				ContentType = FormDataDecoder.ContentType;
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

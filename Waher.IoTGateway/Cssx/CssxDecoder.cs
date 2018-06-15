using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Cssx
{
	/// <summary>
	/// CSSX encoder/decoder.
	/// </summary>
	public class CssxDecoder : IContentDecoder
	{
		/// <summary>
		/// CSSX encoder/decoder.
		/// </summary>
		public CssxDecoder()
		{
		}

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] CssxContentTypes = new string[] 
		{
 			"text/x.cssx"
		};

		/// <summary>
		/// Plain text file extensions.
		/// </summary>
		public static readonly string[] CssxFileExtensions = new string[] 
		{ 
			"cssx"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes
		{
			get { return CssxContentTypes; }
		}

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions
		{
			get { return CssxFileExtensions; }
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == "text/x.cssx")
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
			if (Encoding == null)
				return System.Text.Encoding.UTF8.GetString(Data);
			else
				return Encoding.GetString(Data);
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
				case "cssx":
					ContentType = "text/x.cssx";
					return true;

				default:
					ContentType = string.Empty;
					return false;
			}
		}
	}
}

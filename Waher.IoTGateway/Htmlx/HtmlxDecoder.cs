﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.IoTGateway.Htmlx
{
	/// <summary>
	/// HTMLX decoder.
	/// </summary>
	public class HtmlxDecoder : IContentDecoder
	{
		/// <summary>
		/// HTMLX encoder/decoder.
		/// </summary>
		public HtmlxDecoder()
		{
		}

		/// <summary>
		/// Default Content-Type for HTMLX: text/x-htmlx
		/// </summary>
		public const string DefaultContentType = "text/x-htmlx";

		/// <summary>
		/// Plain text content types.
		/// </summary>
		public static readonly string[] HtmlxContentTypes = new string[] { DefaultContentType };

		/// <summary>
		/// Plain text file extensions.
		/// </summary>
		public static readonly string[] HtmlxFileExtensions = new string[] { "htmlx" };

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => HtmlxContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => HtmlxFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == DefaultContentType)
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
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, System.Text.Encoding Encoding, 
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			return Task.FromResult(new ContentResponse(ContentType, Strings.GetString(Data, Encoding), Data));
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
				case "htmlx":
					ContentType = DefaultContentType;
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
				case DefaultContentType:
					FileExtension = "htmlx";
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

	}
}

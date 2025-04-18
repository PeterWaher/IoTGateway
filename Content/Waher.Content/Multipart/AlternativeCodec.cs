﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Multipart
{
	/// <summary>
	/// Codec of alternative data.
	/// 
	/// http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
	/// </summary>
	public class AlternativeCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// multipart/alternative
		/// </summary>
		public const string ContentType = "multipart/alternative";

		/// <summary>
		/// Codec of alternative data.
		/// 
		/// http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
		/// </summary>
		public AlternativeCodec()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => contentTypes;

		private static readonly string[] contentTypes = new string[] { ContentType };

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => new string[] { "alternative" };

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == AlternativeCodec.ContentType)
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
		public async Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding,
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			ChunkedList<EmbeddedContent> List = new ChunkedList<EmbeddedContent>();

			Exception Error = await FormDataDecoder.Decode(Data, Fields, null, List, BaseUri, Progress);

			if (Error is null)
				return new ContentResponse(ContentType, new ContentAlternatives(List.ToArray()), Data);
			else
				return new ContentResponse(Error);
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, "alternative", true) == 0)
			{
				ContentType = AlternativeCodec.ContentType;
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
				case AlternativeCodec.ContentType:
					FileExtension = "alternative";
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
			if (Object is ContentAlternatives &&
				InternetContent.IsAccepted(contentTypes, AcceptedContentTypes))
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
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		public async Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding,
			ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (Object is ContentAlternatives Alternatives &&
				InternetContent.IsAccepted(contentTypes, AcceptedContentTypes))
			{
				string Boundary = Guid.NewGuid().ToString();
				string ContentType = AlternativeCodec.ContentType + "; boundary=\"" + Boundary + "\"";
				return new ContentResponse(ContentType, Object,
					await FormDataDecoder.Encode(Alternatives.Content, Boundary, Progress));
			}
			else
				return new ContentResponse(new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object)));
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;

namespace Waher.Security.JWT
{
	/// <summary>
	/// A JWT content coder/decoder.
	/// </summary>
	public class JwtCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// application/jwt
		/// </summary>
		public const string ContentType = "application/jwt";

		/// <summary>
		/// A JWT content coder/decoder.
		/// </summary>
		public JwtCodec()
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
		public string[] FileExtensions => new string[] { "jwt" };

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		///	<param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding,
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			string Token = (Encoding ?? Encoding.ASCII).GetString(Data);

			if (JwtToken.TryParse(Token, out JwtToken ParsedToken, out string Reason))
				return Task.FromResult(new ContentResponse(ContentType, ParsedToken, Data));
			else
				return Task.FromResult(new ContentResponse(new BadRequestException(Reason)));
		}

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == JwtCodec.ContentType)
			{
				Grade = Grade.Perfect;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, "jwt", true) == 0)
			{
				ContentType = JwtCodec.ContentType;
				return true;
			}
			else
			{
				ContentType = null;
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
				case JwtCodec.ContentType:
					FileExtension = "jwt";
					return true;

				default:
					FileExtension = string.Empty;
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
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (!(Object is JwtToken Token))
				return Task.FromResult(new ContentResponse(new ArgumentException("Object not a JWT token.", nameof(Object))));

			string ContentType = JwtCodec.ContentType;
			string s = Token.Header + "." + Token.Payload + "." + Token.Signature;
			byte[] Bin;

			if (Encoding is null)
				Bin = Encoding.ASCII.GetBytes(s);
			else
				Bin = Encoding.GetBytes(s);

			return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
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
			if (Object is JwtToken && InternetContent.IsAccepted(ContentType, AcceptedContentTypes))
			{
				Grade = Grade.Perfect;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}
	}
}

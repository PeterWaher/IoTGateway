﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Content.Binary
{
	/// <summary>
	/// Encodes files.
	/// </summary>
	public class FileEncoder : IContentEncoder
	{
		/// <summary>
		/// Encodes files.
		/// </summary>
		public FileEncoder()
		{
		}

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => Array.Empty<string>();

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => Array.Empty<string>();

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
			if (Object is FileReference Ref)
			{
				using (FileStream f = File.OpenRead(Ref.FileName))
				{
					byte[] Bin = await f.ReadAllAsync();
					return new ContentResponse(Ref.ContentType, Object, Bin);
				}
			}
			else
				return new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object)));
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
			if (Object is FileReference Ref)
			{
				if (InternetContent.IsAccepted(Ref.ContentType, AcceptedContentTypes))
				{
					Grade = Grade.Ok;
					return true;
				}
			}

			Grade = Grade.NotAtAll;
			return false;
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			ContentType = null;
			return false;
		}

		/// <summary>
		/// Tries to get the file extension of an item, given its Content-Type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			FileExtension = null;
			return false;
		}
	}
}

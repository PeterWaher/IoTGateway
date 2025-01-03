using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Text
{
	/// <summary>
	/// CSV codec.
	/// </summary>
	public class CsvCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// CSV codec.
		/// </summary>
		public CsvCodec()
		{
		}

		/// <summary>
		/// Content-Type for CSV files.
		/// </summary>
		public const string ContentType = "text/csv";

		/// <summary>
		/// CSV content types.
		/// </summary>
		public static readonly string[] CsvContentTypes = new string[] { ContentType };

		/// <summary>
		/// CSV content types.
		/// </summary>
		public static readonly string[] CsvFileExtensions = new string[] { "csv" };

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => CsvContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => CsvFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (ContentType == CsvCodec.ContentType)
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
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			string s = Strings.GetString(Data, Encoding);
			return Task.FromResult(new ContentResponse(ContentType, CSV.Parse(s), Data));
		}


		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, "csv", true) == 0)
			{
				ContentType = CsvCodec.ContentType;
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
				case CsvCodec.ContentType:
					FileExtension = "csv";
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
			if (InternetContent.IsAccepted(CsvContentTypes, AcceptedContentTypes) &&
				(Object is string[][] || Object is IMatrix))
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
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			string Csv;

			if (Object is string[][] Records)
				Csv = CSV.Encode(Records);
			else if (Object is IMatrix M)
				Csv = CSV.Encode(M);
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode as CSV.", nameof(Object))));

			if (Encoding is null)
				Encoding = Encoding.UTF8;

			return Task.FromResult(new ContentResponse(ContentType, Csv, Encoding.GetBytes(Csv)));
		}
	}
}
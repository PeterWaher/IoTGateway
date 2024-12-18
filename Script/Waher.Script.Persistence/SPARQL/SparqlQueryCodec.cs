using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Encoder and Decoder of SPARQL queries.
	/// https://www.w3.org/TR/sparql12-query/
	/// </summary>
	public class SparqlQueryCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of SPARQL queries.
		/// </summary>
		public SparqlQueryCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => SparqlQueryContentTypes;

		private static readonly string[] SparqlQueryContentTypes = new string[]
		{
			"application/sparql-query"
		};

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => SparqlQueryFileExtensions;

		private static readonly string[] SparqlQueryFileExtensions = new string[]
		{
			"rq"
		};

		/// <summary>
		/// If the decoder decodes content of a given Internet Content Type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Grade">How well the decoder supports the given content type.</param>
		/// <returns>If the decoder decodes the given content type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(SparqlQueryContentTypes, ContentType) >= 0)
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
		/// Decodes an object
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Data">Binary representation of object.</param>
		/// <param name="Encoding">Encoding</param>
		/// <param name="Fields">Additional fields</param>
		/// <param name="BaseUri">Base URI</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			string s = CommonTypes.GetString(Data, Encoding ?? Encoding.UTF8);
			Expression Exp = new Expression(s, BaseUri?.AbsolutePath);

			if (!(Exp.Root is SparqlQuery Query))
				throw new Exception("Invalid SPARQL query.");

			return Task.FromResult(new ContentResponse(ContentType, Query, Data));
		}

		/// <summary>
		/// If the encoder encodes a specific object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder supports the given object.</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>If the encoder encodes the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is SparqlQuery &&
				InternetContent.IsAccepted(SparqlQueryContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (Object is Expression Exp && 
				Exp.Root is SparqlQuery &&
				InternetContent.IsAccepted(SparqlQueryContentTypes, AcceptedContentTypes))
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
		/// Encodes an object
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="Encoding">Encoding</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>Encoded object.</returns>
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (Encoding is null)
				Encoding = Encoding.UTF8;

			if (!(Object is SparqlQuery Query))
			{
				if (Object is Expression Exp && Exp.Root is SparqlQuery Query2)
					Query = Query2;
				else
					throw new ArgumentException("Unable to encode object.", nameof(Object));
			}

			string Text = JSON.Encode(Query.SubExpression, false);
			byte[] Bin = Encoding.GetBytes(Text);
			string ContentType = SparqlQueryContentTypes[0] + "; charset=" + Encoding.WebName;

			return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
		}

		/// <summary>
		/// Tries to get the content type of content of a given file extension.
		/// </summary>
		/// <param name="FileExtension">File Extension</param>
		/// <param name="ContentType">Content Type, if recognized.</param>
		/// <returns>If File Extension was recognized and Content Type found.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, SparqlQueryFileExtensions[0], true) == 0)
			{
				ContentType = SparqlQueryContentTypes[0];
				return true;
			}
			else
			{
				ContentType = null;
				return false;
			}
		}

		/// <summary>
		/// Tries to get the file extension of content of a given content type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="FileExtension">File Extension, if recognized.</param>
		/// <returns>If Content Type was recognized and File Extension found.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			if (Array.IndexOf(SparqlQueryContentTypes, ContentType) >= 0)
			{
				FileExtension = SparqlQueryFileExtensions[0];
				return true;
			}
			else
			{
				FileExtension = null;
				return false;
			}
		}
	}
}

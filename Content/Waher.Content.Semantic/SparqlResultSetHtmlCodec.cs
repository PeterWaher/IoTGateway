using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Encoder and Decoder of semantic information from SPARQL queries using HTML.
	/// </summary>
	public class SparqlResultSetHtmlCodec : IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information from SPARQL queries using HTML.
		/// </summary>
		public SparqlResultSetHtmlCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => new string[0];

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => new string[0];

		/// <summary>
		/// If the encoder encodes a specific object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder supports the given object.</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>If the encoder encodes the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if (Object is SparqlResultSet &&
				InternetContent.IsAccepted(HtmlCodec.HtmlContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (Object is ObjectMatrix M && M.HasColumnNames &&
				InternetContent.IsAccepted(HtmlCodec.HtmlContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Ok;
				return true;
			}
			else if (Object is bool &&
				InternetContent.IsAccepted(HtmlCodec.HtmlContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Barely;
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
		public async Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			string Html;

			if (Encoding is null)
				Encoding = Encoding.UTF8;

			if (Object is SparqlResultSet Result)
			{
				if (Result.BooleanResult.HasValue)
					Html = CommonTypes.Encode(Result.BooleanResult.Value);
				else
				{
					using (HtmlRenderer Renderer = new HtmlRenderer(new HtmlSettings()))
					{
						await Renderer.RenderObject(Result.ToMatrix(), true, new Script.Variables());
						Html = Renderer.ToString();
					}
				}
			}
			else if (Object is ObjectMatrix M)
			{
				using (HtmlRenderer Renderer = new HtmlRenderer(new HtmlSettings()))
				{
					await Renderer.RenderObject(M, true, new Script.Variables());
					Html = Renderer.ToString();
				}
			}
			else if (Object is bool b)
				Html = CommonTypes.Encode(b);
			else
				return new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object)));

			byte[] Bin = Encoding.GetBytes(Html);
			string ContentType = HtmlCodec.HtmlContentTypes[0] + "; charset=" + Encoding.WebName;

			return new ContentResponse(ContentType, Object, Bin);
		}

		/// <summary>
		/// Tries to get the content type of content of a given file extension.
		/// </summary>
		/// <param name="FileExtension">File Extension</param>
		/// <param name="ContentType">Content Type, if recognized.</param>
		/// <returns>If File Extension was recognized and Content Type found.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			ContentType = null;
			return false;
		}

		/// <summary>
		/// Tries to get the file extension of content of a given content type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="FileExtension">File Extension, if recognized.</param>
		/// <returns>If Content Type was recognized and File Extension found.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			FileExtension = null;
			return false;
		}
	}
}

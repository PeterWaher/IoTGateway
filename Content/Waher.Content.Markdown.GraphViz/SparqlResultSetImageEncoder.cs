using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Images;
using Waher.Content.Semantic;
using Waher.Content.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.GraphViz
{
	/// <summary>
	/// Encoder of semantic information from SPARQL queries as images.
	/// </summary>
	public class SparqlResultSetImageEncoder : IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information from SPARQL queries using HTML.
		/// </summary>
		public SparqlResultSetImageEncoder()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => Array.Empty<string>();

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => Array.Empty<string>();

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
				(InternetContent.IsAccepted(ImageCodec.ContentTypeSvg, AcceptedContentTypes) ||
				InternetContent.IsAccepted(ImageCodec.ContentTypePng, AcceptedContentTypes) ||
				InternetContent.IsAccepted(GraphVizCodec.DefaultContentType, AcceptedContentTypes)))
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
		/// Encodes an object
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="Encoding">Encoding</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>Encoded object.</returns>
		public async Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (!(Object is SparqlResultSet Result))
				return new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object)));

			if ((Result.Variables?.Length ?? 0) != 3)
				return new ContentResponse(new ArgumentException("Graphs can only be generated for semantic-triple result sets.", nameof(Object)));

			InMemorySemanticCube Cube = new InMemorySemanticCube();
			string SubjectVariable = Result.Variables[0];
			string PredicateVariable = Result.Variables[1];
			string ObjectVariable = Result.Variables[2];

			foreach (ISparqlResultRecord Record in Result.Records)
			{
				ISemanticElement Subject = Record[SubjectVariable];
				ISemanticElement Predicate = Record[PredicateVariable];
				ISemanticElement Object2 = Record[ObjectVariable];

				if (Subject is null || Predicate is null || Object2 is null)
					return new ContentResponse(new ArgumentException("Graphs can only be generated for semantic-triple result sets.", nameof(Object)));

				Cube.Add(Subject, Predicate, Object2);
			}

			string Graph = GraphVizUtilities.GenerateGraph(Cube, string.Empty);
			string ContentType;
			byte[] Bin;

			if (InternetContent.IsAccepted(ImageCodec.ContentTypeSvg, AcceptedContentTypes))
			{
				Bin = await GraphVizUtilities.DotToImage(Graph, ResultType.Svg);
				ContentType = ImageCodec.ContentTypeSvg;
			}
			else if (InternetContent.IsAccepted(ImageCodec.ContentTypePng, AcceptedContentTypes))
			{
				Bin = await GraphVizUtilities.DotToImage(Graph, ResultType.Png);
				ContentType = ImageCodec.ContentTypePng;
			}
			else if (InternetContent.IsAccepted(GraphVizCodec.DefaultContentType, AcceptedContentTypes))
			{
				if (Encoding is null)
				{
					ContentType = GraphVizCodec.DefaultContentType + "; charset=utf-8";
					Encoding = Encoding.UTF8;
				}
				else
					ContentType = GraphVizCodec.DefaultContentType + "; charset=" + Encoding.WebName;

				Bin = Encoding.GetBytes(Graph);
			}
			else
				return new ContentResponse(new ArgumentException("Requestd Content-Type not acceptable.", nameof(AcceptedContentTypes)));

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

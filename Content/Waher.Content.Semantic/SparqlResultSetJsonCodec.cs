using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Encoder and Decoder of semantic information from SPARQL queries using JSON.
	/// https://www.w3.org/TR/sparql11-results-json/
	/// </summary>
	public class SparqlResultSetJsonCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information from SPARQL queries using JSON.
		/// </summary>
		public SparqlResultSetJsonCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => SparqlResultSetContentTypes;

		private static readonly string[] SparqlResultSetContentTypes = new string[]
		{
			"application/sparql-results+json"
		};

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => SparqlResultSetFileExtensions;

		private static readonly string[] SparqlResultSetFileExtensions = new string[]
		{
			"srj"
		};

		/// <summary>
		/// If the decoder decodes content of a given Internet Content Type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Grade">How well the decoder supports the given content type.</param>
		/// <returns>If the decoder decodes the given content type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(SparqlResultSetContentTypes, ContentType) >= 0)
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
			object Obj = JSON.Parse(s);

			if (!(Obj is Dictionary<string, object> Doc))
				return Task.FromResult(new ContentResponse(new Exception("Unable to decode JSON.")));

			SparqlResultSet Parsed = new SparqlResultSet(Doc, BaseUri);
			return Task.FromResult(new ContentResponse(ContentType, Parsed, Data));
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
			if (Object is SparqlResultSet &&
				InternetContent.IsAccepted(SparqlResultSetContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (Object is ObjectMatrix M && M.HasColumnNames &&
				InternetContent.IsAccepted(SparqlResultSetContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Ok;
				return true;
			}
			else if (Object is bool &&
				InternetContent.IsAccepted(SparqlResultSetContentTypes, AcceptedContentTypes))
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
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			if (Encoding is null)
				Encoding = Encoding.UTF8;

			object ResultObj;
			bool Pretty = false;

			if (Object is SparqlResultSet Result)
			{
				ResultObj = this.EncodeAsync(Result);
				Pretty = Result.Pretty;
			}
			else if (Object is ObjectMatrix M)
				ResultObj = this.EncodeAsync(M);
			else if (Object is bool b)
				ResultObj = this.EncodeAsync(b);
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object))));

			string Text = JSON.Encode(ResultObj, Pretty);
			byte[] Bin = Encoding.GetBytes(Text);
			string ContentType = SparqlResultSetContentTypes[0] + "; charset=" + Encoding.WebName;

			return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
		}

		private Dictionary<string, object> EncodeAsync(SparqlResultSet Result)
		{
			Dictionary<string, object> Head = new Dictionary<string, object>();
			Dictionary<string, object> ResultObj = new Dictionary<string, object>()
			{
				{ "head", Head }
			};

			if (!(Result.Variables is null))
				Head["vars"] = Result.Variables;

			if (!(Result.Links is null))
			{
				List<string> Links = new List<string>();

				foreach (Uri Link in Result.Links)
					Links.Add(Link.ToString());

				Head["link"] = Links.ToArray();
			}

			if (Result.BooleanResult.HasValue)
				ResultObj["boolean"] = Result.BooleanResult.Value;
			else
			{
				Dictionary<string, object> Results = new Dictionary<string, object>();
				ResultObj["results"] = Results;

				if (!(Result.Records is null))
				{
					List<Dictionary<string, object>> Bindings = new List<Dictionary<string, object>>();

					foreach (ISparqlResultRecord Record in Result.Records)
					{
						Dictionary<string, object> Binding = new Dictionary<string, object>();

						foreach (ISparqlResultItem Item in Record)
							Binding[Item.Name] = OutputValue(Item.Value);

						Bindings.Add(Binding);
					}

					Results["bindings"] = Bindings.ToArray();
				}
			}

			return ResultObj;
		}

		private Dictionary<string, object> EncodeAsync(ObjectMatrix Result)
		{
			Dictionary<string, object> Head = new Dictionary<string, object>();
			Dictionary<string, object> ResultObj = new Dictionary<string, object>()
			{
				{ "head", Head }
			};

			if (!(Result.ColumnNames is null))
				Head["vars"] = Result.ColumnNames;

			Dictionary<string, object> Results = new Dictionary<string, object>();
			ResultObj["results"] = Results;

			List<Dictionary<string, object>> Bindings = new List<Dictionary<string, object>>();

			int x, y;
			int NrRows = Result.Rows;
			int NrColumns = Result.Columns;

			for (y = 0; y < NrRows; y++)
			{
				Dictionary<string, object> Binding = new Dictionary<string, object>();

				for (x = 0; x < NrColumns; x++)
					Binding[Result.ColumnNames[x]] = OutputValue(Result.GetElement(x, y)?.AssociatedObjectValue);

				Bindings.Add(Binding);
			}

			Results["bindings"] = Bindings.ToArray();

			return ResultObj;
		}

		private Dictionary<string, object> EncodeAsync(bool Result)
		{
			Dictionary<string, object> Head = new Dictionary<string, object>();
			Dictionary<string, object> ResultObj = new Dictionary<string, object>()
			{
				{ "head", Head },
				{ "boolean", Result }
			};

			return ResultObj;
		}

		private static Dictionary<string, object> OutputValue(object Value)
		{
			Dictionary<string, object> Result;

			if (Value is ISemanticElement E)
			{
				if (E is ISemanticLiteral Literal)
				{
					Result = new Dictionary<string, object>()
					{
						{ "type", "literal" },
						{ "value", Literal.Value }
					};

					if (!string.IsNullOrEmpty(Literal.StringType))
						Result["datatype"] = Literal.StringType;

					if (Literal is StringLiteral StringLiteral &&
						!string.IsNullOrEmpty(StringLiteral.Language))
					{
						Result["xml:lang"] = StringLiteral.Language;
					}
					else if (Literal is CustomLiteral CustomLiteral &&
						!string.IsNullOrEmpty(CustomLiteral.Language))
					{
						Result["xml:lang"] = CustomLiteral.Language;
					}
				}
				else if (E is UriNode N)
				{
					Result = new Dictionary<string, object>()
					{
						{ "type", "uri" },
						{ "value", N.Uri.ToString() }
					};
				}
				else if (E is BlankNode N2)
				{
					Result = new Dictionary<string, object>()
					{
						{ "type", "bnode" },
						{ "value", N2.NodeId }
					};
				}
				else if (E is ISemanticTriple T)
				{
					Dictionary<string, object> Triple = new Dictionary<string, object>()
					{
						{ "subject", OutputValue(T.Subject) },
						{ "predicate", OutputValue(T.Predicate) },
						{ "object", OutputValue(T.Object) }
					};
					Result = new Dictionary<string, object>()
					{
						{ "type", "triple" },
						{ "value", Triple }
					};
				}
				else
				{
					Result = new Dictionary<string, object>()
					{
						{ "type", "literal" },
						{ "value", Value?.ToString() }
					};
				}
			}
			else
			{
				Result = new Dictionary<string, object>()
					{
						{ "type", "literal" },
						{ "value", Value?.ToString() }
					};
			}

			return Result;
		}

		/// <summary>
		/// Tries to get the content type of content of a given file extension.
		/// </summary>
		/// <param name="FileExtension">File Extension</param>
		/// <param name="ContentType">Content Type, if recognized.</param>
		/// <returns>If File Extension was recognized and Content Type found.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, SparqlResultSetFileExtensions[0], true) == 0)
			{
				ContentType = SparqlResultSetContentTypes[0];
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
			if (Array.IndexOf(SparqlResultSetContentTypes, ContentType) >= 0)
			{
				FileExtension = SparqlResultSetFileExtensions[0];
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

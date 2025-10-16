﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Encoder and Decoder of semantic information from SPARQL queries using XML.
	/// https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/
	/// </summary>
	public class SparqlResultSetXmlCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information from SPARQL queries using XML.
		/// </summary>
		public SparqlResultSetXmlCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => SparqlResultSetContentTypes;

		private static readonly string[] SparqlResultSetContentTypes = new string[]
		{
			"application/sparql-results+xml"
		};

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => SparqlResultSetFileExtensions;

		private static readonly string[] SparqlResultSetFileExtensions = new string[]
		{
			"srx"
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
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>Decoded object.</returns>
		public Task<ContentResponse> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding,
			KeyValuePair<string, string>[] Fields, Uri BaseUri, ICodecProgress Progress)
		{
			string s = Strings.GetString(Data, Encoding ?? Encoding.UTF8);
			SparqlResultSet Parsed = new SparqlResultSet(s, BaseUri);
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
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <param name="AcceptedContentTypes">Accepted content types.</param>
		/// <returns>Encoded object.</returns>
		public Task<ContentResponse> EncodeAsync(object Object, Encoding Encoding, ICodecProgress Progress, params string[] AcceptedContentTypes)
		{
			if (Encoding is null)
				Encoding = Encoding.UTF8;

			StringBuilder sb = new StringBuilder();
			sb.Append("<?xml version=\"1.0\" encoding=\"");
			sb.Append(Encoding.WebName);
			sb.AppendLine("\"?>");

			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding,
				Indent = false,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				NewLineHandling = NewLineHandling.None,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true,
				WriteEndDocumentOnClose = true
			};

			if (Object is SparqlResultSet Result && Result.Pretty)
			{
				Settings.Indent = true;
				Settings.IndentChars = "\t";
			}

			using (XmlWriter w = XmlWriter.Create(sb, Settings))
			{
				if (Object is SparqlResultSet Result2)
					this.Encode(Result2, w);
				else if (Object is ObjectMatrix M)
					this.Encode(M, w);
				else if (Object is bool b)
					this.Encode(b, w);
				else
					return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object))));

				w.Flush();

				string Text = sb.ToString();

				byte[] Bin = Encoding.GetBytes(Text);
				string ContentType = SparqlResultSetContentTypes[0] + "; charset=" + Encoding.WebName;

				return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
			}
		}

		private void Encode(SparqlResultSet Result, XmlWriter w)
		{
			w.WriteStartElement(SparqlResultSet.LocalName, SparqlResultSet.Namespace);

			w.WriteStartElement("head");

			if (!(Result.Variables is null))
			{
				foreach (string Name in Result.Variables)
				{
					w.WriteStartElement("variable");
					w.WriteAttributeString("name", Name);
					w.WriteEndElement();
				}
			}

			if (!(Result.Links is null))
			{
				foreach (Uri Link in Result.Links)
				{
					w.WriteStartElement("link");
					w.WriteAttributeString("href", Link.ToString());
					w.WriteEndElement();
				}
			}

			w.WriteEndElement();

			if (Result.BooleanResult.HasValue)
				w.WriteElementString("boolean", CommonTypes.Encode(Result.BooleanResult.Value));
			else
			{
				w.WriteStartElement("results");

				if (!(Result.Records is null))
				{
					foreach (ISparqlResultRecord Record in Result.Records)
					{
						w.WriteStartElement("result");

						foreach (ISparqlResultItem Item in Record)
						{
							w.WriteStartElement("binding");
							w.WriteAttributeString("name", Item.Name);

							OutputValue(w, Item.Value);

							w.WriteEndElement();
						}

						w.WriteEndElement();
					}
				}

				w.WriteEndElement();
			}

			w.WriteEndElement();
		}

		private void Encode(ObjectMatrix Result, XmlWriter w)
		{
			w.WriteStartElement(SparqlResultSet.LocalName, SparqlResultSet.Namespace);

			w.WriteStartElement("head");

			if (!(Result.ColumnNames is null))
			{
				foreach (string Name in Result.ColumnNames)
				{
					w.WriteStartElement("variable");
					w.WriteAttributeString("name", Name);
					w.WriteEndAttribute();
				}
			}

			w.WriteEndElement();
			w.WriteStartElement("results");

			int x, y;
			int NrRows = Result.Rows;
			int NrColumns = Result.Columns;

			for (y = 0; y < NrRows; y++)
			{
				w.WriteStartElement("result");

				for (x = 0; x < NrColumns; x++)
				{
					w.WriteStartElement("binding");
					w.WriteAttributeString("name", Result.ColumnNames[x]);

					OutputValue(w, Result.GetElement(x, y)?.AssociatedObjectValue);

					w.WriteEndElement();
				}

				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.WriteEndElement();
		}

		private void Encode(bool Result, XmlWriter w)
		{
			w.WriteStartElement(SparqlResultSet.LocalName, SparqlResultSet.Namespace);
			w.WriteElementString("head", string.Empty);
			w.WriteElementString("boolean", CommonTypes.Encode(Result));
			w.WriteEndElement();
		}

		private static void OutputValue(XmlWriter w, object Value)
		{
			if (Value is ISemanticElement E)
			{
				if (E is ISemanticLiteral Literal)
				{
					w.WriteStartElement("literal");

					if (!string.IsNullOrEmpty(Literal.StringType))
						w.WriteAttributeString("datatype", Literal.StringType);

					if (Literal is StringLiteral StringLiteral &&
						!string.IsNullOrEmpty(StringLiteral.Language))
					{
						w.WriteAttributeString("xml", "lang", null, StringLiteral.Language);
					}
					else if (Literal is CustomLiteral CustomLiteral &&
						!string.IsNullOrEmpty(CustomLiteral.Language))
					{
						w.WriteAttributeString("xml", "lang", null, CustomLiteral.Language);
					}

					w.WriteValue(Literal.Value);
					w.WriteEndElement();
				}
				else if (E is UriNode N)
					w.WriteElementString("uri", N.Uri.ToString());
				else if (E is BlankNode N2)
					w.WriteElementString("bnode", N2.NodeId);
				else if (E is ISemanticTriple T)
				{
					w.WriteStartElement("triple");

					w.WriteStartElement("subject");
					OutputValue(w, T.Subject);
					w.WriteEndElement();

					w.WriteStartElement("predicate");
					OutputValue(w, T.Predicate);
					w.WriteEndElement();

					w.WriteStartElement("object");
					OutputValue(w, T.Object);
					w.WriteEndElement();

					w.WriteEndElement();
				}
				else
					w.WriteElementString("literal", Value?.ToString() ?? string.Empty);
			}
			else
				w.WriteElementString("literal", Value?.ToString() ?? string.Empty);
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

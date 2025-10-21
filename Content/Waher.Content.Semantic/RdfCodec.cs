﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Content.Xml.Text;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Encoder and Decoder of semantic information in RDF Documents.
	/// </summary>
	public class RdfCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Encoder and Decoder of semantic information in RDF Documents.
		/// </summary>
		public RdfCodec()
		{
		}

		/// <summary>
		/// Supported Internet Content Types.
		/// </summary>
		public string[] ContentTypes => RdfContentTypes;

		private static readonly string[] RdfContentTypes = new string[]
		{
			DefaultContentType
		};

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => RdfFileExtensions;

		private static readonly string[] RdfFileExtensions = new string[]
		{
			DefaultExtension
		};

		/// <summary>
		/// application/rdf+xml
		/// </summary>
		public const string DefaultContentType = "application/rdf+xml";

		/// <summary>
		/// rdf
		/// </summary>
		public const string DefaultExtension = "rdf";

		/// <summary>
		/// If the decoder decodes content of a given Internet Content Type.
		/// </summary>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Grade">How well the decoder supports the given content type.</param>
		/// <returns>If the decoder decodes the given content type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf(RdfContentTypes, ContentType) >= 0)
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
			RdfDocument Parsed = new RdfDocument(s, BaseUri, "n", BlankNodeIdMode.Guid);
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
			if (Object is RdfDocument &&
				InternetContent.IsAccepted(RdfContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (Object is ISemanticModel)
			{
				if (InternetContent.IsAccepted(RdfContentTypes, AcceptedContentTypes))
				{
					Grade = Grade.Ok;
					return true;
				}
				else if (InternetContent.IsAccepted(XmlCodec.XmlContentTypes, AcceptedContentTypes))
				{
					Grade = Grade.Barely;
					return true;
				}
			}

			Grade = Grade.NotAtAll;
			return false;
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

			string Text;

			if (Object is RdfDocument Doc)
				Text = Doc.Text;
			else if (Object is ISemanticModel Model)
			{
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

				using (XmlWriter w = XmlWriter.Create(sb, Settings))
				{
					Dictionary<string, string> Prefixes = new Dictionary<string, string>();
					Dictionary<string, ChunkedList<ISemanticTriple>> PerSubject = new Dictionary<string, ChunkedList<ISemanticTriple>>();
					string s;

					foreach (ISemanticTriple Triple in Model)
					{
						CheckPrefix(Triple.Subject, Prefixes);
						CheckPrefix(Triple.Predicate, Prefixes);
						CheckPrefix(Triple.Object, Prefixes);

						if (Triple.Subject is UriNode UriNode)
							s = UriNode.Uri.ToString();
						else
							s = Triple.Subject.ToString();

						if (!PerSubject.TryGetValue(s, out ChunkedList<ISemanticTriple> List))
						{
							List = new ChunkedList<ISemanticTriple>();
							PerSubject[s] = List;
						}

						List.Add(Triple);
					}

					w.WriteStartElement("rdf", "RDF", Rdf.Namespace);

					foreach (KeyValuePair<string, string> P in Prefixes)
						w.WriteAttributeString("xmlns", P.Value, string.Empty, P.Key);

					foreach (KeyValuePair<string, ChunkedList<ISemanticTriple>> Subject in PerSubject)
					{
						w.WriteStartElement("rdf", "Description", Rdf.Namespace);

						if (Subject.Key.StartsWith("_:"))
							w.WriteAttributeString("rdf", "nodeID", Rdf.Namespace, Subject.Key.Substring(2));
						else
							w.WriteAttributeString("rdf", "about", Rdf.Namespace, Subject.Key);

						foreach (ISemanticTriple Triple in Subject.Value)
						{
							if (Triple.Predicate is UriNode Predicate)
							{
								string Uri = Predicate.UriString;
								string Namespace = GetNamespace(Uri);

								if (!(Namespace is null) &&
									Prefixes.TryGetValue(Namespace, out string Prefix))
								{
									string LocalName = Uri.Substring(Namespace.Length);
									w.WriteStartElement(Prefix, LocalName, Namespace);
								}
								else
									w.WriteStartElement(Uri);

								if (Triple.Object is SemanticLiteral Literal)
								{
									if (string.IsNullOrEmpty(Literal.StringType))
									{
										if (Literal is StringLiteral StringLiteral &&
											!string.IsNullOrEmpty(StringLiteral.Language))
										{
											w.WriteAttributeString("xml", "lang", null, StringLiteral.Language);
											w.WriteValue(Literal.StringValue);
										}
										else
											w.WriteValue(Literal.StringValue);
									}
									else
									{
										w.WriteAttributeString("rdf", "datatype", Rdf.Namespace, Literal.StringType);
										w.WriteValue(Literal.StringValue);
									}
								}
								else if (Triple.Object is BlankNode BlankNode)
									w.WriteAttributeString("rdf", "nodeID", Rdf.Namespace, BlankNode.NodeId);
								else if (Triple.Object is UriNode UriNode)
									w.WriteAttributeString("rdf", "resource", Rdf.Namespace, UriNode.UriString);
								else
									w.WriteValue(Triple.Object.ToString());

								w.WriteEndElement();
							}
							else
								return Task.FromResult(new ContentResponse(new Exception("Unable to encode semantic model as RDF document. RDF documents require predicates to be URIs.")));
						}

						w.WriteEndElement();
					}

					w.WriteEndElement();
					w.Flush();

					Text = sb.ToString();
				}
			}
			else
				return Task.FromResult(new ContentResponse(new ArgumentException("Unable to encode object.", nameof(Object))));

			byte[] Bin = Encoding.GetBytes(Text);
			string ContentType = RdfContentTypes[0] + "; charset=" + Encoding.WebName;

			return Task.FromResult(new ContentResponse(ContentType, Object, Bin));
		}

		private static void CheckPrefix(ISemanticElement Element, Dictionary<string, string> Prefixes)
		{
			if (Element is UriNode UriNode)
			{
				string Namespace = GetNamespace(UriNode.UriString);
				if (!string.IsNullOrEmpty(Namespace) && !Prefixes.ContainsKey(Namespace))
				{
					string Prefix = "p" + (Prefixes.Count + 1).ToString();
					Prefixes[Namespace] = Prefix;
				}
			}
		}

		private static string GetNamespace(string Uri)
		{
			int i = Uri.LastIndexOf('#');
			if (i >= 0)
				return Uri.Substring(0, i + 1);

			i = Uri.LastIndexOf('/');
			if (i >= 0)
				return Uri.Substring(0, i + 1);

			i = Uri.LastIndexOf(':');
			if (i >= 0)
				return Uri.Substring(0, i + 1);

			return null;
		}

		/// <summary>
		/// Tries to get the content type of content of a given file extension.
		/// </summary>
		/// <param name="FileExtension">File Extension</param>
		/// <param name="ContentType">Content Type, if recognized.</param>
		/// <returns>If File Extension was recognized and Content Type found.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			if (string.Compare(FileExtension, RdfFileExtensions[0], true) == 0)
			{
				ContentType = RdfContentTypes[0];
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
			if (Array.IndexOf(RdfContentTypes, ContentType) >= 0)
			{
				FileExtension = RdfFileExtensions[0];
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

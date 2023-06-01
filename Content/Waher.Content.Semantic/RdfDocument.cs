﻿using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains semantic information stored in an RDF document.
	/// 
	/// Ref: https://www.w3.org/TR/rdf-syntax-grammar/
	/// </summary>
	public class RdfDocument : SemanticModel
	{
		/// <summary>
		/// http://www.w3.org/1999/02/22-rdf-syntax-ns#
		/// </summary>
		public const string RdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

		/// <summary>
		/// rdf:type predicate
		/// </summary>
		public static readonly UriNode RdfType = new UriNode(new Uri(RdfNamespace + "type"));

		/// <summary>
		/// Predefined reference to first element in a collection.
		/// </summary>
		public readonly static UriNode RdfFirst = new UriNode(new Uri(RdfNamespace + "first"));

		/// <summary>
		/// Predefined reference to next element in a collection.
		/// </summary>
		public readonly static UriNode RdfNext = new UriNode(new Uri(RdfNamespace + "rest"));

		/// <summary>
		/// Predefined reference to end of collection.
		/// </summary>
		public readonly static UriNode RdfNil = new UriNode(new Uri(RdfNamespace + "nil"));

		/// <summary>
		/// Subject reference, during reification.
		/// </summary>
		public readonly static UriNode RdfSubject = new UriNode(new Uri(RdfNamespace + "subject"));

		/// <summary>
		/// Predicate reference, during reification.
		/// </summary>
		public readonly static UriNode RdfPredicate = new UriNode(new Uri(RdfNamespace + "predicate"));

		/// <summary>
		/// object reference, during reification.
		/// </summary>
		public readonly static UriNode RdfObject = new UriNode(new Uri(RdfNamespace + "object"));

		/// <summary>
		/// Statement reference, during reification.
		/// </summary>
		public readonly static UriNode RdfStatement = new UriNode(new Uri(RdfNamespace + "Statement"));

		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly XmlDocument xml;
		private readonly string blankNodeIdPrefix;
		private readonly BlankNodeIdMode blankNodeIdMode;
		private readonly string text;
		private int blankNodeIndex = 0;

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		public RdfDocument(XmlDocument Xml)
			: this(Xml, Xml.OuterXml, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		public RdfDocument(XmlDocument Xml, Uri BaseUri)
			: this(Xml, Xml.OuterXml, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public RdfDocument(XmlDocument Xml, Uri BaseUri, string BlankNodeIdPrefix)
			: this(Xml, Xml.OuterXml, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public RdfDocument(XmlDocument Xml, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
			: this(Xml, Xml.OuterXml, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		public RdfDocument(string Text)
			: this(ToXml(Text), Text, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		public RdfDocument(string Text, Uri BaseUri)
			: this(ToXml(Text), Text, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public RdfDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix)
			: this(ToXml(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public RdfDocument(string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
			: this(ToXml(Text), Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode)
		{
		}

		private static XmlDocument ToXml(string Text)
		{
			XmlDocument Xml = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Xml.LoadXml(Text);

			return Xml;
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		public RdfDocument(XmlDocument Xml, string Text)
			: this(Xml, Text, null)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		public RdfDocument(XmlDocument Xml, string Text, Uri BaseUri)
			: this(Xml, Text, BaseUri, "n")
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		public RdfDocument(XmlDocument Xml, string Text, Uri BaseUri, string BlankNodeIdPrefix)
			: this(Xml, Text, BaseUri, BlankNodeIdPrefix, BlankNodeIdMode.Sequential)
		{
		}

		/// <summary>
		/// Contains semantic information stored in a turtle document.
		/// </summary>
		/// <param name="Xml">XML content of RDF document.</param>
		/// <param name="Text">Text representation of RDF document.</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="BlankNodeIdPrefix">Prefix to use when creating blank nodes.</param>
		/// <param name="BlankNodeIdMode">How Blank Node IDs are generated</param>
		public RdfDocument(XmlDocument Xml, string Text, Uri BaseUri, string BlankNodeIdPrefix, BlankNodeIdMode BlankNodeIdMode)
		{
			this.xml = Xml;
			this.blankNodeIdPrefix = BlankNodeIdPrefix;
			this.blankNodeIdMode = BlankNodeIdMode;
			this.text = Text;

			if (Xml is null || Xml.DocumentElement is null)
				throw new ArgumentNullException(nameof(Xml));

			if (Xml.DocumentElement.LocalName != "RDF" || Xml.DocumentElement.NamespaceURI != RdfNamespace)
				throw new ArgumentException("XML document not valid RDF.", nameof(Xml));

			string Language = null;

			foreach (XmlAttribute Attr in Xml.DocumentElement.Attributes)
			{
				if (Attr.Name == "xml:lang")
				{
					Language = Attr.Value;
					continue;
				}
				else if (Attr.Name == "xml:base")
				{
					BaseUri = this.CreateUri(Attr.Value, BaseUri);
					continue;
				}
			}

			this.ParseDescriptions(Xml.DocumentElement, Language, BaseUri);
		}

		/// <summary>
		/// Original XML of document.
		/// </summary>
		public XmlDocument Xml => this.xml;

		/// <summary>
		/// Text representation.
		/// </summary>
		public string Text => this.text;

		private void ParseDescriptions(XmlElement E, string Language, Uri BaseUri)
		{
			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				int ItemCounter = 0;
				this.ParseDescription(E2, Language, BaseUri, ref ItemCounter);
			}
		}

		private ISemanticElement ParseDescription(XmlElement E, string Language, Uri BaseUri,
			ref int ItemCounter)
		{
			string About = null;
			string NodeId = null;
			LinkedList<KeyValuePair<string, string>> Properties = null;

			foreach (XmlAttribute Attr in E.Attributes)
			{
				if (Attr.NamespaceURI == RdfNamespace)
				{
					switch (Attr.LocalName)
					{
						case "resource":
						case "datatype":
						case "parseType":
							continue;

						case "nodeID":
							NodeId = Attr.Value;
							continue;
						
						case "about":
							About = Attr.Value;
							continue;

						case "ID":
							About = "#" + Attr.Value;
							continue;
					}
				}
				else if (Attr.Name == "xml:lang")
				{
					Language = Attr.Value;
					continue;
				}
				else if (Attr.Name == "xml:base")
				{
					BaseUri = this.CreateUri(Attr.Value, BaseUri);
					continue;
				}
				else if (Attr.Prefix == "xmlns" || Attr.Name == "xmlns")
					continue;

				if (Properties is null)
					Properties = new LinkedList<KeyValuePair<string, string>>();

				Properties.AddLast(new KeyValuePair<string, string>(Attr.NamespaceURI + Attr.LocalName, Attr.Value));
			}

			bool HasLanguage = !string.IsNullOrEmpty(Language);
			ISemanticElement Subject;

			if (!string.IsNullOrEmpty(About))
				Subject = new UriNode(this.CreateUri(About, BaseUri));
			else if (!string.IsNullOrEmpty(NodeId))
				Subject = new BlankNode(NodeId);
			else
				Subject = this.CreateBlankNode();

			if (E.NamespaceURI != RdfNamespace)
			{
				this.triples.Add(new SemanticTriple(Subject, RdfType,
					new UriNode(this.CreateUri(E.NamespaceURI + E.LocalName, BaseUri))));
			}
			else if (E.LocalName != "Description")
			{
				if (E.LocalName == "li")
				{
					ItemCounter++;

					this.triples.Add(new SemanticTriple(Subject, RdfType,
						new UriNode(this.CreateUri(RdfNamespace + "_" + ItemCounter.ToString(), BaseUri))));
				}
				else
				{
					this.triples.Add(new SemanticTriple(Subject, RdfType,
						new UriNode(this.CreateUri(E.NamespaceURI + E.LocalName, BaseUri))));
				}
			}

			if (!(Properties is null))
			{
				foreach (KeyValuePair<string, string> P in Properties)
				{
					UriNode Predicate = new UriNode(this.CreateUri(P.Key, BaseUri));
					StringLiteral Object;

					if (HasLanguage)
						Object = new StringLiteral(P.Value, Language);
					else
						Object = new StringLiteral(P.Value);

					this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
				}
			}

			return this.ParseDescription(E, Subject, Language, BaseUri);
		}

		private ISemanticElement ParseDescription(XmlElement E, ISemanticElement Subject, string Language, Uri BaseUri)
		{
			int ItemCounter = 0;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				UriNode Predicate = new UriNode(this.CreateUri(E2.NamespaceURI + E2.LocalName, BaseUri));
				LinkedList<KeyValuePair<string, string>> Properties = null;
				ISemanticElement Object = null;
				Uri BaseUri2 = BaseUri;
				string Resource = null;
				string NodeId = null;
				string Id = null;
				string DataType = null;
				string Language2 = Language;
				string ParseType = null;

				foreach (XmlAttribute Attr in E2.Attributes)
				{
					if (Attr.NamespaceURI == RdfNamespace)
					{
						switch (Attr.LocalName)
						{
							case "resource":
								Resource = Attr.Value;
								continue;

							case "nodeID":
								NodeId = Attr.Value;
								continue;

							case "ID":
								Id = Attr.Value;
								continue;

							case "datatype":
								DataType = Attr.Value;
								continue;

							case "parseType":
								ParseType = Attr.Value;
								continue;
						}
					}
					else if (Attr.Name == "xml:lang")
					{
						Language2 = Attr.Value;
						continue;
					}
					else if (Attr.Name == "xml:base")
					{
						BaseUri2 = this.CreateUri(Attr.Value, BaseUri2);
						continue;
					}
					else if (Attr.Prefix == "xmlns" || Attr.Name == "xmlns")
						continue;

					if (Properties is null)
						Properties = new LinkedList<KeyValuePair<string, string>>();

					Properties.AddLast(new KeyValuePair<string, string>(Attr.NamespaceURI + Attr.LocalName, Attr.Value));
				}

				if (!(Properties is null))
				{
					if (Object is null)
						Object = this.CreateBlankNode();

					bool HasLanguage2 = !string.IsNullOrEmpty(Language2);

					this.triples.Add(new SemanticTriple(Subject, Predicate, Object));

					foreach (KeyValuePair<string, string> P in Properties)
					{
						ISemanticElement Predicate2 = new UriNode(this.CreateUri(P.Key, BaseUri2));
						ISemanticElement Literal;

						if (HasLanguage2)
							Literal = new StringLiteral(P.Value, Language2);
						else
							Literal = new StringLiteral(P.Value);

						this.triples.Add(new SemanticTriple(Object, Predicate2, Literal));
					}
				}

				if (!string.IsNullOrEmpty(Resource))
				{
					Object = new UriNode(this.CreateUri(Resource, BaseUri2));
					this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
				}
				else if (!string.IsNullOrEmpty(NodeId))
				{
					Object = new BlankNode(NodeId);
					this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
				}

				switch (ParseType)
				{
					case null:
						foreach (XmlNode N2 in E2.ChildNodes)
						{
							if (N2 is XmlText Text)
							{
								if (DataType is null)
								{
									if (string.IsNullOrEmpty(Language2))
										Object = new StringLiteral(Text.InnerText);
									else
										Object = new StringLiteral(Text.InnerText, Language2);
								}
								else
								{
									Uri DataTypeUri = this.CreateUri(DataType, BaseUri2);
									DataType = DataTypeUri.AbsoluteUri;

									if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
									{
										LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
											?? new CustomLiteral(string.Empty, DataType);

										this.dataTypes[DataType] = LiteralType;
									}

									Object = LiteralType.Parse(Text.InnerText, DataType);
								}

								this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
							}
							else if (N2 is XmlElement E3)
							{
								ISemanticElement Def = this.ParseDescription(E3, Language2, BaseUri2, ref ItemCounter);

								if (Object is null || Object.ToString() != Def.ToString())
									this.triples.Add(new SemanticTriple(Subject, Predicate, Def));
							}
						}
						break;

					case "Literal":
						Object = new XmlLiteral(E2.ChildNodes, E2.NamespaceURI);
						this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
						break;

					case "Resource":
						Object = this.CreateBlankNode();
						this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
						this.ParseDescription(E2, Object, Language2, BaseUri2);
						break;

					case "Collection":
						LinkedList<ISemanticElement> Elements = null;

						foreach (XmlNode N2 in E2.ChildNodes)
						{
							ISemanticElement Element;

							if (N2 is XmlText Text)
							{
								if (DataType is null)
								{
									if (string.IsNullOrEmpty(Language2))
										Element = new StringLiteral(Text.InnerText);
									else
										Element = new StringLiteral(Text.InnerText, Language2);
								}
								else
								{
									Uri DataTypeUri = this.CreateUri(DataType, BaseUri2);
									DataType = DataTypeUri.AbsoluteUri;

									if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
									{
										LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
											?? new CustomLiteral(string.Empty, DataType);

										this.dataTypes[DataType] = LiteralType;
									}

									Element = LiteralType.Parse(Text.InnerText, DataType);
								}
							}
							else if (N2 is XmlElement E3)
								Element = this.ParseDescription(E3, Language2, BaseUri2, ref ItemCounter);
							else
								continue;

							if (Elements is null)
								Elements = new LinkedList<ISemanticElement>();

							Elements.AddLast(Element);
						}

						if (Elements is null)
							this.triples.Add(new SemanticTriple(Subject, Predicate, RdfNil));
						else
						{
							LinkedListNode<ISemanticElement> Loop = Elements.First;
							BlankNode Current = this.CreateBlankNode();

							this.triples.Add(new SemanticTriple(Subject, Predicate, Current));

							while (!(Loop is null))
							{
								this.triples.Add(new SemanticTriple(Current, RdfFirst, Loop.Value));

								Loop = Loop.Next;

								if (!(Loop is null))
								{
									BlankNode Next = this.CreateBlankNode();
									this.triples.Add(new SemanticTriple(Current, RdfNext, Next));
									Current = Next;
								}
							}

							this.triples.Add(new SemanticTriple(Current, RdfNext, RdfNil));
						}
						break;

					default:
						throw this.ParsingException("Unrecognized parse type: " + ParseType);
				}


				if (!string.IsNullOrEmpty(Id))
				{
					UriNode ReificationNode = new UriNode(this.CreateUri("#" + Id, BaseUri2));

					this.triples.Add(new SemanticTriple(ReificationNode, RdfType, RdfStatement));
					this.triples.Add(new SemanticTriple(ReificationNode, RdfSubject, Subject));
					this.triples.Add(new SemanticTriple(ReificationNode, RdfPredicate, Predicate));

					if (!(Object is null))
						this.triples.Add(new SemanticTriple(ReificationNode, RdfObject, Object));
				}
			}

			return Subject;
		}

		private Uri CreateUri(string Reference, Uri BaseUri)
		{
			if (BaseUri is null)
			{
				if (Uri.TryCreate(Reference, UriKind.Absolute, out Uri URI))
					return URI;
				else
					throw this.ParsingException("Invalid URI: " + Reference);
			}
			else
			{
				if (string.IsNullOrEmpty(Reference))
					return BaseUri;
				else if (Uri.TryCreate(BaseUri, Reference, out Uri URI))
					return URI;
				else
					throw this.ParsingException("Invalid URI: " + Reference + " (base: " + BaseUri.ToString() + ")");
			}
		}

		private BlankNode CreateBlankNode()
		{
			if (this.blankNodeIdMode == BlankNodeIdMode.Guid)
				return new BlankNode(this.blankNodeIdPrefix + Guid.NewGuid().ToString());
			else
				return new BlankNode(this.blankNodeIdPrefix + (++this.blankNodeIndex).ToString());
		}

		private Exception ParsingException(string Message)
		{
			return new Exception(Message);
		}
	}
}

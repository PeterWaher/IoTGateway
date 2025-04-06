using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Getters;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains semantic information stored in an RDF document.
	/// 
	/// Ref: https://www.w3.org/TR/rdf-syntax-grammar/
	/// </summary>
	public class RdfDocument : InMemorySemanticCube, IWebServerMetaContent
	{
		/// <summary>
		/// rdf:type predicate
		/// </summary>
		public static UriNode RdfType => new UriNode(Rdf.Type, "rdf:type");

		/// <summary>
		/// Predefined reference to first element in a collection.
		/// </summary>
		public static UriNode RdfFirst => new UriNode(Rdf.First, "rdf:first");

		/// <summary>
		/// Predefined reference to next element in a collection.
		/// </summary>
		public static UriNode RdfRest => new UriNode(Rdf.Rest, "rdf:rest");

		/// <summary>
		/// Predefined reference to end of collection.
		/// </summary>
		public static UriNode RdfNil => new UriNode(Rdf.Nil, "rdf:nil");

		/// <summary>
		/// Subject reference, during reification.
		/// </summary>
		public static UriNode RdfSubject => new UriNode(Rdf.Subject, "rdf:subject");

		/// <summary>
		/// Predicate reference, during reification.
		/// </summary>
		public static UriNode RdfPredicate => new UriNode(Rdf.Predicate, "rdf:predicate");

		/// <summary>
		/// object reference, during reification.
		/// </summary>
		public static UriNode RdfObject => new UriNode(Rdf.Object, "rdf:object");

		/// <summary>
		/// Statement reference
		/// </summary>
		public static UriNode RdfStatement => new UriNode(Rdf.Statement, "rdf:Statement");

		/// <summary>
		/// Bag reference
		/// </summary>
		public static UriNode RdfBag => new UriNode(Rdf.Bag, "rdf:Bag");

		/// <summary>
		/// List item reference.
		/// </summary>
		public static UriNode RdfLi => new UriNode(Rdf.Li, "rdf:li");

		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly XmlDocument xml;
		private readonly string blankNodeIdPrefix;
		private readonly BlankNodeIdMode blankNodeIdMode;
		private readonly string text;
		private Dictionary<Uri, XmlElement> aboutEach = null;
		private Dictionary<Uri, XmlElement> aboutEachPrefix = null;
		private DateTimeOffset? date = null;
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

		internal static XmlDocument ToXml(string Text)
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

			string Language = null;
			string BagId = null;

			foreach (XmlAttribute Attr in Xml.DocumentElement.Attributes)
			{
				if (Attr.Prefix == "xml")
				{
					switch (Attr.LocalName)
					{
						case "lang":
							Language = Attr.Value;
							break;

						case "base":
							BaseUri = this.CreateUri(Attr.Value, BaseUri);
							break;
					}
				}
				else if (Attr.NamespaceURI == Rdf.Namespace)
				{
					switch (Attr.LocalName)
					{
						case "bagID":
							BagId = Attr.Value;
							break;
					}
				}
			}

			ISemanticElement Bag;

			if (BagId is null)
				Bag = null;
			else
			{
				Bag = this.CreateUriNode("#" + BagId, BaseUri);
				this.Add(new SemanticTriple(Bag, RdfType, RdfBag));
			}

			if (Xml.DocumentElement.LocalName == "RDF" && Xml.DocumentElement.NamespaceURI == Rdf.Namespace)
				this.ParseDescriptions(Xml.DocumentElement, Language, BaseUri);
			else
			{
				BlankNode RootSubject = this.CreateBlankNode();
				UriNode RootType = this.CreateUriNode(Xml.DocumentElement, BaseUri);

				this.Add(new SemanticTriple(RootSubject, RdfType, RootType));

				int ItemCounter = 0;
				this.ParseDescription(Xml.DocumentElement, RootSubject, Language, BaseUri, Bag, null, ref ItemCounter);
			}
		}

		/// <summary>
		/// Original XML of document.
		/// </summary>
		public XmlDocument Xml => this.xml;

		/// <summary>
		/// Text representation.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Server timestamp of document.
		/// </summary>
		public DateTimeOffset? Date => this.date;

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
			XmlAttribute About = null;
			XmlAttribute Type = null;
			XmlAttribute Id = null;
			string NodeId = null;
			string BagId = null;
			string AboutEach = null;
			string AboutEachPrefix = null;
			ChunkedList<XmlAttribute> Properties = null;
			XmlElement ForEach = null;

			foreach (XmlAttribute Attr in E.Attributes)
			{
				if (Attr.NamespaceURI == Rdf.Namespace)
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
							About = Attr;
							continue;

						case "ID":
							Id = Attr;
							continue;

						case "bagID":
							BagId = Attr.Value;
							continue;

						case "type":
							Type = Attr;
							continue;

						case "aboutEach":
							AboutEach = Attr.Value;
							continue;

						case "aboutEachPrefix":
							AboutEachPrefix = Attr.Value;
							continue;
					}
				}
				else if (Attr.Prefix == "xml")
				{
					switch (Attr.LocalName)
					{
						case "lang":
							Language = Attr.Value;
							break;

						case "base":
							BaseUri = this.CreateUri(Attr.Value, BaseUri);
							break;
					}

					continue;
				}
				else if (Attr.Prefix == "xmlns" || Attr.Name == "xmlns")
					continue;

				if (Properties is null)
					Properties = new ChunkedList<XmlAttribute>();

				Properties.Add(Attr);
			}

			if (!(AboutEach is null))
			{
				if (this.aboutEach is null)
					this.aboutEach = new Dictionary<Uri, XmlElement>();

				this.aboutEach[this.CreateUri(AboutEach, BaseUri)] = E;
				return null;
			}

			if (!(AboutEachPrefix is null))
			{
				if (this.aboutEachPrefix is null)
					this.aboutEachPrefix = new Dictionary<Uri, XmlElement>();

				this.aboutEachPrefix[this.CreateUri(AboutEachPrefix, BaseUri)] = E;
				return null;
			}

			bool HasLanguage = !string.IsNullOrEmpty(Language);
			ISemanticElement Subject;
			ISemanticElement Bag;

			if (BagId is null)
				Bag = null;
			else
			{
				Bag = this.CreateUriNode("#" + BagId, BaseUri);
				this.Add(new SemanticTriple(Bag, RdfType, RdfBag));
			}

			if (!(About is null) || !(Id is null))
			{
				UriNode AboutUriNode = About is null ?
					this.CreateUriNode("#" + Id.Value, BaseUri) :
					this.CreateUriNode(About.Value, BaseUri);
				Subject = AboutUriNode;

				if (ForEach is null)
				{
					if (!(Id is null))
					{
						if (!(this.aboutEach?.TryGetValue(AboutUriNode.Uri, out ForEach) ?? false) &&
							!(this.aboutEachPrefix is null))
						{
							foreach (KeyValuePair<Uri, XmlElement> P in this.aboutEachPrefix)
							{
								if (AboutUriNode.Uri.ToString().StartsWith(P.Key.ToString()))
								{
									ForEach = P.Value;
									break;
								}
							}
						}
					}
				}
			}
			else if (!(NodeId is null))
				Subject = new BlankNode(NodeId);
			else
				Subject = this.CreateBlankNode();

			ISemanticElement DescriptionNode = null;

			if (E.NamespaceURI != Rdf.Namespace)
			{
				DescriptionNode = this.CreateUriNode(E, BaseUri);
				this.Add(new SemanticTriple(Subject, RdfType, DescriptionNode));
			}
			else if (E.LocalName != "Description")
			{
				if (E.LocalName == "li")
				{
					string Item = "_" + (++ItemCounter).ToString();
					this.Add(new SemanticTriple(Subject, RdfType,
						new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item)));
				}
				else
				{
					DescriptionNode = this.CreateUriNode(E, BaseUri);
					this.Add(new SemanticTriple(Subject, RdfType, DescriptionNode));
				}
			}

			if (!(Bag is null) && !(DescriptionNode is null))
			{
				ISemanticElement BagDescription = this.CreateBlankNode();

				string Item = "_" + (++ItemCounter).ToString();
				this.Add(new SemanticTriple(Bag,
					new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item),
					BagDescription));

				this.Add(new SemanticTriple(BagDescription, RdfType, RdfStatement));
				this.Add(new SemanticTriple(BagDescription, RdfSubject, Subject));
				this.Add(new SemanticTriple(BagDescription, RdfPredicate, RdfType));
				this.Add(new SemanticTriple(BagDescription, RdfObject, DescriptionNode));
			}

			if (!(Type is null))
			{
				ISemanticElement TypeObject = this.CreateUriNode(Type.Value, BaseUri);

				this.Add(new SemanticTriple(Subject, RdfType, TypeObject));

				if (!(Bag is null))
				{
					ISemanticElement ReificationNode = this.CreateBlankNode();

					string Item = "_" + (++ItemCounter).ToString();
					this.Add(new SemanticTriple(Bag,
						new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item),
						ReificationNode));

					this.Add(new SemanticTriple(ReificationNode, RdfType, RdfStatement));
					this.Add(new SemanticTriple(ReificationNode, RdfSubject, Subject));
					this.Add(new SemanticTriple(ReificationNode, RdfPredicate, RdfType));
					this.Add(new SemanticTriple(ReificationNode, RdfObject, TypeObject));
				}
			}

			if (!(Properties is null))
			{
				foreach (XmlAttribute Attr in Properties)
				{
					UriNode Predicate = this.CreateUriNode(Attr, BaseUri);
					StringLiteral Object;

					if (HasLanguage)
						Object = new StringLiteral(Attr.Value, Language);
					else
						Object = new StringLiteral(Attr.Value);

					this.Add(new SemanticTriple(Subject, Predicate, Object));

					if (!(Bag is null))
					{
						ISemanticElement ReificationNode = this.CreateBlankNode();

						string Item = "_" + (++ItemCounter).ToString();
						this.Add(new SemanticTriple(Bag,
							new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item),
							ReificationNode));

						this.Add(new SemanticTriple(ReificationNode, RdfType, RdfStatement));
						this.Add(new SemanticTriple(ReificationNode, RdfSubject, Subject));
						this.Add(new SemanticTriple(ReificationNode, RdfPredicate, Predicate));
						this.Add(new SemanticTriple(ReificationNode, RdfObject, Object));
					}
				}
			}

			int ChildItemCounter = Bag is null ? 0 : ItemCounter;
			
			ISemanticElement Result = this.ParseDescription(E, Subject, Language, BaseUri, Bag, ForEach, ref ChildItemCounter);
			return Result;

			// TODO: Fix item counters & bags.

			//
			//if (!(Bag is null))
			//	ItemCounter = ChildItemCounter;
			//
			//return Result;
			//
			//return this.ParseDescription(E, Subject, Language, BaseUri, Bag, ForEach, ref ItemCounter);
		}

		private ISemanticElement ParseDescription(XmlElement E, ISemanticElement Subject,
			string Language, Uri BaseUri, ISemanticElement Bag, XmlElement ForEach,
			ref int ItemCounter)
		{
			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				UriNode Predicate = this.CreateUriNode(E2, BaseUri);
				if (Predicate.UriString.EndsWith("#li") && Predicate.Uri == Rdf.Li)
				{
					string Item = "_" + (++ItemCounter).ToString();
					Predicate = new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item);
				}

				IEnumerable Attributes = ForEach is null ? (IEnumerable)E2.Attributes : new JoinAttributes(E2.Attributes, ForEach.Attributes);
				IEnumerable ChildNodes = ForEach is null ? (IEnumerable)E2.ChildNodes : new JoinNodes(E2.ChildNodes, ForEach.ChildNodes);
				ChunkedList<XmlAttribute> Properties = null;
				ISemanticElement Object = null;
				ISemanticElement Bag2 = null;
				Uri BaseUri2 = BaseUri;
				XmlAttribute Resource = null;
				string NodeId = null;
				string Id = null;
				string DataType = null;
				string Language2 = Language;
				string ParseType = null;
				string BagId = null;

				foreach (XmlAttribute Attr in Attributes)
				{
					if (Attr.NamespaceURI == Rdf.Namespace)
					{
						switch (Attr.LocalName)
						{
							case "resource":
								Resource = Attr;
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

							case "bagID":
								BagId = Attr.Value;
								continue;

							case "aboutEach":
							case "aboutEachPrefix":
								continue;
						}
					}
					else if (Attr.Prefix == "xml")
					{
						switch (Attr.LocalName)
						{
							case "lang":
								Language2 = Attr.Value;
								break;

							case "base":
								BaseUri2 = this.CreateUri(Attr.Value, BaseUri2);
								break;
						}

						continue;
					}
					else if (Attr.Prefix == "xmlns" || Attr.Name == "xmlns")
						continue;

					if (Properties is null)
						Properties = new ChunkedList<XmlAttribute>();

					Properties.Add(Attr);
				}

				if (!(BagId is null))
				{
					Bag2 = this.CreateUriNode("#" + BagId, BaseUri2);
					this.Add(new SemanticTriple(Bag2, RdfType, RdfBag));
				}

				if (!(Resource is null))
				{
					Object = this.CreateUriNode(Resource.Value, BaseUri2);
					this.Add(new SemanticTriple(Subject, Predicate, Object));
				}
				else if (!(NodeId is null))
				{
					Object = new BlankNode(NodeId);
					this.Add(new SemanticTriple(Subject, Predicate, Object));
				}

				if (!(Properties is null))
				{
					if (Object is null)
					{
						Object = this.CreateBlankNode();
						this.Add(new SemanticTriple(Subject, Predicate, Object));
					}

					bool HasLanguage2 = !string.IsNullOrEmpty(Language2);

					foreach (XmlAttribute Attr in Properties)
					{
						ISemanticElement Predicate2 = this.CreateUriNode(Attr, BaseUri2);
						ISemanticElement Literal;

						if (HasLanguage2)
							Literal = new StringLiteral(Attr.Value, Language2);
						else
							Literal = new StringLiteral(Attr.Value);

						this.Add(new SemanticTriple(Object, Predicate2, Literal));

						if (!(Bag2 is null))
						{
							ISemanticElement ReificationNode = this.CreateBlankNode();

							string Item = "_" + (++ItemCounter).ToString();
							this.Add(new SemanticTriple(Bag2,
								new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item),
								ReificationNode));

							this.Add(new SemanticTriple(ReificationNode, RdfType, RdfStatement));
							this.Add(new SemanticTriple(ReificationNode, RdfSubject, Object));
							this.Add(new SemanticTriple(ReificationNode, RdfPredicate, Predicate2));
							this.Add(new SemanticTriple(ReificationNode, RdfObject, Literal));
						}
					}
				}

				switch (ParseType)
				{
					case null:
						foreach (XmlNode N2 in ChildNodes)
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
									DataType = DataTypeUri.ToString();

									if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
									{
										LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
											?? new CustomLiteral(string.Empty, DataType, Language2);

										this.dataTypes[DataType] = LiteralType;
									}

									Object = LiteralType.Parse(Text.InnerText, DataType, Language2);
								}

								this.Add(new SemanticTriple(Subject, Predicate, Object));
							}
							else if (N2 is XmlElement E3)
							{
								ISemanticElement Def = this.ParseDescription(E3, Language2, BaseUri2, ref ItemCounter);

								if (!(Def is null))
								{
									if (Object is null || Object.ToString() != Def.ToString())
										this.Add(new SemanticTriple(Subject, Predicate, Def));

									if (Object is null)
										Object = Def;
								}
							}
						}

						if (Object is null)
						{
							if (!(Bag2 is null))
								Object = this.CreateBlankNode();
							else
								Object = new StringLiteral(string.Empty);

							this.Add(new SemanticTriple(Subject, Predicate, Object));
						}
						break;

					case "Literal":
					default:
						Object = new XmlLiteral(ChildNodes, E2.NamespaceURI, Language2);
						this.Add(new SemanticTriple(Subject, Predicate, Object));
						break;

					case "Resource":
						Object = this.CreateBlankNode();
						this.Add(new SemanticTriple(Subject, Predicate, Object));

						int ChildItemCounter = 0;
						this.ParseDescription(E2, Object, Language2, BaseUri2, Bag2, null, ref ChildItemCounter);
						break;

					case "Collection":
						ChunkedList<ISemanticElement> Elements = null;

						foreach (XmlNode N2 in ChildNodes)
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
									DataType = DataTypeUri.ToString();

									if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
									{
										LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
											?? new CustomLiteral(string.Empty, DataType);

										this.dataTypes[DataType] = LiteralType;
									}

									Element = LiteralType.Parse(Text.InnerText, DataType, Language2);
								}
							}
							else if (N2 is XmlElement E3)
								Element = this.ParseDescription(E3, Language2, BaseUri2, ref ItemCounter);
							else
								continue;

							if (Elements is null)
								Elements = new ChunkedList<ISemanticElement>();

							if (!(Element is null))
								Elements.Add(Element);
						}

						if (Elements is null)
							this.Add(new SemanticTriple(Subject, Predicate, RdfNil));
						else
						{
							ChunkNode<ISemanticElement> Loop = Elements.FirstChunk;
							BlankNode Current = this.CreateBlankNode();
							int i, c;

							this.Add(new SemanticTriple(Subject, Predicate, Current));

							while (!(Loop is null))
							{
								for (i = Loop.Start, c = Loop.Pos; i < c; i++)
								{
									this.Add(new SemanticTriple(Current, RdfFirst, Loop[i]));

									if (i < c - 1 || !(Loop.Next is null))
									{
										BlankNode Next = this.CreateBlankNode();
										this.Add(new SemanticTriple(Current, RdfRest, Next));
										Current = Next;
									}
								}

								Loop = Loop.Next;
							}

							this.Add(new SemanticTriple(Current, RdfRest, RdfNil));
						}
						break;
				}


				if (!(Id is null) || !(Bag is null))
				{
					ISemanticElement ReificationNode;

					if (!(Id is null))
						ReificationNode = this.CreateUriNode("#" + Id, BaseUri2);
					else
						ReificationNode = this.CreateBlankNode();

					if (!(Bag is null))
					{
						string Item = "_" + (++ItemCounter).ToString();
						this.Add(new SemanticTriple(Bag,
							new UriNode(new Uri(Rdf.Namespace + Item), "rdf:" + Item),
							ReificationNode));
					}

					this.Add(new SemanticTriple(ReificationNode, RdfType, RdfStatement));
					this.Add(new SemanticTriple(ReificationNode, RdfSubject, Subject));
					this.Add(new SemanticTriple(ReificationNode, RdfPredicate, Predicate));
					this.Add(new SemanticTriple(ReificationNode, RdfObject, Object));
				}
			}

			return Subject;
		}

		private UriNode CreateUriNode(string Reference, Uri BaseUri)
		{
			return new UriNode(this.CreateUri(Reference, BaseUri), Reference);
		}

		private UriNode CreateUriNode(XmlElement Reference, Uri BaseUri)
		{
			if (string.IsNullOrEmpty(Reference.Prefix))
				return new UriNode(this.CreateUri(Reference.NamespaceURI + Reference.LocalName, BaseUri), Reference.LocalName);
			else
				return new UriNode(this.CreateUri(Reference.NamespaceURI + Reference.LocalName, BaseUri), Reference.Prefix + ":" + Reference.LocalName);
		}

		private UriNode CreateUriNode(XmlAttribute Reference, Uri BaseUri)
		{
			if (string.IsNullOrEmpty(Reference.Prefix))
				return new UriNode(this.CreateUri(Reference.NamespaceURI + Reference.LocalName, BaseUri), Reference.LocalName);
			else
				return new UriNode(this.CreateUri(Reference.NamespaceURI + Reference.LocalName, BaseUri), Reference.Prefix + ":" + Reference.LocalName);
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

		/// <summary>
		/// Decodes meta-information available in the HTTP Response.
		/// </summary>
		/// <param name="HttpResponse">HTTP Response.</param>
		public Task DecodeMetaInformation(HttpResponseMessage HttpResponse)
		{
			this.date = HttpResponse.Headers.Date;
			return Task.CompletedTask;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Matrices;
using Waher.Script.Operators.Vectors;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Contains the results of a SPARQL query.
	/// </summary>
	/// <seealso cref="https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/"/>
	public class SparqlResultSet : IToMatrix, IToVector
	{
		/// <summary>
		/// http://www.w3.org/2005/sparql-results#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2005/sparql-results#";

		/// <summary>
		/// sparql
		/// </summary>
		public const string LocalName = "sparql";

		private readonly Dictionary<string, ISemanticLiteral> dataTypes = new Dictionary<string, ISemanticLiteral>();
		private readonly Uri baseUri;

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Xml">Query results in XML format.</param>
		/// <seealso cref="https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/"/>
		public SparqlResultSet(XmlDocument Xml)
			: this(Xml, null)
		{
		}

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Xml">Query results in XML format.</param>
		/// <seealso cref="https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/"/>
		public SparqlResultSet(string Xml)
			: this(Xml, null)
		{
		}

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Xml">Query results in XML format.</param>
		/// <param name="BaseUri">Base URI of document.</param>
		/// <seealso cref="https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/"/>
		public SparqlResultSet(string Xml, Uri BaseUri)
			: this(RdfDocument.ToXml(Xml), BaseUri)
		{
		}

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Xml">Query results in XML format.</param>
		/// <param name="BaseUri">Base URI of document.</param>
		/// <seealso cref="https://www.w3.org/TR/2023/WD-sparql12-results-xml-20230516/"/>
		public SparqlResultSet(XmlDocument Xml, Uri BaseUri)
		{
			if (Xml is null ||
				Xml.DocumentElement is null ||
				Xml.DocumentElement.LocalName != LocalName ||
				Xml.DocumentElement.NamespaceURI != Namespace)
			{
				throw new ArgumentException("Invalid SPARQL Result XML document.", nameof(Xml));
			}

			this.baseUri = BaseUri;
			this.BooleanResult = null;

			List<string> Variables = new List<string>();
			List<Uri> Links = new List<Uri>();
			List<SparqlResultRecord> Records = new List<SparqlResultRecord>();

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (!(N is XmlElement E) || E.NamespaceURI != Namespace)
					continue;

				switch (E.LocalName)
				{
					case "head":
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (!(N2 is XmlElement E2) || E2.NamespaceURI != Namespace)
								continue;

							switch (E2.LocalName)
							{
								case "variable":
									string Name = XML.Attribute(E2, "name");
									if (!string.IsNullOrEmpty(Name))
										Variables.Add(Name);
									break;

								case "link":
									string HRef = XML.Attribute(E2, "href");
									if (string.IsNullOrEmpty(HRef))
										break;

									if (this.baseUri is null)
									{
										if (Uri.TryCreate(HRef, UriKind.RelativeOrAbsolute, out Uri Link))
											Links.Add(Link);
									}
									else
									{
										if (Uri.TryCreate(this.baseUri, HRef, out Uri Link))
											Links.Add(Link);
									}
									break;
							}
						}
						break;

					case "boolean":
						if (CommonTypes.TryParse(E.InnerText, out bool b))
							this.BooleanResult = b;
						break;

					case "results":
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (!(N2 is XmlElement E2) || E2.NamespaceURI != Namespace)
								continue;

							switch (E2.LocalName)
							{
								case "result":
									Dictionary<string, SparqlResultItem> Record = new Dictionary<string, SparqlResultItem>();
									int Index = 0;

									foreach (XmlNode N3 in E2.ChildNodes)
									{
										if (!(N3 is XmlElement E3) || E3.NamespaceURI != Namespace)
											continue;

										switch (E3.LocalName)
										{
											case "binding":
												string Name = XML.Attribute(E3, "name");
												ISemanticElement Value = this.ParseValue(E3);
												Record[Name] = new SparqlResultItem(Name, Value, Index++);
												break;
										}
									}

									Records.Add(new SparqlResultRecord(Record));
									break;
							}
						}
						break;
				}
			}

			this.Variables = Variables.ToArray();
			this.Links = Links.ToArray();
			this.Records = Records.ToArray();
		}

		private ISemanticElement ParseValue(XmlElement Xml)
		{
			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E) || E.NamespaceURI != Namespace)
					continue;

				switch (E.LocalName)
				{
					case "uri":
						if (this.baseUri is null)
						{
							if (Uri.TryCreate(E.InnerText, UriKind.RelativeOrAbsolute, out Uri UriValue))
								return new UriNode(UriValue, E.InnerText);
							else
								return new StringLiteral(E.InnerText);
						}
						else
						{
							if (Uri.TryCreate(this.baseUri, E.InnerText, out Uri UriValue))
								return new UriNode(UriValue, E.InnerText);
							else
								return new StringLiteral(E.InnerText);
						}

					case "bnode":
						return new BlankNode(E.InnerText);

					case "literal":
						string s = E.InnerText;
						string DataType = null;
						string Language = null;

						foreach (XmlAttribute Attr in E.Attributes)
						{
							switch (Attr.Name)
							{
								case "xml:lang":
									Language = Attr.Value;
									break;

								case "datatype":
									DataType = Attr.Value;
									break;
							}
						}

						if (!string.IsNullOrEmpty(DataType))
						{
							if (!this.dataTypes.TryGetValue(DataType, out ISemanticLiteral LiteralType))
							{
								LiteralType = Types.FindBest<ISemanticLiteral, string>(DataType)
									?? new CustomLiteral(string.Empty, DataType);

								this.dataTypes[DataType] = LiteralType;
							}

							return LiteralType.Parse(s, DataType, Language);
						}
						else if (!string.IsNullOrEmpty(Language))
							return new StringLiteral(s, Language);
						else
							return new StringLiteral(s);

					case "triple":
						ISemanticElement Subject = null;
						ISemanticElement Predicate = null;
						ISemanticElement Object = null;

						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (!(N2 is XmlElement E2) || E2.NamespaceURI != Namespace)
								continue;

							switch (E2.LocalName)
							{
								case "subject":
									Subject = this.ParseValue(E2);
									break;

								case "predicate":
									Predicate = this.ParseValue(E2);
									break;

								case "object":
									Object = this.ParseValue(E2);
									break;
							}
						}

						return new SemanticTriple(Subject, Predicate, Object);

					default:
						continue;
				}
			}

			return null;
		}

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Result">Boolean result</param>
		public SparqlResultSet(bool Result)
		{
			this.Variables = null;
			this.Links = null;
			this.Records = null;
			this.baseUri = null;
			this.BooleanResult = Result;
		}

		/// <summary>
		/// Contains the results of a SPARQL query.
		/// </summary>
		/// <param name="Variables">Names of variables in result set.</param>
		/// <param name="Links">Links to additional metadata about result set.</param>
		/// <param name="Records">Records in result set.</param>
		public SparqlResultSet(string[] Variables, Uri[] Links, SparqlResultRecord[] Records)
		{
			this.Variables = Variables;
			this.Links = Links;
			this.Records = Records;
			this.baseUri = null;
			this.BooleanResult = null;
		}

		/// <summary>
		/// Any Boolean result returned.
		/// </summary>
		public bool? BooleanResult { get; }

		/// <summary>
		/// Names of variables in result set.
		/// </summary>
		public string[] Variables { get; }

		/// <summary>
		/// Links to additional metadata about result set.
		/// </summary>
		public Uri[] Links { get; }

		/// <summary>
		/// Records in result set.
		/// </summary>
		public SparqlResultRecord[] Records { get; }

		/// <summary>
		/// Converts the object to a matrix.
		/// </summary>
		/// <returns>Matrix.</returns>
		public IMatrix ToMatrix()
		{
			if (this.BooleanResult.HasValue)
				return new BooleanMatrix(new bool[1, 1] { { this.BooleanResult.Value } });

			int Columns = this.Variables.Length;
			int Rows = this.Records.Length;
			IElement[,] Elements = new IElement[Rows, Columns];
			int x, y;

			for (y = 0; y < Rows; y++)
			{
				SparqlResultRecord Record = this.Records[y];
				ISemanticElement Item;

				for (x = 0; x < Columns; x++)
				{
					Item = Record[this.Variables[x]];
					Elements[y, x] = Expression.Encapsulate(Item?.ElementValue);
				}
			}

			return new ObjectMatrix(Elements)
			{
				ColumnNames = this.Variables
			};
		}

		/// <summary>
		/// Converts the object to a vector.
		/// </summary>
		/// <returns>Matrix.</returns>
		public IVector ToVector()
		{
			return new ObjectVector(((ObjectMatrix)this.ToMatrix()).VectorElements);
		}

	}
}

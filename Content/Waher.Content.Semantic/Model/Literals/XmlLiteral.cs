using System;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents an XML literal.
	/// </summary>
	public class XmlLiteral : SemanticLiteral
	{
		private readonly string encapsulatingNamespace;
		private string normalizedXml = null;

		/// <summary>
		/// Represents an XML literal.
		/// </summary>
		public XmlLiteral()
			: base()
		{
		}

		/// <summary>
		/// Represents an XML literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="EncapsulatingNamespace">Namespace</param>
		public XmlLiteral(XmlNodeList Value, string EncapsulatingNamespace)
			: base(Value, ToString(Value))
		{
			this.encapsulatingNamespace = EncapsulatingNamespace;
		}

		private static string ToString(XmlNodeList List)
		{
			StringBuilder sb = new StringBuilder();

			foreach (XmlNode N in List)
				sb.Append(N.OuterXml);

			return sb.ToString();
		}

		/// <summary>
		/// Represents a Uri literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value</param>
		public XmlLiteral(XmlNodeList Value, string EncapsulatingNamespace, string StringValue)
			: base(Value, StringValue)
		{
			this.encapsulatingNamespace = EncapsulatingNamespace;
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => "http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral";

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			try
			{
				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};

				Doc.LoadXml(Value);

				return new XmlLiteral(Doc.ChildNodes, Value);
			}
			catch (Exception)
			{
				return new CustomLiteral(Value, DataType);
			}
		}

		/// <summary>
		/// Normalized XML
		/// </summary>
		public string NormalizedXml
		{
			get
			{
				if (this.normalizedXml is null)
				{
					XmlNormalizationState State = new XmlNormalizationState();
					XML.NormalizeXml((XmlNodeList)this.Value, false, this.encapsulatingNamespace, State);
					this.normalizedXml = State.ToString();
				}

				return this.normalizedXml;
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is XmlLiteral Typed))
				return false;

			string s1 = this.NormalizedXml;
			string s2 = Typed.NormalizedXml;

			return s1 == s2;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.NormalizedXml.GetHashCode();
		}

	}
}

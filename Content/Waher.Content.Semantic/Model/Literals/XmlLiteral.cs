using System;
using System.Collections;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents an XML literal.
	/// </summary>
	public class XmlLiteral : SemanticLiteral
	{
		private readonly string encapsulatingNamespace;
		private readonly string language = null;
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
		/// <param name="Language">Language</param>
		public XmlLiteral(IEnumerable Value, string EncapsulatingNamespace, string Language)
			: base(Value, ToString(Value))
		{
			this.encapsulatingNamespace = EncapsulatingNamespace;
			this.language = Language;
		}

		private static string ToString(IEnumerable List)
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
		/// <param name="EncapsulatingNamespace">Namespace of encapsulating elements.</param>
		/// <param name="StringValue">String value</param>
		/// <param name="Language">Language</param>
		public XmlLiteral(XmlNodeList Value, string EncapsulatingNamespace, string StringValue, string Language)
			: base(Value, StringValue)
		{
			this.encapsulatingNamespace = EncapsulatingNamespace;
			this.language = Language;
		}

		private const string RdfXmlLiteral = "http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral";

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => RdfXmlLiteral;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return Grade.NotAtAll;
		}

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
		{
			try
			{
				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};

				Doc.LoadXml(Value);

				return new XmlLiteral(Doc.ChildNodes, string.Empty, Value, Language);
			}
			catch (Exception)
			{
				return new CustomLiteral(Value, DataType, Language);
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
			string s2;

			if (obj is XmlLiteral Typed)
			{
				if (this.language != Typed.language)
					return false;

				s2 = Typed.NormalizedXml;
			}
			else if (obj is CustomLiteral Custom && Custom.StringType == RdfXmlLiteral)
			{
				if (this.language != Custom.Language)
					return false;

				try
				{
					XmlDocument Doc = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Doc.LoadXml("<root>" + Custom.StringValue + "</root>");

					s2 = XML.NormalizeXml(Doc.DocumentElement.ChildNodes, false);
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				return false;

			string s1 = this.NormalizedXml;

			return s1 == s2;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.NormalizedXml.GetHashCode();
			Result ^= Result << 5 ^ (this.language?.GetHashCode() ?? 0);
			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('"');
			sb.Append(JSON.Encode(this.StringValue));
			sb.Append('"');

			if (!string.IsNullOrEmpty(this.language))
			{
				sb.Append('@');
				sb.Append(this.language);
			}

			sb.Append("^^<");
			sb.Append(this.StringType);
			sb.Append('>');

			return sb.ToString();
		}

	}
}

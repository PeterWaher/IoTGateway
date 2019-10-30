using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// ASN.1 Macro
	/// </summary>
	public class Asn1Macro : Asn1Node, INamedNode
	{
		private readonly UserDefinedItem typeNotation;
		private readonly UserDefinedItem valueNotation;
		private readonly SupportingSyntax[] supportingSyntax;
		private readonly Asn1Document document;
		private readonly string name;

		/// <summary>
		/// ASN.1 Macro
		/// </summary>
		/// <param name="Name">Name of macro.</param>
		/// <param name="TypeNotation">Type Notation</param>
		/// <param name="ValueNotation">Value Notation</param>
		/// <param name="SupportingSyntax">Supporting Syntax</param>
		/// <param name="Document">Document defining macro.</param>
		public Asn1Macro(string Name, UserDefinedItem TypeNotation, UserDefinedItem ValueNotation,
			SupportingSyntax[] SupportingSyntax, Asn1Document Document)
		{
			this.name = Name;
			this.typeNotation = TypeNotation;
			this.valueNotation = ValueNotation;
			this.supportingSyntax = SupportingSyntax;
			this.document = Document;
		}

		/// <summary>
		/// Name of macro.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type notation
		/// </summary>
		public UserDefinedItem TypeNotation => this.typeNotation;

		/// <summary>
		/// Value notation
		/// </summary>
		public UserDefinedItem ValueNotation => this.valueNotation;

		/// <summary>
		/// Supporting syntax
		/// </summary>
		public SupportingSyntax[] SupportingSyntax => this.supportingSyntax;

		/// <summary>
		/// ASN.1 document defining macro.
		/// </summary>
		public Asn1Document Document => this.document;

		/// <summary>
		/// Parses the portion of the document at the current position, according to the
		/// instructions available in the macro.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public KeyValuePair<Asn1Type, Asn1Value> Parse(Asn1Document Document)
		{
			this.typeNotation.Parse(Document, this);

			Asn1Node Node = this.valueNotation.Parse(Document, this);

			if (!(Node is Asn1Value Value))
				throw Document.SyntaxError("Value expected.");

			return new KeyValuePair<Asn1Type, Asn1Value>(null, Value);
		}

	}
}

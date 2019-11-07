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
		internal readonly Dictionary<string, SupportingSyntax> supportingSyntax;
		internal readonly SupportingSyntax[] supportingSyntaxArray;
		private readonly UserDefinedItem typeNotation;
		private readonly UserDefinedItem valueNotation;
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
			this.supportingSyntaxArray = SupportingSyntax;
			this.document = Document;

			this.supportingSyntax = new Dictionary<string, SupportingSyntax>();

			foreach (SupportingSyntax Syntax in SupportingSyntax)
				this.supportingSyntax[Syntax.Name] = Syntax;
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
		public SupportingSyntax[] SupportingSyntax => this.supportingSyntaxArray;

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
		public Asn1Value Parse(Asn1Document Document)
		{
			this.typeNotation.Parse(Document, this);

			Document.AssertNextToken("::=");

			Asn1Node Node = this.valueNotation.Parse(Document, this);

			if (!(Node is Asn1Value Value))
				throw Document.SyntaxError("Value expected.");

			return Value;
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			// Don't export macro.
		}

		/// <summary>
		/// Gets the ASN.1 type corresponding to the value.
		/// </summary>
		/// <returns>ASN.1 type.</returns>
		public Asn1Type GetValueType()
		{
			if (this.valueNotation is UserDefinedSpecifiedPart Part)
				return Part.Type;
			else
				return new Types.Asn1Any();
		}

	}
}

using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Abstract base class for string types
	/// </summary>
	public abstract class Asn1StringType : Asn1Type
	{
		/// <summary>
		/// Abstract base class for string types
		/// </summary>
		public Asn1StringType()
			: base()
		{
		}

		/// <summary>
		/// Parses the portion of the document at the current position, according to the type.
		/// </summary>
		/// <param name="Document">ASN.1 document being parsed.</param>
		/// <param name="Macro">Macro performing parsing.</param>
		/// <returns>Parsed ASN.1 node.</returns>
		public override Asn1Node Parse(Asn1Document Document, Asn1Macro Macro)
		{
			if (Document.ParseValue() is Values.Asn1StringValue Value)
				return Value;
			else
				throw Document.SyntaxError("String value expected.");
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
			if (Pass == CSharpExportPass.Explicit)
				Output.Append("String");
		}
	}
}

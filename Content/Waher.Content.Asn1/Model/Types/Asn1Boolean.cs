using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Asn1.Model.Macro;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BOOLEAN
	/// </summary>
	public class Asn1Boolean : Asn1Type
	{
		/// <summary>
		/// BOOLEAN
		/// </summary>
		public Asn1Boolean()
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
			if (Document.ParseValue() is Values.Asn1BooleanValue Value)
				return Value;
			else
				throw Document.SyntaxError("Boolean value expected.");
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
			{
				Output.Append("Boolean");
				if (this.Optional.HasValue && this.Optional.Value)
					Output.Append('?');
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents a named value.
	/// </summary>
	public class Asn1NamedValue : Asn1ValueReference
	{
		private readonly Asn1Value value;

		/// <summary>
		/// Represents a named value.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Value">Optional Value</param>
		/// <param name="Document">ASN.1 Document containing the reference</param>
		public Asn1NamedValue(string Identifier, Asn1Value Value, Asn1Document Document)
			: base(Identifier, Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Optional Value
		/// </summary>
		public Asn1Value Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				this.value.ExportCSharp(Output, State, Indent, Pass);

				Output.Append(" /* ");
				base.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
				Output.Append(" */");
			}
		}
	}
}

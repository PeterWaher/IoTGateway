using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// Represents an ASN.1 type reference.
	/// </summary>
	public class Asn1TypeReference : Asn1Type
	{
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 type reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		public Asn1TypeReference(string Identifier)
			: base()
		{
			this.identifier = Identifier;
		}

		/// <summary>
		/// Identifier
		/// </summary>
		public string Identifier => identifier;

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
				Output.Append(this.identifier);
		}
	}
}

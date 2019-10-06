using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents an ASN.1 value reference.
	/// </summary>
	public class Asn1ValueReference : Asn1Value
	{
		private readonly string identifier;

		/// <summary>
		/// Represents an ASN.1 value reference.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		public Asn1ValueReference(string Identifier)
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
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings, int Indent)
		{
			Output.Append(this.identifier);
		}
	}
}

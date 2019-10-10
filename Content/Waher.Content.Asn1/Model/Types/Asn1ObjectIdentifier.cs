using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// OBJECT IDENTIFIER, or RELATIVE-OID
	/// </summary>
	public class Asn1ObjectIdentifier : Asn1Type
	{
		private readonly bool relative;

		/// <summary>
		/// OBJECT IDENTIFIER, or RELATIVE-OID
		/// </summary>
		/// <param name="Relative">If it is a relative object ID.</param>
		public Asn1ObjectIdentifier(bool Relative)
			: base()
		{
			this.relative = Relative;
		}

		/// <summary>
		/// If it is a relative object ID.
		/// </summary>
		public bool Relative => this.relative;

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
				Output.Append("Array<Int32>");
		}
	}
}

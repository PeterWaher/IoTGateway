using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// BIT STRING
	/// </summary>
	public class Asn1BitString : Asn1Type
	{
		/// <summary>
		/// BIT STRING
		/// </summary>
		public Asn1BitString()
			: base()
		{
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append("Array<Byte>");
		
			return Task.CompletedTask;
		}
	}
}

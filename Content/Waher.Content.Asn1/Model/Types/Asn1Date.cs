using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// DATE
	/// </summary>
	public class Asn1Date : Asn1Type
	{
		/// <summary>
		/// DATE
		/// </summary>
		public Asn1Date()
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
		public override void ExportCSharp(StringBuilder Output, CSharpExportState State, int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
			{
				Output.Append("DateTime");
				if (this.Optional.HasValue && this.Optional.Value)
					Output.Append('?');
			}
		}
	}
}

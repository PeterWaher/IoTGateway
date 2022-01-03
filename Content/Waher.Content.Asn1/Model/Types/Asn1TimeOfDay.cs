using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Types
{
	/// <summary>
	/// TIME-OF-DAY
	/// </summary>
	public class Asn1TimeOfDay : Asn1Type
	{
		/// <summary>
		/// TIME-OF-DAY
		/// </summary>
		public Asn1TimeOfDay()
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
			{
				Output.Append("TimeSpan");
				if (this.Optional.HasValue && this.Optional.Value)
					Output.Append('?');
			}
	
			return Task.CompletedTask;
		}
	}
}

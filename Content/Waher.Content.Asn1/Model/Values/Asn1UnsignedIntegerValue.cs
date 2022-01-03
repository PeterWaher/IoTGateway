using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Unsigned Integer value
	/// </summary>
	public class Asn1UnsignedIntegerValue : Asn1Value
	{
		private readonly ulong value;

		/// <summary>
		/// Integer value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1UnsignedIntegerValue(ulong Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public ulong Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Explicit)
				Output.Append(this.value.ToString());
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "UInt64";
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Boolean value
	/// </summary>
	public class Asn1BooleanValue : Asn1Value
	{
		private readonly bool value;

		/// <summary>
		/// Boolean value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1BooleanValue(bool Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public bool Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings,
			int Indent, CSharpExportPass Pass)
		{
			Output.Append(this.value ? "true" : "false");
		}
	}
}

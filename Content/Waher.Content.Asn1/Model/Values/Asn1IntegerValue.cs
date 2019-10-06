using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Integer value
	/// </summary>
	public class Asn1IntegerValue : Asn1Value
	{
		private readonly long value;

		/// <summary>
		/// Integer value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1IntegerValue(long Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public long Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings,
			int Indent)
		{
			Output.Append(this.value.ToString());
		}
	}
}

using System;
using System.Globalization;
using System.Text;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Floating-point value
	/// </summary>
	public class Asn1FloatingPointValue : Asn1Value
	{
		private readonly double value;

		/// <summary>
		/// Floating-point value
		/// </summary>
		/// <param name="Value">Value</param>
		public Asn1FloatingPointValue(double Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public double Value => this.value;

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="Settings">C# export settings.</param>
		/// <param name="Indent">Indentation</param>
		public override void ExportCSharp(StringBuilder Output, CSharpExportSettings Settings,
			int Indent)
		{
			Output.Append(this.value.ToString().Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
		}
	}
}

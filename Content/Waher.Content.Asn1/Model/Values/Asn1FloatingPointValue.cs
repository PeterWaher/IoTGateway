using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Floating-point value
	/// </summary>
	public class Asn1FloatingPointValue : Asn1Value
	{
		private double value;

		/// <summary>
		/// Floating-point value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1FloatingPointValue(double Value, Asn1Document Document)
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public double Value => this.value;

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public override void TwosPower()
		{
			if (!this.PowerOfTwo)
			{
				this.PowerOfTwo = true;
				this.value = Math.Pow(2, this.value);
			}
		}

		/// <inheritdoc/>
		public override string ToString() => this.value.ToString(CultureInfo.InvariantCulture);

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
				Output.Append(this.value.ToString().Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Corresponding C# type.
		/// </summary>
		public override string CSharpType => "double";
	}
}

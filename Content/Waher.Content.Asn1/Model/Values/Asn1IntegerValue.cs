using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Integer value
	/// </summary>
	public class Asn1IntegerValue : Asn1Value
	{
		private long value;

		/// <summary>
		/// Integer value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1IntegerValue(long Value, Asn1Document Document)
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public long Value => this.value;

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public override void TwosPower()
		{
			if (!this.PowerOfTwo)
			{
				this.PowerOfTwo = true;

				if (this.value < 0 || this.value >= 63)
					base.TwosPower();
				else
					this.value = 1L << (int)this.value;
			}
		}

		/// <inheritdoc/>
		public override string ToString() => this.value.ToString();

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
		public override string CSharpType => "long";
	}
}

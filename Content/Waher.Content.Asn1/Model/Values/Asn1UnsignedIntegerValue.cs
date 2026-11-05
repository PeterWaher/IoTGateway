using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Unsigned Integer value
	/// </summary>
	public class Asn1UnsignedIntegerValue : Asn1Value
	{
		private ulong value;

		/// <summary>
		/// Integer value
		/// </summary>
		/// <param name="Value">Value</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1UnsignedIntegerValue(ulong Value, Asn1Document Document)
			: base(Document)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public ulong Value => this.value;

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public override void TwosPower()
		{
			if (!this.PowerOfTwo)
			{
				this.PowerOfTwo = true;

				if (this.value >= 64)
					base.TwosPower();
				else
					this.value = 1UL << (int)this.value;
			}
		}

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
		public override string CSharpType => "ulong";

		/// <inheritdoc/>
		public override string ToString() => this.value.ToString();
	}
}

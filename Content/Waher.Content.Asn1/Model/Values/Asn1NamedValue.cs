using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Values
{
	/// <summary>
	/// Represents a named value.
	/// </summary>
	public class Asn1NamedValue : Asn1ValueReference
	{
		private readonly Asn1Value value;
		private readonly Asn1Type typeDefinition;
		private bool enumeratedValue = false;

		/// <summary>
		/// Represents a named value.
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Value">Optional Value</param>
		/// <param name="TypeDefinition">Type definition.</param>
		/// <param name="Document">ASN.1 Document containing the reference</param>
		public Asn1NamedValue(string Identifier, Asn1Value Value, 
			Asn1Type TypeDefinition, Asn1Document Document)
			: base(Identifier, Document)
		{
			this.value = Value;
			this.typeDefinition = TypeDefinition;
		
			this.SetAsChild(this.value);
			this.SetAsChild(this.typeDefinition);
		}

		/// <summary>
		/// Optional Value
		/// </summary>
		public Asn1Value Value => this.value;

		/// <summary>
		/// Type definition.
		/// </summary>
		public Asn1Type TypeDefinition => this.typeDefinition;

		/// <summary>
		/// Makes the value 2^value, for use with <see cref="System.FlagsAttribute"/>
		/// enumerations.
		/// </summary>
		public override void TwosPower()
		{
			this.value.TwosPower();
		}

		/// <inheritdoc/>
		public override string ToString() => this.value.ToString();

		/// <summary>
		/// If the named value is part of an enumerated value definition.
		/// </summary>
		public bool EnumeratedValue
		{
			get => this.enumeratedValue;
			internal set => this.enumeratedValue = value;
		}

		/// <summary>
		/// Exports to C#
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		/// <param name="Pass">Export pass</param>
		public override async Task ExportCSharp(StringBuilder Output, CSharpExportState State,
			int Indent, CSharpExportPass Pass)
		{
			if (Pass == CSharpExportPass.Implicit)
			{
				if (!this.enumeratedValue && 
					!(this.value is null) && 
					!(this.typeDefinition is null))
				{
					JSON.Indent(Output, State.ExportingValuesIndent);
					Output.Append("public static readonly ");

					await this.typeDefinition.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);

					Output.Append(' ');
					await base.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
					Output.Append(" = ");
					await this.value.ExportCSharp(Output, State, Indent, Pass);
					Output.AppendLine(";");
				}
			}
			else if (Pass == CSharpExportPass.Explicit)
			{
				if (this.enumeratedValue)
				{
					await base.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
					Output.Append(" = ");
				}

				await this.value.ExportCSharp(Output, State, Indent, Pass);

				if (!this.enumeratedValue)
				{
					Output.Append(" /* ");
					await base.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
					Output.Append(" */");
				}
			}
		}
	}
}

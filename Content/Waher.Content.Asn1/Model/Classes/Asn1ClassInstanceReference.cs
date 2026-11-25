using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Instance of a class.
	/// </summary>
	public class Asn1ClassInstanceReference : Asn1ClassInstanceElement
	{
		private readonly string name;

		/// <summary>
		/// Instance of a class.
		/// </summary>
		/// <param name="Name">Class instance name.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ClassInstanceReference(string Name, Asn1Document Document)
			: base(Document)
		{
			this.name = Name;
		}

		/// <summary>
		/// Class instance name.
		/// </summary>
		public string Name => this.name;

		/// <inheritdoc/>
		public override string ToString() => this.name;

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
			if (Pass == CSharpExportPass.Variables)
			{
				Output.Append("new ");
				Output.Append(ToCSharp(State, this, this.name));
				Output.Append("()");
			}

			return Task.CompletedTask;
		}
	}
}

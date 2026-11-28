using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model.Sets
{
	/// <summary>
	/// Set of explicitly defined elements.
	/// </summary>
	public class Asn1ExplicitSet : Asn1SetOfValues
	{
		private readonly Asn1Value[] elements;
		private readonly bool extensible;

		/// <summary>
		/// Set of explicitly defined elements.
		/// </summary>
		/// <param name="Elements">Elements</param>
		/// <param name="Extensible">If the set is extensible.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1ExplicitSet(Asn1Value[] Elements, bool Extensible, Asn1Document Document)
			: base(Document)
		{
			this.elements = Elements;
			this.extensible = Extensible;

			foreach (Asn1Value Element in this.elements)
				this.SetAsChild(Element);
		}

		/// <summary>
		/// Elements
		/// </summary>
		public Asn1Value[] Elements => this.elements;

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public override Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent)
		{
			if (this.extensible)
				Output.Append("new Asn1.Construct.AllValues()");
			else
			{
				bool First = true;

				foreach (Asn1Value Element in this.elements)
				{
					if (First)
					{
						First = false;

						Output.Append("new Asn1.Construct.EnumerableSetOfValues<");
						Output.Append(Element.CSharpType);
						Output.Append(">(");
					}
					else
						Output.Append(", ");

					Element.ExportCSharp(Output, State, Indent, CSharpExportPass.Explicit);
				}

				if (First)
					Output.Append("new Asn1.Construct.NoValues()");
				else
					Output.Append(')');
			}

			return Task.CompletedTask;
		}
	}
}

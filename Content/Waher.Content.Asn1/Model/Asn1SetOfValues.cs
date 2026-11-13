using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for sets of values
	/// </summary>
	public abstract class Asn1SetOfValues : Asn1Node
	{
		/// <summary>
		/// Abstract base class for sets of values
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1SetOfValues(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// Name of set.
		/// </summary>
		public string GetSetName(string VariableName)
		{
			if (VariableName.StartsWith("this."))
			{
				VariableName = VariableName.Substring(5);
				VariableName = Asn1FieldValueDefinition.DefinedValues + "." + VariableName;
			}

			Asn1Node Loop = this.Parent;
			string s = null;

			while (!(Loop is null) && s is null)
			{
				if (Loop is Asn1Restriction Restriction)
				{
					s = Restriction.GetType().Name;
					if (s.StartsWith("Asn1"))
						s = s.Substring(4);
				}

				Loop = Loop.Parent;
			}

			if (string.IsNullOrEmpty(s))
				s = "Restriction";

			s = VariableName + s;

			int i = 1;
			string Suffix = string.Empty;

			while (this.Document.namedNodes.ContainsKey(s + Suffix))
			{
				i++;
				Suffix = i.ToString();
			}

			s += Suffix;

			return s;
		}

		/// <summary>
		/// Exports the set definition to C#.
		/// </summary>
		/// <param name="Output">C# Output.</param>
		/// <param name="State">C# export state.</param>
		/// <param name="Indent">Indentation</param>
		public abstract Task ExportSetDefinitionCSharp(StringBuilder Output,
			CSharpExportState State, int Indent);
	}
}

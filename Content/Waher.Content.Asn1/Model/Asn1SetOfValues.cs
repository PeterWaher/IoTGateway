using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Abstract base class for sets of values
	/// </summary>
	public abstract class Asn1SetOfValues : Asn1Node
	{
		private Dictionary<string, string> setNamesPerVariable = null;

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

			if (this.setNamesPerVariable?.TryGetValue(VariableName, out string Name) ?? false)
				return Name;

			Asn1Node Loop = this.Parent;
			Name = null;

			while (!(Loop is null) && Name is null)
			{
				if (Loop is Asn1Restriction Restriction)
				{
					Name = Restriction.GetType().Name;
					if (Name.StartsWith("Asn1"))
						Name = Name.Substring(4);
				}

				Loop = Loop.Parent;
			}

			if (string.IsNullOrEmpty(Name))
				Name = "Restriction";

			Name = VariableName + Name;

			int i = 1;
			string Suffix = string.Empty;

			while (this.Document.namedNodes.ContainsKey(Name + Suffix))
			{
				i++;
				Suffix = i.ToString();
			}

			Name += Suffix;
			this.Document.namedNodes[Name] = this;

			if (this.setNamesPerVariable is null)
				this.setNamesPerVariable = new Dictionary<string, string>();

			this.setNamesPerVariable[VariableName] = Name;

			return Name;
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
